//
// Mono.ILASM.TypeDef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;
using System.Collections;

namespace Mono.ILASM {

        public class TypeDef {

                private PEAPI.TypeAttr attr;
                private string name_space;
                private string name;
                private bool is_defined;
                private bool is_intransit;
                private IClassRef parent;
                private ArrayList impl_list;
                private PEAPI.ClassDef classdef;
                private ArrayList field_list;
                private ArrayList method_list;
                private ArrayList data_list;
                private TypeDef outer;

                public TypeDef (PEAPI.TypeAttr attr, string name_space, string name,
                                IClassRef parent, ArrayList impl_list, Location location)
                {
                        this.attr = attr;
                        this.name_space = name_space;
                        this.name = name;
                        this.parent = parent;
                        this.impl_list = impl_list;
                        field_list = new ArrayList ();
                        method_list = new ArrayList ();
                        data_list = new ArrayList ();

                        is_defined = false;
                        is_intransit = false;
                }

                public string Name {
                        get { return name; }
                }

                public string FullName {
                        get { return MakeFullName (); }
                }

                public TypeDef OuterType {
                        get { return outer; }
                        set { outer = value; }
                }

                public PEAPI.ClassDef PeapiType {
                        get { return classdef; }
                }

                public PEAPI.ClassDef ClassDef {
                        get { return classdef; }
                }

                public bool IsDefined {
                        get { return is_defined; }
                }

                public void AddFieldDef (FieldDef fielddef)
                {
                        field_list.Add (fielddef);
                }

                public void AddDataDef (DataDef datadef)
                {
                        data_list.Add (datadef);
                }

                public void AddMethodDef (MethodDef methoddef)
                {
                        method_list.Add (methoddef);
                }

                public void Define (CodeGen code_gen)
                {
                        if (is_defined)
                                return;

                        if (is_intransit) {
                                // Circular definition
                                throw new Exception ("Circular definition of class: " + FullName);
                        }

                        if (parent != null) {
                                is_intransit = true;
                                parent.Resolve (code_gen);
                                is_intransit = false;
                                if (parent.PeapiClass == null) {
                                        throw new Exception ("this type can not be a base type: "
                                                        + parent);
                                }
                                if (outer != null) {
                                        if (!outer.IsDefined)
                                                outer.Define (code_gen);
                                        classdef = outer.PeapiType.AddNestedClass (attr,
                                                        name_space, name, parent.PeapiClass);
                                } else {
                                        classdef = code_gen.PEFile.AddClass (attr,
                                                name_space, name, parent.PeapiClass);
                                }
                        } else {
                                if (outer != null) {
                                        if (!outer.IsDefined)
                                                outer.Define (code_gen);
                                        classdef = outer.PeapiType.AddNestedClass (attr,
                                                name_space, name);
                                } else {
                                        classdef = code_gen.PEFile.AddClass (attr,
                                                name_space, name);
                                }
                        }

                        is_intransit = false;
                        is_defined = true;

                        foreach (FieldDef fielddef in field_list) {
                                fielddef.Define (code_gen, classdef);
                        }

                        foreach (MethodDef methoddef in method_list) {
                                methoddef.Define (code_gen, classdef);
                        }
                }

                private string MakeFullName ()
                {
                        if (name_space == null || name_space == String.Empty)
                                return name;

                        return name_space + "." + name;
                }
        }

}

