//
// Mono.ILASM.ClassTable.cs
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//

using PEAPI;
using System;
using System.Collections;

namespace Mono.ILASM {

        public class ClassTable {

                private class ClassTableItem {

                        private static readonly int DefinedFlag = 2;

                        private int flags;

                        public ArrayList LocationList;
                        public ClassDef Class;
                        public MethodTable method_table;
                        public FieldTable field_table;

                        public ClassTableItem (ClassDef klass, Location location)
                        {
                                flags = 0;
                                Class = klass;
                                LocationList = new ArrayList ();
                                LocationList.Add (location);
                                method_table = new MethodTable (klass);
                                field_table = new FieldTable (klass);
                        }

                        public bool Defined {
                                get { return ((flags & DefinedFlag) != 0); }
                                set {
                                        if (value)
                                                flags |= DefinedFlag;
                                        else
                                                flags ^= DefinedFlag;
                                }
                        }

                        public bool CheckDefined ()
                        {
                                if (!Defined)
                                        return false;

                                if (!FieldTable.CheckDefined ())
                                        return false;

                                if (!MethodTable.CheckDefined ())
                                        return false;

                                return true;
                        }

                        public MethodTable MethodTable {
                                get { return method_table; }
                        }

                        public FieldTable FieldTable {
                                get { return field_table; }
                        }

                }

                protected readonly TypeAttr DefaultAttr;
                protected Hashtable table;
                protected PEFile pefile;

                public ClassTable (PEFile pefile)
                {
                        DefaultAttr = TypeAttr.Public;
                        this.pefile = pefile;
                        table = new Hashtable ();
                }

                public Class Get (string full_name)
                {
                        ClassTableItem item = table[full_name] as ClassTableItem;

                        if (item == null)
                                return null;

                        return item.Class;
                }

                public Class GetReference (string full_name, Location location)
                {
                        ClassTableItem item = table[full_name] as ClassTableItem;

                        if (item != null) {
                                item.LocationList.Add (location);
                                return item.Class;
                        }

                        string name_space, name;
                        GetNameAndNamespace (full_name, out name_space, out name);
                        ClassDef klass = pefile.AddClass (DefaultAttr, name_space, name);
                        AddReference (full_name, klass, location);

                        return klass;
                }

                public MethodTable GetMethodTable (string full_name, Location location)
                {
                        ClassTableItem item = table[full_name] as ClassTableItem;

                        if (item == null) {
                                GetReference (full_name, location);
                                return GetMethodTable (full_name, location);
                        }

                        return item.MethodTable;
                }

                public FieldTable GetFieldTable (string full_name, Location location)
                {
                        ClassTableItem item = table[full_name] as ClassTableItem;

                        if (item == null) {
                                GetReference (full_name, location);
                                return GetFieldTable (full_name, location);
                        }

                        return item.FieldTable;
                }

                public ClassDef AddDefinition (string name_space, string name,
                        TypeAttr attr, Location location)
                {
                        string full_name;

                        if (name_space != null)
                                full_name = String.Format ("{0}.{1}", name_space, name);
                        else
                                full_name = name;

                        ClassTableItem item = (ClassTableItem) table[full_name];

                        if (item == null) {
                                ClassDef klass = pefile.AddClass (attr, name_space, name);
                                AddDefined (full_name, klass, location);
                                return klass;
                        }

                        item.Class.AddAttribute (attr);
                        item.Defined = true;

                        return item.Class;
                }

                public ClassDef AddDefinition (string name_space, string name,
                        TypeAttr attr, Class parent, Location location)
                {
                        string full_name;

                        if (name_space != null)
                                full_name = String.Format ("{0}.{1}", name_space, name);
                        else
                                full_name = name;

                        ClassTableItem item = (ClassTableItem) table[full_name];

                        if (item == null) {
                                ClassDef klass = pefile.AddClass (attr, name_space, name, parent);
                                AddDefined (full_name, klass, location);
                                return klass;
                        }

                        /// TODO: Need to set parent, will need to modify PEAPI for this.
                        item.Class.AddAttribute (attr);
                        item.Defined = true;

                        return item.Class;
                }

                /// <summary>
                ///  When there is no code left to compile, check to make sure referenced types where defined
                ///  TODO: Proper error reporting
                /// </summary>
                public void CheckForUndefined ()
                {
                        foreach (DictionaryEntry dic_entry in table) {
                                ClassTableItem table_item = (ClassTableItem) dic_entry.Value;
                                if (table_item.CheckDefined ())
                                        continue;
                                Report.Error (String.Format ("Type: {0} is not defined.", dic_entry.Key));
                        }
                }

                /// <summary>
                ///  If a type is allready defined throw an Error
                /// </summary>
                protected void CheckExists (string full_name)
                {
                        ClassTableItem item = table[full_name] as ClassTableItem;

                        if ((item != null) && (item.Defined)) {
                                Report.Error (String.Format ("Class: {0} defined in multiple locations.",
                                        full_name));
                        }
                }

                protected void AddDefined (string full_name, ClassDef klass, Location location)
                {
                        if (table.Contains (full_name))
                                return;

                        ClassTableItem item = new ClassTableItem (klass, location);
                        item.Defined = true;

                        table[full_name] = item;
                }

                protected void AddReference (string full_name, ClassDef klass, Location location)
                {
                        if (table.Contains (full_name))
                                return;

                        ClassTableItem item = new ClassTableItem (klass, location);

                        table[full_name] = item;
                }

                public static void GetNameAndNamespace (string full_name,
                        out string name_space, out string name) {

                        int last_dot = full_name.LastIndexOf ('.');

                        if (last_dot < 0) {
                                name_space = String.Empty;
                                name = full_name;
                                return;
                        }

                        name_space = full_name.Substring (0, last_dot);
                        name = full_name.Substring (last_dot + 1);
                }

        }

}

