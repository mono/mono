//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.


namespace System.Data.Services.Client {
    using System;
    using System.Reflection;
    using System.Globalization;
    using System.Resources;
    using System.Text;
    using System.Threading;
    using System.ComponentModel;
    using System.Security.Permissions;

    [AttributeUsage(AttributeTargets.All)]
    internal sealed class TextResDescriptionAttribute : DescriptionAttribute {

        private bool replaced;

        public TextResDescriptionAttribute(string description) : base(description) {
        }

        public override string Description {
            get {
                if (!replaced) {
                    replaced = true;
                    DescriptionValue = TextRes.GetString(base.Description);
                }
                return base.Description;
            }
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    internal sealed class TextResCategoryAttribute : CategoryAttribute {

        public TextResCategoryAttribute(string category) : base(category) {
        }

        protected override string GetLocalizedString(string value) {
            return TextRes.GetString(value);
        }
    }

    
    internal sealed class TextRes {
        internal const string BatchStream_MissingBoundary = "BatchStream_MissingBoundary";
        internal const string BatchStream_ContentExpected = "BatchStream_ContentExpected";
        internal const string BatchStream_ContentUnexpected = "BatchStream_ContentUnexpected";
        internal const string BatchStream_GetMethodNotSupportedInChangeset = "BatchStream_GetMethodNotSupportedInChangeset";
        internal const string BatchStream_InvalidBatchFormat = "BatchStream_InvalidBatchFormat";
        internal const string BatchStream_InvalidDelimiter = "BatchStream_InvalidDelimiter";
        internal const string BatchStream_MissingEndChangesetDelimiter = "BatchStream_MissingEndChangesetDelimiter";
        internal const string BatchStream_InvalidHeaderValueSpecified = "BatchStream_InvalidHeaderValueSpecified";
        internal const string BatchStream_InvalidContentLengthSpecified = "BatchStream_InvalidContentLengthSpecified";
        internal const string BatchStream_OnlyGETOperationsCanBeSpecifiedInBatch = "BatchStream_OnlyGETOperationsCanBeSpecifiedInBatch";
        internal const string BatchStream_InvalidOperationHeaderSpecified = "BatchStream_InvalidOperationHeaderSpecified";
        internal const string BatchStream_InvalidHttpMethodName = "BatchStream_InvalidHttpMethodName";
        internal const string BatchStream_MoreDataAfterEndOfBatch = "BatchStream_MoreDataAfterEndOfBatch";
        internal const string BatchStream_InternalBufferRequestTooSmall = "BatchStream_InternalBufferRequestTooSmall";
        internal const string BatchStream_InvalidMethodHeaderSpecified = "BatchStream_InvalidMethodHeaderSpecified";
        internal const string BatchStream_InvalidHttpVersionSpecified = "BatchStream_InvalidHttpVersionSpecified";
        internal const string BatchStream_InvalidNumberOfHeadersAtOperationStart = "BatchStream_InvalidNumberOfHeadersAtOperationStart";
        internal const string BatchStream_MissingOrInvalidContentEncodingHeader = "BatchStream_MissingOrInvalidContentEncodingHeader";
        internal const string BatchStream_InvalidNumberOfHeadersAtChangeSetStart = "BatchStream_InvalidNumberOfHeadersAtChangeSetStart";
        internal const string BatchStream_MissingContentTypeHeader = "BatchStream_MissingContentTypeHeader";
        internal const string BatchStream_InvalidContentTypeSpecified = "BatchStream_InvalidContentTypeSpecified";
        internal const string Batch_ExpectedContentType = "Batch_ExpectedContentType";
        internal const string Batch_ExpectedResponse = "Batch_ExpectedResponse";
        internal const string Batch_IncompleteResponseCount = "Batch_IncompleteResponseCount";
        internal const string Batch_UnexpectedContent = "Batch_UnexpectedContent";
        internal const string Context_BaseUri = "Context_BaseUri";
        internal const string Context_CannotConvertKey = "Context_CannotConvertKey";
        internal const string Context_TrackingExpectsAbsoluteUri = "Context_TrackingExpectsAbsoluteUri";
        internal const string Context_LinkResourceInsertFailure = "Context_LinkResourceInsertFailure";
        internal const string Context_InternalError = "Context_InternalError";
        internal const string Context_BatchExecuteError = "Context_BatchExecuteError";
        internal const string Context_EntitySetName = "Context_EntitySetName";
        internal const string Context_MissingEditLinkInResponseBody = "Context_MissingEditLinkInResponseBody";
        internal const string Context_MissingSelfLinkInResponseBody = "Context_MissingSelfLinkInResponseBody";
        internal const string Context_MissingEditMediaLinkInResponseBody = "Context_MissingEditMediaLinkInResponseBody";
        internal const string Content_EntityWithoutKey = "Content_EntityWithoutKey";
        internal const string Content_EntityIsNotEntityType = "Content_EntityIsNotEntityType";
        internal const string Context_EntityNotContained = "Context_EntityNotContained";
        internal const string Context_EntityAlreadyContained = "Context_EntityAlreadyContained";
        internal const string Context_DifferentEntityAlreadyContained = "Context_DifferentEntityAlreadyContained";
        internal const string Context_DidNotOriginateAsync = "Context_DidNotOriginateAsync";
        internal const string Context_AsyncAlreadyDone = "Context_AsyncAlreadyDone";
        internal const string Context_OperationCanceled = "Context_OperationCanceled";
        internal const string Context_NoLoadWithInsertEnd = "Context_NoLoadWithInsertEnd";
        internal const string Context_NoRelationWithInsertEnd = "Context_NoRelationWithInsertEnd";
        internal const string Context_NoRelationWithDeleteEnd = "Context_NoRelationWithDeleteEnd";
        internal const string Context_RelationAlreadyContained = "Context_RelationAlreadyContained";
        internal const string Context_RelationNotRefOrCollection = "Context_RelationNotRefOrCollection";
        internal const string Context_AddLinkCollectionOnly = "Context_AddLinkCollectionOnly";
        internal const string Context_AddRelatedObjectCollectionOnly = "Context_AddRelatedObjectCollectionOnly";
        internal const string Context_AddRelatedObjectSourceDeleted = "Context_AddRelatedObjectSourceDeleted";
        internal const string Context_SetLinkReferenceOnly = "Context_SetLinkReferenceOnly";
        internal const string Context_NoContentTypeForMediaLink = "Context_NoContentTypeForMediaLink";
        internal const string Context_BatchNotSupportedForMediaLink = "Context_BatchNotSupportedForMediaLink";
        internal const string Context_UnexpectedZeroRawRead = "Context_UnexpectedZeroRawRead";
        internal const string Context_VersionNotSupported = "Context_VersionNotSupported";
        internal const string Context_ChildResourceExists = "Context_ChildResourceExists";
        internal const string Context_EntityNotMediaLinkEntry = "Context_EntityNotMediaLinkEntry";
        internal const string Context_MLEWithoutSaveStream = "Context_MLEWithoutSaveStream";
        internal const string Context_SetSaveStreamOnMediaEntryProperty = "Context_SetSaveStreamOnMediaEntryProperty";
        internal const string Context_SetSaveStreamWithoutEditMediaLink = "Context_SetSaveStreamWithoutEditMediaLink";
        internal const string Collection_NullCollectionReference = "Collection_NullCollectionReference";
        internal const string ClientType_MissingOpenProperty = "ClientType_MissingOpenProperty";
        internal const string Clienttype_MultipleOpenProperty = "Clienttype_MultipleOpenProperty";
        internal const string ClientType_MissingProperty = "ClientType_MissingProperty";
        internal const string ClientType_KeysMustBeSimpleTypes = "ClientType_KeysMustBeSimpleTypes";
        internal const string ClientType_KeysOnDifferentDeclaredType = "ClientType_KeysOnDifferentDeclaredType";
        internal const string ClientType_MissingMimeTypeProperty = "ClientType_MissingMimeTypeProperty";
        internal const string ClientType_MissingMediaEntryProperty = "ClientType_MissingMediaEntryProperty";
        internal const string ClientType_NoSettableFields = "ClientType_NoSettableFields";
        internal const string ClientType_MultipleImplementationNotSupported = "ClientType_MultipleImplementationNotSupported";
        internal const string ClientType_NullOpenProperties = "ClientType_NullOpenProperties";
        internal const string ClientType_CollectionOfNonEntities = "ClientType_CollectionOfNonEntities";
        internal const string ClientType_Ambiguous = "ClientType_Ambiguous";
        internal const string DataServiceException_GeneralError = "DataServiceException_GeneralError";
        internal const string Deserialize_GetEnumerator = "Deserialize_GetEnumerator";
        internal const string Deserialize_Current = "Deserialize_Current";
        internal const string Deserialize_MixedTextWithComment = "Deserialize_MixedTextWithComment";
        internal const string Deserialize_ExpectingSimpleValue = "Deserialize_ExpectingSimpleValue";
        internal const string Deserialize_NotApplicationXml = "Deserialize_NotApplicationXml";
        internal const string Deserialize_MismatchAtomLinkLocalSimple = "Deserialize_MismatchAtomLinkLocalSimple";
        internal const string Deserialize_MismatchAtomLinkFeedPropertyNotCollection = "Deserialize_MismatchAtomLinkFeedPropertyNotCollection";
        internal const string Deserialize_MismatchAtomLinkEntryPropertyIsCollection = "Deserialize_MismatchAtomLinkEntryPropertyIsCollection";
        internal const string Deserialize_UnknownMimeTypeSpecified = "Deserialize_UnknownMimeTypeSpecified";
        internal const string Deserialize_ExpectedEmptyMediaLinkEntryContent = "Deserialize_ExpectedEmptyMediaLinkEntryContent";
        internal const string Deserialize_ContentPlusPropertiesNotAllowed = "Deserialize_ContentPlusPropertiesNotAllowed";
        internal const string Deserialize_NoLocationHeader = "Deserialize_NoLocationHeader";
        internal const string Deserialize_ServerException = "Deserialize_ServerException";
        internal const string Deserialize_MissingIdElement = "Deserialize_MissingIdElement";
        internal const string EpmClientType_PropertyIsComplex = "EpmClientType_PropertyIsComplex";
        internal const string EpmClientType_PropertyIsPrimitive = "EpmClientType_PropertyIsPrimitive";
        internal const string EpmSourceTree_InvalidSourcePath = "EpmSourceTree_InvalidSourcePath";
        internal const string EpmSourceTree_DuplicateEpmAttrsWithSameSourceName = "EpmSourceTree_DuplicateEpmAttrsWithSameSourceName";
        internal const string EpmSourceTree_InaccessiblePropertyOnType = "EpmSourceTree_InaccessiblePropertyOnType";
        internal const string EpmTargetTree_InvalidTargetPath = "EpmTargetTree_InvalidTargetPath";
        internal const string EpmTargetTree_AttributeInMiddle = "EpmTargetTree_AttributeInMiddle";
        internal const string EpmTargetTree_DuplicateEpmAttrsWithSameTargetName = "EpmTargetTree_DuplicateEpmAttrsWithSameTargetName";
        internal const string EntityPropertyMapping_EpmAttribute = "EntityPropertyMapping_EpmAttribute";
        internal const string EntityPropertyMapping_TargetNamespaceUriNotValid = "EntityPropertyMapping_TargetNamespaceUriNotValid";
        internal const string HttpProcessUtility_ContentTypeMissing = "HttpProcessUtility_ContentTypeMissing";
        internal const string HttpProcessUtility_MediaTypeMissingValue = "HttpProcessUtility_MediaTypeMissingValue";
        internal const string HttpProcessUtility_MediaTypeRequiresSemicolonBeforeParameter = "HttpProcessUtility_MediaTypeRequiresSemicolonBeforeParameter";
        internal const string HttpProcessUtility_MediaTypeRequiresSlash = "HttpProcessUtility_MediaTypeRequiresSlash";
        internal const string HttpProcessUtility_MediaTypeRequiresSubType = "HttpProcessUtility_MediaTypeRequiresSubType";
        internal const string HttpProcessUtility_MediaTypeUnspecified = "HttpProcessUtility_MediaTypeUnspecified";
        internal const string HttpProcessUtility_EncodingNotSupported = "HttpProcessUtility_EncodingNotSupported";
        internal const string HttpProcessUtility_EscapeCharWithoutQuotes = "HttpProcessUtility_EscapeCharWithoutQuotes";
        internal const string HttpProcessUtility_EscapeCharAtEnd = "HttpProcessUtility_EscapeCharAtEnd";
        internal const string HttpProcessUtility_ClosingQuoteNotFound = "HttpProcessUtility_ClosingQuoteNotFound";
        internal const string MaterializeFromAtom_CountNotPresent = "MaterializeFromAtom_CountNotPresent";
        internal const string MaterializeFromAtom_CountFormatError = "MaterializeFromAtom_CountFormatError";
        internal const string MaterializeFromAtom_TopLevelLinkNotAvailable = "MaterializeFromAtom_TopLevelLinkNotAvailable";
        internal const string MaterializeFromAtom_CollectionKeyNotPresentInLinkTable = "MaterializeFromAtom_CollectionKeyNotPresentInLinkTable";
        internal const string MaterializeFromAtom_GetNestLinkForFlatCollection = "MaterializeFromAtom_GetNestLinkForFlatCollection";
        internal const string Serializer_NullKeysAreNotSupported = "Serializer_NullKeysAreNotSupported";
        internal const string Util_EmptyString = "Util_EmptyString";
        internal const string Util_EmptyArray = "Util_EmptyArray";
        internal const string Util_NullArrayElement = "Util_NullArrayElement";
        internal const string ALinq_UnsupportedExpression = "ALinq_UnsupportedExpression";
        internal const string ALinq_CouldNotConvert = "ALinq_CouldNotConvert";
        internal const string ALinq_MethodNotSupported = "ALinq_MethodNotSupported";
        internal const string ALinq_UnaryNotSupported = "ALinq_UnaryNotSupported";
        internal const string ALinq_BinaryNotSupported = "ALinq_BinaryNotSupported";
        internal const string ALinq_ConstantNotSupported = "ALinq_ConstantNotSupported";
        internal const string ALinq_TypeBinaryNotSupported = "ALinq_TypeBinaryNotSupported";
        internal const string ALinq_ConditionalNotSupported = "ALinq_ConditionalNotSupported";
        internal const string ALinq_ParameterNotSupported = "ALinq_ParameterNotSupported";
        internal const string ALinq_MemberAccessNotSupported = "ALinq_MemberAccessNotSupported";
        internal const string ALinq_LambdaNotSupported = "ALinq_LambdaNotSupported";
        internal const string ALinq_NewNotSupported = "ALinq_NewNotSupported";
        internal const string ALinq_MemberInitNotSupported = "ALinq_MemberInitNotSupported";
        internal const string ALinq_ListInitNotSupported = "ALinq_ListInitNotSupported";
        internal const string ALinq_NewArrayNotSupported = "ALinq_NewArrayNotSupported";
        internal const string ALinq_InvocationNotSupported = "ALinq_InvocationNotSupported";
        internal const string ALinq_QueryOptionsOnlyAllowedOnLeafNodes = "ALinq_QueryOptionsOnlyAllowedOnLeafNodes";
        internal const string ALinq_CantExpand = "ALinq_CantExpand";
        internal const string ALinq_CantCastToUnsupportedPrimitive = "ALinq_CantCastToUnsupportedPrimitive";
        internal const string ALinq_CantNavigateWithoutKeyPredicate = "ALinq_CantNavigateWithoutKeyPredicate";
        internal const string ALinq_CanOnlyApplyOneKeyPredicate = "ALinq_CanOnlyApplyOneKeyPredicate";
        internal const string ALinq_CantTranslateExpression = "ALinq_CantTranslateExpression";
        internal const string ALinq_TranslationError = "ALinq_TranslationError";
        internal const string ALinq_CantAddQueryOption = "ALinq_CantAddQueryOption";
        internal const string ALinq_CantAddDuplicateQueryOption = "ALinq_CantAddDuplicateQueryOption";
        internal const string ALinq_CantAddAstoriaQueryOption = "ALinq_CantAddAstoriaQueryOption";
        internal const string ALinq_CantAddQueryOptionStartingWithDollarSign = "ALinq_CantAddQueryOptionStartingWithDollarSign";
        internal const string ALinq_CantReferToPublicField = "ALinq_CantReferToPublicField";
        internal const string ALinq_QueryOptionsOnlyAllowedOnSingletons = "ALinq_QueryOptionsOnlyAllowedOnSingletons";
        internal const string ALinq_QueryOptionOutOfOrder = "ALinq_QueryOptionOutOfOrder";
        internal const string ALinq_CannotAddCountOption = "ALinq_CannotAddCountOption";
        internal const string ALinq_CannotAddCountOptionConflict = "ALinq_CannotAddCountOptionConflict";
        internal const string ALinq_ProjectionOnlyAllowedOnLeafNodes = "ALinq_ProjectionOnlyAllowedOnLeafNodes";
        internal const string ALinq_ProjectionCanOnlyHaveOneProjection = "ALinq_ProjectionCanOnlyHaveOneProjection";
        internal const string ALinq_ProjectionMemberAssignmentMismatch = "ALinq_ProjectionMemberAssignmentMismatch";
        internal const string ALinq_ExpressionNotSupportedInProjectionToEntity = "ALinq_ExpressionNotSupportedInProjectionToEntity";
        internal const string ALinq_ExpressionNotSupportedInProjection = "ALinq_ExpressionNotSupportedInProjection";
        internal const string ALinq_CannotConstructKnownEntityTypes = "ALinq_CannotConstructKnownEntityTypes";
        internal const string ALinq_CannotCreateConstantEntity = "ALinq_CannotCreateConstantEntity";
        internal const string ALinq_PropertyNamesMustMatchInProjections = "ALinq_PropertyNamesMustMatchInProjections";
        internal const string ALinq_CanOnlyProjectTheLeaf = "ALinq_CanOnlyProjectTheLeaf";
        internal const string ALinq_CannotProjectWithExplicitExpansion = "ALinq_CannotProjectWithExplicitExpansion";
        internal const string DSKAttribute_MustSpecifyAtleastOnePropertyName = "DSKAttribute_MustSpecifyAtleastOnePropertyName";
        internal const string HttpWeb_Internal = "HttpWeb_Internal";
        internal const string HttpWeb_InternalArgument = "HttpWeb_InternalArgument";
        internal const string HttpWebRequest_Aborted = "HttpWebRequest_Aborted";
        internal const string DataServiceCollection_LoadRequiresTargetCollectionObserved = "DataServiceCollection_LoadRequiresTargetCollectionObserved";
        internal const string DataServiceCollection_CannotStopTrackingChildCollection = "DataServiceCollection_CannotStopTrackingChildCollection";
        internal const string DataServiceCollection_DataServiceQueryCanNotBeEnumerated = "DataServiceCollection_DataServiceQueryCanNotBeEnumerated";
        internal const string DataServiceCollection_OperationForTrackedOnly = "DataServiceCollection_OperationForTrackedOnly";
        internal const string DataServiceCollection_CannotDetermineContextFromItems = "DataServiceCollection_CannotDetermineContextFromItems";
        internal const string DataServiceCollection_InsertIntoTrackedButNotLoadedCollection = "DataServiceCollection_InsertIntoTrackedButNotLoadedCollection";
        internal const string DataServiceCollection_MultipleLoadAsyncOperationsAtTheSameTime = "DataServiceCollection_MultipleLoadAsyncOperationsAtTheSameTime";
        internal const string DataServiceCollection_LoadAsyncNoParamsWithoutParentEntity = "DataServiceCollection_LoadAsyncNoParamsWithoutParentEntity";
        internal const string DataServiceCollection_LoadAsyncRequiresDataServiceQuery = "DataServiceCollection_LoadAsyncRequiresDataServiceQuery";
        internal const string DataBinding_DataServiceCollectionArgumentMustHaveEntityType = "DataBinding_DataServiceCollectionArgumentMustHaveEntityType";
        internal const string DataBinding_CollectionPropertySetterValueHasObserver = "DataBinding_CollectionPropertySetterValueHasObserver";
        internal const string DataBinding_CollectionChangedUnknownAction = "DataBinding_CollectionChangedUnknownAction";
        internal const string DataBinding_BindingOperation_DetachedSource = "DataBinding_BindingOperation_DetachedSource";
        internal const string DataBinding_BindingOperation_ArrayItemNull = "DataBinding_BindingOperation_ArrayItemNull";
        internal const string DataBinding_BindingOperation_ArrayItemNotEntity = "DataBinding_BindingOperation_ArrayItemNotEntity";
        internal const string DataBinding_Util_UnknownEntitySetName = "DataBinding_Util_UnknownEntitySetName";
        internal const string DataBinding_EntityAlreadyInCollection = "DataBinding_EntityAlreadyInCollection";
        internal const string DataBinding_NotifyPropertyChangedNotImpl = "DataBinding_NotifyPropertyChangedNotImpl";
        internal const string DataBinding_ComplexObjectAssociatedWithMultipleEntities = "DataBinding_ComplexObjectAssociatedWithMultipleEntities";
        internal const string AtomParser_FeedUnexpected = "AtomParser_FeedUnexpected";
        internal const string AtomParser_PagingLinkOutsideOfFeed = "AtomParser_PagingLinkOutsideOfFeed";
        internal const string AtomParser_ManyFeedCounts = "AtomParser_ManyFeedCounts";
        internal const string AtomParser_FeedCountNotUnderFeed = "AtomParser_FeedCountNotUnderFeed";
        internal const string AtomParser_UnexpectedContentUnderExpandedLink = "AtomParser_UnexpectedContentUnderExpandedLink";
        internal const string AtomMaterializer_CannotAssignNull = "AtomMaterializer_CannotAssignNull";
        internal const string AtomMaterializer_DuplicatedNextLink = "AtomMaterializer_DuplicatedNextLink";
        internal const string AtomMaterializer_EntryIntoCollectionMismatch = "AtomMaterializer_EntryIntoCollectionMismatch";
        internal const string AtomMaterializer_EntryToAccessIsNull = "AtomMaterializer_EntryToAccessIsNull";
        internal const string AtomMaterializer_EntryToInitializeIsNull = "AtomMaterializer_EntryToInitializeIsNull";
        internal const string AtomMaterializer_ProjectEntityTypeMismatch = "AtomMaterializer_ProjectEntityTypeMismatch";
        internal const string AtomMaterializer_LinksMissingHref = "AtomMaterializer_LinksMissingHref";
        internal const string AtomMaterializer_PropertyMissing = "AtomMaterializer_PropertyMissing";
        internal const string AtomMaterializer_PropertyMissingFromEntry = "AtomMaterializer_PropertyMissingFromEntry";
        internal const string AtomMaterializer_PropertyNotExpectedEntry = "AtomMaterializer_PropertyNotExpectedEntry";
        internal const string DataServiceQuery_EnumerationNotSupportedInSL = "DataServiceQuery_EnumerationNotSupportedInSL";

        static TextRes loader;
        ResourceManager resources;

        internal TextRes() {
            resources = new System.Resources.ResourceManager("System.Data.Services.Client", this.GetType().Assembly);
        }
        
        private static TextRes GetLoader() {
            if (loader == null) {
                TextRes sr = new TextRes();
                Interlocked.CompareExchange(ref loader, sr, null);
            }
            return loader;
        }

        private static CultureInfo Culture {
            get { return null; }
        }
        
#if !TEXTRES_ONLYGETSTRING
        public static ResourceManager Resources {
            get {
                return GetLoader().resources;
            }
        }
        
#endif
        public static string GetString(string name, params object[] args) {
            TextRes sys = GetLoader();
            if (sys == null)
                return null;
            string res = sys.resources.GetString(name, TextRes.Culture);

            if (args != null && args.Length > 0) {
                for (int i = 0; i < args.Length; i ++) {
                    String value = args[i] as String;
                    if (value != null && value.Length > 1024) {
                        args[i] = value.Substring(0, 1024 - 3) + "...";
                    }
                }
                return String.Format(CultureInfo.CurrentCulture, res, args);
            }
            else {
                return res;
            }
        }

        public static string GetString(string name) {
            TextRes sys = GetLoader();
            if (sys == null)
                return null;
            return sys.resources.GetString(name, TextRes.Culture);
        }
        
#if !TEXTRES_ONLYGETSTRING
        public static object GetObject(string name) {
            TextRes sys = GetLoader();
            if (sys == null)
                return null;
            return sys.resources.GetObject(name, TextRes.Culture);
        }
#endif
    }
}
