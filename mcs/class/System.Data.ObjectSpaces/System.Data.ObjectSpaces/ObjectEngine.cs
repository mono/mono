//
// System.Data.ObjectSpaces.ObjectEngine.cs : Handles low-level object persistence operations with data sources.
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
//

#if NET_1_2

using System.Collections;
using System.Data.Mapping;

namespace System.Data.ObjectSpaces
{
        [MonoTODO]
        public class ObjectEngine
        {
                [MonoTODO]
                public static void Fetch (MappingSchema map, ObjectSources sources, ObjectContext context,
                        object obj, string propertyName) {}

                [MonoTODO]
                public static ObjectReader GetObjectReader  (ObjectSources sources, ObjectContext context,
                        CompiledQuery compiledQuery, object[] parameters)
                {
                        return null;        
                }

                [MonoTODO]
                public static ValueRecord GetPersistentValueRecord (MappingSchema map, ObjectSources sources, object obj)
                {
                        return null;        
                }
                                                                       
                [MonoTODO]
                public static void PersistChanges(MappingSchema map, ObjectSources sources,
                        ObjectContext context, ICollection objs, PersistenceOptions options) {}
                                                  
                [MonoTODO]
                public static void Resync (MappingSchema map, ObjectSources sources,
                        ObjectContext context, ICollection objs, Depth depth) {}
                                                                                                                 
        }
}

#endif