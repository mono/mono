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

        public class MethodRef : IMethodRef {

                private TypeRef owner;
                private ITypeRef ret_type;
                private string name;
                private ITypeRef[] param;

                private PEAPI.Method peapi_method;

                public MethodRef (TypeRef owner,
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
                        TypeDef owner_def = code_gen.TypeManager[owner.FullName];
                        string sig = MethodDef.CreateSignature (name, param);
                        peapi_method = owner_def.ResolveMethod (sig, code_gen);
                }

        }

}

