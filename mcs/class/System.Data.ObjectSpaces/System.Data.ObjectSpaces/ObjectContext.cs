//
// System.Data.ObjectSpaces.ObjectContext.cs : Handles identity and state for persistent objects.
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
                  
                public static ObjectContext GetInternalContext (ObjectSpace objectSpace)
                {
			return objectSpace.ObjectContext;
                }
                
                public static ObjectContext GetInternalContext (ObjectSet objectSet)
                {
			return objectSet.ObjectContext;
                }
                
                public abstract ObjectState GetObjectState (object obj);
                public abstract ValueRecord GetOriginalValueRecord (object obj);
                public abstract void Import (ObjectContext context);
                public abstract void Import (ObjectContext context, object obj);
                public abstract void Remove (object obj);
                                
        }
}

#endif
