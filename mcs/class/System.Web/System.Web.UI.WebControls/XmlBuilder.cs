//
// System.Web.UI.WebControls.XmlBuilder.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
//

using System;
using System.Collections;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	internal class XmlBuilder : ControlBuilder
	{
		public override void AppendLiteralString (string s)
		{	
		}

		public override Type GetChildControlType (string tagName, IDictionary attribs)
		{
			return null;
		}

		public override bool NeedsTagInnerText ()
		{
			return true;
		}

		[MonoTODO ("find out what this does and implement")]
		public override void SetTagInnerText (string text)
		{
			throw new NotImplementedException ();
		}
	}
}
