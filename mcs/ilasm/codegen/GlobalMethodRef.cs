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

        public class GlobalMethodRef : IMethodRef {

                private ITypeRef ret_type;
                private string name;
                private ITypeRef[] param;
                private PEAPI.CallConv call_conv;

                private PEAPI.Method peapi_method;
		private bool is_resolved;

                public GlobalMethodRef (ITypeRef ret_type, PEAPI.CallConv call_conv,
                                string name, ITypeRef[] param)
                {
                        this.ret_type = ret_type;
                        this.call_conv = call_conv;
                        this.name = name;
                        this.param = param;

			is_resolved = false;
                }

                public PEAPI.Method PeapiMethod {
                        get { return peapi_method; }
                }

		public PEAPI.CallConv CallConv {
			get { return call_conv; }
			set { call_conv = value; }
		}

                public void Resolve (CodeGen code_gen)
                {
			if (is_resolved)
				return;

                        string sig = MethodDef.CreateSignature (ret_type, name, param);

                        if ((call_conv & PEAPI.CallConv.Vararg) == 0) {
                                peapi_method = code_gen.ResolveMethod (sig);
                        } else {
                                ArrayList opt_list = new ArrayList ();
                                bool in_opt = false;
                                foreach (ITypeRef type in param) {
                                        if (type is SentinelTypeRef) {
                                                in_opt = true;
                                        } else if (in_opt) {
                                                type.Resolve (code_gen);
                                                opt_list.Add (type.PeapiType);
                                        }
                                }
                                peapi_method = code_gen.ResolveVarargMethod (sig, code_gen,
                                                (PEAPI.Type[]) opt_list.ToArray (typeof (PEAPI.Type)));
                        }

                        peapi_method.AddCallConv (call_conv);
			
			is_resolved = true;
                }

        }

}

