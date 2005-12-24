//
// Mono.ILASM.FeatureAttr
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;

namespace Mono.ILASM {

        [Flags]
        public enum FeatureAttr {
                None = 0x0,
                Instance = 0x1,
                Rtspecialname = 0x2,
                Specialname = 0x4
        }

}

