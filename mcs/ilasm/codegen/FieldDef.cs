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

                private bool is_resolved;

                private uint offset;
                private PEAPI.Constant constant;

                public FieldDef (PEAPI.FieldAttr attr, string name,
                                ITypeRef type)
                {
                        this.attr = attr;
                        this.name = name;
                        this.type = type;

                        offset_set = false;
                        datavalue_set = false;
                        value_set = false;

                        is_resolved = false;
                }

                public string Name {
                        get { return name; }
                }

                public PEAPI.FieldDef PeapiFieldDef {
                        get { return field_def; }
                }

                public void SetOffset (uint val)
                {
                        offset_set = true;
                        offset = val;
                }

                public void SetValue (PEAPI.Constant constant)
                {
                        value_set = true;
                        this.constant = constant;
                }

                public PEAPI.FieldDef Resolve (CodeGen code_gen)
                {
                        if (is_resolved)
                                return field_def;

                        type.Resolve (code_gen);
                        field_def = code_gen.PEFile.AddField (attr, name, type.PeapiType);

                        is_resolved = true;

                        return field_def;
                }

                public PEAPI.FieldDef Resolve (CodeGen code_gen, PEAPI.ClassDef classdef)
                {
                        if (is_resolved)
                                return field_def;

                        type.Resolve (code_gen);
                        field_def = classdef.AddField (attr, name, type.PeapiType);

                        is_resolved = true;

                        return field_def;
                }

                /// <summary>
                ///  Define a global field
                /// </summary>
                public void Define (CodeGen code_gen)
                {
                        Resolve (code_gen);
                        WriteCode (field_def);
                }

                /// <summary>
                ///  Define a field member of the specified class
                /// </summary>
                public void Define (CodeGen code_gen, PEAPI.ClassDef class_def)
                {
                        Resolve (code_gen, class_def);
                        WriteCode (field_def);
                }

                protected void WriteCode (PEAPI.FieldDef field_def)
                {
                        if (offset_set)
                                field_def.SetOffset (offset);

                        if (value_set)
                                field_def.AddValue (constant);
                }
        }

}

