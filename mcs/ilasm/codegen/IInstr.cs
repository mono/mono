//
// Mono.ILASM.IInstr
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;

namespace Mono.ILASM {

        public abstract class IInstr {

		public readonly Location Location;

		/// <summary>
		/// </summary>
		/// <param name="opcode"></param>
		public IInstr (Location loc)
		{
			this.Location = (Location) loc.Clone ();
		}

                /// <summary>
                ///  Add this instruction to the supplied codebuffer
                /// </summary>
                public abstract void Emit (CodeGen code_gen, MethodDef meth, 
					   PEAPI.CILInstructions cil);
        }

}
