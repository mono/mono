//
// System.Data.ObjectSpaces.Depth.cs - Specifies the shallowness of object changing operations
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
//

#if NET_2_0

namespace System.Data.ObjectSpaces
{
        public enum Depth
        {
             SingleObject,
             ObjectGraph
        }
}

#endif
