//
// System.Data.ObjectSpaces.UpdateErrorType.cs - The type of an update error
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
//

#if NET_2_0

namespace System.Data.ObjectSpaces
{
        public enum UpdateErrorType
        {
             Inserting,
             Deleting,
             Updating,
             Unknown
        }
}

#endif