// System.Xml.Xsl.IXsltContextFunction
//
// Author: Tim Coleman <tim@timcoleman.com>
// (C) Copyright 2002 Tim Coleman

using System;
using System.Xml.XPath;

namespace System.Xml.Xsl
{
	public interface IXsltContextFunction
	{
		#region Properties

		XPathResultType [] ArgTypes { get; }
		int Maxargs { get; }
		int Minargs { get; }
		XPathResultType ReturnType { get; }

		#endregion

		#region Methods

		object Invoke (XsltContext xsltContext, object [] args, XPathNavigator docContext);

		#endregion
	}
}
