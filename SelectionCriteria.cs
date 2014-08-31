using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PeteMontgomery.PredicateBuilder;

namespace JeffDege.SelectionCriteria
{
	public class SelectionCriteria
	{
		#region static convenience functions

		public static SelectionCriteria compare(SelectionComparison comparison, string fieldName, object fieldValue)
		{
			return new SelectionCriteria
			{
				selectionComparison = comparison,
				fieldName = fieldName,
				fieldValue = fieldValue
			};
		}

		/// <summary>
		/// returns a SelectionCriteria that is always true
		/// </summary>
		/// <returns>SelectionCriteria</returns>
		public static SelectionCriteria isTrue()
		{
			return new SelectionCriteria { selectionComparison = SelectionComparison.IsTrue };
		}

		/// <summary>
		/// returns a SelectionCriteria that is always false
		/// </summary>
		/// <returns>SelectionCriteria</returns>
		public static SelectionCriteria isFalse()
		{
			return new SelectionCriteria { selectionComparison = SelectionComparison.IsFalse };
		}

		/// <summary>
		/// returns a SelectionCriteria that is true if fieldName contains fieldValue as a substring
		/// </summary>
		/// <returns>SelectionCriteria</returns>
		public static SelectionCriteria contains(string fieldName, object fieldValue)
		{
			return compare(SelectionComparison.Contains, fieldName, fieldValue);
		}

		/// <summary>
		/// returns a SelectionCriteria that is true if fieldName starts with fieldValue as a substring
		/// </summary>
		/// <returns>SelectionCriteria</returns>
		public static SelectionCriteria startsWith(string fieldName, object fieldValue)
		{
			return compare(SelectionComparison.StartsWith, fieldName, fieldValue);
		}

		/// <summary>
		/// returns a SelectionCriteria that is true if fieldName ends with fieldValue as a substring
		/// </summary>
		/// <returns>SelectionCriteria</returns>
		public static SelectionCriteria endsWith(string fieldName, object fieldValue)
		{
			return compare(SelectionComparison.EndsWith, fieldName, fieldValue);
		}

		/// <summary>
		/// returns a SelectionCriteria that is true if fieldName matches fieldValue as a SQL LIKE pattern
		/// </summary>
		/// <returns>SelectionCriteria</returns>
		public static SelectionCriteria like(string fieldName, object fieldValue)
		{
			return compare(SelectionComparison.Like, fieldName, fieldValue);
		}

		/// <summary>
		/// returns a SelectionCriteria that is true if the innerCriteria evaluates true
		/// on the fieldName. (Assumes fieldName is a one-to-many navigational property).
		/// </summary>
		/// <returns>SelectionCriteria</returns>
		public static SelectionCriteria any(string fieldName, SelectionCriteria innerCriteria)
		{
			return new SelectionCriteria
			{
				selectionComparison = SelectionComparison.Any,
				fieldName = fieldName,
				innerCriteria = innerCriteria
			};
		}

		/// <summary>
		/// returns a SelectionCriteria that is true if fieldName is equal to fieldValue
		/// </summary>
		/// <returns>SelectionCriteria</returns>
		public static SelectionCriteria equal(string fieldName, object fieldValue)
		{
			return compare(SelectionComparison.Equal, fieldName, fieldValue);
		}

		/// <summary>
		/// returns a SelectionCriteria that is true if fieldName is not equal to fieldValue
		/// </summary>
		/// <returns>SelectionCriteria</returns>
		public static SelectionCriteria notEqual(string fieldName, object fieldValue)
		{
			return compare(SelectionComparison.NotEqual, fieldName, fieldValue);
		}

		/// <summary>
		/// returns a SelectionCriteria that is true if fieldName is greater than fieldValue
		/// </summary>
		/// <returns>SelectionCriteria</returns>
		public static SelectionCriteria greaterThan(string fieldName, object fieldValue)
		{
			return compare(SelectionComparison.GreaterThan, fieldName, fieldValue);
		}

		/// <summary>
		/// returns a SelectionCriteria that is true if fieldName is greater than or equal to fieldValue
		/// </summary>
		/// <returns>SelectionCriteria</returns>
		public static SelectionCriteria greaterThanOrEqual(string fieldName, object fieldValue)
		{
			return compare(SelectionComparison.GreaterThanOrEqual, fieldName, fieldValue);
		}

		/// <summary>
		/// returns a SelectionCriteria that is true if fieldName is less than fieldValue
		/// </summary>
		/// <returns>SelectionCriteria</returns>
		public static SelectionCriteria lessThan(string fieldName, object fieldValue)
		{
			return compare(SelectionComparison.LessThan, fieldName, fieldValue);
		}

		/// <summary>
		/// returns a SelectionCriteria that is true if fieldName is less than or equal to fieldValue
		/// </summary>
		/// <returns>SelectionCriteria</returns>
		public static SelectionCriteria lessThanOrEqual(string fieldName, object fieldValue)
		{
			return compare(SelectionComparison.LessThanOrEqual, fieldName, fieldValue);
		}

		/// <summary>
		/// returns a SelectionCriteria that is true if all of the selectionCriteriae evaluate true
		/// </summary>
		/// <returns>SelectionCriteria</returns>
		public static SelectionCriteria and(IEnumerable<SelectionCriteria> selectionCriteriae)
		{
			return new SelectionCriteria
			{
				selectionComparison = SelectionComparison.And,
				aggregateList = selectionCriteriae
			};
		}

		/// <summary>
		/// returns a SelectionCriteria that is true if at least one of the selectionCriteriae evaluates true
		/// </summary>
		/// <returns>SelectionCriteria</returns>
		public static SelectionCriteria or(IEnumerable<SelectionCriteria> selectionCriteriae)
		{
			return new SelectionCriteria
			{
				selectionComparison = SelectionComparison.Or,
				aggregateList = selectionCriteriae
			};
		}

		/// <summary>
		/// returns a SelectionCriteria that is true if at least one the selectionCriteriae evaluates false
		/// </summary>
		/// <returns>SelectionCriteria</returns>
		public static SelectionCriteria nand(IEnumerable<SelectionCriteria> selectionCriteriae)
		{
			return new SelectionCriteria
			{
				selectionComparison = SelectionComparison.Nand,
				aggregateList = selectionCriteriae
			};
		}

		/// <summary>
		/// returns a SelectionCriteria that is true if all of the selectionCriteriae evaluate false
		/// </summary>
		/// <returns>SelectionCriteria</returns>
		public static SelectionCriteria nor(IEnumerable<SelectionCriteria> selectionCriteriae)
		{
			return new SelectionCriteria
			{
				selectionComparison = SelectionComparison.Nor,
				aggregateList = selectionCriteriae
			};
		}

		/// <summary>
		/// returns a SelectionCriteria that is true if all of the selectionCriteria evaluates false
		/// </summary>
		/// <returns>SelectionCriteria</returns>
		public static SelectionCriteria not(SelectionCriteria selectionCriteria)
		{
			return nand(new[] { selectionCriteria });
		}

		/// <summary>
		/// returns a SelectionCriteria that is true if fieldName is equal to one of the objects
		/// </summary>
		/// <returns>SelectionCriteria</returns>
		public static SelectionCriteria containedIn(string fieldName, IEnumerable<object> objects)
		{
			return or(objects.Select(o => equal(fieldName, o)));
		}

		/// <summary>
		/// returns a SelectionCriteria that is true if fieldName is greatder than or equal to left
		/// and less than or equal to right
		/// </summary>
		/// <returns>SelectionCriteria</returns>
		public static SelectionCriteria between(string fieldName, object left, object right)
		{
			return and(new[]
			{
				greaterThanOrEqual(fieldName, left),
				lessThanOrEqual(fieldName, right)
			});
		}

		#endregion

		#region Member variables

		[JsonConverter(typeof(StringEnumConverter))]
		public SelectionComparison selectionComparison { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string fieldName { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public object fieldValue { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public SelectionCriteria innerCriteria { get; set; }

		private IEnumerable<SelectionCriteria> aggregateList_ = null;

		public IEnumerable<SelectionCriteria> aggregateList
		{
			get { return this.aggregateList_ ?? (this.aggregateList_ = new List<SelectionCriteria>()); }
			set { this.aggregateList_ = value; }
		}

		#endregion

		#region Constructors

		public SelectionCriteria()
		{
			// We default to Equal because that's our most common pattern:
			this.selectionComparison = SelectionComparison.Equal;
		}

		public SelectionCriteria(SelectionComparison selectionComparison, string fieldName, object fieldValue)
		{
			this.selectionComparison = selectionComparison;
			this.fieldName = fieldName;
			this.fieldValue = fieldValue;
		}

		#endregion

		// This just tells JSON.net whether to include aggregateList when serializing an object
		public bool ShouldSerializeaggregateList()
		{
			return this.aggregateList_ != null;
		}

		#region processing types
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

		#region predicate constructors
		public Expression<Func<T, bool>> constructPredicate<T>()
		{
			if (this.isUnary)
				return this.selectionComparison == SelectionComparison.IsTrue
					? PredicateBuilder.True<T>()
					: PredicateBuilder.False<T>();

			if (this.isAggregate)
				return this.constructAggregatePredicate<T>();

			if (this.isMethodCall)
				return this.constructMethodCallPredicate<T>();

			if (this.isStaticCall)
				return this.constructStaticCallPredicate<T>();

			if (this.isAny)
				return this.constructAnyPredicate<T>();

			return this.constructSinglePredicate<T>();
		}

		private Expression<Func<T, bool>> constructAggregatePredicate<T>()
		{
			var predicate = this.isAnd ? PredicateBuilder.True<T>() : PredicateBuilder.False<T>();

			foreach (var item in this.aggregateList)
			{
				predicate = this.isAnd ? predicate.And(item.constructPredicate<T>()) : predicate.Or(item.constructPredicate<T>());
			}

			if (this.isNegate)
				predicate = negate(predicate);

			return predicate;
		}

		public static Expression<Func<T, bool>> negate<T>(Expression<Func<T, bool>> one)
		{
			if (one.Parameters.Count != 1)
				throw new InvalidOperationException("Cannot \"not\" a compound expression");
			var candidateExpr = one.Parameters[0];
			var body = Expression.Not(one.Body);

			return Expression.Lambda<Func<T, bool>>(body, candidateExpr);
		}

		private Expression<Func<T, bool>> constructMethodCallPredicate<T>()
		{
			var type = typeof(T);

			if (type.GetProperty(this.fieldName) == null && type.GetField(this.fieldName) == null)
				throw new MissingMemberException(type.Name, this.fieldName);

			MethodInfo method;
			if (!methodMap.TryGetValue(this.selectionComparison, out method))
				throw new ArgumentOutOfRangeException("selectionComparison", this.selectionComparison, "Invalid filter operation");

			var parameter = Expression.Parameter(type);
			var member = Expression.PropertyOrField(parameter, this.fieldName);
			var value = (this.fieldValue == null)
				? Expression.Constant(null)
				: Expression.Constant(this.fieldValue, this.fieldValue.GetType());

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
					String.Format("Cannot convert value \"{0}\" of type \"{1}\" to field \"{2}\" of type \"{3}\"", this.fieldValue,
						value.Type, this.fieldName, member.Type));
			}
		}

		private Expression<Func<T, bool>> constructStaticCallPredicate<T>()
		{
			var type = typeof(T);

			if (type.GetProperty(this.fieldName) == null && type.GetField(this.fieldName) == null)
				throw new MissingMemberException(type.Name, this.fieldName);

			var parameter = Expression.Parameter(type);
			var member = Expression.PropertyOrField(parameter, this.fieldName);
			var value = (this.fieldValue == null)
				? Expression.Constant(null)
				: Expression.Constant(this.fieldValue, this.fieldValue.GetType());

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
					String.Format("Cannot convert value \"{0}\" of type \"{1}\" to field \"{2}\" of type \"{3}\"", this.fieldValue,
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

		private Expression<Func<T, bool>> constructAnyPredicate<T>()
		{
			var type = typeof(T);

			var parameter = Expression.Parameter(type);

			if (this.innerCriteria == null)
				throw new MissingMemberException("\"Any\" criteria must have an innerCriteria");

			var member = this.getMember<T>(type, parameter);

			var collectionType = member.Type;
			var memberType = collectionType.GenericTypeArguments[0];

			var constructPredicateMethod = typeof(SelectionCriteria).GetMethod("constructPredicate");
			var constructPredicateMethodGeneric = constructPredicateMethod.MakeGenericMethod(memberType);
			var innerPredicate = (Expression)constructPredicateMethodGeneric.Invoke(this.innerCriteria, null);

			var call = Expression.Call(typeof(Enumerable), "Any",
				new Type[] { memberType }, new[] { member, innerPredicate });

			var lambda = Expression.Lambda<Func<T, bool>>(call, parameter);

			return lambda;
		}

		private Expression<Func<T, bool>> constructSinglePredicate<T>()
		{
			var type = typeof(T);

			var parameter = Expression.Parameter(type);

			var member = this.getMember<T>(type, parameter);

			var value = (this.fieldValue == null)
				? Expression.Constant(null)
				: Expression.Constant(this.fieldValue, this.fieldValue.GetType());

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
					String.Format("Cannot convert value \"{0}\" of type \"{1}\" to field \"{2}\" of type \"{3}\"", this.fieldValue,
						value.Type, this.fieldName, member.Type));
			}
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
		Any
	};

}
