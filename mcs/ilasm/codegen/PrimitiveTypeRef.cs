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

                /// <summary>
                /// Primitive types can be created like this System.String instead
                /// of like a normal type that would be [mscorlib]System.String This
                /// method returns a proper primitive type if the supplied name is
                /// the name of a primitive type.
                /// </summary>
                public static PrimitiveTypeRef GetPrimitiveType (string full_name)
                {
                        switch (full_name) {
                        case "System.String":
                                return new PrimitiveTypeRef (PEAPI.PrimitiveType.String, full_name);
                        case "System.Object":
                                return new PrimitiveTypeRef (PEAPI.PrimitiveType.Object, full_name);
                        default:
                                return null;
                        }
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

