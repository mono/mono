//
// System.Data.ObjectSpaces.CommonObjectContext.cs : A basic ObjectContext for handling persistent object identity and state.
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
//

#if NET_1_2

using System.Data.ObjectSpaces.Schema;

namespace System.Data.ObjectSpaces
{
        public class CommonObjectContext : ObjectContext
        {
                 [MonoTODO]
                public CommonObjectContext (ObjectSchema objectSchema)
                {
                        if (objectSchema == null)
                                throw ObjectException.CreateObjectException ();
                }
                
                [MonoTODO]
                public override void Add (object obj, ObjectState state)
                {
                        if (obj == null)
                                throw ObjectException.CreateObjectException ();
                }
                
                [MonoTODO]
                public override void Delete (object obj)
                {
                        if (obj == null)
                                throw ObjectException.CreateObjectException ();
                }
                
                [MonoTODO]
                public override ValueRecord GetCurrentValueRecord (object obj)
                {
                        if (obj == null)
                                throw ObjectException.CreateObjectException ();
                        
                        return null;        
                }
                               
                [MonoTODO]
                public override ObjectState GetObjectState (object obj)
                {
                        if (obj == null)
                                throw ObjectException.CreateObjectException ();
                      
			throw new NotImplementedException ();
                }
                
                [MonoTODO]
                public override ValueRecord GetOriginalValueRecord (object obj)
                {
                        if (obj == null)
                                throw ObjectException.CreateObjectException ();
                        
                        return null;        
                }
                
                [MonoTODO]
                public override void Import (ObjectContext context)
                {
                        if (context == null)
                                throw ObjectException.CreateObjectException ();
                }
                                
                [MonoTODO]
                public override void Import (ObjectContext context, object obj)
                {
                        if (context == null)
                                throw ObjectException.CreateObjectException ();
                }

                [MonoTODO]
                public override void Remove (object obj)
                {
                        if (obj == null)
                                throw ObjectException.CreateObjectException ();
                }
                
        }
}

#endif
