//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Novell, Inc
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
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Mono.MonoConfig
{
	public class ConfigBlockNodeHandler : IDocumentNodeHandler, IStorageConsumer, IConfigBlockContainer
	{
		string name;
		Section requirements;
		string contents;
		Dictionary <string, ConfigBlockBlock> storage;
		
		public void ReadConfiguration (XPathNavigator nav)
		{
			name = Helpers.GetRequiredNonEmptyAttribute (nav, "name");
			
			requirements = new Section ();
			Helpers.BuildSectionTree (nav.Select ("requires/section[string-length(@name) > 0]"), requirements);

			XPathNodeIterator iter = nav.Select ("contents/text()");
			StringBuilder sb = new StringBuilder ();
			
			while (iter.MoveNext ())
				sb.Append (iter.Current.Value);
			if (sb.Length > 0)
				contents = sb.ToString ();
		}
		
		public void StoreConfiguration ()
		{
			AssertStorage ();
			
			ConfigBlockBlock block = new ConfigBlockBlock (name, requirements, contents);
			if (storage.ContainsKey (name))
				storage [name] = block; // allow for silent override
			else
				storage.Add (name, block);

			// Prepare to handle more sections
			requirements = new Section ();
			contents = null;
		}

		public void SetStorage (object storage)
		{
			this.storage = storage as Dictionary <string, ConfigBlockBlock>;
			if (this.storage == null)
				throw new ApplicationException ("Invalid storage type.");
		}

		public ConfigBlockBlock FindConfigBlock (string name)
		{
			AssertStorage ();

			if (storage.ContainsKey (name))
				return storage [name];

			return null;
		}
		
		void AssertStorage ()
		{
			if (storage == null)
				throw new ApplicationException ("No storage attached");
		}
	}
}
