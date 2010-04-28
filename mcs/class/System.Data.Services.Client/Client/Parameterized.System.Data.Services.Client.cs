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
    using System.Resources;

    internal static class Strings {
        internal static string BatchStream_MissingBoundary {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.BatchStream_MissingBoundary);
            }
        }

        internal static string BatchStream_ContentExpected(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.BatchStream_ContentExpected,p0);
        }

        internal static string BatchStream_ContentUnexpected(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.BatchStream_ContentUnexpected,p0);
        }

        internal static string BatchStream_GetMethodNotSupportedInChangeset {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.BatchStream_GetMethodNotSupportedInChangeset);
            }
        }

        internal static string BatchStream_InvalidBatchFormat {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.BatchStream_InvalidBatchFormat);
            }
        }

        internal static string BatchStream_InvalidDelimiter(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.BatchStream_InvalidDelimiter,p0);
        }

        internal static string BatchStream_MissingEndChangesetDelimiter {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.BatchStream_MissingEndChangesetDelimiter);
            }
        }

        internal static string BatchStream_InvalidHeaderValueSpecified(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.BatchStream_InvalidHeaderValueSpecified,p0);
        }

        internal static string BatchStream_InvalidContentLengthSpecified(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.BatchStream_InvalidContentLengthSpecified,p0);
        }

        internal static string BatchStream_OnlyGETOperationsCanBeSpecifiedInBatch {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.BatchStream_OnlyGETOperationsCanBeSpecifiedInBatch);
            }
        }

        internal static string BatchStream_InvalidOperationHeaderSpecified {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.BatchStream_InvalidOperationHeaderSpecified);
            }
        }

        internal static string BatchStream_InvalidHttpMethodName(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.BatchStream_InvalidHttpMethodName,p0);
        }

        internal static string BatchStream_MoreDataAfterEndOfBatch {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.BatchStream_MoreDataAfterEndOfBatch);
            }
        }

        internal static string BatchStream_InternalBufferRequestTooSmall {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.BatchStream_InternalBufferRequestTooSmall);
            }
        }

        internal static string BatchStream_InvalidMethodHeaderSpecified(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.BatchStream_InvalidMethodHeaderSpecified,p0);
        }

        internal static string BatchStream_InvalidHttpVersionSpecified(object p0, object p1) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.BatchStream_InvalidHttpVersionSpecified,p0,p1);
        }

        internal static string BatchStream_InvalidNumberOfHeadersAtOperationStart(object p0, object p1) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.BatchStream_InvalidNumberOfHeadersAtOperationStart,p0,p1);
        }

        internal static string BatchStream_MissingOrInvalidContentEncodingHeader(object p0, object p1) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.BatchStream_MissingOrInvalidContentEncodingHeader,p0,p1);
        }

        internal static string BatchStream_InvalidNumberOfHeadersAtChangeSetStart(object p0, object p1) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.BatchStream_InvalidNumberOfHeadersAtChangeSetStart,p0,p1);
        }

        internal static string BatchStream_MissingContentTypeHeader(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.BatchStream_MissingContentTypeHeader,p0);
        }

        internal static string BatchStream_InvalidContentTypeSpecified(object p0, object p1, object p2, object p3) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.BatchStream_InvalidContentTypeSpecified,p0,p1,p2,p3);
        }

        internal static string Batch_ExpectedContentType(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Batch_ExpectedContentType,p0);
        }

        internal static string Batch_ExpectedResponse(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Batch_ExpectedResponse,p0);
        }

        internal static string Batch_IncompleteResponseCount {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Batch_IncompleteResponseCount);
            }
        }

        internal static string Batch_UnexpectedContent(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Batch_UnexpectedContent,p0);
        }

        internal static string Context_BaseUri {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_BaseUri);
            }
        }

        internal static string Context_CannotConvertKey(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_CannotConvertKey,p0);
        }

        internal static string Context_TrackingExpectsAbsoluteUri {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_TrackingExpectsAbsoluteUri);
            }
        }

        internal static string Context_LinkResourceInsertFailure {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_LinkResourceInsertFailure);
            }
        }

        internal static string Context_InternalError(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_InternalError,p0);
        }

        internal static string Context_BatchExecuteError {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_BatchExecuteError);
            }
        }

        internal static string Context_EntitySetName {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_EntitySetName);
            }
        }

        internal static string Context_MissingEditLinkInResponseBody {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_MissingEditLinkInResponseBody);
            }
        }

        internal static string Context_MissingSelfLinkInResponseBody {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_MissingSelfLinkInResponseBody);
            }
        }

        internal static string Context_MissingEditMediaLinkInResponseBody {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_MissingEditMediaLinkInResponseBody);
            }
        }

        internal static string Content_EntityWithoutKey {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Content_EntityWithoutKey);
            }
        }

        internal static string Content_EntityIsNotEntityType {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Content_EntityIsNotEntityType);
            }
        }

        internal static string Context_EntityNotContained {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_EntityNotContained);
            }
        }

        internal static string Context_EntityAlreadyContained {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_EntityAlreadyContained);
            }
        }

        internal static string Context_DifferentEntityAlreadyContained {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_DifferentEntityAlreadyContained);
            }
        }

        internal static string Context_DidNotOriginateAsync {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_DidNotOriginateAsync);
            }
        }

        internal static string Context_AsyncAlreadyDone {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_AsyncAlreadyDone);
            }
        }

        internal static string Context_OperationCanceled {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_OperationCanceled);
            }
        }

        internal static string Context_NoLoadWithInsertEnd {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_NoLoadWithInsertEnd);
            }
        }

        internal static string Context_NoRelationWithInsertEnd {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_NoRelationWithInsertEnd);
            }
        }

        internal static string Context_NoRelationWithDeleteEnd {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_NoRelationWithDeleteEnd);
            }
        }

        internal static string Context_RelationAlreadyContained {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_RelationAlreadyContained);
            }
        }

        internal static string Context_RelationNotRefOrCollection {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_RelationNotRefOrCollection);
            }
        }

        internal static string Context_AddLinkCollectionOnly {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_AddLinkCollectionOnly);
            }
        }

        internal static string Context_AddRelatedObjectCollectionOnly {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_AddRelatedObjectCollectionOnly);
            }
        }

        internal static string Context_AddRelatedObjectSourceDeleted {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_AddRelatedObjectSourceDeleted);
            }
        }

        internal static string Context_SetLinkReferenceOnly {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_SetLinkReferenceOnly);
            }
        }

        internal static string Context_NoContentTypeForMediaLink(object p0, object p1) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_NoContentTypeForMediaLink,p0,p1);
        }

        internal static string Context_BatchNotSupportedForMediaLink {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_BatchNotSupportedForMediaLink);
            }
        }

        internal static string Context_UnexpectedZeroRawRead {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_UnexpectedZeroRawRead);
            }
        }

        internal static string Context_VersionNotSupported(object p0, object p1) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_VersionNotSupported,p0,p1);
        }

        internal static string Context_SendingRequestEventArgsNotHttp {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_SendingRequestEventArgsNotHttp);
            }
        }

        internal static string Context_ChildResourceExists {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_ChildResourceExists);
            }
        }

        internal static string Context_EntityNotMediaLinkEntry {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_EntityNotMediaLinkEntry);
            }
        }

        internal static string Context_MLEWithoutSaveStream(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_MLEWithoutSaveStream,p0);
        }

        internal static string Context_SetSaveStreamOnMediaEntryProperty(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_SetSaveStreamOnMediaEntryProperty,p0);
        }

        internal static string Context_SetSaveStreamWithoutEditMediaLink {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Context_SetSaveStreamWithoutEditMediaLink);
            }
        }

        internal static string Collection_NullCollectionReference(object p0, object p1) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Collection_NullCollectionReference,p0,p1);
        }

        internal static string ClientType_MissingOpenProperty(object p0, object p1) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ClientType_MissingOpenProperty,p0,p1);
        }

        internal static string Clienttype_MultipleOpenProperty(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Clienttype_MultipleOpenProperty,p0);
        }

        internal static string ClientType_MissingProperty(object p0, object p1) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ClientType_MissingProperty,p0,p1);
        }

        internal static string ClientType_KeysMustBeSimpleTypes(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ClientType_KeysMustBeSimpleTypes,p0);
        }

        internal static string ClientType_KeysOnDifferentDeclaredType(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ClientType_KeysOnDifferentDeclaredType,p0);
        }

        internal static string ClientType_MissingMimeTypeProperty(object p0, object p1) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ClientType_MissingMimeTypeProperty,p0,p1);
        }

        internal static string ClientType_MissingMediaEntryProperty(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ClientType_MissingMediaEntryProperty,p0);
        }

        internal static string ClientType_NoSettableFields(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ClientType_NoSettableFields,p0);
        }

        internal static string ClientType_MultipleImplementationNotSupported {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ClientType_MultipleImplementationNotSupported);
            }
        }

        internal static string ClientType_NullOpenProperties(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ClientType_NullOpenProperties,p0);
        }

        internal static string ClientType_CollectionOfNonEntities {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ClientType_CollectionOfNonEntities);
            }
        }

        internal static string ClientType_Ambiguous(object p0, object p1) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ClientType_Ambiguous,p0,p1);
        }

        internal static string DataServiceException_GeneralError {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.DataServiceException_GeneralError);
            }
        }

        internal static string DataServiceRequest_FailGetCount {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.DataServiceRequest_FailGetCount);
            }
        }

        internal static string Deserialize_GetEnumerator {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Deserialize_GetEnumerator);
            }
        }

        internal static string Deserialize_Current(object p0, object p1) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Deserialize_Current,p0,p1);
        }

        internal static string Deserialize_MixedTextWithComment {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Deserialize_MixedTextWithComment);
            }
        }

        internal static string Deserialize_ExpectingSimpleValue {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Deserialize_ExpectingSimpleValue);
            }
        }

        internal static string Deserialize_NotApplicationXml {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Deserialize_NotApplicationXml);
            }
        }

        internal static string Deserialize_MismatchAtomLinkLocalSimple {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Deserialize_MismatchAtomLinkLocalSimple);
            }
        }

        internal static string Deserialize_MismatchAtomLinkFeedPropertyNotCollection(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Deserialize_MismatchAtomLinkFeedPropertyNotCollection,p0);
        }

        internal static string Deserialize_MismatchAtomLinkEntryPropertyIsCollection(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Deserialize_MismatchAtomLinkEntryPropertyIsCollection,p0);
        }

        internal static string Deserialize_UnknownMimeTypeSpecified(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Deserialize_UnknownMimeTypeSpecified,p0);
        }

        internal static string Deserialize_ExpectedEmptyMediaLinkEntryContent {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Deserialize_ExpectedEmptyMediaLinkEntryContent);
            }
        }

        internal static string Deserialize_ContentPlusPropertiesNotAllowed {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Deserialize_ContentPlusPropertiesNotAllowed);
            }
        }

        internal static string Deserialize_NoLocationHeader {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Deserialize_NoLocationHeader);
            }
        }

        internal static string Deserialize_ServerException(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Deserialize_ServerException,p0);
        }

        internal static string Deserialize_MissingIdElement {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Deserialize_MissingIdElement);
            }
        }

        internal static string EpmClientType_PropertyIsComplex(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.EpmClientType_PropertyIsComplex,p0);
        }

        internal static string EpmClientType_PropertyIsPrimitive(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.EpmClientType_PropertyIsPrimitive,p0);
        }

        internal static string EpmSourceTree_InvalidSourcePath(object p0, object p1) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.EpmSourceTree_InvalidSourcePath,p0,p1);
        }

        internal static string EpmSourceTree_DuplicateEpmAttrsWithSameSourceName(object p0, object p1) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.EpmSourceTree_DuplicateEpmAttrsWithSameSourceName,p0,p1);
        }

        internal static string EpmSourceTree_InaccessiblePropertyOnType(object p0, object p1) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.EpmSourceTree_InaccessiblePropertyOnType,p0,p1);
        }

        internal static string EpmTargetTree_InvalidTargetPath(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.EpmTargetTree_InvalidTargetPath,p0);
        }

        internal static string EpmTargetTree_AttributeInMiddle(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.EpmTargetTree_AttributeInMiddle,p0);
        }

        internal static string EpmTargetTree_DuplicateEpmAttrsWithSameTargetName(object p0, object p1, object p2, object p3) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.EpmTargetTree_DuplicateEpmAttrsWithSameTargetName,p0,p1,p2,p3);
        }

        internal static string EntityPropertyMapping_EpmAttribute(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.EntityPropertyMapping_EpmAttribute,p0);
        }

        internal static string EntityPropertyMapping_TargetNamespaceUriNotValid(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.EntityPropertyMapping_TargetNamespaceUriNotValid,p0);
        }

        internal static string HttpProcessUtility_ContentTypeMissing {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.HttpProcessUtility_ContentTypeMissing);
            }
        }

        internal static string HttpProcessUtility_MediaTypeMissingValue {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.HttpProcessUtility_MediaTypeMissingValue);
            }
        }

        internal static string HttpProcessUtility_MediaTypeRequiresSemicolonBeforeParameter {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.HttpProcessUtility_MediaTypeRequiresSemicolonBeforeParameter);
            }
        }

        internal static string HttpProcessUtility_MediaTypeRequiresSlash {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.HttpProcessUtility_MediaTypeRequiresSlash);
            }
        }

        internal static string HttpProcessUtility_MediaTypeRequiresSubType {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.HttpProcessUtility_MediaTypeRequiresSubType);
            }
        }

        internal static string HttpProcessUtility_MediaTypeUnspecified {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.HttpProcessUtility_MediaTypeUnspecified);
            }
        }

        internal static string HttpProcessUtility_EncodingNotSupported(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.HttpProcessUtility_EncodingNotSupported,p0);
        }

        internal static string HttpProcessUtility_EscapeCharWithoutQuotes(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.HttpProcessUtility_EscapeCharWithoutQuotes,p0);
        }

        internal static string HttpProcessUtility_EscapeCharAtEnd(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.HttpProcessUtility_EscapeCharAtEnd,p0);
        }

        internal static string HttpProcessUtility_ClosingQuoteNotFound(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.HttpProcessUtility_ClosingQuoteNotFound,p0);
        }

        internal static string MaterializeFromAtom_CountNotPresent {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.MaterializeFromAtom_CountNotPresent);
            }
        }

        internal static string MaterializeFromAtom_CountFormatError {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.MaterializeFromAtom_CountFormatError);
            }
        }

        internal static string MaterializeFromAtom_TopLevelLinkNotAvailable {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.MaterializeFromAtom_TopLevelLinkNotAvailable);
            }
        }

        internal static string MaterializeFromAtom_CollectionKeyNotPresentInLinkTable {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.MaterializeFromAtom_CollectionKeyNotPresentInLinkTable);
            }
        }

        internal static string MaterializeFromAtom_GetNestLinkForFlatCollection {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.MaterializeFromAtom_GetNestLinkForFlatCollection);
            }
        }

        internal static string Serializer_NullKeysAreNotSupported(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Serializer_NullKeysAreNotSupported,p0);
        }

        internal static string Util_EmptyString {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Util_EmptyString);
            }
        }

        internal static string Util_EmptyArray {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Util_EmptyArray);
            }
        }

        internal static string Util_NullArrayElement {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.Util_NullArrayElement);
            }
        }

        internal static string ALinq_UnsupportedExpression(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_UnsupportedExpression,p0);
        }

        internal static string ALinq_CouldNotConvert(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_CouldNotConvert,p0);
        }

        internal static string ALinq_MethodNotSupported(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_MethodNotSupported,p0);
        }

        internal static string ALinq_UnaryNotSupported(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_UnaryNotSupported,p0);
        }

        internal static string ALinq_BinaryNotSupported(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_BinaryNotSupported,p0);
        }

        internal static string ALinq_ConstantNotSupported(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_ConstantNotSupported,p0);
        }

        internal static string ALinq_TypeBinaryNotSupported {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_TypeBinaryNotSupported);
            }
        }

        internal static string ALinq_ConditionalNotSupported {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_ConditionalNotSupported);
            }
        }

        internal static string ALinq_ParameterNotSupported {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_ParameterNotSupported);
            }
        }

        internal static string ALinq_MemberAccessNotSupported(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_MemberAccessNotSupported,p0);
        }

        internal static string ALinq_LambdaNotSupported {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_LambdaNotSupported);
            }
        }

        internal static string ALinq_NewNotSupported {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_NewNotSupported);
            }
        }

        internal static string ALinq_MemberInitNotSupported {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_MemberInitNotSupported);
            }
        }

        internal static string ALinq_ListInitNotSupported {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_ListInitNotSupported);
            }
        }

        internal static string ALinq_NewArrayNotSupported {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_NewArrayNotSupported);
            }
        }

        internal static string ALinq_InvocationNotSupported {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_InvocationNotSupported);
            }
        }

        internal static string ALinq_QueryOptionsOnlyAllowedOnLeafNodes {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_QueryOptionsOnlyAllowedOnLeafNodes);
            }
        }

        internal static string ALinq_CantExpand {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_CantExpand);
            }
        }

        internal static string ALinq_CantCastToUnsupportedPrimitive(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_CantCastToUnsupportedPrimitive,p0);
        }

        internal static string ALinq_CantNavigateWithoutKeyPredicate {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_CantNavigateWithoutKeyPredicate);
            }
        }

        internal static string ALinq_CanOnlyApplyOneKeyPredicate {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_CanOnlyApplyOneKeyPredicate);
            }
        }

        internal static string ALinq_CantTranslateExpression(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_CantTranslateExpression,p0);
        }

        internal static string ALinq_TranslationError(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_TranslationError,p0);
        }

        internal static string ALinq_CantAddQueryOption {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_CantAddQueryOption);
            }
        }

        internal static string ALinq_CantAddDuplicateQueryOption(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_CantAddDuplicateQueryOption,p0);
        }

        internal static string ALinq_CantAddAstoriaQueryOption(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_CantAddAstoriaQueryOption,p0);
        }

        internal static string ALinq_CantAddQueryOptionStartingWithDollarSign(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_CantAddQueryOptionStartingWithDollarSign,p0);
        }

        internal static string ALinq_CantReferToPublicField(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_CantReferToPublicField,p0);
        }

        internal static string ALinq_QueryOptionsOnlyAllowedOnSingletons {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_QueryOptionsOnlyAllowedOnSingletons);
            }
        }

        internal static string ALinq_QueryOptionOutOfOrder(object p0, object p1) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_QueryOptionOutOfOrder,p0,p1);
        }

        internal static string ALinq_CannotAddCountOption {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_CannotAddCountOption);
            }
        }

        internal static string ALinq_CannotAddCountOptionConflict {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_CannotAddCountOptionConflict);
            }
        }

        internal static string ALinq_ProjectionOnlyAllowedOnLeafNodes {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_ProjectionOnlyAllowedOnLeafNodes);
            }
        }

        internal static string ALinq_ProjectionCanOnlyHaveOneProjection {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_ProjectionCanOnlyHaveOneProjection);
            }
        }

        internal static string ALinq_ProjectionMemberAssignmentMismatch(object p0, object p1, object p2) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_ProjectionMemberAssignmentMismatch,p0,p1,p2);
        }

        internal static string ALinq_ExpressionNotSupportedInProjectionToEntity(object p0, object p1) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_ExpressionNotSupportedInProjectionToEntity,p0,p1);
        }

        internal static string ALinq_ExpressionNotSupportedInProjection(object p0, object p1) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_ExpressionNotSupportedInProjection,p0,p1);
        }

        internal static string ALinq_CannotConstructKnownEntityTypes {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_CannotConstructKnownEntityTypes);
            }
        }

        internal static string ALinq_CannotCreateConstantEntity {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_CannotCreateConstantEntity);
            }
        }

        internal static string ALinq_PropertyNamesMustMatchInProjections(object p0, object p1) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_PropertyNamesMustMatchInProjections,p0,p1);
        }

        internal static string ALinq_CanOnlyProjectTheLeaf {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_CanOnlyProjectTheLeaf);
            }
        }

        internal static string ALinq_CannotProjectWithExplicitExpansion {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.ALinq_CannotProjectWithExplicitExpansion);
            }
        }

        internal static string DSKAttribute_MustSpecifyAtleastOnePropertyName {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.DSKAttribute_MustSpecifyAtleastOnePropertyName);
            }
        }

        internal static string DataServiceCollection_LoadRequiresTargetCollectionObserved {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.DataServiceCollection_LoadRequiresTargetCollectionObserved);
            }
        }

        internal static string DataServiceCollection_CannotStopTrackingChildCollection {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.DataServiceCollection_CannotStopTrackingChildCollection);
            }
        }

        internal static string DataServiceCollection_OperationForTrackedOnly {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.DataServiceCollection_OperationForTrackedOnly);
            }
        }

        internal static string DataServiceCollection_CannotDetermineContextFromItems {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.DataServiceCollection_CannotDetermineContextFromItems);
            }
        }

        internal static string DataServiceCollection_InsertIntoTrackedButNotLoadedCollection {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.DataServiceCollection_InsertIntoTrackedButNotLoadedCollection);
            }
        }

        internal static string DataBinding_DataServiceCollectionArgumentMustHaveEntityType(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.DataBinding_DataServiceCollectionArgumentMustHaveEntityType,p0);
        }

        internal static string DataBinding_CollectionPropertySetterValueHasObserver(object p0, object p1) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.DataBinding_CollectionPropertySetterValueHasObserver,p0,p1);
        }

        internal static string DataBinding_CollectionChangedUnknownAction(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.DataBinding_CollectionChangedUnknownAction,p0);
        }

        internal static string DataBinding_BindingOperation_DetachedSource {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.DataBinding_BindingOperation_DetachedSource);
            }
        }

        internal static string DataBinding_BindingOperation_ArrayItemNull(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.DataBinding_BindingOperation_ArrayItemNull,p0);
        }

        internal static string DataBinding_BindingOperation_ArrayItemNotEntity(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.DataBinding_BindingOperation_ArrayItemNotEntity,p0);
        }

        internal static string DataBinding_Util_UnknownEntitySetName(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.DataBinding_Util_UnknownEntitySetName,p0);
        }

        internal static string DataBinding_EntityAlreadyInCollection(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.DataBinding_EntityAlreadyInCollection,p0);
        }

        internal static string DataBinding_NotifyPropertyChangedNotImpl(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.DataBinding_NotifyPropertyChangedNotImpl,p0);
        }

        internal static string DataBinding_ComplexObjectAssociatedWithMultipleEntities(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.DataBinding_ComplexObjectAssociatedWithMultipleEntities,p0);
        }

        internal static string AtomParser_FeedUnexpected {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.AtomParser_FeedUnexpected);
            }
        }

        internal static string AtomParser_PagingLinkOutsideOfFeed {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.AtomParser_PagingLinkOutsideOfFeed);
            }
        }

        internal static string AtomParser_ManyFeedCounts {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.AtomParser_ManyFeedCounts);
            }
        }

        internal static string AtomParser_FeedCountNotUnderFeed {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.AtomParser_FeedCountNotUnderFeed);
            }
        }

        internal static string AtomParser_UnexpectedContentUnderExpandedLink {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.AtomParser_UnexpectedContentUnderExpandedLink);
            }
        }

        internal static string AtomMaterializer_CannotAssignNull(object p0, object p1) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.AtomMaterializer_CannotAssignNull,p0,p1);
        }

        internal static string AtomMaterializer_DuplicatedNextLink {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.AtomMaterializer_DuplicatedNextLink);
            }
        }

        internal static string AtomMaterializer_EntryIntoCollectionMismatch(object p0, object p1) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.AtomMaterializer_EntryIntoCollectionMismatch,p0,p1);
        }

        internal static string AtomMaterializer_EntryToAccessIsNull(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.AtomMaterializer_EntryToAccessIsNull,p0);
        }

        internal static string AtomMaterializer_EntryToInitializeIsNull(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.AtomMaterializer_EntryToInitializeIsNull,p0);
        }

        internal static string AtomMaterializer_ProjectEntityTypeMismatch(object p0, object p1, object p2) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.AtomMaterializer_ProjectEntityTypeMismatch,p0,p1,p2);
        }

        internal static string AtomMaterializer_LinksMissingHref {
            get {
                return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.AtomMaterializer_LinksMissingHref);
            }
        }

        internal static string AtomMaterializer_PropertyMissing(object p0) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.AtomMaterializer_PropertyMissing,p0);
        }

        internal static string AtomMaterializer_PropertyMissingFromEntry(object p0, object p1) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.AtomMaterializer_PropertyMissingFromEntry,p0,p1);
        }

        internal static string AtomMaterializer_PropertyNotExpectedEntry(object p0, object p1) {
            return System.Data.Services.Client.TextRes.GetString(System.Data.Services.Client.TextRes.AtomMaterializer_PropertyNotExpectedEntry,p0,p1);
        }

    }

    internal static partial class Error {

        internal static Exception ArgumentNull(string paramName) {
            return new ArgumentNullException(paramName);
        }
        
        internal static Exception ArgumentOutOfRange(string paramName) {
            return new ArgumentOutOfRangeException(paramName);
        }

        internal static Exception NotImplemented() {
            return new NotImplementedException();
        }

        internal static Exception NotSupported() {
            return new NotSupportedException();
        }        
    }
}
