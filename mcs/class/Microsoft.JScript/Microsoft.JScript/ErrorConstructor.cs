//
// ErrorConstructor.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
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
using System.Reflection;

namespace Microsoft.JScript {

	public enum ErrorType : int {
		OtherError,
		EvalError,
		RangeError,
		ReferenceError,
		SyntaxError,
		TypeError,
		URIError
	};

	public class ErrorConstructor : ScriptFunction {

		ErrorType error_type;
		internal static ErrorConstructor Ctr = new ErrorConstructor ();
		internal static ErrorConstructor EvalErrorCtr = new ErrorConstructor (ErrorType.EvalError);
		internal static ErrorConstructor RangeErrorCtr = new ErrorConstructor (ErrorType.RangeError);
		internal static ErrorConstructor ReferenceErrorCtr = new ErrorConstructor (ErrorType.ReferenceError);
		internal static ErrorConstructor SyntaxErrorCtr = new ErrorConstructor (ErrorType.SyntaxError);
		internal static ErrorConstructor TypeErrorCtr = new ErrorConstructor (ErrorType.TypeError);
		internal static ErrorConstructor URIErrorCtr = new ErrorConstructor (ErrorType.URIError);

		static Type ErrorTypeToClass (ErrorType type)
		{
			switch (type) {
			case ErrorType.EvalError:
				return typeof (EvalErrorObject);
			case ErrorType.RangeError:
				return typeof (RangeErrorObject);
			case ErrorType.ReferenceError:
				return typeof (ReferenceErrorObject);
			case ErrorType.SyntaxError:
				return typeof (SyntaxErrorObject);
			case ErrorType.TypeError:
				return typeof (TypeErrorObject);
			case ErrorType.URIError:
				return typeof (URIErrorObject);
			}

			Console.WriteLine ("ErrorTypeToClass: unknown type {0}", type);
			throw new NotImplementedException ();
		}

		static string ErrorTypeToName (ErrorType type)
		{
			switch (type) {
			case ErrorType.EvalError:
				return "EvalError";
			case ErrorType.OtherError:
				return "Error"; // TODO: Is this correct?
			case ErrorType.RangeError:
				return "RangeError";
			case ErrorType.ReferenceError:
				return "ReferenceError";
			case ErrorType.SyntaxError:
				return "SyntaxError";
			case ErrorType.TypeError:
				return "TypeError";
			case ErrorType.URIError:
				return "URIError";
			}

			Console.WriteLine ("ErrorTypeToName: unknown type {0}", type);
			throw new NotImplementedException ();
		}

		internal ErrorConstructor ()
		{
			name = "Error";
			_prototype = ErrorPrototype.Proto;
			_length = 1;
		}

		internal ErrorConstructor (ErrorType errorType)
		{
			error_type = errorType;
			name = ErrorTypeToName (errorType);
			_prototype = ErrorPrototype.Proto;
			_length = 1;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasVarArgs)]
		public new ErrorObject CreateInstance (params Object [] args)
		{
			string message = "";
			if (args.Length >= 1)
				message = Convert.ToString (args[0]);

			Type type = ErrorTypeToClass (error_type);
			return (ErrorObject) Activator.CreateInstance (type, new object [] { message });
		}

		[JSFunctionAttribute(JSFunctionAttributeEnum.HasVarArgs)]
		public Object Invoke (params Object [] args)
		{
			return CreateInstance (args);
		}
	}
}
