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
                private PEAPI.CallConv call_conv;
                private ITypeRef ret_type;
                private string name;
                private ITypeRef[] param;

                private PEAPI.Method peapi_method;

                public MethodRef (TypeRef owner, PEAPI.CallConv call_conv,
                        ITypeRef ret_type, string name, ITypeRef[] param)
                {
                        this.owner = owner;
                        this.call_conv = call_conv;
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
                        string write_name;

                        if (name == "<init>")
                                write_name = ".ctor";
                        else
                                write_name = name;

                        string sig = MethodDef.CreateSignature (name, param);


                        if ((call_conv & PEAPI.CallConv.Vararg) == 0) {
                                peapi_method = owner_def.ResolveMethod (sig, code_gen);
                        } else {
                                ArrayList opt_list = new ArrayList ();
                                bool in_opt = false;
                                foreach (ITypeRef type in param) {
                                        if (TypeRef.Ellipsis == type) {
                                                in_opt = true;
                                        } else if (in_opt) {
                                                type.Resolve (code_gen);
                                                opt_list.Add (type.PeapiType);
                                        }
                                }
                                peapi_method = owner_def.ResolveVarargMethod (sig, code_gen,
                                                (PEAPI.Type[]) opt_list.ToArray (typeof (PEAPI.Type)));
                        }

                }

        }

}

