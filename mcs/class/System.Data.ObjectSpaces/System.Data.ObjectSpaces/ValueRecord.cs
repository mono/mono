//
// System.Data.ObjectSpaces.ValueRecord.cs
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
        public class ValueRecord : IEnumerable
        {

                [MonoTODO]
                public Type ObjectType { 
                        get { return null; }
                }
                
                [MonoTODO]
                public object this[string propertyName] { 
                        get { return null; } 
                        set {}
                } 
 
                [MonoTODO]
                public IEnumerator GetEnumerator () {}
                
                [MonoTODO]
                public override int GetHashCode () 
                {
                        return 0;        
                }
                
                [MonoTODO]
                public override bool Equals (object value) 
                {
                        return false;        
                }
        }
}

#endif