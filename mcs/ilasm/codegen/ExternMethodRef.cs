//
// Mono.ILASM.ExternMethodRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;
using System.Collections;

namespace Mono.ILASM {

        public class ExternMethodRef : BaseMethodRef {

                public ExternMethodRef (ExternTypeRef owner, BaseTypeRef ret_type,
                        PEAPI.CallConv call_conv, string name, BaseTypeRef[] param, int gen_param_count)
                        : base (owner, call_conv, ret_type, name, param, gen_param_count)
                {
                }

                public override void Resolve (CodeGen code_gen)
                {
			if (is_resolved)
				return;

                        if ((call_conv & PEAPI.CallConv.Vararg) != 0) {
                                ResolveVararg (code_gen);
                                return;
                        }

                        PEAPI.Type[] param_list = new PEAPI.Type[param.Length];
                        string write_name;

                        ret_type.Resolve (code_gen);

                        int count = 0;
                        foreach (BaseTypeRef typeref in param) {
                                typeref.Resolve (code_gen);
                                param_list[count++] = typeref.PeapiType;
                        }

                        if (name == "<init>")
                                write_name = ".ctor";
                        else
                                write_name = name;

                        owner.Resolve (code_gen);

                        if (owner.UseTypeSpec) {
                                PEAPI.Type owner_ref = owner.PeapiType;
                                peapi_method = code_gen.PEFile.AddMethodToTypeSpec (owner_ref, write_name,
                                                ret_type.PeapiType, param_list, gen_param_count);
                        } else {
                                PEAPI.ClassRef owner_ref;
                                owner_ref = (PEAPI.ClassRef) owner.PeapiType;
                                peapi_method = owner_ref.AddMethod (write_name,
                                                ret_type.PeapiType, param_list, gen_param_count);
                        }

                        peapi_method.AddCallConv (call_conv);

			is_resolved = true;
                }

                protected void ResolveVararg (CodeGen code_gen)
                {
			if (is_resolved)
				return;

                        ArrayList param_list = new ArrayList ();
                        ArrayList opt_list = new ArrayList ();
                        bool in_opt = false;
                        string write_name;

                        ret_type.Resolve (code_gen);

                        foreach (BaseTypeRef typeref in param) {
                                if (in_opt) {
                                        typeref.Resolve (code_gen);
                                        opt_list.Add (typeref.PeapiType);
                                } else if (typeref is SentinelTypeRef) {
                                        in_opt = true;
                                } else {
                                        typeref.Resolve (code_gen);
                                        param_list.Add (typeref.PeapiType);
                                }
                        }

                        if (name == "<init>")
                                write_name = ".ctor";
                        else
                                write_name = name;

                        if (owner.IsArray)
                                Report.Error ("Vararg methods on arrays are not supported yet.");

                        owner.Resolve (code_gen);

                        if (owner.UseTypeSpec) {
                                PEAPI.Type owner_ref = owner.PeapiType;
                                peapi_method = code_gen.PEFile.AddVarArgMethodToTypeSpec (owner_ref,
                                                write_name, ret_type.PeapiType,
                                                (PEAPI.Type[]) param_list.ToArray (typeof (PEAPI.Type)),
                                                (PEAPI.Type[]) opt_list.ToArray (typeof (PEAPI.Type)));
                        } else {
                                PEAPI.ClassRef owner_ref;
                                owner_ref = (PEAPI.ClassRef) owner.PeapiType;
                                peapi_method = owner_ref.AddVarArgMethod (write_name,
                                                ret_type.PeapiType,
                                                (PEAPI.Type[]) param_list.ToArray (typeof (PEAPI.Type)),
                                                (PEAPI.Type[]) opt_list.ToArray (typeof (PEAPI.Type)));
                        }


                        peapi_method.AddCallConv (call_conv);
			
			is_resolved = true;
                }
        }

}

