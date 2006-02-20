//
// Mono.ILASM.LabelInfo
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//

using System;

namespace Mono.ILASM {

        public class LabelInfo : IComparable {

                public readonly string Name;
                public readonly int Pos;
                public readonly uint Offset;
                public PEAPI.CILLabel Label;
                public bool UseOffset;

                public LabelInfo (string name, int pos, uint offset)
                {
                        Name = name;
                        Pos = pos;
                        Offset = offset;
                        Label = null;
                        UseOffset = true;
                }

                public LabelInfo (string name, int pos)
                {
                        Name = name;
                        Pos = pos;
                        Label = null;
                        UseOffset = false;
                }

                public void Define (PEAPI.CILLabel label)
                {
                        Label = label;
                }

                public int CompareTo (object obj)
                {
                        LabelInfo other = obj as LabelInfo;

                        if(other != null)
                                return Pos.CompareTo(other.Pos);

                        throw new InternalErrorException ("object is not a LabelInfo");
                }

                public override string ToString ()
                {
                        if (Name != null)
                                return Name;
                        return "IL_" + Pos;
                }
        }

}

