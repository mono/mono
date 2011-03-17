//
// System.Web.Configuration.CompilerProviderOptionsCollection.cs
//
// Authors:
//	Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Novell, Inc (http://www.novell.com)
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

#if CONFIGURATION_DEP
using System;
using System.Configuration;
using System.Collections.Generic;

namespace System.CodeDom.Compiler
{
	[ConfigurationCollection (typeof (CompilerProviderOption), CollectionType = ConfigurationElementCollectionType.BasicMap, AddItemName = "providerOption")]
	internal sealed class CompilerProviderOptionsCollection : ConfigurationElementCollection
	{
		static ConfigurationPropertyCollection properties;

		static CompilerProviderOptionsCollection ()
		{
			properties = new ConfigurationPropertyCollection ();
		}

		public CompilerProviderOptionsCollection ()
		{
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new CompilerProviderOption ();
		}

		public CompilerProviderOption Get (int index)
		{
			return (CompilerProviderOption) BaseGet (index);
		}

		public CompilerProviderOption Get (string name)
		{
			return (CompilerProviderOption) BaseGet (name);
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((CompilerProviderOption) element).Name;
		}

		public string GetKey (int index)
		{
			return (string) BaseGetKey (index);
		}

		public string[] AllKeys {
			get {
				int count = Count;
				string[] keys = new string [count];
				for (int i = 0; i < count; i++)
					keys [i] = this [i].Name;

				return keys;
			}
		}

		protected override string ElementName {
			get { return "providerOption"; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		public Dictionary <string, string> ProviderOptions {
			get {
				int count = Count;

				if (count == 0)
					return null;

				Dictionary <string, string> ret = new Dictionary <string, string> (count);
				CompilerProviderOption opt;
				
				for (int i = 0; i < count; i++) {
					opt = this [i];
					ret.Add (opt.Name, opt.Value);
				}

				return ret;
			}
		}
		
		public CompilerProviderOption this [int index] {
			get { return (CompilerProviderOption) BaseGet (index); }
		}

		public new CompilerProviderOption this [string name] {
			get {
				foreach (CompilerProviderOption c in this) {
					if (c.Name == name)
						return c;
				}

				return null;
			}
		}
	}
}
#endif