//
// System.Data.ObjectSpaces.DbObjectReader.cs
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
//

#if NET_1_2

using System.Data.MappingSchema;

namespace System.Data.ObjectSpaces
{
        public class DbObjectReader : ObjectReader
        {
                [MonoTODO]
                public DbObjectReader (IDataReader dataReader, Type type, MappingSchema map) 
                {
                        if (dataReader == null || type == null || map == null)
                                throw new ObjectException();
                        
                }
                        
                [MonoTODO]                        
                public DbObjectReader(IDataReader dataReader, Type type, MappingSchema map,
                        ObjectContext context)
                {
                        if (dataReader == null || type == null || map == null || context == null)
                                throw new ObjectException(); 
                }
                
                [MonoTODO]
                public bool NextResult (Type type, MappingSchema map)
                {
                        return false;       
                }

                [MonoTODO]
                public bool NextResult (Type type, MappingSchema map, ObjectContext context)
                {
                        return false;       
                }
                
                [MonoTODO]
                public override void Close ()
                { 
                        base.Close();
                }
         
                [MonoTODO]
                public override bool Read()
                {
                        return false;       
                }
        }
}

#endif
