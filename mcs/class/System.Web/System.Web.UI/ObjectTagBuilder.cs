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
		string className;
		
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

			this.id = attribs ["id"] as string;
			if (this.id == null || this.id.Trim () == "")
				throw new ParseException (parser.Location, "Object tag must have a valid ID.");

			scope = attribs ["scope"] as string;
			className = attribs ["class"] as string;
			attribs.Remove ("scope");
			attribs.Remove ("class");
			if (className == null || className.Trim () == "")
				throw new ParseException (parser.Location, "Object tag must have 'class' attribute.");


			if (attribs ["progid"] != null || attribs ["classid"] != null)
				throw new ParseException (parser.Location, "ClassID and ProgID are not supported.");
			
			base.Init (parser, parentBuilder, type, tagName, this.id, attribs);	
			// class, id, scope
		}

		internal string ObjectID {
			get { return id; }
		}

		internal string Scope {
			get { return scope; }
		}

		internal string ClassName {
			get { return className; }
		}
	}
}

