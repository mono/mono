//
// System.Data.ObjectSpaces.ObjectState.cs - The modification state of a persitent object
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
//

#if NET_1_2

namespace System.Data.ObjectSpaces
{
        public enum ObjectState
        {
             Unknown,
             Unchanged,
             Inserted,
             Updated,
             Deleted
        }
}

#endif