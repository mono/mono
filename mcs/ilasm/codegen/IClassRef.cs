//
// Mono.ILASM.IClassRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//

using System;

namespace Mono.ILASM {

        public interface IClassRef : ITypeRef {

                PEAPI.Class PeapiClass { get; }

        }

}

