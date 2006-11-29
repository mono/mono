//
// System.Web.HttpVaryByHeaders.cs 
//
// Author:
//	Chris Toshok (toshok@novell.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Security.Permissions;

namespace System.Web {

	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class HttpCacheVaryByHeaders {

		/* I would have much rather seen this class just use the
		 * Hashtable, and have the getter/setters for the builtin
		 * fields just manipulate that Hashtable, but that doesn't
		 * appear to work in MS's implementation.  If you do:
		 *
		 *	   vary_by_hdrs["User-Agent"] = true;
		 *
		 * then
		 *
		 *	   (vary_by_hdrs.UserAgent == true)
		 *
		 * will be false, which is completely counterintuitive
		 * and broken. The same holds true in reverse:
		 *
		 *	   vary_by_hdrs.UserAgent = true;
		 *
		 * does not mean
		 *
		 *	   vary_by_hdrs["User-Agent"] == true.
		 */
		bool vary_by_unspecified;

		bool vary_by_accept;
		bool vary_by_user_agent;
		bool vary_by_user_charset;
		bool vary_by_user_language;

		Hashtable fields;

		internal HttpCacheVaryByHeaders ()
		{
			/* the field names are meant to be case insensitive */
#if NET_2_0
			fields = new Hashtable (StringComparer.InvariantCultureIgnoreCase);
#else
			fields = new Hashtable(CaseInsensitiveHashCodeProvider.Default,
					       CaseInsensitiveComparer.Default);
#endif
		}

		internal string[] GetHeaderNames (bool omitVaryStar)
		{
			string[] names;

			if (vary_by_unspecified && !omitVaryStar) {
				names = new string[1];
				names[0] = "*";
			}
			else {
				int builtin_count = ((vary_by_accept ? 1 : 0)
						     + (vary_by_user_agent ? 1 : 0)
						     + (vary_by_user_charset ? 1 : 0)
						     + (vary_by_user_language ? 1 : 0));

				names = new string [fields.Count + builtin_count];

				int i = 0;
				if (vary_by_accept) names[i++] = "Accept";
				if (vary_by_user_agent) names[i++] = "User-Agent";
				if (vary_by_user_charset) names[i++] = "Accept-Charset";
				if (vary_by_user_language) names[i++] = "Accept-Language";

				fields.Keys.CopyTo (names, builtin_count);
			}

			return names;
		}

		public bool AcceptTypes {
			get {
				return vary_by_accept;
			}
			set {
				vary_by_unspecified = false;
				vary_by_accept = value;
			}
		}

		public bool UserAgent {
			get {
				return vary_by_user_agent;
			}
			set {
				vary_by_unspecified = false;
				vary_by_user_agent = value;
			}
		}

		public bool UserCharSet {
			get {
				return vary_by_user_charset;
			}
			set {
				vary_by_unspecified = false;
				vary_by_user_charset = value;
			}
		}

		public bool UserLanguage {
			get {
				return vary_by_user_language;
			}
			set {
				vary_by_unspecified = false;
				vary_by_user_language = value;
			}
		}

		public bool this [ string header ] {
			get {
				if (header == null)
					throw new ArgumentNullException ();

				return fields.Contains (header);
			}
			set {
				if (header == null)
					throw new ArgumentNullException ();

				vary_by_unspecified = false;
				if (value)
					if (!fields.Contains (header))
						fields.Add (header, true);
				else
					fields.Remove (header);
			}
		}

		public void VaryByUnspecifiedParameters ()
		{
			fields.Clear();

			vary_by_unspecified =
			  vary_by_accept = 
			  vary_by_user_agent =
			  vary_by_user_charset =
			  vary_by_user_language = false;

			vary_by_unspecified = true;
		}
	}

}
