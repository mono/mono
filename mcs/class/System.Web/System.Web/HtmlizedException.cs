//
// System.Web.HtmlizedException
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.IO;
using System.Text;

namespace System.Web
{
	internal abstract class HtmlizedException : Exception
	{
		protected HtmlizedException ()
		{
		}

		protected HtmlizedException (string message)
			: base (message)
		{
		}

		protected HtmlizedException (string message, Exception inner)
			: base (message, inner)
		{
		}

		public abstract string Title { get; }
		public abstract string Description { get; }
		public abstract string ErrorMessage { get; }
		public abstract string FileName { get; }
	}
}

