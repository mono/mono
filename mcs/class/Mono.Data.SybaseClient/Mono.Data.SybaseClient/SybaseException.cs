//
// Mono.Data.SybaseClient.SybaseException.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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

using Mono.Data.Tds.Protocol;
using System;
using System.Data;
using System.Runtime.Serialization;
using System.Text;

namespace Mono.Data.SybaseClient {
	[Serializable]
	public sealed class SybaseException : SystemException
	{
		#region Fields

		SybaseErrorCollection errors; 

		#endregion Fields

		#region Constructors

		internal SybaseException () 
			: base ("a SQL Exception has occurred.") 
		{
			errors = new SybaseErrorCollection();
		}

		internal SybaseException (byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state) 
			: base (message) 
		{
			errors = new SybaseErrorCollection (theClass, lineNumber, message, number, procedure, server, source, state);
		}

		#endregion // Constructors

		#region Properties

		public byte Class {
			get { return errors [0].Class; }
		}

		public SybaseErrorCollection Errors {
			get { return errors; }
		}

		public int LineNumber {
			get { return errors [0].LineNumber; }
		}
		
		public override string Message 	{
			get {
				StringBuilder result = new StringBuilder ();
				foreach (SybaseError error in Errors) {
					if (result.Length > 0)
						result.Append ('\n');
					result.Append (error.Message);
				}
				return result.ToString ();
			}                                                                
		}
		
		public int Number {
			get { return errors [0].Number; }
		}
		
		public string Procedure {
			get { return errors [0].Procedure; }
		}

		public string Server {
			get { return errors [0].Server; }
		}
		
		public override string Source {
			get { return errors [0].Source; }
		}

		public byte State {
			get { return errors [0].State; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override void GetObjectData (SerializationInfo si, StreamingContext context) 
		{
			throw new NotImplementedException ();
		}

		internal static SybaseException FromTdsInternalException (TdsInternalException e)
		{
			return new SybaseException (e.Class, e.LineNumber, e.Message, e.Number, e.Procedure, e.Server, "Mono SybaseClient Data Provider", e.State);
		}

		#endregion // Methods
	}
}
