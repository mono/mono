//
// System.Data.ObjectSpaces.ValueRecord.cs
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
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

using System.Collections;

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
                public IEnumerator GetEnumerator () 
		{
			throw new NotImplementedException ();
		}
                
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
