//
// Mono.ILASM.GenericTypeRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;

namespace Mono.ILASM {

        public class GenericTypeRef : ModifiableType, ITypeRef {

                private PEAPI.Type type;
                private string full_name;
                private string sig_mod;

                public GenericTypeRef (PEAPI.GenericTypeSpec gen_type,
                                string full_name)
                {
                        this.type = gen_type;
                        this.full_name = full_name;
                        sig_mod = String.Empty;
                }

                public string FullName {
                        get { return full_name + sig_mod; }
                }

                public override string SigMod {
                        get { return sig_mod; }
                        set { sig_mod = value; }
                }

                public PEAPI.Type PeapiType {
                        get { return type; }
                }

                public void Resolve (CodeGen code_gen)
                {
                        type = Modify (code_gen, type);
                }

                public IMethodRef GetMethodRef (ITypeRef ret_type, PEAPI.CallConv call_conv,
                                string name, ITypeRef[] param)
                {
                        return new TypeSpecMethodRef (this, ret_type, call_conv, name, param);
                }

                public IFieldRef GetFieldRef (ITypeRef ret_type, string name)
                {
                        return new TypeSpecFieldRef (this, ret_type, name);
                }
        }

}

