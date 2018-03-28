//------------------------------------------------------------------------------
// <copyright file="SessionState.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * HttpSessionState
 * 
 * Copyright (c) 1998-1999, Microsoft Corporation
 * 
 */

namespace System.Web.SessionState {

    using System.Threading;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Web.Util;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Text;
    using System.Globalization;
    using System.Security.Permissions;

    public class HttpSessionStateContainer : IHttpSessionState {
        String                      _id;
        ISessionStateItemCollection      _sessionItems;
        HttpStaticObjectsCollection _staticObjects;
        int                         _timeout;
        bool                        _newSession;
        HttpCookieMode              _cookieMode;
        SessionStateMode            _mode;
        bool                        _abandon;
        bool                        _isReadonly;
        SessionStateModule          _stateModule;   // used for optimized InProc session id callback

        public HttpSessionStateContainer(
                                 String                      id, 
                                 ISessionStateItemCollection sessionItems,
                                 HttpStaticObjectsCollection staticObjects,
                                 int                         timeout,
                                 bool                        newSession,
                                 HttpCookieMode              cookieMode,
                                 SessionStateMode            mode,
                                 bool                        isReadonly) 
            : this(null, id, sessionItems, staticObjects, timeout, newSession, cookieMode, mode, isReadonly) {
            if (id == null) {
                throw new ArgumentNullException("id");
            }
        }

        internal HttpSessionStateContainer(
                                 SessionStateModule          stateModule, 
                                 string                      id,
                                 ISessionStateItemCollection      sessionItems,
                                 HttpStaticObjectsCollection staticObjects,
                                 int                         timeout,
                                 bool                        newSession,
                                 HttpCookieMode              cookieMode,
                                 SessionStateMode            mode,
                                 bool                        isReadonly) {
            _stateModule = stateModule;
            _id = id;   // If null, it means we're delaying session id reading
            _sessionItems = sessionItems;
            _staticObjects = staticObjects;
            _timeout = timeout;    
            _newSession = newSession; 
            _cookieMode = cookieMode;
            _mode = mode;
            _isReadonly = isReadonly;
        }

        internal HttpSessionStateContainer() {
        }

        /*
         * The Id of the session.
         */
        public String SessionID {
            get {
                if (_id == null) {
                    Debug.Assert(_stateModule != null, "_stateModule != null");
                    _id = _stateModule.DelayedGetSessionId();
                }
                return _id;
            }
        }

        /*
         * The length of a session before it times out, in minutes.
         */
        public int Timeout {
            get {return _timeout;}
            set {
                if (value <= 0) {
                    throw new ArgumentException(SR.GetString(SR.Timeout_must_be_positive));
                }

                if (value > SessionStateModule.MAX_CACHE_BASED_TIMEOUT_MINUTES &&
                    (Mode == SessionStateMode.InProc ||
                    Mode == SessionStateMode.StateServer)) {
                    throw new ArgumentException(
                        SR.GetString(SR.Invalid_cache_based_session_timeout));
                }

                _timeout = value;
            }
        }

        /*
         * Is this a new session?
         */

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool IsNewSession {
            get {return _newSession;}
        }

        /*
         * Is session state in a separate process
         */

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SessionStateMode Mode {
            get {return _mode;}
        }

        /*
         * Is session state cookieless?
         */
        public bool IsCookieless {
            get {
                if (_stateModule != null) {
                    // See VSWhidbey 399907
                    return _stateModule.SessionIDManagerUseCookieless;
                }
                else {
                    // For container created by custom session state module,
                    // sorry, we currently don't have a way to tell and thus we rely blindly
                    // on cookieMode.
                    return CookieMode == HttpCookieMode.UseUri;
                }
            }
        }

        public HttpCookieMode CookieMode {
            get {return _cookieMode;}
        }

        /*
         * Abandon the session.
         * 
         */

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Abandon() {
            _abandon = true;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int LCID {
            // 


            get { return Thread.CurrentThread.CurrentCulture.LCID; }
            set { Thread.CurrentThread.CurrentCulture = HttpServerUtility.CreateReadOnlyCultureInfo(value); }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int CodePage {
            // 


            get { 
                if (HttpContext.Current != null)
                    return HttpContext.Current.Response.ContentEncoding.CodePage;
                else
                    return Encoding.Default.CodePage;
            }
            set { 
                if (HttpContext.Current != null)
                    HttpContext.Current.Response.ContentEncoding = Encoding.GetEncoding(value);
            }
        }

        public bool IsAbandoned {
            get {return _abandon;}
        }

        public HttpStaticObjectsCollection StaticObjects {
            get { return _staticObjects;}
        }

        public Object this[String name]
        {
            get {
                return _sessionItems[name];
            }
            set {
                _sessionItems[name] = value;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Object this[int index]
        {
            get {return _sessionItems[index];}
            set {_sessionItems[index] = value;}
        }
        

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Add(String name, Object value) {
            _sessionItems[name] = value;        
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(String name) {
            _sessionItems.Remove(name);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void RemoveAt(int index) {
            _sessionItems.RemoveAt(index);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Clear() {
            _sessionItems.Clear();
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void RemoveAll() {
            Clear();
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Count {
            get {return _sessionItems.Count;}
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public NameObjectCollectionBase.KeysCollection Keys {
            get {return _sessionItems.Keys;}
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public IEnumerator GetEnumerator() {
            return _sessionItems.GetEnumerator();
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(Array array, int index) {
            for (IEnumerator e = this.GetEnumerator(); e.MoveNext();)
                array.SetValue(e.Current, index++);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Object SyncRoot {
            get { return this;}
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool IsReadOnly {
            get { return _isReadonly;}
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool IsSynchronized {
            get { return false;}
        }
    }
}

