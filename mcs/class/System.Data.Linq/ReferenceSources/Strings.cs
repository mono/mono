namespace System.Data.Linq
{
  internal static class Strings
  {
    internal static string OwningTeam {
		get { return SR.OwningTeam; }      
    }

	internal static string CannotAddChangeConflicts {
		get { return SR.CannotAddChangeConflicts; }
	}

    internal static string CannotRemoveChangeConflicts {
		get { return SR.CannotRemoveChangeConflicts; }
	}
    
    internal static string InconsistentAssociationAndKeyChange (object p0, object p1) {
		return SR.Format (SR.InconsistentAssociationAndKeyChange, p0, p1);      
    }

    internal static string UnableToDetermineDataContext {
		get { return SR.UnableToDetermineDataContext; }
	}    

    internal static string ArgumentTypeHasNoIdentityKey (object p0) {
		return SR.Format (SR.ArgumentTypeHasNoIdentityKey, p0);
	}
    
    internal static string CouldNotConvert (object p0, object p1) {
		return SR.Format (SR.CouldNotConvert, p0, p1);
	}
    
    internal static string CannotRemoveUnattachedEntity {
		get { return SR.CannotRemoveUnattachedEntity; }      
    }

    internal static string ColumnMappedMoreThanOnce (object p0) {
		return SR.Format (SR.ColumnMappedMoreThanOnce, p0);	  
    }

    internal static string CouldNotAttach {
		get { return SR.CouldNotAttach; }
	}
    
    internal static string CouldNotGetTableForSubtype (object p0, object p1) {
		return SR.Format (SR.CouldNotGetTableForSubtype, p0, p1);
	}
    
    internal static string CouldNotRemoveRelationshipBecauseOneSideCannotBeNull (object p0, object p1, object p2) {
		return SR.Format (SR.CouldNotRemoveRelationshipBecauseOneSideCannotBeNull, p0, p1, p2);
	}
    
    internal static string EntitySetAlreadyLoaded {
		get { return SR.EntitySetAlreadyLoaded; }
	}
    
    internal static string EntitySetModifiedDuringEnumeration {
		get { return SR.EntitySetModifiedDuringEnumeration; }
	}
    
    internal static string ExpectedQueryableArgument (object p0, object p1) {
		return SR.Format (SR.ExpectedQueryableArgument, p0, p1);
	}
    
    internal static string ExpectedUpdateDeleteOrChange {
		get { return SR.ExpectedUpdateDeleteOrChange; }
	}
    
    internal static string KeyIsWrongSize (object p0, object p1) {
		return SR.Format (SR.KeyIsWrongSize, p0, p1);
	}
    
    internal static string KeyValueIsWrongType (object p0, object p1) {
		return SR.Format (SR.KeyValueIsWrongType, p0, p1);
	}
    
    internal static string IdentityChangeNotAllowed (object p0, object p1) {
		return SR.Format (SR.IdentityChangeNotAllowed, p0, p1);
	}
    
    internal static string DbGeneratedChangeNotAllowed (object p0, object p1) {
		return SR.Format (SR.DbGeneratedChangeNotAllowed, p0, p1);
	}
    
    internal static string ModifyDuringAddOrRemove {
		get { return SR.ModifyDuringAddOrRemove; }
	}
    
    internal static string ProviderDoesNotImplementRequiredInterface (object p0, object p1) {
		return SR.Format (SR.ProviderDoesNotImplementRequiredInterface, p0, p1);
	}
    
    internal static string ProviderTypeNull {
		get { return SR.ProviderTypeNull; }
	}
    
    internal static string TypeCouldNotBeAdded (object p0) {
		return SR.Format (SR.TypeCouldNotBeAdded, p0);
	}
    
    internal static string TypeCouldNotBeRemoved (object p0) {
		return SR.Format (SR.TypeCouldNotBeRemoved, p0);
	}
    
    internal static string TypeCouldNotBeTracked (object p0) {
		return SR.Format (SR.TypeCouldNotBeTracked, p0);
	}
    
    internal static string TypeIsNotEntity (object p0) {
		return SR.Format (SR.TypeIsNotEntity, p0);
	}
    
    internal static string UnrecognizedRefreshObject {
		get { return SR.UnrecognizedRefreshObject; }
	}
    
    internal static string UnhandledExpressionType (object p0) {
		return SR.Format (SR.UnhandledExpressionType, p0);
	}
    
    internal static string UnhandledBindingType (object p0) {
		return SR.Format (SR.UnhandledBindingType, p0);
	}
    
    internal static string ObjectTrackingRequired {
		get { return SR.ObjectTrackingRequired; }
	}
    
    internal static string OptionsCannotBeModifiedAfterQuery {
		get { return SR.OptionsCannotBeModifiedAfterQuery; }
	}
    
    internal static string DeferredLoadingRequiresObjectTracking {
		get { return SR.DeferredLoadingRequiresObjectTracking; }
	}
    
    internal static string SubqueryDoesNotSupportOperator (object p0) {
		return SR.Format (SR.SubqueryDoesNotSupportOperator, p0);
	}
    
    internal static string SubqueryNotSupportedOn (object p0) {
		return SR.Format (SR.SubqueryNotSupportedOn, p0);
	}
    
    internal static string SubqueryNotSupportedOnType (object p0, object p1) {
		return SR.Format (SR.SubqueryNotSupportedOnType, p0, p1);
	}
    
	internal static string SubqueryNotAllowedAfterFreeze {
		get { return SR.SubqueryNotAllowedAfterFreeze; }
	}
    
    internal static string IncludeNotAllowedAfterFreeze {
		get{ return SR.IncludeNotAllowedAfterFreeze; }
	}
    
    internal static string LoadOptionsChangeNotAllowedAfterQuery {
		get { return SR.LoadOptionsChangeNotAllowedAfterQuery; }
	}
    
    internal static string IncludeCycleNotAllowed {
		get { return SR.IncludeCycleNotAllowed; }
	}
    
    internal static string SubqueryMustBeSequence {
		get { return SR.SubqueryMustBeSequence; }
	}
    
    internal static string RefreshOfDeletedObject {
		get { return SR.RefreshOfDeletedObject; }
	}
    
    internal static string RefreshOfNewObject {
		get { return SR.RefreshOfNewObject; }
	}
    
    internal static string CannotChangeInheritanceType (object p0, object p1, object p2, object p3) {
		return SR.Format (SR.CannotChangeInheritanceType, p0, p1, p2, p3);
	}
    
    internal static string DataContextCannotBeUsedAfterDispose {
		get { return SR.DataContextCannotBeUsedAfterDispose; }
	}
    
    internal static string TypeIsNotMarkedAsTable (object p0) {
		return SR.Format (SR.TypeIsNotMarkedAsTable, p0);
	}
    
    internal static string NonEntityAssociationMapping (object p0, object p1, object p2) {
		return SR.Format (SR.NonEntityAssociationMapping, p0, p1, p2);
	}
    
    internal static string CannotPerformCUDOnReadOnlyTable (object p0) {
		return SR.Format (SR.CannotPerformCUDOnReadOnlyTable, p0);
	}
    
    internal static string InsertCallbackComment {
		get { return SR.InsertCallbackComment; }
	}
    
    internal static string UpdateCallbackComment {
		get { return SR.UpdateCallbackComment; }
	}
    
    internal static string DeleteCallbackComment {
		get { return SR.DeleteCallbackComment; }
	}
    
    internal static string RowNotFoundOrChanged {
		get { return SR.RowNotFoundOrChanged; }
	}
    
    internal static string UpdatesFailedMessage (object p0, object p1) {
		return SR.Format (SR.UpdatesFailedMessage, p0, p1);
	}
    
    internal static string CycleDetected {
		get { return SR.CycleDetected; }
	}
    
    internal static string CantAddAlreadyExistingItem {
		get { return SR.CantAddAlreadyExistingItem; }
	}
    
    internal static string CantAddAlreadyExistingKey {
		get { return SR.CantAddAlreadyExistingKey; }
	}
    
    internal static string DatabaseGeneratedAlreadyExistingKey {
		get { return SR.DatabaseGeneratedAlreadyExistingKey; }
	}
    
    internal static string InsertAutoSyncFailure {
		get { return SR.InsertAutoSyncFailure; }
	}
    
    internal static string EntitySetDataBindingWithAbstractBaseClass (object p0) {
		return SR.Format (SR.EntitySetDataBindingWithAbstractBaseClass, p0);
	}
    
    internal static string EntitySetDataBindingWithNonPublicDefaultConstructor (object p0) {
		return SR.Format (SR.EntitySetDataBindingWithNonPublicDefaultConstructor, p0);
	}

	internal static string TextNTextAndImageCannotOccurInDistinct (object p0) {
		return SR.Format (SR.TextNTextAndImageCannotOccurInDistinct, p0);
	}

	internal static string TextNTextAndImageCannotOccurInUnion (object p0) {
		return SR.Format (SR.TextNTextAndImageCannotOccurInUnion, p0);
	}

	internal static string LenOfTextOrNTextNotSupported (object p0) {
		return SR.Format (SR.LenOfTextOrNTextNotSupported, p0);
	}

	internal static string SourceExpressionAnnotation (object p0) {
		return SR.Format (SR.SourceExpressionAnnotation, p0);
	}

	internal static string LogGeneralInfoMessage (object p0, object p1) {
		return SR.Format (SR.LogGeneralInfoMessage, p0, p1);
	}

	internal static string LogAttemptingToDeleteDatabase (object p0) {
		return SR.Format (SR.LogAttemptingToDeleteDatabase, p0);
	}

	internal static string MaxSizeNotSupported (object p0) {
		return SR.Format (SR.MaxSizeNotSupported, p0);	
	}
    
    internal static string InvalidLoadOptionsLoadMemberSpecification {
		get { return SR.InvalidLoadOptionsLoadMemberSpecification; }
	}
    
    internal static string EntityIsTheWrongType {
		get { return SR.EntityIsTheWrongType; }
	}
    
    internal static string OriginalEntityIsWrongType {
		get { return SR.OriginalEntityIsWrongType; }
	}
    
    internal static string CannotAttachAlreadyExistingEntity {
		get { return SR.CannotAttachAlreadyExistingEntity; }
	}
    
    internal static string CannotAttachAsModifiedWithoutOriginalState {
		get { return SR.CannotAttachAsModifiedWithoutOriginalState; }
	}
    
    internal static string CannotPerformOperationDuringSubmitChanges {
		get { return SR.CannotPerformOperationDuringSubmitChanges; }
	}
    
    internal static string CannotPerformOperationOutsideSubmitChanges {
		get { return SR.CannotPerformOperationOutsideSubmitChanges; }
	}
    
    internal static string CannotPerformOperationForUntrackedObject { 
		get { return SR.CannotPerformOperationForUntrackedObject; }
	}

	internal static string CannotTranslateExpressionToSql {
		get { return SR.CannotTranslateExpressionToSql; }
	}
    
    internal static string CannotAttachAddNonNewEntities {
		get { return SR.CannotAttachAddNonNewEntities; }
	}
    
    internal static string QueryWasCompiledForDifferentMappingSource {
		get { return SR.QueryWasCompiledForDifferentMappingSource; }
	}
  }
}
