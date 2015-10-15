//------------------------------------------------------------------------------
// <copyright file="HttpServerVarsCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Collection of server variables with callback to HttpRequest for 'dynamic' ones
 * 
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Web {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.Hosting;
    using System.Web.Util;
    
    internal class HttpServerVarsCollection : HttpValueCollection {
        private bool _populated;
        private HttpRequest _request;
        private IIS7WorkerRequest _iis7workerRequest;
        private List<HttpServerVarsCollectionEntry> _unsyncedEntries;

        // We preallocate the base collection with a size that should be sufficient
        // to store all server variables w/o having to expand
        internal HttpServerVarsCollection(HttpWorkerRequest wr, HttpRequest request) : base(59) {
            // if this is an IIS7WorkerRequest, then the collection will be writeable and we will
            // call into IIS7 to update the server var block when changes are made.
            _iis7workerRequest = wr as IIS7WorkerRequest;
            _request = request;
            _populated = false;

            Debug.Assert( _request != null );
        }

        [SuppressMessage("Microsoft.Usage", "CA2236:CallBaseClassMethodsOnISerializableTypes",
            Justification = "this class, while derived from class implementing ISerializable, is not serializable")]
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            throw new SerializationException();
        }

        internal void Dispose() {
            _request = null;
        }

        internal void AddStatic(String name, String value) {
            if (value == null)
                value = String.Empty;

            InvalidateCachedArrays();
            BaseAdd(name, new HttpServerVarsCollectionEntry(name, value));
        }

        internal void AddDynamic(String name, DynamicServerVariable var) {
            InvalidateCachedArrays();
            BaseAdd(name, new HttpServerVarsCollectionEntry(name, var));
        }

        private String GetServerVar(Object e) {
            HttpServerVarsCollectionEntry entry = (HttpServerVarsCollectionEntry)e;
            return (entry != null) ? entry.GetValue(_request) : null;
        }

        //
        //  Support for deferred population of the collection
        //

        private void Populate() {
            if (!_populated) {
                if (_request != null) {
                    MakeReadWrite();
                    _request.FillInServerVariablesCollection();

                    // Add all unsynchronized entries, if any
                    if (_unsyncedEntries != null) {
                        foreach (var entry in _unsyncedEntries) { 
                            var existingEntry = (HttpServerVarsCollectionEntry)BaseGet(entry.Name);
                            if (existingEntry != null && existingEntry.IsDynamic) {
                                // Exisiting dynamic server variables cannot be modified - ignore the new value
                                continue; 
                            }

                            InvalidateCachedArrays();
                            BaseSet(entry.Name, entry); // Update an existing entry, or create one if it's new
                        }

                        _unsyncedEntries.Clear();
                    }

                    if (_iis7workerRequest == null) {
                        MakeReadOnly();
                    }
                }
                _populated = true;
            }
        }

        private String GetSimpleServerVar(String name) {
            // get server var without population of the collection
            // only most popular are included

            if (name != null && name.Length > 1 && _request != null) {
                switch (name[0]) {
                    case 'A':
                    case 'a':
                        if (StringUtil.EqualsIgnoreCase(name, "AUTH_TYPE"))
                            return _request.CalcDynamicServerVariable(DynamicServerVariable.AUTH_TYPE);
                        else if (StringUtil.EqualsIgnoreCase(name, "AUTH_USER"))
                            return _request.CalcDynamicServerVariable(DynamicServerVariable.AUTH_USER);
                        break;
                    case 'H':
                    case 'h':
                        if (StringUtil.EqualsIgnoreCase(name, "HTTP_USER_AGENT"))
                            return _request.UserAgent;
                        break;
                    case 'Q':
                    case 'q':
                        if (StringUtil.EqualsIgnoreCase(name, "QUERY_STRING"))
                            return _request.QueryStringText;
                        break;
                    case 'P':
                    case 'p':
                        if (StringUtil.EqualsIgnoreCase(name, "PATH_INFO"))
                            return _request.Path;
                        else if (StringUtil.EqualsIgnoreCase(name, "PATH_TRANSLATED"))
                            return _request.PhysicalPath;
                        break;
                    case 'R':
                    case 'r':
                        if (StringUtil.EqualsIgnoreCase(name, "REQUEST_METHOD"))
                            return _request.HttpMethod;
                        else if (StringUtil.EqualsIgnoreCase(name, "REMOTE_USER"))
                            return _request.CalcDynamicServerVariable(DynamicServerVariable.AUTH_USER);
                        else if (StringUtil.EqualsIgnoreCase(name, "REMOTE_HOST"))
                            return _request.UserHostName;
                        else if (StringUtil.EqualsIgnoreCase(name, "REMOTE_ADDRESS"))
                            return _request.UserHostAddress;
                        break;
                    case 'S':
                    case 's':
                        if (StringUtil.EqualsIgnoreCase(name, "SCRIPT_NAME"))
                            return _request.FilePath;
                        break;
                }
            }

            // do the default processing (populate the collection)
            return null;
        }

        //
        //  Enumerator must pre-populate the collection
        //

        public override IEnumerator GetEnumerator() {
            Populate();
            return base.GetEnumerator();
        }

        //
        //  NameValueCollection overrides
        //

        public override int Count {
            get {
                Populate();
                return base.Count;
            }
        }

        public override void Add(String name, String value) {
            // not supported because it appends the value to a comma separated list
            throw new NotSupportedException();
        }

        public override void Clear() {
            throw new NotSupportedException();
        }

        public override String Get(String name) {
            if (!_populated) {
                String value = GetSimpleServerVar(name);

                if (value != null)
                    return value;

                Populate();
            }

            if (_iis7workerRequest != null) {
                string var = GetServerVar(BaseGet(name));

                if (String.IsNullOrEmpty(var)) {
                    var = _request.FetchServerVariable(name);
                }

                return var;
            }
            else {
                return GetServerVar(BaseGet(name));
            }
        }

        public override String[] GetValues(String name) {
            String s = Get(name);
            return(s != null) ? new String[1] { s} : null;
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.High)]
        public override void Set(String name, String value) {
            if (_iis7workerRequest == null) {
                throw new PlatformNotSupportedException();
            }
            
            if (name == null) {
                throw new ArgumentNullException("name");
            }
            
            SetNoDemand(name, value);
        }

        internal void SetNoDemand(String name, String value) {
            if (value == null) {
                value = String.Empty;
            }
          
            _iis7workerRequest.SetServerVariable(name, value);
            SetServerVariableManagedOnly(name, value);
            SynchronizeHeader(name, value);
            _request.InvalidateParams();
        }

        private void SynchronizeHeader(String name, String value) {
            if (StringUtil.StringStartsWith(name, "HTTP_"))
            {
                // update managed copy of header
                string headerName = name.Substring("HTTP_".Length);
                headerName = headerName.Replace('_', '-');
                int knownIndex = HttpWorkerRequest.GetKnownRequestHeaderIndex(headerName);
                if (knownIndex > -1) {
                    headerName = HttpWorkerRequest.GetKnownRequestHeaderName(knownIndex);
                }

                HttpHeaderCollection headers = _request.Headers as HttpHeaderCollection;
                if (headers != null) {
                    headers.SynchronizeHeader(headerName, value);
                }
            }
        }

        // updates managed copy of server variable with current value from native header block
        internal void SynchronizeServerVariable(String name, String value, bool ensurePopulated = true) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }
            
            if (value != null) {
                if (this._populated || ensurePopulated) {
                    SetServerVariableManagedOnly(name, value);
                }
                else {
                    // Lazy synchronization - when populate is indeed required
                    if (_unsyncedEntries == null) {
                        _unsyncedEntries = new List<HttpServerVarsCollectionEntry>();
                    }

                    _unsyncedEntries.Add(new HttpServerVarsCollectionEntry(name, value));
                }
            }
            else {
                base.Remove(name);
            }

            _request.InvalidateParams();
        }

        // updates managed copy of server variable with current value from native header block
        private void SetServerVariableManagedOnly(String name, String value) {
            Debug.Assert(name != null);
            Debug.Assert(value != null);

            // populate in order to identify dynamic variables
            Populate();
            
            // dynamic server variables cannot be modified
            HttpServerVarsCollectionEntry entry = (HttpServerVarsCollectionEntry) BaseGet(name);
            if (entry != null && entry.IsDynamic) {
                throw new HttpException(SR.GetString(SR.Server_variable_cannot_be_modified));
            }

            InvalidateCachedArrays();            
            // this will update an existing entry, or create one if it's new
            BaseSet(name, new HttpServerVarsCollectionEntry(name, value));
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.High)]
        public override void Remove(String name) {
            if (_iis7workerRequest == null) {
                throw new PlatformNotSupportedException();
            }

            if (name == null) {
                throw new ArgumentNullException("name");
            }

            RemoveNoDemand(name);
        }

        internal void RemoveNoDemand(String name) {
            // delete by sending null value
            _iis7workerRequest.SetServerVariable(name, null /*value*/);

            base.Remove(name);
            SynchronizeHeader(name, null);
            _request.InvalidateParams();
        }

        public override String Get(int index)  {
            Populate();
            return GetServerVar(BaseGet(index));
        }

        public override String[] GetValues(int index) {
            String s = Get(index);
            return(s != null) ? new String[1] { s} : null;
        }

        public override String GetKey(int index) {
            Populate();
            return base.GetKey(index);
        }

        public override string[] AllKeys {
            get {
                Populate();
                return base.AllKeys;
            }
        }

        //
        //  HttpValueCollection overrides
        //

        internal override string ToString(bool urlencoded) {
            Populate();

            StringBuilder s = new StringBuilder();
            int n = Count;
            String key, value;

            for (int i = 0; i < n; i++) {
                if (i > 0)
                    s.Append('&');

                key = GetKey(i);
                if (urlencoded)
                    key = UrlEncodeForToString(key);
                s.Append(key);

                s.Append('=');

                value = Get(i);
                if (urlencoded)
                    value = UrlEncodeForToString(value);
                s.Append(value);
            }

            return s.ToString();
        }
    }

/*
 *  Entry in a server vars colleciton
 */
    internal class HttpServerVarsCollectionEntry {
        internal readonly String Name;
        internal readonly bool   IsDynamic;
        internal readonly String Value;
        internal readonly DynamicServerVariable Var;

        internal HttpServerVarsCollectionEntry(String name, String value) {
            Name = name;
            Value = value;
            IsDynamic = false;
        }

        internal HttpServerVarsCollectionEntry(String name, DynamicServerVariable var) {
            Name = name;
            Var = var;
            IsDynamic = true;
        }

        internal String GetValue(HttpRequest request) {
            String v = null;

            if (IsDynamic) {
                if (request != null)
                    v = request.CalcDynamicServerVariable(Var);
            }
            else {
                v = Value;
            }

            return v;
        }
    }


}
