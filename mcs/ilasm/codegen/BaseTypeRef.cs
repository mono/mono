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

                public virtual void Resolve (CodeGen code_gen)
                {
                        throw new Exception ("Should not be called");
                }

                public  virtual IMethodRef GetMethodRef (BaseTypeRef ret_type,
                        PEAPI.CallConv call_conv, string name, BaseTypeRef[] param, int gen_param_count)
                {
                        throw new Exception ("Should not be called");
                }

                public virtual IFieldRef GetFieldRef (BaseTypeRef ret_type, string name)
                {
                        throw new Exception ("Should not be called");
                }

        }

}
