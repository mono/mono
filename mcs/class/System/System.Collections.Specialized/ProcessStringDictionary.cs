//
// System.Collections.Specialized.ProcessStringDictionary.cs
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximin.com)
//
// (c) 2004 Novell, Inc. (http://www.novell.com)
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
			IHashCodeProvider hash_provider = null;
			IComparer comparer = null;

			// check for non-Unix platforms - see FAQ for more details
			// http://www.mono-project.com/FAQ:_Technical#How_to_detect_the_execution_platform_.3F
			int platform = (int) Environment.OSVersion.Platform;
			if ((platform != 4) && (platform != 128)) {
				hash_provider = CaseInsensitiveHashCodeProvider.DefaultInvariant;
				comparer = CaseInsensitiveComparer.DefaultInvariant;
			}

			table = new Hashtable (hash_provider, comparer);
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

