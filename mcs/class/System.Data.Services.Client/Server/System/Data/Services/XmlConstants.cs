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


namespace System.Data.Services.Client
{
    internal static class XmlConstants
    {
        #region CLR / Reflection constants.

        internal const string ClrServiceInitializationMethodName = "InitializeService";

        #endregion CLR / Reflection constants.

        #region HTTP constants.

        internal const string HttpContentID = "Content-ID";

        internal const string HttpContentLength = "Content-Length";

        internal const string HttpContentType = "Content-Type";

        internal const string HttpContentDisposition = "Content-Disposition";

        internal const string HttpDataServiceVersion = "DataServiceVersion";

        internal const string HttpMaxDataServiceVersion = "MaxDataServiceVersion";

        internal const string HttpCacheControlNoCache = "no-cache";

        internal const string HttpCharsetParameter = "charset";

        internal const string HttpMethodGet = "GET";

        internal const string HttpMethodPost = "POST";

        internal const string HttpMethodPut = "PUT";

        internal const string HttpMethodDelete = "DELETE";

        internal const string HttpMethodMerge = "MERGE";

        internal const string HttpQueryStringExpand = "$expand";

        internal const string HttpQueryStringFilter = "$filter";

        internal const string HttpQueryStringOrderBy = "$orderby";

        internal const string HttpQueryStringSkip = "$skip";

        internal const string HttpQueryStringTop = "$top";

        internal const string HttpQueryStringInlineCount = "$inlinecount";

        internal const string HttpQueryStringSkipToken = "$skiptoken";

        internal const string SkipTokenPropertyPrefix = "SkipTokenProperty";

        internal const string HttpQueryStringValueCount = "$count";

        internal const string HttpQueryStringSelect = "$select";

        internal const string HttpQValueParameter = "q";

        internal const string HttpXMethod = "X-HTTP-Method";

        internal const string HttpRequestAccept = "Accept";

        internal const string HttpRequestAcceptCharset = "Accept-Charset";

        internal const string HttpRequestIfMatch = "If-Match";

        internal const string HttpRequestIfNoneMatch = "If-None-Match";

        internal const string HttpMultipartBoundary = "boundary";
        internal const string HttpMultipartBoundaryBatch = "batch";

        internal const string HttpMultipartBoundaryChangeSet = "changeset";

        internal const string HttpResponseAllow = "Allow";

        internal const string HttpResponseCacheControl = "Cache-Control";

        internal const string HttpResponseETag = "ETag";

        internal const string HttpResponseLocation = "Location";

        internal const string HttpResponseStatusCode = "Status-Code";

        internal const string HttpMultipartBoundaryBatchResponse = "batchresponse";

        internal const string HttpMultipartBoundaryChangesetResponse = "changesetresponse";

        internal const string HttpContentTransferEncoding = "Content-Transfer-Encoding";

        internal const string HttpVersionInBatching = "HTTP/1.1";

        internal const string HttpAnyETag = "*";

        internal const string HttpWeakETagPrefix = "W/\"";

        internal const string HttpAcceptCharset = "Accept-Charset";

        internal const string HttpCookie = "Cookie";

        internal const string HttpSlug = "Slug";

        #endregion HTTP constants.

        #region MIME constants.

        internal const string MimeAny = "*/*";

        internal const string MimeApplicationAtom = "application/atom+xml";

        internal const string MimeApplicationAtomService = "application/atomsvc+xml";

        internal const string MimeApplicationJson = "application/json";

        internal const string MimeApplicationOctetStream = "application/octet-stream";

        internal const string MimeApplicationHttp = "application/http";

        internal const string MimeApplicationType = "application";

        internal const string MimeApplicationXml = "application/xml";

        internal const string MimeJsonSubType = "json";

        internal const string MimeMetadata = MimeApplicationXml;

        internal const string MimeMultiPartMixed = "multipart/mixed";

        internal const string MimeTextPlain = "text/plain";

        internal const string MimeTextType = "text";

        internal const string MimeTextXml = "text/xml";

        internal const string MimeXmlSubType = "xml";

        internal const string BatchRequestContentTransferEncoding = "binary";

        internal const string LinkMimeTypeFeed = "application/atom+xml;type=feed";

        internal const string LinkMimeTypeEntry = "application/atom+xml;type=entry";

        internal const string Utf8Encoding = "UTF-8";

        internal const string MimeTypeUtf8Encoding = ";charset=" + Utf8Encoding;
        #endregion MIME constants.

        #region URI constants.

        internal const string UriHttpAbsolutePrefix = "http://host";

        internal const string UriMetadataSegment = "$metadata";

        internal const string UriValueSegment = "$value";

        internal const string UriBatchSegment = "$batch";

        internal const string UriLinkSegment = "$links";

        internal const string UriCountSegment = "$count";

        internal const string UriRowCountAllOption = "allpages";

        internal const string UriRowCountOffOption = "none";

        #endregion URI constants.

        #region WCF constants.

        internal const string WcfBinaryElementName = "Binary";

        #endregion WCF constants.

        #region ATOM constants
        internal const string AtomContentElementName = "content";

        internal const string AtomEntryElementName = "entry";

        internal const string AtomFeedElementName = "feed";

        internal const string AtomAuthorElementName = "author";

        internal const string AtomContributorElementName = "contributor";

        internal const string AtomCategoryElementName = "category";

        internal const string AtomCategorySchemeAttributeName = "scheme";

        internal const string AtomCategoryTermAttributeName = "term";

        internal const string AtomIdElementName = "id";

        internal const string AtomLinkElementName = "link";

        internal const string AtomLinkRelationAttributeName = "rel";

        internal const string AtomContentSrcAttributeName = "src";

        internal const string AtomLinkNextAttributeString = "next";

        internal const string MetadataAttributeEpmContentKind = "FC_ContentKind";

        internal const string MetadataAttributeEpmKeepInContent = "FC_KeepInContent";

        internal const string MetadataAttributeEpmNsPrefix = "FC_NsPrefix";

        internal const string MetadataAttributeEpmNsUri = "FC_NsUri";

        internal const string MetadataAttributeEpmTargetPath = "FC_TargetPath";

        internal const string MetadataAttributeEpmSourcePath = "FC_SourcePath";

        internal const string SyndAuthorEmail = "SyndicationAuthorEmail";

        internal const string SyndAuthorName = "SyndicationAuthorName";

        internal const string SyndAuthorUri = "SyndicationAuthorUri";

        internal const string SyndPublished = "SyndicationPublished";

        internal const string SyndRights = "SyndicationRights";

        internal const string SyndSummary = "SyndicationSummary";

        internal const string SyndTitle = "SyndicationTitle";

        internal const string AtomUpdatedElementName = "updated";

        internal const string SyndContributorEmail = "SyndicationContributorEmail";

        internal const string SyndContributorName = "SyndicationContributorName";

        internal const string SyndContributorUri = "SyndicationContributorUri";

        internal const string SyndUpdated = "SyndicationUpdated";

        internal const string SyndContentKindPlaintext = "text";

        internal const string SyndContentKindHtml = "html";

        internal const string SyndContentKindXHtml = "xhtml";

        internal const string AtomHRefAttributeName = "href";

        internal const string AtomSummaryElementName = "summary";

        internal const string AtomNameElementName = "name";

        internal const string AtomEmailElementName = "email";

        internal const string AtomUriElementName = "uri";

        internal const string AtomPublishedElementName = "published";

        internal const string AtomRightsElementName = "rights";

        internal const string AtomPublishingCollectionElementName = "collection";

        internal const string AtomPublishingServiceElementName = "service";

        internal const string AtomPublishingWorkspaceDefaultValue = "Default";

        internal const string AtomPublishingWorkspaceElementName = "workspace";

        internal const string AtomTitleElementName = "title";

        internal const string AtomTypeAttributeName = "type";

        internal const string AtomSelfRelationAttributeValue = "self";

        internal const string AtomEditRelationAttributeValue = "edit";

        internal const string AtomEditMediaRelationAttributeValue = "edit-media";

        internal const string AtomNullAttributeName = "null";

        internal const string AtomETagAttributeName = "etag";
        
        internal const string AtomInlineElementName = "inline";

        internal const string AtomPropertiesElementName = "properties";

        internal const string RowCountElement = "count";

        #endregion ATOM constants

        #region XML constants.

        internal const string XmlCollectionItemElementName = "element";

        internal const string XmlErrorElementName = "error";
        
        internal const string XmlErrorCodeElementName = "code";

        internal const string XmlErrorInnerElementName = "innererror";

        internal const string XmlErrorInternalExceptionElementName = "internalexception";

        internal const string XmlErrorTypeElementName = "type";

        internal const string XmlErrorStackTraceElementName = "stacktrace";
        
        internal const string XmlErrorMessageElementName = "message";
        
        internal const string XmlFalseLiteral = "false";

        internal const string XmlTrueLiteral = "true";

        internal const string XmlInfinityLiteral = "INF";

        internal const string XmlNaNLiteral = "NaN";

        internal const string XmlBaseAttributeName = "base";

        internal const string XmlLangAttributeName = "lang";

        internal const string XmlSpaceAttributeName = "space";

        internal const string XmlSpacePreserveValue = "preserve";

        internal const string XmlBaseAttributeNameWithPrefix = "xml:base";

        #endregion XML constants.

        #region XML namespaces.

        internal const string EdmV1Namespace = "http://schemas.microsoft.com/ado/2006/04/edm";

        internal const string EdmV1dot1Namespace = "http://schemas.microsoft.com/ado/2007/05/edm";

        internal const string EdmV1dot2Namespace = "http://schemas.microsoft.com/ado/2008/01/edm";

        internal const string DataWebNamespace = "http://schemas.microsoft.com/ado/2007/08/dataservices";

        internal const string DataWebMetadataNamespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        internal const string DataWebRelatedNamespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/related/";

        internal const string DataWebSchemeNamespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/scheme";

        internal const string AppNamespace = "http://www.w3.org/2007/app";

        internal const string AtomNamespace = "http://www.w3.org/2005/Atom";

        internal const string XmlnsNamespacePrefix = "xmlns";

        internal const string XmlNamespacePrefix = "xml";

        internal const string DataWebNamespacePrefix = "d";

        internal const string DataWebMetadataNamespacePrefix = "m";

        internal const string XmlNamespacesNamespace = "http://www.w3.org/2000/xmlns/";

        internal const string EdmxNamespace = "http://schemas.microsoft.com/ado/2007/06/edmx";

        internal const string EdmxNamespacePrefix = "edmx";

        #endregion XML namespaces.

        #region CDM Schema Xml NodeNames

        #region Constant node names in the CDM schema xml

        internal const string Association = "Association";

        internal const string AssociationSet = "AssociationSet";

        internal const string ComplexType = "ComplexType";

        internal const string Dependent = "Dependent";

        internal const string EdmCollectionTypeFormat = "Collection({0})";

        internal const string EdmEntitySetAttributeName = "EntitySet";

        internal const string EdmFunctionImportElementName = "FunctionImport";

        internal const string EdmModeAttributeName = "Mode";

        internal const string EdmModeInValue = "In";

        internal const string EdmParameterElementName = "Parameter";

        internal const string EdmReturnTypeAttributeName = "ReturnType";

        internal const string End = "End";

        internal const string EntityType = "EntityType";

        internal const string EntityContainer = "EntityContainer";

        internal const string Key = "Key";

        internal const string NavigationProperty = "NavigationProperty";

        internal const string OnDelete = "OnDelete";

        internal const string Principal = "Principal";

        internal const string Property = "Property";

        internal const string PropertyRef = "PropertyRef";

        internal const string ReferentialConstraint = "ReferentialConstraint";

        internal const string Role = "Role";

        internal const string Schema = "Schema";

        internal const string EdmxElement = "Edmx";

        internal const string EdmxDataServicesElement = "DataServices";

        internal const string EdmxVersion = "Version";

        internal const string EdmxVersionValue = "1.0";

        #endregion 

        #region const attribute names in the CDM schema XML

        internal const string Action = "Action";

        internal const string BaseType = "BaseType";

        internal const string EntitySet = "EntitySet";

        internal const string FromRole = "FromRole";

        internal const string Abstract = "Abstract";

        internal const string Multiplicity = "Multiplicity";

        internal const string Name = "Name";

        internal const string Namespace = "Namespace";

        internal const string ToRole = "ToRole";

        internal const string Type = "Type";

        internal const string Relationship = "Relationship";
        #endregion 

        #region values for multiplicity in Edm

        internal const string Many = "*";

        internal const string One = "1";

        internal const string ZeroOrOne = "0..1";
        #endregion

        #region Edm Facets Names and Values

        internal const string Nullable = "Nullable";

        internal const string ConcurrencyAttribute = "ConcurrencyMode";

        internal const string ConcurrencyFixedValue = "Fixed";

        #endregion

        #endregion // CDM Schema Xml NodeNames

        #region DataWeb Elements and Attributes.

        internal const string DataWebMimeTypeAttributeName = "MimeType";

        internal const string DataWebOpenTypeAttributeName = "OpenType";

        internal const string DataWebAccessHasStreamAttribute = "HasStream";

        internal const string DataWebAccessDefaultStreamPropertyValue = "true";

        internal const string IsDefaultEntityContainerAttribute = "IsDefaultEntityContainer";

        internal const string ServiceOperationHttpMethodName = "HttpMethod";

        internal const string UriElementName = "uri";
        
        internal const string NextElementName = "next";

        internal const string LinkCollectionElementName = "links";

        #endregion DataWeb Elements and Attributes.

        #region JSON Format constants

        internal const string JsonError = "error";

        internal const string JsonErrorCode = "code";

        internal const string JsonErrorInner = "innererror";

        internal const string JsonErrorInternalException = "internalexception";

        internal const string JsonErrorMessage = "message";

        internal const string JsonErrorStackTrace = "stacktrace";

        internal const string JsonErrorType = "type";

        internal const string JsonErrorValue = "value";

        internal const string JsonMetadataString = "__metadata";

        internal const string JsonUriString = "uri";

        internal const string JsonTypeString = "type";

        internal const string JsonEditMediaString = "edit_media";

        internal const string JsonMediaSrcString = "media_src";

        internal const string JsonContentTypeString = "content_type";

        internal const string JsonMediaETagString = "media_etag";

        internal const string JsonDeferredString = "__deferred";

        internal const string JsonETagString = "etag";

        internal const string JsonRowCountString = "__count";

        internal const string JsonNextString = "__next";
        
        #endregion 

        #region Edm Primitive Type Names
        internal const string EdmNamespace = "Edm";

        internal const string EdmBinaryTypeName = "Edm.Binary";

        internal const string EdmBooleanTypeName = "Edm.Boolean";

        internal const string EdmByteTypeName = "Edm.Byte";

        internal const string EdmDateTimeTypeName = "Edm.DateTime";

        internal const string EdmDecimalTypeName = "Edm.Decimal";

        internal const string EdmDoubleTypeName = "Edm.Double";

        internal const string EdmGuidTypeName = "Edm.Guid";

        internal const string EdmSingleTypeName = "Edm.Single";

        internal const string EdmSByteTypeName = "Edm.SByte";

        internal const string EdmInt16TypeName = "Edm.Int16";

        internal const string EdmInt32TypeName = "Edm.Int32";

        internal const string EdmInt64TypeName = "Edm.Int64";

        internal const string EdmStringTypeName = "Edm.String";
        #endregion

        #region Astoria Constants

        internal const string DataServiceVersion1Dot0 = "1.0";

        internal const string DataServiceVersion2Dot0 = "2.0";

        internal const string DataServiceVersionCurrent = DataServiceVersion2Dot0 + ";";

        internal const int DataServiceVersionCurrentMajor = 1;

        internal const int DataServiceVersionCurrentMinor = 0;

        internal const string LiteralPrefixBinary = "binary";

        internal const string LiteralPrefixDateTime = "datetime";

        internal const string LiteralPrefixGuid = "guid";
        
        internal const string XmlBinaryPrefix = "X";

        internal const string XmlDecimalLiteralSuffix = "M";

        internal const string XmlInt64LiteralSuffix = "L";

        internal const string XmlSingleLiteralSuffix = "f";

        internal const string XmlDoubleLiteralSuffix = "D";

        internal const string NullLiteralInETag = "null";

        internal const string MicrosoftDataServicesRequestUri = "MicrosoftDataServicesRequestUri";

        internal const string MicrosoftDataServicesRootUri = "MicrosoftDataServicesRootUri";

        #endregion 
    }
}
