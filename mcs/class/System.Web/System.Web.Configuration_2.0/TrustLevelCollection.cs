//
// System.Web.Configuration.TrustLevelCollection
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

using System;
using System.Configuration;


namespace System.Web.Configuration {

	[ConfigurationCollection (typeof (TrustLevel), AddItemName = "trustLevel")]
	public sealed class TrustLevelCollection : ConfigurationElementCollection
	{
		static ConfigurationPropertyCollection properties;

		static TrustLevelCollection ()
		{
			properties = new ConfigurationPropertyCollection ();
		}

		public void Add (TrustLevel trustLevel)
		{
			BaseAdd (trustLevel);
		}

		public void Clear ()
		{
			BaseClear ();
		}

		public TrustLevel Get (int index)
		{
			return (TrustLevel) BaseGet (index);
		}

		public void Remove (TrustLevel trustLevel)
		{
			BaseRemove (trustLevel.Name);
		}

		public void RemoveAt (int index)
		{
			BaseRemoveAt (index);
		}

		public void Set (int index, TrustLevel trustLevel)
		{
			if (BaseGet (index) != null)
				BaseRemoveAt (index);
			BaseAdd (index, trustLevel);
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new TrustLevel ();
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((TrustLevel)element).Name;
		}

		protected override bool IsElementName (string elementname)
		{
			return elementname == "trustlevel";
		}

		public override ConfigurationElementCollectionType CollectionType {
			get { return ConfigurationElementCollectionType.BasicMap; }
		}

		protected override string ElementName {
			get { return "trustLevel"; }
		}

		public new TrustLevel this [string key] {
			get { return (TrustLevel) BaseGet (key); }
		}

		public TrustLevel this [int index] {
			get { return (TrustLevel) BaseGet (index); }
			set { Set (index, value); }
		}

		protected override bool ThrowOnDuplicate {
			get { return false; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

