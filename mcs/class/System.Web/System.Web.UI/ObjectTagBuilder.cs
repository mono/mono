//
// System.Web.UI.ObjectTagBuilder
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc. (http://www.ximian.com)
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

using System.Collections;
using System.Security.Permissions;
using System.Web.Compilation;

namespace System.Web.UI
{
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class ObjectTagBuilder : ControlBuilder
	{
		string id;
		string scope;
		Type type;
		
		public ObjectTagBuilder ()
		{
			SetTagName ("object");
		}

		public override void AppendLiteralString (string s) 
		{
			// Do nothing
		}

		public override void AppendSubBuilder (ControlBuilder subBuilder) 
		{
			// Do nothing
		}
 
		public override void Init (TemplateParser parser,
					   ControlBuilder parentBuilder,
					   Type type,
					   string tagName,
					   string id,
					   IDictionary attribs) 
		{
			if (id == null && attribs == null)
				throw new HttpException ("Missing 'id'.");
			if (attribs == null)
				throw new ParseException (parser.Location, "Error in ObjectTag.");

			attribs.Remove ("runat");
			this.id = attribs ["id"] as string;
			attribs.Remove ("id");
			if (this.id == null || this.id.Trim () == "")
				throw new ParseException (parser.Location, "Object tag must have a valid ID.");

			scope = attribs ["scope"] as string;
			string className = attribs ["class"] as string;
			attribs.Remove ("scope");
			attribs.Remove ("class");
			if (className == null || className.Trim () == "")
				throw new ParseException (parser.Location, "Object tag must have 'class' attribute.");

			this.type = parser.LoadType (className);
			if (this.type == null)
				throw new ParseException (parser.Location, "Type " + className + " not found.");

			if (attribs ["progid"] != null || attribs ["classid"] != null)
				throw new ParseException (parser.Location, "ClassID and ProgID are not supported.");

			if (attribs.Count > 0)
				throw new ParseException (parser.Location, "Unknown attribute");
		}

		internal Type Type {
			get { return type; }
		}

		internal string ObjectID {
			get { return id; }
		}

		internal string Scope {
			get { return scope; }
		}
	}
}

