//
// Mono.ILASM.FieldDef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;
using System.Collections;

namespace Mono.ILASM {

        public class FieldDef : ICustomAttrTarget {

                private string name;
                private BaseTypeRef type;
                private PEAPI.FieldAttr attr;
                private PEAPI.FieldDef field_def;
                private ArrayList customattr_list;
                private PEAPI.NativeType native_type;

                private bool offset_set;
//                private bool datavalue_set;
                private bool value_set;

                private bool is_resolved;

                private uint offset;
                private PEAPI.Constant constant;
                private string at_data_id;

                public FieldDef (PEAPI.FieldAttr attr, string name,
                                BaseTypeRef type)
                {
                        this.attr = attr;
                        this.name = name;
                        this.type = type;

                        offset_set = false;
//                        datavalue_set = false;
                        value_set = false;

                        at_data_id = null;

                        is_resolved = false;
                }

                public string Name {
                        get { return name; }
                }

                public PEAPI.FieldDef PeapiFieldDef {
                        get { return field_def; }
                }

                public bool IsStatic {
                        get { return (attr & PEAPI.FieldAttr.Static) != 0; }
                }

                public PEAPI.FieldAttr Attributes {
                        get { return attr; }
                        set { attr = value; }
                }

                public BaseTypeRef Type {
                        get { return type; }
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

                public void AddDataValue (string at_data_id)
                {
                        this.at_data_id = at_data_id;
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

                        if (customattr_list != null)
                                foreach (CustomAttr customattr in customattr_list)
                                        customattr.AddTo (code_gen, field_def);

                        if (native_type != null)
                                field_def.SetMarshalInfo (native_type);

                        is_resolved = true;

                        return field_def;
                }

                /// <summary>
                ///  Define a global field
                /// </summary>
                public void Define (CodeGen code_gen)
                {
                        Resolve (code_gen);
                        WriteCode (code_gen, field_def);
                }

                /// <summary>
                ///  Define a field member of the specified class
                /// </summary>
                public void Define (CodeGen code_gen, PEAPI.ClassDef class_def)
                {
                        Resolve (code_gen, class_def);
                        WriteCode (code_gen, field_def);
                }

                protected void WriteCode (CodeGen code_gen, PEAPI.FieldDef field_def)
                {
                        if (offset_set)
                                field_def.SetOffset (offset);

			if (value_set) {
				PEAPI.ByteArrConst dc = constant as PEAPI.ByteArrConst;
				if (dc != null)
					dc.Type = type.PeapiType;
				field_def.AddValue (constant);
			}

                        if (at_data_id != null) {
                                PEAPI.DataConstant dc = code_gen.GetDataConst (at_data_id);
                                field_def.AddDataValue (dc);
                        }
                }
        }

}

