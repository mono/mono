/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System; using Microsoft;


#if !SILVERLIGHT // ComObject

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Threading;
using ComTypes = System.Runtime.InteropServices.ComTypes;

#if CODEPLEX_40
namespace System.Dynamic {
#else
namespace Microsoft.Scripting {
#endif

    internal class ComTypeDesc {
        private string _typeName;
        private string _documentation;
        private Guid _guid;

        //Hashtable is threadsafe for multiple readers single writer. 
        //Enumerating and writing is mutually exclusive so require locking.
        private Hashtable _funcs;
        private Hashtable _puts;
        private Hashtable _putRefs;
        private ComMethodDesc _getItem;
        private ComMethodDesc _setItem;

        private Dictionary<string, ComEventDesc> _events;

        private static readonly Dictionary<string, ComEventDesc> _EmptyEventsDict = new Dictionary<string, ComEventDesc>();

        internal ComTypeDesc(ITypeInfo typeInfo) {
            if (typeInfo != null) {
                ComRuntimeHelpers.GetInfoFromType(typeInfo, out _typeName, out _documentation);
            }
        }

        [SecurityCritical]
        internal static ComTypeDesc FromITypeInfo(ComTypes.ITypeInfo typeInfo, ComTypes.TYPEATTR typeAttr) {
            if (typeAttr.typekind == ComTypes.TYPEKIND.TKIND_COCLASS) {
                return new ComTypeClassDesc(typeInfo);
            } else if (typeAttr.typekind == ComTypes.TYPEKIND.TKIND_ENUM) {
                return new ComTypeEnumDesc(typeInfo);
            } else if ((typeAttr.typekind == ComTypes.TYPEKIND.TKIND_DISPATCH) ||
                  (typeAttr.typekind == ComTypes.TYPEKIND.TKIND_INTERFACE)) {

                return new ComTypeDesc(typeInfo);
            } else {
                throw Error.UnsupportedEnumType();
            }
        }

        internal static ComTypeDesc CreateEmptyTypeDesc() {
            ComTypeDesc typeDesc = new ComTypeDesc(null);
            typeDesc._funcs = new Hashtable();
            typeDesc._puts = new Hashtable();
            typeDesc._putRefs = new Hashtable();
            typeDesc._events = _EmptyEventsDict;

            return typeDesc;
        }

        internal static Dictionary<string, ComEventDesc> EmptyEvents {
            get { return _EmptyEventsDict; }
        }

        internal Hashtable Funcs {
            get { return _funcs; }
            set { _funcs = value; }
        }

        internal Hashtable Puts {
            set { _puts = value; }
        }

        internal Hashtable PutRefs {
            set { _putRefs = value; }
        }

        internal Dictionary<string, ComEventDesc> Events {
            get { return _events; }
            set { _events = value; }
        }

        internal bool TryGetFunc(string name, out ComMethodDesc method) {
            name = name.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
            if (_funcs.ContainsKey(name)) {
                method = _funcs[name] as ComMethodDesc;
                return true;
            }
            method = null;
            return false;
        }

        internal void AddFunc(string name, ComMethodDesc method) {
            name = name.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
            lock (_funcs) {
                _funcs[name] = method;
            }
        }

        internal bool TryGetPut(string name, out ComMethodDesc method) {
            name = name.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
            if (_puts.ContainsKey(name)) {
                method = _puts[name] as ComMethodDesc;
                return true;
            }
            method = null;
            return false;
        }

        internal void AddPut(string name, ComMethodDesc method) {
            name = name.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
            lock (_puts) {
                _puts[name] = method;
            }
        }

        internal bool TryGetPutRef(string name, out ComMethodDesc method) {
            name = name.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
            if (_putRefs.ContainsKey(name)) {
                method = _putRefs[name] as ComMethodDesc;
                return true;
            }
            method = null;
            return false;
        }
        internal void AddPutRef(string name, ComMethodDesc method) {
            name = name.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
            lock (_putRefs) {
                _putRefs[name] = method;
            }
        }

        internal bool TryGetEvent(string name, out ComEventDesc @event) {
            name = name.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
            return _events.TryGetValue(name, out @event);
        }

        internal string[] GetMemberNames(bool dataOnly) {
            var names = new Dictionary<string, object>();

            lock (_funcs) {
                foreach (ComMethodDesc func in _funcs.Values) {
                    if (!dataOnly || func.IsDataMember) {
                        names.Add(func.Name, null);
                    }
                }
            }

            if (!dataOnly) {
                lock (_puts) {
                    foreach (ComMethodDesc func in _puts.Values) {
                        if (!names.ContainsKey(func.Name)) {
                            names.Add(func.Name, null);
                        }
                    }
                }

                lock (_putRefs) {
                    foreach (ComMethodDesc func in _putRefs.Values) {
                        if (!names.ContainsKey(func.Name)) {
                            names.Add(func.Name, null);
                        }
                    }
                }

                if (_events != null && _events.Count > 0) {
                    foreach (string name in _events.Keys) {
                        if (!names.ContainsKey(name)) {
                            names.Add(name, null);
                        }
                    }
                }
            }

            string[] result = new string[names.Keys.Count];
            names.Keys.CopyTo(result, 0);
            return result;
        }

        internal string TypeName {
            get { return _typeName; }
        }

        internal Guid Guid {
            get { return _guid; }
            set { _guid = value; }
        }

        internal ComMethodDesc GetItem {
            get { return _getItem; }
        }

        internal void EnsureGetItem(ComMethodDesc candidate) {
            Interlocked.CompareExchange(ref _getItem, candidate, null);
        }

        internal ComMethodDesc SetItem {
            get { return _setItem; }
        }

        internal void EnsureSetItem(ComMethodDesc candidate) {
            Interlocked.CompareExchange(ref _setItem, candidate, null);
        }
    }
}

#endif
