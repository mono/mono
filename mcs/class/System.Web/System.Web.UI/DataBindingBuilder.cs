//
// System.Web.UI.DataBindingBuilder
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Reflection;

namespace System.Web.UI
{
	sealed class DataBindingBuilder : CodeBuilder
	{
		public DataBindingBuilder (string code, bool isAssign, string fileName, int line)
			: base (code, isAssign, fileName, line)
		{
		}
	}
}

