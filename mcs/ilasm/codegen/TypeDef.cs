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

                protected class GenericInfo {
                        public string Id;
                        public ArrayList ConstraintList;
                }

                private PEAPI.TypeAttr attr;
                private string name_space;
                private string name;
                private bool is_defined;
                private bool is_intransit;
                private IClassRef parent;
                private ArrayList impl_list;
                private PEAPI.ClassDef classdef;
                private Hashtable field_table;
                private ArrayList field_list;
                private Hashtable method_table;
                private ArrayList customattr_list;
                private ArrayList event_list;
                private ArrayList property_list;
                private ArrayList typar_list;
                private ArrayList override_list;
                private ArrayList override_long_list;
                private TypeDef outer;

                private EventDef current_event;
                private PropertyDef current_property;

                private int size;
                private int pack;

                private bool is_value_class;
                private bool is_enum_class;

                public TypeDef (PEAPI.TypeAttr attr, string name_space, string name,
                                IClassRef parent, ArrayList impl_list, Location location)
                {
                        this.attr = attr;
                        this.name_space = name_space;
                        this.name = name;
                        this.parent = parent;
                        this.impl_list = impl_list;

                        field_table = new Hashtable ();
                        field_list = new ArrayList ();

                        method_table = new Hashtable ();

                        size = -1;
                        pack = -1;

                        is_defined = false;
                        is_intransit = false;

                        is_value_class = false;
                        is_value_class = false;
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

                public bool IsInterface {
                        get { return (attr & PEAPI.TypeAttr.Interface) != 0; }
                }

                public void AddOverride (MethodDef body, ITypeRef parent, string name)
                {
                        if (override_list == null)
                                override_list = new ArrayList ();
                        override_list.Add (new DictionaryEntry (body,
                                           new DictionaryEntry (parent, name)));
                }

                public void AddOverride (string sig, IMethodRef decl)
                {
                        if (override_long_list == null)
                                override_long_list = new ArrayList ();
                        override_long_list.Add (new DictionaryEntry (sig,
                                                                decl));
                }

                public void MakeValueClass ()
                {
                        is_value_class = true;
                }

                public void MakeEnumClass ()
                {
                        is_enum_class = true;
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
                        if (IsInterface && !fielddef.IsStatic) {
                                Console.WriteLine ("warning -- Non-static field in interface, set to such");
                                fielddef.Attributes |= PEAPI.FieldAttr.Static;
                        }

                        field_table.Add (fielddef.Name, fielddef);
                        field_list.Add (fielddef);
                }

                public void AddMethodDef (MethodDef methoddef)
                {
                        if (IsInterface && !(methoddef.IsVirtual || methoddef.IsAbstract)) {
                                Console.WriteLine ("warning -- Non-virtual, non-abstract instance method in interface, set to such");
                                methoddef.Attributes |= PEAPI.MethAttr.Abstract | PEAPI.MethAttr.Virtual;
                        }

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

                        GenericInfo gi = new GenericInfo ();
                        gi.Id = id;

                        typar_list.Add (gi);
                }

                public void AddGenericConstraint (int index, ITypeRef constraint)
                {
                        GenericInfo gi = (GenericInfo) typar_list[index];

                        if (gi.ConstraintList == null)
                                gi.ConstraintList = new ArrayList ();
                        gi.ConstraintList.Add (constraint);
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
                                        if (is_value_class) {
                                                // Should probably confirm that the parent is System.ValueType
                                                classdef = code_gen.PEFile.AddValueClass (attr,
                                                        name_space, name);
                                        } else {
                                                classdef = code_gen.PEFile.AddClass (attr,
                                                        name_space, name, parent.PeapiClass);
                                        }
                                }
                        } else {
                                if (outer != null) {
                                        if (!outer.IsDefined)
                                                outer.Define (code_gen);
                                        classdef = outer.PeapiType.AddNestedClass (attr,
                                                name_space, name);
                                } else {
                                        if (is_value_class) {
                                                classdef = code_gen.PEFile.AddValueClass (attr,
                                                        name_space, name);
                                        } else {
                                                classdef = code_gen.PEFile.AddClass (attr,
                                                        name_space, name);
                                        }
                                }
                                if (FullName == "System.Object")
                                        classdef.SpecialNoSuper ();
                        }

                        if (size != -1)
                                classdef.AddLayoutInfo (pack, size);

                        if (impl_list != null) {
                                foreach (IClassRef impl in impl_list) {
                                        impl.Resolve (code_gen);
                                        classdef.AddImplementedInterface (impl.PeapiClass);
                                }
                        }

                        if (typar_list != null) {
                                short index = 0;
                                foreach (GenericInfo gi in typar_list) {
                                        PEAPI.GenericParameter gp = classdef.AddGenericParameter (index++, gi.Id);
                                        if (gi.ConstraintList != null) {
                                                foreach (ITypeRef cnst in gi.ConstraintList) {
                                                        cnst.Resolve (code_gen);
                                                        gp.AddConstraint (cnst.PeapiType);
                                                }
                                        }
                                }
                        }

                        is_intransit = false;
                        is_defined = true;

                        code_gen.AddToDefineContentsList (this);
                }

                public void DefineContents (CodeGen code_gen)
                {
                        ArrayList fielddef_list = new ArrayList ();
                        foreach (FieldDef fielddef in field_list) {
                                fielddef.Define (code_gen, classdef);
                                fielddef_list.Add (fielddef.PeapiFieldDef);
                        }

                        classdef.SetFieldOrder (fielddef_list);

                        foreach (MethodDef methoddef in method_table.Values) {
                                methoddef.Define (code_gen, this);
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

                        if (override_list != null) {
                                foreach (DictionaryEntry entry in override_list) {
                                        MethodDef body = (MethodDef) entry.Key;
                                        DictionaryEntry decl = (DictionaryEntry) entry.Value;
                                        ITypeRef parent_type = (ITypeRef) decl.Key;
                                        parent_type.Resolve (code_gen);
                                        string over_name = (string) decl.Value;
                                        IMethodRef over_meth = parent_type.GetMethodRef (body.RetType,
                                                        body.CallConv, over_name, body.ParamTypeList ());
                                        over_meth.Resolve (code_gen);
                                        classdef.AddMethodOverride (over_meth.PeapiMethod,
                                                        body.PeapiMethodDef);
                                }
                        }

                        if (override_long_list != null) {
                                foreach (DictionaryEntry entry in override_long_list) {
                                        string sig = (string) entry.Key;
                                        IMethodRef decl = (IMethodRef) entry.Value;
                                        MethodDef body = (MethodDef) method_table[sig];
                                        decl.Resolve (code_gen);
                                        classdef.AddMethodOverride (decl.PeapiMethod,
                                                        body.PeapiMethodDef);
                                }
                        }
                }

                public PEAPI.MethodDef ResolveMethod (string signature, CodeGen code_gen)
                {
                        MethodDef methoddef = (MethodDef) method_table[signature];

                        if (methoddef == null) {
                                code_gen.Report.Error ("Unable to resolve method: " + signature);
                                Environment.Exit (1);
                        }

                        return methoddef.Resolve (code_gen, classdef);
                }

                public PEAPI.Method ResolveVarargMethod (string signature,
                                CodeGen code_gen, PEAPI.Type[] opt)
                {
                        MethodDef methoddef = (MethodDef) method_table[signature];

                        if (methoddef == null) {
                                code_gen.Report.Error ("Unable to resolve method: " + signature);
                                Environment.Exit (1);
                        }

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

