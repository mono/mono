//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2010 Novell, Inc http://novell.com/
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
using System.Web;
using System.Web.Hosting;

using MonoTests.stand_alone.WebHarness;

namespace StandAloneRunnerSupport
{
	public sealed class Helpers
	{
		public const string BEGIN_CODE_MARKER = "<!-- @CODE_BEGIN@ -->";
		public const string END_CODE_MARKER = "<!-- @CODE_END@ -->";
		
		public static string ExtractCodeFromHtml (string html)
		{
			AppDomain ad = AppDomain.CurrentDomain;
			return HtmlDiff.GetControlFromPageHtml (html, BEGIN_CODE_MARKER, END_CODE_MARKER);
		}

		public static void ExtractAndCompareCodeFromHtml (string html, string original, string msg)
		{
			string rendered = ExtractCodeFromHtml (html);
			HtmlDiff.AssertAreEqual (original, rendered, msg);
		}

		public static string StripWebResourceAxdQuery (string origHtml)
		{
			return StripWebResourceAxdQuery (StripWebResourceAxdQuery (origHtml, "\""), "&quot;");
		}

		static string StripWebResourceAxdQuery (string origHtml, string delimiter)
		{
			if (String.IsNullOrEmpty (origHtml))
				return origHtml;
			
			// Naive approach, enough for now
			int idx = origHtml.IndexOf (delimiter + "/WebResource.axd");
			if (idx == -1)
				return origHtml;

			var sb = new StringBuilder ();
			sb.Append (origHtml.Substring (0, idx));
			sb.Append (delimiter);
			idx++;
			int idx2 = origHtml.IndexOf (delimiter, idx);
			string webRes;
			sb.Append ("/WebResource.axd");
			
			if (idx2 > -1)
				sb.Append (origHtml.Substring (idx2));

			return sb.ToString ();
		}

		public static bool HasException (string html, Type exceptionType)
		{
			if (exceptionType == null)
				throw new ArgumentNullException ("exceptionType");

			return HasException (html, exceptionType.FullName);
		}
		
		public static bool HasException (string html, string exceptionType)
		{
			if (String.IsNullOrEmpty (exceptionType))
				throw new ArgumentNullException ("exceptionType");
			
			if (String.IsNullOrEmpty (html))
				return false;
			
			return html.IndexOf ("[" + exceptionType + "]:") != -1;
		}
	}
}
