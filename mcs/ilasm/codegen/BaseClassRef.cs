//
// Mono.ILASM.BaseClassRef
//
// Author(s):
//  Ankit Jain  <jankit@novell.com>
//
// Copyright 2006 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;

namespace Mono.ILASM {

        public abstract class BaseClassRef : BaseTypeRef {

                protected Hashtable p_genericinst_table;
                protected bool is_valuetype;

                protected BaseClassRef (string full_name, bool is_valuetype)
                        : this (full_name, is_valuetype, null, null)
                {
                }

                protected BaseClassRef (string full_name, bool is_valuetype, ArrayList conv_list, string sig_mod)
                        : base (full_name, conv_list, sig_mod)
                {
                        this.is_valuetype = is_valuetype;
                        p_genericinst_table = null;
                }

                public PEAPI.Class PeapiClass {
                        get { return type as PEAPI.Class; }
                }

                public virtual void MakeValueClass ()
                {
                        is_valuetype = true;
                }

                public virtual GenericTypeInst GetGenericTypeInst (GenericArguments gen_args)
                {
                        return new GenericTypeInst (this, gen_args, is_valuetype);
                }

                public virtual PEAPI.Type ResolveInstance (CodeGen code_gen, GenericArguments gen_args)
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
        }


}
