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
                private ITypeRef owner;

                private PEAPI.CallConv call_conv;
                private ITypeRef ret_type;
                private string name;
                private ITypeRef[] param;

                private bool is_resolved;

                public TypeSpecMethodRef (ITypeRef owner,
                                ITypeRef ret_type, PEAPI.CallConv call_conv,
                                string name, ITypeRef[] param)
                {
                        this.owner = owner;
                        this.call_conv = call_conv;
                        this.ret_type = ret_type;
                        this.name = name;
                        this.param = param;
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

                public void Resolve (CodeGen code_gen)
                {
                        if (is_resolved)
                                return;

                        PEAPI.Type[] param_list = new PEAPI.Type[param.Length];
                        string write_name;

                        ret_type.Resolve (code_gen);

                        int count = 0;
                        foreach (ITypeRef typeref in param) {
                                typeref.Resolve (code_gen);
                                param_list[count++] = typeref.PeapiType;
                        }

                        if (name == "<init>")
                                write_name = ".ctor";
                        else
                                write_name = name;

                        owner.Resolve (code_gen);
                        peapi_method = code_gen.PEFile.AddMethodToTypeSpec (owner.PeapiType, write_name,
                                        ret_type.PeapiType, param_list);

                        peapi_method.AddCallConv (call_conv);
                }
        }

}



