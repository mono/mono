//
// System.Data.ObjectSpaces.DbObjectReader.cs
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

using System.Data;
using System.Data.Mapping;

namespace System.Data.ObjectSpaces
{
        public class DbObjectReader : ObjectReader
        {
                [MonoTODO]
                public DbObjectReader (IDataReader dataReader, Type type, MappingSchema map) 
                {
                        if (dataReader == null || type == null || map == null)
                                throw new ObjectException ();
                        
                }
                        
                [MonoTODO]                        
                public DbObjectReader (IDataReader dataReader, Type type, MappingSchema map, ObjectContext context)
                {
                        if (dataReader == null || type == null || map == null || context == null)
                                throw new ObjectException (); 
                }

		[MonoTODO]
		public override bool HasObjects {
			get { throw new NotImplementedException (); }
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
