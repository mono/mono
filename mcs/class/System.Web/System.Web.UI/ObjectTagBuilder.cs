//
// System.Web.UI.ObjectTagBuilder
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc. (http://www.ximian.com)


using System;
using System.Collections;

namespace System.Web.UI
{
	public sealed class ObjectTagBuilder : ControlBuilder
	{
		public ObjectTagBuilder ()
		{
		}

		public override void AppendLiteralString (string s) 
		{
			// Empty
		}

		public override void AppendSubBuilder (ControlBuilder subBuilder) 
		{
			// Empty
		}
 
 		[MonoTODO]
		public override void Init (TemplateParser parser,
					   ControlBuilder parentBuilder,
					   Type type,
					   string tagName,
					   string id,
					   IDictionary attribs) 
		{
		}
	}
}

