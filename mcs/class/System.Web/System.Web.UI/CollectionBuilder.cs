//
// System.Web.UI.CollectionBuilder.cs
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Reflection;

namespace System.Web.UI
{
	sealed class CollectionBuilder : ControlBuilder
	{
		Type elementType;

		internal CollectionBuilder ()
		{
		}

		public override void AppendLiteralString (string s)
		{
			if (s != null && s.Trim () != "")
				throw new HttpException ("Literal content not allowed for " + ControlType);
		}

		public override Type GetChildControlType (string tagName, IDictionary attribs)
		{
			Type t = Root.GetChildControlType (tagName, attribs);
			if (elementType != null && !elementType.IsAssignableFrom (t)) 
				throw new HttpException ("Cannot add a " + t + " to " + elementType);

			return t;
		}

		public override void Init (TemplateParser parser,
					   ControlBuilder parentBuilder,
					   Type type,
					   string tagName,
					   string id,
					   IDictionary attribs)
		{
			base.Init (parser, parentBuilder, type, tagName, id, attribs);

			PropertyInfo prop = parentBuilder.ControlType.GetProperty (tagName, flagsNoCase);
			SetControlType (prop.PropertyType);

			prop = ControlType.GetProperty ("Item", flagsNoCase & ~BindingFlags.IgnoreCase);
			elementType = prop.PropertyType;
		}
	}
}

