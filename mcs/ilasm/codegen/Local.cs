//
// Mono.ILASM.Local
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;


namespace Mono.ILASM {

        public class Local {

                private int slot;
                private string name;
                private ITypeRef type;

                public Local (int slot, ITypeRef type) : this (slot, null, type) {

                }

                public Local (int slot, string name, ITypeRef type) {
                        this.slot = slot;
                        this.name = name;
                        this.type = type;
                }

                public int Slot {
                        get { return slot; }
                        set { slot = value; }
                }

                public string Name {
                        get { return name; }
                }

                public ITypeRef Type {
                        get { return type; }
                }

                public PEAPI.Local GetPeapiLocal (CodeGen code_gen)
                {
                        int ec = code_gen.Report.ErrorCount;
                        IGenericTypeRef gtr = type as IGenericTypeRef;
                        if (gtr == null)
                                type.Resolve (code_gen);
                        else
                                gtr.ResolveNoTypeSpec (code_gen);

                        if (code_gen.Report.ErrorCount > ec)
                                return null;

                        return new PEAPI.Local (name, type.PeapiType);
                }
        }

}

