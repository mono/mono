//
// Mono.ILASM.ExternTypeRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;

namespace Mono.ILASM {

        /// <summary>
        /// A reference to a type in another assembly
        /// </summary>
        public class ExternTypeRef : PeapiTypeRef, IClassRef {

                public ExternTypeRef (PEAPI.ClassRef extern_type,
                                string full_name) : base (extern_type, full_name)
                {

                }

                public PEAPI.Class PeapiClass {
                        get {
                                return PeapiType as PEAPI.Class;
                        }
                }

                public PEAPI.ClassRef PeapiClassRef {
                        get {
                                return PeapiType as PEAPI.ClassRef;
                        }
                }

                public IMethodRef GetMethodRef (ITypeRef ret_type, PEAPI.CallConv call_conv,
                                string name, ITypeRef[] param)
                {
                        return new ExternMethodRef (this, ret_type, call_conv, name, param);
                }

                public IFieldRef GetFieldRef (ITypeRef ret_type, string name)
                {
                        return new ExternFieldRef (this, ret_type, name);
                }

                public IClassRef AsClassRef (CodeGen code_gen)
                {
                        return this;
                }
        }

}

