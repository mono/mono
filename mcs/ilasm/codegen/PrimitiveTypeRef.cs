//
// Mono.ILASM.PrimitiveTypeRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;

namespace Mono.ILASM {

        /// <summary>
        /// Reference to a primitive type, ie string, object, char
        /// </summary>
        public class PrimitiveTypeRef : PeapiTypeRef, ITypeRef {

                public PrimitiveTypeRef (PEAPI.PrimitiveType prim_type,
                                string full_name) : base (prim_type, full_name)
                {

                }

                public IClassRef AsClassRef (CodeGen code_gen)
                {
                        PEAPI.ClassRef class_ref = code_gen.ExternTable.GetValueClass ("corlib", FullName);
                        ExternTypeRef type_ref = new ExternTypeRef (class_ref, FullName);

                        // TODO: Need to do the rest of the conversion (in order)
                        if (IsArray)
                                type_ref.MakeArray ();

                        return type_ref;
                }

        }

}

