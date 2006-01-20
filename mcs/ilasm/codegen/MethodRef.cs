//
// Mono.ILASM.MethodRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;
using System.Collections;

namespace Mono.ILASM {

        public class MethodRef : BaseMethodRef {

                public MethodRef (TypeRef owner, PEAPI.CallConv call_conv,
                        BaseTypeRef ret_type, string name, BaseTypeRef[] param, int gen_param_count)
                        : base (owner, call_conv, ret_type, name, param, gen_param_count)
                {
                }

                public override void Resolve (CodeGen code_gen)
                {
                        if (is_resolved)
                                return;

			owner.Resolve (code_gen);

                        TypeDef owner_def = code_gen.TypeManager[owner.FullName];
			if (owner_def == null)
				throw new Exception (String.Format ("Reference to undefined class '{0}'", owner.FullName));

                        string write_name;

                        if (name == "<init>")
                                write_name = ".ctor";
                        else
                                write_name = name;

                        string sig;

                        if ((call_conv & PEAPI.CallConv.Vararg) == 0) {
                                sig = MethodDef.CreateSignature (ret_type, name, param, gen_param_count);
                                peapi_method = owner_def.ResolveMethod (sig, code_gen);
                        } else {
                                sig = MethodDef.CreateVarargSignature (ret_type, name, param);
                                ArrayList opt_list = new ArrayList ();
                                bool in_opt = false;
                                foreach (BaseTypeRef type in param) {
                                        if (type is SentinelTypeRef) {
                                                in_opt = true;
                                        } else if (in_opt) {
                                                type.Resolve (code_gen);
                                                opt_list.Add (type.PeapiType);
                                        }
                                }
                                peapi_method = owner_def.ResolveVarargMethod (sig, code_gen,
                                                (PEAPI.Type[]) opt_list.ToArray (typeof (PEAPI.Type)));
                        }

                        is_resolved = true;

                }

        }

}

