//
// System.Data.ObjectSpaces.ObjectKey.cs : Provides a unique identifer for persistable objects
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
//

#if NET_1_2

namespace System.Data.ObjectSpaces
{
        public class ObjectKey
        {
                
                [MonoTODO]
                public ObjectKey (Type type, object[] values) {}
                
                [MonoTODO]
                public ObjectKey (Type type, string key) {}

                [MonoTODO]
                public string KeyText { 
                        get{ return String.Empty; }
                }
                
                [MonoTODO]
                public Type Type { 
                        get{ return typeof(object); }
                }
                
                [MonoTODO]
                public object[] Values { 
                        get{ return null; }
                }
        }
}

#endif