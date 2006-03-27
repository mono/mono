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
		
                private LabelInfo from_label;
                private LabelInfo to_label;

                public HandlerBlock (LabelInfo from_label, LabelInfo to_label)
                {
                        this.from_label = from_label;
                        this.to_label = to_label;
                }

                public PEAPI.CILLabel GetFromLabel (CodeGen code_gen, MethodDef method)
                {
			return from_label.Label;
                }

                public PEAPI.CILLabel GetToLabel (CodeGen code_gen, MethodDef method)
                {
			return to_label.Label;
                }
        }

}


