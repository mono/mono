//
// Mono.ILASM.IClassRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//  Ankit Jain  (JAnkit@novell.com)
//
// (C) 2003 Jackson Harper, All rights reserved
// (C) 2005 Novell, Inc (http://www.novell.com)
//

using System;

namespace Mono.ILASM {

        public interface BaseClassRef : BaseTypeRef {

                PEAPI.Class PeapiClass { get; }

                void MakeValueClass ();

                IClassRef Clone ();
                
                /* Returns the Generic Instance for the BaseClassRef */
                GenericTypeInst GetGenericTypeInst (GenericArguments gen_args);

                /* Resolves the Generic instance and returns the 
                   resolved type (typically, PEAPI.GenericTypeInst) */
                PEAPI.Type ResolveInstance (CodeGen code_gen, GenericArguments gen_args);
        }

}

