//
// Mono.ILASM.GenericParamRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//  Ankit Jain  (JAnkit@novell.com)
//
// (C) 2003 Jackson Harper, All rights reserved
// (C) 2006 Novell, Inc (http://www.novell.com)
//


using System;
using System.Collections;

namespace Mono.ILASM {

        public class GenericParamRef : BaseGenericTypeRef {

                /* PEAPI.Type type: Might get modified */
                /* Unmodified GenParam */
                private PEAPI.GenParam param;
                private bool is_added; /* Added to TypeSpec table ? */

                public GenericParamRef (PEAPI.GenParam gen_param, string full_name)
                        : this (gen_param, full_name, null)
                {
                }

                public GenericParamRef (PEAPI.GenParam gen_param, string full_name, ArrayList conv_list)
                        : base (full_name, false, conv_list, "")
                {
                        this.type = gen_param;
                        this.param = gen_param;
                        is_added = false;
                }

                public override string FullName {
                        get { 
                                return (param.Type == PEAPI.GenParamType.Var ? "!" : "!!") 
                                        + param.Index + sig_mod;
                        }
                }

                public override void MakeValueClass ()
                {
                        throw new Exception ("Not supported");
                }

                public override BaseClassRef Clone ()
                {
                        return new GenericParamRef (param, full_name, (ArrayList) ConversionList.Clone ());
                }

                public override void ResolveNoTypeSpec (CodeGen code_gen)
                {
                        if (is_resolved)
                                return;
                        
                        type = Modify (code_gen, type);
                        is_resolved = true;
                }

                public override void Resolve (CodeGen code_gen)
                {
                        ResolveNoTypeSpec (code_gen);
                        if (is_added)
                                return;

                        code_gen.PEFile.AddGenericParam (param);
                        is_added = true;
                }
                
                public override void Resolve (GenericParameters type_gen_params, GenericParameters method_gen_params)
                {
                        if (param.Name == "") {
                                /* Name wasn't specified */
                                return;
                        }

                        if (param.Type == PEAPI.GenParamType.MVar && method_gen_params != null)
                                param.Index = method_gen_params.GetGenericParamNum (param.Name); 
                        else if (type_gen_params != null)
                                param.Index = type_gen_params.GetGenericParamNum (param.Name);

                        if (param.Index < 0)
                                /* TODO: Report error */
                                throw new Exception (String.Format ("Invalid {0}type parameter '{1}'", 
                                                        (param.Type == PEAPI.GenParamType.MVar ? "method " : ""),
                                                         param.Name));
                }

                public override IMethodRef GetMethodRef (BaseTypeRef ret_type, PEAPI.CallConv call_conv,
                                string name, BaseTypeRef[] param, int gen_param_count)
                {
                        return new TypeSpecMethodRef (this, call_conv, ret_type, name, param, gen_param_count);
                }

                public override IFieldRef GetFieldRef (BaseTypeRef ret_type, string name)
                {
                        return new TypeSpecFieldRef (this, ret_type, name);
                }
        }

}

