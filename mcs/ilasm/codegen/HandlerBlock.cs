//
// Mono.ILASM.HandlerBlock
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper (Jackson@LatitudeGeo.com)
//


using System;

namespace Mono.ILASM {

        public class HandlerBlock {

                private string from_label;
                private string to_label;

                public HandlerBlock (string from_label, string to_label)
                {
                        this.from_label = from_label;
                        this.to_label = to_label;
                }

                public PEAPI.CILLabel GetFromLabel (CodeGen code_gen, MethodDef method)
                {
                        return method.GetLabelDef (from_label);
                }

                public PEAPI.CILLabel GetToLabel (CodeGen code_gen, MethodDef method)
                {
                        return method.GetLabelDef (to_label);
                }
        }

}


