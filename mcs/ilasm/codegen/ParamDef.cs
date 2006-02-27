//
// Mono.ILASM.ParamDef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;
using System.Collections;


namespace Mono.ILASM {

        /// <summary>
        ///  Definition of a parameter passed to a method
        /// </summary>
        public class ParamDef : ICustomAttrTarget {

                private PEAPI.ParamAttr attr;
                private string name;
                private BaseTypeRef typeref;
                private bool is_defined;
                private PEAPI.Param peapi_param;
                private PEAPI.Constant defval;
                private ArrayList customattr_list;
                private PEAPI.NativeType native_type;

                public static readonly ParamDef Ellipsis = new ParamDef (new PEAPI.ParamAttr (), "ELLIPSIS", null);

                public ParamDef (PEAPI.ParamAttr attr, string name,
                                BaseTypeRef typeref) {
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

                public void AddCustomAttribute (CustomAttr customattr)
                {
                        if (customattr_list == null)
                                customattr_list = new ArrayList ();

                        customattr_list.Add (customattr);
                }

                public void AddMarshalInfo (PEAPI.NativeType native_type)
                {
                        this.native_type = native_type;
                }

                public BaseTypeRef Type {
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

                        if (customattr_list != null)
                                foreach (CustomAttr customattr in customattr_list)
                                        customattr.AddTo (code_gen, peapi_param);

                        if (native_type != null)
                                peapi_param.AddMarshallInfo (native_type);

                        is_defined = true;
                }
        }

}

