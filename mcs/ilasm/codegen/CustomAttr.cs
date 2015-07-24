//
// Mono.ILASM.CustomAttr
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;
using System.Collections;

namespace Mono.ILASM {

        public interface ICustomAttrTarget {
                void AddCustomAttribute (CustomAttr customattr);
        }

        public class CustomAttr {

                private BaseMethodRef method_ref;
                PEAPI.Constant constant;
                public CustomAttr (BaseMethodRef method_ref, PEAPI.Constant constant)
                {
                    this.method_ref = method_ref;
                    this.constant = constant;
                }

                public void AddTo (CodeGen code_gen, PEAPI.MetaDataElement elem)
                {
                        method_ref.Resolve (code_gen);
                        code_gen.PEFile.AddCustomAttribute (method_ref.PeapiMethod, constant, elem);
                }

                public bool IsSuppressUnmanaged (CodeGen codegen)
                {
			string asmname = "";
			
			BaseTypeRef owner = method_ref.Owner;
			if (owner == null)
				return false;
				
			ExternTypeRef etr = owner as ExternTypeRef;
			if (etr != null) {
				ExternAssembly ea = etr.ExternRef as ExternAssembly;
				if (ea != null)
					asmname = ea.Name;
			}	

                       	return (owner.FullName == "System.Security.SuppressUnmanagedCodeSecurityAttribute" 
				&& (asmname == "mscorlib" || codegen.IsThisAssembly ("mscorlib")) );
                }
        }

}

