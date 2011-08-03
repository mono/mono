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

namespace System.Web
{
	sealed class DefaultExceptionPageTemplate : ExceptionPageTemplate
	{
		public override void Init ()
		{
			List <ExceptionPageTemplateFragment> fragments = Fragments;
			
			fragments.Add (new ExceptionPageTemplateFragment {
					Name = Template_PageTopName,
					ResourceName = "ErrorTemplateCommon_Top.html",
					MacroNames = new List <string> {
						"Title",
						"ExceptionType",
						"ExceptionMessage",
						"Description",
						"Details"
					}
				}
			);

			fragments.Add (new ExceptionPageTemplateFragment {
					Name = Template_PageCustomErrorDefaultName,
					ResourceName = "DefaultErrorTemplate_CustomErrorDefault.html",
					ValidForPageType = ExceptionPageTemplateType.CustomErrorDefault
				}
			);

			fragments.Add (new ExceptionPageTemplateFragment {
					Name = Template_PageStandardName,
					ResourceName = "DefaultErrorTemplate_StandardPage.html",
					ValidForPageType = ExceptionPageTemplateType.Standard,
					MacroNames = new List <string> {
						"StackTrace"
					}
				}
			);

			fragments.Add (new ExceptionPageTemplateFragment {
					Name = Template_PageHtmlizedExceptionName,
					ResourceName = "HtmlizedExceptionPage_Top.html",
					ValidForPageType = ExceptionPageTemplateType.Htmlized,
					MacroNames = new List <string> {
						"StackTrace",
						"HtmlizedExceptionOrigin",
						"HtmlizedExceptionSourceFile"
					}
				}
			);

			fragments.Add (new ExceptionPageTemplateFragment {
					Name = "File Short Source",
					ResourceName = "HtmlizedExceptionPage_FileShortSource.html",
					ValidForPageType = ExceptionPageTemplateType.SourceError,
					MacroNames = new List <string> {
						"HtmlizedExceptionShortSource",
						"HtmlizedExceptionSourceFile",
						"HtmlizedExceptionErrorLines"
					}
				}
			);

			fragments.Add (new ExceptionPageTemplateFragment {
					Name = "File Long Source",
					ResourceName = "HtmlizedExceptionPage_FileLongSource.html",
					ValidForPageType = ExceptionPageTemplateType.SourceError,
					MacroNames = new List <string> {
						"HtmlizedExceptionLongSource",
						"HtmlizedExceptionSourceFile",
						"HtmlizedExceptionErrorLines"
					},
					RequiredMacros = new List <string> {
						"HtmlizedExceptionLongSource"
					}
				}
			);

			fragments.Add (new ExceptionPageTemplateFragment {
					Name = "Compiler Output",
					ResourceName = "HtmlizedExceptionPage_CompilerOutput.html",
					ValidForPageType = ExceptionPageTemplateType.SourceError,
					MacroNames = new List <string> {
						"HtmlizedExceptionCompilerOutput",
						"HtmlizedExceptionSourceFile",
						"HtmlizedExceptionErrorLines"
					},
					RequiredMacros = new List <string> {
						"HtmlizedExceptionCompilerOutput"
					}
				}
			);
			
			fragments.Add (new ExceptionPageTemplateFragment {
					Name = Template_PageBottomName,
					ResourceName = "ErrorTemplateCommon_Bottom.html",
					MacroNames = new List <string> {
						"RuntimeVersionInformation",
						"AspNetVersionInformation",
						"FullStackTrace"
					}
				}
			);
		}
	}
}
