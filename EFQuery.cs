using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PeteMontgomery.PredicateBuilder;

namespace JeffDege.EFQuery
{
	public class EFQuery
	{
#region static convenience functions

		public static EFQuery compare(SelectionComparison comparison, string fieldName, object rightHandSide)
		{
			return new EFQuery
				{
					selectionComparison = comparison,
					fieldName = fieldName,
					rightHandSide = rightHandSide as EFQuery ?? EFQuery.constant(rightHandSide)
				};
		}

		/// <summary>
		/// returns a EFQuery that is always true
		/// </summary>
		/// <returns>EFQuery</returns>
		public static EFQuery isTrue()
		{
			return new EFQuery { selectionComparison = SelectionComparison.IsTrue };
		}

		/// <summary>
		/// returns a EFQuery that is always false
		/// </summary>
		/// <returns>EFQuery</returns>
		public static EFQuery isFalse()
		{
			return new EFQuery { selectionComparison = SelectionComparison.IsFalse };
		}

		/// <summary>
		/// returns a EFQuery that is true if fieldName contains rightHandSide as a substring
		/// </summary>
		/// <returns>EFQuery</returns>
		public static EFQuery contains(string fieldName, object rightHandSide)
		{
			return compare(SelectionComparison.Contains, fieldName, rightHandSide);
		}

		/// <summary>
		/// returns a EFQuery that is true if fieldName starts with rightHandSide as a substring
		/// </summary>
		/// <returns>EFQuery</returns>
		public static EFQuery startsWith(string fieldName, object rightHandSide)
		{
			return compare(SelectionComparison.StartsWith, fieldName, rightHandSide);
		}

		/// <summary>
		/// returns a EFQuery that is true if fieldName ends with rightHandSide as a substring
		/// </summary>
		/// <returns>EFQuery</returns>
		public static EFQuery endsWith(string fieldName, object rightHandSide)
		{
			return compare(SelectionComparison.EndsWith, fieldName, rightHandSide);
		}

		/// <summary>
		/// returns a EFQuery that is true if fieldName matches rightHandSide as a SQL LIKE pattern
		/// </summary>
		/// <returns>EFQuery</returns>
		public static EFQuery like(string fieldName, object rightHandSide)
		{
			return compare(SelectionComparison.Like, fieldName, rightHandSide);
		}

		/// <summary>
		/// returns a EFQuery that is true if the innerCriteria evaluates true
		/// on the fieldName. (Assumes fieldName is a one-to-many navigational property).
		/// </summary>
		/// <returns>EFQuery</returns>
		public static EFQuery any(string fieldName, EFQuery innerCriteria)
		{
			return new EFQuery
			{
				selectionComparison = SelectionComparison.Any,
				fieldName = fieldName,
				innerCriteria = innerCriteria
			};
		}

		/// <summary>
		/// returns a EFQuery that is true if fieldName is equal to rightHandSide
		/// </summary>
		/// <returns>EFQuery</returns>
		public static EFQuery equal(string fieldName, object rightHandSide)
		{
			return compare(SelectionComparison.Equal, fieldName, rightHandSide);
		}

		/// <summary>
		/// returns a EFQuery that is true if fieldName is not equal to rightHandSide
		/// </summary>
		/// <returns>EFQuery</returns>
		public static EFQuery notEqual(string fieldName, object rightHandSide)
		{
			return compare(SelectionComparison.NotEqual, fieldName, rightHandSide);
		}

		/// <summary>
		/// returns a EFQuery that is true if fieldName is greater than rightHandSide
		/// </summary>
		/// <returns>EFQuery</returns>
		public static EFQuery greaterThan(string fieldName, object rightHandSide)
		{
			return compare(SelectionComparison.GreaterThan, fieldName, rightHandSide);
		}

		/// <summary>
		/// returns a EFQuery that is true if fieldName is greater than or equal to rightHandSide
		/// </summary>
		/// <returns>EFQuery</returns>
		public static EFQuery greaterThanOrEqual(string fieldName, object rightHandSide)
		{
			return compare(SelectionComparison.GreaterThanOrEqual, fieldName, rightHandSide);
		}

		/// <summary>
		/// returns a EFQuery that is true if fieldName is less than rightHandSide
		/// </summary>
		/// <returns>EFQuery</returns>
		public static EFQuery lessThan(string fieldName, object rightHandSide)
		{
			return compare(SelectionComparison.LessThan, fieldName, rightHandSide);
		}

		/// <summary>
		/// returns a EFQuery that is true if fieldName is less than or equal to rightHandSide
		/// </summary>
		/// <returns>EFQuery</returns>
		public static EFQuery lessThanOrEqual(string fieldName, object rightHandSide)
		{
			return compare(SelectionComparison.LessThanOrEqual, fieldName, rightHandSide);
		}

		/// <summary>
		/// returns a EFQuery that is true if all of the efQueries evaluate true
		/// </summary>
		/// <returns>EFQuery</returns>
		public static EFQuery and(IEnumerable<EFQuery> efQueries)
		{
			return new EFQuery
			{
				selectionComparison = SelectionComparison.And,
				aggregateList = efQueries
			};
		}

		/// <summary>
		/// returns a EFQuery that is true if at least one of the efQueries evaluates true
		/// </summary>
		/// <returns>EFQuery</returns>
		public static EFQuery or(IEnumerable<EFQuery> efQueries)
		{
			return new EFQuery
			{
				selectionComparison = SelectionComparison.Or,
				aggregateList = efQueries
			};
		}

		/// <summary>
		/// returns a EFQuery that is true if at least one the efQueries evaluates false
		/// </summary>
		/// <returns>EFQuery</returns>
		public static EFQuery nand(IEnumerable<EFQuery> efQueries)
		{
			return new EFQuery
			{
				selectionComparison = SelectionComparison.Nand,
				aggregateList = efQueries
			};
		}

		/// <summary>
		/// returns a EFQuery that is true if all of the efQueries evaluate false
		/// </summary>
		/// <returns>EFQuery</returns>
		public static EFQuery nor(IEnumerable<EFQuery> efQueries)
		{
			return new EFQuery
			{
				selectionComparison = SelectionComparison.Nor,
				aggregateList = efQueries
			};
		}

		/// <summary>
		/// returns a EFQuery that is true if all of the EFQuery evaluates false
		/// </summary>
		/// <returns>EFQuery</returns>
		public static EFQuery not(EFQuery efQuery)
		{
			return nand(new[] { efQuery });
		}

		/// <summary>
		/// returns a EFQuery that is true if fieldName is equal to one of the objects
		/// </summary>
		/// <returns>EFQuery</returns>
		public static EFQuery containedIn(string fieldName, IEnumerable<object> objects)
		{
			return or(objects.Select(o => equal(fieldName, o)));
		}

		/// <summary>
		/// returns a EFQuery that is true if fieldName is greatder than or equal to left
		/// and less than or equal to right
		/// </summary>
		/// <returns>EFQuery</returns>
		public static EFQuery between(string fieldName, object left, object right)
		{
			return and(new[]
				{
					greaterThanOrEqual(fieldName, left),
					lessThanOrEqual(fieldName, right)
				});
		}

		public static EFQuery constant(object value)
		{
			return new EFQuery
			{
				selectionComparison = SelectionComparison.Constant,
				constantValue = value
			};
		}

		public static EFQuery add(object left, object right)
		{
			return new EFQuery
			{
				selectionComparison = SelectionComparison.Add,
				aggregateList = new[]
				{
					EFQuery.constant(left),
					EFQuery.constant(right)
				}
			};
		}
#endregion

#region Member variables

		[JsonConverter(typeof(StringEnumConverter))]
		public SelectionComparison selectionComparison { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string fieldName { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public EFQuery rightHandSide { get; set; }

		// This is here so we can deserialize old-style EFQuery strings
		public object fieldValue
		{
			set { this.rightHandSide = EFQuery.constant((value)); }
		}

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public object constantValue { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public EFQuery innerCriteria { get; set; }

		private IEnumerable<EFQuery> aggregateList_ = null;

		public IEnumerable<EFQuery> aggregateList
		{
			get { return this.aggregateList_ ?? (this.aggregateList_ = new List<EFQuery>()); }
			set { this.aggregateList_ = value; }
		}
		
#endregion

#region Constructors

		public EFQuery()
		{
			this.selectionComparison = SelectionComparison.Constant;
			this.constantValue = null;
		}

#endregion

		// This just tells JSON.net whether to include aggregateList when serializing an object
		public bool ShouldSerializeaggregateList()
		{
			return this.aggregateList_ != null;
		}

#region processing types

		[JsonIgnore]
		public bool isAdd
		{
			get { return this.selectionComparison == SelectionComparison.Add; }
		}

		[JsonIgnore]
		public bool isConstant
		{
			get { return this.selectionComparison == SelectionComparison.Constant; }
		}

		[JsonIgnore]
		public bool isMethodCall
		{
			get
			{
				switch (this.selectionComparison)
				{
					case SelectionComparison.Contains:
					case SelectionComparison.StartsWith:
					case SelectionComparison.EndsWith:
						return true;
					default:
						return false;
				}
			}
		}

		[JsonIgnore]
		public bool isStaticCall
		{
			get
			{
				switch (this.selectionComparison)
				{
					case SelectionComparison.Like:
						return true;
					default:
						return false;
				}
			}
		}

		[JsonIgnore]
		public bool isAny
		{
			get
			{
				switch (this.selectionComparison)
				{
					case SelectionComparison.Any:
						return true;
					default:
						return false;
				}
			}
		}

		[JsonIgnore]
		public bool isAggregate
		{
			get
			{
				switch (this.selectionComparison)
				{
					case SelectionComparison.And:
					case SelectionComparison.Or:
					case SelectionComparison.Nand:
					case SelectionComparison.Nor:
						return true;
					default:
						return false;
				}
			}
		}

		[JsonIgnore]
		public bool isNegate
		{
			get
			{
				switch (this.selectionComparison)
				{
					case SelectionComparison.Nand:
					case SelectionComparison.Nor:
						return true;
					default:
						return false;
				}
			}
		}

		[JsonIgnore]
		public bool isAnd
		{
			get
			{
				switch (this.selectionComparison)
				{
					case SelectionComparison.And:
					case SelectionComparison.Nand:
						return true;
					default:
						return false;
				}
			}
		}

		[JsonIgnore]
		public bool isUnary
		{
			get
			{
				switch (this.selectionComparison)
				{
					case SelectionComparison.IsTrue:
					case SelectionComparison.IsFalse:
						return true;
					default:
						return false;
				}
			}
		}
#endregion

#region predicate construction functions
		public Expression<Func<T, bool>> constructPredicate<T>(object context = null)
		{
			if (this.isUnary)
				return this.selectionComparison == SelectionComparison.IsTrue
					? PredicateBuilder.True<T>()
					: PredicateBuilder.False<T>();

			if (this.isAggregate)
				return this.constructAggregatePredicate<T>(context);

			if (this.isMethodCall)
				return this.constructMethodCallPredicate<T>(context);

			if (this.isStaticCall)
				return this.constructStaticCallPredicate<T>(context);

			if (this.isAny)
				return this.constructAnyPredicate<T>(context);

			return this.constructSinglePredicate<T>(context);
		}

		private Expression<Func<T, bool>> constructAggregatePredicate<T>(object context)
		{
			var predicate = this.isAnd ? PredicateBuilder.True<T>() : PredicateBuilder.False<T>();

			foreach (var item in this.aggregateList)
			{
				predicate = this.isAnd 
					? predicate.And(item.constructPredicate<T>(context))
					: predicate.Or(item.constructPredicate<T>(context));
			}

			if (this.isNegate)
				predicate = negate(predicate);

			return predicate;
		}

		private static Expression<Func<T, bool>> negate<T>(Expression<Func<T, bool>> one)
		{
			if (one.Parameters.Count != 1)
				throw new InvalidOperationException("Cannot \"not\" a compound expression");
			var candidateExpr = one.Parameters[0];
			var body = Expression.Not(one.Body);

			return Expression.Lambda<Func<T, bool>>(body, candidateExpr);
		}

		private Expression<Func<T, bool>> constructMethodCallPredicate<T>(object context)
		{
			var type = typeof(T);

			if (type.GetProperty(this.fieldName) == null && type.GetField(this.fieldName) == null)
				throw new MissingMemberException(type.Name, this.fieldName);

			MethodInfo method;
			if (!methodMap.TryGetValue(this.selectionComparison, out method))
				throw new ArgumentOutOfRangeException("selectionComparison", this.selectionComparison, "Invalid filter operation");

			var parameter = Expression.Parameter(type);
			var member = Expression.PropertyOrField(parameter, this.fieldName);
			var value = this.constructConstantExpression<T>(this.rightHandSide, context);

			try
			{
				var converted = (value.Type != member.Type)
					? (Expression)Expression.Convert(value, member.Type)
					: (Expression)value;

				var methodExpression = Expression.Call(member, method, converted);

				var lambda = Expression.Lambda<Func<T, bool>>(methodExpression, parameter);

				return lambda;
			}
			catch (Exception)
			{
				throw new InvalidOperationException(
					String.Format("Cannot convert value \"{0}\" of type \"{1}\" to field \"{2}\" of type \"{3}\"", this.rightHandSide,
						value.Type, this.fieldName, member.Type));
			}
		}

		private Expression<Func<T, bool>> constructStaticCallPredicate<T>(object context)
		{
			var type = typeof(T);

			if (type.GetProperty(this.fieldName) == null && type.GetField(this.fieldName) == null)
				throw new MissingMemberException(type.Name, this.fieldName);

			var parameter = Expression.Parameter(type);
			var member = Expression.PropertyOrField(parameter, this.fieldName);
			var value = this.constructConstantExpression<T>(this.rightHandSide, context);

			try
			{
				var converted = (value.Type != member.Type)
					? (Expression)Expression.Convert(value, member.Type)
					: (Expression)value;

				var methodExpression = this.getStaticCallMethod(this.selectionComparison, member, converted);

				var lambda = Expression.Lambda<Func<T, bool>>(methodExpression, parameter);

				return lambda;
			}
			catch (Exception)
			{
				throw new InvalidOperationException(
					String.Format("Cannot convert value \"{0}\" of type \"{1}\" to field \"{2}\" of type \"{3}\"", this.rightHandSide,
						value.Type, this.fieldName, member.Type));
			}
		}

		private Expression getStaticCallMethod(
			SelectionComparison selectionComparison,
			MemberExpression member,
			Expression converted)
		{
			if (selectionComparison == SelectionComparison.Like)
			{
				var method = typeof(SqlFunctions).GetMethod("PatIndex");

				var methodExpression = Expression.Call(method, converted, member);
				var zeroExpression = Expression.Constant(0, typeof(int?));
				var greaterThanExpression = Expression.GreaterThan(methodExpression, zeroExpression);

				return greaterThanExpression;
			}

			throw new InvalidExpressionException(String.Format("Invalid SelectionComparison {0}", selectionComparison));
		}

		public Expression<Func<T, bool>> constructAnyPredicate<T>(object context)
		{
			var type = typeof(T);

			var parameter = Expression.Parameter(type);

			if (this.innerCriteria == null)
				throw new MissingMemberException("\"Any\" criteria must have an innerCriteria");

			var member = this.getMember<T>(type, parameter);

			var collectionType = member.Type;
			var memberType = collectionType.GenericTypeArguments[0];

			var constructPredicateMethod = typeof(EFQuery).GetMethod("constructPredicate");
			var constructPredicateMethodGeneric = constructPredicateMethod.MakeGenericMethod(memberType);
			var innerPredicate = (Expression)constructPredicateMethodGeneric.Invoke(this.innerCriteria, new object[] { context });

			var call = Expression.Call(typeof(Enumerable), "Any",
				new Type[] { memberType }, new[] { member, innerPredicate });

			var lambda = Expression.Lambda<Func<T, bool>>(call, parameter);

			return lambda;
		}

		public Expression<Func<T, bool>> constructSinglePredicate<T>(object context)
		{
			var type = typeof(T);

			var parameter = Expression.Parameter(type);

			var member = this.getMember<T>(type, parameter);

			var value = this.constructConstantExpression<T>(this.rightHandSide, context);

			ExpressionType operation;
			if (!operationMap.TryGetValue(this.selectionComparison, out operation))
				throw new ArgumentOutOfRangeException("selectionComparison", this.selectionComparison, "Invalid filter operation");

			try
			{
				var converted = (value.Type != member.Type)
					? (Expression)Expression.Convert(value, member.Type)
					: (Expression)value;

				Expression comparison = null;

				if (value.Type == typeof(string))
				{
					if (operation == ExpressionType.GreaterThanOrEqual ||
						operation == ExpressionType.GreaterThan ||
						operation == ExpressionType.LessThanOrEqual ||
						operation == ExpressionType.LessThan)
					{
						MethodInfo method = value.Type.GetMethod("CompareTo", new[] { typeof(string) });
						var zero = Expression.Constant(0);

						var result = Expression.Call(member, method, converted);

						comparison = Expression.MakeBinary(operation, result, zero);
					}
				}

				if (comparison == null)
					comparison = Expression.MakeBinary(operation, member, converted);

				var lambda = Expression.Lambda<Func<T, bool>>(comparison, parameter);

				return lambda;
			}
			catch (Exception)
			{
				throw new InvalidOperationException(
					String.Format("Cannot convert value \"{0}\" of type \"{1}\" to field \"{2}\" of type \"{3}\"", this.rightHandSide,
						value.Type, this.fieldName, member.Type));
			}
		}

		private ConstantExpression constructConstantExpression<T>(object value, object context)
		{
			var sc = value as EFQuery;
			if (sc != null)
			{
				if (sc.isAdd)
					value = this.executeAddExpression<T>(sc, context);
				else if (sc.isConstant)
					value = sc.constantValue;
			}

			value = getConstantValue(value, context);

			if (value == null)
				return Expression.Constant(null);

			return Expression.Constant(value, value.GetType());
		}

		private object executeAddExpression<T>(EFQuery sc, object context)
		{
			object result = null;

			foreach (var arg in sc.aggregateList)
			{
				if (!arg.isConstant)
					throw new ArgumentException(String.Format("{0} must be constant", sc));

				if (result == null)
				{
					result = getConstantValue(arg.constantValue, context);
					continue;
				}

				var value = getConstantValue(arg.constantValue, context);

				// Until we figure out how to make Json.NET deserialize into TimeSpan objects ...
				var s = value as string;
				if (s != null)
				{
					TimeSpan span;
					if (TimeSpan.TryParse(s, out span))
						value = span;
				}

				var leftType = result.GetType();
				var rightType = value.GetType();

				var methodInfo = leftType.GetMethod("Add", new[] {rightType});

				if (methodInfo == null)
					throw new ArgumentException(
						String.Format("Cannot find operation \"Add\" for type \"{0}\"", leftType));

				result = methodInfo.Invoke(result, new[] { value });
			}

			return result;
		}

		private static object getConstantValue(object value, object context)
		{
			string dtKludge = null;
			var s = value as String;
			if (s != null)
			{
				var re = new Regex("^{(.*)}(:.*)?$");
				var match = re.Match(s);
				if (match.Success)
				{
					var matchString = match.Groups[1].Value;
					dtKludge = match.Groups[2].Value;
					if (!String.IsNullOrEmpty(matchString))
					{
						if (matchString == "NOW")
						{
							value = DateTime.Now;
						}
						else if (matchString == "NOW:DATE")
						{
							value = DateTime.Now.Date;
						}
						else if (matchString == "NOW:TIME")
						{
							value = DateTime.Now.TimeOfDay;
						}
						else
						{
							var parts = matchString.Split(new[] {'.'});
							foreach (var part in parts)
							{
								var dict = context as Dictionary<string, object>;
								if (dict != null)
								{
									var val = dict[part];
									context = val;
									value = val;
								}
								else
								{
									var type = context.GetType();
									var prop = type.GetProperty(part);
									if (prop != null)
									{
										var val = prop.GetValue(context, null);
										context = val;
										value = val;
									}
								}
							}
						}
					}
				}
			}

			if (value is DateTime)
			{
				var dt = (DateTime) value;

				if (dtKludge == ":date")
				{
					value = dt.Date;
				}
				else if (dtKludge == ":time")
				{
					value = dt.ToString("HH:mm:ss.fff");
				}
				else if (dtKludge == ":null")
				{
					var f = dt.ToString("HH:mm:ss.fff");
					if (f == "00:00:00.000")
						value = null;
					else
						value = f;
				}
			}

			return value;
		}

		private MemberExpression getMember<T>(Type type, ParameterExpression parameter)
		{
			MemberExpression rVal = null;

			if (this.fieldName.Contains("."))
			{
				var parts = this.fieldName.Split(new[] { '.' });

				foreach (var part in parts)
				{
					if (rVal == null)
					{
						rVal = Expression.PropertyOrField(parameter, part);
					}
					else
					{
						rVal = Expression.PropertyOrField(rVal, part);
					}
				}
			}
			else
			{
				if (type.GetProperty(this.fieldName) == null && type.GetField(this.fieldName) == null)
					throw new MissingMemberException(type.Name, this.fieldName);
				rVal = Expression.PropertyOrField(parameter, this.fieldName);
			}

			return rVal;
		} 
#endregion


#region internal dictionaries

		private static readonly Dictionary<SelectionComparison, ExpressionType> operationMap =
			new Dictionary<SelectionComparison, ExpressionType>
			{
				{ SelectionComparison.Equal, ExpressionType.Equal },
				{ SelectionComparison.NotEqual, ExpressionType.NotEqual },
				{ SelectionComparison.LessThan, ExpressionType.LessThan },
				{ SelectionComparison.LessThanOrEqual, ExpressionType.LessThanOrEqual },
				{ SelectionComparison.GreaterThan, ExpressionType.GreaterThan },
				{ SelectionComparison.GreaterThanOrEqual, ExpressionType.GreaterThanOrEqual },
				{ SelectionComparison.IsTrue, ExpressionType.IsTrue },
				{ SelectionComparison.IsFalse, ExpressionType.IsFalse },
			};


		private static readonly Dictionary<SelectionComparison, MethodInfo> methodMap =
			new Dictionary<SelectionComparison, MethodInfo>
			{
				{ SelectionComparison.Contains, typeof(string).GetMethod("Contains", new[] { typeof(string) }) },
				{ SelectionComparison.StartsWith, typeof(string).GetMethod("StartsWith", new[] { typeof(string) }) },
				{ SelectionComparison.EndsWith, typeof(string).GetMethod("EndsWith", new[] { typeof(string) }) },
			};
#endregion

	}

	public enum SelectionComparison
	{
		Equal,
		NotEqual,
		LessThan,
		LessThanOrEqual,
		GreaterThan,
		GreaterThanOrEqual,
		And,
		Or,
		Nand,
		Nor,
		IsTrue,
		IsFalse,
		Contains,
		StartsWith,
		EndsWith,
		Like,
		Any,
		Constant,
		Add
	};

}

