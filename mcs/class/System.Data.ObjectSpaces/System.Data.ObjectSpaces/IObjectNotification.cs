//
// System.Data.ObjectSpaces.IObjectNotification.cs - Provides notification of events during a persisted objects lifetime
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
//

#if NET_1_2

namespace System.Data.ObjectSpaces
{
        public interface IObjectNotification
        {
                void OnCreated ();
                void OnCreating ();
                void OnDeleted ();
                void OnDeleting ();
                void OnMaterialized ();
                void OnPersistError ();
                void OnUpdated ();
                void OnUpdating ();
                                                                                                                 
        }
}

#endif
