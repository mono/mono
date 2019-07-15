using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace System.Data.Linq
{
	// this class is somehow generated in referencesource, manually created an equivalent for now
	// TODO: we need to make sure we throw the proper exception for each method
	internal class Error
	{
		public static Exception VbLikeDoesNotSupportMultipleCharacterRanges()
		{
			return new Exception (SR.VbLikeDoesNotSupportMultipleCharacterRanges);
		}
		public static Exception VbLikeUnclosedBracket()
		{
			return new Exception (SR.VbLikeUnclosedBracket);
		}
		public static Exception UnrecognizedProviderMode(object p0)
		{
			return new Exception (SR.Format (SR.UnrecognizedProviderMode, p0));
		}
		public static Exception CompiledQueryCannotReturnType(object p0)
		{
			return new Exception (SR.Format (SR.CompiledQueryCannotReturnType, p0));
		}
		public static Exception ArgumentEmpty(object p0)
		{
			return new Exception (SR.Format (SR.ArgumentEmpty, p0));
		}
		public static Exception ProviderCannotBeUsedAfterDispose()
		{
			return new Exception (SR.ProviderCannotBeUsedAfterDispose);
		}
		public static Exception ArgumentTypeMismatch(object p0)
		{
			return new Exception (SR.Format (SR.ArgumentTypeMismatch, p0));
		}
		public static Exception ContextNotInitialized()
		{
			return new Exception (SR.ContextNotInitialized);
		}
		public static Exception CouldNotDetermineSqlType(object p0)
		{
			return new Exception (SR.Format (SR.CouldNotDetermineSqlType, p0));
		}
		public static Exception CouldNotDetermineDbGeneratedSqlType(object p0)
		{
			return new Exception (SR.Format (SR.CouldNotDetermineDbGeneratedSqlType, p0));
		}
		public static Exception CouldNotDetermineCatalogName()
		{
			return new Exception (SR.CouldNotDetermineCatalogName);
		}
		public static Exception CreateDatabaseFailedBecauseOfClassWithNoMembers(object p0)
		{
			return new Exception (SR.Format (SR.CreateDatabaseFailedBecauseOfClassWithNoMembers, p0));
		}
		public static Exception CreateDatabaseFailedBecauseOfContextWithNoTables(object p0)
		{
			return new Exception (SR.Format (SR.CreateDatabaseFailedBecauseOfContextWithNoTables, p0));
		}
		public static Exception CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists(object p0)
		{
			return new Exception (SR.Format (SR.CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists, p0));
		}
		public static Exception DistributedTransactionsAreNotAllowed()
		{
			return new Exception (SR.DistributedTransactionsAreNotAllowed);
		}
		public static Exception InvalidConnectionArgument(object p0)
		{
			return new Exception (SR.Format (SR.InvalidConnectionArgument, p0));
		}
		public static Exception CannotEnumerateResultsMoreThanOnce()
		{
			return new Exception (SR.CannotEnumerateResultsMoreThanOnce);
		}
		public static Exception IifReturnTypesMustBeEqual(object p0, object p1)
		{
			return new Exception (SR.Format (SR.IifReturnTypesMustBeEqual, p0, p1));
		}
		public static Exception MethodNotMappedToStoredProcedure(object p0)
		{
			return new Exception (SR.Format (SR.MethodNotMappedToStoredProcedure, p0));
		}
		public static Exception ResultTypeNotMappedToFunction(object p0, object p1)
		{
			return new Exception (SR.Format (SR.ResultTypeNotMappedToFunction, p0, p1));
		}
		public static Exception ToStringOnlySupportedForPrimitiveTypes()
		{
			return new Exception (SR.ToStringOnlySupportedForPrimitiveTypes);
		}
		public static Exception TransactionDoesNotMatchConnection()
		{
			return new Exception (SR.TransactionDoesNotMatchConnection);
		}
		public static Exception UnexpectedTypeCode(object p0)
		{
			return new Exception (SR.Format (SR.UnexpectedTypeCode, p0));
		}
		public static Exception UnsupportedDateTimeConstructorForm()
		{
			return new Exception (SR.UnsupportedDateTimeConstructorForm);
		}
		public static Exception UnsupportedDateTimeOffsetConstructorForm()
		{
			return new Exception (SR.UnsupportedDateTimeOffsetConstructorForm);
		}
		public static Exception UnsupportedStringConstructorForm()
		{
			return new Exception (SR.UnsupportedStringConstructorForm);
		}
		public static Exception UnsupportedTimeSpanConstructorForm()
		{
			return new Exception (SR.UnsupportedTimeSpanConstructorForm);
		}
		public static Exception UnsupportedTypeConstructorForm(object p0)
		{
			return new Exception (SR.Format (SR.UnsupportedTypeConstructorForm, p0));
		}
		public static Exception WrongNumberOfValuesInCollectionArgument(object p0, object p1, object p2)
		{
			return new Exception (SR.Format (SR.WrongNumberOfValuesInCollectionArgument, p0, p1, p2));
		}
		public static Exception MemberCannotBeTranslated(object p0, object p1)
		{
			return new Exception (SR.Format (SR.MemberCannotBeTranslated, p0, p1));
		}
		public static Exception NonConstantExpressionsNotSupportedFor(object p0)
		{
			return new Exception (SR.Format (SR.NonConstantExpressionsNotSupportedFor, p0));
		}
		public static Exception MathRoundNotSupported()
		{
			return new Exception (SR.MathRoundNotSupported);
		}
		public static Exception SqlMethodOnlyForSql(object p0)
		{
			return new Exception (SR.Format (SR.SqlMethodOnlyForSql, p0));
		}
		public static Exception NonConstantExpressionsNotSupportedForRounding()
		{
			return new Exception (SR.NonConstantExpressionsNotSupportedForRounding);
		}
		public static Exception CompiledQueryAgainstMultipleShapesNotSupported()
		{
			return new Exception (SR.CompiledQueryAgainstMultipleShapesNotSupported);
		}
		public static Exception IndexOfWithStringComparisonArgNotSupported()
		{
			return new Exception (SR.IndexOfWithStringComparisonArgNotSupported);
		}
		public static Exception LastIndexOfWithStringComparisonArgNotSupported()
		{
			return new Exception (SR.LastIndexOfWithStringComparisonArgNotSupported);
		}
		public static Exception ConvertToCharFromBoolNotSupported()
		{
			return new Exception (SR.ConvertToCharFromBoolNotSupported);
		}
		public static Exception ConvertToDateTimeOnlyForDateTimeOrString()
		{
			return new Exception (SR.ConvertToDateTimeOnlyForDateTimeOrString);
		}
		public static Exception SkipIsValidOnlyOverOrderedQueries()
		{
			return new Exception (SR.SkipIsValidOnlyOverOrderedQueries);
		}
		public static Exception SkipRequiresSingleTableQueryWithPKs()
		{
			return new Exception (SR.SkipRequiresSingleTableQueryWithPKs);
		}
		public static Exception NoMethodInTypeMatchingArguments(object p0)
		{
			return new Exception (SR.Format (SR.NoMethodInTypeMatchingArguments, p0));
		}
		public static Exception CannotConvertToEntityRef(object p0)
		{
			return new Exception (SR.Format (SR.CannotConvertToEntityRef, p0));
		}
		public static Exception ExpressionNotDeferredQuerySource()
		{
			return new Exception (SR.ExpressionNotDeferredQuerySource);
		}
		public static Exception DeferredMemberWrongType()
		{
			return new Exception (SR.DeferredMemberWrongType);
		}
		public static Exception ArgumentWrongType(object p0, object p1, object p2)
		{
			return new Exception (SR.Format (SR.ArgumentWrongType, p0, p1, p2));
		}
		public static Exception ArgumentWrongValue(object p0)
		{
			return new Exception (SR.Format (SR.ArgumentWrongValue, p0));
		}
		public static Exception BadProjectionInSelect()
		{
			return new Exception (SR.BadProjectionInSelect);
		}
		public static Exception InvalidReturnFromSproc(object p0)
		{
			return new Exception (SR.Format (SR.InvalidReturnFromSproc, p0));
		}
		public static Exception WrongDataContext()
		{
			return new Exception (SR.WrongDataContext);
		}
		public static Exception BinaryOperatorNotRecognized(object p0)
		{
			return new Exception (SR.Format (SR.BinaryOperatorNotRecognized, p0));
		}
		public static Exception CannotAggregateType(object p0)
		{
			return new Exception (SR.Format (SR.CannotAggregateType, p0));
		}
		public static Exception CannotCompareItemsAssociatedWithDifferentTable()
		{
			return new Exception (SR.CannotCompareItemsAssociatedWithDifferentTable);
		}
		public static Exception CannotDeleteTypesOf(object p0)
		{
			return new Exception (SR.Format (SR.CannotDeleteTypesOf, p0));
		}
		public static Exception ClassLiteralsNotAllowed(object p0)
		{
			return new Exception (SR.Format (SR.ClassLiteralsNotAllowed, p0));
		}
		public static Exception ClientCaseShouldNotHold(object p0)
		{
			return new Exception (SR.Format (SR.ClientCaseShouldNotHold, p0));
		}
		public static Exception ClrBoolDoesNotAgreeWithSqlType(object p0)
		{
			return new Exception (SR.Format (SR.ClrBoolDoesNotAgreeWithSqlType, p0));
		}
		public static Exception ColumnCannotReferToItself()
		{
			return new Exception (SR.ColumnCannotReferToItself);
		}
		public static Exception ColumnClrTypeDoesNotAgreeWithExpressionsClrType()
		{
			return new Exception (SR.ColumnClrTypeDoesNotAgreeWithExpressionsClrType);
		}
		public static Exception ColumnIsDefinedInMultiplePlaces(object p0)
		{
			return new Exception (SR.Format (SR.ColumnIsDefinedInMultiplePlaces, p0));
		}
		public static Exception ColumnIsNotAccessibleThroughGroupBy(object p0)
		{
			return new Exception (SR.Format (SR.ColumnIsNotAccessibleThroughGroupBy, p0));
		}
		public static Exception ColumnIsNotAccessibleThroughDistinct(object p0)
		{
			return new Exception (SR.Format (SR.ColumnIsNotAccessibleThroughDistinct, p0));
		}
		public static Exception ColumnReferencedIsNotInScope(object p0)
		{
			return new Exception (SR.Format (SR.ColumnReferencedIsNotInScope, p0));
		}
		public static Exception ConstructedArraysNotSupported()
		{
			return new Exception (SR.ConstructedArraysNotSupported);
		}
		public static Exception ParametersCannotBeSequences()
		{
			return new Exception (SR.ParametersCannotBeSequences);
		}
		public static Exception CapturedValuesCannotBeSequences()
		{
			return new Exception (SR.CapturedValuesCannotBeSequences);
		}
		public static Exception IQueryableCannotReturnSelfReferencingConstantExpression()
		{
			return new Exception (SR.IQueryableCannotReturnSelfReferencingConstantExpression);
		}
		public static Exception CouldNotAssignSequence(object p0, object p1)
		{
			return new Exception (SR.Format (SR.CouldNotAssignSequence, p0, p1));
		}
		public static Exception CouldNotTranslateExpressionForReading(object p0)
		{
			return new Exception (SR.Format (SR.CouldNotTranslateExpressionForReading, p0));
		}
		public static Exception CouldNotGetClrType()
		{
			return new Exception (SR.CouldNotGetClrType);
		}
		public static Exception CouldNotGetSqlType()
		{
			return new Exception (SR.CouldNotGetSqlType);
		}
		public static Exception CouldNotHandleAliasRef(object p0)
		{
			return new Exception (SR.Format (SR.CouldNotHandleAliasRef, p0));
		}
		public static Exception DidNotExpectAs(object p0)
		{
			return new Exception (SR.Format (SR.DidNotExpectAs, p0));
		}
		public static Exception DidNotExpectTypeBinding()
		{
			return new Exception (SR.DidNotExpectTypeBinding);
		}
		public static Exception DidNotExpectTypeChange(object p0, object p1)
		{
			return new Exception (SR.Format (SR.DidNotExpectTypeChange, p0, p1));
		}
		public static Exception EmptyCaseNotSupported()
		{
			return new Exception (SR.EmptyCaseNotSupported);
		}
		public static Exception ExpectedNoObjectType()
		{
			return new Exception (SR.ExpectedNoObjectType);
		}
		public static Exception ExpectedBitFoundPredicate()
		{
			return new Exception (SR.ExpectedBitFoundPredicate);
		}
		public static Exception ExpectedClrTypesToAgree(object p0, object p1)
		{
			return new Exception (SR.Format (SR.ExpectedClrTypesToAgree, p0, p1));
		}
		public static Exception ExpectedPredicateFoundBit()
		{
			return new Exception (SR.ExpectedPredicateFoundBit);
		}
		public static Exception ExpectedQueryableArgument(object p0, object p1, object p2)
		{
			return new Exception (SR.Format (SR.ExpectedQueryableArgument, p0, p1, p2));
		}
		public static Exception InvalidGroupByExpressionType(object p0)
		{
			return new Exception (SR.Format (SR.InvalidGroupByExpressionType, p0));
		}
		public static Exception InvalidGroupByExpression()
		{
			return new Exception (SR.InvalidGroupByExpression);
		}
		public static Exception InvalidOrderByExpression(object p0)
		{
			return new Exception (SR.Format (SR.InvalidOrderByExpression, p0));
		}
		public static Exception Impossible()
		{
			return new Exception (SR.Impossible);
		}
		public static Exception InfiniteDescent()
		{
			return new Exception (SR.InfiniteDescent);
		}
		public static Exception InvalidFormatNode(object p0)
		{
			return new Exception (SR.Format (SR.InvalidFormatNode, p0));
		}
		public static Exception InvalidReferenceToRemovedAliasDuringDeflation()
		{
			return new Exception (SR.InvalidReferenceToRemovedAliasDuringDeflation);
		}
		public static Exception InvalidSequenceOperatorCall(object p0)
		{
			return new Exception (SR.Format (SR.InvalidSequenceOperatorCall, p0));
		}
		public static Exception ParameterNotInScope(object p0)
		{
			return new Exception (SR.Format (SR.ParameterNotInScope, p0));
		}
		public static Exception MemberAccessIllegal(object p0, object p1, object p2)
		{
			return new Exception (SR.Format (SR.MemberAccessIllegal, p0, p1, p2));
		}
		public static Exception MemberCouldNotBeTranslated(object p0, object p1)
		{
			return new Exception (SR.Format (SR.MemberCouldNotBeTranslated, p0, p1));
		}
		public static Exception MemberNotPartOfProjection(object p0, object p1)
		{
			return new Exception (SR.Format (SR.MemberNotPartOfProjection, p0, p1));
		}
		public static Exception MethodHasNoSupportConversionToSql(object p0)
		{
			return new Exception (SR.Format (SR.MethodHasNoSupportConversionToSql, p0));
		}
		public static Exception MethodFormHasNoSupportConversionToSql(object p0, object p1)
		{
			return new Exception (SR.Format (SR.MethodFormHasNoSupportConversionToSql, p0, p1));
		}
		public static Exception UnableToBindUnmappedMember(object p0, object p1, object p2)
		{
			return new Exception (SR.Format (SR.UnableToBindUnmappedMember, p0, p1, p2));
		}
		public static Exception QueryOperatorNotSupported(object p0)
		{
			return new Exception (SR.Format (SR.QueryOperatorNotSupported, p0));
		}
		public static Exception QueryOperatorOverloadNotSupported(object p0)
		{
			return new Exception (SR.Format (SR.QueryOperatorOverloadNotSupported, p0));
		}
		public static Exception ReaderUsedAfterDispose()
		{
			return new Exception (SR.ReaderUsedAfterDispose);
		}
		public static Exception RequiredColumnDoesNotExist(object p0)
		{
			return new Exception (SR.Format (SR.RequiredColumnDoesNotExist, p0));
		}
		public static Exception SimpleCaseShouldNotHold(object p0)
		{
			return new Exception (SR.Format (SR.SimpleCaseShouldNotHold, p0));
		}
		public static Exception TypeBinaryOperatorNotRecognized()
		{
			return new Exception (SR.TypeBinaryOperatorNotRecognized);
		}
		public static Exception UnexpectedNode(object p0)
		{
			return new Exception (SR.Format (SR.UnexpectedNode, p0));
		}
		public static Exception UnexpectedFloatingColumn()
		{
			return new Exception (SR.UnexpectedFloatingColumn);
		}
		public static Exception UnexpectedSharedExpression()
		{
			return new Exception (SR.UnexpectedSharedExpression);
		}
		public static Exception UnexpectedSharedExpressionReference()
		{
			return new Exception (SR.UnexpectedSharedExpressionReference);
		}
		public static Exception UnhandledBindingType(object p0)
		{
			return new Exception (SR.Format (SR.UnhandledBindingType, p0));
		}
		public static Exception UnhandledStringTypeComparison()
		{
			return new Exception (SR.UnhandledStringTypeComparison);
		}
		public static Exception UnhandledMemberAccess(object p0, object p1)
		{
			return new Exception (SR.Format (SR.UnhandledMemberAccess, p0, p1));
		}
		public static Exception UnmappedDataMember(object p0, object p1, object p2)
		{
			return new Exception (SR.Format (SR.UnmappedDataMember, p0, p1, p2));
		}
		public static Exception UnrecognizedExpressionNode(object p0)
		{
			return new Exception (SR.Format (SR.UnrecognizedExpressionNode, p0));
		}
		public static Exception ValueHasNoLiteralInSql(object p0)
		{
			return new Exception (SR.Format (SR.ValueHasNoLiteralInSql, p0));
		}
		public static Exception UnionIncompatibleConstruction()
		{
			return new Exception (SR.UnionIncompatibleConstruction);
		}
		public static Exception UnionDifferentMembers()
		{
			return new Exception (SR.UnionDifferentMembers);
		}
		public static Exception UnionDifferentMemberOrder()
		{
			return new Exception (SR.UnionDifferentMemberOrder);
		}
		public static Exception UnionOfIncompatibleDynamicTypes()
		{
			return new Exception (SR.UnionOfIncompatibleDynamicTypes);
		}
		public static Exception UnionWithHierarchy()
		{
			return new Exception (SR.UnionWithHierarchy);
		}
		public static Exception UnhandledExpressionType(object p0)
		{
			return new Exception (SR.Format (SR.UnhandledExpressionType, p0));
		}
		public static Exception IntersectNotSupportedForHierarchicalTypes()
		{
			return new Exception (SR.IntersectNotSupportedForHierarchicalTypes);
		}
		public static Exception ExceptNotSupportedForHierarchicalTypes()
		{
			return new Exception (SR.ExceptNotSupportedForHierarchicalTypes);
		}
		public static Exception NonCountAggregateFunctionsAreNotValidOnProjections(object p0)
		{
			return new Exception (SR.Format (SR.NonCountAggregateFunctionsAreNotValidOnProjections, p0));
		}
		public static Exception GroupingNotSupportedAsOrderCriterion()
		{
			return new Exception (SR.GroupingNotSupportedAsOrderCriterion);
		}
		public static Exception SelectManyDoesNotSupportStrings()
		{
			return new Exception (SR.SelectManyDoesNotSupportStrings);
		}
		public static Exception SequenceOperatorsNotSupportedForType(object p0)
		{
			return new Exception (SR.Format (SR.SequenceOperatorsNotSupportedForType, p0));
		}
		public static Exception SkipNotSupportedForSequenceTypes()
		{
			return new Exception (SR.SkipNotSupportedForSequenceTypes);
		}
		public static Exception ComparisonNotSupportedForType(object p0)
		{
			return new Exception (SR.Format (SR.ComparisonNotSupportedForType, p0));
		}
		public static Exception QueryOnLocalCollectionNotSupported()
		{
			return new Exception (SR.QueryOnLocalCollectionNotSupported);
		}
		public static Exception UnsupportedNodeType(object p0)
		{
			return new Exception (SR.Format (SR.UnsupportedNodeType, p0));
		}
		public static Exception TypeColumnWithUnhandledSource()
		{
			return new Exception (SR.TypeColumnWithUnhandledSource);
		}
		public static Exception GeneralCollectionMaterializationNotSupported()
		{
			return new Exception (SR.GeneralCollectionMaterializationNotSupported);
		}
		public static Exception TypeCannotBeOrdered(object p0)
		{
			return new Exception (SR.Format (SR.TypeCannotBeOrdered, p0));
		}
		public static Exception InvalidMethodExecution(object p0)
		{
			return new Exception (SR.Format (SR.InvalidMethodExecution, p0));
		}
		public static Exception SprocsCannotBeComposed()
		{
			return new Exception (SR.SprocsCannotBeComposed);
		}
		public static Exception InsertItemMustBeConstant()
		{
			return new Exception (SR.InsertItemMustBeConstant);
		}
		public static Exception UpdateItemMustBeConstant()
		{
			return new Exception (SR.UpdateItemMustBeConstant);
		}
		public static Exception CouldNotConvertToPropertyOrField(object p0)
		{
			return new Exception (SR.Format (SR.CouldNotConvertToPropertyOrField, p0));
		}
		public static Exception BadParameterType(object p0)
		{
			return new Exception (SR.Format (SR.BadParameterType, p0));
		}
		public static Exception CannotAssignToMember(object p0)
		{
			return new Exception (SR.Format (SR.CannotAssignToMember, p0));
		}
		public static Exception MappedTypeMustHaveDefaultConstructor(object p0)
		{
			return new Exception (SR.Format (SR.MappedTypeMustHaveDefaultConstructor, p0));
		}
		public static Exception UnsafeStringConversion(object p0, object p1)
		{
			return new Exception (SR.Format (SR.UnsafeStringConversion, p0, p1));
		}
		public static Exception CannotAssignNull(object p0)
		{
			return new Exception (SR.Format (SR.CannotAssignNull, p0));
		}
		public static Exception ProviderNotInstalled(object p0, object p1)
		{
			return new Exception (SR.Format (SR.ProviderNotInstalled, p0, p1));
		}
		public static Exception InvalidProviderType(object p0)
		{
			return new Exception (SR.Format (SR.InvalidProviderType, p0));
		}
		public static Exception InvalidDbGeneratedType(object p0)
		{
			return new Exception (SR.Format (SR.InvalidDbGeneratedType, p0));
		}
		public static Exception DatabaseDeleteThroughContext()
		{
			return new Exception (SR.DatabaseDeleteThroughContext);
		}
		public static Exception CannotMaterializeEntityType(object p0)
		{
			return new Exception (SR.Format (SR.CannotMaterializeEntityType, p0));
		}
		public static Exception CannotMaterializeList(object p0)
		{
			return new Exception (SR.Format (SR.CannotMaterializeList, p0));
		}
		public static Exception CouldNotConvert(object p0, object p1)
		{
			return new Exception (SR.Format (SR.CouldNotConvert, p0, p1));
		}
		public static Exception CannotAddChangeConflicts()
		{
			return new Exception (SR.CannotAddChangeConflicts);
		}
		public static Exception CannotRemoveChangeConflicts()
		{
			return new Exception (SR.CannotRemoveChangeConflicts);
		}
		public static Exception InconsistentAssociationAndKeyChange(object p0, object p1)
		{
			return new Exception (SR.Format (SR.InconsistentAssociationAndKeyChange, p0, p1));
		}
		public static Exception UnableToDetermineDataContext()
		{
			return new Exception (SR.UnableToDetermineDataContext);
		}
		public static Exception ArgumentTypeHasNoIdentityKey(object p0)
		{
			return new Exception (SR.Format (SR.ArgumentTypeHasNoIdentityKey, p0));
		}
		public static Exception CannotRemoveUnattachedEntity()
		{
			return new Exception (SR.CannotRemoveUnattachedEntity);
		}
		public static Exception ColumnMappedMoreThanOnce(object p0)
		{
			return new Exception (SR.Format (SR.ColumnMappedMoreThanOnce, p0));
		}
		public static Exception CouldNotAttach()
		{
			return new Exception (SR.CouldNotAttach);
		}
		public static Exception CouldNotGetTableForSubtype(object p0, object p1)
		{
			return new Exception (SR.Format (SR.CouldNotGetTableForSubtype, p0, p1));
		}
		public static Exception CouldNotRemoveRelationshipBecauseOneSideCannotBeNull(object p0, object p1, object p2)
		{
			return new Exception (SR.Format (SR.CouldNotRemoveRelationshipBecauseOneSideCannotBeNull, p0, p1, p2));
		}
		public static InvalidOperationException EntitySetAlreadyLoaded()
		{
			return new InvalidOperationException (SR.EntitySetAlreadyLoaded);
		}
		public static Exception EntitySetModifiedDuringEnumeration()
		{
			return new Exception (SR.EntitySetModifiedDuringEnumeration);
		}
		public static Exception ExpectedQueryableArgument(object p0, object p1)
		{
			return new Exception (SR.Format (SR.ExpectedQueryableArgument, p0, p1));
		}
		public static Exception ExpectedUpdateDeleteOrChange()
		{
			return new Exception (SR.ExpectedUpdateDeleteOrChange);
		}
		public static Exception KeyIsWrongSize(object p0, object p1)
		{
			return new Exception (SR.Format (SR.KeyIsWrongSize, p0, p1));
		}
		public static Exception KeyValueIsWrongType(object p0, object p1)
		{
			return new Exception (SR.Format (SR.KeyValueIsWrongType, p0, p1));
		}
		public static Exception IdentityChangeNotAllowed(object p0, object p1)
		{
			return new Exception (SR.Format (SR.IdentityChangeNotAllowed, p0, p1));
		}
		public static Exception DbGeneratedChangeNotAllowed(object p0, object p1)
		{
			return new Exception (SR.Format (SR.DbGeneratedChangeNotAllowed, p0, p1));
		}
		public static Exception ModifyDuringAddOrRemove()
		{
			return new Exception (SR.ModifyDuringAddOrRemove);
		}
		public static Exception ProviderDoesNotImplementRequiredInterface(object p0, object p1)
		{
			return new Exception (SR.Format (SR.ProviderDoesNotImplementRequiredInterface, p0, p1));
		}
		public static Exception ProviderTypeNull()
		{
			return new Exception (SR.ProviderTypeNull);
		}
		public static Exception TypeCouldNotBeAdded(object p0)
		{
			return new Exception (SR.Format (SR.TypeCouldNotBeAdded, p0));
		}
		public static Exception TypeCouldNotBeRemoved(object p0)
		{
			return new Exception (SR.Format (SR.TypeCouldNotBeRemoved, p0));
		}
		public static Exception TypeCouldNotBeTracked(object p0)
		{
			return new Exception (SR.Format (SR.TypeCouldNotBeTracked, p0));
		}
		public static Exception TypeIsNotEntity(object p0)
		{
			return new Exception (SR.Format (SR.TypeIsNotEntity, p0));
		}
		public static Exception UnrecognizedRefreshObject()
		{
			return new Exception (SR.UnrecognizedRefreshObject);
		}
		public static Exception ObjectTrackingRequired()
		{
			return new Exception (SR.ObjectTrackingRequired);
		}
		public static Exception OptionsCannotBeModifiedAfterQuery()
		{
			return new Exception (SR.OptionsCannotBeModifiedAfterQuery);
		}
		public static Exception DeferredLoadingRequiresObjectTracking()
		{
			return new Exception (SR.DeferredLoadingRequiresObjectTracking);
		}
		public static Exception SubqueryDoesNotSupportOperator(object p0)
		{
			return new Exception (SR.Format (SR.SubqueryDoesNotSupportOperator, p0));
		}
		public static Exception SubqueryNotSupportedOn(object p0)
		{
			return new Exception (SR.Format (SR.SubqueryNotSupportedOn, p0));
		}
		public static Exception SubqueryNotSupportedOnType(object p0, object p1)
		{
			return new Exception (SR.Format (SR.SubqueryNotSupportedOnType, p0, p1));
		}
		public static Exception SubqueryNotAllowedAfterFreeze()
		{
			return new Exception (SR.SubqueryNotAllowedAfterFreeze);
		}
		public static Exception IncludeNotAllowedAfterFreeze()
		{
			return new Exception (SR.IncludeNotAllowedAfterFreeze);
		}
		public static Exception LoadOptionsChangeNotAllowedAfterQuery()
		{
			return new Exception (SR.LoadOptionsChangeNotAllowedAfterQuery);
		}
		public static Exception IncludeCycleNotAllowed()
		{
			return new Exception (SR.IncludeCycleNotAllowed);
		}
		public static Exception SubqueryMustBeSequence()
		{
			return new Exception (SR.SubqueryMustBeSequence);
		}
		public static Exception RefreshOfDeletedObject()
		{
			return new Exception (SR.RefreshOfDeletedObject);
		}
		public static Exception RefreshOfNewObject()
		{
			return new Exception (SR.RefreshOfNewObject);
		}
		public static Exception CannotChangeInheritanceType(object p0, object p1, object p2, object p3)
		{
			return new Exception (SR.Format (SR.CannotChangeInheritanceType, p0, p1, p2, p3));
		}
		public static Exception DataContextCannotBeUsedAfterDispose()
		{
			return new Exception (SR.DataContextCannotBeUsedAfterDispose);
		}
		public static InvalidOperationException TypeIsNotMarkedAsTable(object p0)
		{
			return new InvalidOperationException (SR.Format (SR.TypeIsNotMarkedAsTable, p0));
		}
		public static Exception NonEntityAssociationMapping(object p0, object p1, object p2)
		{
			return new Exception (SR.Format (SR.NonEntityAssociationMapping, p0, p1, p2));
		}
		public static Exception CannotPerformCUDOnReadOnlyTable(object p0)
		{
			return new Exception (SR.Format (SR.CannotPerformCUDOnReadOnlyTable, p0));
		}
		public static Exception CycleDetected()
		{
			return new Exception (SR.CycleDetected);
		}
		public static Exception CantAddAlreadyExistingItem()
		{
			return new Exception (SR.CantAddAlreadyExistingItem);
		}
		public static Exception InsertAutoSyncFailure()
		{
			return new Exception (SR.InsertAutoSyncFailure);
		}
		public static Exception EntitySetDataBindingWithAbstractBaseClass(object p0)
		{
			return new Exception (SR.Format (SR.EntitySetDataBindingWithAbstractBaseClass, p0));
		}
		public static Exception EntitySetDataBindingWithNonPublicDefaultConstructor(object p0)
		{
			return new Exception (SR.Format (SR.EntitySetDataBindingWithNonPublicDefaultConstructor, p0));
		}
		public static Exception InvalidLoadOptionsLoadMemberSpecification()
		{
			return new Exception (SR.InvalidLoadOptionsLoadMemberSpecification);
		}
		public static Exception EntityIsTheWrongType()
		{
			return new Exception (SR.EntityIsTheWrongType);
		}
		public static Exception OriginalEntityIsWrongType()
		{
			return new Exception (SR.OriginalEntityIsWrongType);
		}
		public static Exception CannotAttachAlreadyExistingEntity()
		{
			return new Exception (SR.CannotAttachAlreadyExistingEntity);
		}
		public static Exception CannotAttachAsModifiedWithoutOriginalState()
		{
			return new Exception (SR.CannotAttachAsModifiedWithoutOriginalState);
		}
		public static Exception CannotPerformOperationDuringSubmitChanges()
		{
			return new Exception (SR.CannotPerformOperationDuringSubmitChanges);
		}
		public static Exception CannotPerformOperationOutsideSubmitChanges()
		{
			return new Exception (SR.CannotPerformOperationOutsideSubmitChanges);
		}
		public static Exception CannotPerformOperationForUntrackedObject()
		{
			return new Exception (SR.CannotPerformOperationForUntrackedObject);
		}
		public static Exception CannotAttachAddNonNewEntities()
		{
			return new Exception (SR.CannotAttachAddNonNewEntities);
		}
		public static Exception QueryWasCompiledForDifferentMappingSource()
		{
			return new Exception (SR.QueryWasCompiledForDifferentMappingSource);
		}

		public static Exception InvalidFieldInfo(object p0, object p1, object p2)
		{
			return new Exception (SR.Format (SR.InvalidFieldInfo, p0, p1, p2));
		}
		public static Exception CouldNotCreateAccessorToProperty(object p0, object p1, object p2)
		{
			return new Exception (SR.Format (SR.CouldNotCreateAccessorToProperty, p0, p1, p2));
		}
		public static Exception UnableToAssignValueToReadonlyProperty(object p0)
		{
			return new Exception (SR.Format (SR.UnableToAssignValueToReadonlyProperty, p0));
		}
		public static Exception LinkAlreadyLoaded()
		{
			return new Exception (SR.LinkAlreadyLoaded);
		}
		public static Exception EntityRefAlreadyLoaded()
		{
			return new Exception (SR.EntityRefAlreadyLoaded);
		}
		public static Exception NoDiscriminatorFound(object p0)
		{
			return new Exception (SR.Format (SR.NoDiscriminatorFound, p0));
		}
		public static Exception InheritanceTypeDoesNotDeriveFromRoot(object p0, object p1)
		{
			return new Exception (SR.Format (SR.InheritanceTypeDoesNotDeriveFromRoot, p0, p1));
		}
		public static Exception AbstractClassAssignInheritanceDiscriminator(object p0)
		{
			return new Exception (SR.Format (SR.AbstractClassAssignInheritanceDiscriminator, p0));
		}
		public static Exception CannotGetInheritanceDefaultFromNonInheritanceClass()
		{
			return new Exception (SR.CannotGetInheritanceDefaultFromNonInheritanceClass);
		}
		public static Exception InheritanceCodeMayNotBeNull()
		{
			return new Exception (SR.InheritanceCodeMayNotBeNull);
		}
		public static Exception InheritanceTypeHasMultipleDiscriminators(object p0)
		{
			return new Exception (SR.Format (SR.InheritanceTypeHasMultipleDiscriminators, p0));
		}
		public static Exception InheritanceCodeUsedForMultipleTypes(object p0)
		{
			return new Exception (SR.Format (SR.InheritanceCodeUsedForMultipleTypes, p0));
		}
		public static Exception InheritanceTypeHasMultipleDefaults(object p0)
		{
			return new Exception (SR.Format (SR.InheritanceTypeHasMultipleDefaults, p0));
		}
		public static Exception InheritanceHierarchyDoesNotDefineDefault(object p0)
		{
			return new Exception (SR.Format (SR.InheritanceHierarchyDoesNotDefineDefault, p0));
		}
		public static Exception InheritanceSubTypeIsAlsoRoot(object p0)
		{
			return new Exception (SR.Format (SR.InheritanceSubTypeIsAlsoRoot, p0));
		}
		public static Exception NonInheritanceClassHasDiscriminator(object p0)
		{
			return new Exception (SR.Format (SR.NonInheritanceClassHasDiscriminator, p0));
		}
		public static Exception MemberMappedMoreThanOnce(object p0)
		{
			return new Exception (SR.Format (SR.MemberMappedMoreThanOnce, p0));
		}
		public static Exception BadStorageProperty(object p0, object p1, object p2)
		{
			return new Exception (SR.Format (SR.BadStorageProperty, p0, p1, p2));
		}
		public static Exception IncorrectAutoSyncSpecification(object p0)
		{
			return new Exception (SR.Format (SR.IncorrectAutoSyncSpecification, p0));
		}
		public static Exception UnhandledDeferredStorageType(object p0)
		{
			return new Exception (SR.Format (SR.UnhandledDeferredStorageType, p0));
		}
		public static Exception BadKeyMember(object p0, object p1, object p2)
		{
			return new Exception (SR.Format (SR.BadKeyMember, p0, p1, p2));
		}
		public static Exception ProviderTypeNotFound(object p0)
		{
			return new Exception (SR.Format (SR.ProviderTypeNotFound, p0));
		}
		public static Exception MethodCannotBeFound(object p0)
		{
			return new Exception (SR.Format (SR.MethodCannotBeFound, p0));
		}
		public static Exception UnableToResolveRootForType(object p0)
		{
			return new Exception (SR.Format (SR.UnableToResolveRootForType, p0));
		}
		public static Exception MappingForTableUndefined(object p0)
		{
			return new Exception (SR.Format (SR.MappingForTableUndefined, p0));
		}
		public static Exception CouldNotFindTypeFromMapping(object p0)
		{
			return new Exception (SR.Format (SR.CouldNotFindTypeFromMapping, p0));
		}
		public static Exception TwoMembersMarkedAsPrimaryKeyAndDBGenerated(object p0, object p1)
		{
			return new Exception (SR.Format (SR.TwoMembersMarkedAsPrimaryKeyAndDBGenerated, p0, p1));
		}
		public static Exception TwoMembersMarkedAsRowVersion(object p0, object p1)
		{
			return new Exception (SR.Format (SR.TwoMembersMarkedAsRowVersion, p0, p1));
		}
		public static Exception TwoMembersMarkedAsInheritanceDiscriminator(object p0, object p1)
		{
			return new Exception (SR.Format (SR.TwoMembersMarkedAsInheritanceDiscriminator, p0, p1));
		}
		public static Exception CouldNotFindRuntimeTypeForMapping(object p0)
		{
			return new Exception (SR.Format (SR.CouldNotFindRuntimeTypeForMapping, p0));
		}
		public static Exception UnexpectedNull(object p0)
		{
			return new Exception (SR.Format (SR.UnexpectedNull, p0));
		}
		public static Exception CouldNotFindElementTypeInModel(object p0)
		{
			return new Exception (SR.Format (SR.CouldNotFindElementTypeInModel, p0));
		}
		public static Exception BadFunctionTypeInMethodMapping(object p0)
		{
			return new Exception (SR.Format (SR.BadFunctionTypeInMethodMapping, p0));
		}
		public static Exception IncorrectNumberOfParametersMappedForMethod(object p0)
		{
			return new Exception (SR.Format (SR.IncorrectNumberOfParametersMappedForMethod, p0));
		}
		public static Exception CouldNotFindRequiredAttribute(object p0, object p1)
		{
			return new Exception (SR.Format (SR.CouldNotFindRequiredAttribute, p0, p1));
		}
		public static Exception InvalidDeleteOnNullSpecification(object p0)
		{
			return new Exception (SR.Format (SR.InvalidDeleteOnNullSpecification, p0));
		}
		public static Exception MappedMemberHadNoCorrespondingMemberInType(object p0, object p1)
		{
			return new Exception (SR.Format (SR.MappedMemberHadNoCorrespondingMemberInType, p0, p1));
		}
		public static Exception UnrecognizedAttribute(object p0)
		{
			return new Exception (SR.Format (SR.UnrecognizedAttribute, p0));
		}
		public static Exception UnrecognizedElement(object p0)
		{
			return new Exception (SR.Format (SR.UnrecognizedElement, p0));
		}
		public static Exception TooManyResultTypesDeclaredForFunction(object p0)
		{
			return new Exception (SR.Format (SR.TooManyResultTypesDeclaredForFunction, p0));
		}
		public static Exception NoResultTypesDeclaredForFunction(object p0)
		{
			return new Exception (SR.Format (SR.NoResultTypesDeclaredForFunction, p0));
		}
		public static Exception UnexpectedElement(object p0, object p1)
		{
			return new Exception (SR.Format (SR.UnexpectedElement, p0, p1));
		}
		public static Exception ExpectedEmptyElement(object p0, object p1, object p2)
		{
			return new Exception (SR.Format (SR.ExpectedEmptyElement, p0, p1, p2));
		}
		public static Exception DatabaseNodeNotFound(object p0)
		{
			return new Exception (SR.Format (SR.DatabaseNodeNotFound, p0));
		}
		public static Exception DiscriminatorClrTypeNotSupported(object p0, object p1, object p2)
		{
			return new Exception (SR.Format (SR.DiscriminatorClrTypeNotSupported, p0, p1, p2));
		}
		public static Exception IdentityClrTypeNotSupported(object p0, object p1, object p2)
		{
			return new Exception (SR.Format (SR.IdentityClrTypeNotSupported, p0, p1, p2));
		}
		public static Exception PrimaryKeyInSubTypeNotSupported(object p0, object p1)
		{
			return new Exception (SR.Format (SR.PrimaryKeyInSubTypeNotSupported, p0, p1));
		}
		public static Exception MismatchedThisKeyOtherKey(object p0, object p1)
		{
			return new Exception (SR.Format (SR.MismatchedThisKeyOtherKey, p0, p1));
		}
		public static Exception InvalidUseOfGenericMethodAsMappedFunction(object p0)
		{
			return new Exception (SR.Format (SR.InvalidUseOfGenericMethodAsMappedFunction, p0));
		}
		public static Exception MappingOfInterfacesMemberIsNotSupported(object p0, object p1)
		{
			return new Exception (SR.Format (SR.MappingOfInterfacesMemberIsNotSupported, p0, p1));
		}
		public static Exception UnmappedClassMember(object p0, object p1)
		{
			return new Exception (SR.Format (SR.UnmappedClassMember, p0, p1));
		}

		// common
		public static Exception ArgumentNull(string paramName)
		{
			return new ArgumentNullException (paramName);
		}
		public static Exception ArgumentOutOfRange(string paramName)
		{
			return new ArgumentOutOfRangeException (paramName);
		}
		public static Exception NotImplemented()
		{
			return new NotImplementedException ();
		}
		public static Exception NotSupported()
		{
			return new NotSupportedException ();
		}
	}
}

namespace System.Data.Linq.SqlClient
{
	// in the generated class in referencesource this seems to be in two different namespaces, we just forward to the other
	internal partial class Error
	{
		public static Exception ArgumentNull(string paramName)
		{
			return System.Data.Linq.Error.ArgumentNull(paramName);
		}
		public static Exception ArgumentOutOfRange(string paramName)
		{
			return System.Data.Linq.Error.ArgumentOutOfRange(paramName);
		}
		public static Exception ArgumentWrongType(object p0, object p1, object p2)
		{
			return System.Data.Linq.Error.ArgumentWrongType(p0, p1, p2);
		}
		public static Exception ArgumentTypeMismatch(object p0)
		{
			return System.Data.Linq.Error.ArgumentTypeMismatch(p0);
		}
		public static Exception ArgumentWrongValue(object p0)
		{
			return System.Data.Linq.Error.ArgumentWrongValue(p0);
		}
		public static Exception BadParameterType(object p0)
		{
			return System.Data.Linq.Error.BadParameterType(p0);
		}
		public static Exception BadProjectionInSelect()
		{
			return System.Data.Linq.Error.BadProjectionInSelect();
		}
		public static Exception BinaryOperatorNotRecognized(object p0)
		{
			return System.Data.Linq.Error.BinaryOperatorNotRecognized(p0);
		}
		public static Exception CannotAggregateType(object p0)
		{
			return System.Data.Linq.Error.CannotAggregateType(p0);
		}
		public static Exception CannotAssignNull(object p0)
		{
			return System.Data.Linq.Error.CannotAssignNull(p0);
		}
		public static Exception CannotAssignToMember(object p0)
		{
			return System.Data.Linq.Error.CannotAssignToMember(p0);
		}
		public static Exception CannotCompareItemsAssociatedWithDifferentTable()
		{
			return System.Data.Linq.Error.CannotCompareItemsAssociatedWithDifferentTable();
		}
		public static Exception CannotConvertToEntityRef(object p0)
		{
			return System.Data.Linq.Error.CannotConvertToEntityRef(p0);
		}
		public static Exception CannotEnumerateResultsMoreThanOnce()
		{
			return System.Data.Linq.Error.CannotEnumerateResultsMoreThanOnce();
		}
		public static Exception CannotMaterializeEntityType(object p0)
		{
			return System.Data.Linq.Error.CannotMaterializeEntityType(p0);
		}
		public static Exception CannotMaterializeList(object p0)
		{
			return System.Data.Linq.Error.CannotMaterializeList(p0);
		}
		public static Exception CapturedValuesCannotBeSequences()
		{
			return System.Data.Linq.Error.CapturedValuesCannotBeSequences();
		}
		public static Exception ClassLiteralsNotAllowed(object p0)
		{
			return System.Data.Linq.Error.ClassLiteralsNotAllowed(p0);
		}
		public static Exception ColumnCannotReferToItself()
		{
			return System.Data.Linq.Error.ColumnCannotReferToItself();
		}
		public static Exception ColumnClrTypeDoesNotAgreeWithExpressionsClrType()
		{
			return System.Data.Linq.Error.ColumnClrTypeDoesNotAgreeWithExpressionsClrType();
		}
		public static Exception ColumnIsDefinedInMultiplePlaces(object p0)
		{
			return System.Data.Linq.Error.ColumnIsDefinedInMultiplePlaces(p0);
		}
		public static Exception ColumnIsNotAccessibleThroughDistinct(object p0)
		{
			return System.Data.Linq.Error.ColumnIsNotAccessibleThroughDistinct(p0);
		}
		public static Exception ColumnIsNotAccessibleThroughGroupBy(object p0)
		{
			return System.Data.Linq.Error.ColumnIsNotAccessibleThroughGroupBy(p0);
		}
		public static Exception ColumnReferencedIsNotInScope(object p0)
		{
			return System.Data.Linq.Error.ColumnReferencedIsNotInScope(p0);
		}
		public static Exception ComparisonNotSupportedForType(object p0)
		{
			return System.Data.Linq.Error.ComparisonNotSupportedForType(p0);
		}
		public static Exception CompiledQueryAgainstMultipleShapesNotSupported()
		{
			return System.Data.Linq.Error.CompiledQueryAgainstMultipleShapesNotSupported();
		}
		public static Exception ConstructedArraysNotSupported()
		{
			return System.Data.Linq.Error.ConstructedArraysNotSupported();
		}
		public static Exception ContextNotInitialized()
		{
			return System.Data.Linq.Error.ContextNotInitialized();
		}
		public static Exception ConvertToCharFromBoolNotSupported()
		{
			return System.Data.Linq.Error.ConvertToCharFromBoolNotSupported();
		}
		public static Exception ConvertToDateTimeOnlyForDateTimeOrString()
		{
			return System.Data.Linq.Error.ConvertToDateTimeOnlyForDateTimeOrString();
		}
		public static Exception CouldNotConvert(object p0, object p1)
		{
			return System.Data.Linq.Error.CouldNotConvert(p0, p1);
		}
		public static Exception CouldNotConvertToPropertyOrField(object p0)
		{
			return System.Data.Linq.Error.CouldNotConvertToPropertyOrField(p0);
		}
		public static Exception CouldNotDetermineCatalogName()
		{
			return System.Data.Linq.Error.CouldNotDetermineCatalogName();
		}
		public static Exception CouldNotDetermineDbGeneratedSqlType(object p0)
		{
			return System.Data.Linq.Error.CouldNotDetermineDbGeneratedSqlType(p0);
		}
		public static Exception CouldNotDetermineSqlType(object p0)
		{
			return System.Data.Linq.Error.CouldNotDetermineSqlType(p0);
		}
		public static Exception CouldNotGetClrType()
		{
			return System.Data.Linq.Error.CouldNotGetClrType();
		}
		public static Exception CouldNotGetSqlType()
		{
			return System.Data.Linq.Error.CouldNotGetSqlType();
		}
		public static Exception CouldNotHandleAliasRef(object p0)
		{
			return System.Data.Linq.Error.CouldNotHandleAliasRef(p0);
		}
		public static Exception CouldNotTranslateExpressionForReading(object p0)
		{
			return System.Data.Linq.Error.CouldNotTranslateExpressionForReading(p0);
		}
		public static Exception CreateDatabaseFailedBecauseOfClassWithNoMembers(object p0)
		{
			return System.Data.Linq.Error.CreateDatabaseFailedBecauseOfClassWithNoMembers(p0);
		}
		public static Exception CreateDatabaseFailedBecauseOfContextWithNoTables(object p0)
		{
			return System.Data.Linq.Error.CreateDatabaseFailedBecauseOfContextWithNoTables(p0);
		}
		public static Exception CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists(object p0)
		{
			return System.Data.Linq.Error.CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists(p0);
		}
		public static Exception DatabaseDeleteThroughContext()
		{
			return System.Data.Linq.Error.DatabaseDeleteThroughContext();
		}
		public static Exception DeferredMemberWrongType()
		{
			return System.Data.Linq.Error.DeferredMemberWrongType();
		}
		public static Exception DidNotExpectAs(object p0)
		{
			return System.Data.Linq.Error.DidNotExpectAs(p0);
		}
		public static Exception DidNotExpectTypeBinding()
		{
			return System.Data.Linq.Error.DidNotExpectTypeBinding();
		}
		public static Exception DidNotExpectTypeChange(object p0, object p1)
		{
			return System.Data.Linq.Error.DidNotExpectTypeChange(p0, p1);
		}
		public static Exception EmptyCaseNotSupported()
		{
			return System.Data.Linq.Error.EmptyCaseNotSupported();
		}
		public static Exception ExceptNotSupportedForHierarchicalTypes()
		{
			return System.Data.Linq.Error.ExceptNotSupportedForHierarchicalTypes();
		}
		public static Exception ExpectedBitFoundPredicate()
		{
			return System.Data.Linq.Error.ExpectedBitFoundPredicate();
		}
		public static Exception ExpectedClrTypesToAgree(object p0, object p1)
		{
			return System.Data.Linq.Error.ExpectedClrTypesToAgree(p0, p1);
		}
		public static Exception ExpectedPredicateFoundBit()
		{
			return System.Data.Linq.Error.ExpectedPredicateFoundBit();
		}
		public static Exception ExpressionNotDeferredQuerySource()
		{
			return System.Data.Linq.Error.ExpressionNotDeferredQuerySource();
		}
		public static Exception GeneralCollectionMaterializationNotSupported()
		{
			return System.Data.Linq.Error.GeneralCollectionMaterializationNotSupported();
		}
		public static Exception GroupingNotSupportedAsOrderCriterion()
		{
			return System.Data.Linq.Error.GroupingNotSupportedAsOrderCriterion();
		}
		public static Exception IQueryableCannotReturnSelfReferencingConstantExpression()
		{
			return System.Data.Linq.Error.IQueryableCannotReturnSelfReferencingConstantExpression();
		}
		public static Exception IifReturnTypesMustBeEqual(object p0, object p1)
		{
			return System.Data.Linq.Error.IifReturnTypesMustBeEqual(p0, p1);
		}
		public static Exception IndexOfWithStringComparisonArgNotSupported()
		{
			return System.Data.Linq.Error.IndexOfWithStringComparisonArgNotSupported();
		}
		public static Exception InsertItemMustBeConstant()
		{
			return System.Data.Linq.Error.InsertItemMustBeConstant();
		}
		public static Exception IntersectNotSupportedForHierarchicalTypes()
		{
			return System.Data.Linq.Error.IntersectNotSupportedForHierarchicalTypes();
		}
		public static Exception InvalidConnectionArgument(object p0)
		{
			return System.Data.Linq.Error.InvalidConnectionArgument(p0);
		}
		public static Exception InvalidDbGeneratedType(object p0)
		{
			return System.Data.Linq.Error.InvalidDbGeneratedType(p0);
		}
		public static Exception InvalidFormatNode(object p0)
		{
			return System.Data.Linq.Error.InvalidFormatNode(p0);
		}
		public static Exception InvalidGroupByExpression()
		{
			return System.Data.Linq.Error.InvalidGroupByExpression();
		}
		public static Exception InvalidGroupByExpressionType(object p0)
		{
			return System.Data.Linq.Error.InvalidGroupByExpressionType(p0);
		}
		public static Exception InvalidMethodExecution(object p0)
		{
			return System.Data.Linq.Error.InvalidMethodExecution(p0);
		}
		public static Exception InvalidOrderByExpression(object p0)
		{
			return System.Data.Linq.Error.InvalidOrderByExpression(p0);
		}
		public static Exception InvalidProviderType(object p0)
		{
			return System.Data.Linq.Error.InvalidProviderType(p0);
		}
		public static Exception InvalidReferenceToRemovedAliasDuringDeflation()
		{
			return System.Data.Linq.Error.InvalidReferenceToRemovedAliasDuringDeflation();
		}
		public static Exception InvalidReturnFromSproc(object p0)
		{
			return System.Data.Linq.Error.InvalidReturnFromSproc(p0);
		}
		public static Exception InvalidSequenceOperatorCall(object p0)
		{
			return System.Data.Linq.Error.InvalidSequenceOperatorCall(p0);
		}
		public static Exception LastIndexOfWithStringComparisonArgNotSupported()
		{
			return System.Data.Linq.Error.LastIndexOfWithStringComparisonArgNotSupported();
		}
		public static Exception MappedTypeMustHaveDefaultConstructor(object p0)
		{
			return System.Data.Linq.Error.MappedTypeMustHaveDefaultConstructor(p0);
		}
		public static Exception MathRoundNotSupported()
		{
			return System.Data.Linq.Error.MathRoundNotSupported();
		}
		public static Exception MemberAccessIllegal(object p0, object p1, object p2)
		{
			return System.Data.Linq.Error.MemberAccessIllegal(p0, p1, p2);
		}
		public static Exception MemberCannotBeTranslated(object p0, object p1)
		{
			return System.Data.Linq.Error.MemberCannotBeTranslated(p0, p1);
		}
		public static Exception MemberNotPartOfProjection(object p0, object p1)
		{
			return System.Data.Linq.Error.MemberNotPartOfProjection(p0, p1);
		}
		public static Exception MethodFormHasNoSupportConversionToSql(object p0, object p1)
		{
			return System.Data.Linq.Error.MethodFormHasNoSupportConversionToSql(p0, p1);
		}
		public static Exception MethodHasNoSupportConversionToSql(object p0)
		{
			return System.Data.Linq.Error.MethodHasNoSupportConversionToSql(p0);
		}
		public static Exception NoMethodInTypeMatchingArguments(object p0)
		{
			return System.Data.Linq.Error.NoMethodInTypeMatchingArguments(p0);
		}
		public static Exception NonConstantExpressionsNotSupportedFor(object p0)
		{
			return System.Data.Linq.Error.NonConstantExpressionsNotSupportedFor(p0);
		}
		public static Exception NonConstantExpressionsNotSupportedForRounding()
		{
			return System.Data.Linq.Error.NonConstantExpressionsNotSupportedForRounding();
		}
		public static Exception NonCountAggregateFunctionsAreNotValidOnProjections(object p0)
		{
			return System.Data.Linq.Error.NonCountAggregateFunctionsAreNotValidOnProjections(p0);
		}
		public static Exception NotSupported()
		{
			return System.Data.Linq.Error.NotSupported();
		}
		public static Exception ParameterNotInScope(object p0)
		{
			return System.Data.Linq.Error.ParameterNotInScope(p0);
		}
		public static Exception ParametersCannotBeSequences()
		{
			return System.Data.Linq.Error.ParametersCannotBeSequences();
		}
		public static Exception ProviderCannotBeUsedAfterDispose()
		{
			return System.Data.Linq.Error.ProviderCannotBeUsedAfterDispose();
		}
		public static Exception ProviderNotInstalled(object p0, object p1)
		{
			return System.Data.Linq.Error.ProviderNotInstalled(p0, p1);
		}
		public static Exception QueryOnLocalCollectionNotSupported()
		{
			return System.Data.Linq.Error.QueryOnLocalCollectionNotSupported();
		}
		public static Exception QueryOperatorNotSupported(object p0)
		{
			return System.Data.Linq.Error.QueryOperatorNotSupported(p0);
		}
		public static Exception QueryOperatorOverloadNotSupported(object p0)
		{
			return System.Data.Linq.Error.QueryOperatorOverloadNotSupported(p0);
		}
		public static Exception RequiredColumnDoesNotExist(object p0)
		{
			return System.Data.Linq.Error.RequiredColumnDoesNotExist(p0);
		}
		public static Exception SequenceOperatorsNotSupportedForType(object p0)
		{
			return System.Data.Linq.Error.SequenceOperatorsNotSupportedForType(p0);
		}
		public static Exception SkipNotSupportedForSequenceTypes()
		{
			return System.Data.Linq.Error.SkipNotSupportedForSequenceTypes();
		}
		public static Exception SkipRequiresSingleTableQueryWithPKs()
		{
			return System.Data.Linq.Error.SkipRequiresSingleTableQueryWithPKs();
		}
		public static Exception SprocsCannotBeComposed()
		{
			return System.Data.Linq.Error.SprocsCannotBeComposed();
		}
		public static Exception SqlMethodOnlyForSql(object p0)
		{
			return System.Data.Linq.Error.SqlMethodOnlyForSql(p0);
		}
		public static Exception ToStringOnlySupportedForPrimitiveTypes()
		{
			return System.Data.Linq.Error.ToStringOnlySupportedForPrimitiveTypes();
		}
		public static Exception TransactionDoesNotMatchConnection()
		{
			return System.Data.Linq.Error.TransactionDoesNotMatchConnection();
		}
		public static Exception TypeBinaryOperatorNotRecognized()
		{
			return System.Data.Linq.Error.TypeBinaryOperatorNotRecognized();
		}
		public static Exception TypeCannotBeOrdered(object p0)
		{
			return System.Data.Linq.Error.TypeCannotBeOrdered(p0);
		}
		public static Exception UnexpectedFloatingColumn()
		{
			return System.Data.Linq.Error.UnexpectedFloatingColumn();
		}
		public static Exception UnexpectedNode(object p0)
		{
			return System.Data.Linq.Error.UnexpectedNode(p0);
		}
		public static Exception UnexpectedSharedExpression()
		{
			return System.Data.Linq.Error.UnexpectedSharedExpression();
		}
		public static Exception UnexpectedSharedExpressionReference()
		{
			return System.Data.Linq.Error.UnexpectedSharedExpressionReference();
		}
		public static Exception UnexpectedTypeCode(object p0)
		{
			return System.Data.Linq.Error.UnexpectedTypeCode(p0);
		}
		public static Exception UnhandledBindingType(object p0)
		{
			return System.Data.Linq.Error.UnhandledBindingType(p0);
		}
		public static Exception UnhandledExpressionType(object p0)
		{
			return System.Data.Linq.Error.UnhandledExpressionType(p0);
		}
		public static Exception UnhandledStringTypeComparison()
		{
			return System.Data.Linq.Error.UnhandledStringTypeComparison();
		}
		public static Exception UnionDifferentMemberOrder()
		{
			return System.Data.Linq.Error.UnionDifferentMemberOrder();
		}
		public static Exception UnionDifferentMembers()
		{
			return System.Data.Linq.Error.UnionDifferentMembers();
		}
		public static Exception UnionIncompatibleConstruction()
		{
			return System.Data.Linq.Error.UnionIncompatibleConstruction();
		}
		public static Exception UnionWithHierarchy()
		{
			return System.Data.Linq.Error.UnionWithHierarchy();
		}
		public static Exception UnmappedDataMember(object p0, object p1, object p2)
		{
			return System.Data.Linq.Error.UnmappedDataMember(p0, p1, p2);
		}
		public static Exception UnrecognizedExpressionNode(object p0)
		{
			return System.Data.Linq.Error.UnrecognizedExpressionNode(p0);
		}
		public static Exception UnsafeStringConversion(object p0, object p1)
		{
			return System.Data.Linq.Error.UnsafeStringConversion(p0, p1);
		}
		public static Exception UnsupportedDateTimeConstructorForm()
		{
			return System.Data.Linq.Error.UnsupportedDateTimeConstructorForm();
		}
		public static Exception UnsupportedDateTimeOffsetConstructorForm()
		{
			return System.Data.Linq.Error.UnsupportedDateTimeOffsetConstructorForm();
		}
		public static Exception UnsupportedNodeType(object p0)
		{
			return System.Data.Linq.Error.UnsupportedNodeType(p0);
		}
		public static Exception UnsupportedStringConstructorForm()
		{
			return System.Data.Linq.Error.UnsupportedStringConstructorForm();
		}
		public static Exception UnsupportedTimeSpanConstructorForm()
		{
			return System.Data.Linq.Error.UnsupportedTimeSpanConstructorForm();
		}
		public static Exception UpdateItemMustBeConstant()
		{
			return System.Data.Linq.Error.UpdateItemMustBeConstant();
		}
		public static Exception ValueHasNoLiteralInSql(object p0)
		{
			return System.Data.Linq.Error.ValueHasNoLiteralInSql(p0);
		}
		public static Exception VbLikeDoesNotSupportMultipleCharacterRanges()
		{
			return System.Data.Linq.Error.VbLikeDoesNotSupportMultipleCharacterRanges();
		}
		public static Exception VbLikeUnclosedBracket()
		{
			return System.Data.Linq.Error.VbLikeUnclosedBracket();
		}
		public static Exception WrongDataContext()
		{
			return System.Data.Linq.Error.WrongDataContext();
		}
	}
}