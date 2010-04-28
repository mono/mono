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
    #region Namespaces.

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Data.Services.Common;
#if !ASTORIA_LIGHT
    using System.Net;
#else    
    using System.Data.Services.Http;
#endif
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;

    #endregion Namespaces.

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506", Justification = "Central class of the API, likely to have many cross-references")]
    public class DataServiceContext
    {
#if !TESTUNIXNEWLINE
        private static readonly string NewLine = System.Environment.NewLine;
#else
        private const string NewLine = "\n";
#endif

        private readonly System.Uri baseUri;

        private readonly System.Uri baseUriWithSlash;

#if !ASTORIA_LIGHT        
        private System.Net.ICredentials credentials;
#endif

          private string dataNamespace;

        private Func<Type, string> resolveName;

        private Func<string, Type> resolveType;

#if !ASTORIA_LIGHT        
        private int timeout;
#endif

        private bool postTunneling;

        private bool ignoreMissingProperties;

        private MergeOption mergeOption;

        private SaveChangesOptions saveChangesDefaultOptions;

        private Uri typeScheme;

        private bool ignoreResourceNotFoundException;

#if ASTORIA_LIGHT        
        private HttpStack httpStack;
#endif

        #region Resource state management

        private uint nextChange;

        private Dictionary<object, EntityDescriptor> entityDescriptors = new Dictionary<object, EntityDescriptor>(EqualityComparer<object>.Default);

        private Dictionary<String, EntityDescriptor> identityToDescriptor;

        private Dictionary<LinkDescriptor, LinkDescriptor> bindings = new Dictionary<LinkDescriptor, LinkDescriptor>(LinkDescriptor.EquivalenceComparer);

        private bool applyingChanges;

        #endregion

        #region ctor

        public DataServiceContext(Uri serviceRoot)
        {
            Util.CheckArgumentNull(serviceRoot, "serviceRoot");

#if ASTORIA_LIGHT
            if (!serviceRoot.IsAbsoluteUri)
            {
                if (XHRHttpWebRequest.IsAvailable())
                {
                    serviceRoot = new Uri(System.Windows.Browser.HtmlPage.Document.DocumentUri, serviceRoot);
                }
                else
                {
                    System.Net.WebClient webClient = new System.Net.WebClient();
                    serviceRoot = new Uri(new Uri(webClient.BaseAddress), serviceRoot);
                }
            }
#endif
            if (!serviceRoot.IsAbsoluteUri ||
                !Uri.IsWellFormedUriString(serviceRoot.OriginalString, UriKind.Absolute) ||
                !String.IsNullOrEmpty(serviceRoot.Query) ||
                !string.IsNullOrEmpty(serviceRoot.Fragment) ||
                ((serviceRoot.Scheme != "http") && (serviceRoot.Scheme != "https")))
            {
                throw Error.Argument(Strings.Context_BaseUri, "serviceRoot");
            }

            this.baseUri = serviceRoot;
            this.baseUriWithSlash = serviceRoot;
            if (!serviceRoot.OriginalString.EndsWith("/", StringComparison.Ordinal))
            {
                this.baseUriWithSlash = Util.CreateUri(serviceRoot.OriginalString + "/", UriKind.Absolute);
            }

            this.mergeOption = MergeOption.AppendOnly;
            this.DataNamespace = XmlConstants.DataWebNamespace;
#if ASTORIA_LIGHT
            this.UsePostTunneling = true;
#else
            this.UsePostTunneling = false;
#endif
            this.typeScheme = new Uri(XmlConstants.DataWebSchemeNamespace);
#if ASTORIA_LIGHT
            this.httpStack = HttpStack.Auto;
#endif
        }

        #endregion

        public event EventHandler<SendingRequestEventArgs> SendingRequest;

        public event EventHandler<ReadingWritingEntityEventArgs> ReadingEntity;

        public event EventHandler<ReadingWritingEntityEventArgs> WritingEntity;

        internal event EventHandler<SaveChangesEventArgs> ChangesSaved;

        #region BaseUri, Credentials, MergeOption, Timeout, Links, Entities
        public Uri BaseUri
        {
            get { return this.baseUri; }
        }

#if !ASTORIA_LIGHT        
        public System.Net.ICredentials Credentials
        {
            get { return this.credentials; }
            set { this.credentials = value; }
        }
#endif

        public MergeOption MergeOption
        {
            get { return this.mergeOption; }
            set { this.mergeOption = Util.CheckEnumerationValue(value, "MergeOption"); }
        }

        public bool ApplyingChanges
        {
            get { return this.applyingChanges; }
            internal set { this.applyingChanges = value; }
        }

        public bool IgnoreMissingProperties
        {
            get { return this.ignoreMissingProperties; }
            set { this.ignoreMissingProperties = value; }
        }

        public string DataNamespace
        {
            get
            {
                return this.dataNamespace;
            }

            set
            {
                Util.CheckArgumentNull(value, "value");
                this.dataNamespace = value;
            }
        }

        public Func<Type, string> ResolveName
        {
            get { return this.resolveName; }
            set { this.resolveName = value; }
        }

        public Func<string, Type> ResolveType
        {
            get { return this.resolveType; }
            set { this.resolveType = value; }
        }

#if !ASTORIA_LIGHT        
        public int Timeout
        {
            get
            {
                return this.timeout;
            }

            set
            {
                if (value < 0)
                {
                    throw Error.ArgumentOutOfRange("Timeout");
                }

                this.timeout = value;
            }
        }
#endif

        public Uri TypeScheme
        {
            get
            {
                return this.typeScheme;
            }

            set
            {
                Util.CheckArgumentNull(value, "value");
                this.typeScheme = value;
            }
        }

        public bool UsePostTunneling
        {
            get { return this.postTunneling; }
            set { this.postTunneling = value; }
        }

        public ReadOnlyCollection<LinkDescriptor> Links
        {
            get
            {
                return this.bindings.Values.OrderBy(l => l.ChangeOrder).ToList().AsReadOnly();
            }
        }

        public ReadOnlyCollection<EntityDescriptor> Entities
        {
            get
            {
                return this.entityDescriptors.Values.OrderBy(d => d.ChangeOrder).ToList().AsReadOnly();
            }
        }

        public SaveChangesOptions SaveChangesDefaultOptions
        {
            get
            {
                return this.saveChangesDefaultOptions;
            }

            set
            {
                ValidateSaveChangesOptions(value);
                this.saveChangesDefaultOptions = value;
            }
        }

        #endregion

        public bool IgnoreResourceNotFoundException
        {
            get { return this.ignoreResourceNotFoundException; }
            set { this.ignoreResourceNotFoundException = value; }
        }

#if ASTORIA_LIGHT        
public HttpStack HttpStack
        {
            get { return this.httpStack; }
            set { this.httpStack = Util.CheckEnumerationValue(value, "HttpStack"); }
        }
#endif

        internal Uri BaseUriWithSlash
        {
            get { return this.baseUriWithSlash; }
        }

        internal bool HasReadingEntityHandlers
        {
            [DebuggerStepThrough]
            get { return this.ReadingEntity != null; }
        }

        #region Entity and Link Tracking
        
        public EntityDescriptor GetEntityDescriptor(object entity)
        {
            Util.CheckArgumentNull(entity, "entity");

            EntityDescriptor descriptor;
            if (this.entityDescriptors.TryGetValue(entity, out descriptor))
            {
                return descriptor;
            }
            else
            {
                return null;
            }
        }
        
        public LinkDescriptor GetLinkDescriptor(object source, string sourceProperty, object target)
        {
            Util.CheckArgumentNull(source, "source");
            Util.CheckArgumentNotEmpty(sourceProperty, "sourceProperty");
            Util.CheckArgumentNull(target, "target");
            
            LinkDescriptor link;
            
            if (this.bindings.TryGetValue(new LinkDescriptor(source, sourceProperty, target), out link))
            {
                return link;
            }
            else
            {
                return null;
            }
        }
        
        #endregion

        #region CancelRequest
        public void CancelRequest(IAsyncResult asyncResult)
        {
            Util.CheckArgumentNull(asyncResult, "asyncResult");
            BaseAsyncResult result = asyncResult as BaseAsyncResult;

            if ((null == result) || (this != result.Source))
            {
                object context = null;
                DataServiceQuery query = null;
                if (null != result)
                {
                    query = result.Source as DataServiceQuery;

                    if (null != query)
                    {
                        DataServiceQueryProvider provider = query.Provider as DataServiceQueryProvider;
                        if (null != provider)
                        {
                            context = provider.Context;
                        }
                    }
                }

                if (this != context)
                {
                    throw Error.Argument(Strings.Context_DidNotOriginateAsync, "asyncResult");
                }
            }

            if (!result.IsCompletedInternally)
            {
                result.SetAborted();

                WebRequest request = result.Abortable;
                if (null != request)
                {
                    request.Abort();
                }
            }
        }
        #endregion

        #region CreateQuery
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "required for this feature")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads", Justification = "required for this feature")]
        public DataServiceQuery<T> CreateQuery<T>(string entitySetName)
        {
            Util.CheckArgumentNotEmpty(entitySetName, "entitySetName");
            this.ValidateEntitySetName(ref entitySetName);

            ResourceSetExpression rse = new ResourceSetExpression(typeof(IOrderedQueryable<T>), null, Expression.Constant(entitySetName), typeof(T), null, CountOption.None, null, null);
            return new DataServiceQuery<T>.DataServiceOrderedQuery(rse, new DataServiceQueryProvider(this));
        }
        #endregion

        #region GetMetadataUri
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "required for this feature")]
        public Uri GetMetadataUri()
        {
            Uri metadataUri = Util.CreateUri(this.baseUriWithSlash.OriginalString + XmlConstants.UriMetadataSegment, UriKind.Absolute);
            return metadataUri;
        }
        #endregion

        #region LoadProperty

        public IAsyncResult BeginLoadProperty(object entity, string propertyName, AsyncCallback callback, object state)
        {
            return this.BeginLoadProperty(entity, propertyName, (Uri)null, callback, state);
        }

        public IAsyncResult BeginLoadProperty(object entity, string propertyName, Uri nextLinkUri, AsyncCallback callback, object state)
        {
            LoadPropertyResult result = this.CreateLoadPropertyRequest(entity, propertyName, callback, state, nextLinkUri, null);
            result.BeginExecute();
            return result;
        }

        public IAsyncResult BeginLoadProperty(object entity, string propertyName, DataServiceQueryContinuation continuation, AsyncCallback callback, object state)
        {
            Util.CheckArgumentNull(continuation, "continuation");
            LoadPropertyResult result = this.CreateLoadPropertyRequest(entity, propertyName, callback, state, null, continuation);
            result.BeginExecute();
            return result;
        }

        public QueryOperationResponse EndLoadProperty(IAsyncResult asyncResult)
        {
            LoadPropertyResult response = QueryResult.EndExecute<LoadPropertyResult>(this, "LoadProperty", asyncResult);
            return response.LoadProperty();
        }

#if !ASTORIA_LIGHT        
        public QueryOperationResponse LoadProperty(object entity, string propertyName)
        {
            return this.LoadProperty(entity, propertyName, (Uri)null);
        }

        public QueryOperationResponse LoadProperty(object entity, string propertyName, Uri nextLinkUri)
        {
            LoadPropertyResult result = this.CreateLoadPropertyRequest(entity, propertyName, null, null, nextLinkUri, null);
            result.Execute();
            return result.LoadProperty();
        }

        public QueryOperationResponse LoadProperty(object entity, string propertyName, DataServiceQueryContinuation continuation)
        {
            LoadPropertyResult result = this.CreateLoadPropertyRequest(entity, propertyName, null, null, null, continuation);
            result.Execute();
            return result.LoadProperty();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011", Justification = "allows compiler to infer 'T'")]
        public QueryOperationResponse<T> LoadProperty<T>(object entity, string propertyName, DataServiceQueryContinuation<T> continuation)
        {
            LoadPropertyResult result = this.CreateLoadPropertyRequest(entity, propertyName, null, null, null, continuation);
            result.Execute();
            return (QueryOperationResponse<T>)result.LoadProperty();
        }

#endif
        #endregion

        #region GetReadStreamUri
        public Uri GetReadStreamUri(object entity) 
        {
            EntityDescriptor box = this.EnsureContained(entity, "entity");
            return box.GetMediaResourceUri(this.baseUriWithSlash);
        }
        #endregion

        #region GetReadStream, BeginGetReadStream, EndGetReadStream

        public IAsyncResult BeginGetReadStream(object entity, DataServiceRequestArgs args, AsyncCallback callback, object state)
        {
            GetReadStreamResult result;
            result = this.CreateGetReadStreamResult(entity, args, callback, state);
            result.Begin();
            return result;
        }

        public DataServiceStreamResponse EndGetReadStream(IAsyncResult asyncResult)
        {
            GetReadStreamResult result = BaseAsyncResult.EndExecute<GetReadStreamResult>(this, "GetReadStream", asyncResult);
            return result.End();
        }

#if !ASTORIA_LIGHT
        public DataServiceStreamResponse GetReadStream(object entity)
        {
            DataServiceRequestArgs args = new DataServiceRequestArgs();
            return this.GetReadStream(entity, args);
        }

        public DataServiceStreamResponse GetReadStream(object entity, string acceptContentType)
        {
            Util.CheckArgumentNotEmpty(acceptContentType, "acceptContentType");
            DataServiceRequestArgs args = new DataServiceRequestArgs();
            args.AcceptContentType = acceptContentType;
            return this.GetReadStream(entity, args);
        }

        public DataServiceStreamResponse GetReadStream(object entity, DataServiceRequestArgs args)
        {
            GetReadStreamResult result = this.CreateGetReadStreamResult(entity, args, null, null);
            return result.Execute();
        }

#endif
        #endregion

        #region SetSaveStream

        public void SetSaveStream(object entity, Stream stream, bool closeStream, string contentType, string slug)
        {
            Util.CheckArgumentNull(contentType, "contentType");
            Util.CheckArgumentNull(slug, "slug");

            DataServiceRequestArgs args = new DataServiceRequestArgs();
            args.ContentType = contentType;
            args.Slug = slug;
            this.SetSaveStream(entity, stream, closeStream, args);
        }

        public void SetSaveStream(object entity, Stream stream, bool closeStream, DataServiceRequestArgs args)
        {
            EntityDescriptor box = this.EnsureContained(entity, "entity");
            Util.CheckArgumentNull(stream, "stream");
            Util.CheckArgumentNull(args, "args");

            ClientType clientType = ClientType.Create(entity.GetType());
            if (clientType.MediaDataMember != null)
            { 
                throw new ArgumentException(
                    Strings.Context_SetSaveStreamOnMediaEntryProperty(clientType.ElementTypeName), 
                    "entity");
            }

            box.SaveStream = new DataServiceSaveStream(stream, closeStream, args);

            Debug.Assert(box.State != EntityStates.Detached, "We should never have a detached entity in the entityDescriptor dictionary.");
            switch (box.State)
            {
                case EntityStates.Added:
                    box.StreamState = StreamStates.Added;
                    break;

                case EntityStates.Modified:
                case EntityStates.Unchanged:
                    box.StreamState = StreamStates.Modified;
                    break;

                case EntityStates.Deleted:
                default:
                    throw new DataServiceClientException(Strings.DataServiceException_GeneralError);
            }

        }

        #endregion

        #region ExecuteBatch, BeginExecuteBatch, EndExecuteBatch

        public IAsyncResult BeginExecuteBatch(AsyncCallback callback, object state, params DataServiceRequest[] queries)
        {
            Util.CheckArgumentNotEmpty(queries, "queries");

            SaveResult result = new SaveResult(this, "ExecuteBatch", queries, SaveChangesOptions.Batch, callback, state, true);
            result.BatchBeginRequest(false);
            return result;
        }


        public DataServiceResponse EndExecuteBatch(IAsyncResult asyncResult)
        {
            SaveResult result = BaseAsyncResult.EndExecute<SaveResult>(this, "ExecuteBatch", asyncResult);
            return result.EndRequest();
        }

#if !ASTORIA_LIGHT 
        public DataServiceResponse ExecuteBatch(params DataServiceRequest[] queries)
        {
            Util.CheckArgumentNotEmpty(queries, "queries");

            SaveResult result = new SaveResult(this, "ExecuteBatch", queries, SaveChangesOptions.Batch, null, null, false);
            result.BatchRequest(false );
            return result.EndRequest();
        }
#endif

        #endregion

        #region Execute(Uri), BeginExecute(Uri), EndExecute(Uri)

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Type is used to infer result")]
        public IAsyncResult BeginExecute<TElement>(Uri requestUri, AsyncCallback callback, object state)
        {
            requestUri = Util.CreateUri(this.baseUriWithSlash, requestUri);
            QueryComponents qc = new QueryComponents(requestUri, Util.DataServiceVersionEmpty, typeof(TElement), null, null);
            return (new DataServiceRequest<TElement>(qc, null)).BeginExecute(this, this, callback, state);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Type is used to infer result")]
        public IAsyncResult BeginExecute<T>(DataServiceQueryContinuation<T> continuation, AsyncCallback callback, object state)
        {
            Util.CheckArgumentNull(continuation, "continuation");
            QueryComponents qc = continuation.CreateQueryComponents();
            return (new DataServiceRequest<T>(qc, continuation.Plan)).BeginExecute(this, this, callback, state);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Type is used to infer result")]
        public IEnumerable<TElement> EndExecute<TElement>(IAsyncResult asyncResult)
        {
            Util.CheckArgumentNull(asyncResult, "asyncResult");
            return DataServiceRequest.EndExecute<TElement>(this, this, asyncResult);
        }

#if !ASTORIA_LIGHT 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Type is used to infer result")]
        public IEnumerable<TElement> Execute<TElement>(Uri requestUri)
        {
            requestUri = Util.CreateUri(this.baseUriWithSlash, requestUri);
            QueryComponents qc = new QueryComponents(requestUri, Util.DataServiceVersionEmpty, typeof(TElement), null, null);
            DataServiceRequest request = new DataServiceRequest<TElement>(qc, null);
            return request.Execute<TElement>(this, qc);
        }

        public QueryOperationResponse<T> Execute<T>(DataServiceQueryContinuation<T> continuation)
        {
            Util.CheckArgumentNull(continuation, "continuation");
            QueryComponents qc = continuation.CreateQueryComponents();
            DataServiceRequest request = new DataServiceRequest<T>(qc, continuation.Plan);
            return request.Execute<T>(this, qc);
        }
#endif
        #endregion

        #region SaveChanges, BeginSaveChanges, EndSaveChanges


        public IAsyncResult BeginSaveChanges(AsyncCallback callback, object state)
        {
            return this.BeginSaveChanges(this.SaveChangesDefaultOptions, callback, state);
        }


        public IAsyncResult BeginSaveChanges(SaveChangesOptions options, AsyncCallback callback, object state)
        {
            ValidateSaveChangesOptions(options);
            SaveResult result = new SaveResult(this, "SaveChanges", null, options, callback, state, true);
            bool replaceOnUpdate = IsFlagSet(options, SaveChangesOptions.ReplaceOnUpdate);
            if (IsFlagSet(options, SaveChangesOptions.Batch))
            {
                result.BatchBeginRequest(replaceOnUpdate);
            }
            else
            {
                result.BeginNextChange(replaceOnUpdate); 
            }

            return result;
        }

        public DataServiceResponse EndSaveChanges(IAsyncResult asyncResult)
        {
            SaveResult result = BaseAsyncResult.EndExecute<SaveResult>(this, "SaveChanges", asyncResult);
            
            DataServiceResponse errors = result.EndRequest();

            if (this.ChangesSaved != null)
            {
                this.ChangesSaved(this, new SaveChangesEventArgs(errors));
            }

            return errors;
        }

#if !ASTORIA_LIGHT
        public DataServiceResponse SaveChanges()
        {
            return this.SaveChanges(this.SaveChangesDefaultOptions);
        }

 
        public DataServiceResponse SaveChanges(SaveChangesOptions options)
        {
            DataServiceResponse errors = null;
            ValidateSaveChangesOptions(options);

            SaveResult result = new SaveResult(this, "SaveChanges", null, options, null, null, false);
            bool replaceOnUpdate = IsFlagSet(options, SaveChangesOptions.ReplaceOnUpdate);
            if (IsFlagSet(options, SaveChangesOptions.Batch))
            {
                result.BatchRequest(replaceOnUpdate);
            }
            else
            {
                result.BeginNextChange(replaceOnUpdate);
            }

            errors = result.EndRequest();

            Debug.Assert(null != errors, "null errors");

            if (this.ChangesSaved != null)
            {
                this.ChangesSaved(this, new SaveChangesEventArgs(errors));
            }

            return errors;
        }
#endif
        #endregion

        #region Add, Attach, Delete, Detach, Update, TryGetEntity, TryGetUri


        public void AddLink(object source, string sourceProperty, object target)
        {
            this.EnsureRelatable(source, sourceProperty, target, EntityStates.Added);

            LinkDescriptor relation = new LinkDescriptor(source, sourceProperty, target);
            if (this.bindings.ContainsKey(relation))
            {
                throw Error.InvalidOperation(Strings.Context_RelationAlreadyContained);
            }

            relation.State = EntityStates.Added;
            this.bindings.Add(relation, relation);
            this.IncrementChange(relation);
        }


        public void AttachLink(object source, string sourceProperty, object target)
        {
            this.AttachLink(source, sourceProperty, target, MergeOption.NoTracking);
        }


        public bool DetachLink(object source, string sourceProperty, object target)
        {
            Util.CheckArgumentNull(source, "source");
            Util.CheckArgumentNotEmpty(sourceProperty, "sourceProperty");

            LinkDescriptor existing;
            LinkDescriptor relation = new LinkDescriptor(source, sourceProperty, target);
            if (!this.bindings.TryGetValue(relation, out existing))
            {
                return false;
            }

            this.DetachExistingLink(existing, false);
            return true;
        }


        public void DeleteLink(object source, string sourceProperty, object target)
        {
            bool delay = this.EnsureRelatable(source, sourceProperty, target, EntityStates.Deleted);

            LinkDescriptor existing = null;
            LinkDescriptor relation = new LinkDescriptor(source, sourceProperty, target);
            if (this.bindings.TryGetValue(relation, out existing) && (EntityStates.Added == existing.State))
            {   
                this.DetachExistingLink(existing, false);
            }
            else
            {
                if (delay)
                {  
                    throw Error.InvalidOperation(Strings.Context_NoRelationWithInsertEnd);
                }

                if (null == existing)
                {  
                    this.bindings.Add(relation, relation);
                    existing = relation;
                }

                if (EntityStates.Deleted != existing.State)
                {
                    existing.State = EntityStates.Deleted;


                    this.IncrementChange(existing);
                }
            }
        }


        public void SetLink(object source, string sourceProperty, object target)
        {
            this.EnsureRelatable(source, sourceProperty, target, EntityStates.Modified);

            LinkDescriptor relation = this.DetachReferenceLink(source, sourceProperty, target, MergeOption.NoTracking);
            if (null == relation)
            {
                relation = new LinkDescriptor(source, sourceProperty, target);
                this.bindings.Add(relation, relation);
            }

            Debug.Assert(
                0 == relation.State ||
                IncludeLinkState(relation.State),
                "set link entity state");

            if (EntityStates.Modified != relation.State)
            {
                relation.State = EntityStates.Modified;
                this.IncrementChange(relation);
            }
        }

        #endregion

        #region AddObject, AttachTo, DeleteObject, Detach, TryGetEntity, TryGetUri

        public void AddObject(string entitySetName, object entity)
        {
            this.ValidateEntitySetName(ref entitySetName);
            ValidateEntityType(entity);

            EntityDescriptor resource = new EntityDescriptor(null, null , null , entity, null, null, entitySetName, null, EntityStates.Added);

            try
            {
                this.entityDescriptors.Add(entity, resource);
            }
            catch (ArgumentException)
            {
                throw Error.InvalidOperation(Strings.Context_EntityAlreadyContained);
            }

            this.IncrementChange(resource);
        }


        public void AddRelatedObject(object source, string sourceProperty, object target)
        {
            Util.CheckArgumentNull(source, "source");
            Util.CheckArgumentNotEmpty(sourceProperty, "propertyName");
            Util.CheckArgumentNull(target, "target");


            ValidateEntityType(source);

            EntityDescriptor sourceResource = this.EnsureContained(source, "source");

    
            if (sourceResource.State == EntityStates.Deleted)
            {
                throw Error.InvalidOperation(Strings.Context_AddRelatedObjectSourceDeleted);
            }

            ClientType parentType = ClientType.Create(source.GetType());
            ClientType.ClientProperty property = parentType.GetProperty(sourceProperty, false);
            if (property.IsKnownType || property.CollectionType == null)
            {
                throw Error.InvalidOperation(Strings.Context_AddRelatedObjectCollectionOnly);
            }


            ClientType childType = ClientType.Create(target.GetType());
            ValidateEntityType(target);


            ClientType propertyElementType = ClientType.Create(property.CollectionType);
            if (!propertyElementType.ElementType.IsAssignableFrom(childType.ElementType))
            {

                throw Error.Argument(Strings.Context_RelationNotRefOrCollection, "target");
            }

            EntityDescriptor targetResource = new EntityDescriptor(null, null, null, target, sourceResource, sourceProperty, null , null, EntityStates.Added);

            try
            {
                this.entityDescriptors.Add(target, targetResource);
            }
            catch (ArgumentException)
            {
                throw Error.InvalidOperation(Strings.Context_EntityAlreadyContained);
            }


            LinkDescriptor end = targetResource.GetRelatedEnd();
            end.State = EntityStates.Added;
            this.bindings.Add(end, end);

            this.IncrementChange(targetResource);
        }


        public void AttachTo(string entitySetName, object entity)
        {
            this.AttachTo(entitySetName, entity, null);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", MessageId = "etag", Justification = "represents ETag in request")]
        public void AttachTo(string entitySetName, object entity, string etag)
        {
            this.ValidateEntitySetName(ref entitySetName);
            Uri editLink = GenerateEditLinkUri(this.baseUriWithSlash, entitySetName, entity);


            String identity = Util.ReferenceIdentity(editLink.ToString());

            EntityDescriptor descriptor = new EntityDescriptor(identity, null , editLink, entity, null , null , null , etag, EntityStates.Unchanged);
            this.InternalAttachEntityDescriptor(descriptor, true);
        }


        public void DeleteObject(object entity)
        {
            Util.CheckArgumentNull(entity, "entity");

            EntityDescriptor resource = null;
            if (!this.entityDescriptors.TryGetValue(entity, out resource))
            {   
                throw Error.InvalidOperation(Strings.Context_EntityNotContained);
            }

            EntityStates state = resource.State;
            if (EntityStates.Added == state)
            {  
                this.DetachResource(resource);
            }
            else if (EntityStates.Deleted != state)
            {
                Debug.Assert(
                    IncludeLinkState(state),
                    "bad state transition to deleted");


                resource.State = EntityStates.Deleted;
                this.IncrementChange(resource);
            }
        }

        public bool Detach(object entity)
        {
            Util.CheckArgumentNull(entity, "entity");

            EntityDescriptor resource = null;
            if (this.entityDescriptors.TryGetValue(entity, out resource))
            {
                return this.DetachResource(resource);
            }

            return false;
        }


        public void UpdateObject(object entity)
        {
            Util.CheckArgumentNull(entity, "entity");

            EntityDescriptor resource = null;
            if (!this.entityDescriptors.TryGetValue(entity, out resource))
            {
                throw Error.Argument(Strings.Context_EntityNotContained, "entity");
            }

            if (EntityStates.Unchanged == resource.State)
            {
                resource.State = EntityStates.Modified;
                this.IncrementChange(resource);
            }
        }

   public bool TryGetEntity<TEntity>(Uri identity, out TEntity entity) where TEntity : class
        {
            entity = null;
            Util.CheckArgumentNull(identity, "relativeUri");

            EntityStates state;


            entity = (TEntity)this.TryGetEntity(Util.ReferenceIdentity(identity.ToString()), null, MergeOption.AppendOnly, out state);
            return (null != entity);
        }


        public bool TryGetUri(object entity, out Uri identity)
        {
            identity = null;
            Util.CheckArgumentNull(entity, "entity");

            EntityDescriptor resource = null;
            if ((null != this.identityToDescriptor) &&
                this.entityDescriptors.TryGetValue(entity, out resource) &&
                (null != resource.Identity) &&
                Object.ReferenceEquals(resource, this.identityToDescriptor[resource.Identity]))
            {

                string identityUri = Util.DereferenceIdentity(resource.Identity);
                identity = Util.CreateUri(identityUri, UriKind.Absolute);
            }

            return (null != identity);
        }

        internal static Exception HandleResponse(
            HttpStatusCode statusCode,
            string responseVersion,
            Func<Stream> getResponseStream,
            bool throwOnFailure)
        {
            InvalidOperationException failure = null;
            if (!CanHandleResponseVersion(responseVersion))
            {
                string description = Strings.Context_VersionNotSupported(
                    responseVersion,
                    SerializeSupportedVersions());

                failure = Error.InvalidOperation(description);
            }

            if (failure == null && !WebUtil.SuccessStatusCode(statusCode))
            {
                failure = GetResponseText(getResponseStream, statusCode);
            }

            if (failure != null && throwOnFailure)
            {
                throw failure;
            }

            return failure;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031", Justification = "Cache exception so user can examine it later")]
        internal static DataServiceClientException GetResponseText(Func<Stream> getResponseStream, HttpStatusCode statusCode)
        {
            string message = null;
            using (System.IO.Stream stream = getResponseStream())
            {
                if ((null != stream) && stream.CanRead)
                {
                    message = new StreamReader(stream).ReadToEnd();
                }
            }

            if (String.IsNullOrEmpty(message))
            {
                message = statusCode.ToString();
            }

            return new DataServiceClientException(message, (int)statusCode);
        }

        internal void AttachIdentity(String identity, Uri selfLink, Uri editLink, object entity, string etag)
        { 
            Debug.Assert(null != identity, "must have identity");

            this.EnsureIdentityToResource();

          
            EntityDescriptor resource = this.entityDescriptors[entity];

            this.DetachResourceIdentity(resource);

           
            if (resource.IsDeepInsert)
            {
                LinkDescriptor end = this.bindings[resource.GetRelatedEnd()];
                end.State = EntityStates.Unchanged;
            }

            resource.ETag = etag;
            resource.Identity = identity; 
            resource.SelfLink = selfLink;
            resource.EditLink = editLink;

            resource.State = EntityStates.Unchanged;


            this.identityToDescriptor[identity] = resource;
        }


        internal void AttachLocation(object entity, string location)
        {
            Debug.Assert(null != entity, "null != entity");
            Uri editLink = new Uri(location, UriKind.Absolute);
            String identity = Util.ReferenceIdentity(editLink.ToString());

            this.EnsureIdentityToResource();

            EntityDescriptor resource = this.entityDescriptors[entity];
            this.DetachResourceIdentity(resource);


            if (resource.IsDeepInsert)
            {
                LinkDescriptor end = this.bindings[resource.GetRelatedEnd()];
                end.State = EntityStates.Unchanged;
            }

            resource.Identity = identity; 
            resource.EditLink = editLink;

          this.identityToDescriptor[identity] = resource;
        }


        internal void AttachLink(object source, string sourceProperty, object target, MergeOption linkMerge)
        {
            this.EnsureRelatable(source, sourceProperty, target, EntityStates.Unchanged);

            LinkDescriptor existing = null;
            LinkDescriptor relation = new LinkDescriptor(source, sourceProperty, target);
            if (this.bindings.TryGetValue(relation, out existing))
            {
                switch (linkMerge)
                {
                    case MergeOption.AppendOnly:
                        break;

                    case MergeOption.OverwriteChanges:
                        relation = existing;
                        break;

                    case MergeOption.PreserveChanges:
                        if ((EntityStates.Added == existing.State) ||
                            (EntityStates.Unchanged == existing.State) ||
                            (EntityStates.Modified == existing.State && null != existing.Target))
                        {
                            relation = existing;
                        }

                        break;

                    case MergeOption.NoTracking: 
                        throw Error.InvalidOperation(Strings.Context_RelationAlreadyContained);
                }
            }
            else
            {
                bool collectionProperty = (null != ClientType.Create(source.GetType()).GetProperty(sourceProperty, false).CollectionType);
                if (collectionProperty || (null == (existing = this.DetachReferenceLink(source, sourceProperty, target, linkMerge))))
                {
                    this.bindings.Add(relation, relation);
                    this.IncrementChange(relation);
                }
                else if (!((MergeOption.AppendOnly == linkMerge) ||
                           (MergeOption.PreserveChanges == linkMerge && EntityStates.Modified == existing.State)))
                {
         
                    relation = existing;
                }
            }

            relation.State = EntityStates.Unchanged;
        }


        internal EntityDescriptor InternalAttachEntityDescriptor(EntityDescriptor descriptor, bool failIfDuplicated)
        {
            Debug.Assert((null != descriptor.Identity), "must have identity");
            Debug.Assert(null != descriptor.Entity && ClientType.Create(descriptor.Entity.GetType()).IsEntityType, "must be entity type to attach");

            this.EnsureIdentityToResource();

            EntityDescriptor resource;
            this.entityDescriptors.TryGetValue(descriptor.Entity, out resource);

            EntityDescriptor existing;
            this.identityToDescriptor.TryGetValue(descriptor.Identity, out existing);

            if (failIfDuplicated && (null != resource))
            {
                throw Error.InvalidOperation(Strings.Context_EntityAlreadyContained);
            }
            else if (resource != existing)
            {
                throw Error.InvalidOperation(Strings.Context_DifferentEntityAlreadyContained);
            }
            else if (null == resource)
            {
                resource = descriptor;
                
            
                this.IncrementChange(descriptor);
                this.entityDescriptors.Add(descriptor.Entity, descriptor);
                this.identityToDescriptor.Add(descriptor.Identity, descriptor);
            }


            return resource;
        }

        #endregion

#if ASTORIA_LIGHT

        internal HttpWebRequest CreateRequest(Uri requestUri, string method, bool allowAnyType, string contentType, Version requestVersion, bool sendChunked)
        {
            return CreateRequest(requestUri, method, allowAnyType, contentType, requestVersion, sendChunked, HttpStack.Auto);
        }
#endif

#if !ASTORIA_LIGHT
       

        internal HttpWebRequest CreateRequest(Uri requestUri, string method, bool allowAnyType, string contentType, Version requestVersion, bool sendChunked)
#else

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "sendChunked", Justification = "common parameter not used in silverlight")]
        internal HttpWebRequest CreateRequest(Uri requestUri, string method, bool allowAnyType, string contentType, Version requestVersion, bool sendChunked, HttpStack httpStackArg)
#endif
        {
            Debug.Assert(null != requestUri, "request uri is null");
            Debug.Assert(requestUri.IsAbsoluteUri, "request uri is not absolute uri");
            Debug.Assert(
                requestUri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) ||
                    requestUri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase), 
                "request uri is not for HTTP");

            Debug.Assert(
                Object.ReferenceEquals(XmlConstants.HttpMethodDelete, method) ||
                Object.ReferenceEquals(XmlConstants.HttpMethodGet, method) ||
                Object.ReferenceEquals(XmlConstants.HttpMethodPost, method) ||
                Object.ReferenceEquals(XmlConstants.HttpMethodPut, method) ||
                Object.ReferenceEquals(XmlConstants.HttpMethodMerge, method),
                "unexpected http method string reference");

#if !ASTORIA_LIGHT
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
#else
            if (httpStackArg == HttpStack.Auto)
            {
                httpStackArg = this.httpStack;
            }
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri, httpStackArg);
#endif

#if !ASTORIA_LIGHT 
            if (null != this.Credentials)
            {
                request.Credentials = this.Credentials;
            }
#endif

#if !ASTORIA_LIGHT 
            if (0 != this.timeout)
            {
                request.Timeout = (int)Math.Min(Int32.MaxValue, new TimeSpan(0, 0, this.timeout).TotalMilliseconds);
            }
#endif

#if !ASTORIA_LIGHT 
            request.KeepAlive = true;
#endif

#if !ASTORIA_LIGHT 
            request.UserAgent = "Microsoft ADO.NET Data Services";
#endif

            if (this.UsePostTunneling &&
                (!Object.ReferenceEquals(XmlConstants.HttpMethodPost, method)) &&
                (!Object.ReferenceEquals(XmlConstants.HttpMethodGet, method)))
            {
                request.Headers[XmlConstants.HttpXMethod] = method;
                request.Method = XmlConstants.HttpMethodPost;
            }
            else
            {
                request.Method = method;
            }

            if (requestVersion != null && requestVersion.Major > 0)
            {
         
                request.Headers[XmlConstants.HttpDataServiceVersion] = requestVersion.ToString() + Util.VersionSuffix;
            }

            request.Headers[XmlConstants.HttpMaxDataServiceVersion] = Util.MaxResponseVersion.ToString() + Util.VersionSuffix;

#if !ASTORIA_LIGHT 
            if (sendChunked)
            {
                request.SendChunked = true;
            }
#endif

            if (this.SendingRequest != null)
            {
                System.Net.WebHeaderCollection requestHeaders;
#if !ASTORIA_LIGHT
                requestHeaders = request.Headers;
                SendingRequestEventArgs args = new SendingRequestEventArgs(request, requestHeaders);
#else
                requestHeaders = request.CreateEmptyWebHeaderCollection();
                SendingRequestEventArgs args = new SendingRequestEventArgs(null, requestHeaders);
#endif
                this.SendingRequest(this, args);

#if !ASTORIA_LIGHT
                if (!Object.ReferenceEquals(args.Request, request))
                {
                    request = (System.Net.HttpWebRequest)args.Request;
                }
#else
              
                foreach (string key in requestHeaders.AllKeys)
                {
                    request.Headers[key] = requestHeaders[key];
                }
#endif
            }

            request.Accept = allowAnyType ?
                    XmlConstants.MimeAny :
                    (XmlConstants.MimeApplicationAtom + "," + XmlConstants.MimeApplicationXml);

            request.Headers[HttpRequestHeader.AcceptCharset] = XmlConstants.Utf8Encoding;

#if !ASTORIA_LIGHT 
            bool allowStreamBuffering = false;
            bool removeXMethod = true;
#endif

            if (!Object.ReferenceEquals(XmlConstants.HttpMethodGet, method))
            {
                Debug.Assert(!String.IsNullOrEmpty(contentType), "Content-Type must be specified for non get operation");
                request.ContentType = contentType;
                if (Object.ReferenceEquals(XmlConstants.HttpMethodDelete, method))
                {
                    request.ContentLength = 0;
                }
#if !ASTORIA_LIGHT 
                // else
                {   
                    allowStreamBuffering = true;
                }
#endif

                if (this.UsePostTunneling && (!Object.ReferenceEquals(XmlConstants.HttpMethodPost, method)))
                {
                    request.Headers[XmlConstants.HttpXMethod] = method;
                    method = XmlConstants.HttpMethodPost;
#if !ASTORIA_LIGHT
                    removeXMethod = false;
#endif
                }
            }
            else
            {
                Debug.Assert(contentType == null, "Content-Type for get methods should be null");
            }

#if !ASTORIA_LIGHT 
            request.AllowWriteStreamBuffering = allowStreamBuffering;
#endif

            ICollection<string> headers;
            headers = request.Headers.AllKeys;

#if !ASTORIA_LIGHT  
            if (headers.Contains(XmlConstants.HttpRequestIfMatch))
            {
                request.Headers.Remove(HttpRequestHeader.IfMatch);
            }
#endif

#if !ASTORIA_LIGHT  
            if (removeXMethod && headers.Contains(XmlConstants.HttpXMethod))
            {
                request.Headers.Remove(XmlConstants.HttpXMethod);
            }
#endif

            request.Method = method;
            return request;
        }


        internal object TryGetEntity(String resourceUri, string etag, MergeOption merger, out EntityStates state)
        {
            Debug.Assert(null != resourceUri, "null uri");
            state = EntityStates.Detached;

            EntityDescriptor resource = null;
            if ((null != this.identityToDescriptor) &&
                 this.identityToDescriptor.TryGetValue(resourceUri, out resource))
            {
                state = resource.State;
                if ((null != etag) && (MergeOption.AppendOnly != merger))
                {  
                    resource.ETag = etag;
                }

                Debug.Assert(null != resource.Entity, "null entity");
                return resource.Entity;
            }

            return null;
        }


        internal IEnumerable<LinkDescriptor> GetLinks(object source, string sourceProperty)
        {
            return this.bindings.Values.Where(o => (o.Source == source) && (o.SourceProperty == sourceProperty));
        }


        internal Type ResolveTypeFromName(string wireName, Type userType, bool checkAssignable)
        {
            Debug.Assert(null != userType, "null != baseType");

            if (String.IsNullOrEmpty(wireName))
            {
                return userType;
            }

            Type payloadType;
            if (!ClientConvert.ToNamedType(wireName, out payloadType))
            {
                payloadType = null;

                Func<string, Type> resolve = this.ResolveType;
                if (null != resolve)
                {
             
                    payloadType = resolve(wireName);
                }

                if (null == payloadType)
                {
                 
#if !ASTORIA_LIGHT
                    payloadType = ClientType.ResolveFromName(wireName, userType);
#else
                    payloadType = ClientType.ResolveFromName(wireName, userType, this.GetType());
#endif
                }

                if (checkAssignable && (null != payloadType) && (!userType.IsAssignableFrom(payloadType)))
                {
          
                    throw Error.InvalidOperation(Strings.Deserialize_Current(userType, payloadType));
                }
            }

            return payloadType ?? userType;
        }


        internal string ResolveNameFromType(Type type)
        {
            Debug.Assert(null != type, "null type");
            Func<Type, string> resolve = this.ResolveName;
            return ((null != resolve) ? resolve(type) : (String)null);
        }


        internal string GetServerTypeName(EntityDescriptor descriptor)
        {
            Debug.Assert(descriptor != null && descriptor.Entity != null, "Null descriptor or no entity in descriptor");

            if (this.resolveName != null)
            {
               
                Type entityType = descriptor.Entity.GetType();
                var codegenAttr = this.resolveName.Method.GetCustomAttributes(false).OfType<System.CodeDom.Compiler.GeneratedCodeAttribute>().FirstOrDefault();
                if (codegenAttr == null || codegenAttr.Tool != Util.CodeGeneratorToolName)
                {
                   
                    return this.resolveName(entityType) ?? descriptor.ServerTypeName;
                }
                else
                {
                    return descriptor.ServerTypeName ?? this.resolveName(entityType);
                }
            }
            else
            {
                return descriptor.ServerTypeName;
            }
        }


        internal void FireReadingEntityEvent(object entity, XElement data)
        {
            Debug.Assert(entity != null, "entity != null");
            Debug.Assert(data != null, "data != null");

            ReadingWritingEntityEventArgs args = new ReadingWritingEntityEventArgs(entity, data);
            this.ReadingEntity(this, args);
        }

        #region Ensure

  
        private static bool IncludeLinkState(EntityStates x)
        {
            return ((EntityStates.Modified == x) || (EntityStates.Unchanged == x));
        }

        #endregion

        private static bool CanHandleResponseVersion(string responseVersion)
        {
            if (!String.IsNullOrEmpty(responseVersion))
            {
                KeyValuePair<Version, string> version;
                if (!HttpProcessUtility.TryReadVersion(responseVersion, out version))
                {
                    return false;
                }

                if (!Util.SupportedResponseVersions.Contains(version.Key))
                {
                    return false;
                }
            }

            return true;
        }


        private static string SerializeSupportedVersions()
        {
            Debug.Assert(Util.SupportedResponseVersions.Length > 0, "At least one supported version must exist.");

            StringBuilder supportedVersions = new StringBuilder("'").Append(Util.SupportedResponseVersions[0].ToString());
            for (int versionIdx = 1; versionIdx < Util.SupportedResponseVersions.Length; versionIdx++)
            {
                supportedVersions.Append("', '");
                supportedVersions.Append(Util.SupportedResponseVersions[versionIdx].ToString());
            }

            supportedVersions.Append("'");

            return supportedVersions.ToString();
        }

        private static Uri GenerateEditLinkUri(Uri baseUriWithSlash, string entitySetName, object entity)
        {
            Debug.Assert(null != baseUriWithSlash && baseUriWithSlash.IsAbsoluteUri && baseUriWithSlash.OriginalString.EndsWith("/", StringComparison.Ordinal), "baseUriWithSlash");
            Debug.Assert(!String.IsNullOrEmpty(entitySetName) && !entitySetName.StartsWith("/", StringComparison.Ordinal), "entitySetName");
     
            ValidateEntityTypeHasKeys(entity);

            StringBuilder builder = new StringBuilder();
            builder.Append(baseUriWithSlash.AbsoluteUri);
            builder.Append(entitySetName);
            builder.Append("(");

            string prefix = String.Empty;
            ClientType clientType = ClientType.Create(entity.GetType());

            ClientType.ClientProperty[] keys = clientType.Properties.Where<ClientType.ClientProperty>(ClientType.ClientProperty.GetKeyProperty).ToArray();
            foreach (ClientType.ClientProperty property in keys)
            {
#if ASTORIA_OPEN_OBJECT
                Debug.Assert(!property.OpenObjectProperty, "key property values can't be OpenProperties");
#endif

                builder.Append(prefix);
                if (1 < keys.Length)
                {
                    builder.Append(property.PropertyName).Append("=");
                }

                object value = property.GetValue(entity);
                if (null == value)
                {
                    throw Error.InvalidOperation(Strings.Serializer_NullKeysAreNotSupported(property.PropertyName));
                }

                string converted;
                if (!ClientConvert.TryKeyPrimitiveToString(value, out converted))
                {
                    throw Error.InvalidOperation(Strings.Context_CannotConvertKey(value));
                }

                builder.Append(System.Uri.EscapeDataString(converted));
                prefix = ",";
            }

            builder.Append(")");

            return Util.CreateUri(builder.ToString(), UriKind.Absolute);
        }

 
        private static string GetEntityHttpMethod(EntityStates state, bool replaceOnUpdate)
        {
            switch (state)
            {
                case EntityStates.Deleted:
                    return XmlConstants.HttpMethodDelete;
                case EntityStates.Modified:
                    if (replaceOnUpdate)
                    {
                        return XmlConstants.HttpMethodPut;
                    }
                    else
                    {
                        return XmlConstants.HttpMethodMerge;
                    }

                case EntityStates.Added:
                    return XmlConstants.HttpMethodPost;
                default:
                    throw Error.InternalError(InternalError.UnvalidatedEntityState);
            }
        }

    
        private static string GetLinkHttpMethod(LinkDescriptor link)
        {
            bool collection = (null != ClientType.Create(link.Source.GetType()).GetProperty(link.SourceProperty, false).CollectionType);
            if (!collection)
            {
                Debug.Assert(EntityStates.Modified == link.State, "not Modified state");
                if (null == link.Target)
                {   
                    return XmlConstants.HttpMethodDelete;
                }
                else
                {   
                    return XmlConstants.HttpMethodPut;
                }
            }
            else if (EntityStates.Deleted == link.State)
            {   
                return XmlConstants.HttpMethodDelete;
            }
            else
            {   
                Debug.Assert(EntityStates.Added == link.State, "not Added state");
                return XmlConstants.HttpMethodPost;
            }
        }


        private static void HandleResponsePost(LinkDescriptor entry)
        {
            if (!((EntityStates.Added == entry.State) || (EntityStates.Modified == entry.State && null != entry.Target)))
            {
                Error.ThrowBatchUnexpectedContent(InternalError.LinkNotAddedState);
            }

            entry.State = EntityStates.Unchanged;
        }


        private static void HandleResponsePut(Descriptor entry, string etag)
        {
            if (entry.IsResource)
            {
                EntityDescriptor descriptor = (EntityDescriptor)entry;
                if (EntityStates.Modified != descriptor.State && StreamStates.Modified != descriptor.StreamState)
                {
                    Error.ThrowBatchUnexpectedContent(InternalError.EntryNotModified);
                }

                if (descriptor.StreamState == StreamStates.Modified)
                {
                    descriptor.StreamETag = etag;
                    descriptor.StreamState = StreamStates.NoStream;
                }
                else
                {
                    Debug.Assert(descriptor.State == EntityStates.Modified, "descriptor.State == EntityStates.Modified");
                    descriptor.ETag = etag;
                    descriptor.State = EntityStates.Unchanged;
                }
            }
            else
            {
                LinkDescriptor link = (LinkDescriptor)entry;
                if ((EntityStates.Added == entry.State) || (EntityStates.Modified == entry.State))
                {
                    link.State = EntityStates.Unchanged;
                }
                else if (EntityStates.Detached != entry.State)
                {   
                    Error.ThrowBatchUnexpectedContent(InternalError.LinkBadState);
                }
            }
        }

        private static void WriteContentProperty(XmlWriter writer, string namespaceName, ClientType.ClientProperty property, object propertyValue)
        {
            writer.WriteStartElement(property.PropertyName, namespaceName);

            string typename = ClientConvert.GetEdmType(property.PropertyType);
            if (null != typename)
            {
                writer.WriteAttributeString(XmlConstants.AtomTypeAttributeName, XmlConstants.DataWebMetadataNamespace, typename);
            }

            if (null == propertyValue)
            {   
                writer.WriteAttributeString(XmlConstants.AtomNullAttributeName, XmlConstants.DataWebMetadataNamespace, XmlConstants.XmlTrueLiteral);

                if (property.KeyProperty)
                {
                    throw Error.InvalidOperation(Strings.Serializer_NullKeysAreNotSupported(property.PropertyName));
                }
            }
            else
            {
                string convertedValue = ClientConvert.ToString(propertyValue, false );
                if (0 == convertedValue.Length)
                {  
                    writer.WriteAttributeString(XmlConstants.AtomNullAttributeName, XmlConstants.DataWebMetadataNamespace, XmlConstants.XmlFalseLiteral);
                }
                else
                {   
                    if (Char.IsWhiteSpace(convertedValue[0]) ||
                        Char.IsWhiteSpace(convertedValue[convertedValue.Length - 1]))
                    {  
                        writer.WriteAttributeString(XmlConstants.XmlSpaceAttributeName, XmlConstants.XmlNamespacesNamespace, XmlConstants.XmlSpacePreserveValue);
                    }

                    writer.WriteValue(convertedValue);
                }
            }

            writer.WriteEndElement();
        }


        private static void ValidateEntityType(object entity)
        {
            Util.CheckArgumentNull(entity, "entity");

            if (!ClientType.Create(entity.GetType()).IsEntityType)
            {
                throw Error.Argument(Strings.Content_EntityIsNotEntityType, "entity");
            }
        }


        private static void ValidateEntityTypeHasKeys(object entity)
        {
            Util.CheckArgumentNull(entity, "entity");

            if (ClientType.Create(entity.GetType()).KeyCount <= 0)
            {
                throw Error.Argument(Strings.Content_EntityWithoutKey, "entity");
            }
        }

 
        private static void ValidateSaveChangesOptions(SaveChangesOptions options)
        {
            const SaveChangesOptions All =
                SaveChangesOptions.ContinueOnError |
                SaveChangesOptions.Batch |
                SaveChangesOptions.ReplaceOnUpdate;

            
            if ((options | All) != All)
            {
                throw Error.ArgumentOutOfRange("options");
            }

    
            if (IsFlagSet(options, SaveChangesOptions.Batch | SaveChangesOptions.ContinueOnError))
            {
                throw Error.ArgumentOutOfRange("options");
            }
        }

 
        private static bool IsFlagSet(SaveChangesOptions options, SaveChangesOptions flag)
        {
            return ((options & flag) == flag);
        }

        private static void WriteOperationRequestHeaders(StreamWriter writer, string methodName, string uri, Version requestVersion)
        {
            writer.WriteLine("{0}: {1}", XmlConstants.HttpContentType, XmlConstants.MimeApplicationHttp);
            writer.WriteLine("{0}: {1}", XmlConstants.HttpContentTransferEncoding, XmlConstants.BatchRequestContentTransferEncoding);
            writer.WriteLine();

            writer.WriteLine("{0} {1} {2}", methodName, uri, XmlConstants.HttpVersionInBatching);
            if (requestVersion != Util.DataServiceVersion1 && requestVersion != Util.DataServiceVersionEmpty)
            {
                writer.WriteLine("{0}: {1}{2}", XmlConstants.HttpDataServiceVersion, requestVersion, Util.VersionSuffix);
            }
        }

        private static void WriteOperationResponseHeaders(StreamWriter writer, int statusCode)
        {
            writer.WriteLine("{0}: {1}", XmlConstants.HttpContentType, XmlConstants.MimeApplicationHttp);
            writer.WriteLine("{0}: {1}", XmlConstants.HttpContentTransferEncoding, XmlConstants.BatchRequestContentTransferEncoding);
            writer.WriteLine();

            writer.WriteLine("{0} {1} {2}", XmlConstants.HttpVersionInBatching, statusCode, (HttpStatusCode)statusCode);
        }


        private bool DetachResource(EntityDescriptor resource)
        {
          
            foreach (LinkDescriptor end in this.bindings.Values.Where(resource.IsRelatedEntity).ToList())
            {
                this.DetachExistingLink(
                        end, 
                        end.Target == resource.Entity && resource.State == EntityStates.Added);
            }

            resource.ChangeOrder = UInt32.MaxValue;
            resource.State = EntityStates.Detached;
            bool flag = this.entityDescriptors.Remove(resource.Entity);
            Debug.Assert(flag, "should have removed existing entity");
            this.DetachResourceIdentity(resource);

            return true;
        }


        private void DetachResourceIdentity(EntityDescriptor resource)
        {
            EntityDescriptor existing = null;
            if ((null != resource.Identity) &&
                this.identityToDescriptor.TryGetValue(resource.Identity, out existing) &&
                Object.ReferenceEquals(existing, resource))
            {
                bool removed = this.identityToDescriptor.Remove(resource.Identity);
                Debug.Assert(removed, "should have removed existing identity");
            }
        }


        private HttpWebRequest CreateRequest(LinkDescriptor binding)
        {
            Debug.Assert(null != binding, "null binding");
            if (binding.ContentGeneratedForSave)
            {
                return null;
            }

            EntityDescriptor sourceResource = this.entityDescriptors[binding.Source];
            EntityDescriptor targetResource = (null != binding.Target) ? this.entityDescriptors[binding.Target] : null;

      
            if (null == sourceResource.Identity)
            {
                Debug.Assert(!binding.ContentGeneratedForSave, "already saved link");
                binding.ContentGeneratedForSave = true;
                Debug.Assert(EntityStates.Added == sourceResource.State, "expected added state");
                throw Error.InvalidOperation(Strings.Context_LinkResourceInsertFailure, sourceResource.SaveError);
            }
            else if ((null != targetResource) && (null == targetResource.Identity))
            {
                Debug.Assert(!binding.ContentGeneratedForSave, "already saved link");
                binding.ContentGeneratedForSave = true;
                Debug.Assert(EntityStates.Added == targetResource.State, "expected added state");
                throw Error.InvalidOperation(Strings.Context_LinkResourceInsertFailure, targetResource.SaveError);
            }

            Debug.Assert(null != sourceResource.Identity, "missing sourceResource.Identity");
            return this.CreateRequest(this.CreateRequestUri(sourceResource, binding), GetLinkHttpMethod(binding), false, XmlConstants.MimeApplicationXml, Util.DataServiceVersion1, false);
        }

        private Uri CreateRequestUri(EntityDescriptor sourceResource, LinkDescriptor binding)
        {
            Uri requestUri = Util.CreateUri(sourceResource.GetResourceUri(this.baseUriWithSlash, false ), this.CreateRequestRelativeUri(binding));
            return requestUri;
        }

        private Uri CreateRequestRelativeUri(LinkDescriptor binding)
        {
            Uri relative;
            bool collection = (null != ClientType.Create(binding.Source.GetType()).GetProperty(binding.SourceProperty, false).CollectionType);
            if (collection && (EntityStates.Added != binding.State))
            {  
                Debug.Assert(null != binding.Target, "null target in collection");
                EntityDescriptor targetResource = this.entityDescriptors[binding.Target];

               Uri navigationPropertyUri = this.BaseUriWithSlash.MakeRelativeUri(DataServiceContext.GenerateEditLinkUri(this.BaseUriWithSlash, binding.SourceProperty, targetResource.Entity));

                
                relative = Util.CreateUri(XmlConstants.UriLinkSegment + "/" + navigationPropertyUri.OriginalString, UriKind.Relative);
            }
            else
            {   
                relative = Util.CreateUri(XmlConstants.UriLinkSegment + "/" + binding.SourceProperty, UriKind.Relative);
            }

            Debug.Assert(!relative.IsAbsoluteUri, "should be relative uri");
            return relative;
        }


        private void CreateRequestBatch(LinkDescriptor binding, StreamWriter text)
        {
            EntityDescriptor sourceResource = this.entityDescriptors[binding.Source];
            string requestString;
            if (null != sourceResource.Identity)
            {
                requestString = this.CreateRequestUri(sourceResource, binding).AbsoluteUri;
            }
            else
            {
                Uri relative = this.CreateRequestRelativeUri(binding);
                requestString = "$" + sourceResource.ChangeOrder.ToString(CultureInfo.InvariantCulture) + "/" + relative.OriginalString;
            }

            WriteOperationRequestHeaders(text, GetLinkHttpMethod(binding), requestString, Util.DataServiceVersion1);
            text.WriteLine("{0}: {1}", XmlConstants.HttpContentID, binding.ChangeOrder);

       
            if ((EntityStates.Added == binding.State) || (EntityStates.Modified == binding.State && (null != binding.Target)))
            {
                text.WriteLine("{0}: {1}", XmlConstants.HttpContentType, XmlConstants.MimeApplicationXml);
            }
        }


        private MemoryStream CreateRequestData(LinkDescriptor binding, bool newline)
        {
            Debug.Assert(
                (binding.State == EntityStates.Added) ||
                (binding.State == EntityStates.Modified && null != binding.Target),
                "This method must be called only when a binding is added or put");
            MemoryStream stream = new MemoryStream();
            XmlWriter writer = XmlUtil.CreateXmlWriterAndWriteProcessingInstruction(stream, HttpProcessUtility.EncodingUtf8NoPreamble);
            EntityDescriptor targetResource = this.entityDescriptors[binding.Target];

            #region <uri xmlns="metadata">
            writer.WriteStartElement(XmlConstants.UriElementName, XmlConstants.DataWebMetadataNamespace);

            string id;
            if (null != targetResource.Identity)
            {
 
                id = targetResource.GetResourceUri(this.baseUriWithSlash, false ).OriginalString;
            }
            else
            {
                id = "$" + targetResource.ChangeOrder.ToString(CultureInfo.InvariantCulture);
            }

            writer.WriteValue(id);
            writer.WriteEndElement();
            #endregion

            writer.Flush();

            if (newline)
            {
             
                for (int kk = 0; kk < NewLine.Length; ++kk)
                {
                    stream.WriteByte((byte)NewLine[kk]);
                }
            }


            stream.Position = 0;
            return stream;
        }


        private HttpWebRequest CreateRequest(EntityDescriptor box, EntityStates state, bool replaceOnUpdate)
        {
            Debug.Assert(null != box && ((EntityStates.Added == state) || (EntityStates.Modified == state) || (EntityStates.Deleted == state)), "unexpected entity ResourceState");

            string httpMethod = GetEntityHttpMethod(state, replaceOnUpdate);
            Uri requestUri = box.GetResourceUri(this.baseUriWithSlash, false );

            Version requestVersion = ClientType.Create(box.Entity.GetType()).EpmIsV1Compatible ? Util.DataServiceVersion1 : Util.DataServiceVersion2;
            HttpWebRequest request = this.CreateRequest(requestUri, httpMethod, false, XmlConstants.MimeApplicationAtom, requestVersion, false);
            if ((null != box.ETag) && ((EntityStates.Deleted == state) || (EntityStates.Modified == state)))
            {
                request.Headers.Set(HttpRequestHeader.IfMatch, box.ETag);
            }

            return request;
        }

        private void CreateRequestBatch(EntityDescriptor box, StreamWriter text, bool replaceOnUpdate)
        {
            Debug.Assert(null != box, "null box");
            Debug.Assert(null != text, "null text");
            Debug.Assert(box.State == EntityStates.Added || box.State == EntityStates.Deleted || box.State == EntityStates.Modified, "the entity must be in one of the 3 possible states");

            Uri requestUri = box.GetResourceUri(this.baseUriWithSlash, false);

            Debug.Assert(null != requestUri, "request uri is null");
            Debug.Assert(requestUri.IsAbsoluteUri, "request uri is not absolute uri");

            Version requestVersion = ClientType.Create(box.Entity.GetType()).EpmIsV1Compatible ? Util.DataServiceVersion1 : Util.DataServiceVersion2;
            WriteOperationRequestHeaders(text, GetEntityHttpMethod(box.State, replaceOnUpdate), requestUri.AbsoluteUri, requestVersion);
            text.WriteLine("{0}: {1}", XmlConstants.HttpContentID, box.ChangeOrder);
            if (EntityStates.Deleted != box.State)
            {
                text.WriteLine("{0}: {1}", XmlConstants.HttpContentType, XmlConstants.LinkMimeTypeEntry);
            }

            if ((null != box.ETag) && (EntityStates.Deleted == box.State || EntityStates.Modified == box.State))
            {
                text.WriteLine("{0}: {1}", XmlConstants.HttpRequestIfMatch, box.ETag);
            }
        }


        private Stream CreateRequestData(EntityDescriptor box, bool newline)
        {
            Debug.Assert(null != box, "null box");
            MemoryStream stream = null;
            switch (box.State)
            {
                case EntityStates.Deleted:
                    break;
                case EntityStates.Modified:
                case EntityStates.Added:
                    stream = new MemoryStream();
                    break;
                default:
                    Error.ThrowInternalError(InternalError.UnvalidatedEntityState);
                    break;
            }

            if (null != stream)
            {
                XmlWriter writer;
                XDocument node = null;
                if (this.WritingEntity != null)
                {
                
                    node = new XDocument();
                    writer = node.CreateWriter();
                }
                else
                {
                    writer = XmlUtil.CreateXmlWriterAndWriteProcessingInstruction(stream, HttpProcessUtility.EncodingUtf8NoPreamble);
                }

                ClientType type = ClientType.Create(box.Entity.GetType());

                string typeName = this.GetServerTypeName(box);

                #region <entry xmlns="Atom" xmlns:d="DataWeb", xmlns:m="DataWebMetadata">
                writer.WriteStartElement(XmlConstants.AtomEntryElementName, XmlConstants.AtomNamespace);
                writer.WriteAttributeString(XmlConstants.DataWebNamespacePrefix, XmlConstants.XmlNamespacesNamespace, this.DataNamespace);
                writer.WriteAttributeString(XmlConstants.DataWebMetadataNamespacePrefix, XmlConstants.XmlNamespacesNamespace, XmlConstants.DataWebMetadataNamespace);

 
                if (!String.IsNullOrEmpty(typeName))
                {
                    writer.WriteStartElement(XmlConstants.AtomCategoryElementName, XmlConstants.AtomNamespace);
                    writer.WriteAttributeString(XmlConstants.AtomCategorySchemeAttributeName, this.typeScheme.OriginalString);
                    writer.WriteAttributeString(XmlConstants.AtomCategoryTermAttributeName, typeName);
                    writer.WriteEndElement();
                }

                if (type.HasEntityPropertyMappings)
                {
                    using (EpmSyndicationContentSerializer s = new EpmSyndicationContentSerializer(type.EpmTargetTree, box.Entity, writer))
                    {
                        s.Serialize();
                    }
                }
                else
                {
                    writer.WriteElementString(XmlConstants.AtomTitleElementName, XmlConstants.AtomNamespace, String.Empty);
                    writer.WriteStartElement(XmlConstants.AtomAuthorElementName, XmlConstants.AtomNamespace);
                    writer.WriteElementString(XmlConstants.AtomNameElementName, XmlConstants.AtomNamespace, String.Empty);
                    writer.WriteEndElement();

                    writer.WriteElementString(XmlConstants.AtomUpdatedElementName, XmlConstants.AtomNamespace, XmlConvert.ToString(DateTime.UtcNow, XmlDateTimeSerializationMode.RoundtripKind));
                }

                if (EntityStates.Modified == box.State)
                {
    
                    writer.WriteElementString(XmlConstants.AtomIdElementName, Util.DereferenceIdentity(box.Identity));
                }
                else
                {
                    writer.WriteElementString(XmlConstants.AtomIdElementName, XmlConstants.AtomNamespace, String.Empty);
                }

                #region <link href=”%EditLink%” rel=”%DataWebRelatedNamespace%%AssociationName%” type=”application/atom+xml;feed” />
                if (EntityStates.Added == box.State)
                {
                    this.CreateRequestDataLinks(box, writer);
                }
                #endregion

                #region <content type="application/xml"><m:Properites> or <m:Properties>
    
                if (!type.IsMediaLinkEntry && !box.IsMediaLinkEntry)
                {
                    writer.WriteStartElement(XmlConstants.AtomContentElementName, XmlConstants.AtomNamespace); 
                    writer.WriteAttributeString(XmlConstants.AtomTypeAttributeName, XmlConstants.MimeApplicationXml); 
                }

                writer.WriteStartElement(XmlConstants.AtomPropertiesElementName, XmlConstants.DataWebMetadataNamespace); 
                bool propertiesWritten;
                this.WriteContentProperties(writer, type, box.Entity, type.HasEntityPropertyMappings ? type.EpmSourceTree.Root : null, out propertiesWritten);

                writer.WriteEndElement(); 

                if (!type.IsMediaLinkEntry && !box.IsMediaLinkEntry)
                {
                    writer.WriteEndElement(); 
                }

                if (type.HasEntityPropertyMappings)
                {
                    using (EpmCustomContentSerializer s = new EpmCustomContentSerializer(type.EpmTargetTree, box.Entity, writer))
                    {
                        s.Serialize();
                    }
                }

                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
                #endregion
                #endregion

                if (this.WritingEntity != null)
                {
                    ReadingWritingEntityEventArgs args = new ReadingWritingEntityEventArgs(box.Entity, node.Root);
                    this.WritingEntity(this, args);

                   
                  
                    XmlWriterSettings settings = XmlUtil.CreateXmlWriterSettings(HttpProcessUtility.EncodingUtf8NoPreamble);
                    settings.ConformanceLevel = ConformanceLevel.Auto;
                    using (XmlWriter streamWriter = XmlWriter.Create(stream, settings))
                    {
                        node.Save(streamWriter);
                    }
                }

                if (newline)
                {

                    for (int kk = 0; kk < NewLine.Length; ++kk)
                    {
                        stream.WriteByte((byte)NewLine[kk]);
                    }
                }

                stream.Position = 0;
            }

            return stream;
        }


        private void CreateRequestDataLinks(EntityDescriptor box, XmlWriter writer)
        {
            Debug.Assert(EntityStates.Added == box.State, "entity not added state");

            ClientType clientType = null;
            foreach (LinkDescriptor end in this.RelatedLinks(box))
            {
                Debug.Assert(!end.ContentGeneratedForSave, "already saved link");
                end.ContentGeneratedForSave = true;

                if (null == clientType)
                {
                    clientType = ClientType.Create(box.Entity.GetType());
                }

                string typeAttributeValue;
                if (null != clientType.GetProperty(end.SourceProperty, false).CollectionType)
                {
                    typeAttributeValue = XmlConstants.LinkMimeTypeFeed;
                }
                else
                {
                    typeAttributeValue = XmlConstants.LinkMimeTypeEntry;
                }

                Debug.Assert(null != end.Target, "null is DELETE");
                String targetEditLink = this.entityDescriptors[end.Target].EditLink.ToString();

                writer.WriteStartElement(XmlConstants.AtomLinkElementName, XmlConstants.AtomNamespace);
                writer.WriteAttributeString(XmlConstants.AtomHRefAttributeName, targetEditLink);
                writer.WriteAttributeString(XmlConstants.AtomLinkRelationAttributeName, XmlConstants.DataWebRelatedNamespace + end.SourceProperty);
                writer.WriteAttributeString(XmlConstants.AtomTypeAttributeName, typeAttributeValue);
                writer.WriteEndElement();
            }
        }


        private void HandleResponseDelete(Descriptor entry)
        {
            if (EntityStates.Deleted != entry.State)
            {
                Error.ThrowBatchUnexpectedContent(InternalError.EntityNotDeleted);
            }

            if (entry.IsResource)
            {
                EntityDescriptor resource = (EntityDescriptor)entry;
                this.DetachResource(resource);
            }
            else
            {
                this.DetachExistingLink((LinkDescriptor)entry, false);
            }
        }

        private void HandleResponsePost(EntityDescriptor entry, MaterializeAtom materializer, Uri editLink, string etag)
        {
            Debug.Assert(editLink != null, "location header must be specified in POST responses.");

            if (EntityStates.Added != entry.State && StreamStates.Added != entry.StreamState)
            {
                Error.ThrowBatchUnexpectedContent(InternalError.EntityNotAddedState);
            }

            if (materializer == null)
            {
               
                String identity = Util.ReferenceIdentity(editLink.ToString());
                this.AttachIdentity(identity, null , editLink, entry.Entity, etag);
            }
            else
            {
                materializer.SetInsertingObject(entry.Entity);

                foreach (object x in materializer)
                {
                    Debug.Assert(null != entry.Identity, "updated inserted should always gain an identity");
                    Debug.Assert(x == entry.Entity, "x == box.Entity, should have same object generated by response");
                    Debug.Assert(EntityStates.Unchanged == entry.State, "should have moved out of insert");
                    Debug.Assert((null != this.identityToDescriptor) && this.identityToDescriptor.ContainsKey(entry.Identity), "should have identity tracked");

              
                    if (entry.EditLink == null)
                    {
                        entry.EditLink = editLink;
                    }

                    if (entry.ETag == null)
                    {
                        entry.ETag = etag;
                    }
                }
            }

            foreach (LinkDescriptor end in this.RelatedLinks(entry))
            {
                Debug.Assert(0 != end.SaveResultWasProcessed, "link should have been saved with the enty");

        
                if (IncludeLinkState(end.SaveResultWasProcessed) || end.SaveResultWasProcessed == EntityStates.Added)
                {
                    HandleResponsePost(end);
                }
            }
        }


        private int SaveResultProcessed(Descriptor entry)
        {
      
            entry.SaveResultWasProcessed = entry.State;

            int count = 0;
            if (entry.IsResource && (EntityStates.Added == entry.State))
            {
                foreach (LinkDescriptor end in this.RelatedLinks((EntityDescriptor)entry))
                {
                    Debug.Assert(end.ContentGeneratedForSave, "link should have been saved with the enty");
                    if (end.ContentGeneratedForSave)
                    {
                        Debug.Assert(0 == end.SaveResultWasProcessed, "this link already had a result");
                        end.SaveResultWasProcessed = end.State;
                        count++;
                    }
                }
            }

            return count;
        }


        private IEnumerable<LinkDescriptor> RelatedLinks(EntityDescriptor box)
        {
            foreach (LinkDescriptor end in this.bindings.Values)
            {
                if (end.Source == box.Entity)
                {
                    if (null != end.Target)
                    {   
                        EntityDescriptor target = this.entityDescriptors[end.Target];


                        if (IncludeLinkState(target.SaveResultWasProcessed) || ((0 == target.SaveResultWasProcessed) && IncludeLinkState(target.State)) ||
                            ((null != target.Identity) && (target.ChangeOrder < box.ChangeOrder) &&
                             ((0 == target.SaveResultWasProcessed && EntityStates.Added == target.State) ||
                              (EntityStates.Added == target.SaveResultWasProcessed))))
                        {
                            Debug.Assert(box.ChangeOrder < end.ChangeOrder, "saving is out of order");
                            yield return end;
                        }
                    }
                }
            }
        }

        private LoadPropertyResult CreateLoadPropertyRequest(object entity, string propertyName, AsyncCallback callback, object state, Uri requestUri, DataServiceQueryContinuation continuation)
        {
            Debug.Assert(continuation == null || requestUri == null, "continuation == null || requestUri == null -- only one or the either (or neither) may be passed in");
            EntityDescriptor box = this.EnsureContained(entity, "entity");
            Util.CheckArgumentNotEmpty(propertyName, "propertyName");

            ClientType type = ClientType.Create(entity.GetType());
            Debug.Assert(type.IsEntityType, "must be entity type to be contained");

            if (EntityStates.Added == box.State)
            {
                throw Error.InvalidOperation(Strings.Context_NoLoadWithInsertEnd);
            }

            ClientType.ClientProperty property = type.GetProperty(propertyName, false);
            Debug.Assert(null != property, "should have thrown if propertyName didn't exist");

            ProjectionPlan plan;
            if (continuation == null)
            {
                plan = null;
            }
            else
            {
                plan = continuation.Plan;
                requestUri = continuation.NextLinkUri;
            }

            bool mediaLink = (type.MediaDataMember != null && propertyName == type.MediaDataMember.PropertyName);
            Version requestVersion;
            if (requestUri == null)
            {
                Uri relativeUri;
                if (mediaLink)
                {
                   
                    relativeUri = Util.CreateUri(XmlConstants.UriValueSegment, UriKind.Relative);
                }
                else
                {
                    relativeUri = Util.CreateUri(propertyName + (null != property.CollectionType ? "()" : String.Empty), UriKind.Relative);
                }

                requestUri = Util.CreateUri(box.GetResourceUri(this.baseUriWithSlash, true ), relativeUri);
                requestVersion = Util.DataServiceVersion1;
            }
            else
            {
  
                requestVersion = Util.DataServiceVersionEmpty;
            }

            HttpWebRequest request = this.CreateRequest(requestUri, XmlConstants.HttpMethodGet, mediaLink, null, requestVersion, false);
            DataServiceRequest dataServiceRequest = DataServiceRequest.GetInstance(property.PropertyType, requestUri);
            return new LoadPropertyResult(entity, propertyName, this, request, callback, state, dataServiceRequest, plan);
        }


        private void WriteContentProperties(XmlWriter writer, ClientType type, object resource, EpmSourcePathSegment currentSegment, out bool propertiesWritten)
        {
            #region <d:property>value</property>
            propertiesWritten = false;
            foreach (ClientType.ClientProperty property in type.Properties)
            {
                if (property == type.MediaDataMember ||
                    (type.MediaDataMember != null &&
                     type.MediaDataMember.MimeTypeProperty == property))
                {
                    continue;
                }

                object propertyValue = property.GetValue(resource);

                EpmSourcePathSegment matchedSegment = currentSegment != null ? currentSegment.SubProperties.SingleOrDefault(s => s.PropertyName == property.PropertyName) : null;

                if (property.IsKnownType)
                {
                    if (propertyValue == null || matchedSegment == null || matchedSegment.EpmInfo.Attribute.KeepInContent)
                    {
                        WriteContentProperty(writer, this.DataNamespace, property, propertyValue);
                        propertiesWritten = true;
                    }
                }
#if ASTORIA_OPEN_OBJECT
                else if (property.OpenObjectProperty)
                {
                    foreach (KeyValuePair<string, object> pair in (IDictionary<string, object>)propertyValue)
                    {
                        if ((null == pair.Value) || ClientConvert.IsKnownType(pair.Value.GetType()))
                        {
                            Type valueType = pair.Value != null ? pair.Value.GetType() : typeof(string);
                            ClientType.ClientProperty openProperty = new ClientType.ClientProperty(null, valueType, false, true);
                            WriteContentProperty(writer, this.DataNamespace, openProperty, pair.Value);
                            propertiesWritten = true;
                        }
                    }
                }
#endif
                else if (null == property.CollectionType)
                {
                    ClientType nested = ClientType.Create(property.PropertyType);
                    if (!nested.IsEntityType)
                    {
                        #region complex type
                        XElement complexProperty = new XElement(((XNamespace)this.DataNamespace) + property.PropertyName);
                        bool shouldWriteComplexProperty = false;
                        string typeName = this.ResolveNameFromType(nested.ElementType);
                        if (!String.IsNullOrEmpty(typeName))
                        {
                            complexProperty.Add(new XAttribute(((XNamespace)XmlConstants.DataWebMetadataNamespace) + XmlConstants.AtomTypeAttributeName, typeName));
                        }
                        
           
                        if (null == propertyValue)
                        {
                            complexProperty.Add(new XAttribute(((XNamespace)XmlConstants.DataWebMetadataNamespace) + XmlConstants.AtomNullAttributeName, XmlConstants.XmlTrueLiteral));
                            shouldWriteComplexProperty = true;
                        }
                        else
                        {
                            using (XmlWriter complexPropertyWriter = complexProperty.CreateWriter())
                            {
                                this.WriteContentProperties(complexPropertyWriter, nested, propertyValue, matchedSegment, out shouldWriteComplexProperty);
                            }
                        }

                        if (shouldWriteComplexProperty)
                        {
                            complexProperty.WriteTo(writer);
                            propertiesWritten = true;
                        }
                        #endregion
                    }
                }
            }
            #endregion
        }


        private void DetachExistingLink(LinkDescriptor existingLink, bool targetDelete)
        {
     
            if (existingLink.Target != null)
            {
              
                EntityDescriptor targetResource = this.entityDescriptors[existingLink.Target];
                
    
                if (targetResource.IsDeepInsert && !targetDelete)
                {
                    EntityDescriptor parentOfTarget = targetResource.ParentForInsert;
                    if (Object.ReferenceEquals(targetResource.ParentEntity, existingLink.Source) && 
                       (parentOfTarget.State != EntityStates.Deleted || 
                        parentOfTarget.State != EntityStates.Detached))
                    {
                        throw new InvalidOperationException(Strings.Context_ChildResourceExists);
                    }
                }
            }
        
            if (this.bindings.Remove(existingLink))
            {   
                existingLink.State = EntityStates.Detached;
            }
        }


        private LinkDescriptor DetachReferenceLink(object source, string sourceProperty, object target, MergeOption linkMerge)
        {
            LinkDescriptor existing = this.GetLinks(source, sourceProperty).FirstOrDefault();
            if (null != existing)
            {
                if ((target == existing.Target) ||
                    (MergeOption.AppendOnly == linkMerge) ||
                    (MergeOption.PreserveChanges == linkMerge && EntityStates.Modified == existing.State))
                {
                    return existing;
                }

                this.DetachExistingLink(existing, false);
                Debug.Assert(!this.bindings.Values.Any(o => (o.Source == source) && (o.SourceProperty == sourceProperty)), "only expecting one");
            }

            return null;
        }


        private EntityDescriptor EnsureContained(object resource, string parameterName)
        {
            Util.CheckArgumentNull(resource, parameterName);

            EntityDescriptor box = null;
            if (!this.entityDescriptors.TryGetValue(resource, out box))
            {
                throw Error.InvalidOperation(Strings.Context_EntityNotContained);
            }

            return box;
        }

         private bool EnsureRelatable(object source, string sourceProperty, object target, EntityStates state)
        {
            EntityDescriptor sourceResource = this.EnsureContained(source, "source");
            EntityDescriptor targetResource = null;
            if ((null != target) || ((EntityStates.Modified != state) && (EntityStates.Unchanged != state)))
            {
                targetResource = this.EnsureContained(target, "target");
            }

            Util.CheckArgumentNotEmpty(sourceProperty, "sourceProperty");

            ClientType type = ClientType.Create(source.GetType());
            Debug.Assert(type.IsEntityType, "should be enforced by just adding an object");

     
            ClientType.ClientProperty property = type.GetProperty(sourceProperty, false);

            if (property.IsKnownType)
            {
                throw Error.InvalidOperation(Strings.Context_RelationNotRefOrCollection);
            }

            if ((EntityStates.Unchanged == state) && (null == target) && (null != property.CollectionType))
            {
                targetResource = this.EnsureContained(target, "target");
            }

            if (((EntityStates.Added == state) || (EntityStates.Deleted == state)) && (null == property.CollectionType))
            {
                throw Error.InvalidOperation(Strings.Context_AddLinkCollectionOnly);
            }
            else if ((EntityStates.Modified == state) && (null != property.CollectionType))
            {
                throw Error.InvalidOperation(Strings.Context_SetLinkReferenceOnly);
            }

             type = ClientType.Create(property.CollectionType ?? property.PropertyType);
            Debug.Assert(type.IsEntityType, "should be enforced by just adding an object");

            if ((null != target) && !type.ElementType.IsInstanceOfType(target))
            {
                throw Error.Argument(Strings.Context_RelationNotRefOrCollection, "target");
            }

            if ((EntityStates.Added == state) || (EntityStates.Unchanged == state))
            {
                if ((sourceResource.State == EntityStates.Deleted) ||
                    ((targetResource != null) && (targetResource.State == EntityStates.Deleted)))
                {
                    throw Error.InvalidOperation(Strings.Context_NoRelationWithDeleteEnd);
                }
            }

            if ((EntityStates.Deleted == state) || (EntityStates.Unchanged == state))
            {
                if ((sourceResource.State == EntityStates.Added) ||
                    ((targetResource != null) && (targetResource.State == EntityStates.Added)))
                {
                    if (EntityStates.Deleted == state)
                    {
                        return true;
                    }

                    throw Error.InvalidOperation(Strings.Context_NoRelationWithInsertEnd);
                }
            }

            return false;
        }

        private void ValidateEntitySetName(ref string entitySetName)
        {
            Util.CheckArgumentNotEmpty(entitySetName, "entitySetName");
            entitySetName = entitySetName.Trim(Util.ForwardSlash);

            Util.CheckArgumentNotEmpty(entitySetName, "entitySetName");

            Uri tmp = Util.CreateUri(entitySetName, UriKind.RelativeOrAbsolute);
            if (tmp.IsAbsoluteUri ||
                !String.IsNullOrEmpty(Util.CreateUri(this.baseUriWithSlash, tmp)
                                     .GetComponents(UriComponents.Query | UriComponents.Fragment, UriFormat.SafeUnescaped)))
            {
                throw Error.Argument(Strings.Context_EntitySetName, "entitySetName");
            }
        }

        private void EnsureIdentityToResource()
        {
            if (null == this.identityToDescriptor)
            {
                System.Threading.Interlocked.CompareExchange(ref this.identityToDescriptor, new Dictionary<String, EntityDescriptor>(EqualityComparer<String>.Default), null);
            }
        }

        private void IncrementChange(Descriptor descriptor)
        {
            descriptor.ChangeOrder = ++this.nextChange;
        }

        private GetReadStreamResult CreateGetReadStreamResult(
            object entity, 
            DataServiceRequestArgs args,
            AsyncCallback callback, 
            object state)
        {
            EntityDescriptor box = this.EnsureContained(entity, "entity");
            Util.CheckArgumentNull(args, "args");

            Uri requestUri = box.GetMediaResourceUri(this.baseUriWithSlash);
            if (requestUri == null)
            {
                throw new ArgumentException(Strings.Context_EntityNotMediaLinkEntry, "entity");
            }

#if ASTORIA_LIGHT
           HttpWebRequest request = this.CreateRequest(requestUri, XmlConstants.HttpMethodGet, true, null, null, false, HttpStack.ClientHttp);
#else
            HttpWebRequest request = this.CreateRequest(requestUri, XmlConstants.HttpMethodGet, true, null, null, false);
#endif

            WebUtil.ApplyHeadersToRequest(args.Headers, request, false);

            return new GetReadStreamResult(this, "GetReadStream", request, callback, state);
        }

        internal class DataServiceSaveStream
        {
            private readonly Stream stream;

             private readonly bool close;

            private readonly DataServiceRequestArgs args;

             internal DataServiceSaveStream(Stream stream, bool close, DataServiceRequestArgs args)
            {
                Debug.Assert(stream != null, "stream must not be null.");

                this.stream = stream;
                this.close = close;
                this.args = args;
            }

            internal Stream Stream
            {
                get 
                {
                    return this.stream;
                }
            }

            internal DataServiceRequestArgs Args
            {
                get { return this.args; }
            }

            internal void Close()
            {
                if (this.stream != null && this.close)
                {
                    this.stream.Close();
                }
            }
        }

        private class LoadPropertyResult : QueryResult
        {
            #region Private fields.

            private readonly object entity;

            private readonly ProjectionPlan plan;

            private readonly string propertyName;

            #endregion Private fields.

            internal LoadPropertyResult(object entity, string propertyName, DataServiceContext context, HttpWebRequest request, AsyncCallback callback, object state, DataServiceRequest dataServiceRequest, ProjectionPlan plan)
                : base(context, "LoadProperty", dataServiceRequest, request, callback, state)
            {
                this.entity = entity;
                this.propertyName = propertyName;
                this.plan = plan;
            }

            internal QueryOperationResponse LoadProperty()
            {
                MaterializeAtom results = null;

                DataServiceContext context = (DataServiceContext)this.Source;

                ClientType type = ClientType.Create(this.entity.GetType());
                Debug.Assert(type.IsEntityType, "must be entity type to be contained");

                EntityDescriptor box = context.EnsureContained(this.entity, "entity");

                if (EntityStates.Added == box.State)
                {
                    throw Error.InvalidOperation(Strings.Context_NoLoadWithInsertEnd);
                }

                ClientType.ClientProperty property = type.GetProperty(this.propertyName, false);
                Type elementType = property.CollectionType ?? property.NullablePropertyType;
                try
                {
                    if (type.MediaDataMember == property)
                    {
                        results = this.ReadPropertyFromRawData(property);
                    }
                    else
                    {
                        results = this.ReadPropertyFromAtom(box, property);
                    }
                    
                    return this.GetResponseWithType(results, elementType);
                }
                catch (InvalidOperationException ex)
                {
                    QueryOperationResponse response = this.GetResponseWithType(results, elementType);
                    if (response != null)
                    {
                        response.Error = ex;
                        throw new DataServiceQueryException(Strings.DataServiceException_GeneralError, ex, response);
                    }

                    throw;
                }
            }

            private static byte[] ReadByteArrayWithContentLength(Stream responseStream, int totalLength)
            {
                byte[] buffer = new byte[totalLength];
                int read = 0;
                while (read < totalLength)
                {
                    int r = responseStream.Read(buffer, read, totalLength - read);
                    if (r <= 0)
                    {
                        throw Error.InvalidOperation(Strings.Context_UnexpectedZeroRawRead);
                    }

                    read += r;
                }

                return buffer;
            }

            private static byte[] ReadByteArrayChunked(Stream responseStream)
            {
                byte[] completeBuffer = null;
                using (MemoryStream m = new MemoryStream())
                {
                    byte[] buffer = new byte[4096];
                    int numRead = 0;
                    int totalRead = 0;
                    while (true)
                    {
                        numRead = responseStream.Read(buffer, 0, buffer.Length);
                        if (numRead <= 0)
                        {
                            break;
                        }

                        m.Write(buffer, 0, numRead);
                        totalRead += numRead;
                    }

                    completeBuffer = new byte[totalRead];
                    m.Position = 0;
                    numRead = m.Read(completeBuffer, 0, completeBuffer.Length);
                }

                return completeBuffer;
            }

            private MaterializeAtom ReadPropertyFromAtom(EntityDescriptor box, ClientType.ClientProperty property)
            {
                DataServiceContext context = (DataServiceContext)this.Source;

                bool merging = context.ApplyingChanges;

                try
                {
                    context.ApplyingChanges = true;

                    bool deletedState = (EntityStates.Deleted == box.State);

                    Type nestedType;
#if ASTORIA_OPEN_OBJECT
                if (property.OpenObjectProperty)
                {
                    nestedType = typeof(OpenObject);
                }
                else
#endif
                    {
                        nestedType = property.CollectionType ?? property.NullablePropertyType;
                    }

                    ClientType clientType = ClientType.Create(nestedType);

                    bool setNestedValue = false;
                    object collection = this.entity;
                    if (null != property.CollectionType)
                    {   collection = property.GetValue(this.entity);
                        if (null == collection)
                        {
                            setNestedValue = true;
                            if (BindingEntityInfo.IsDataServiceCollection(property.PropertyType))
                            {
                                Debug.Assert(WebUtil.GetDataServiceCollectionOfT(nestedType) != null, "DataServiceCollection<> must be available here.");

                                collection = Activator.CreateInstance(
                                    WebUtil.GetDataServiceCollectionOfT(nestedType), 
                                    null,
                                    TrackingMode.None);
                            }
                            else
                            {
                                collection = Activator.CreateInstance(typeof(List<>).MakeGenericType(nestedType));
                            }
                        }
                    }

                    Type elementType = property.CollectionType ?? property.NullablePropertyType;
                    IList results = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));

                    DataServiceQueryContinuation continuation = null;

                    using (MaterializeAtom materializer = this.GetMaterializer(context, this.plan))
                    {
                        Debug.Assert(materializer != null, "materializer != null -- otherwise GetMaterializer() returned null rather than empty");
                        int count = 0;
#if ASTORIA_OPEN_OBJECT
                        object openProperties = null;
#endif
                        foreach (object child in materializer)
                        {
                            results.Add(child);
                            count++;
#if ASTORIA_OPEN_OBJECT
                            property.SetValue(collection, child, this.propertyName, ref openProperties, true);
#else
                            property.SetValue(collection, child, this.propertyName, true);
#endif

                            if ((null != child) && (MergeOption.NoTracking != materializer.MergeOptionValue) && clientType.IsEntityType)
                            {
                                if (deletedState)
                                {
                                    context.DeleteLink(this.entity, this.propertyName, child);
                                }
                                else
                                {   context.AttachLink(this.entity, this.propertyName, child, materializer.MergeOptionValue);
                                }
                            }
                        }

                        continuation = materializer.GetContinuation(null);
                        Util.SetNextLinkForCollection(collection, continuation);

                    }

                    if (setNestedValue)
                    {
#if ASTORIA_OPEN_OBJECT
                    object openProperties = null;
                    property.SetValue(this.entity, collection, this.propertyName, ref openProperties, false);
#else
                        property.SetValue(this.entity, collection, this.propertyName, false);
#endif
                    }

                    return MaterializeAtom.CreateWrapper(results, continuation);
                }
                finally
                {
                    context.ApplyingChanges = merging;
                }
            }

           private MaterializeAtom ReadPropertyFromRawData(ClientType.ClientProperty property)
            {
                DataServiceContext context = (DataServiceContext)this.Source;

                bool merging = context.ApplyingChanges;

                try
                {
                    context.ApplyingChanges = true;

#if ASTORIA_OPEN_OBJECT
                object openProps = null;
#endif
                    string mimeType = null;
                    Encoding encoding = null;
                    Type elementType = property.CollectionType ?? property.NullablePropertyType;
                    IList results = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                    HttpProcessUtility.ReadContentType(this.ContentType, out mimeType, out encoding);

                    using (Stream responseStream = this.GetResponseStream())
                    {
                        if (property.PropertyType == typeof(byte[]))
                        {
                            int total = checked((int)this.ContentLength);
                            byte[] buffer = null;
                            if (total >= 0)
                            {
                                buffer = LoadPropertyResult.ReadByteArrayWithContentLength(responseStream, total);
                            }
                            else
                            {
                                buffer = LoadPropertyResult.ReadByteArrayChunked(responseStream);
                            }

                            results.Add(buffer);
#if ASTORIA_OPEN_OBJECT
                            property.SetValue(this.entity, buffer, this.propertyName, ref openProps, false);
#else
                            property.SetValue(this.entity, buffer, this.propertyName, false);
#endif
                        }
                        else
                        {
                            StreamReader reader = new StreamReader(responseStream, encoding);
                            object convertedValue = property.PropertyType == typeof(string) ?
                                                        reader.ReadToEnd() :
                                                        ClientConvert.ChangeType(reader.ReadToEnd(), property.PropertyType);
                            results.Add(convertedValue);
#if ASTORIA_OPEN_OBJECT
                            property.SetValue(this.entity, convertedValue, this.propertyName, ref openProps, false);
#else
                            property.SetValue(this.entity, convertedValue, this.propertyName, false);
#endif
                        }
                    }

#if ASTORIA_OPEN_OBJECT
                Debug.Assert(openProps == null, "These should not be set in this path");
#endif
                    if (property.MimeTypeProperty != null)
                    {
                       
#if ASTORIA_OPEN_OBJECT
                    property.MimeTypeProperty.SetValue(this.entity, mimeType, null, ref openProps, false);
                    Debug.Assert(openProps == null, "These should not be set in this path");
#else
                        property.MimeTypeProperty.SetValue(this.entity, mimeType, null, false);
#endif
                    }

                    return MaterializeAtom.CreateWrapper(results);
                }
                finally
                {
                    context.ApplyingChanges = merging;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Pending")]
        private class SaveResult : BaseAsyncResult
        {
            private readonly DataServiceContext Context;

           private readonly List<Descriptor> ChangedEntries;

            private readonly DataServiceRequest[] Queries;

            private readonly List<OperationResponse> Responses;

             private readonly string batchBoundary;

            private readonly SaveChangesOptions options;

            private readonly bool executeAsync;

             private int changesCompleted;

            private PerRequest request;

            private HttpWebResponse batchResponse;

            private Stream httpWebResponseStream;

            private DataServiceResponse service;

            private int entryIndex = -1;

           private bool processingMediaLinkEntry;

            private bool processingMediaLinkEntryPut;

            private Stream mediaResourceRequestStream;

            private BatchStream responseBatchStream;

            private byte[] buildBatchBuffer;

            private StreamWriter buildBatchWriter;

            private long copiedContentLength;

            private string changesetBoundary;

            private bool changesetStarted;

            #region constructors
            internal SaveResult(DataServiceContext context, string method, DataServiceRequest[] queries, SaveChangesOptions options, AsyncCallback callback, object state, bool async)
                : base(context, method, callback, state)
            {
                this.executeAsync = async;
                this.Context = context;
                this.Queries = queries;
                this.options = options;

                this.Responses = new List<OperationResponse>();

                if (null == queries)
                {
                    #region changed entries
                    this.ChangedEntries = context.entityDescriptors.Values.Cast<Descriptor>()
                                          .Union(context.bindings.Values.Cast<Descriptor>())
                                          .Where(o => o.IsModified && o.ChangeOrder != UInt32.MaxValue)
                                          .OrderBy(o => o.ChangeOrder)
                                          .ToList();

                    foreach (Descriptor e in this.ChangedEntries)
                    {
                        e.ContentGeneratedForSave = false;
                        e.SaveResultWasProcessed = 0;
                        e.SaveError = null;

                        if (!e.IsResource)
                        {
                            object target = ((LinkDescriptor)e).Target;
                            if (null != target)
                            {
                                Descriptor f = context.entityDescriptors[target];
                                if (EntityStates.Unchanged == f.State)
                                {
                                    f.ContentGeneratedForSave = false;
                                    f.SaveResultWasProcessed = 0;
                                    f.SaveError = null;
                                }
                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    this.ChangedEntries = new List<Descriptor>();
                }

                if (IsFlagSet(options, SaveChangesOptions.Batch))
                {
                    this.batchBoundary = XmlConstants.HttpMultipartBoundaryBatch + "_" + Guid.NewGuid().ToString();
                }
                else
                {
                    this.batchBoundary = XmlConstants.HttpMultipartBoundaryBatchResponse + "_" + Guid.NewGuid().ToString();
                    this.DataServiceResponse = new DataServiceResponse(null, -1, this.Responses, false );
                }
            }
            #endregion constructor

            #region end

            internal DataServiceResponse DataServiceResponse
            {
                get
                {
                    return this.service;
                }

                set
                {
                    this.service = value;
                }
            }

            internal DataServiceResponse EndRequest()
            {
                foreach (EntityDescriptor box in this.ChangedEntries.Where(e => e.IsResource).Cast<EntityDescriptor>())
                {
                    box.CloseSaveStream();
                }

                if ((null != this.responseBatchStream) || (null != this.httpWebResponseStream))
                {
                    this.HandleBatchResponse();
                }

                return this.DataServiceResponse;
            }

            #endregion

            #region start a batch

            internal void BatchBeginRequest(bool replaceOnUpdate)
            {
                PerRequest pereq = null;
                try
                {
                    MemoryStream memory = this.GenerateBatchRequest(replaceOnUpdate);
                    if (null != memory)
                    {
                        HttpWebRequest httpWebRequest = this.CreateBatchRequest(memory);
                        this.Abortable = httpWebRequest;

                        this.request = pereq = new PerRequest();
                        pereq.Request = httpWebRequest;
                        pereq.RequestContentStream = new PerRequest.ContentStream(memory, true);

                        this.httpWebResponseStream = new MemoryStream();

                        IAsyncResult asyncResult = BaseAsyncResult.InvokeAsync(httpWebRequest.BeginGetRequestStream, this.AsyncEndGetRequestStream, pereq);
                        pereq.RequestCompletedSynchronously &= asyncResult.CompletedSynchronously;
                    }
                    else
                    {
                        Debug.Assert(this.CompletedSynchronously, "completedSynchronously");
                        Debug.Assert(this.IsCompletedInternally, "completed");
                    }
                }
                catch (Exception e)
                {
                    this.HandleFailure(pereq, e);
                    throw; 
                }
                finally
                {
                    this.HandleCompleted(pereq); 
                }

                Debug.Assert((this.CompletedSynchronously && this.IsCompleted) || !this.CompletedSynchronously, "sync without complete");
            }

#if !ASTORIA_LIGHT 
            internal void BatchRequest(bool replaceOnUpdate)
            {
                MemoryStream memory = this.GenerateBatchRequest(replaceOnUpdate);
                if ((null != memory) && (0 < memory.Length))
                {
                    HttpWebRequest httpWebRequest = this.CreateBatchRequest(memory);
                    using (System.IO.Stream requestStream = httpWebRequest.GetRequestStream())
                    {
                        byte[] buffer = memory.GetBuffer();
                        int bufferOffset = checked((int)memory.Position);
                        int bufferLength = checked((int)memory.Length) - bufferOffset;

                       requestStream.Write(buffer, bufferOffset, bufferLength);
                    }

                    HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    this.batchResponse = httpWebResponse;

                    if (null != httpWebResponse)
                    {
                        this.httpWebResponseStream = httpWebResponse.GetResponseStream();
                    }
                }
            }
#endif
            #endregion

            #region start a non-batch requests
            internal void BeginNextChange(bool replaceOnUpdate)
            {
                Debug.Assert(!this.IsCompletedInternally, "why being called if already completed?");

                PerRequest pereq = null;
                IAsyncResult asyncResult = null;
                do
                {
                    HttpWebRequest httpWebRequest = null;
                    HttpWebResponse response = null;
                    try
                    {
                        if (null != this.request)
                        {
                            this.SetCompleted();
                            Error.ThrowInternalError(InternalError.InvalidBeginNextChange);
                        }

                        this.Abortable = httpWebRequest = this.CreateNextRequest(replaceOnUpdate);
                        if ((null != httpWebRequest) || (this.entryIndex < this.ChangedEntries.Count))
                        {
                            if (this.ChangedEntries[this.entryIndex].ContentGeneratedForSave)
                            {
                                Debug.Assert(this.ChangedEntries[this.entryIndex] is LinkDescriptor, "only expected RelatedEnd to presave");
                                Debug.Assert(
                                    this.ChangedEntries[this.entryIndex].State == EntityStates.Added ||
                                    this.ChangedEntries[this.entryIndex].State == EntityStates.Modified,
                                    "only expected added to presave");
                                continue;
                            }

                            PerRequest.ContentStream contentStream = this.CreateChangeData(this.entryIndex, false);
                            if (this.executeAsync)
                            {
                                #region async
                                this.request = pereq = new PerRequest();
                                pereq.Request = httpWebRequest;

                                if (null == contentStream || null == contentStream.Stream)
                                {
                                    asyncResult = BaseAsyncResult.InvokeAsync(httpWebRequest.BeginGetResponse, this.AsyncEndGetResponse, pereq);
                                }
                                else
                                {
                                    if (contentStream.IsKnownMemoryStream)
                                    {
                                        httpWebRequest.ContentLength = contentStream.Stream.Length - contentStream.Stream.Position;
                                    }

                                    pereq.RequestContentStream = contentStream;
                                    asyncResult = BaseAsyncResult.InvokeAsync(httpWebRequest.BeginGetRequestStream, this.AsyncEndGetRequestStream, pereq);
                                }

                                pereq.RequestCompletedSynchronously &= asyncResult.CompletedSynchronously;
                                this.CompletedSynchronously &= asyncResult.CompletedSynchronously;
                                #endregion
                            }
#if !ASTORIA_LIGHT 
                            else
                            {
                                #region sync
                                if (null != contentStream && null != contentStream.Stream)
                                {
                                    if (contentStream.IsKnownMemoryStream)
                                    {
                                        httpWebRequest.ContentLength = contentStream.Stream.Length - contentStream.Stream.Position;
                                    }

                                    using (Stream stream = httpWebRequest.GetRequestStream())
                                    {
                                        byte[] buffer = new byte[64 * 1024];
                                        int read;
                                        do
                                        {
                                            read = contentStream.Stream.Read(buffer, 0, buffer.Length);
                                            if (read > 0)
                                            {
                                                stream.Write(buffer, 0, read);
                                            }
                                        }
                                        while (read > 0);
                                    }
                                }

                                response = (HttpWebResponse)httpWebRequest.GetResponse();
                                if (!this.processingMediaLinkEntry)
                                {
                                    this.changesCompleted++;
                                }

                                this.HandleOperationResponse(response);
                                this.HandleOperationResponseData(response);
                                this.HandleOperationEnd();
                                this.request = null;
                                #endregion
                            }
#endif
                        }
                        else
                        {
                            this.SetCompleted();

                            if (this.CompletedSynchronously)
                            {
                                this.HandleCompleted(pereq);
                            }
                        }
                    }
                    catch (InvalidOperationException e)
                    {
                        WebUtil.GetHttpWebResponse(e, ref response);
                        this.HandleOperationException(e, response);
                        this.HandleCompleted(pereq);
                    }
                    finally
                    {
                        if (null != response)
                        {
                            response.Close();
                        }
                    }

                   
                }
                while (((null == pereq) || (pereq.RequestCompleted && asyncResult != null && asyncResult.CompletedSynchronously)) && !this.IsCompletedInternally);

                Debug.Assert(this.executeAsync || this.CompletedSynchronously, "sync !CompletedSynchronously");
                Debug.Assert((this.CompletedSynchronously && this.IsCompleted) || !this.CompletedSynchronously, "sync without complete");
                Debug.Assert(this.entryIndex < this.ChangedEntries.Count || this.ChangedEntries.All(o => o.ContentGeneratedForSave), "didn't generate content for all entities/links");
            }

             protected override void CompletedRequest()
            {
                this.buildBatchBuffer = null;
                if (null != this.buildBatchWriter)
                {
                    Debug.Assert(!IsFlagSet(this.options, SaveChangesOptions.Batch), "should be non-batch");
                    this.HandleOperationEnd();
                    this.buildBatchWriter.WriteLine("--{0}--", this.batchBoundary);

                    this.buildBatchWriter.Flush();
                    Debug.Assert(Object.ReferenceEquals(this.httpWebResponseStream, this.buildBatchWriter.BaseStream), "expected different stream");
                    this.httpWebResponseStream.Position = 0;

                    this.buildBatchWriter = null;

                   this.responseBatchStream = new BatchStream(this.httpWebResponseStream, this.batchBoundary, HttpProcessUtility.EncodingUtf8NoPreamble, false);
                }
            }

            private static void CompleteCheck(PerRequest value, InternalError errorcode)
            {
                if ((null == value) || value.RequestCompleted)
                {
                   Error.ThrowInternalError(errorcode);
                }
            }

            private static void EqualRefCheck(PerRequest actual, PerRequest expected, InternalError errorcode)
            {
                if (!Object.ReferenceEquals(actual, expected))
                {
                    Error.ThrowInternalError(errorcode);
                }
            }

            private void HandleCompleted(PerRequest pereq)
            {
                if (null != pereq)
                {
                    this.CompletedSynchronously &= pereq.RequestCompletedSynchronously;

                    if (pereq.RequestCompleted)
                    {
                        System.Threading.Interlocked.CompareExchange(ref this.request, null, pereq);
                        if (IsFlagSet(this.options, SaveChangesOptions.Batch))
                        {   
                            System.Threading.Interlocked.CompareExchange(ref this.batchResponse, pereq.HttpWebResponse, null);
                            pereq.HttpWebResponse = null;
                        }

                        pereq.Dispose();
                    }
                }

                this.HandleCompleted();
            }

            private bool HandleFailure(PerRequest pereq, Exception e)
            {
                if (null != pereq)
                {
                    if (IsAborted)
                    {
                        pereq.SetAborted();
                    }
                    else
                    {
                        pereq.SetComplete();
                    }
                }

                return this.HandleFailure(e);
            }

            private HttpWebRequest CreateNextRequest(bool replaceOnUpdate)
            {
                if (!this.processingMediaLinkEntry)
                {
                    this.entryIndex++;
                }
                else
                {
                    Debug.Assert(this.ChangedEntries[this.entryIndex].IsResource, "Only resources can have MR's.");
                    EntityDescriptor box = (EntityDescriptor)this.ChangedEntries[this.entryIndex];
                    if (this.processingMediaLinkEntryPut && EntityStates.Unchanged == box.State)
                    {
                        box.ContentGeneratedForSave = true;
                        this.entryIndex++;
                    }

                    this.processingMediaLinkEntry = false;
                    this.processingMediaLinkEntryPut = false;

                    box.CloseSaveStream();
                }

                if (unchecked((uint)this.entryIndex < (uint)this.ChangedEntries.Count))
                {
                    Descriptor entry = this.ChangedEntries[this.entryIndex];
                    if (entry.IsResource)
                    {
                        EntityDescriptor box = (EntityDescriptor)entry;

                        HttpWebRequest req;
                        if (((EntityStates.Unchanged == entry.State) || (EntityStates.Modified == entry.State)) &&
                            (null != (req = this.CheckAndProcessMediaEntryPut(box))))
                        {
                            this.processingMediaLinkEntry = true;
                            this.processingMediaLinkEntryPut = true;
                        }
                        else if ((EntityStates.Added == entry.State) && (null != (req = this.CheckAndProcessMediaEntryPost(box))))
                        {
                            this.processingMediaLinkEntry = true;
                            this.processingMediaLinkEntryPut = false;
                        }
                        else
                        {
                            Debug.Assert(!this.processingMediaLinkEntry || entry.State == EntityStates.Modified, "!this.processingMediaLinkEntry || entry.State == EntityStates.Modified");
                            req = this.Context.CreateRequest(box, entry.State, replaceOnUpdate);
                        }

                        return req;
                    }

                    return this.Context.CreateRequest((LinkDescriptor)entry);
                }

                return null;
            }

            private HttpWebRequest CheckAndProcessMediaEntryPost(EntityDescriptor entityDescriptor)
            {
                ClientType type = ClientType.Create(entityDescriptor.Entity.GetType());

                if (!type.IsMediaLinkEntry && !entityDescriptor.IsMediaLinkEntry)
                {
                    return null;
                }

                if (type.MediaDataMember == null && entityDescriptor.SaveStream == null)
                {
                    throw Error.InvalidOperation(Strings.Context_MLEWithoutSaveStream(type.ElementTypeName));
                }

                Debug.Assert(
                    (type.MediaDataMember != null && entityDescriptor.SaveStream == null) ||
                    (type.MediaDataMember == null && entityDescriptor.SaveStream != null),
                    "Only one way of specifying the MR content is allowed.");

                HttpWebRequest mediaRequest = this.CreateMediaResourceRequest(
                    entityDescriptor.GetResourceUri(this.Context.baseUriWithSlash, false),
                    XmlConstants.HttpMethodPost,
                    type.MediaDataMember == null);

                if (type.MediaDataMember != null)
                {
                    if (type.MediaDataMember.MimeTypeProperty == null)
                    {
                        mediaRequest.ContentType = XmlConstants.MimeApplicationOctetStream;
                    }
                    else
                    {
                        object mimeTypeValue = type.MediaDataMember.MimeTypeProperty.GetValue(entityDescriptor.Entity);
                        String mimeType = mimeTypeValue != null ? mimeTypeValue.ToString() : null;

                        if (String.IsNullOrEmpty(mimeType))
                        {
                            throw Error.InvalidOperation(
                                Strings.Context_NoContentTypeForMediaLink(
                                    type.ElementTypeName,
                                    type.MediaDataMember.MimeTypeProperty.PropertyName));
                        }

                        mediaRequest.ContentType = mimeType;
                    }

                    object value = type.MediaDataMember.GetValue(entityDescriptor.Entity);
                    if (value == null)
                    {
                        mediaRequest.ContentLength = 0;
                        this.mediaResourceRequestStream = null;
                    }
                    else
                    {
                        byte[] buffer = value as byte[];
                        if (buffer == null)
                        {
                            string mime;
                            Encoding encoding;
                            HttpProcessUtility.ReadContentType(mediaRequest.ContentType, out mime, out encoding);

                            if (encoding == null)
                            {
                                encoding = Encoding.UTF8;
                                mediaRequest.ContentType += XmlConstants.MimeTypeUtf8Encoding;
                            }

                            buffer = encoding.GetBytes(ClientConvert.ToString(value, false));
                        }

                        mediaRequest.ContentLength = buffer.Length;

                        this.mediaResourceRequestStream = new MemoryStream(buffer, 0, buffer.Length, false, true);
                    }
                }
                else
                {
                    this.SetupMediaResourceRequest(mediaRequest, entityDescriptor);
                }

                entityDescriptor.State = EntityStates.Modified;

                return mediaRequest;
            }

            private HttpWebRequest CheckAndProcessMediaEntryPut(EntityDescriptor box)
            {
                if (box.SaveStream == null)
                {
                    return null;
                }

                Uri requestUri = box.GetEditMediaResourceUri(this.Context.baseUriWithSlash);
                if (requestUri == null)
                {
                    throw Error.InvalidOperation(
                        Strings.Context_SetSaveStreamWithoutEditMediaLink);
                }

                HttpWebRequest mediaResourceRequest = this.CreateMediaResourceRequest(requestUri, XmlConstants.HttpMethodPut, true);
                this.SetupMediaResourceRequest(mediaResourceRequest, box);

                if (box.StreamETag != null)
                {
                    mediaResourceRequest.Headers.Set(HttpRequestHeader.IfMatch, box.StreamETag);
                }

                return mediaResourceRequest;
            }

            private HttpWebRequest CreateMediaResourceRequest(Uri requestUri, string method, bool sendChunked)
            {
#if ASTORIA_LIGHT
                HttpWebRequest mediaResourceRequest = this.Context.CreateRequest(
                    requestUri,
                    method,
                    false,
                    XmlConstants.MimeAny,
                    Util.DataServiceVersion1,
                    sendChunked,
                    HttpStack.ClientHttp);
#else
                HttpWebRequest mediaResourceRequest = this.Context.CreateRequest(
                    requestUri,
                    method,
                    false,
                    XmlConstants.MimeAny,
                    Util.DataServiceVersion1,
                    sendChunked);
#endif
                return mediaResourceRequest;
            }

           private void SetupMediaResourceRequest(HttpWebRequest mediaResourceRequest, EntityDescriptor box)
            {
                this.mediaResourceRequestStream = box.SaveStream.Stream;

                WebUtil.ApplyHeadersToRequest(box.SaveStream.Args.Headers, mediaResourceRequest, true);

           }

           private PerRequest.ContentStream CreateChangeData(int index, bool newline)
            {
                Descriptor entry = this.ChangedEntries[index];
                Debug.Assert(!entry.ContentGeneratedForSave, "already saved entity/link");

                if (entry.IsResource)
                {
                    EntityDescriptor box = (EntityDescriptor)entry;
                    if (this.processingMediaLinkEntry)
                    {
                        Debug.Assert(
                            this.processingMediaLinkEntryPut || entry.State == EntityStates.Modified, 
                            "We should have modified the MLE state to Modified when we've created the MR POST request.");
                        Debug.Assert(
                            !this.processingMediaLinkEntryPut || (entry.State == EntityStates.Unchanged || entry.State == EntityStates.Modified),
                            "If we're processing MR PUT the entity must be either in Unchanged or Modified state.");

                        Debug.Assert(this.mediaResourceRequestStream != null, "We should have precreated the MR stream already.");
                        return new PerRequest.ContentStream(this.mediaResourceRequestStream, false);
                    }
                    else
                    {
                        entry.ContentGeneratedForSave = true;
                        return new PerRequest.ContentStream(this.Context.CreateRequestData(box, newline), true);
                    }
                }
                else
                {
                    entry.ContentGeneratedForSave = true;
                    LinkDescriptor link = (LinkDescriptor)entry;
                    if ((EntityStates.Added == link.State) ||
                        ((EntityStates.Modified == link.State) && (null != link.Target)))
                    {
                        return new PerRequest.ContentStream(this.Context.CreateRequestData(link, newline), true);
                    }
                }

                return null;
            }
            #endregion

            #region generate batch response from non-batch

            private void HandleOperationStart()
            {
                this.HandleOperationEnd();

                if (null == this.httpWebResponseStream)
                {
                    this.httpWebResponseStream = new MemoryStream();
                }

                if (null == this.buildBatchWriter)
                {
                    this.buildBatchWriter = new StreamWriter(this.httpWebResponseStream);    
#if TESTUNIXNEWLINE
                    this.buildBatchWriter.NewLine = NewLine;
#endif
                }

                if (null == this.changesetBoundary)
                {
                    this.changesetBoundary = XmlConstants.HttpMultipartBoundaryChangesetResponse + "_" + Guid.NewGuid().ToString();
                }

                this.changesetStarted = true;
                this.buildBatchWriter.WriteLine("--{0}", this.batchBoundary);
                this.buildBatchWriter.WriteLine("{0}: {1}; boundary={2}", XmlConstants.HttpContentType, XmlConstants.MimeMultiPartMixed, this.changesetBoundary);
                this.buildBatchWriter.WriteLine();
                this.buildBatchWriter.WriteLine("--{0}", this.changesetBoundary);
            }

             private void HandleOperationEnd()
            {
                if (this.changesetStarted)
                {
                    Debug.Assert(null != this.buildBatchWriter, "buildBatchWriter");
                    Debug.Assert(null != this.changesetBoundary, "changesetBoundary");
                    this.buildBatchWriter.WriteLine();
                    this.buildBatchWriter.WriteLine("--{0}--", this.changesetBoundary);
                    this.changesetStarted = false;
                }
            }

            private void HandleOperationException(Exception e, HttpWebResponse response)
            {
                if (null != response)
                {
                    this.HandleOperationResponse(response);
                    this.HandleOperationResponseData(response);
                    this.HandleOperationEnd();
                }
                else
                {
                    this.HandleOperationStart();
                    WriteOperationResponseHeaders(this.buildBatchWriter, 500);
                    this.buildBatchWriter.WriteLine("{0}: {1}", XmlConstants.HttpContentType, XmlConstants.MimeTextPlain);
                    this.buildBatchWriter.WriteLine("{0}: {1}", XmlConstants.HttpContentID, this.ChangedEntries[this.entryIndex].ChangeOrder);
                    this.buildBatchWriter.WriteLine();
                    this.buildBatchWriter.WriteLine(e.ToString());
                    this.HandleOperationEnd();
                }

                this.request = null;
                if (!IsFlagSet(this.options, SaveChangesOptions.ContinueOnError))
                {
                    this.SetCompleted();

                    this.processingMediaLinkEntry = false;

                    this.ChangedEntries[this.entryIndex].ContentGeneratedForSave = true;
                }
            }

            private void HandleOperationResponse(HttpWebResponse response)
            {
                this.HandleOperationStart();

                Descriptor entry = this.ChangedEntries[this.entryIndex];

                if (entry.IsResource)
                {
                    EntityDescriptor entityDescriptor = (EntityDescriptor)entry;

                    if (entry.State == EntityStates.Added ||
                         (entry.State == EntityStates.Modified &&
                          this.processingMediaLinkEntry && !this.processingMediaLinkEntryPut))
                    {
                        string location = response.Headers[XmlConstants.HttpResponseLocation];

                        if (WebUtil.SuccessStatusCode(response.StatusCode))
                        {
                            if (null != location)
                            {
                                this.Context.AttachLocation(entityDescriptor.Entity, location);
                            }
                            else
                            {
                                throw Error.NotSupported(Strings.Deserialize_NoLocationHeader);
                            }
                        }
                    }

                    if (this.processingMediaLinkEntry)
                    {
                        if (!WebUtil.SuccessStatusCode(response.StatusCode))
                        {
                           this.processingMediaLinkEntry = false;

                            if (!this.processingMediaLinkEntryPut)
                            {
                                Debug.Assert(entry.State == EntityStates.Modified, "Entity state should be set to Modified once we've sent the POST MR");
                                entry.State = EntityStates.Added;
                                this.processingMediaLinkEntryPut = false;
                            }

                           entry.ContentGeneratedForSave = true;
                        }
                        else if (response.StatusCode == HttpStatusCode.Created)
                        {
                            entityDescriptor.ETag = response.Headers[XmlConstants.HttpResponseETag];

                        }
                    }
                }

                WriteOperationResponseHeaders(this.buildBatchWriter, (int)response.StatusCode);
                foreach (string name in response.Headers.AllKeys)
                {
                    if (XmlConstants.HttpContentLength != name)
                    {
                        this.buildBatchWriter.WriteLine("{0}: {1}", name, response.Headers[name]);
                    }
                }

                this.buildBatchWriter.WriteLine("{0}: {1}", XmlConstants.HttpContentID, entry.ChangeOrder);
                this.buildBatchWriter.WriteLine();
            }

           private void HandleOperationResponseData(HttpWebResponse response)
            {
                using (Stream stream = response.GetResponseStream())
                {
                    if (null != stream)
                    {
                        this.buildBatchWriter.Flush();
                        if (0 == WebUtil.CopyStream(stream, this.buildBatchWriter.BaseStream, ref this.buildBatchBuffer))
                        {
                            this.HandleOperationResponseNoData();
                        }
                    }
                }
            }

            private void HandleOperationResponseNoData()
            {
                Debug.Assert(null != this.buildBatchWriter, "null buildBatchWriter");
                this.buildBatchWriter.Flush();
#if DEBUG
                MemoryStream memory = this.buildBatchWriter.BaseStream as MemoryStream;
                Debug.Assert(null != memory, "expected MemoryStream");
                Debug.Assert(this.buildBatchWriter.NewLine == NewLine, "mismatch NewLine");
                for (int kk = 0; kk < NewLine.Length; ++kk)
                {
                    Debug.Assert((char)memory.GetBuffer()[memory.Length - (NewLine.Length - kk)] == NewLine[kk], "didn't end with newline");
                }
#endif
                this.buildBatchWriter.BaseStream.Position -= NewLine.Length;
                this.buildBatchWriter.WriteLine("{0}: {1}", XmlConstants.HttpContentLength, 0);
                this.buildBatchWriter.WriteLine();
            }

            #endregion

             private HttpWebRequest CreateBatchRequest(MemoryStream memory)
            {
                Uri requestUri = Util.CreateUri(this.Context.baseUriWithSlash, Util.CreateUri("$batch", UriKind.Relative));
                string contentType = XmlConstants.MimeMultiPartMixed + "; " + XmlConstants.HttpMultipartBoundary + "=" + this.batchBoundary;
                HttpWebRequest httpWebRequest = this.Context.CreateRequest(requestUri, XmlConstants.HttpMethodPost, false, contentType, Util.DataServiceVersion1, false);
                httpWebRequest.ContentLength = memory.Length - memory.Position;
                return httpWebRequest;
            }

            private MemoryStream GenerateBatchRequest(bool replaceOnUpdate)
            {
                this.changesetBoundary = null;
                if (null != this.Queries)
                {
                }
                else if (0 == this.ChangedEntries.Count)
                {
                    this.DataServiceResponse = new DataServiceResponse(null, (int)WebExceptionStatus.Success, this.Responses, true );
                    this.SetCompleted();
                    return null;
                }
                else
                {
                    this.changesetBoundary = XmlConstants.HttpMultipartBoundaryChangeSet + "_" + Guid.NewGuid().ToString();
                }

                MemoryStream memory = new MemoryStream();
                StreamWriter text = new StreamWriter(memory);     

#if TESTUNIXNEWLINE
                text.NewLine = NewLine;
#endif

                if (null != this.Queries)
                {
                    for (int i = 0; i < this.Queries.Length; ++i)
                    {
                        Uri requestUri = Util.CreateUri(this.Context.baseUriWithSlash, this.Queries[i].QueryComponents.Uri);

                        Debug.Assert(null != requestUri, "request uri is null");
                        Debug.Assert(requestUri.IsAbsoluteUri, "request uri is not absolute uri");

                        text.WriteLine("--{0}", this.batchBoundary);
                        WriteOperationRequestHeaders(text, XmlConstants.HttpMethodGet, requestUri.AbsoluteUri, this.Queries[i].QueryComponents.Version);
                        text.WriteLine();
                    }
                }
                else if (0 < this.ChangedEntries.Count)
                {
                    text.WriteLine("--{0}", this.batchBoundary);
                    text.WriteLine("{0}: {1}; boundary={2}", XmlConstants.HttpContentType, XmlConstants.MimeMultiPartMixed, this.changesetBoundary);
                    text.WriteLine();

                    for (int i = 0; i < this.ChangedEntries.Count; ++i)
                    {
                        #region validate changeset boundary starts on newline
#if DEBUG
                        {
                            text.Flush();
                            for (int kk = 0; kk < NewLine.Length; ++kk)
                            {
                                Debug.Assert((char)memory.GetBuffer()[memory.Length - (NewLine.Length - kk)] == NewLine[kk], "boundary didn't start with newline");
                            }
                        }
#endif
                        #endregion

                        Descriptor entry = this.ChangedEntries[i];
                        if (entry.ContentGeneratedForSave)
                        {
                            continue;
                        }

                        text.WriteLine("--{0}", this.changesetBoundary);

                        EntityDescriptor entityDescriptor = entry as EntityDescriptor;
                        if (entry.IsResource)
                        {
                            if (entityDescriptor.State == EntityStates.Added)
                            {
                                ClientType type = ClientType.Create(entityDescriptor.Entity.GetType());
                                if (type.IsMediaLinkEntry || entityDescriptor.IsMediaLinkEntry)
                                {
                                    throw Error.NotSupported(Strings.Context_BatchNotSupportedForMediaLink);
                                }
                            }
                            else if (entityDescriptor.State == EntityStates.Unchanged || entityDescriptor.State == EntityStates.Modified)
                            {
                                if (entityDescriptor.SaveStream != null)
                                {
                                    throw Error.NotSupported(Strings.Context_BatchNotSupportedForMediaLink);
                                }
                            }
                        }

                        PerRequest.ContentStream contentStream = this.CreateChangeData(i, true);
                        MemoryStream stream = null;
                        if (null != contentStream)
                        {
                            Debug.Assert(contentStream.IsKnownMemoryStream, "Batch requests don't support MRs yet");
                            stream = contentStream.Stream as MemoryStream;
                        }

                        if (entry.IsResource)
                        {
                            this.Context.CreateRequestBatch(entityDescriptor, text, replaceOnUpdate);
                        }
                        else
                        {
                            this.Context.CreateRequestBatch((LinkDescriptor)entry, text);
                        }

                        byte[] buffer = null;
                        int bufferOffset = 0, bufferLength = 0;
                        if (null != stream)
                        {
                            buffer = stream.GetBuffer();
                            bufferOffset = checked((int)stream.Position);
                            bufferLength = checked((int)stream.Length) - bufferOffset;
                        }

                        if (0 < bufferLength)
                        {
                            text.WriteLine("{0}: {1}", XmlConstants.HttpContentLength, bufferLength);
                        }

                        text.WriteLine();

                        if (0 < bufferLength)
                        {
                            text.Flush();
                            text.BaseStream.Write(buffer, bufferOffset, bufferLength);
                        }
                    }

                    #region validate changeset boundary ended with newline
#if DEBUG
                    {
                        text.Flush();

                        for (int kk = 0; kk < NewLine.Length; ++kk)
                        {
                            Debug.Assert((char)memory.GetBuffer()[memory.Length - (NewLine.Length - kk)] == NewLine[kk], "post CreateRequest boundary didn't start with newline");
                        }
                    }
#endif
                    #endregion

                   text.WriteLine("--{0}--", this.changesetBoundary);
                }

                text.WriteLine("--{0}--", this.batchBoundary);

                text.Flush();
                Debug.Assert(Object.ReferenceEquals(text.BaseStream, memory), "should be same");
                Debug.Assert(this.ChangedEntries.All(o => o.ContentGeneratedForSave), "didn't generated content for all entities/links");

                #region Validate batch format
#if DEBUG
                int testGetCount = 0;
                int testOpCount = 0;
                int testBeginSetCount = 0;
                int testEndSetCount = 0;
                memory.Position = 0;
                BatchStream testBatch = new BatchStream(memory, this.batchBoundary, HttpProcessUtility.EncodingUtf8NoPreamble, true);
                while (testBatch.MoveNext())
                {
                    switch (testBatch.State)
                    {
                        case BatchStreamState.StartBatch:
                        case BatchStreamState.EndBatch:
                        default:
                            Debug.Assert(false, "shouldn't happen");
                            break;

                        case BatchStreamState.Get:
                            testGetCount++;
                            break;

                        case BatchStreamState.BeginChangeSet:
                            testBeginSetCount++;
                            break;
                        case BatchStreamState.EndChangeSet:
                            testEndSetCount++;
                            break;
                        case BatchStreamState.Post:
                        case BatchStreamState.Put:
                        case BatchStreamState.Delete:
                        case BatchStreamState.Merge:
                            testOpCount++;
                            break;
                    }
                }

                Debug.Assert((null == this.Queries && 1 == testBeginSetCount) || (0 == testBeginSetCount), "more than one BeginChangeSet");
                Debug.Assert(testBeginSetCount == testEndSetCount, "more than one EndChangeSet");
                Debug.Assert((null == this.Queries && testGetCount == 0) || this.Queries.Length == testGetCount, "too many get count");
                Debug.Assert(BatchStreamState.EndBatch == testBatch.State, "should have ended propertly");
#endif
                #endregion

                this.changesetBoundary = null;

                memory.Position = 0;
                return memory;
            }

            #region handle batch response

            private void HandleBatchResponse()
            {
                string boundary = this.batchBoundary;
                Encoding encoding = Encoding.UTF8;
                Dictionary<string, string> headers = null;
                Exception exception = null;

                try
                {
                    if (IsFlagSet(this.options, SaveChangesOptions.Batch))
                    {
                        if ((null == this.batchResponse) || (HttpStatusCode.NoContent == this.batchResponse.StatusCode))
                        {   
                            throw Error.InvalidOperation(Strings.Batch_ExpectedResponse(1));
                        }

                        headers = WebUtil.WrapResponseHeaders(this.batchResponse);
                        HandleResponse(
                            this.batchResponse.StatusCode,                                    
                            this.batchResponse.Headers[XmlConstants.HttpDataServiceVersion],   
                            delegate() { return this.httpWebResponseStream; },                 
                            true);                                                              

                        if (!BatchStream.GetBoundaryAndEncodingFromMultipartMixedContentType(this.batchResponse.ContentType, out boundary, out encoding))
                        {
                            string mime;
                            Exception inner = null;
                            HttpProcessUtility.ReadContentType(this.batchResponse.ContentType, out mime, out encoding);
                            if (String.Equals(XmlConstants.MimeTextPlain, mime))
                            {
                                inner = GetResponseText(this.batchResponse.GetResponseStream, this.batchResponse.StatusCode);
                            }

                            throw Error.InvalidOperation(Strings.Batch_ExpectedContentType(this.batchResponse.ContentType), inner);
                        }

                        if (null == this.httpWebResponseStream)
                        {
                            Error.ThrowBatchExpectedResponse(InternalError.NullResponseStream);
                        }

                        this.DataServiceResponse = new DataServiceResponse(headers, (int)this.batchResponse.StatusCode, this.Responses, true);
                    }

                    bool close = true;
                    BatchStream batchStream = null;
                    try
                    {
                        batchStream = this.responseBatchStream ?? new BatchStream(this.httpWebResponseStream, boundary, encoding, false);
                        this.httpWebResponseStream = null;
                        this.responseBatchStream = null;

                        IEnumerable<OperationResponse> responses = this.HandleBatchResponse(batchStream);
                        if (IsFlagSet(this.options, SaveChangesOptions.Batch) && (null != this.Queries))
                        {
                            close = false;
                            this.responseBatchStream = batchStream;

                            this.DataServiceResponse = new DataServiceResponse(
                                (Dictionary<string, string>)this.DataServiceResponse.BatchHeaders,
                                this.DataServiceResponse.BatchStatusCode,
                                responses,
                                true );
                        }
                        else
                        {   
                            foreach (ChangeOperationResponse response in responses)
                            {
                                if (exception == null && response.Error != null)
                                {
                                    exception = response.Error;
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (close && (null != batchStream))
                        {
                            batchStream.Close();
                        }
                    }
                }
                catch (InvalidOperationException ex)
                {
                    exception = ex;
                }

                if (exception != null)
                {
                    if (this.DataServiceResponse == null)
                    {
                        int statusCode = this.batchResponse == null ? (int)HttpStatusCode.InternalServerError : (int)this.batchResponse.StatusCode;
                        this.DataServiceResponse = new DataServiceResponse(headers, statusCode, null, IsFlagSet(this.options, SaveChangesOptions.Batch));
                    }

                    throw new DataServiceRequestException(Strings.DataServiceException_GeneralError, exception, this.DataServiceResponse);
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506", Justification = "Central method of the API, likely to have many cross-references")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031", Justification = "Cache exception so user can examine it later")]
            private IEnumerable<OperationResponse> HandleBatchResponse(BatchStream batch)
            {
                if (!batch.CanRead)
                {
                    yield break;
                }

                string contentType;
                string location;
                string etag;

                Uri editLink = null;

                HttpStatusCode status;
                int changesetIndex = 0;
                int queryCount = 0;
                int operationCount = 0;
                this.entryIndex = 0;
                while (batch.MoveNext())
                {
                    var contentHeaders = batch.ContentHeaders; 

                    Descriptor entry;
                    switch (batch.State)
                    {
                        #region BeginChangeSet
                        case BatchStreamState.BeginChangeSet:
                            if ((IsFlagSet(this.options, SaveChangesOptions.Batch) && (0 != changesetIndex)) ||
                                (0 != operationCount))
                            {   
                                Error.ThrowBatchUnexpectedContent(InternalError.UnexpectedBeginChangeSet);
                            }

                            break;
                        #endregion

                        #region EndChangeSet
                        case BatchStreamState.EndChangeSet:
                            changesetIndex++;
                            operationCount = 0;
                            break;
                        #endregion

                        #region GetResponse
                        case BatchStreamState.GetResponse:
                            Debug.Assert(0 == operationCount, "missing an EndChangeSet 2");

                            contentHeaders.TryGetValue(XmlConstants.HttpContentType, out contentType);
                            status = (HttpStatusCode)(-1);

                            Exception ex = null;
                            QueryOperationResponse qresponse = null;
                            try
                            {
                                status = batch.GetStatusCode();

                                ex = HandleResponse(status, batch.GetResponseVersion(), batch.GetContentStream, false);
                                if (null == ex)
                                {
                                    DataServiceRequest query = this.Queries[queryCount];
                                    MaterializeAtom materializer = DataServiceRequest.Materialize(this.Context, query.QueryComponents, null, contentType, batch.GetContentStream());
                                    qresponse = QueryOperationResponse.GetInstance(query.ElementType, contentHeaders, query, materializer);
                                }
                            }
                            catch (ArgumentException e)
                            {
                                ex = e;
                            }
                            catch (FormatException e)
                            {
                                ex = e;
                            }
                            catch (InvalidOperationException e)
                            {
                                ex = e;
                            }

                            if (null == qresponse)
                            {
                                if (null != this.Queries)
                                {
                                     DataServiceRequest query = this.Queries[queryCount];

                                    if (this.Context.ignoreResourceNotFoundException && status == HttpStatusCode.NotFound)
                                    {
                                        qresponse = QueryOperationResponse.GetInstance(query.ElementType, contentHeaders, query, MaterializeAtom.EmptyResults);
                                    }
                                    else
                                    {
                                        qresponse = QueryOperationResponse.GetInstance(query.ElementType, contentHeaders, query, MaterializeAtom.EmptyResults);
                                        qresponse.Error = ex;
                                    }
                                }
                                else
                                {
                                   throw ex;
                                }
                            }

                            qresponse.StatusCode = (int)status;
                            queryCount++;
                            yield return qresponse;
                            break;
                        #endregion

                        #region ChangeResponse
                        case BatchStreamState.ChangeResponse:

                            HttpStatusCode statusCode = batch.GetStatusCode();
                            Exception error = HandleResponse(statusCode, batch.GetResponseVersion(), batch.GetContentStream, false);
                            int index = this.ValidateContentID(contentHeaders);

                            try
                            {
                                entry = this.ChangedEntries[index];
                                operationCount += this.Context.SaveResultProcessed(entry);

                                if (null != error)
                                {
                                    throw error;
                                }

                                StreamStates streamState = StreamStates.NoStream;
                                if (entry.IsResource)
                                {
                                    EntityDescriptor descriptor = (EntityDescriptor)entry;
                                    streamState = descriptor.StreamState;
#if DEBUG
                                    if (descriptor.StreamState == StreamStates.Added)
                                    {
                                        Debug.Assert(
                                            statusCode == HttpStatusCode.Created && entry.State == EntityStates.Modified && descriptor.IsMediaLinkEntry,
                                            "statusCode == HttpStatusCode.Created && entry.State == EntityStates.Modified && descriptor.IsMediaLinkEntry -- Processing Post MR");
                                    }
                                    else if (descriptor.StreamState == StreamStates.Modified)
                                    {
                                        Debug.Assert(
                                            statusCode == HttpStatusCode.NoContent && descriptor.IsMediaLinkEntry,
                                            "statusCode == HttpStatusCode.NoContent && descriptor.IsMediaLinkEntry -- Processing Put MR");
                                    }
#endif
                                }

                                if (streamState == StreamStates.Added || entry.State == EntityStates.Added)
                                {
                                    #region Post
                                    if (entry.IsResource)
                                    {
                                        string mime = null;
                                        Encoding postEncoding = null;
                                        contentHeaders.TryGetValue(XmlConstants.HttpContentType, out contentType);
                                        contentHeaders.TryGetValue(XmlConstants.HttpResponseLocation, out location);
                                        contentHeaders.TryGetValue(XmlConstants.HttpResponseETag, out etag);
                                        EntityDescriptor entityDescriptor = (EntityDescriptor)entry;

                                        if (location != null)
                                        {
                                            editLink = Util.CreateUri(location, UriKind.Absolute);
                                        }
                                        else
                                        {
                                            throw Error.NotSupported(Strings.Deserialize_NoLocationHeader);
                                        }

                                        Stream stream = batch.GetContentStream();
                                        if (null != stream)
                                        {
                                            HttpProcessUtility.ReadContentType(contentType, out mime, out postEncoding);
                                            if (!String.Equals(XmlConstants.MimeApplicationAtom, mime, StringComparison.OrdinalIgnoreCase))
                                            {
                                                throw Error.InvalidOperation(Strings.Deserialize_UnknownMimeTypeSpecified(mime));
                                            }

                                            XmlReader reader = XmlUtil.CreateXmlReader(stream, postEncoding);
                                            QueryComponents qc = new QueryComponents(null, Util.DataServiceVersionEmpty, entityDescriptor.Entity.GetType(), null, null);
                                            EntityDescriptor descriptor = (EntityDescriptor)entry;
                                            MergeOption mergeOption = MergeOption.OverwriteChanges;

                                            if (descriptor.StreamState == StreamStates.Added)
                                            {
                                                mergeOption = MergeOption.PreserveChanges;
                                                Debug.Assert(descriptor.State == EntityStates.Modified, "The MLE state must be Modified.");
                                            }

                                            try
                                            {
                                                using (MaterializeAtom atom = new MaterializeAtom(this.Context, reader, qc, null, mergeOption))
                                                {
                                                    this.Context.HandleResponsePost(entityDescriptor, atom, editLink, etag);
                                                }
                                            }
                                            finally
                                            {
                                                if (descriptor.StreamState == StreamStates.Added)
                                                {
                                                   Debug.Assert(descriptor.State == EntityStates.Unchanged, "The materializer should always set the entity state to Unchanged.");
                                                    descriptor.State = EntityStates.Modified;

                                                    descriptor.StreamState = StreamStates.NoStream;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            this.Context.HandleResponsePost(entityDescriptor, null, editLink, etag);
                                        }
                                    }
                                    else
                                    {
                                        HandleResponsePost((LinkDescriptor)entry);
                                    }
                                    #endregion
                                }
                                else if (streamState == StreamStates.Modified || entry.State == EntityStates.Modified)
                                {
                                    #region Put, Merge
                                    contentHeaders.TryGetValue(XmlConstants.HttpResponseETag, out etag);
                                    HandleResponsePut(entry, etag);
                                    #endregion
                                }
                                else if (entry.State == EntityStates.Deleted)
                                {
                                    #region Delete
                                    this.Context.HandleResponseDelete(entry);
                                    #endregion
                                }

                           }
                            catch (Exception e)
                            {
                                this.ChangedEntries[index].SaveError = e;
                                error = e;
                            }

                            ChangeOperationResponse changeOperationResponse = 
                                new ChangeOperationResponse(contentHeaders, this.ChangedEntries[index]);
                            changeOperationResponse.StatusCode = (int)statusCode;
                            if (error != null)
                            {
                                changeOperationResponse.Error = error;
                            }

                            this.Responses.Add(changeOperationResponse);
                            operationCount++;
                            this.entryIndex++;
                            yield return changeOperationResponse;
                            break;
                        #endregion

                        default:
                            Error.ThrowBatchExpectedResponse(InternalError.UnexpectedBatchState);
                            break;
                    }
                }

                Debug.Assert(batch.State == BatchStreamState.EndBatch, "unexpected batch state");

               if ((null == this.Queries && 
                    (0 == changesetIndex || 
                     0 < queryCount || 
                     this.ChangedEntries.Any(o => o.ContentGeneratedForSave && 0 == o.SaveResultWasProcessed) &&
                     (!IsFlagSet(this.options, SaveChangesOptions.Batch) || null == this.ChangedEntries.FirstOrDefault(o => null != o.SaveError)))) ||
                    (null != this.Queries && queryCount != this.Queries.Length))
                {
                    throw Error.InvalidOperation(Strings.Batch_IncompleteResponseCount);
                }

                batch.Dispose();
            }

           private int ValidateContentID(Dictionary<string, string> contentHeaders)
            {
                int contentID = 0;
                string contentValueID;

                if (!contentHeaders.TryGetValue(XmlConstants.HttpContentID, out contentValueID) ||
                    !Int32.TryParse(contentValueID, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out contentID))
                {
                    Error.ThrowBatchUnexpectedContent(InternalError.ChangeResponseMissingContentID);
                }

                for (int i = 0; i < this.ChangedEntries.Count; ++i)
                {
                    if (this.ChangedEntries[i].ChangeOrder == contentID)
                    {
                        return i;
                    }
                }

                Error.ThrowBatchUnexpectedContent(InternalError.ChangeResponseUnknownContentID);
                return -1;
            }

            #endregion Batch

            #region callback handlers

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "required for this feature")]
            private void AsyncEndGetRequestStream(IAsyncResult asyncResult)
            {
                Debug.Assert(asyncResult != null && asyncResult.IsCompleted, "asyncResult.IsCompleted");
                PerRequest pereq = asyncResult == null ? null : asyncResult.AsyncState as PerRequest;
                try
                {
                    CompleteCheck(pereq, InternalError.InvalidEndGetRequestCompleted);
                    pereq.RequestCompletedSynchronously &= asyncResult.CompletedSynchronously; 

                    EqualRefCheck(this.request, pereq, InternalError.InvalidEndGetRequestStream);
                    HttpWebRequest httpWebRequest = Util.NullCheck(pereq.Request, InternalError.InvalidEndGetRequestStreamRequest);

                    Stream stream = Util.NullCheck(httpWebRequest.EndGetRequestStream(asyncResult), InternalError.InvalidEndGetRequestStreamStream);
                    pereq.RequestStream = stream;

                    PerRequest.ContentStream contentStream = pereq.RequestContentStream;
                    Util.NullCheck(contentStream, InternalError.InvalidEndGetRequestStreamContent);
                    Util.NullCheck(contentStream.Stream, InternalError.InvalidEndGetRequestStreamContent);
                    if (contentStream.IsKnownMemoryStream)
                    {
                        MemoryStream memoryStream = contentStream.Stream as MemoryStream;
                        byte[] buffer = memoryStream.GetBuffer();
                        int bufferOffset = checked((int)memoryStream.Position);
                        int bufferLength = checked((int)memoryStream.Length) - bufferOffset;
                        if ((null == buffer) || (0 == bufferLength))
                        {
                            Error.ThrowInternalError(InternalError.InvalidEndGetRequestStreamContentLength);
                        }
                    }

                    pereq.RequestContentBufferValidLength = -1;

                    Util.DebugInjectFault("SaveAsyncResult::AsyncEndGetRequestStream_BeforeBeginRead");
                    asyncResult = BaseAsyncResult.InvokeAsync(contentStream.Stream.BeginRead, pereq.RequestContentBuffer, 0, pereq.RequestContentBuffer.Length, this.AsyncRequestContentEndRead, pereq);
                    pereq.RequestCompletedSynchronously &= asyncResult.CompletedSynchronously;
                }
                catch (Exception e)
                {
                    if (this.HandleFailure(pereq, e))
                    {
                        throw;
                    }
                }
                finally
                {
                    this.HandleCompleted(pereq);
                }
            }

            private void AsyncRequestContentEndRead(IAsyncResult asyncResult)
            {
                Debug.Assert(asyncResult != null && asyncResult.IsCompleted, "asyncResult.IsCompleted");
                PerRequest pereq = asyncResult == null ? null : asyncResult.AsyncState as PerRequest;
                try
                {
                    CompleteCheck(pereq, InternalError.InvalidEndReadCompleted);
                    pereq.RequestCompletedSynchronously &= asyncResult.CompletedSynchronously; 

                    EqualRefCheck(this.request, pereq, InternalError.InvalidEndRead);
                    PerRequest.ContentStream contentStream = pereq.RequestContentStream;
                    Util.NullCheck(contentStream, InternalError.InvalidEndReadStream);
                    Util.NullCheck(contentStream.Stream, InternalError.InvalidEndReadStream);
                    Stream stream = Util.NullCheck(pereq.RequestStream, InternalError.InvalidEndReadStream);

                    Util.DebugInjectFault("SaveAsyncResult::AsyncRequestContentEndRead_BeforeEndRead");
                    int count = contentStream.Stream.EndRead(asyncResult);
                    if (0 < count)
                    {
                        bool firstEndRead = (pereq.RequestContentBufferValidLength == -1);
                        pereq.RequestContentBufferValidLength = count;

                        if (!asyncResult.CompletedSynchronously || firstEndRead)
                        {
                            do
                            {
                                Util.DebugInjectFault("SaveAsyncResult::AsyncRequestContentEndRead_BeforeBeginWrite");
                                asyncResult = BaseAsyncResult.InvokeAsync(stream.BeginWrite, pereq.RequestContentBuffer, 0, pereq.RequestContentBufferValidLength, this.AsyncEndWrite, pereq);
                                pereq.RequestCompletedSynchronously &= asyncResult.CompletedSynchronously;

                                if (asyncResult.CompletedSynchronously && !pereq.RequestCompleted && !this.IsCompletedInternally)
                                {
                                    Util.DebugInjectFault("SaveAsyncResult::AsyncRequestContentEndRead_BeforeBeginRead");
                                    asyncResult = BaseAsyncResult.InvokeAsync(contentStream.Stream.BeginRead, pereq.RequestContentBuffer, 0, pereq.RequestContentBuffer.Length, this.AsyncRequestContentEndRead, pereq);
                                    pereq.RequestCompletedSynchronously &= asyncResult.CompletedSynchronously;
                                }


                            }
                            while (asyncResult.CompletedSynchronously && !pereq.RequestCompleted && !this.IsCompletedInternally &&
                                pereq.RequestContentBufferValidLength > 0);
                        }
                    }
                    else
                    {
                        pereq.RequestContentBufferValidLength = 0;
                        pereq.RequestStream = null;
                        stream.Close();

                        HttpWebRequest httpWebRequest = Util.NullCheck(pereq.Request, InternalError.InvalidEndWriteRequest);
                        asyncResult = BaseAsyncResult.InvokeAsync(httpWebRequest.BeginGetResponse, this.AsyncEndGetResponse, pereq);
                        pereq.RequestCompletedSynchronously &= asyncResult.CompletedSynchronously; 
                    }
                }
                catch (Exception e)
                {
                    if (this.HandleFailure(pereq, e))
                    {
                        throw;
                    }
                }
                finally
                {
                    this.HandleCompleted(pereq);
                }
            }
                        
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "required for this feature")]
            private void AsyncEndWrite(IAsyncResult asyncResult)
            {
                Debug.Assert(asyncResult != null && asyncResult.IsCompleted, "asyncResult.IsCompleted");
                PerRequest pereq = asyncResult == null ? null : asyncResult.AsyncState as PerRequest;
                try
                {
                    CompleteCheck(pereq, InternalError.InvalidEndWriteCompleted);
                    pereq.RequestCompletedSynchronously &= asyncResult.CompletedSynchronously;

                    EqualRefCheck(this.request, pereq, InternalError.InvalidEndWrite);

                    PerRequest.ContentStream contentStream = pereq.RequestContentStream;
                    Util.NullCheck(contentStream, InternalError.InvalidEndWriteStream);
                    Util.NullCheck(contentStream.Stream, InternalError.InvalidEndWriteStream);
                    Stream stream = Util.NullCheck(pereq.RequestStream, InternalError.InvalidEndWriteStream);
                    Util.DebugInjectFault("SaveAsyncResult::AsyncEndWrite_BeforeEndWrite");
                    stream.EndWrite(asyncResult);

                   if (!asyncResult.CompletedSynchronously)
                    {
                        Util.DebugInjectFault("SaveAsyncResult::AsyncEndWrite_BeforeBeginRead");
                        asyncResult = BaseAsyncResult.InvokeAsync(contentStream.Stream.BeginRead, pereq.RequestContentBuffer, 0, pereq.RequestContentBuffer.Length, this.AsyncRequestContentEndRead, pereq);
                        pereq.RequestCompletedSynchronously &= asyncResult.CompletedSynchronously;
                    }
                }
                catch (Exception e)
                {
                    if (this.HandleFailure(pereq, e))
                    {
                        throw;
                    }
                }
                finally
                {
                    this.HandleCompleted(pereq);
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "required for this feature")]
            private void AsyncEndGetResponse(IAsyncResult asyncResult)
            {
                Debug.Assert(asyncResult != null && asyncResult.IsCompleted, "asyncResult.IsCompleted");
                PerRequest pereq = asyncResult == null ? null : asyncResult.AsyncState as PerRequest;
                try
                {
                    CompleteCheck(pereq, InternalError.InvalidEndGetResponseCompleted);
                    pereq.RequestCompletedSynchronously &= asyncResult.CompletedSynchronously; 

                    EqualRefCheck(this.request, pereq, InternalError.InvalidEndGetResponse);
                    HttpWebRequest httpWebRequest = Util.NullCheck(pereq.Request, InternalError.InvalidEndGetResponseRequest);

                    HttpWebResponse response = null;
                    try
                    {
                        Util.DebugInjectFault("SaveAsyncResult::AsyncEndGetResponse::BeforeEndGetResponse");
                        response = (HttpWebResponse)httpWebRequest.EndGetResponse(asyncResult);
                    }
                    catch (WebException e)
                    {
                        response = (HttpWebResponse)e.Response;
                        if (null == response)
                        {
                            throw;
                        }
                    }

                    pereq.HttpWebResponse = Util.NullCheck(response, InternalError.InvalidEndGetResponseResponse);

                    if (!IsFlagSet(this.options, SaveChangesOptions.Batch))
                    {
                        this.HandleOperationResponse(response);
                    }

                    this.copiedContentLength = 0;
                    Util.DebugInjectFault("SaveAsyncResult::AsyncEndGetResponse_BeforeGetStream");
                    Stream stream = response.GetResponseStream();
                    pereq.ResponseStream = stream;
                    if ((null != stream) && stream.CanRead)
                    {
                        if (null != this.buildBatchWriter)
                        {
                            this.buildBatchWriter.Flush();
                        }

                        if (null == this.buildBatchBuffer)
                        {
                            this.buildBatchBuffer = new byte[8000];
                        }

                        do
                        {
                            Util.DebugInjectFault("SaveAsyncResult::AsyncEndGetResponse_BeforeBeginRead");
                            asyncResult = BaseAsyncResult.InvokeAsync(stream.BeginRead, this.buildBatchBuffer, 0, this.buildBatchBuffer.Length, this.AsyncEndRead, pereq);
                            pereq.RequestCompletedSynchronously &= asyncResult.CompletedSynchronously; 
                        }
                        while (asyncResult.CompletedSynchronously && !pereq.RequestCompleted && !this.IsCompletedInternally && stream.CanRead);
                    }
                    else
                    {
                        pereq.SetComplete();

                        if (!this.IsCompletedInternally)
                        {
                            this.SaveNextChange(pereq);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (this.HandleFailure(pereq, e))
                    {
                        throw;
                    }
                }
                finally
                {
                    this.HandleCompleted(pereq);
                }
            }

           [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "required for this feature")]
            private void AsyncEndRead(IAsyncResult asyncResult)
            {
                Debug.Assert(asyncResult != null && asyncResult.IsCompleted, "asyncResult.IsCompleted");
                PerRequest pereq = asyncResult.AsyncState as PerRequest;
                int count = 0;
                try
                {
                    CompleteCheck(pereq, InternalError.InvalidEndReadCompleted);
                    pereq.RequestCompletedSynchronously &= asyncResult.CompletedSynchronously; 

                    EqualRefCheck(this.request, pereq, InternalError.InvalidEndRead);
                    Stream stream = Util.NullCheck(pereq.ResponseStream, InternalError.InvalidEndReadStream);

                    Util.DebugInjectFault("SaveAsyncResult::AsyncEndRead_BeforeEndRead");
                    count = stream.EndRead(asyncResult);
                    if (0 < count)
                    {
                        Stream outputResponse = Util.NullCheck(this.httpWebResponseStream, InternalError.InvalidEndReadCopy);
                        outputResponse.Write(this.buildBatchBuffer, 0, count);
                        this.copiedContentLength += count;

                        if (!asyncResult.CompletedSynchronously && stream.CanRead)
                        {   
                             do
                            {
                                asyncResult = BaseAsyncResult.InvokeAsync(stream.BeginRead, this.buildBatchBuffer, 0, this.buildBatchBuffer.Length, this.AsyncEndRead, pereq);
                                pereq.RequestCompletedSynchronously &= asyncResult.CompletedSynchronously; 
                            }
                            while (asyncResult.CompletedSynchronously && !pereq.RequestCompleted && !this.IsCompletedInternally && stream.CanRead);
                        }
                    }
                    else
                    {
                        pereq.SetComplete();

                        if (!this.IsCompletedInternally)
                        {
                            this.SaveNextChange(pereq);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (this.HandleFailure(pereq, e))
                    {
                        throw;
                    }
                }
                finally
                {
                    this.HandleCompleted(pereq);
                }
            }

            private void SaveNextChange(PerRequest pereq)
            {
                Debug.Assert(this.executeAsync, "should be async");
                if (!pereq.RequestCompleted)
                {
                    Error.ThrowInternalError(InternalError.SaveNextChangeIncomplete);
                }

                EqualRefCheck(this.request, pereq, InternalError.InvalidSaveNextChange);

                if (IsFlagSet(this.options, SaveChangesOptions.Batch))
                {
                    this.httpWebResponseStream.Position = 0;
                    this.request = null;
                    this.SetCompleted();
                }
                else
                {
                    if (0 == this.copiedContentLength)
                    {
                        this.HandleOperationResponseNoData();
                    }

                    this.HandleOperationEnd();

                    if (!this.processingMediaLinkEntry)
                    {
                        this.changesCompleted++;
                    }

                    pereq.Dispose();
                    this.request = null;
                    if (!pereq.RequestCompletedSynchronously)
                    {   
                        if (!this.IsCompletedInternally)
                        {
                            this.BeginNextChange(IsFlagSet(this.options, SaveChangesOptions.ReplaceOnUpdate));
                        }
                    }
                }
            }
            #endregion

            private sealed class PerRequest
            {
                private int requestStatus;

                private byte[] requestContentBuffer;

                internal PerRequest()
                {
                    this.RequestCompletedSynchronously = true;
                }

                internal HttpWebRequest Request
                {
                    get;
                    set;
                }

                internal Stream RequestStream
                {
                    get;
                    set;
                }

                internal ContentStream RequestContentStream
                {
                    get;
                    set;
                }

                internal HttpWebResponse HttpWebResponse
                {
                    get;
                    set;
                }

                internal Stream ResponseStream
                {
                    get;
                    set;
                }

                internal bool RequestCompletedSynchronously
                {
                    get;
                    set;
                }

                internal bool RequestCompleted
                {
                    get { return this.requestStatus != 0; }
                }

                internal bool RequestAborted
                {
                    get { return this.requestStatus == 2; }
                }

                internal byte[] RequestContentBuffer
                {
                    get
                    {
                        if (this.requestContentBuffer == null)
                        {
                            this.requestContentBuffer = new byte[64 * 1024];
                        }

                        return this.requestContentBuffer;
                    }
                }

                internal int RequestContentBufferValidLength
                {
                    get;
                    set;
                }

                internal void SetComplete()
                {
                    System.Threading.Interlocked.CompareExchange(ref this.requestStatus, 1, 0);
                }

                internal void SetAborted()
                {
                    System.Threading.Interlocked.Exchange(ref this.requestStatus, 2);
                }
                
                internal void Dispose()
                {
                    Stream stream = null;

                    if (null != (stream = this.ResponseStream))
                    {
                        this.ResponseStream = null;
                        stream.Dispose();
                    }

                    if (null != this.RequestContentStream)
                    {
                        if (this.RequestContentStream.Stream != null && this.RequestContentStream.IsKnownMemoryStream)
                        {
                            this.RequestContentStream.Stream.Dispose();
                        }

                        this.RequestContentStream = null;
                    }
                    
                    if (null != (stream = this.RequestStream))
                    {
                        this.RequestStream = null;
                        try
                        {
                            Util.DebugInjectFault("PerRequest::Dispose_BeforeRequestStreamDisposed");
                            stream.Dispose();
                        }
                        catch (WebException)
                        {
                            if (!this.RequestAborted)
                            {
                                throw;
                            }

                             Util.DebugInjectFault("PerRequest::Dispose_WebExceptionThrown");
                        }
                    }

                    HttpWebResponse response = this.HttpWebResponse;
                    if (null != response)
                    {
                        response.Close();
                    }

                    this.Request = null;
                    this.SetComplete();
                }

                internal class ContentStream
                {
                     private readonly Stream stream;

                    private readonly bool isKnownMemoryStream;

                   public ContentStream(Stream stream, bool isKnownMemoryStream)
                    {
                        this.stream = stream;
                        this.isKnownMemoryStream = isKnownMemoryStream;
                    }

                   public Stream Stream
                    {
                        get { return this.stream; }
                    }

                   public bool IsKnownMemoryStream
                    {
                        get { return this.isKnownMemoryStream; }
                    }
                }
            }
        }
    }
}
