//
// Mono.ILASM.ParamDef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;


namespace Mono.ILASM {

        /// <summary>
        ///  Definition of a parameter passed to a method
        /// </summary>
        public class ParamDef {

                private PEAPI.ParamAttr attr;
                private string name;
                private ITypeRef typeref;
                private bool is_defined;
                private PEAPI.Param peapi_param;

                public ParamDef (PEAPI.ParamAttr attr, string name,
                                ITypeRef typeref) {
                        this.attr = attr;
                        this.name = name;
                        this.typeref = typeref;
                        is_defined = false;
                }

                public string TypeName {
                        get { return typeref.FullName; }
                }

                public string Name {
                        get { return name; }
                }

                public PEAPI.Param PeapiParam {
                        get { return peapi_param; }
                }

                public void Define (CodeGen code_gen)
                {
                        if (is_defined)
                                return;

                        typeref.Resolve (code_gen);

                        peapi_param = new PEAPI.Param (attr,
                                        name, typeref.PeapiType);

                        is_defined = true;
                }
        }

}

