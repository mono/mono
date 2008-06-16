//
// Mono.ILASM.GlobalMethodRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;
using System.Collections;

namespace Mono.ILASM {

        public class GlobalMethodRef : BaseMethodRef {

                public GlobalMethodRef (BaseTypeRef ret_type, PEAPI.CallConv call_conv,
                                string name, BaseTypeRef[] param, int gen_param_count)
                        : base (null, call_conv, ret_type, name, param, gen_param_count)
                {
                }

                public override void Resolve (CodeGen code_gen)
                {
			if (is_resolved)
				return;

                        if ((call_conv & PEAPI.CallConv.Vararg) == 0) {
                                string sig = MethodDef.CreateSignature (ret_type, call_conv, name, param, gen_param_count, false);
                                peapi_method = code_gen.ResolveMethod (sig);
                        } else {
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

                                string sig_only_required_params = MethodDef.CreateSignature (ret_type, call_conv, name, param, gen_param_count, false);
                                string sig_with_optional_params = MethodDef.CreateSignature (ret_type, call_conv, name, param, gen_param_count, true);
                                peapi_method = code_gen.ResolveVarargMethod (sig_only_required_params, sig_with_optional_params, code_gen,
                                                (PEAPI.Type[]) opt_list.ToArray (typeof (PEAPI.Type)));
                        }

                        peapi_method.AddCallConv (call_conv);
			
			is_resolved = true;
                }

        }

}

