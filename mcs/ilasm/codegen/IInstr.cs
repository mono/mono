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

        public interface IInstr {

                /// <summary>
                ///  Add this instruction to the supplied codebuffer
                /// </summary>
                void Emit (CodeGen code_gen, MethodDef meth, 
			   PEAPI.CILInstructions cil);
        }

}

