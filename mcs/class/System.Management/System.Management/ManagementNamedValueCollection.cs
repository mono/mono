//
// System.Management.ManagementNamedValueCollection
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace System.Management
{
	public class ManagementNamedValueCollection : NameObjectCollectionBase
	{
		public ManagementNamedValueCollection ()
		{
		}

		public ManagementNamedValueCollection (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public void Add (string name, object value)
		{
			if (BaseGet (name) != null)
				BaseRemove (name);

			BaseAdd (name, value);
		}

		public ManagementNamedValueCollection Clone ()
		{
			ManagementNamedValueCollection result = new ManagementNamedValueCollection ();
			foreach (string key in Keys) {
				object value = BaseGet (key);
				if (value == null) {
					result.Add (key, value);
					continue;
				}

				if (value is ICloneable) {
					result.Add (key, ((ICloneable) value).Clone ());
				} else {
					result.Add (key, value);
				}
			}

			return result;
		}

		public void Remove (string name)
		{
			BaseRemove (name);
		}

		public void RemoveAll ()
		{
			BaseClear ();
		}

		public object this [string name]
		{
			get { return BaseGet (name); }
		}
	}
}

