//
// Mono.ILASM.GenericTypeInst
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Latitude Geographics Group, All rights reserved
//


using System;
using System.Collections;

namespace Mono.ILASM {

        public class GenericTypeInst : ModifiableType, ITypeRef {

                private string full_name;
                private string sig_mod;
                private ITypeRef[] type_list;
                private PEAPI.Type gen_inst;

                private bool is_resolved;

                public GenericTypeInst (string full_name,
                                ITypeRef[] type_list)
                {
                        this.full_name = full_name;
                        this.type_list = type_list;
                        sig_mod = String.Empty;
                        is_resolved = false;
                }

                public string FullName {
                        get { return full_name + sig_mod; }
                }

                public override string SigMod {
                        get { return sig_mod; }
                        set { sig_mod = value; }
                }

                public PEAPI.Type PeapiType {
                        get { return gen_inst; }
                }

                public void Resolve (CodeGen code_gen)
                {
                        if (is_resolved)
                                return;

                        PEAPI.Type p_gen_type;
                        PEAPI.Type[] p_type_list = new PEAPI.Type[type_list.Length];

                        p_gen_type = code_gen.TypeManager.GetPeapiType (full_name);

                        for (int i=0; i<p_type_list.Length; i++) {
                                type_list[i].Resolve (code_gen);
                                p_type_list[i] = type_list[i].PeapiType;
                        }

                        gen_inst = new PEAPI.GenericTypeInst (p_gen_type, p_type_list);
                        gen_inst = Modify (code_gen, gen_inst);

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

        }

}


