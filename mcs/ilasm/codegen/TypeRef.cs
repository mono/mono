//
// Mono.ILASM.TypeRef
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
        /// Reference to a type in the module being compiled.
        /// </summary>
        public class TypeRef : ModifiableType, IClassRef {


                private Location location;
                private string full_name;
                private string sig_mod;
                private PEAPI.Type type;
                private bool is_valuetype;
                private Hashtable genericinst_table;
                private Hashtable p_genericinst_table;

                private bool is_resolved;

                public static readonly TypeRef Ellipsis = new TypeRef ("ELLIPSIS", false, null);
                public static readonly TypeRef Any = new TypeRef ("any", false, null);

                public TypeRef (string full_name, bool is_valuetype, Location location)
                        : this (full_name, is_valuetype, location, null, null)
                {
                }

                public TypeRef (string full_name, bool is_valuetype, Location location, ArrayList conv_list, string sig_mod)
                {
                        this.full_name = full_name;
                        this.location = location;
                        this.is_valuetype = is_valuetype;
                        this.sig_mod = sig_mod;
                        if (conv_list != null)
                                ConversionList = conv_list;
                        is_resolved = false;
                }
                
                public string FullName {
                        get { return full_name + sig_mod; }
                }

                public override string SigMod {
                        get { return sig_mod; }
                        set { sig_mod = value; }
                }

                public PEAPI.Type PeapiType {
                        get { return type; }
                }

                public PEAPI.Class PeapiClass {
                        get { return type as PEAPI.Class; }
                }

                public IClassRef Clone ()
                {
                        return new TypeRef (full_name, is_valuetype, location, (ArrayList) ConversionList.Clone (), sig_mod);
                }

                public bool IsResolved {
                        get { return is_resolved; }
                }

                public void MakeValueClass ()
                {
                        is_valuetype = true;
                }

                public GenericTypeInst GetGenericTypeInst (GenericArguments gen_args)
                {
                        string sig = gen_args.ToString ();
                        GenericTypeInst gtri = null;

                        if (genericinst_table == null)
                                genericinst_table = new Hashtable ();
                        else
                                gtri = genericinst_table [sig] as GenericTypeInst;

                        if (gtri == null) {
                                gtri = new GenericTypeInst (this, gen_args, is_valuetype);
                                genericinst_table [sig] = gtri;
                        }

                        return gtri;
                }

                public PEAPI.Type ResolveInstance (CodeGen code_gen, GenericArguments gen_args)
                {
                        PEAPI.GenericTypeInst gtri = null;
                        string sig = gen_args.ToString ();

                        if (p_genericinst_table == null)
                                p_genericinst_table = new Hashtable ();
                        else
                                gtri = p_genericinst_table [sig] as PEAPI.GenericTypeInst;

                        if (gtri == null) {
                                if (!IsResolved)
                                        Resolve (code_gen);

				gtri = new PEAPI.GenericTypeInst (PeapiType, gen_args.Resolve (code_gen));
                                p_genericinst_table [sig] = gtri;
                        }
                        
                        return gtri;
                }

                public  IMethodRef GetMethodRef (ITypeRef ret_type,
                        PEAPI.CallConv call_conv, string name, ITypeRef[] param, int gen_param_count)
                {
                        return new MethodRef (this, call_conv, ret_type, name, param, gen_param_count);
                }

                public IFieldRef GetFieldRef (ITypeRef ret_type, string name)
                {
                        return new FieldRef (this, ret_type, name);
                }

                public void Resolve (CodeGen code_gen)
                {
                        if (is_resolved)
                                return;

                        PEAPI.Type base_type;

                        base_type = code_gen.TypeManager.GetPeapiType (full_name);

                        if (base_type == null) {
                                code_gen.Report.Error ("Reference to undefined class '" +
                                                       FullName + "'");
                                return;
                        }
                        type = Modify (code_gen, base_type);

                        is_resolved = true;
                }

                public IClassRef AsClassRef (CodeGen code_gen)
                {
                        return this;
                }

                public override string ToString ()
                {
                        return FullName;
                }

        }

}

