//
// System.Web.UI.ObjectPersistData.cs
//
// Authors:
//     Arina Itkes (arinai@mainsoft.com)
//
// (C) 2007 Mainsoft Co. (http://www.mainsoft.com)
//
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
using System.Web.UI;

namespace System.Web.UI
{
	public class ObjectPersistData
	{
		public ObjectPersistData (ControlBuilder builder, IDictionary builtObjects) {
			throw new NotImplementedException ();
		}
		public ICollection AllPropertyEntries { get { throw new NotImplementedException (); } }
		public IDictionary BuiltObjects { get { throw new NotImplementedException (); } }
		public ICollection CollectionItems { get { throw new NotImplementedException (); } }
		public ICollection EventEntries { get { throw new NotImplementedException (); } }
		public bool IsCollection { get { throw new NotImplementedException (); } }
		public bool Localize { get { throw new NotImplementedException (); } }
		public Type ObjectType { get { throw new NotImplementedException (); } }
		public string ResourceKey { get { throw new NotImplementedException (); } }
		public void AddToObjectControlBuilderTable (IDictionary table) {
			throw new NotImplementedException ();
		}
		public IDictionary GetFilteredProperties (string filter) {
			throw new NotImplementedException ();
		}
		public PropertyEntry GetFilteredProperty (string filter, string name) {
			throw new NotImplementedException ();
		}
		public ICollection GetPropertyAllFilters (string name) {
			throw new NotImplementedException ();
		}
	}
}
#endif
