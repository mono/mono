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

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0

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
