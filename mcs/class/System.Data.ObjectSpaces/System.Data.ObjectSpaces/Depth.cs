//
// System.Data.ObjectSpaces.Depth.cs - Specifies the shallowness of object changing operations
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
//

#if NET_1_2

namespace System.Data.ObjectSpaces
{
        public enum Depth
        {
             SingleObject,
             ObjectGraph
        }
}

#endif
