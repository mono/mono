//
// System.Data.SqlClient.SqlException.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc
// Copyright (C) Tim Coleman, 2002
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

using Mono.Data.Tds.Protocol;
using System;
using System.ComponentModel;
using System.Data;
using System.Runtime.Serialization;
using System.Text;

namespace System.Data.SqlClient {
	[Serializable]
	public sealed class SqlException : SystemException
	{
		#region Fields

		private SqlErrorCollection errors; 

		#endregion // Fields

		#region Constructors

		internal SqlException () 
			: base ("a SQL Exception has occurred.") 
		{
			errors = new SqlErrorCollection();
		}

		internal SqlException (byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state) 
			: base (message) 
		{
			errors = new SqlErrorCollection (theClass, lineNumber, message, number, procedure, server, source, state);
		}
		
		private SqlException(SerializationInfo si, StreamingContext sc)
		{
			errors = (SqlErrorCollection)si.GetValue("Errors", typeof(SqlErrorCollection));
		}

		#endregion // Constructors

		#region Properties

		public byte Class {
			get { return Errors [0].Class; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public SqlErrorCollection Errors {
			get { return errors; }
		}

		public int LineNumber {
			get { return Errors [0].LineNumber; }
		}
		
		public override string Message 	{
			get { 
				StringBuilder result = new StringBuilder ();
				foreach (SqlError error in Errors) {
					if (result.Length > 0)
						result.Append ('\n');
					result.Append (error.Message);
				}
				return result.ToString ();
			}
		}
		
		public int Number {
			get { return Errors [0].Number; }
		}
		
		public string Procedure {
			get { return Errors [0].Procedure; }
		}

		public string Server {
			get { return Errors [0].Server; }
		}
		
		public override string Source {
			get { return Errors [0].Source; }
		}

		public byte State {
			get { return Errors [0].State; }
		}

		#endregion // Properties

		#region Methods

		internal static SqlException FromTdsInternalException (TdsInternalException e)
		{
			return new SqlException (e.Class, e.LineNumber, e.Message, e.Number, e.Procedure, e.Server, "Mono SqlClient Data Provider", e.State);
		}

		public override void GetObjectData (SerializationInfo si, StreamingContext context) 
		{
			if (si == null)
				throw new ArgumentNullException ("si");

			si.AddValue ("Errors", errors, typeof(SqlErrorCollection));
			base.GetObjectData (si, context);
		}

		#endregion // Methods
	}
}
