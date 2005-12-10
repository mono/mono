//
// JScriptException.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
// (C) 2005, Novell Inc, (http://novell.com)
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
using Microsoft.Vsa;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.JScript {

	[Serializable]
	public class JScriptException : ApplicationException, IVsaError	{

		private JSError error_number;
		private string user_error = null;
		private string extra_data = "";

		public JScriptException (JSError errorNumber)
		{
			error_number = errorNumber;
		}

		internal JScriptException (JSError errorNumber, string extraData)
		{
			error_number = errorNumber;
			extra_data = extraData;
		}

		internal JScriptException (string user_err)
		{
			error_number = JSError.NoError;
			user_error = user_err;
		}

		public string SourceMoniker {
			get { throw new NotImplementedException (); }
		}


		public int StartColumn {
			get { throw new NotImplementedException (); }
		}

	
		public int Column {
			get { throw new NotImplementedException (); }
		}


		public string Description {
			get {
				if (error_number == JSError.NoError)
					return user_error;
				else if (error_number == JSError.ArrayLengthConstructIncorrect)
					return "Array length must be zero or a positive integer";
				else if (error_number == JSError.PrecisionOutOfRange)
					return "The number of fractional digits is out of range";
				else if (error_number == JSError.RegExpSyntax)
					return "Syntax error in regular expression";
				else if (error_number == JSError.TypeMismatch)
					return "Unexpected type";
				else if (error_number == JSError.BooleanExpected)
					return "Invoked Boolean.prototype.toString or Boolean.prototype.valueOf method on non-boolean";
				else if (error_number == JSError.NumberExpected)
					return "Invoked Number.prototype.toString or Number.prototype.valueOf method on non-number";
				else if (error_number == JSError.StringExpected)
					return "Invoked String.prototype.toString or String.prototype.valueOf method on non-string";
				else if (error_number == JSError.FunctionExpected)
					return "Invoked Function.prototype.toString on non-function";
				else if (error_number == JSError.AssignmentToReadOnly)
					return "Tried to assign to read only property";
				else if (error_number == JSError.NotCollection)
					return "Object is not a collection";
				else {
					Console.WriteLine ("JScriptException:get_Message: unknown error_number {0}", error_number);
					throw new NotImplementedException ();
				}
			}
		}
		

		public int EndLine {
			get { throw new NotImplementedException (); }
		}


		public int EndColumn {
			get { throw new NotImplementedException (); }
		}

	
		public int Number {
			get { return (int) error_number; }
		}


		public int ErrorNumber {
			get { return (int) error_number; }
		}


		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}


		public int Line {
			get { throw new NotImplementedException (); }
		}


		public string LineText {
			get { throw new NotImplementedException (); }
		}


		public override string Message {
			get
			{
				StringBuilder result = new StringBuilder (Description);
				if (extra_data != "") {
					result.Append (": ");
					result.Append (extra_data);
				}
				return result.ToString ();
			}
		}


		public int Severity {
			get { throw new NotImplementedException (); }
		}


		public IVsaItem SourceItem {
			get { throw new NotImplementedException (); }
		}


		public override string StackTrace {
			get { 
				return base.StackTrace;
			}
		}
	}

	
	public class NoContextException : ApplicationException 
	{}
}
		
