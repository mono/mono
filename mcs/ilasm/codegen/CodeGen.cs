//
// Mono.ILASM.CodeGen.cs
//
// Author(s):
//  Sergey Chaban (serge@wildwestsoftware.com)
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) Sergey Chaban
// (C) 2003 Jackson Harper, All rights reserved
//

using PEAPI;
using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.ILASM {

        public class CodeGen {

                private PEFile pefile;
                private string assembly_name;
                private string current_namespace;
                private TypeDef current_typedef;
                private MethodDef current_methoddef;
                private Stack typedef_stack;

                private TypeManager type_manager;
                private ExternTable extern_table;
                private Hashtable global_field_table;
                private Hashtable global_method_table;
                private ArrayList global_data_list;

                private ArrayList defcont_list;

                private int sub_system;
                private int cor_flags;
                private long image_base;

                public CodeGen (string output_file, bool is_dll, bool is_assembly)
                {
                        pefile = new PEFile (output_file, is_dll, is_assembly);
                        type_manager = new TypeManager (this);
                        extern_table = new ExternTable (pefile);
                        typedef_stack = new Stack ();
                        global_field_table = new Hashtable ();
                        global_method_table = new Hashtable ();
                        global_data_list = new ArrayList ();

                        defcont_list = new ArrayList ();

                        sub_system = -1;
                        cor_flags = -1;
                        image_base = -1;
                }

                public PEFile PEFile {
                        get { return pefile; }
                }

                public string CurrentNameSpace {
                        get { return current_namespace; }
                        set { current_namespace = value; }
                }

                public TypeDef CurrentTypeDef {
                        get { return current_typedef; }
                }

                public MethodDef CurrentMethodDef {
                        get { return current_methoddef; }
                }

                public ExternTable ExternTable {
                        get { return extern_table; }
                }

                public TypeManager TypeManager {
                        get { return type_manager; }
                }

                public void SetSubSystem (int sub_system)
                {
                        this.sub_system = sub_system;
                }

                public void SetCorFlags (int cor_flags)
                {
                        this.cor_flags = cor_flags;
                }

                public void SetImageBase (long image_base)
                {
                        this.image_base = image_base;
                }

                public void SetAssemblyName (string name)
                {
                        assembly_name = name;
                }

                public bool IsThisAssembly (string name)
                {
                        return (name == assembly_name);
                }

                public void BeginTypeDef (TypeAttr attr, string name, IClassRef parent,
                                ArrayList impl_list, Location location)
                {
                        TypeDef outer = null;
                        string cache_name = CacheName (name);

                        if (typedef_stack.Count > 0) {
                                outer = (TypeDef) typedef_stack.Peek ();
                                cache_name = CacheName (outer.Name + '/' + name);
                        }

                        TypeDef typedef = type_manager[cache_name];

                        if (typedef != null) {
                                // Class head is allready defined, we are just reopening the class
                                current_typedef = typedef;
                                typedef_stack.Push (current_typedef);
                                return;
                        }

                        typedef = new TypeDef (attr, current_namespace,
                                        name, parent, impl_list, location);

                        if (outer != null)
                                typedef.OuterType = outer;

                        type_manager[cache_name] = typedef;
                        current_typedef = typedef;
                        typedef_stack.Push (typedef);
                }

                public void AddFieldDef (FieldDef fielddef)
                {
                        if (current_typedef != null) {
                                current_typedef.AddFieldDef (fielddef);
                        } else {
                                global_field_table.Add (fielddef.Name,
                                                fielddef);
                        }
                }

                public void AddDataDef (DataDef datadef)
                {
                        if (current_typedef != null) {
                                current_typedef.AddDataDef (datadef);
                        } else {
                                global_data_list.Add (datadef);
                        }
                }

                public void BeginMethodDef (MethodDef methoddef)
                {
                        if (current_typedef != null) {
                                current_typedef.AddMethodDef (methoddef);
                        } else {
                                global_method_table.Add (methoddef.Signature,
                                                methoddef);
                        }

                        current_methoddef = methoddef;
                }

                public void EndMethodDef ()
                {
                        current_methoddef = null;
                }

                public void EndTypeDef ()
                {
                        typedef_stack.Pop ();
                        current_typedef = null;
                }

                public void AddToDefineContentsList (TypeDef typedef)
                {
                        defcont_list.Add (typedef);
                }

                public void Write ()
                {
                        type_manager.DefineAll ();

                        foreach (FieldDef fielddef in global_field_table.Values) {
                                fielddef.Define (this);
                        }

                        foreach (MethodDef methoddef in global_method_table.Values) {
                                methoddef.Define (this);
                        }

                        foreach (TypeDef typedef in defcont_list) {
                                typedef.DefineContents (this);
                        }

                        if (sub_system != -1)
                                pefile.SetSubSystem ((PEAPI.SubSystem) sub_system);
                        if (cor_flags != -1)
                                pefile.SetCorFlags (cor_flags);

                        pefile.WritePEFile ();
                }

                public PEAPI.Method ResolveMethod (string signature)
                {
                        MethodDef methoddef = (MethodDef) global_method_table[signature];

                        return methoddef.Resolve (this);
                }

                public PEAPI.Method ResolveVarargMethod (string signature,
                                CodeGen code_gen, PEAPI.Type[] opt)
                {
                        MethodDef methoddef = (MethodDef) global_method_table[signature];
                        methoddef.Resolve (code_gen);

                        return methoddef.GetVarargSig (opt);
                }

                public PEAPI.Field ResolveField (string name)
                {
                        FieldDef fielddef = (FieldDef) global_field_table[name];

                        return fielddef.Resolve (this);
                }

                private string CacheName (string name)
                {
                        if (current_namespace == null ||
                                        current_namespace == String.Empty)
                                return name;

                        return current_namespace + "." + name;
                }
        }

}

