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
	class DefaultNode
	{
		string contents;
		FeatureTarget target;

		public string Contents {
			get { return contents; }
		}

		public FeatureTarget Target {
			get { return target; }
		}

		public DefaultNode (string contents, FeatureTarget target)
		{
			this.contents = contents;
			this.target = target;
		}
	}
	
	public class DefaultNodeHandler : IDocumentNodeHandler, IDefaultContainer, IStorageConsumer
	{
		string section;
		string contents;
		FeatureTarget target;
		
		Dictionary <string, DefaultNode> storage;
		
		public void ReadConfiguration (XPathNavigator nav)
		{
			section = Helpers.GetRequiredNonEmptyAttribute (nav, "section");
			target = Helpers.ConvertEnum <FeatureTarget>  (Helpers.GetRequiredNonEmptyAttribute (nav, "target"), "target");
			
			XPathNodeIterator iter = nav.Select ("./text()");
			StringBuilder sb = new StringBuilder ();

			while (iter.MoveNext ())
				sb.Append (iter.Current.Value);
			if (sb.Length > 0)
				contents = sb.ToString ();
		}
		
		public void StoreConfiguration ()
		{
			AssertStorage ();

			DefaultNode dn = new DefaultNode (contents, target);
			
			if (storage.ContainsKey (section))
				storage [section] = dn;
			else
				storage.Add (section, dn);

			section = null;
			contents = null;
			storage = null;
		}

		public string FindDefault (string sectionName, FeatureTarget target)
		{
			AssertStorage ();

			if (storage.ContainsKey (sectionName)) {
				DefaultNode dn = storage [sectionName];
				
				if (target == FeatureTarget.Any || dn.Target == FeatureTarget.Any || dn.Target == target)
					return dn.Contents;
			}
			
			return null;
		}

		public void SetStorage (object storage)
		{
			this.storage = storage as Dictionary <string, DefaultNode>;
			if (this.storage == null)
				throw new ApplicationException ("Invalid storage type");
		}
		
		void AssertStorage ()
		{
			if (storage == null)
				throw new ApplicationException ("No storage attached");
		}
	}
}
