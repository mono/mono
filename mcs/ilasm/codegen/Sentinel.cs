//
// Mono.ILASM.SentinelTypeRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//

using System;

namespace Mono.ILASM {

        public class SentinelTypeRef : ModifiableType, ITypeRef {

                private PEAPI.Type peapi_type;
                private string name;
                private string sig_mod;
                private bool is_resolved;

                public SentinelTypeRef ()
                {
                        name = "...";
                        sig_mod = String.Empty;
                        is_resolved = false;
                }

                public PEAPI.Type PeapiType {
                        get { return peapi_type; }
                }

                public string FullName {
                        get { return name; }
                }

                public override string SigMod {
                        get { return sig_mod; }
                        set { sig_mod = value; }
                }

                public void Resolve (CodeGen code_gen)
                {
                        if (is_resolved)
                                return;

                        peapi_type = new PEAPI.Sentinel ();
                        peapi_type = Modify (code_gen, peapi_type);

                        is_resolved = true;
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

                public override string ToString ()
                {
                        return "Sentinel  " + name;
                }
        }

}

