//
// System.Data.ObjectSpaces.ObjectContext.cs : Handles identity and state for persistent objects.
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
//

#if NET_1_2

namespace System.Data.ObjectSpaces
{
        public abstract class ObjectContext
        {
                [MonoTODO]
                protected ObjectContext () {}
                
                [MonoTODO]
                public virtual void Add (object obj) {}
                
                [MonoTODO]
                public virtual void Add (object obj, ObjectState state) {}
                
                [MonoTODO]                
                public abstract void Delete (object obj) {}
                
                [MonoTODO]
                public abstract ValueRecord GetCurrentValueRecord (object obj)
                {
                        return null;        
                }
                  
                [MonoTODO]
                public static ObjectContext GetInternalContext (ObjectSpace objectSpace)
                {
                        return null;
                }
                
                [MonoTODO]
                public static ObjectContext GetInternalContext (ObjectSet objectSet)
                {
                        return null;
                }
                
                [MonoTODO]
                public abstract ObjectState GetObjectState (object obj)
                {
                        return null;        
                }
                
                [MonoTODO]
                public abstract ValueRecord GetOriginalValueRecord (object obj)
                {
                        return null;        
                }
                               
                [MonoTODO]
                public abstract void Import (ObjectContext context) {}
                                
                [MonoTODO]
                public abstract void Import (ObjectContext context, object obj) {}

			    [MonoTODO]
                public abstract void Remove (object obj) {}
                                
        }
}

#endif