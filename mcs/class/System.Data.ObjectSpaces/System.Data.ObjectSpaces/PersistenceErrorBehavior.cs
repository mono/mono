//
// System.Data.ObjectSpaces.PersistenceErrorBehavior.cs - The behaviour to follow when a persistence error occurs
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data.ObjectSpaces
{
        public enum PersistenceErrorBehavior
        {
             ThrowAtFirstError,
             ThrowAfterCompletion
        }
}

#endif
