//
// System.Data.ObjectSpaces.PersistenceErrorBehaviour.cs - The behaviour to follow when a persistence error occurs
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
             ThrowAtFirstError,
             ThrowAfterCompletion
        }
}

#endif