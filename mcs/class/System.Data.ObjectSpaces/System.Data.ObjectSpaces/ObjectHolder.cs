//
// System.Data.ObjectSpaces.ObjectHolder.cs - An object wrapper to facilitate delayed loading
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
        public class ObjectHolder
        {
                private object innerObject;     //The wrapped object
                
                [MonoTODO]
                public ObjectHolder () {}
                
                [MonoTODO]
                public object InnerObject {
                        get { return this.innerObject; }
                        set { this.innerObject = value; }
                }        
                                
        }
}

#endif