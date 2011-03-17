//
// System.Collections.Specialized.StringDictionary.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if !NET_2_1
using System.ComponentModel.Design.Serialization;
#endif
using System.Globalization;

namespace System.Collections.Specialized {

	[Serializable]
#if !NET_2_1
	[DesignerSerializer ("System.Diagnostics.Design.StringDictionaryCodeDomSerializer, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.Serialization.CodeDomSerializer, " + Consts.AssemblySystem_Design)]
#endif
	public class StringDictionary : IEnumerable
	{
		private Hashtable contents;
			
		public StringDictionary()
		{
			contents = new Hashtable();
		}
		
		// Public Instance Properties
		
		public virtual int Count
		{
			get {
				return contents.Count;
			}
		}
		
		public virtual bool IsSynchronized
		{
			get {
				return false;
			}
		}
		
		public virtual string this[string key]
		{
			get {
				if (key == null)
					throw new ArgumentNullException ("key");
				return (string) contents [key.ToLower (CultureInfo.InvariantCulture)];
			}
			
			set {
				if (key == null)
					throw new ArgumentNullException ("key");
				contents[key.ToLower(CultureInfo.InvariantCulture)] = value;
			}
		}
		
		public virtual ICollection Keys
		{
			get {
				return contents.Keys;
			}
		}
		
		public virtual ICollection Values
		{
			get {
				return contents.Values;
			}
		}
		
		public virtual object SyncRoot
		{
			get {
				return contents.SyncRoot;
			}
		}
		
		// Public Instance Methods
		
		public virtual void Add(string key, string value)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			contents.Add (key.ToLower (CultureInfo.InvariantCulture), value);
		}
		
		public virtual void Clear()
		{
			contents.Clear();
		}
		
		public virtual bool ContainsKey(string key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			return contents.ContainsKey (key.ToLower (CultureInfo.InvariantCulture));
		}
		
		public virtual bool ContainsValue(string value)
		{
			return contents.ContainsValue(value);
		}
		
		public virtual void CopyTo(Array array, int index)
		{
			contents.CopyTo(array, index);
		}
		
		public virtual IEnumerator GetEnumerator()
		{
			return contents.GetEnumerator();
		}
		
		public virtual void Remove(string key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			contents.Remove (key.ToLower (CultureInfo.InvariantCulture));
		}
	}
}
