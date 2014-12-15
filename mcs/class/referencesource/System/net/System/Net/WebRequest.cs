//------------------------------------------------------------------------------
// <copyright file="WebRequest.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Diagnostics.Tracing;
    using System.Globalization;
    using System.IO;
    using System.Net.Cache;
    using System.Net.Configuration;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net.Security;
    using System.ComponentModel;
    using System.Security;
    //
    // WebRequest - the base class of all Web resource/protocol objects. Provides
    // common methods, data and proprties for making the top level request
    //

    /// <devdoc>
    ///    <para>A request to a Uniform Resource Identifier (Uri). This
    ///       is an abstract class.</para>
    /// </devdoc>
    [Serializable]
    public abstract class WebRequest : MarshalByRefObject, ISerializable
    {
#if FEATURE_PAL // ROTORTODO - after speed ups (like real JIT and GC) remove this one
#if DEBUG
        internal const int DefaultTimeout = 100000 * 10;
#else // DEBUG
        internal const int DefaultTimeout = 100000 * 5;
#endif // DEBUG
#else // FEATURE_PAL
        internal const int DefaultTimeout = 100000; // default timeout is 100 seconds (ASP .NET is 90 seconds)
#endif // FEATURE_PAL
        private static volatile ArrayList s_PrefixList;
        private static Object s_InternalSyncObject;
        private static TimerThread.Queue s_DefaultTimerQueue = TimerThread.CreateQueue(DefaultTimeout);

#if !FEATURE_PAL
        private  AuthenticationLevel m_AuthenticationLevel;
        private  TokenImpersonationLevel m_ImpersonationLevel;
#endif
        private RequestCachePolicy      m_CachePolicy;
        private RequestCacheProtocol    m_CacheProtocol;
        private RequestCacheBinding     m_CacheBinding;

#region designer support for System.Windows.dll
        internal class DesignerWebRequestCreate : IWebRequestCreate
        {
            public WebRequest Create(Uri uri)
            {
                return WebRequest.Create(uri);
            }
        }
        private static DesignerWebRequestCreate webRequestCreate = new DesignerWebRequestCreate();
        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual IWebRequestCreate CreatorInstance { get { return webRequestCreate; } }

        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void RegisterPortableWebRequestCreator(IWebRequestCreate creator) { }       
#endregion        

        private static Object InternalSyncObject {
            get {
                if (s_InternalSyncObject == null) {
                    Object o = new Object();
                    Interlocked.CompareExchange(ref s_InternalSyncObject, o, null);
                }
                return s_InternalSyncObject;
            }
        }

        internal static TimerThread.Queue DefaultTimerQueue {
            get {
                return s_DefaultTimerQueue;
            }
        }

        /*++

            Create - Create a WebRequest.

            This is the main creation routine. We take a Uri object, look
            up the Uri in the prefix match table, and invoke the appropriate
            handler to create the object. We also have a parameter that
            tells us whether or not to use the whole Uri or just the
            scheme portion of it.

            Input:

                RequestUri          - Uri object for request.
                UseUriBase          - True if we're only to look at the scheme
                                        portion of the Uri.

            Returns:

                Newly created WebRequest.
        --*/

        private static WebRequest Create(Uri requestUri, bool useUriBase) {
            if(Logging.On)Logging.Enter(Logging.Web, "WebRequest", "Create", requestUri.ToString());

            string LookupUri;
            WebRequestPrefixElement Current = null;
            bool Found = false;

            if (!useUriBase)
            {
                LookupUri = requestUri.AbsoluteUri;
            }
            else {

                //
                // schemes are registered as <schemeName>":", so add the separator
                // to the string returned from the Uri object
                //

                LookupUri = requestUri.Scheme + ':';
            }

            int LookupLength = LookupUri.Length;

            // Copy the prefix list so that if it is updated it will
            // not affect us on this thread.

            ArrayList prefixList = PrefixList;

            // Look for the longest matching prefix.

            // Walk down the list of prefixes. The prefixes are kept longest
            // first. When we find a prefix that is shorter or the same size
            // as this Uri, we'll do a compare to see if they match. If they
            // do we'll break out of the loop and call the creator.

            for (int i = 0; i < prefixList.Count; i++) {
                Current = (WebRequestPrefixElement)prefixList[i];

                //
                // See if this prefix is short enough.
                //

                if (LookupLength >= Current.Prefix.Length) {

                    //
                    // It is. See if these match.
                    //

                    if (String.Compare(Current.Prefix,
                                       0,
                                       LookupUri,
                                       0,
                                       Current.Prefix.Length,
                                       StringComparison.OrdinalIgnoreCase ) == 0) {

                        //
                        // These match. Remember that we found it and break
                        // out.
                        //

                        Found = true;
                        break;
                    }
                }
            }

            WebRequest webRequest = null;

            if (Found) {

                //
                // We found a match, so just call the creator and return what it
                // does.
                //

                webRequest = Current.Creator.Create(requestUri);
                if(Logging.On)Logging.Exit(Logging.Web, "WebRequest", "Create", webRequest);
                return webRequest;
            }

            if(Logging.On)Logging.Exit(Logging.Web, "WebRequest", "Create", null);

            //
            // Otherwise no match, throw an exception.
            //

            throw new NotSupportedException(SR.GetString(SR.net_unknown_prefix));
        }

        /*++

            Create - Create a WebRequest.

            An overloaded utility version of the real Create that takes a
            string instead of an Uri object.


            Input:

                RequestString       - Uri string to create.

            Returns:

                Newly created WebRequest.
        --*/

        /// <devdoc>
        ///    <para>
        ///       Creates a new <see cref='System.Net.WebRequest'/>
        ///       instance for
        ///       the specified Uri scheme.
        ///    </para>
        /// </devdoc>
        public static WebRequest Create(string requestUriString) {
            if (requestUriString == null) {
                throw new ArgumentNullException("requestUriString");
            }
            // In .NET FX v4.0, custom IWebRequestCreate implementations can 
            // cause this to return null.  Consider tightening this in the future.
            //Contract.Ensures(Contract.Result<WebRequest>() != null);

            return Create(new Uri(requestUriString), false);
        }

        /*++

            Create - Create a WebRequest.

            Another overloaded version of the Create function that doesn't
            take the UseUriBase parameter.

            Input:

                RequestUri          - Uri object for request.

            Returns:

                Newly created WebRequest.
        --*/

        /// <devdoc>
        ///    <para>
        ///       Creates a new <see cref='System.Net.WebRequest'/> instance for the specified Uri scheme.
        ///    </para>
        /// </devdoc>
        public static WebRequest Create(Uri requestUri) {
            if (requestUri == null) {
                throw new ArgumentNullException("requestUri");
            }
            // In .NET FX v4.0, custom IWebRequestCreate implementations can 
            // cause this to return null.  Consider tightening this in the future.
            //Contract.Ensures(Contract.Result<WebRequest>() != null);

            return Create(requestUri, false);
        }

        /*++

            CreateDefault - Create a default WebRequest.

            This is the creation routine that creates a default WebRequest.
            We take a Uri object and pass it to the base create routine,
            setting the useUriBase parameter to true.

            Input:

                RequestUri          - Uri object for request.

            Returns:

                Newly created WebRequest.
        --*/
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static WebRequest CreateDefault(Uri requestUri) {
            if (requestUri == null) {
                throw new ArgumentNullException("requestUri");
            }
            // In .NET FX v4.0, custom IWebRequestCreate implementations can 
            // cause this to return null.  Consider tightening this in the future.
            //Contract.Ensures(Contract.Result<WebRequest>() != null);

            return Create(requestUri, true);
        }
        
        // For portability
        public static HttpWebRequest CreateHttp(string requestUriString) {
            if (requestUriString == null) {
                throw new ArgumentNullException("requestUriString");
            }
            return CreateHttp(new Uri(requestUriString));
        }

        // For portability
        public static HttpWebRequest CreateHttp(Uri requestUri) {
            if (requestUri == null) {
                throw new ArgumentNullException("requestUri");
            }
            if ((requestUri.Scheme != Uri.UriSchemeHttp) && (requestUri.Scheme != Uri.UriSchemeHttps)) {
                throw new NotSupportedException(SR.GetString(SR.net_unknown_prefix));
            }
            return (HttpWebRequest)CreateDefault(requestUri);
        }

        /*++

            RegisterPrefix - Register an Uri prefix for creating WebRequests.

            This function registers a prefix for creating WebRequests. When an
            user wants to create a WebRequest, we scan a table looking for a
            longest prefix match for the Uri they're passing. We then invoke
            the sub creator for that prefix. This function puts entries in
            that table.

            We don't allow duplicate entries, so if there is a dup this call
            will fail.

        Input:

            Prefix           - Represents Uri prefix being registered.
            Creator         - Interface for sub creator.

        Returns:

            True if the registration worked, false otherwise.

        --*/
        /// <devdoc>
        ///    <para>
        ///       Registers a <see cref='System.Net.WebRequest'/> descendent
        ///       for a specific Uniform Resource Identifier.
        ///    </para>
        /// </devdoc>
        public static bool RegisterPrefix(string prefix, IWebRequestCreate creator) {

            bool Error = false;
            int i;
            WebRequestPrefixElement Current;

            if (prefix == null) {
                throw new ArgumentNullException("prefix");
            }
            if (creator == null) {
                throw new ArgumentNullException("creator");
            }

            ExceptionHelper.WebPermissionUnrestricted.Demand();

            // Lock this object, then walk down PrefixList looking for a place to
            // to insert this prefix.

            lock(InternalSyncObject) {
                //
                // clone the object and update the clone thus
                // allowing other threads to still read from it
                //

                ArrayList prefixList = (ArrayList)PrefixList.Clone();

                // As AbsoluteUri is used later for Create, account for formating changes 
                // like Unicode escaping, default ports, etc.
                Uri tempUri;
                if (Uri.TryCreate(prefix, UriKind.Absolute, out tempUri))
                {
                    String cookedUri = tempUri.AbsoluteUri;

                    // Special case for when a partial host matching is requested, drop the added trailing slash
                    // IE: http://host could match host or host.domain
                    if (!prefix.EndsWith("/", StringComparison.Ordinal) 
                        && tempUri.GetComponents(UriComponents.PathAndQuery | UriComponents.Fragment, 
                            UriFormat.UriEscaped)
                            .Equals("/"))
                        cookedUri = cookedUri.Substring(0, cookedUri.Length - 1);

                    prefix = cookedUri;
                }

                i = 0;

                // The prefix list is sorted with longest entries at the front. We
                // walk down the list until we find a prefix shorter than this
                // one, then we insert in front of it. Along the way we check
                // equal length prefixes to make sure this isn't a dupe.

                while (i < prefixList.Count) {
                    Current = (WebRequestPrefixElement)prefixList[i];

                    // See if the new one is longer than the one we're looking at.

                    if (prefix.Length > Current.Prefix.Length) {
                        // It is. Break out of the loop here.
                        break;
                    }

                    // If these are of equal length, compare them.

                    if (prefix.Length == Current.Prefix.Length) {
                        // They're the same length.
                        if (String.Compare(Current.Prefix, prefix, StringComparison.OrdinalIgnoreCase) == 0) {
                            // ...and the strings are identical. This is an error.

                            Error = true;
                            break;
                        }
                    }
                    i++;
                }

                // When we get here either i contains the index to insert at or
                // we've had an error, in which case Error is true.

                if (!Error) {
                    // No error, so insert.

                    prefixList.Insert(i,
                                        new WebRequestPrefixElement(prefix, creator)
                                       );

                    //
                    // no error, assign the clone to the static object, other
                    // threads using it will have copied the oriignal object already
                    //
                    PrefixList = prefixList;
                }
            }
            return !Error;
        }

        /*
        public static bool UnregisterPrefix(string prefix) {
            if (prefix == null) {
                throw new ArgumentNullException("prefix");
            }
            ExceptionHelper.WebPermissionUnrestricted.Demand();

            // Lock this object, then walk down PrefixList looking for a place to
            // to insert this prefix.

            lock(InternalSyncObject) {
                //
                // clone the object and update the clone thus
                // allowing other threads to still read from it
                //

                ArrayList prefixList = (ArrayList) PrefixList.Clone();

                int i = 0;
                WebRequestPrefixElement Current;

                // The prefix list is sorted with longest entries at the front. We
                // walk down the list until we find a prefix shorter than this
                // one, then we insert in front of it. Along the way we check
                // equal length prefixes to make sure this isn't a dupe.
                while (i < prefixList.Count) {
                    Current = (WebRequestPrefixElement)prefixList[i];

                    // See if the new one is longer than the one we're looking at.

                    if (prefix.Length > Current.Prefix.Length) {
                        return fasle;
                    }

                    // If these are of equal length, compare them.

                    if (prefix.Length == Current.Prefix.Length) {
                        // They're the same length.
                        if (String.Compare(Current.Prefix, prefix, StringComparison.OrdinalIgnoreCase ) == 0) {
                            prefixList.RemoveAt(i);
                            PrefixList = prefixList;
                            return true;
                        }
                    }
                    i++;
                }
            }

            return false;

        }
        */

        /*++

            PrefixList - Returns And Initialize our prefix list.


            This is the method that initializes the prefix list. We create
            an ArrayList for the PrefixList, then an HttpRequestCreator object,
            and then we register the HTTP and HTTPS prefixes.

            Input: Nothing

            Returns: true

        --*/
        internal static ArrayList PrefixList {

            get {
                //
                // GetConfig() might use us, so we have a circular dependency issue,
                // that causes us to nest here, we grab the lock, only
                // if we haven't initialized.
                //
                if (s_PrefixList == null) {

                    lock (InternalSyncObject) {
                        if (s_PrefixList == null) {
                            GlobalLog.Print("WebRequest::Initialize(): calling ConfigurationManager.GetSection()");
                            s_PrefixList = WebRequestModulesSectionInternal.GetSection().WebRequestModules;
                        }
                    }
                }

                return s_PrefixList;
            }
            set {
                s_PrefixList = value;
            }
        }

        // constructors

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Net.WebRequest'/>
        ///       class.
        ///    </para>
        /// </devdoc>

        protected WebRequest()
        {
#if !FEATURE_PAL
            // Defautl values are set as per V1.0 behavior
            m_ImpersonationLevel = TokenImpersonationLevel.Delegation;
            m_AuthenticationLevel= AuthenticationLevel.MutualAuthRequested;
#endif
        }
        //
        // ISerializable constructor
        //
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected WebRequest(SerializationInfo serializationInfo, StreamingContext streamingContext) {
        }

        //
        // ISerializable method
        //
        /// <internalonly/>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand")]
        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter, SerializationFormatter=true)]
        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            GetObjectData(serializationInfo, streamingContext);
        }

        //
        // FxCop: Provide a way for inherited classes to access base.GetObjectData in case they also implement ISerializable.
        //
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        protected virtual void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
        }

        // This is a shortcut that would set the default policy for HTTP/HTTPS.
        // The default policy is overridden by any prefix-registered policy.
        // Will demand permission for set{}
        public static RequestCachePolicy DefaultCachePolicy {
            get {
                return RequestCacheManager.GetBinding(string.Empty).Policy;
            }
            set {
                // This is a replacement of RequestCachePermission demand since we are not including the latest in the product.
                ExceptionHelper.WebPermissionUnrestricted.Demand();

                RequestCacheBinding binding = RequestCacheManager.GetBinding(string.Empty);
                RequestCacheManager.SetBinding(string.Empty, new RequestCacheBinding(binding.Cache, binding.Validator, value));
            }
        }

        //
        //
        public virtual RequestCachePolicy CachePolicy {
            get {
                return m_CachePolicy;
            }
            set
            {
                // Delayed creation of CacheProtocol until caching is actually turned on.
                InternalSetCachePolicy(value);
            }
        }


        void InternalSetCachePolicy(RequestCachePolicy policy){
            // Delayed creation of CacheProtocol until caching is actually turned on.
            if (m_CacheBinding != null &&
                m_CacheBinding.Cache != null &&
                m_CacheBinding.Validator != null &&
                CacheProtocol == null &&
                policy != null &&
                policy.Level != RequestCacheLevel.BypassCache)
            {
                CacheProtocol = new RequestCacheProtocol(m_CacheBinding.Cache, m_CacheBinding.Validator.CreateValidator());
            }

            m_CachePolicy = policy;
        }


        /// <devdoc>
        ///    <para>When overridden in a derived class, gets and
        ///       sets
        ///       the
        ///       protocol method used in this request. Default value should be
        ///       "GET".</para>
        /// </devdoc>
        public virtual string Method {
            get {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
            set {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }


        /// <devdoc>
        /// <para>When overridden in a derived class, gets a <see cref='Uri'/>
        /// instance representing the resource associated with
        /// the request.</para>
        /// </devdoc>
        public virtual Uri RequestUri {                               // read-only
            get {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        //
        // This is a group of connections that may need to be used for
        //  grouping connecitons.
        //
        /// <devdoc>
        /// </devdoc>
        public virtual string ConnectionGroupName {
            get {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
            set {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }


        /*++

            Headers  - Gets any request specific headers associated
             with this request, this is simply a name/value pair collection

            Input: Nothing.

            Returns: This property returns WebHeaderCollection.

                    read-only

        --*/

        /// <devdoc>
        ///    <para>When overridden in a derived class,
        ///       gets
        ///       a collection of header name-value pairs associated with this
        ///       request.</para>
        /// </devdoc>
        public virtual WebHeaderCollection Headers {
            get {
                Contract.Ensures(Contract.Result<WebHeaderCollection>() != null);
                throw ExceptionHelper.PropertyNotImplementedException;
            }
            set {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }


        /// <devdoc>
        ///    <para>When
        ///       overridden in a derived class, gets
        ///       and sets
        ///       the
        ///       content length of request data being sent.</para>
        /// </devdoc>
        public virtual long ContentLength {
            get {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
            set {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        /// <devdoc>
        ///    <para>When
        ///       overridden in a derived class, gets
        ///       and
        ///       sets
        ///       the content type of the request data being sent.</para>
        /// </devdoc>
        public virtual string ContentType {
            get {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
            set {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        /// <devdoc>
        ///     <para>When overridden in a derived class, gets and sets the network
        ///       credentials used for authentication to this Uri.</para>
        /// </devdoc>
        public virtual ICredentials Credentials {
            get {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
            set {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        /// <devdoc>
        ///    <para>Sets Credentials to CredentialCache.DefaultCredentials</para>
        /// </devdoc>
        public virtual bool UseDefaultCredentials  {
            get {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
            set {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        /// <devdoc>
        ///    <para>When overridden in a derived class,
        ///       gets and set proxy info. </para>
        /// </devdoc>
        public virtual IWebProxy Proxy {
            get {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
            set {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        /// <devdoc>
        ///    <para>When overridden in a derived class,
        ///       enables or disables pre-authentication.</para>
        /// </devdoc>
        public virtual bool PreAuthenticate {
            get {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
            set {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        //
        // Timeout in milliseconds, if request  takes longer
        // than timeout, a WebException is thrown
        //
        //UEUE
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual int Timeout {
            get {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
            set {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }


        /// <devdoc>
        ///    <para>When overridden in a derived class,
        ///       returns a <see cref='System.IO.Stream'/> object that is used for writing data
        ///       to the resource identified by <see cref='WebRequest.RequestUri'/>
        ///       .</para>
        /// </devdoc>
        public virtual Stream GetRequestStream() {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        /// <devdoc>
        ///    <para>When overridden in a derived class,
        ///       returns the response
        ///       to an Internet request.</para>
        /// </devdoc>
        public virtual WebResponse GetResponse() {
            Contract.Ensures(Contract.Result<WebResponse>() != null);

            throw ExceptionHelper.MethodNotImplementedException;
        }

        /// <devdoc>
        ///    <para>Asynchronous version of GetResponse.</para>
        /// </devdoc>
        [HostProtection(ExternalThreading=true)]
        public virtual IAsyncResult BeginGetResponse(AsyncCallback callback, object state) {
            throw ExceptionHelper.MethodNotImplementedException;
        }


        /// <devdoc>
        ///    <para>Returns a WebResponse object.</para>
        /// </devdoc>
        public virtual WebResponse EndGetResponse(IAsyncResult asyncResult) {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        /// <devdoc>
        ///    <para>Asynchronous version of GetRequestStream
        ///       method.</para>
        /// </devdoc>
        [HostProtection(ExternalThreading=true)]
        public virtual IAsyncResult BeginGetRequestStream(AsyncCallback callback, Object state) {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        /// <devdoc>
        /// <para>Returns a <see cref='System.IO.Stream'/> object that is used for writing data to the resource
        ///    identified by <see cref='System.Net.WebRequest.RequestUri'/>
        ///    .</para>
        /// </devdoc>
        public virtual Stream EndGetRequestStream(IAsyncResult asyncResult) {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        // Offload to a different thread to avoid blocking the caller durring request submission.
        [HostProtection(ExternalThreading = true)]
        public virtual Task<Stream> GetRequestStreamAsync()
        {
            IWebProxy proxy = null;
            try { proxy = Proxy; }
            catch (NotImplementedException) { }

            // Preserve context for authentication
            if (ExecutionContext.IsFlowSuppressed() 
                && (UseDefaultCredentials || Credentials != null
                    || (proxy != null && proxy.Credentials != null)))
            {
                WindowsIdentity currentUser = SafeCaptureIdenity();

                // When flow is suppressed we would lose track of the current user across this thread switch.
                // Flow manually so that UseDefaultCredentials will work. BeginGetRequestStream will
                // take over from there.
                return Task.Run(() =>
                {
                    using (currentUser)
                    {
                        using (currentUser.Impersonate())
                        {
                            return Task<Stream>.Factory.FromAsync(this.BeginGetRequestStream,
                                this.EndGetRequestStream, null);
                        }
                    }
                });
            }
            else
            {
                return Task.Run(() => Task<Stream>.Factory.FromAsync(this.BeginGetRequestStream, 
                    this.EndGetRequestStream, null));
            }
        }

        // Offload to a different thread to avoid blocking the caller durring request submission.
        [HostProtection(ExternalThreading = true)]
        public virtual Task<WebResponse> GetResponseAsync()
        {
            IWebProxy proxy = null;
            try { proxy = Proxy; }
            catch (NotImplementedException) { }

            // Preserve context for authentication
            if (ExecutionContext.IsFlowSuppressed()
                && (UseDefaultCredentials || Credentials != null
                    || (proxy != null && proxy.Credentials != null)))
            {
                WindowsIdentity currentUser = SafeCaptureIdenity();

                // When flow is suppressed we would lose track of the current user across this thread switch.
                // Flow manually so that UseDefaultCredentials will work. BeginGetResponse will
                // take over from there.
                return Task.Run(() =>
                {
                    using (currentUser)
                    {
                        using (currentUser.Impersonate())
                        {
                            return Task<WebResponse>.Factory.FromAsync(this.BeginGetResponse, 
                                this.EndGetResponse, null);
                        }
                    }
                });
            }
            else
            {
                return Task.Run(() => Task<WebResponse>.Factory.FromAsync(this.BeginGetResponse, 
                    this.EndGetResponse, null));
            }
        }

        // Security: We need an assert for a call into WindowsIdentity.GetCurrent
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.ControlPrincipal)]
        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification = "Needed for identity flow.")]
        private WindowsIdentity SafeCaptureIdenity()
        {
            return WindowsIdentity.GetCurrent();
        }


        /// <summary>
        ///    <para>Aborts the Request</para>
        /// </summary>
        public virtual void Abort() {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        //
        //
        //
        internal RequestCacheProtocol CacheProtocol
        {
            get
            {
                return m_CacheProtocol;
            }
            set
            {
                m_CacheProtocol = value;
            }
        }

#if !FEATURE_PAL
        //
        //
        //
        public AuthenticationLevel AuthenticationLevel {
            get {
                return m_AuthenticationLevel;
            }
            set {
                m_AuthenticationLevel = value;
            }
        }


        // Methods to retrieve the context of the "reading phase" and of the "writing phase" of the request.
        // Each request type can define what goes into what phase.  Typically, the writing phase corresponds to
        // GetRequestStream() and the reading phase to GetResponse(), but if there's no request body, both phases
        // may happen inside GetResponse().
        //
        // Return null only on [....] (if we're on the [....] thread).  Otherwise throw if no context is available.
        internal virtual ContextAwareResult GetConnectingContext()
        {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        internal virtual ContextAwareResult GetWritingContext()
        {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        internal virtual ContextAwareResult GetReadingContext()
        {
            throw ExceptionHelper.MethodNotImplementedException;
        }


        //
        //
        //
        public TokenImpersonationLevel ImpersonationLevel {
            get {
                return m_ImpersonationLevel;
            }
            set {
                m_ImpersonationLevel = value;
            }
        }
#endif  // !FEATURE_PAL

        /// <summary>
        ///    <para>Provides an abstract way of having Async code callback into the request (saves a delegate)</para>
        /// </summary>
        internal virtual void RequestCallback(object obj) {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        //
        // Default Web Proxy implementation.
        //
        private static volatile IWebProxy s_DefaultWebProxy;
        private static volatile bool s_DefaultWebProxyInitialized;

        internal static IWebProxy InternalDefaultWebProxy
        {
            get
            {
                if (!s_DefaultWebProxyInitialized)
                {
                    lock (InternalSyncObject)
                    {
                        if (!s_DefaultWebProxyInitialized)
                        {
                            GlobalLog.Print("WebRequest::get_InternalDefaultWebProxy(): Getting config.");
                            DefaultProxySectionInternal section = DefaultProxySectionInternal.GetSection();
                            if (section != null)
                            {
                                s_DefaultWebProxy = section.WebProxy;
                            }
                            s_DefaultWebProxyInitialized = true;
                        }
                    }
                }
                return s_DefaultWebProxy;
            }

            set
            {
                // Same lock as above.  Avoid hitting config if the proxy is set first.
                if (!s_DefaultWebProxyInitialized)
                {
                    lock (InternalSyncObject)
                    {
                        s_DefaultWebProxy = value;
                        s_DefaultWebProxyInitialized = true;
                    }
                }
                else
                {
                    s_DefaultWebProxy = value;
                }
            }
        }

        //
        // Get and set the global default proxy.  Use this instead of the old GlobalProxySelection.
        //
        public static IWebProxy DefaultWebProxy
        {
            get
            {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                return InternalDefaultWebProxy;
            }

            set
            {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                InternalDefaultWebProxy = value;
            }
        }

        //
        // This returns an "IE Proxy" based on the currently impersonated user's proxy settings.
        //
        public static IWebProxy GetSystemWebProxy()
        {
            ExceptionHelper.WebPermissionUnrestricted.Demand();
            return InternalGetSystemWebProxy();
        }

        internal static IWebProxy InternalGetSystemWebProxy()
        {
            return new WebProxyWrapperOpaque(new WebProxy(true));
        }

        //
        // To maintain backwards-compatibility, GlobalProxySelection.Select must return a proxy castable to WebProxy.
        // To get away from that restriction in the future, new APIs wrap the WebProxy in an internal wrapper.
        // Once Select is removed, the system-proxy functionality can be factored out of the WebProxy class.
        //

        //
        // This doesn't expose the WebProxy.  It's used by GetSystemWebProxy(), which should never be castable to WebProxy.
        //
        internal class WebProxyWrapperOpaque : IAutoWebProxy
        {
            protected readonly WebProxy webProxy;

            internal WebProxyWrapperOpaque(WebProxy webProxy)
            {
                this.webProxy = webProxy;
            }

            public Uri GetProxy(Uri destination)
            {
                return webProxy.GetProxy(destination);
            }

            public bool IsBypassed(Uri host)
            {
                return webProxy.IsBypassed(host);
            }

            public ICredentials Credentials
            {
                get
                {
                    return webProxy.Credentials;
                }

                set
                {
                    webProxy.Credentials = value;
                }
            }

            public ProxyChain GetProxies(Uri destination)
            {
                return ((IAutoWebProxy) webProxy).GetProxies(destination);
            }
        }

        //
        // Select returns the WebProxy out of this one.
        //
        internal class WebProxyWrapper : WebProxyWrapperOpaque
        {
            internal WebProxyWrapper(WebProxy webProxy) :
                base(webProxy)
            { }

            internal WebProxy WebProxy
            {
                get
                {
                    return webProxy;
                }
            }
        }


        //
        internal void SetupCacheProtocol(Uri uri)
        {
            m_CacheBinding = RequestCacheManager.GetBinding(uri.Scheme);

            // Note if the cache is disabled it will give back a bypass policy.
            InternalSetCachePolicy( m_CacheBinding.Policy);
            if (m_CachePolicy == null)
            {
                // If the protocol cache policy is not specifically configured, grab from the base class.
                InternalSetCachePolicy(WebRequest.DefaultCachePolicy);
            }
        }

        delegate void DelEtwFireBeginWRGet(object id, string uri);
        delegate void DelEtwFireEndWRGet(object id);
        static DelEtwFireBeginWRGet s_EtwFireBeginGetResponse;
        static DelEtwFireEndWRGet s_EtwFireEndGetResponse;
        static DelEtwFireBeginWRGet s_EtwFireBeginGetRequestStream;
        static DelEtwFireEndWRGet s_EtwFireEndGetRequestStream;
        static volatile bool s_TriedGetEtwDelegates;

        private static void InitEtwMethods()
        {
            Type fest = typeof(FrameworkEventSource);
            var beginParamTypes = new Type[] { typeof(object), typeof(string) };
            var endParamTypes = new Type[] { typeof(object) };
            var bindingFlags = BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
            var mi1 = fest.GetMethod("BeginGetResponse", bindingFlags, null, beginParamTypes, null);
            var mi2 = fest.GetMethod("EndGetResponse", bindingFlags, null, endParamTypes, null);
            var mi3 = fest.GetMethod("BeginGetRequestStream", bindingFlags, null, beginParamTypes, null);
            var mi4 = fest.GetMethod("EndGetRequestStream", bindingFlags, null, endParamTypes, null);
            if (mi1 != null && mi2 != null && mi3 != null && mi4 != null)
            {
                s_EtwFireBeginGetResponse = (DelEtwFireBeginWRGet) mi1.CreateDelegate(typeof(DelEtwFireBeginWRGet), 
                                                                    FrameworkEventSource.Log);
                s_EtwFireEndGetResponse = (DelEtwFireEndWRGet) mi2.CreateDelegate(typeof(DelEtwFireEndWRGet), 
                                                                       FrameworkEventSource.Log);
                s_EtwFireBeginGetRequestStream = (DelEtwFireBeginWRGet) mi3.CreateDelegate(typeof(DelEtwFireBeginWRGet), 
                                                                       FrameworkEventSource.Log);
                s_EtwFireEndGetRequestStream = (DelEtwFireEndWRGet) mi4.CreateDelegate(typeof(DelEtwFireEndWRGet), 
                                                                       FrameworkEventSource.Log);
            }
            s_TriedGetEtwDelegates = true;
        }

        internal void LogBeginGetResponse(string uri)
        {
            if (!s_TriedGetEtwDelegates) 
                InitEtwMethods();
            if (s_EtwFireBeginGetResponse != null)
                s_EtwFireBeginGetResponse(this, uri);
        }
        internal void LogEndGetResponse()
        {
            if (!s_TriedGetEtwDelegates) 
                InitEtwMethods();
            if (s_EtwFireEndGetResponse != null)
                s_EtwFireEndGetResponse(this);
        }
        internal void LogBeginGetRequestStream(string uri)
        {
            if (!s_TriedGetEtwDelegates) 
                InitEtwMethods();
            if (s_EtwFireBeginGetRequestStream != null)
                s_EtwFireBeginGetRequestStream(this, uri);
        }
        internal void LogEndGetRequestStream()
        {
            if (!s_TriedGetEtwDelegates) 
                InitEtwMethods();
            if (s_EtwFireEndGetRequestStream != null)
                s_EtwFireEndGetRequestStream(this);
        }
    } // class WebRequest
} // namespace System.Net
