//
// ServiceModelConfigurationElementCollection.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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

namespace System.ServiceModel.Configuration
{
	public abstract class ServiceModelConfigurationElementCollection<ConfigurationElementType> : ConfigurationElementCollection
		where ConfigurationElementType : ConfigurationElement, new()
	{
		internal ServiceModelConfigurationElementCollection ()
			: base (StringComparer.Ordinal)
		{
		}

		public ConfigurationElementType this [int index] {
			get { return (ConfigurationElementType) base.BaseGet (index); }
			set {
				if (Count <= index)
					throw new ArgumentOutOfRangeException (String.Format ("Index is out of range: {0}", index), "index");
				BaseRemoveAt (index);
				BaseAdd (index, value);
			}
		}

		public virtual ConfigurationElementType this [object key] {
			get {
				return (ConfigurationElementType) BaseGet (key);
			}
			set {
				if (!GetElementKey(value).Equals (key))
					throw new ArgumentException (String.Format ("The key '{0}' does not match the element key '{1}'", key, GetElementKey(value)));
				Add (value);
			}
		}

		public override ConfigurationElementCollectionType CollectionType {
			get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
		}

		protected override string ElementName {
			get {
				return AddElementName;
			}
		}

		public void Add (ConfigurationElementType element)
		{
			BaseAdd (element);
		}

		protected override void BaseAdd (ConfigurationElement element)
		{
			BaseAdd (element, false);
		}

		public void Clear ()
		{
			BaseClear ();
		}

		public virtual bool ContainsKey (object key)
		{
			return BaseGet (key) != null;
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return (ConfigurationElement) Activator.CreateInstance (typeof (ConfigurationElementType), new object [0]);
		}

		public void CopyTo (ConfigurationElementType [] array, int start)
		{
			base.CopyTo (array, start);
		}

		public int IndexOf (ConfigurationElementType element)
		{
			return BaseIndexOf (element);
		}

		public void Remove (ConfigurationElementType element)
		{
			BaseRemove (GetElementKey (element));
		}

		public void RemoveAt (int index)
		{
			BaseRemoveAt (index);
		}

		public void RemoveAt (object key)
		{
			BaseRemove (key);
		}
	}
}
