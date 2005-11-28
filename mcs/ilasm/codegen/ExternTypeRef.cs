//
// Mono.ILASM.ExternTypeRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;
using System.Collections;

namespace Mono.ILASM {

        /// <summary>
        /// A reference to a type in another assembly
        /// </summary>
        public class ExternTypeRef : ModifiableType, IClassRef, IScope {

                private PEAPI.Type type;
                private IScope extern_ref;
                private string full_name;
                private string sig_mod;
                private bool is_valuetype;

                private bool is_resolved;

                private Hashtable nestedtypes_table;
                private Hashtable nestedclass_table;
                private Hashtable method_table;
                private Hashtable field_table;
                
                public ExternTypeRef (IScope extern_ref, string full_name,
                                bool is_valuetype)
                {
                        this.extern_ref = extern_ref;
                        this.full_name = full_name;
                        this.is_valuetype = is_valuetype;
                        sig_mod = String.Empty;

                        nestedclass_table = new Hashtable ();
                        nestedtypes_table = new Hashtable ();
                        method_table = new Hashtable ();
                        field_table = new Hashtable ();
                        
                        is_resolved = false;
                }

                private ExternTypeRef (IScope extern_ref, string full_name,
                                bool is_valuetype, ArrayList conv_list) : this (
					extern_ref, full_name, is_valuetype)
                {
                        ConversionList = conv_list;
                }
                
                public ExternTypeRef Clone ()
                {
                        return new ExternTypeRef (extern_ref, full_name, is_valuetype,
                                        (ArrayList) ConversionList.Clone ());
                }
                
                public PEAPI.Type PeapiType {
                        get { return type; }
                }

                public PEAPI.Class PeapiClass {
                        get { return type as PEAPI.Class; }
                }

                public string FullName {
                        get { return full_name + sig_mod; }
                }


                public override string SigMod {
                        get { return sig_mod; }
                        set { sig_mod = value; }
                }

                public IScope ExternRef {
                        get { return extern_ref; }
                }

                public void Resolve (CodeGen code_gen)
                {
                        if (is_resolved)
                                return;

                        ExternTypeRef etr = extern_ref as ExternTypeRef;        
                        if (etr != null)        
                                //This is a nested class, so resolve parent
                                etr.Resolve (code_gen);

                        type = extern_ref.GetType (full_name, is_valuetype);
                        type = Modify (code_gen, type);

                        is_resolved = true;
                }

                public void MakeValueClass ()
                {
                        is_valuetype = true;
                }

                public IMethodRef GetMethodRef (ITypeRef ret_type, PEAPI.CallConv call_conv,
                                string name, ITypeRef[] param)
                {
                        string sig = MethodDef.CreateSignature (ret_type, name, param);
                        ExternMethodRef mr = method_table [sig] as ExternMethodRef;
                        
                        if (mr == null) {
                                mr = new ExternMethodRef (this, ret_type, call_conv, name, param);
                                method_table [sig] = mr;
                        }

                        return mr;
                }

                public IFieldRef GetFieldRef (ITypeRef ret_type, string name)
                {
                        ExternFieldRef fr = field_table [name] as ExternFieldRef;

                        if (fr == null) {
                                fr = new ExternFieldRef (this, ret_type, name);
                                field_table [name] = fr;
                        }

                        return fr;
                }

                public ExternTypeRef GetTypeRef (string _name, bool is_valuetype)
                {
                        string first= _name;
                        string rest = "";
                        int slash = _name.IndexOf ('/');

                        if (slash > 0) {
                                first = _name.Substring (0, slash);
                                rest = _name.Substring (slash + 1);
                        }

                        ExternTypeRef ext_typeref = nestedtypes_table [first] as ExternTypeRef;
                        
                        if (ext_typeref != null) {
                                if (is_valuetype && rest == "")
                                        ext_typeref.MakeValueClass ();
                        } else {
                                ext_typeref = new ExternTypeRef (this, first, is_valuetype);
                                nestedtypes_table [first] = ext_typeref;
                        }        
                        
                        return (rest == "" ? ext_typeref : ext_typeref.GetTypeRef (rest, is_valuetype));
                }

                public PEAPI.IExternRef GetExternTypeRef ()
                {
                        //called by GetType for a nested type
                        //should this cant be 'modified' type, so it should
                        //be ClassRef 
                        return (PEAPI.ClassRef) type;
                }

                public PEAPI.ClassRef GetType (string _name, bool is_valuetype)
                {
                        PEAPI.ClassRef klass = nestedclass_table [_name] as PEAPI.ClassRef;
                        
                        if (klass != null)
                                return klass;

                        string name_space, name;
                        ExternTable.GetNameAndNamespace (_name, out name_space, out name);

                        if (is_valuetype)
                                klass = (PEAPI.ClassRef) GetExternTypeRef ().AddValueClass (name_space, name);
                        else        
                                klass = (PEAPI.ClassRef) GetExternTypeRef ().AddClass (name_space, name);

                        nestedclass_table [_name] = klass;

                        return klass;
                }        

                public System.Type GetReflectedType ()
                {
                        ExternRef er = extern_ref as ExternRef;
                        if (er != null) {
                                ExternAssembly ea = er as ExternAssembly;
                                if (ea != null) {
                                        System.Reflection.Assembly asm = System.Reflection.Assembly.Load (er.Name);

                                        return asm.GetType (FullName);
                                }/* else ExternModule */

                        } /*else - nested type? */
                        return null;
                }
        }

}

