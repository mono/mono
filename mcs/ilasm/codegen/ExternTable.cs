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

        public class ExternAssembly {
                        
                        public PEAPI.AssemblyRef AssemblyRef;
                        protected Hashtable class_table;
                        protected Hashtable typeref_table;
                        protected string name;
                        private int major, minor, build, revision;
                        private byte [] public_key;
                        private byte [] public_key_token;
                        private string locale;
                        private byte [] hash;
                        
                        public ExternAssembly (string name, AssemblyName asmb_name)
                        {
                                this.name = name;
                                typeref_table = new Hashtable ();
                                major = minor = build = revision = -1;
                        }

                        public void Resolve (CodeGen code_gen)
                        {
                                AssemblyRef = code_gen.PEFile.AddExternAssembly (name);
                                if (major != -1)
                                        AssemblyRef.AddVersionInfo (major, minor, build, revision);
                                if (public_key != null)
                                        AssemblyRef.AddKey (public_key);
                                if (public_key_token != null)
                                        AssemblyRef.AddKeyToken (public_key_token);
                                if (locale != null)
                                        AssemblyRef.AddCulture (locale);
                                if (hash != null)
                                        AssemblyRef.AddHash (hash);
                                class_table = new Hashtable ();
                        }

                        public void SetVersion (int major, int minor, int build, int revision)
                        {
                                this.major = major;
                                this.minor = minor;
                                this.build = build;
                                this.revision = revision;
                        }

                        public void SetPublicKey (byte [] public_key)
                        {
                                this.public_key = public_key;
                        }

                        public void SetPublicKeyToken (byte [] public_key_token)
                        {
                                this.public_key_token = public_key_token;
                        }

                        public void SetLocale (string locale)
                        {
                                this.locale = locale;
                        }

                        public void SetHash (byte [] hash)
                        {
                                this.hash = hash;
                        }

                        public ExternTypeRef GetTypeRef (string full_name, bool is_valuetype, ExternTable table)
                        {
                                ExternTypeRef type_ref = typeref_table [full_name] as ExternTypeRef;
                                
                                if (type_ref != null)
                                        return type_ref;

                                type_ref = new ExternTypeRef (name, full_name, is_valuetype, table);
                                typeref_table [full_name] = type_ref;

                                return type_ref;
                        }

                        public void ModifyTypeRefName (string old_name, string new_name)
                        {
                                object type_ref = typeref_table [old_name];
                                
                                if (type_ref == null)
                                        throw new Exception ("Modified type name not found. (" + old_name + ")");

                                typeref_table.Remove (old_name);
                                typeref_table [new_name] = type_ref;
                        }
                        
                        public PEAPI.ClassRef GetType (string name_space, string name)
                        {
                                string full_name = String.Format ("{0}.{1}",
                                        name_space, name);
                                PEAPI.ClassRef klass = class_table[full_name] as PEAPI.ClassRef;

                                if (klass != null)
                                        return klass;
                                
                                klass = (PEAPI.ClassRef) AssemblyRef.AddClass (name_space, name);
                                class_table[full_name] = klass;

                                return klass;
                        }

                        public PEAPI.ClassRef GetValueType (string name_space, string name)
                        {
                                string full_name = String.Format ("{0}.{1}",
                                        name_space, name);
                                PEAPI.ClassRef klass = class_table[full_name] as PEAPI.ClassRef;

                                if (klass != null) 
                                        return klass;

                                klass = (PEAPI.ClassRef) AssemblyRef.AddValueClass (name_space, name);
                                class_table[full_name] = klass;

                                return klass;
                        }
        }

        
        public class ExternTable {

                Hashtable assembly_table;

                public ExternTable ()
                {
                        // Add mscorlib
                        string mscorlib_name = "mscorlib";
                        AssemblyName mscorlib = new AssemblyName ();
                        mscorlib.Name = mscorlib_name;
                        AddAssembly (mscorlib_name, mscorlib);

                        // Also need to alias corlib, normally corlib and
                        // mscorlib are used interchangably
                        assembly_table["corlib"] = assembly_table["mscorlib"];
                }

                public ExternAssembly AddAssembly (string name, AssemblyName asmb_name)
                {
                        ExternAssembly ea = null;

                        if (assembly_table == null) {
                                assembly_table = new Hashtable ();
                        } else {
                                ea = assembly_table [name] as ExternAssembly;
                                if (ea != null)
                                        return ea;
                        }

                        ea = new ExternAssembly (name, asmb_name);

                        assembly_table [name] = ea;

                        return ea;
                }

                public void Resolve (CodeGen code_gen)
                {
                        foreach (ExternAssembly ext in assembly_table.Values)
                                ext.Resolve (code_gen);
                }

                public ExternTypeRef GetTypeRef (string asmb_name, string full_name, bool is_valuetype)
                {
                        ExternAssembly ext_asmb;
                        ext_asmb = assembly_table[asmb_name] as ExternAssembly;

                        if (ext_asmb == null)
                                throw new Exception (String.Format ("Assembly {0} not defined.", asmb_name));

                        return ext_asmb.GetTypeRef (full_name, is_valuetype, this);
                }

                public void ModifyTypeRefName (string asmb_name, string old_name, string new_name)
                {
                        ExternAssembly ext_asmb;
                        ext_asmb = assembly_table[asmb_name] as ExternAssembly;

                        if (ext_asmb == null)
                                throw new Exception (String.Format ("Assembly {0} not defined.", asmb_name));

                        ext_asmb.ModifyTypeRefName (old_name, new_name);
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

                 public PEAPI.ClassRef GetValueClass (string asmb_name, string full_name)
                {
                        ExternAssembly ext_asmb;
                        ext_asmb = assembly_table[asmb_name] as ExternAssembly;

                        if (ext_asmb == null)
                                throw new Exception (String.Format ("Assembly {0} not defined.", asmb_name));

                        string name_space, name;

                        GetNameAndNamespace (full_name, out name_space, out name);

                        return ext_asmb.GetValueType (name_space, name);
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

