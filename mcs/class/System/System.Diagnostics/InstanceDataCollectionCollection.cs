//
// System.Diagnostics.InstanceDataCollectionCollection.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

using System;
using System.Collections;
using System.Diagnostics;

namespace System.Diagnostics {

	public class InstanceDataCollectionCollection : DictionaryBase {

		private static void CheckNull (object value, string name)
		{
			if (value == null)
				throw new ArgumentNullException (name);
		}

		// may throw ArgumentNullException
		public InstanceDataCollectionCollection ()
		{
		}

		// may throw ArgumentNullException
		public InstanceDataCollection this [string counterName] {
			get {
				CheckNull (counterName, "counterName");
				return (InstanceDataCollection) Dictionary [counterName];
			}
		}

		public ICollection Keys {
			get {return Dictionary.Keys;}
		}

		public ICollection Values {
			get {return Dictionary.Values;}
		}

		// may throw ArgumentNullException
		public bool Contains (string counterName)
		{
			CheckNull (counterName, "counterName");
			return Dictionary.Contains (counterName);
		}

		public void CopyTo (InstanceDataCollection[] counters, int index)
		{
			Dictionary.CopyTo (counters, index);
		}
	}
}

