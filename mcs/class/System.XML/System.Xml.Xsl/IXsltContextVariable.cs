// System.Xml.Xsl.IXsltContextVariable
//
// Author: Tim Coleman <tim@timcoleman.com>
// (C) Copyright 2002 Tim Coleman

using System;
using System.Xml.XPath;

namespace System.Xml.Xsl
{
	public interface IXsltContextVariable
	{
		#region Properties

		bool IsLocal { get; }
		bool IsParam { get; }
		XPathResultType VariableType { get; }

		#endregion

		#region Methods

		object Evaluate (XsltContext xsltContext);

		#endregion
	}
}
