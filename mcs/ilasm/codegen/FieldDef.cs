//
// Mono.ILASM.FieldDef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;

namespace Mono.ILASM {

        public class FieldDef {

                private string name;
                private ITypeRef type;
                private PEAPI.FieldAttr attr;
                private PEAPI.FieldDef field_def;

                private bool offset_set;
                private bool datavalue_set;
                private bool value_set;

                private uint offset;

                public FieldDef (PEAPI.FieldAttr attr, string name,
                                ITypeRef type)
                {
                        this.attr = attr;
                        this.name = name;
                        this.type = type;

                        offset_set = false;
                        datavalue_set = false;
                        value_set = false;
                }

                public string Name {
                        get { return name; }
                }

                public PEAPI.FieldDef Def {
                        get { return field_def; }
                }

                public void SetOffset (uint val) {
                        offset_set = true;
                        offset = val;
                }

                /// <summary>
                ///  Define a global field
                /// </summary>
                public void Define (CodeGen code_gen)
                {
                        type.Resolve (code_gen);

                        field_def = code_gen.PEFile.AddField (attr, name, type.PeapiType);

                        if (offset_set) {
                                field_def.SetOffset (offset);

                        }
                }

                /// <summary>
                ///  Define a field member of the specified class
                /// </summary>
                public void Define (CodeGen code_gen, PEAPI.ClassDef class_def)
                {
                        type.Resolve (code_gen);

                        class_def.AddField (attr, name, type.PeapiType);
                }
        }

}



