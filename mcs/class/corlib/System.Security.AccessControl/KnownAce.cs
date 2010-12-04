//
// System.Security.AccessControl.KnownAce implementation
//
// Authors:
//	Dick Porter  <dick@ximian.com>
//	Atsushi Enomoto  <atsushi@ximian.com>
//	Kenneth Bell
//
// Copyright (C) 2006-2007 Novell, Inc (http://www.novell.com)
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

using System.Globalization;
using System.Security.Principal;
using System.Text;

namespace System.Security.AccessControl
{
	public abstract class KnownAce : GenericAce
	{
		private int access_mask;
		private SecurityIdentifier identifier;

		internal KnownAce (AceType type, AceFlags flags)
			: base (type, flags)
		{
		}
		
		internal KnownAce (byte[] binaryForm, int offset)
			: base (binaryForm, offset)
		{
		}

		public int AccessMask {
			get { return access_mask; }
			set { access_mask = value; }
		}
		
		public SecurityIdentifier SecurityIdentifier {
			get { return identifier; }
			set { identifier = value; }
		}
		
		internal static string GetSddlAccessRights (int accessMask)
		{
			string ret = GetSddlAliasRights(accessMask);
			if (!string.IsNullOrEmpty(ret))
				return ret;
			
			return string.Format (CultureInfo.InvariantCulture,
			                      "0x{0:x}", accessMask);
		}
		
		private static string GetSddlAliasRights(int accessMask)
		{
			SddlAccessRight[] rights = SddlAccessRight.Decompose(accessMask);
			if (rights == null)
				return null;
			
			StringBuilder ret = new StringBuilder();
			foreach (var right in rights) {
				ret.Append(right.Name);
			}
			
			return ret.ToString();
		}
	}
}

