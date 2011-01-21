//
// Authors:
//   Marek Habersack <grendel@twistedcode.net>
//
// (C) 2011 Novell, Inc (http://novell.com/)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Text;

namespace System.Web
{
	abstract class ExceptionPageTemplate
	{
		public const string Template_PageTopName = "PageTop";
		public const string Template_PageBottomName = "PageBottom";
		public const string Template_PageStandardName = "PageStandard";
		public const string Template_PageCustomErrorDefaultName = "PageCustomErrorDefault";
		public const string Template_PageHtmlizedExceptionName = "PageHtmlizedException";
		public const string Template_PageTitleName = "Title";
		public const string Template_ExceptionTypeName = "ExceptionType";
		public const string Template_ExceptionMessageName = "ExceptionMessage";
		public const string Template_DescriptionName = "Description";
		public const string Template_DetailsName = "Details";
		public const string Template_RuntimeVersionInformationName = "RuntimeVersionInformation";
		public const string Template_AspNetVersionInformationName = "AspNetVersionInformation";
		public const string Template_StackTraceName = "StackTrace";
		public const string Template_FullStackTraceName = "FullStackTrace";
		public const string Template_HtmlizedExceptionOriginName = "HtmlizedExceptionOrigin";
		public const string Template_HtmlizedExceptionShortSourceName = "HtmlizedExceptionShortSource";
		public const string Template_HtmlizedExceptionLongSourceName = "HtmlizedExceptionLongSource";
		public const string Template_HtmlizedExceptionSourceFileName = "HtmlizedExceptionSourceFile";
		public const string Template_HtmlizedExceptionErrorLinesName = "HtmlizedExceptionErrorLines";
		public const string Template_HtmlizedExceptionCompilerOutputName = "HtmlizedExceptionCompilerOutput";
		
		List <ExceptionPageTemplateFragment> fragments;

		public List <ExceptionPageTemplateFragment> Fragments {
			get {
				if (fragments == null)
					fragments = new List <ExceptionPageTemplateFragment> ();
				return fragments;
			}
		}

		public abstract void Init ();
		
		void InitFragments (ExceptionPageTemplateValues values)
		{
			foreach (ExceptionPageTemplateFragment fragment in fragments) {
				if (fragment == null)
					continue;

				fragment.Init (values);
			}
		}
		
		public string Render (ExceptionPageTemplateValues values, ExceptionPageTemplateType pageType)
		{
			if (values == null)
				throw new ArgumentNullException ("values");
			var sb = new StringBuilder ();

			Render (values, pageType, (string text) => {
				sb.Append (text);
			});
			
			return sb.ToString ();
		}

		public void Render (HttpResponse response, ExceptionPageTemplateValues values, ExceptionPageTemplateType pageType)
		{
			if (response == null)
				return;

			if (values == null)
				throw new ArgumentNullException ("values");
			
			Render (values, pageType, (string text) => {
				response.Write (text);
			});
		}

		void Render (ExceptionPageTemplateValues values, ExceptionPageTemplateType pageType, Action <string> writer)
		{
			if (fragments == null || fragments.Count == 0 || values.Count == 0)
				return;

			InitFragments (values);
			string value;
			foreach (ExceptionPageTemplateFragment fragment in fragments) {
				if (fragment == null || (fragment.ValidForPageType & pageType) == 0)
					continue;

				value = values.Get (fragment.Name);
				if (value == null || !fragment.Visible (values))
					continue;

				writer (fragment.ReplaceMacros (value, values));
			}
		}
	}
}
