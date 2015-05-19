//
// System.Web.Configuration.ClientTargetCollection
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

	[ConfigurationCollection (typeof (ClientTarget), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
	public sealed class ClientTargetCollection : ConfigurationElementCollection
	{
		static ConfigurationPropertyCollection properties;
		
		static ClientTargetCollection ()
		{
			properties = new ConfigurationPropertyCollection ();
		}
		
		public void Add (ClientTarget clientTarget)
		{
			BaseAdd (clientTarget);
		}

		public void Clear ()
		{
			BaseClear ();
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new ClientTarget (null, null);
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((ClientTarget)element).Alias;
		}

		public string GetKey (int index)
		{
			return (string)BaseGetKey (index);
		}

		public void Remove (string name)
		{
			BaseRemove (name);
		}

		public void Remove (ClientTarget clientTarget)
		{
			BaseRemove (clientTarget.Alias);
		}

		public void RemoveAt (int index)
		{
			BaseRemoveAt (index);
		}

		public string[] AllKeys {
			get {
				string[] keys = new string[Count];
				for (int i = 0; i < Count; i ++)
					keys[i] = this[i].Alias;
				return keys;
			}
		}

		public ClientTarget this [int index] {
			get { return (ClientTarget) BaseGet (index); }
			set { if (BaseGet (index) != null) BaseRemoveAt (index); BaseAdd (index, value); }
		}

		public new ClientTarget this [string name] {
			get { return (ClientTarget) BaseGet (name); }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}


