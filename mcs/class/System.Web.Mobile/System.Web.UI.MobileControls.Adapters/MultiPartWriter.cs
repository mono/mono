/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls.Adapters
 * Class     : MultiPartWriter
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System;
using System.Web.Mobile;
using System.Web.UI;
using System.IO;

namespace System.Web.UI.MobileControls.Adapters
{
	public class MultiPartWriter : HtmlTextWriter
	{
		public MultiPartWriter(TextWriter writer) : base(writer)
		{
			throw new NotImplementedException();
		}
	}
}
