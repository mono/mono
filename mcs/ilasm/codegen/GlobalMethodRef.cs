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

                private PEAPI.Method peapi_method;

                public GlobalMethodRef (ITypeRef ret_type, string name, ITypeRef[] param)
                {
                        this.ret_type = ret_type;
                        this.name = name;
                        this.param = param;
                }

                public PEAPI.Method PeapiMethod {
                        get { return peapi_method; }
                }

                public void Resolve (CodeGen code_gen)
                {
                        string sig = MethodDef.CreateSignature (name, new ArrayList (param));
                        peapi_method = code_gen.ResolveMethod (sig);
                }

        }

}

