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
                private PEAPI.Constant defval;

                public static readonly ParamDef Ellipsis = new ParamDef (new PEAPI.ParamAttr (), "ELLIPSIS", null);

                public ParamDef (PEAPI.ParamAttr attr, string name,
                                ITypeRef typeref) {
                        this.attr = attr;
                        this.name = name;
                        this.typeref = typeref;
                        is_defined = false;
                        defval = null;
                }

                public void AddDefaultValue (PEAPI.Constant cVal)
                {
                        defval = cVal;
                }

                public ITypeRef Type {
                        get { return typeref; }
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

                public bool IsSentinel ()
                {
                        return (typeref is SentinelTypeRef && this != Ellipsis);
                }

                public void Define (CodeGen code_gen)
                {
                        if (is_defined)
                                return;

                        typeref.Resolve (code_gen);

                        peapi_param = new PEAPI.Param (attr,
                                        name, typeref.PeapiType);
                        if (defval != null) {
                                peapi_param.AddDefaultValue (defval);
                        }

                        is_defined = true;
                }
        }

}

