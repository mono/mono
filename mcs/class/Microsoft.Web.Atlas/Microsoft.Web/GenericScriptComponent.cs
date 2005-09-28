//
// Microsoft.Web.GenericScriptComponent
//
// Author:
//   Chris Toshok (toshok@ximian.com)
//
//
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

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using Microsoft.Web.UI;

namespace Microsoft.Web
{

	public sealed class GenericScriptComponent : ScriptComponent
	{
		public GenericScriptComponent (string tagName)
		{
			this.tagName = tagName;

			valueProperties = new Hashtable ();
			collections = new Hashtable ();
		}

		protected override void AddAttributesToElement (ScriptTextWriter writer)
		{
			base.AddAttributesToElement (writer);

			foreach (string prop in valueProperties.Keys)
				writer.WriteAttributeString (prop, (string)valueProperties[prop]);
		}

		public void AddCollectionItem (string collectionName, IScriptComponent item)
		{
			ScriptComponentCollection col = (ScriptComponentCollection)collections[collectionName];
			if (col == null) {
				col = new ScriptComponentCollection ();
				collections [collectionName] = col;
			}

			col.Add (item);
		}

		public void AddComponentProperty (string propertyName, IScriptComponent component)
		{
		}

		public void AddValueProperty (string propertyName, string propertyValue)
		{
			valueProperties [propertyName] = propertyValue;
		}

		protected override void RenderScriptTagContents (ScriptTextWriter writer)
		{
			base.RenderScriptTagContents (writer);

			foreach (string colName in collections.Keys) {
				ScriptComponentCollection col = (ScriptComponentCollection)collections[colName];
				writer.WriteStartElement (colName);
				foreach (IScriptComponent c in col)
					c.RenderScript (writer);
				writer.WriteEndElement (); // colName
			}
		}

		ScriptEventCollection scriptEvents;
		public new ScriptEventCollection ScriptEvents {
			get {
				return base.ScriptEvents;
			}
		}

		public override string TagName {
			get {
				return tagName;
			}
		}

		Hashtable valueProperties;
		Hashtable collections;
		string tagName;
	}
}

#endif
