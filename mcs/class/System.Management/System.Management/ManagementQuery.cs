//
// System.Management.ManagementQuery
//
// Authors:
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2006 Gert Driesen
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
using System.ComponentModel;

namespace System.Management
{
	[MonoTODO ("System.Management is not implemented")]
	//[TypeConverter ("")]
	public abstract class ManagementQuery : ICloneable
	{
		string sQueryLanguage;
		string sQueryString;

		internal ManagementQuery ()
		{
		}

		internal ManagementQuery (string query)
		{
			QueryString = query;
		}

		internal ManagementQuery (string language, string query)
		{
			QueryLanguage = language;
			QueryString = query;
		}

		static ManagementQuery ()
		{
		}

		public virtual string QueryLanguage {
			get {
				return sQueryLanguage;
			}
			set {
				sQueryLanguage = value;
			}
		}

		public virtual string QueryString {
			get {
				return sQueryString;
			}
			set {
				sQueryString = value;
			}
		}

		public abstract object Clone ();

		[MonoTODO]
		protected internal virtual void ParseQuery (string query)
		{
			throw new NotImplementedException ();
		}
	}
}
