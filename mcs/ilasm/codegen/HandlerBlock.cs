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
		
		private enum Mode {
			Str,
			Pos,
			Offset
		}

		private Mode mode;
                private string from_label;
                private string to_label;
		private int from_pos;
		private int to_pos;
		private int from_offset;
		private int to_offset;

                public HandlerBlock (string from_label, string to_label)
                {
                        this.from_label = from_label;
                        this.to_label = to_label;
			mode = Mode.Str;
                }

		public HandlerBlock (int from_pos, int to_pos)
		{
			this.from_pos = from_pos;
			this.to_pos = to_pos;
			mode = Mode.Pos;
		}

		public HandlerBlock (int from_offset, int to_offset, bool place_holder)
		{
			this.from_offset = from_offset;
			this.to_offset = to_offset;
			mode = Mode.Offset;
		}

                public PEAPI.CILLabel GetFromLabel (CodeGen code_gen, MethodDef method)
                {
			switch (mode) {
			case Mode.Str: 
				return method.GetLabelDef (from_label);
			case Mode.Pos:
				return method.GetLabelDef (from_pos);
			case Mode.Offset:
				return method.GetLabelDef ((uint) from_offset);
			default:
				throw new Exception ("Should not reach this point.");
			}
                }

                public PEAPI.CILLabel GetToLabel (CodeGen code_gen, MethodDef method)
                {
			switch (mode) {
			case Mode.Str: 
				return method.GetLabelDef (to_label);
			case Mode.Pos:
				return method.GetLabelDef (to_pos);
			case Mode.Offset:
				return method.GetLabelDef ((uint) to_offset);
			default:
				throw new Exception ("Should not reach this point.");
			}
                }
        }

}


