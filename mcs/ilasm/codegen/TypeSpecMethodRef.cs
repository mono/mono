//
// Mono.ILASM.TypeSpecMethodRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;


namespace Mono.ILASM {

        public class TypeSpecMethodRef : IMethodRef {

                private PEAPI.MethodRef peapi_method;
                private BaseTypeRef owner;

                private PEAPI.CallConv call_conv;
                private BaseTypeRef ret_type;
                private string name;
                private BaseTypeRef[] param;
                private int gen_param_count;

                private bool is_resolved;

                public TypeSpecMethodRef (BaseTypeRef owner,
                                PEAPI.CallConv call_conv, BaseTypeRef ret_type,
                                string name, BaseTypeRef[] param, int gen_param_count)
                {
                        this.owner = owner;
                        this.call_conv = call_conv;
                        this.ret_type = ret_type;
                        this.name = name;
                        this.param = param;
                        this.gen_param_count = gen_param_count;
			if (gen_param_count > 0)
				CallConv |= PEAPI.CallConv.Generic;
                        is_resolved = false;
                }

                public PEAPI.Method PeapiMethod {
                        get {
                                return peapi_method;
                        }
                }

		public PEAPI.CallConv CallConv {
			get { return call_conv; }
			set { call_conv = value; }
		}

		public BaseTypeRef Owner {
			get { return owner; }
		}

                public void Resolve (CodeGen code_gen)
                {
                        if (is_resolved)
                                return;

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
                        peapi_method = code_gen.PEFile.AddMethodToTypeSpec (owner.PeapiType, write_name,
                                        ret_type.PeapiType, param_list, gen_param_count);

                        peapi_method.AddCallConv (call_conv);

                        is_resolved = true;
                }
        }

}



