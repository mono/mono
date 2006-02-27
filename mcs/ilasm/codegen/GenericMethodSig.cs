//
// Mono.ILASM.GenericMethodSig
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Collections;

namespace Mono.ILASM {

        public class GenericMethodSig {

                private GenericArguments gen_args;
                private bool is_resolved;
                private PEAPI.GenericMethodSig sig;

                private static Hashtable sig_table;

                public GenericMethodSig (GenericArguments gen_args)
                {
                        this.gen_args = gen_args;
                        is_resolved = false;
                }

                public PEAPI.GenericMethodSig Sig {
                        get { return sig; }
                }

                public PEAPI.GenericMethodSig Resolve (CodeGen code_gen)
                {
                        if (is_resolved)
                                return sig;

                        sig = new PEAPI.GenericMethodSig (gen_args.Resolve (code_gen));
                        is_resolved = true;

                        return sig;
                }

                public static GenericMethodSig GetInstance (GenericArguments gen_args)
                {
                        GenericMethodSig sig = null;

                        if (sig_table == null)
                                sig_table = new Hashtable ();
                        else
                                sig = (GenericMethodSig) sig_table [gen_args.ToString ()];

                        if (sig == null) {
                                sig = new GenericMethodSig (gen_args);
                                sig_table [gen_args.ToString ()] = sig;
                        }
                        
                        return sig;
                }
        }

}


