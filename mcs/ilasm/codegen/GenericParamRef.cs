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

        public interface IGenericTypeRef : IClassRef {
                /* Used to resolve any gen params in arguments, constraints etc */
                void Resolve (GenericParameters type_gen_params, GenericParameters method_gen_params);
                
                /* Only resolves, does not add it to the TypeSpec
                   table */
                void ResolveNoTypeSpec (CodeGen code_gen);
        }

        public class GenericParamRef : ModifiableType, IGenericTypeRef {

                /* Might get modified */
                private PEAPI.Type type;
                /* Unmodified GenParam */
                private PEAPI.GenParam param;
                private string full_name;
                private string sig_mod;
                private bool is_resolved;
                private bool is_added; /* Added to TypeSpec table ? */

                public GenericParamRef (PEAPI.GenParam gen_param, string full_name)
                        : this (gen_param, full_name, null)
                {
                }

                public GenericParamRef (PEAPI.GenParam gen_param, string full_name, ArrayList conv_list)
                {
                        this.type = gen_param;
                        this.param = gen_param;
                        this.full_name = full_name;
                        sig_mod = String.Empty;
                        is_resolved = false;
                        is_added = false;
                        if (conv_list != null)
                                ConversionList = conv_list;
                }

                public string FullName {
                        get { 
                                return (param.Type == PEAPI.GenParamType.Var ? "!" : "!!") 
                                        + param.Index + sig_mod;
                        }
                }

                public override string SigMod {
                        get { return sig_mod; }
                        set { sig_mod = value; }
                }

                public PEAPI.Type PeapiType {
                        get { return type; }
                }

                public PEAPI.Class PeapiClass {
                        get { return (PEAPI.Class) type; }
                }

                public void MakeValueClass ()
                {
                        throw new Exception ("Not supported");
                }

                public IClassRef Clone ()
                {
                        return new GenericParamRef (param, full_name, (ArrayList) ConversionList.Clone ());
                }

                public GenericTypeInst GetGenericTypeInst (GenericArguments gen_args)
                {
                        throw new Exception ("Not supported");
                }
                
                public PEAPI.Type ResolveInstance (CodeGen code_gen, GenericArguments gen_args)
                {
                        throw new Exception ("Not supported");
                }

                public void ResolveNoTypeSpec (CodeGen code_gen)
                {
                        if (is_resolved)
                                return;
                        
                        type = Modify (code_gen, type);
                        is_resolved = true;
                }

                public void Resolve (CodeGen code_gen)
                {
                        ResolveNoTypeSpec (code_gen);
                        if (is_added)
                                return;

                        code_gen.PEFile.AddGenericParam (param);
                        is_added = true;
                }
                
                public void Resolve (GenericParameters type_gen_params, GenericParameters method_gen_params)
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

                public IMethodRef GetMethodRef (ITypeRef ret_type, PEAPI.CallConv call_conv,
                                string name, ITypeRef[] param, int gen_param_count)
                {
                        return new TypeSpecMethodRef (this, ret_type, call_conv, name, param, gen_param_count);
                }

                public IFieldRef GetFieldRef (ITypeRef ret_type, string name)
                {
                        return new TypeSpecFieldRef (this, ret_type, name);
                }
        }

}

