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
                protected ObjectContext () 
		{
		}
                
                public virtual void Add (object obj) 
		{
			Add (obj, (ObjectState) (-1));
		}
                
                [MonoTODO]
                public virtual void Add (object obj, ObjectState state) 
		{
		}

                public abstract void Delete (object obj);
                public abstract ValueRecord GetCurrentValueRecord (object obj);
                  
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
                
                public abstract ObjectState GetObjectState (object obj);
                public abstract ValueRecord GetOriginalValueRecord (object obj);
                public abstract void Import (ObjectContext context);
                public abstract void Import (ObjectContext context, object obj);
                public abstract void Remove (object obj);
                                
        }
}

#endif
