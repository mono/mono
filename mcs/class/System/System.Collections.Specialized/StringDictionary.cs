//
// System.Collections.Specialized.StringDictionary.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.ComponentModel.Design.Serialization;

namespace System.Collections.Specialized
{
#if (NET_1_0)
	[DesignerSerializer ("System.Diagnostics.Design.StringDictionaryCodeDomSerializer, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
#endif
#if (NET_1_1)
    	[DesignerSerializer ("System.Diagnostics.Design.StringDictionaryCodeDomSerializer, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
#endif
	public class StringDictionary : IEnumerable
	{
		private Hashtable table;
			
		public StringDictionary()
		{
			table = new Hashtable();
		}
		
		// Public Instance Properties
		
		public virtual int Count
		{
			get {
				return table.Count;
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
				return (string) table[key.ToLower()];
			}
			
			set {
				table[key.ToLower()] = value;
			}
		}
		
		public virtual ICollection Keys
		{
			get {
				return table.Keys;
			}
		}
		
		public virtual ICollection Values
		{
			get {
				return table.Values;
			}
		}
		
		public virtual object SyncRoot
		{
			get {
				return table.SyncRoot;
			}
		}
		
		// Public Instance Methods
		
		public virtual void Add(string key, string value)
		{
			table.Add(key.ToLower(), value);
		}
		
		public virtual void Clear()
		{
			table.Clear();
		}
		
		public virtual bool ContainsKey(string key)
		{
			return table.ContainsKey(key.ToLower());
		}
		
		public virtual bool ContainsValue(string value)
		{
			return table.ContainsValue(value);
		}
		
		public virtual void CopyTo(Array array, int index)
		{
			table.CopyTo(array, index);
		}
		
		public virtual IEnumerator GetEnumerator()
		{
			return table.GetEnumerator();
		}
		
		public virtual void Remove(string key)
		{
			table.Remove(key.ToLower());
		}
	}
}