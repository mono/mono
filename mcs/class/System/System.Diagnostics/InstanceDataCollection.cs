//
// System.Diagnostics.InstanceDataCollection.cs
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

	public class InstanceDataCollection : DictionaryBase {

		private string counterName;

		private static void CheckNull (object value, string name)
		{
			if (value == null)
				throw new ArgumentNullException (name);
		}

		// may throw ArgumentNullException
		public InstanceDataCollection (string counterName)
		{
			CheckNull (counterName, "counterName");
			this.counterName = counterName;
		}

		public string CounterName {
			get {return counterName;}
		}

		// may throw ArgumentNullException
		public InstanceData this [string instanceName] {
			get {
				CheckNull (instanceName, "instanceName");
				return (InstanceData) Dictionary [instanceName];
			}
		}

		public ICollection Keys {
			get {return Dictionary.Keys;}
		}

		public ICollection Values {
			get {return Dictionary.Values;}
		}

		// may throw ArgumentNullException
		public bool Contains (string instanceName)
		{
			CheckNull (instanceName, "instanceName");
			return Dictionary.Contains (instanceName);
		}

		public void CopyTo (InstanceData[] instances, int index)
		{
			Dictionary.CopyTo (instances, index);
		}
	}
}

