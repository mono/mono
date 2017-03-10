//
// Mono.ILASM.DataDef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;

namespace Mono.ILASM {

        public class DataDef {

                private string name;
                private PEAPI.DataSegment segment;

                private PEAPI.Constant constant;

                public DataDef (string name, PEAPI.DataSegment segment)
                {
                        this.name = name;
                        this.segment = segment;
                }

                public PEAPI.Constant PeapiConstant {
                        get { return constant; }
                        set { constant = value; }
                }

                public string Name {
                        get { return name; }
                        set { name = value; }
                }
        }

}

