//
// System.Web.UI.ObjectTagBuilder
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc. (http://www.ximian.com)


using System;
using System.Collections;
using System.Web.Compilation;

namespace System.Web.UI
{
	public sealed class ObjectTagBuilder : ControlBuilder
	{
		string id;
		string scope;
		Type type;
		
		public ObjectTagBuilder ()
		{
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

		public override bool HasBody ()
		{
			return false;
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

