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
                private GenericArguments gen_args;
                private PEAPI.Type gen_inst;

                private bool is_resolved;

                public GenericTypeInst (string name,
                                GenericArguments gen_args)
                {
                        this.name = name;
                        this.gen_args = gen_args;
                        full_name = name + gen_args.ToString ();
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

                        PEAPI.Type p_gen_type = code_gen.TypeManager.GetPeapiType (name);

                        gen_inst = new PEAPI.GenericTypeInst (p_gen_type, gen_args.Resolve (code_gen));
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


