//
// System.Web.Configuration.CustomErrorCollection
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
using System.Collections;

#if NET_2_0

namespace System.Web.Configuration {

	[ConfigurationCollection (typeof (CustomError), AddItemName = "error", CollectionType = ConfigurationElementCollectionType.BasicMap)]
	public sealed class CustomErrorCollection : ConfigurationElementCollection
	{
		static ConfigurationPropertyCollection properties;

		static CustomErrorCollection ()
		{
			properties = new ConfigurationPropertyCollection ();
		}

		public void Add (CustomError customError)
		{
			BaseAdd (customError);
		}

		public void Clear ()
		{
			BaseClear ();
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new CustomError ();
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((CustomError)element).StatusCode.ToString();
		}

		public string GetKey (int index)
		{
			return (string)BaseGetKey (index);
		}

		public CustomError Get (string statusCode)
		{
			return (CustomError)BaseGet (statusCode);
		}

		public CustomError Get (int index)
		{
			return (CustomError)BaseGet (index);
		}

		public void Remove (string statusCode)
		{
			BaseRemove (statusCode);
		}

		public void RemoveAt (int index)
		{
			BaseRemoveAt (index);
		}

		public void Set (CustomError customError)
		{
			CustomError existing = Get (customError.StatusCode.ToString());

			if (existing == null) {
				Add (customError);
			}
			else {
				int index = BaseIndexOf (existing);
				RemoveAt (index);
				BaseAdd (index, customError);
			}
		}

		public string[] AllKeys {
			get {
				string[] keys = new string[Count];
				for (int i = 0; i < Count; i ++)
					keys[i] = this[i].StatusCode.ToString();
				return keys;
			}
		}

		public override ConfigurationElementCollectionType CollectionType {
			get { return ConfigurationElementCollectionType.BasicMap; }
		}

		protected override string ElementName {
			get { return "error"; }
		}

		public CustomError this [int index] {
			get { return (CustomError)BaseGet (index); }
			set { if (BaseGet (index) != null) RemoveAt (index); BaseAdd (index, value); }
		}

		public new CustomError this [string statusCode] {
			get { return (CustomError)BaseGet (statusCode); }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

#endif
