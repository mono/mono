//
// MemoryCacheSettingsCollection.cs
//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc. (http://novell.com/)
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
using System;
using System.Configuration;

namespace System.Runtime.Caching.Configuration
{
	[ConfigurationCollection (typeof(MemoryCacheElement), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
	public sealed class MemoryCacheSettingsCollection : ConfigurationElementCollection
	{
		static ConfigurationPropertyCollection properties;
		
		public override ConfigurationElementCollectionType CollectionType {
			get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
		}

		public MemoryCacheElement this[int index] {
			get { return BaseGet (index) as MemoryCacheElement; }
			set {
				if (BaseGet (index) != null)
					BaseRemoveAt (index);
				BaseAdd (index, value);
			}
		}

		public new MemoryCacheElement this[string key] {
			get {
				foreach (MemoryCacheElement mce in this) {
					if (String.Compare (key, mce.Name, StringComparison.Ordinal) == 0)
						return mce;
				}

				return null;
			}
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
		
		static MemoryCacheSettingsCollection ()
		{
			properties = new ConfigurationPropertyCollection ();
		}
		
		public MemoryCacheSettingsCollection ()
		{
		}

		public void Add (MemoryCacheElement cache)
		{
			BaseAdd (cache);
		}

		public void Clear ()
		{
			BaseClear ();
		}
		
		protected override ConfigurationElement CreateNewElement ()
		{
			return new MemoryCacheElement ();
		}

		protected override ConfigurationElement CreateNewElement (string elementName)
		{
			return new MemoryCacheElement (elementName);
		}
		
		protected override object GetElementKey (ConfigurationElement element)
		{
			if (element == null)
				return null;

			return ((MemoryCacheElement)element).Name;
		}

		public int IndexOf (MemoryCacheElement cache)
		{
			if (cache == null)
				return -1;

			return BaseIndexOf (cache);
		}

		public void Remove (MemoryCacheElement cache)
		{
			if (cache == null)
				return;

			BaseRemove (GetElementKey (cache));
		}

		public void RemoveAt (int index)
		{
			BaseRemoveAt (index);
		}
	}
}
