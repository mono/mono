//
// System.Data.ObjectSpaces.ObjectSpace.cs : Handles high-level object persistence interactions with a data source.
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
//

#if NET_1_2

using System.Collections;
using System.Data;
using System.Data.Mapping;

namespace System.Data.ObjectSpaces
{
        public class ObjectSpace
        {        
                [MonoTODO]                
                public ObjectSpace (MappingSchema map, ObjectSources sources) {}
                
                [MonoTODO]
                public ObjectSpace (string mapFile, IDbConnection conn) {}
                
                [MonoTODO]
                public ObjectSpace (string mapFile, ObjectSources sources) {}
                
                [MonoTODO]
                public ObjectSpace (MappingSchema map, IDbConnection conn) {}

                [MonoTODO]
                public object GetObject (ObjectQuery query, object[] parameters)
                {
                        return null;
                }

                [MonoTODO]
                public object GetObject (Type type, string queryString)
                {
                        return null;
                }

                [MonoTODO]
                public object GetObject (Type type, string queryString, string relatedSpan)
                {
                        return null;
                }

                [MonoTODO]
                public ObjectReader GetObjectReader (ObjectQuery query, object[] parameters)
                {
                        return null;
                }

                [MonoTODO]
                public ObjectReader GetObjectReader (Type type, string queryString)
                {
                        return null;
                }

                [MonoTODO]
                public ObjectReader GetObjectReader (Type type, string queryString, string relatedSpan)
                {
                        return null;
                }

                [MonoTODO]
                public ObjectSet GetObjectSet (ObjectQuery query, object[] parameters)
                {
                        return null;
                }

                [MonoTODO]
                public ObjectSet GetObjectSet (Type type, string queryString)
                {
                        return null;
                }

                [MonoTODO]
                public ObjectSet GetObjectSet (Type type, string queryString, string relatedSpan)
                {
                        return null;
                }

                [MonoTODO]
                public void MarkForDeletion (object obj) {}        

                [MonoTODO]
                public void MarkForDeletion (ICollection objs) {}

                [MonoTODO]
                public void PersistChanges (object obj) {}

                [MonoTODO]
                public void PersistChanges (object obj, PersistenceOptions options) {}

                [MonoTODO]
                public void PersistChanges (ICollection objs) {}

                [MonoTODO]
                public void PersistChanges (ICollection objs, PersistenceOptions options) {}

                [MonoTODO]        
                public void Resync (object obj, Depth depth) {}

                [MonoTODO]
                public void Resync (ICollection objs, Depth depth) {}

                [MonoTODO]
                public void StartTracking (object obj, InitialState state) {}
        
                [MonoTODO]
                public void StartTracking (ICollection objs, InitialState state) {}
        
        }
}

#endif
