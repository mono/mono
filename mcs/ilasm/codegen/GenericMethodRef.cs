//
// Mono.ILASM.GenericMethodRef
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//


using System;

namespace Mono.ILASM {

        public class GenericMethodRef : IMethodRef {

                private IMethodRef meth;
                private GenericMethodSig sig;
                private bool is_resolved;

                public GenericMethodRef (IMethodRef meth, GenericMethodSig sig)
                {
                        this.meth = meth;
                        this.sig = sig;
                        is_resolved = false;
                }

                public PEAPI.Method PeapiMethod {
                        get { return meth.PeapiMethod; }
                }

                public PEAPI.CallConv CallConv {
                        get { return meth.CallConv; }
                        set { meth.CallConv = value; }
                }

                public void Resolve (CodeGen code_gen)
                {
                        if (is_resolved)
                                return;

                        meth.Resolve (code_gen);
                        code_gen.PEFile.AddMethodSpec (meth.PeapiMethod, sig.Resolve (code_gen));

                        is_resolved = true;
                }
        }

}

