//
// Mono.ILASM.GenericMethodSig
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System;

namespace Mono.ILASM {

        public class GenericMethodSig {

                private ITypeRef[] type_list;
                private bool is_resolved;
                private PEAPI.GenericMethodSig sig;

                public GenericMethodSig (ITypeRef[] type_list)
                {
                        this.type_list = type_list;
                        is_resolved = false;
                }

                public PEAPI.GenericMethodSig Sig {
                        get { return sig; }
                }

                public PEAPI.GenericMethodSig Resolve (CodeGen code_gen)
                {
                        if (is_resolved)
                                return sig;

                        PEAPI.Type[] p_type_list = new PEAPI.Type[type_list.Length];
                        for (int i=0; i<p_type_list.Length; i++) {
                                type_list[i].Resolve (code_gen);
                                p_type_list[i] = type_list[i].PeapiType;
                        }

                        sig = new PEAPI.GenericMethodSig (p_type_list);
                        is_resolved = true;

                        return sig;
                }

        }

}


