//
// Mono.ILASM.BaseTypeRef
//
// Author(s):
//  Ankit Jain  <jankit@novell.com>
//
// Copyright 2006 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;

namespace Mono.ILASM {

        public abstract class BaseTypeRef : ModifiableType {
                protected string full_name;
                protected string sig_mod;
                protected PEAPI.Type type;
                protected bool is_resolved;
                protected Hashtable method_table;
                protected Hashtable field_table;

                protected BaseTypeRef (string full_name)
                        : this (full_name, null, null)
                {
                }

                protected BaseTypeRef (string full_name, ArrayList conv_list, string sig_mod)
                {
                        this.full_name = full_name;
                        this.sig_mod = sig_mod;
                        is_resolved = false;
                        if (conv_list != null)
                                ConversionList = conv_list;
                }

                public virtual string FullName {
                        get { return full_name + sig_mod; }
                }

                public override string SigMod {
                        get { return sig_mod; }
                        set { sig_mod = value; }
                }

                public PEAPI.Type PeapiType {
                        get { return type; }
                }

                public bool IsResolved {
                        get { return is_resolved; }
                }

                public abstract void Resolve (CodeGen code_gen);

                public abstract BaseTypeRef Clone ();

                protected abstract BaseMethodRef CreateMethodRef (BaseTypeRef ret_type,
                        PEAPI.CallConv call_conv, string name, BaseTypeRef[] param, int gen_param_count);

                public virtual BaseMethodRef GetMethodRef (BaseTypeRef ret_type,
                        PEAPI.CallConv call_conv, string name, BaseTypeRef[] param, int gen_param_count)
                {
                        BaseMethodRef mr = null;

                        /* Note: FullName not reqd as this is cached per object */
                        string key = MethodDef.CreateSignature (ret_type, call_conv, name, param, gen_param_count, true);
                        if (method_table == null)
                                method_table = new Hashtable ();
                        else
                                mr = (BaseMethodRef) method_table [key];

                        if (mr == null) {
                                mr = CreateMethodRef (ret_type, call_conv, name, param, gen_param_count);
                                method_table [key] = mr;
                        }

                        return mr;
                }

                protected abstract IFieldRef CreateFieldRef (BaseTypeRef ret_type, string name);

                public virtual IFieldRef GetFieldRef (BaseTypeRef ret_type, string name)
                {
                        IFieldRef fr = null;
                        string key = ret_type.FullName + name;

                        if (field_table == null)
                                field_table = new Hashtable ();
                        else
                                fr = (IFieldRef) field_table [key];

                        if (fr == null) {
                                fr = CreateFieldRef (ret_type, name);
                                field_table [key] = fr;
                        }

                        return fr;
                }

        }

}
