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

                private string current_namespace;
                private TypeDef current_typedef;
                private MethodDef current_methoddef;
                private Stack typedef_stack;

                private TypeManager type_manager;
                private ExternTable extern_table;
                private ArrayList global_field_list;
                private ArrayList global_method_list;
                private ArrayList global_data_list;

                public CodeGen (string output_file, bool is_dll, bool is_assembly)
                {
                        pefile = new PEFile (output_file, is_dll, is_assembly);
                        type_manager = new TypeManager (this);
                        extern_table = new ExternTable (pefile);
                        typedef_stack = new Stack ();
                        global_field_list = new ArrayList ();
                        global_method_list = new ArrayList ();
                        global_data_list = new ArrayList ();
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

                public void BeginTypeDef (TypeAttr attr, string name, IClassRef parent,
                                ArrayList impl_list, Location location)
                {
                        TypeDef outer = null;

                        if (typedef_stack.Count > 0) {
                                outer = (TypeDef) typedef_stack.Peek ();
                                name = outer.Name + '/' + name;
                        }

                        TypeDef typedef = type_manager[name];

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

                        type_manager[typedef.FullName] = typedef;
                        current_typedef = typedef;
                        typedef_stack.Push (typedef);
                }

                public void AddFieldDef (FieldDef fielddef)
                {
                        if (current_typedef != null) {
                                current_typedef.AddFieldDef (fielddef);
                        } else {
                                global_field_list.Add (fielddef);
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
                                global_method_list.Add (methoddef);
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

                public void Write ()
                {
                        type_manager.DefineAll ();

                        foreach (FieldDef fielddef in global_field_list) {
                                fielddef.Define (this);
                        }

                        foreach (MethodDef methoddef in global_method_list) {
                                methoddef.Define (this);
                        }

                        pefile.WritePEFile ();
                }

        }

}

