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
