//
// System.Data.ObjectSpaces.ObjectEngine.cs : Handles low-level object persistence operations with data sources.
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
using System.Data.Mapping;
using System.Reflection;

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
