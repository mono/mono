//
// System.Web.UI.DataBindingBuilder
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Web.Compilation;

namespace System.Web.UI
{
	sealed class DataBindingBuilder : CodeBuilder
	{
		public DataBindingBuilder (string code, ILocation location)
			: base (code, false, location)
		{
			SetControlType (typeof (DataBoundLiteralControl));
		}
	}
}

