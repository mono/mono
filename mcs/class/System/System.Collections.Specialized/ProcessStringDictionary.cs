//
// System.Collections.Specialized.ProcessStringDictionary.cs
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximin.com)
//
// (c) 2004 Novell, Inc. (http://www.novell.com)
//

// StringDictionary turns all the keys into lower case. That's not good
// when ProcessStartInfo.EnvironmentVariables is used on *nix.

using System.Collections;

namespace System.Collections.Specialized
{
	class ProcessStringDictionary : StringDictionary, IEnumerable
	{
		Hashtable table;

		public ProcessStringDictionary ()
		{
			table = new Hashtable (CaseInsensitiveHashCodeProvider.Default,
						CaseInsensitiveComparer.Default);
		}
		
		public override int Count {
			get { return table.Count; }
		}
		
		public override bool IsSynchronized {
			get { return false; }
		}
		
		public override string this [string key] {
			get { return (string) table [key]; }
			
			set { table [key] = value; }
		}
		
		public override ICollection Keys {
			get {
				return table.Keys;
			}
		}
		
		public override ICollection Values {
			get { return table.Values; }
		}
		
		public override object SyncRoot {
			get { return table.SyncRoot; }
		}
		
		public override void Add (string key, string value)
		{
			table.Add (key, value);
		}
		
		public override void Clear ()
		{
			table.Clear ();
		}
		
		public override bool ContainsKey (string key)
		{
			return table.ContainsKey (key);
		}
		
		public override bool ContainsValue (string value)
		{
			return table.ContainsValue (value);
		}
		
		public override void CopyTo (Array array, int index)
		{
			table.CopyTo (array, index);
		}
		
		public override IEnumerator GetEnumerator ()
		{
			return table.GetEnumerator();
		}
		
		public override void Remove(string key)
		{
			table.Remove (key);
		}
	}
}

