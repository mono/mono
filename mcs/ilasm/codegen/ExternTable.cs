//
// Mono.ILASM.ExternTable.cs
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//

using System;
using System.Collections;
using System.Reflection;

namespace Mono.ILASM {

        public class ExternTable {

                protected class ExternAssembly {

                        public PEAPI.AssemblyRef AssemblyRef;

                        protected PEAPI.PEFile pefile;
                        protected Hashtable type_table;

                        public ExternAssembly (PEAPI.PEFile pefile, string name,
                                AssemblyName asmb_name)
                        {
                                type_table = new Hashtable ();
                                this.pefile = pefile;
                                AssemblyRef = pefile.AddExternAssembly (name);
                        }

                        public PEAPI.ClassRef GetType (string name_space, string name)
                        {
                                string full_name = String.Format ("{0}.{1}",
                                        name_space, name);
                                PEAPI.ClassRef klass = type_table[full_name] as PEAPI.ClassRef;

                                if (klass != null)
                                        return klass;

                                klass = (PEAPI.ClassRef) AssemblyRef.AddClass (name_space, name);
                                type_table[full_name] = klass;

                                return klass;
                        }
                }

                PEAPI.PEFile pefile;
                Hashtable assembly_table;

                public ExternTable (PEAPI.PEFile pefile)
                {
                        this.pefile = pefile;

                        // Add mscorlib
                        string mscorlib_name = "mscorlib";
                        AssemblyName mscorlib = new AssemblyName ();
                        mscorlib.Name = mscorlib_name;
                        AddAssembly (mscorlib_name, mscorlib);
                }

                public void AddAssembly (string name, AssemblyName asmb_name)
                {
                        if (assembly_table == null) {
                                assembly_table = new Hashtable ();
                        } else if (assembly_table.Contains (name)) {
                                // Maybe this is an error??
                                return;
                        }

                        assembly_table[name] = new ExternAssembly (pefile, name, asmb_name);
                }

                public PEAPI.ClassRef GetClass (string asmb_name, string name_space, string name)
                {
                        ExternAssembly ext_asmb;
                        ext_asmb = assembly_table[asmb_name] as ExternAssembly;

                        if (ext_asmb == null)
                                throw new Exception (String.Format ("Assembly {0} not defined.", asmb_name));

                        return ext_asmb.GetType (name_space, name);
                }

                public PEAPI.ClassRef GetClass (string asmb_name, string full_name)
                {
                        ExternAssembly ext_asmb;
                        ext_asmb = assembly_table[asmb_name] as ExternAssembly;

                        if (ext_asmb == null)
                                throw new Exception (String.Format ("Assembly {0} not defined.", asmb_name));

                        string name_space, name;

                        GetNameAndNamespace (full_name, out name_space, out name);

                        return ext_asmb.GetType (name_space, name);
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

