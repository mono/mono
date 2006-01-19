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

        public class Module : ICustomAttrTarget {
                string name;
                ArrayList customattr_list;

                public Module (string name)
                {
                        this.name = name;
                        customattr_list = null;
                }

                public string Name {
                        get { return name; }
                }

                public void AddCustomAttribute (CustomAttr customattr)
                {
                        if (customattr_list == null)
                                customattr_list = new ArrayList ();

                        customattr_list.Add (customattr);
                }

                public void Resolve (CodeGen code_gen, PEAPI.Module module)
                {
                        if (customattr_list == null)
                                return;

                        foreach (CustomAttr customattr in customattr_list)
                                customattr.AddTo (code_gen, module);
                }
        }
}
