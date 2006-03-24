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

        public class TypeSpecMethodRef : BaseMethodRef {

                public TypeSpecMethodRef (BaseTypeRef owner,
                                PEAPI.CallConv call_conv, BaseTypeRef ret_type,
                                string name, BaseTypeRef[] param, int gen_param_count)
                        : base (owner, call_conv, ret_type, name, param, gen_param_count)
                {
                }

                public override void Resolve (CodeGen code_gen)
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



