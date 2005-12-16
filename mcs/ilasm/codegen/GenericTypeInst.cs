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
using System.Text;

namespace Mono.ILASM {

        public class GenericTypeInst : ModifiableType, ITypeRef {

                private string name;
                private string full_name;
                private string sig_mod;
                private ITypeRef[] type_list;
                private PEAPI.Type gen_inst;

                private bool is_resolved;

                public GenericTypeInst (string name,
                                ITypeRef[] type_list)
                {
                        this.name = name;
                        this.type_list = type_list;
                        sig_mod = String.Empty;

                        //Build full_name (foo < , >)
                        StringBuilder sb = new StringBuilder (name);
                        sb.Append ("<");
                        foreach (ITypeRef tr in type_list)
                                sb.AppendFormat ("{0}, ", tr.FullName);
                        //Remove the extra ', ' at the end
                        sb.Length -= 2;
                        sb.Append (">");
                        full_name = sb.ToString ();

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

                        p_gen_type = code_gen.TypeManager.GetPeapiType (name);

                        for (int i=0; i<p_type_list.Length; i++) {
                                type_list[i].Resolve (code_gen);
                                p_type_list[i] = type_list[i].PeapiType;
                        }

                        gen_inst = new PEAPI.GenericTypeInst (p_gen_type, p_type_list);
                        gen_inst = Modify (code_gen, gen_inst);

                        is_resolved = true;
                }

                public IMethodRef GetMethodRef (ITypeRef ret_type, PEAPI.CallConv call_conv,
                                string meth_name, ITypeRef[] param, int gen_param_count)
                {
                        return new TypeSpecMethodRef (this, ret_type, call_conv, meth_name, param, gen_param_count);
                }

                public IFieldRef GetFieldRef (ITypeRef ret_type, string field_name)
                {
                        return new TypeSpecFieldRef (this, ret_type, field_name);
                }

        }

}


