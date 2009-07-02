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

using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Globalization;
using Marshal = System.Runtime.InteropServices.Marshal;

#if CODEPLEX_40
namespace System.Dynamic {
#else
namespace Microsoft.Scripting {
#endif

    internal sealed class ComMethodDesc {
        private readonly int _memid;  // this is the member id extracted from FUNCDESC.memid
        private readonly string _name;
        internal readonly INVOKEKIND InvokeKind;
        private readonly int _paramCnt;

        private ComMethodDesc(int dispId) {
            _memid = dispId;
        }

        internal ComMethodDesc(string name, int dispId)
            : this(dispId) {
            // no ITypeInfo constructor
            _name = name;
        }

        internal ComMethodDesc(string name, int dispId, INVOKEKIND invkind)
            : this(name, dispId) {
            InvokeKind = invkind;
        }

        internal ComMethodDesc(ITypeInfo typeInfo, FUNCDESC funcDesc)
            : this(funcDesc.memid) {

            InvokeKind = funcDesc.invkind;

            int cNames;
            string[] rgNames = new string[1 + funcDesc.cParams];
            typeInfo.GetNames(_memid, rgNames, rgNames.Length, out cNames);
            if (IsPropertyPut && rgNames[rgNames.Length - 1] == null) {
                rgNames[rgNames.Length - 1] = "value";
                cNames++;
            }
            Debug.Assert(cNames == rgNames.Length);
            _name = rgNames[0];

            _paramCnt = funcDesc.cParams;
        }

        public string Name {
            get {
                Debug.Assert(_name != null);
                return _name;
            }
        }

        public int DispId {
            get { return _memid; }
        }

        public bool IsPropertyGet {
            get {
                return (InvokeKind & INVOKEKIND.INVOKE_PROPERTYGET) != 0;
            }
        }

        public bool IsDataMember {
            get {
                //must be regular get
                if (!IsPropertyGet || DispId == ComDispIds.DISPID_NEWENUM) {
                    return false;
                } 

                //must have no parameters
                return _paramCnt == 0;
            }
        }

        public bool IsPropertyPut {
            get {
                return (InvokeKind & (INVOKEKIND.INVOKE_PROPERTYPUT | INVOKEKIND.INVOKE_PROPERTYPUTREF)) != 0;
            }
        }

        public bool IsPropertyPutRef {
            get {
                return (InvokeKind & INVOKEKIND.INVOKE_PROPERTYPUTREF) != 0;
            }
        }

        internal int ParamCount {
            get {
                return _paramCnt;  
            }
        }
    }
}

#endif
