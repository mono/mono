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

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;
using System.Text;
using System.Diagnostics;
using System.Globalization;

using Mono.Data.Tds.Protocol;

namespace System.Data.SqlClient
{
	[Serializable]
	public sealed class SqlException : DbException
	{
#region ReferenceSource
        internal SqlException InternalClone() {
		var ret = new SqlException ();
		foreach (SqlError e in errors)
			ret.errors.Add (e);
		return ret;
        }

        static internal SqlException CreateException(SqlErrorCollection errorCollection, string serverVersion) {
            return CreateException(errorCollection, serverVersion, Guid.Empty);
        }

        static internal SqlException CreateException(SqlErrorCollection errorCollection, string serverVersion, SqlInternalConnectionTds internalConnection, Exception innerException = null) {
            Guid connectionId = Guid.Empty;
            var exception = CreateException(errorCollection, serverVersion, connectionId, innerException);
/*
            if (internalConnection != null) { 
                if ((internalConnection.OriginalClientConnectionId != Guid.Empty) && (internalConnection.OriginalClientConnectionId != internalConnection.ClientConnectionId)) {
                    exception.Data.Add(OriginalClientConnectionIdKey, internalConnection.OriginalClientConnectionId);
                }

                if (!string.IsNullOrEmpty(internalConnection.RoutingDestination)) {
                    exception.Data.Add(RoutingDestinationKey, internalConnection.RoutingDestination);
                }
            }
*/
            return exception;
        }


        static internal SqlException CreateException(SqlErrorCollection errorCollection, string serverVersion, Guid conId, Exception innerException = null) {
            Debug.Assert(null != errorCollection && errorCollection.Count > 0, "no errorCollection?");
            
            // concat all messages together MDAC 65533
            StringBuilder message = new StringBuilder();
            for (int i = 0; i < errorCollection.Count; i++) {
                if (i > 0) {
                    message.Append(Environment.NewLine);
                }
                message.Append(errorCollection[i].Message);
            }

            if (innerException == null && errorCollection[0].Win32ErrorCode != 0 && errorCollection[0].Win32ErrorCode != -1) {
                innerException = new Win32Exception(errorCollection[0].Win32ErrorCode);
            }

            SqlException exception = new SqlException(message.ToString(), /*errorCollection, */innerException/*, conId*/);

            exception.Data.Add("HelpLink.ProdName",    "Microsoft SQL Server");

            if (!ADP.IsEmpty(serverVersion)) {
                exception.Data.Add("HelpLink.ProdVer", serverVersion);
            }
            exception.Data.Add("HelpLink.EvtSrc",      "MSSQLServer");
            exception.Data.Add("HelpLink.EvtID",       errorCollection[0].Number.ToString(CultureInfo.InvariantCulture));
            exception.Data.Add("HelpLink.BaseHelpUrl", "http://go.microsoft.com/fwlink");
            exception.Data.Add("HelpLink.LinkId",      "20476");

            return exception;
        }

        internal bool _doNotReconnect = false;
#endregion

		#region Fields

		private readonly SqlErrorCollection errors;
		private const string DEF_MESSAGE = "SQL Exception has occured.";

		#endregion // Fields

		#region Constructors

		internal SqlException ()
			: this (DEF_MESSAGE, null, null)
		{
		}
		
		internal SqlException (string message, Exception inner)
			: this (message, inner, null)
		{
		}

		internal SqlException (string message, Exception inner, SqlError sqlError)
			: base (message == null ? DEF_MESSAGE : message, inner)
		{
			HResult = -2146232060;
			errors = new SqlErrorCollection ();
			if (sqlError != null)
				errors.Add (sqlError);
		}

		internal SqlException (byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state) 
			: this (null, 
				null, 
				new SqlError (number, state, theClass, server, message, procedure, lineNumber, 0))
		{
		}
		
		private SqlException(SerializationInfo si, StreamingContext sc)
		{
			HResult = -2146232060;
			errors = (SqlErrorCollection) si.GetValue ("Errors", typeof (SqlErrorCollection));
		}

		#endregion // Constructors

		#region Properties

		public byte Class {
			get { return Errors [0].Class; }
		}

		[MonoTODO]
		public Guid ClientConnectionId {
			get { throw new NotImplementedException (); }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public SqlErrorCollection Errors {
			get { return errors; }
		}

		public int LineNumber {
			get { return Errors [0].LineNumber; }
		}
		
		public override string Message {
			get {
				if (Errors.Count == 0)
					return base.Message;
				StringBuilder result = new StringBuilder ();
				if (base.Message != DEF_MESSAGE) {
					result.Append (base.Message);
					result.Append ("\n");
				}
				for (int i = 0; i < Errors.Count -1; i++) {
					result.Append (Errors [i].Message);
					result.Append ("\n");
				}
				result.Append (Errors [Errors.Count - 1].Message);
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
			return new SqlException (e.Class, e.LineNumber, e.Message,
							  e.Number, e.Procedure, e.Server,
							  "Mono SqlClient Data Provider", e.State);
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
