var EFQuery = (function() {
	'use strict';

	function EFQuery(selectionComparison, fieldName, fieldValue, aggregateList, innerCriteria) {
		this.selectionComparison = selectionComparison;
		this.fieldName = fieldName;
		this.fieldValue = fieldValue;
		this.aggregateList = aggregateList;
		this.innerCriteria = innerCriteria;
	}

	EFQuery.compare = function(comparison, fieldName, fieldValue) {
		return new EFQuery(comparison, fieldName, fieldValue);
	};

	EFQuery.isTrue = function() {
		return new EFQuery("IsTrue");
	};

	EFQuery.isFalse = function() {
		return new EFQuery("IsFalse");
	};

	EFQuery.contains = function(fieldName, fieldValue) {
		return new EFQuery("Contains", fieldName, fieldValue);
	};

	EFQuery.startsWith = function(fieldName, fieldValue) {
		return new EFQuery("StartsWith", fieldName, fieldValue);
	};

	EFQuery.endsWith = function(fieldName, fieldValue) {
		return new EFQuery("EndsWith", fieldName, fieldValue);
	};

	EFQuery.like = function(fieldName, fieldValue) {
		return new EFQuery("Like", fieldName, fieldValue);
	};

	EFQuery.any = function(fieldName, innerCriteria) {
		return new EFQuery("Any", fieldName, null, null, innerCriteria);
	};

	EFQuery.equal = function(fieldName, fieldValue) {
		return new EFQuery("Equal", fieldName, fieldValue);
	};

	EFQuery.notEqual = function(fieldName, fieldValue) {
		return new EFQuery("NotEqual", fieldName, fieldValue);
	};

	EFQuery.greaterThan = function(fieldName, fieldValue) {
		return new EFQuery("GreaterThan", fieldName, fieldValue);
	};

	EFQuery.greaterThanOrEqual = function(fieldName, fieldValue) {
		return new EFQuery("GreaterThanOrEqual", fieldName, fieldValue);
	};

	EFQuery.lessThan = function(fieldName, fieldValue) {
		return new EFQuery("LessThan", fieldName, fieldValue);
	};

	EFQuery.lessThanOrEqual = function(fieldName, fieldValue) {
		return new EFQuery("LessThanOrEqual", fieldName, fieldValue);
	};

	EFQuery.and = function(selectionCriteriae) {
		return new EFQuery("And", undefined, undefined, selectionCriteriae);
	};

	EFQuery.or = function(selectionCriteriae) {
		return new EFQuery("Or", undefined, undefined, selectionCriteriae);
	};

	EFQuery.nand = function(selectionCriteriae) {
		return new EFQuery("Nand", undefined, undefined, selectionCriteriae);
	};

	EFQuery.nor = function(selectionCriteriae) {
		return new EFQuery("Nor", undefined, undefined, selectionCriteriae);
	};

	EFQuery.not = function(selectionCriteria) {
		return new EFQuery("Nand", undefined, undefined, [selectionCriteria]);
	};

	EFQuery.containedIn = function(fieldName, values) {
		var criteriae = [];
		for (var i = 0; i < values.length; i++)
			criteriae.push(EFQuery.equal(fieldName, values[i]));

		return new EFQuery("Or", undefined, undefined, criteriae);
	};

	EFQuery.between = function(fieldName, left, right) {
		var criteriae = [
			EFQuery.greaterThanOrEqual(fieldName, left),
			EFQuery.lessThanOrEqual(fieldName, right)
		];
		return new EFQuery("And", undefined, undefined, criteriae);
	};

	return EFQuery;

})();

