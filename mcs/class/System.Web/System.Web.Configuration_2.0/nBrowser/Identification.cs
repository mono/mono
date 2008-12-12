#if NET_2_0
/*
Used to determine Browser Capabilities by the Browsers UserAgent String and related
Browser supplied Headers.
Copyright (C) 2002-Present  Owen Brady (Ocean at owenbrady dot net) 
and Dean Brettle (dean at brettle dot com)

Permission is hereby granted, free of charge, to any person obtaining a copy 
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights 
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all 
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
namespace System.Web.Configuration.nBrowser
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	class Identification
	{
		private bool MatchType = true;
		private string MatchName = string.Empty;
		private string MatchGroup = string.Empty;
		//FxCop will complain that this is assigned to but never used.
		//Reason I keep it around is to see the actual regex expresion 
		//used without having to drill down deep in regex object to find it
		private string MatchPattern = string.Empty;
		private System.Text.RegularExpressions.Regex RegexPattern;

		/// <summary>
		/// Sets up Initial Identification Object, So that it is easier debuging
		/// and passing Regular expression objects around.
		/// </summary>
		/// <param name="matchType">True = Match, False = NonMatch</param>
		/// <param name="matchGroup">Two Options, capability, header</param>
		/// <param name="matchName">Header Name</param>
		/// <param name="matchPattern">Regular Expression Pattern</param>
		public Identification(bool matchType, string matchGroup, string matchName, string matchPattern)
		{
			this.MatchType = matchType;
			this.MatchGroup = matchGroup;
			this.MatchName = matchName;
			this.MatchPattern = matchPattern;
			RegexPattern = new System.Text.RegularExpressions.Regex(matchPattern);
		}
		/// <summary>
		/// Builds a Match Object from the result of the regular expression
		/// </summary>
		/// <param name="Header">Header Value which the regular expression will evaluate.</param>
		/// <returns>A Match object created from the regular expression and the passed in header.</returns>
		public System.Text.RegularExpressions.Match GetMatch(string Header)
		{
			return RegexPattern.Match(Header == null ? string.Empty : Header);
		}
		/// <summary>
		/// 
		/// </summary>
		public bool IsMatchSuccessful(System.Text.RegularExpressions.Match m)
		{
			// Return true if a "match" matched successfully or a "nonmatch" didn't match.
			return (MatchType == m.Success);
		}

		/// <summary>
		/// 
		/// </summary>
		public string Name
		{
			get
			{
				return this.MatchName;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string Group
		{
			get
			{
				return this.MatchGroup;
			}
		}
		public string Pattern
		{
			get
			{
				return MatchPattern;
			}
		}
	}
}
#endif
