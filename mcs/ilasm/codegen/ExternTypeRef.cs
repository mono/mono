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
                private Hashtable genericinst_table;
                private Hashtable p_genericinst_table;
                
                public ExternTypeRef (IScope extern_ref, string full_name, bool is_valuetype) 
                        : this (extern_ref, full_name, is_valuetype, null, null)
                {
                }

                private ExternTypeRef (IScope extern_ref, string full_name,
                                bool is_valuetype, ArrayList conv_list, string sig_mod)
                {
                        this.extern_ref = extern_ref;
                        this.full_name = full_name;
                        this.is_valuetype = is_valuetype;
                        this.sig_mod = sig_mod;

                        nestedclass_table = new Hashtable ();
                        nestedtypes_table = new Hashtable ();
                        method_table = new Hashtable ();
                        field_table = new Hashtable ();
                        
                        is_resolved = false;
                        if (conv_list != null)
                                ConversionList = conv_list;
                }
                
                public IClassRef Clone ()
                {
                        return new ExternTypeRef (extern_ref, full_name, is_valuetype, 
                                        (ArrayList) ConversionList.Clone (), sig_mod);
                }
                
                public GenericTypeInst GetGenericTypeInst (GenericArguments gen_args)
                {
                        string sig = gen_args.ToString ();
                        GenericTypeInst gti = null;

                        if (genericinst_table == null)
                                genericinst_table = new Hashtable ();
                        else
                                gti = genericinst_table [sig] as GenericTypeInst;

                        if (gti == null) {
                                gti = new GenericTypeInst (this, gen_args, is_valuetype);
                                genericinst_table [sig] = gti;
                        }

                        return gti;
                }

                public PEAPI.Type ResolveInstance (CodeGen code_gen, GenericArguments gen_args)
                {
                        string sig = gen_args.ToString ();
                        PEAPI.GenericTypeInst gti = null;

                        if (p_genericinst_table == null)
                                p_genericinst_table = new Hashtable ();
                        else
                                gti = p_genericinst_table [sig] as PEAPI.GenericTypeInst;

                        if (gti == null) {
                                if (!is_resolved)
                                        throw new Exception ("Can't ResolveInstance on unresolved ExternTypeRef");

				gti = new PEAPI.GenericTypeInst (PeapiType, gen_args.Resolve (code_gen));
                                p_genericinst_table [sig] = gti;
                        }

                        return gti;
                }

                public PEAPI.Type PeapiType {
                        get { return type; }
                }

                public PEAPI.Class PeapiClass {
                        get { return type as PEAPI.Class; }
                }

                public string FullName {
                        get { 
                                if (extern_ref == null)
                                        return full_name + sig_mod;
                                else
                                        return extern_ref.FullName + (extern_ref is ExternTypeRef ? "/" : "") + full_name + sig_mod;
                        }
                }

                public string Name {
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
                                string name, ITypeRef[] param, int gen_param_count)
                {
                        string sig = MethodDef.CreateSignature (ret_type, name, param, gen_param_count);
                        ExternMethodRef mr = method_table [sig] as ExternMethodRef;
                        
                        if (mr == null) {
                                mr = new ExternMethodRef (this, ret_type, call_conv, name, param, gen_param_count);
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
                                        System.Reflection.Assembly asm = System.Reflection.Assembly.Load (ea.Name);

                                        //Type name required here, so don't use FullName
                                        return asm.GetType (Name);
                                }/* else ExternModule */

                        } /*else - nested type? */
                        return null;
                }
        }

}

