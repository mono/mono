//
// Mono.ILASM.CustomAttr
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;


namespace Mono.ILASM {

        public class CustomAttr {

                private IMethodRef method_ref;
                private byte[] data;

                public CustomAttr (IMethodRef method_ref, byte[] data)
                {
                        this.method_ref = method_ref;
                        this.data = data;
                }

                public void AddTo (CodeGen code_gen, PEAPI.MetaDataElement elem)
                {
                        method_ref.Resolve (code_gen);

                        elem.AddCustomAttribute (method_ref.PeapiMethod, data);
                }

        }

}

