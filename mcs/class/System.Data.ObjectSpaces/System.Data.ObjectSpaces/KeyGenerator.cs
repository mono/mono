//
// System.Data.ObjectSpaces.KeyGenerator.cs
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
        public abstract class KeyGenerator
        {
                [MonoTODO]
                protected KeyGenerator () ;
                
                public abstract Type OwnerType { get; }

                public abstract string UserParameter { get; }
                
                public abstract Type GetKeyType ();
                
                public abstract void Initialize (Type type, string userParameter);
                
                public abstract object NextKey ();
        }
}

#endif