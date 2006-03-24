//
// Mono.ILASM.Module
//
// Author(s):
//  Ankit Jain  <jankit@novell.com>
//
// Copyright 2006 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;

namespace Mono.ILASM {

        public class Module : ExternRef {

                PEAPI.Module module;

                public Module (string name)
                        : base (name)
                {
                }

                public PEAPI.Module PeapiModule {
                        get { return module; }
                        set { module = value; }
                }

                public override string FullName {
                        get { 
                                //'name' field should not contain the [ ]
                                //as its used for resolving
                                return String.Format ("[{0}]", name); 
                        }
                }

                public override PEAPI.IExternRef GetExternRef ()
                {
                        return module;
                }

                public override void Resolve (CodeGen code_gen)
                {
                        throw new InternalErrorException ("This should not get called");
                }

                public void Resolve (CodeGen code_gen, PEAPI.Module module)
                {
                        this.module = module;

                        if (customattr_list == null)
                                return;

                        foreach (CustomAttr customattr in customattr_list)
                                customattr.AddTo (code_gen, module);
                }
        }
}
