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
                private Hashtable field_table;
                private Hashtable method_table;
                private ArrayList data_list;
                private ArrayList customattr_list;
                private ArrayList event_list;
                private ArrayList property_list;
                private ArrayList typar_list;
                private Hashtable constraint_table;
                private TypeDef outer;

                private EventDef current_event;
                private PropertyDef current_property;

                private int size;
                private int pack;

                public TypeDef (PEAPI.TypeAttr attr, string name_space, string name,
                                IClassRef parent, ArrayList impl_list, Location location)
                {
                        this.attr = attr;
                        this.name_space = name_space;
                        this.name = name;
                        this.parent = parent;
                        this.impl_list = impl_list;
                        field_table = new Hashtable ();
                        method_table = new Hashtable ();
                        data_list = new ArrayList ();

                        size = -1;
                        pack = -1;

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

                public bool IsGenericType {
                        get { return (typar_list == null); }
                }

                public bool IsDefined {
                        get { return is_defined; }
                }

                public EventDef CurrentEvent {
                        get { return current_event; }
                }

                public PropertyDef CurrentProperty {
                        get { return current_property; }
                }

                public void SetSize (int size)
                {
                        this.size = size;
                }

                public void SetPack (int pack)
                {
                        this.pack = pack;
                }

                public void AddFieldDef (FieldDef fielddef)
                {
                        field_table.Add (fielddef.Name, fielddef);
                }

                public void AddDataDef (DataDef datadef)
                {
                        data_list.Add (datadef);
                }

                public void AddMethodDef (MethodDef methoddef)
                {
                        method_table.Add (methoddef.Signature, methoddef);
                }

                public void BeginEventDef (EventDef event_def)
                {
                        if (current_event != null)
                                throw new Exception ("An event definition was not closed.");

                        current_event = event_def;
                }

                public void EndEventDef ()
                {
                        if (event_list == null)
                                event_list = new ArrayList ();

                        event_list.Add (current_event);
                        current_event = null;
                }

                public void BeginPropertyDef (PropertyDef property_def)
                {
                        if (current_property != null)
                                throw new Exception ("A property definition was not closed.");

                        current_property = property_def;
                }

                public void EndPropertyDef ()
                {
                        if (property_list == null)
                                property_list = new ArrayList ();

                        property_list.Add (current_property);
                        current_property = null;
                }

                public void AddCustomAttribute (CustomAttr customattr)
                {
                        if (customattr_list == null)
                                customattr_list = new ArrayList ();

                        customattr_list.Add (customattr);
                }

                public void AddGenericParam (string id)
                {
                        if (typar_list == null)
                                typar_list = new ArrayList ();

                        typar_list.Add (id);
                }

                public void AddGenericConstraint (int index, ITypeRef constraint)
                {
                        if (constraint_table == null)
                                constraint_table = new Hashtable ();

                        constraint_table.Add (index, constraint);
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

                        if (size != -1)
                                classdef.AddLayoutInfo (pack, size);

                        if (typar_list != null) {
                                short index = 0;
                                foreach (string id in typar_list) {
                                        if (constraint_table != null && constraint_table.Contains ((int) index)) {
                                                ITypeRef constraint = (ITypeRef) constraint_table[(int) index];
                                                constraint.Resolve (code_gen);
                                                classdef.AddGenericParameter (index++, id, constraint.PeapiType);
                                        } else {
                                                classdef.AddGenericParameter (index++, id);
                                        }
                                }
                        }

                        is_intransit = false;
                        is_defined = true;

                        code_gen.AddToDefineContentsList (this);
                }

                public void DefineContents (CodeGen code_gen)
                {
                        foreach (FieldDef fielddef in field_table.Values) {
                                fielddef.Define (code_gen, classdef);
                        }

                        foreach (MethodDef methoddef in method_table.Values) {
                                methoddef.Define (code_gen, classdef);
                        }

                        if (event_list != null) {
                                foreach (EventDef eventdef in event_list) {
                                        eventdef.Define (code_gen, classdef);
                                }
                        }

                        if (property_list != null) {
                                foreach (PropertyDef propdef in property_list) {
                                        propdef.Define (code_gen, classdef);
                                }

                        }

                        if (customattr_list != null) {
                                foreach (CustomAttr customattr in customattr_list)
                                        customattr.AddTo (code_gen, classdef);
                        }
                }

                public PEAPI.MethodDef ResolveMethod (string signature, CodeGen code_gen)
                {
                        MethodDef methoddef = (MethodDef) method_table[signature];

                        return methoddef.Resolve (code_gen, classdef);
                }

                public PEAPI.Method ResolveVarargMethod (string signature,
                                CodeGen code_gen, PEAPI.Type[] opt)
                {
                        MethodDef methoddef = (MethodDef) method_table[signature];
                        methoddef.Resolve (code_gen, classdef);

                        return methoddef.GetVarargSig (opt);
                }

                public PEAPI.Field ResolveField (string name, CodeGen code_gen)
                {
                        FieldDef fielddef = (FieldDef) field_table[name];

                        return fielddef.Resolve (code_gen, classdef);
                }

                private string MakeFullName ()
                {
                        if (name_space == null || name_space == String.Empty)
                                return name;

                        return name_space + "." + name;
                }
        }

}

