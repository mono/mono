//
// Mono.ILASM.DeclSecurity
//
// Author(s):
//  Ankit Jain  <JAnkit@novell.com>
//
// (C) 2005 Ankit Jain, All rights reserved
//


using System;
using System.Collections;

namespace Mono.ILASM {

        public interface IDeclSecurityTarget {
                void AddDeclSecurity (DeclSecurity declsecurity);
        }

        public class DeclSecurity {

		private PEAPI.SecurityAction sec_action;
		private byte[] data;

		public DeclSecurity (PEAPI.SecurityAction sec_action, byte [] data)
                {
			this.sec_action = sec_action;
                        this.data = data;
                }

                public void AddTo (CodeGen code_gen, PEAPI.MetaDataElement elem)
                {
                        code_gen.PEFile.AddDeclSecurity (sec_action, data, elem);
                }

        }

}
