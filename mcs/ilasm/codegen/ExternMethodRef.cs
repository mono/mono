//
// Mono.ILASM.ExternMethodRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;

namespace Mono.ILASM {

        public class ExternMethodRef : IMethodRef {

                private ExternTypeRef owner;
                private ITypeRef ret_type;
                private string name;
                private ITypeRef[] param;

                private PEAPI.MethodRef peapi_method;

                public ExternMethodRef (ExternTypeRef owner,
                        ITypeRef ret_type, string name, ITypeRef[] param)
                {
                        this.owner = owner;
                        this.ret_type = ret_type;
                        this.name = name;
                        this.param = param;
                }

                public PEAPI.Method PeapiMethod {
                        get { return peapi_method; }
                }

                public void Resolve (CodeGen code_gen)
                {
                        PEAPI.Type[] param_list = new PEAPI.Type[param.Length];
                        PEAPI.ClassRef owner_ref;

                        ret_type.Resolve (code_gen);

                        int count = 0;
                        foreach (ITypeRef typeref in param) {
                                typeref.Resolve (code_gen);
                                param_list[count++] = typeref.PeapiType;
                        }

                        owner.Resolve (code_gen);
                        owner_ref = owner.PeapiClassRef;

                        peapi_method = owner_ref.AddMethod (name,
                                        ret_type.PeapiType, param_list);
                }
        }

}


