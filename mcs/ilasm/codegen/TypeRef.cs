//
// Mono.ILASM.TypeRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//

using PEAPI;
using System;


namespace Mono.ILASM {

        public class TypeRef {

                public readonly PEAPI.Type Type;
                public readonly string FullName;

                public bool Pinned;

                public TypeRef (PEAPI.Type type, string full_name)
                {
                        Type = type;
                        FullName = full_name;
                        Pinned = false;
                }

                public override string ToString ()
                {
                        return FullName;
                }

        }

}



