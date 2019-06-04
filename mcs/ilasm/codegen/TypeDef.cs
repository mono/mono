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
using System.Security;

namespace Mono.ILASM {

        public class TypeDef : ICustomAttrTarget, IDeclSecurityTarget, IComparable {

                private PEAPI.TypeAttr attr;
                private string name_space;
                private string name;
                private bool is_defined;
                private bool is_intransit;
                private BaseClassRef parent;
                private ArrayList impl_list;
                private PEAPI.ClassDef classdef;
                private Hashtable field_table;
                private ArrayList field_list;
                private Hashtable method_table;
                private ArrayList method_list;
                private ArrayList customattr_list;
                private DeclSecurity decl_sec;
                private ArrayList event_list;
                private ArrayList property_list;
                private GenericParameters gen_params;
                private ArrayList override_list;
                private ArrayList override_long_list;
                private TypeDef outer;

                private EventDef current_event;
                private PropertyDef current_property;

                private int size;
                private int pack;

                private bool is_value_class;
                private bool is_enum_class;

                private Location location;

                public TypeDef (PEAPI.TypeAttr attr, string name_space, string name,
                                BaseClassRef parent, ArrayList impl_list, Location location, GenericParameters gen_params, TypeDef outer)
                {
                        this.attr = attr;
                        this.parent = parent;
                        this.impl_list = impl_list;
                        this.gen_params = gen_params;
                        this.outer = outer;
                        this.location = location;

                        field_table = new Hashtable ();
                        field_list = new ArrayList ();

                        method_table = new Hashtable ();
                        method_list = new ArrayList ();

                        size = -1;
                        pack = -1;

                        is_defined = false;
                        is_intransit = false;

                        is_value_class = false;
                        is_enum_class = false;

                        ResolveGenParams ();

                        int lastdot = name.LastIndexOf ('.');
                        /* Namespace . name split should not be done for nested classes */
                        if (lastdot >= 0 && outer == null) {
                                if (name_space == null || name_space == "")
                                        this.name_space = name.Substring (0, lastdot);
                                else
                                        this.name_space = name_space + "." + name.Substring (0, lastdot);
                                this.name = name.Substring (lastdot + 1);
                        } else {
                                this.name_space = name_space;
                                this.name = name;
                        }

                        //Fixup attributes
                        if (IsInterface)
                                this.attr |= PEAPI.TypeAttr.Abstract;
                }

				public bool NoAutoInherit { get; set; }

                public string Name {
                        get { return name; }
                }

                public string FullName {
                        get { return MakeFullName (); }
                }

		public string NestedFullName {
			get { return (outer == null ? FullName : (outer.NestedFullName + "/" + FullName)); }
		}

                public TypeDef OuterType {
                        get { return outer; }
                }

                public PEAPI.ClassDef PeapiType {
                        get { return classdef; }
                }

                public PEAPI.ClassDef ClassDef {
                        get { return classdef; }
                }

                public bool IsGenericType {
                        get { return (gen_params == null); }
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

                public bool IsAbstract {
                        get { return (attr & PEAPI.TypeAttr.Abstract) != 0; }
                }

                public GenericParameters TypeParameters {
                        get { return gen_params; }
                }

                public DeclSecurity DeclSecurity {
                        get {
                                if (decl_sec == null)
                                        decl_sec = new DeclSecurity ();
                                return decl_sec;
                        }
                }

                public void AddOverride (MethodDef body, BaseTypeRef parent, string name)
                {
                        if (override_list == null)
                                override_list = new ArrayList ();
                        override_list.Add (new DictionaryEntry (body,
                                           new DictionaryEntry (parent, name)));
                }

                public void AddOverride (string sig, BaseMethodRef decl)
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
                                Report.Warning ("Non-static field in interface, set to such");
                                fielddef.Attributes |= PEAPI.FieldAttr.Static;
                        }

                        DictionaryEntry entry = new DictionaryEntry (fielddef.Name, fielddef.Type.FullName);
                        if (field_table [entry] != null)
                                Report.Error ("Duplicate field declaration: " + fielddef.Type.FullName + " " + fielddef.Name);
                        field_table.Add (entry, fielddef);
                        field_list.Add (fielddef);
                }

                public void AddMethodDef (MethodDef methoddef)
                {
                        if (method_table [methoddef.Signature] != null)
                                Report.Error (methoddef.StartLocation, "Duplicate method declaration: " + methoddef.Signature);

                        method_table.Add (methoddef.Signature, methoddef);
                        method_list.Add (methoddef);
                }

                public void BeginEventDef (EventDef event_def)
                {
                        if (current_event != null)
                                Report.Error ("An event definition was not closed.");

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
                                Report.Error ("A property definition was not closed.");

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

                public GenericParameter GetGenericParam (string id)
                {
                        if (gen_params == null)
                                return null;
                        
                        return gen_params.GetGenericParam (id);
                }

                public GenericParameter GetGenericParam (int index)
                {
                        if (gen_params == null || index < 0 || index >= gen_params.Count)
                                return null;
                        
                        return gen_params [index];
                }

                public int GetGenericParamNum (string id)
                {
                        if (gen_params == null)
                                return -1;
                        
                        return gen_params.GetGenericParamNum (id);
                }

                /* Resolve any GenParams in constraints, parent & impl_list */
                private void ResolveGenParams ()
                {
                        if (gen_params == null)
                                return;

                        gen_params.ResolveConstraints (gen_params, null);

                        BaseGenericTypeRef gtr = parent as BaseGenericTypeRef;
                        if (gtr != null)
                                gtr.Resolve (gen_params, null);
                        
                        if (impl_list == null)
                                return;
                                
                        foreach (BaseClassRef impl in impl_list) {
                                gtr = impl as BaseGenericTypeRef;
                                if (gtr != null)
                                        gtr.Resolve (gen_params, null);
                        }
                }

                private bool IsValueType (string ns, string name)
                {
                        return (ns == "System" && name == "ValueType");
                }

                private bool IsEnumType (string ns, string name)
                {
                        return (ns == "System" && name == "Enum");
                }

                public void Define (CodeGen code_gen)
                {
                        if (is_defined)
                                return;

                        if (is_intransit) {
                                // Circular definition
                                Report.Error ("Circular definition of class: " + FullName);
                        }

                        if (outer != null) {
				PEAPI.TypeAttr vis = attr & PEAPI.TypeAttr.VisibilityMask;

				if (vis == PEAPI.TypeAttr.Private || vis == PEAPI.TypeAttr.Public) {
					/* Nested class, but attr not set accordingly. */
					Report.Warning (location, String.Format ("Nested class '{0}' has non-nested visibility, set to such.", NestedFullName));
					attr = attr ^ vis;
					attr |= (vis == PEAPI.TypeAttr.Public ? PEAPI.TypeAttr.NestedPublic : PEAPI.TypeAttr.NestedPrivate);
				}		
                        }
                        
                        if (parent != null) {
                                is_intransit = true;
                                parent.Resolve (code_gen);

                                is_intransit = false;
                                if (parent.PeapiClass == null) {
                                        Report.Error ("this type can not be a base type: "
                                                        + parent);
                                }

                                if (IsValueType (parent.PeapiClass.nameSpace, parent.PeapiClass.name))
                                        is_value_class = true;
                                else if (IsEnumType (parent.PeapiClass.nameSpace, parent.PeapiClass.name)) {
                                        is_enum_class = true;
                                        is_value_class = false;
                                }

                                if (!IsValueType (name_space, name) && !IsEnumType (name_space, name) &&
                                        is_value_class && (attr & PEAPI.TypeAttr.Sealed) == 0) {

                                        attr |= PEAPI.TypeAttr.Sealed;
                                }

                                if (outer != null) {
                                        if (!outer.IsDefined)
                                                outer.Define (code_gen);
                                        classdef = outer.PeapiType.AddNestedClass (attr,
                                                        name_space, name, parent.PeapiClass);
                                } else {
                                        if (is_value_class || is_enum_class) {
                                                // Should probably confirm that the parent is System.ValueType
                                                classdef = code_gen.PEFile.AddValueClass (attr,
                                                        name_space, name, is_value_class ? PEAPI.ValueClass.ValueType : PEAPI.ValueClass.Enum);
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
                                        if (is_value_class || is_enum_class) {
                                                classdef = code_gen.PEFile.AddValueClass (attr,
                                                        name_space, name, is_value_class ? PEAPI.ValueClass.ValueType : PEAPI.ValueClass.Enum);
                                        } else {
                                                classdef = code_gen.PEFile.AddClass (attr,
                                                        name_space, name);
                                        }
                                }
                                if (FullName == "System.Object" || NoAutoInherit)
                                        classdef.SpecialNoSuper ();
                        }

                        is_defined = true;

                        if (size != -1 || pack != -1)
                                classdef.AddLayoutInfo ( (pack == -1) ? 1 : pack, (size == -1) ? 0 : size);

                        if (impl_list != null) {
                                foreach (BaseClassRef impl in impl_list) {
                                        impl.Resolve (code_gen);
                                        classdef.AddImplementedInterface (impl.PeapiClass);
                                }
                        }

                        if (gen_params != null)
                                gen_params.Resolve (code_gen, classdef);

                        is_intransit = false;

                        code_gen.AddToDefineContentsList (this);
                }

                public void DefineContents (CodeGen code_gen)
                {
                        ArrayList fielddef_list = new ArrayList ();
                        foreach (FieldDef fielddef in field_list) {
                                if (is_enum_class && fielddef.Name == "value__") {
                                    fielddef.Attributes |= PEAPI.FieldAttr.SpecialName | PEAPI.FieldAttr.RTSpecialName;
                                }

                                fielddef.Define (code_gen, classdef);
                                fielddef_list.Add (fielddef.PeapiFieldDef);
                        }

                        classdef.SetFieldOrder (fielddef_list);

                        foreach (MethodDef methoddef in method_list) {
                                methoddef.Define (code_gen);
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
                                foreach (CustomAttr customattr in customattr_list) {
                                        customattr.AddTo (code_gen, classdef);
                                        if (customattr.IsSuppressUnmanaged (code_gen))
                                                classdef.AddAttribute (PEAPI.TypeAttr.HasSecurity);
				}
                        }
                        
                        /// Add declarative security to this class
                        if (decl_sec != null) {
                                decl_sec.AddTo (code_gen, classdef);
                                classdef.AddAttribute (PEAPI.TypeAttr.HasSecurity);
			}	

                        if (override_list != null) {
                                foreach (DictionaryEntry entry in override_list) {
                                        MethodDef body = (MethodDef) entry.Key;
                                        DictionaryEntry decl = (DictionaryEntry) entry.Value;
                                        BaseTypeRef parent_type = (BaseTypeRef) decl.Key;
                                        parent_type.Resolve (code_gen);
                                        string over_name = (string) decl.Value;
                                        BaseMethodRef over_meth = parent_type.GetMethodRef (body.RetType,
                                                        body.CallConv, over_name, body.ParamTypeList (), body.GenParamCount);
                                        over_meth.Resolve (code_gen);
                                        classdef.AddMethodOverride (over_meth.PeapiMethod,
                                                        body.PeapiMethodDef);
                                }
                        }

                        if (override_long_list != null) {
                                foreach (DictionaryEntry entry in override_long_list) {
                                        string sig = (string) entry.Key;
                                        BaseMethodRef decl = (BaseMethodRef) entry.Value;
                                        MethodDef body = (MethodDef) method_table[sig];
                                        decl.Resolve (code_gen);
                                        classdef.AddMethodOverride (decl.PeapiMethod,
                                                        body.PeapiMethodDef);
                                }
                        }
                }

                public PEAPI.Method ResolveMethod (BaseTypeRef ret_type, PEAPI.CallConv call_conv,
                        string name, BaseTypeRef [] param, int gen_param_count, CodeGen code_gen)
                {
                        string signature = MethodDef.CreateSignature (ret_type, call_conv, name, param, gen_param_count, false);
                        MethodDef methoddef = (MethodDef) method_table[signature];

                        if (methoddef != null)
                                return methoddef.Resolve (code_gen, classdef);
                        return ResolveAsMethodRef (ret_type, call_conv, name, param, gen_param_count, code_gen);
                }

                public PEAPI.Method ResolveVarargMethod (BaseTypeRef ret_type, PEAPI.CallConv call_conv,
                        string name, BaseTypeRef [] param, int gen_param_count, PEAPI.Type [] opt, CodeGen code_gen)
                {
                        // Only MethodDef sig required to lookup in the method_table
                        string signature = MethodDef.CreateSignature (ret_type, call_conv, name, param, 0, false);
                        MethodDef methoddef = (MethodDef) method_table[signature];
                        if (methoddef != null) {
                                methoddef.Resolve (code_gen, classdef);
                                return methoddef.GetVarargSig (
                                                opt,
                                                MethodDef.CreateSignature (ret_type, call_conv, name, param, 0, true));
                        }
                        
                        return ResolveAsMethodRef (ret_type, call_conv, name, param, gen_param_count, code_gen);
                }

                private PEAPI.Method ResolveAsMethodRef (BaseTypeRef ret_type, PEAPI.CallConv call_conv,
                        string name, BaseTypeRef [] param, int gen_param_count, CodeGen code_gen)
                {
                        ExternTypeRef type_ref = code_gen.ThisModule.GetTypeRef (FullName, false);
                        ExternMethodRef methodref = (ExternMethodRef) type_ref.GetMethodRef (ret_type, call_conv, name, param, gen_param_count);
                        methodref.Resolve (code_gen);

                        return methodref.PeapiMethod;
                }

                public PEAPI.Field ResolveField (string name, BaseTypeRef ret_type, CodeGen code_gen)
                {
                        FieldDef fielddef = (FieldDef) field_table[new DictionaryEntry (name, ret_type.FullName)];
                        if (fielddef !=null)
                                return fielddef.Resolve (code_gen, classdef);

                        ExternTypeRef type_ref = code_gen.ThisModule.GetTypeRef (FullName, false);
                        IFieldRef fieldref = type_ref.GetFieldRef (ret_type, name);
                        fieldref.Resolve (code_gen);

                        return fieldref.PeapiField;
                }

                private string MakeFullName ()
                {
                        if (name_space == null || name_space == String.Empty)
                                return name;

                        return name_space + "." + name;
                }

                public int CompareTo (object obj)
                {
                        TypeDef type_def = (TypeDef) obj; 

                        return FullName.CompareTo (type_def.FullName);
                }
        }

}

