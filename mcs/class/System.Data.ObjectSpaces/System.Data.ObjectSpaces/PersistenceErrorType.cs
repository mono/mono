//
// System.Data.ObjectSpaces.PersistenceErrorType.cs - The type of persistence error
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
//

#if NET_2_0

namespace System.Data.ObjectSpaces
{
        public enum PersistenceErrorType
        {
             Inserting,
             Deleting,
             Updating,
             Unknown
        }
}

#endif