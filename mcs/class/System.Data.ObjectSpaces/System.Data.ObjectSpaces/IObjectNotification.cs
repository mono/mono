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
        [MonoTODO]        
        public interface IObjectNotification
        {
                [MonoTODO]
                public void OnCreated () {}
                
                [MonoTODO]
                public void OnCreating () {}
                
                [MonoTODO]
                public void OnDeleted () {}
                
                [MonoTODO]
                public void OnDeleting () {}
                
                [MonoTODO]
                public void OnMaterialized () {}
                
                [MonoTODO]
                public void OnPersistError () {}
                
                [MonoTODO]
                public void OnUpdated () {}
                
                [MonoTODO]
                public void OnUpdating () {}
                                                                                                                 
        }
}

#endif