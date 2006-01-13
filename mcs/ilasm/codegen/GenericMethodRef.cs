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

        public class GenericMethodRef : BaseMethodRef {

                private BaseMethodRef meth;
                private GenericMethodSig sig;

                public GenericMethodRef (BaseMethodRef meth, GenericMethodSig sig)
                        : base (null, meth.CallConv, null, "", null, 0)
                {
                        this.meth = meth;
                        this.sig = sig;
                        is_resolved = false;
                }

                public override PEAPI.CallConv CallConv {
                        get { return meth.CallConv; }
                        set { meth.CallConv = value; }
                }

                public override void Resolve (CodeGen code_gen)
                {
                        if (is_resolved)
                                return;

                        meth.Resolve (code_gen);
                        peapi_method = code_gen.PEFile.AddMethodSpec (meth.PeapiMethod, sig.Resolve (code_gen));

                        is_resolved = true;
                }
        }

}

