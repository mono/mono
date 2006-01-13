//
// Mono.ILASM.BaseMethodRef
//
// Author(s):
//  Ankit Jain  <JAnkit@novell.com>
//
// Copyright 2006 Novell, Inc (http://www.novell.com)
//


using System;
using System.Collections;

namespace Mono.ILASM {
       public abstract class BaseMethodRef {

                protected BaseTypeRef owner;
                protected PEAPI.CallConv call_conv;
                protected BaseTypeRef ret_type;
                protected string name;
                protected BaseTypeRef[] param;

                protected PEAPI.Method peapi_method;
                protected bool is_resolved;
                protected int gen_param_count;

                protected Hashtable gen_method_table;

                public BaseMethodRef (BaseTypeRef owner, PEAPI.CallConv call_conv,
                        BaseTypeRef ret_type, string name, BaseTypeRef[] param, int gen_param_count)
                {
                        this.owner = owner;
                        this.call_conv = call_conv;
                        this.ret_type = ret_type;
                        this.name = name;
                        this.param = param;
                        this.gen_param_count = gen_param_count;
                        if (gen_param_count > 0)
                                CallConv |= PEAPI.CallConv.Generic;
                        is_resolved = false;
                }

                public virtual PEAPI.Method PeapiMethod {
                        get { return peapi_method; }
                }

                public virtual PEAPI.CallConv CallConv {
                        get { return call_conv; }
                        set { call_conv = value; }
                }

                public virtual BaseTypeRef Owner {
                        get { return owner; }
                }

                public abstract void Resolve (CodeGen code_gen);

                public GenericMethodRef GetGenericMethodRef (GenericArguments gen_args)
                {
                        GenericMethodRef methref = null;

                        if (gen_method_table == null)
                                gen_method_table = new Hashtable ();
                        else
                                methref = (GenericMethodRef) gen_method_table [gen_args.ToString ()];

                        if (methref == null) {
                                methref = new GenericMethodRef (this, GenericMethodSig.GetInstance (gen_args));
                                gen_method_table [gen_args.ToString ()] = methref;
                        }

                        return methref;
                }

       }
}



