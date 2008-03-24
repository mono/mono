//
// System.Data.Odbc.OdbcException
//
// Author:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2002
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace System.Data.Odbc
{
	[Serializable]
#if NET_2_0
	public sealed class OdbcException : DbException
#else
	public sealed class OdbcException : SystemException
#endif
	{
		OdbcErrorCollection odbcErrors;

		internal OdbcException (OdbcErrorCollection errors)
			: base (CreateMessage (errors))
		{
			odbcErrors = errors;
		}

		public OdbcErrorCollection Errors {
			get {
				return odbcErrors;
			}
		}

		public override string Source {
			get {
				return odbcErrors [0].Source;
			}
		}

#if !NET_2_0
		public override string Message {
			get {
				return base.Message;
			}
		}
#endif

		private OdbcException (SerializationInfo si, StreamingContext sc) : base(si, sc)
		{
			odbcErrors = new OdbcErrorCollection ();
			odbcErrors = ((OdbcErrorCollection) si.GetValue (
				"odbcErrors", typeof (OdbcErrorCollection)));
		}

		#region Methods

		public override void GetObjectData (SerializationInfo si, StreamingContext context)
		{
			if (si == null)
				throw new ArgumentNullException ("si");

			si.AddValue ("odbcErrors", odbcErrors, typeof(OdbcErrorCollection));
			base.GetObjectData (si, context);
		}

		static string CreateMessage (OdbcErrorCollection errors)
		{
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < errors.Count; i++) {
				if (i > 0)
					sb.Append (Environment.NewLine);
				OdbcError error = errors [i];
				sb.AppendFormat ("ERROR [{0}] {1}", error.SQLState,
					error.Message);
			}
			return sb.ToString ();
		}

		#endregion // Methods
	}
}
