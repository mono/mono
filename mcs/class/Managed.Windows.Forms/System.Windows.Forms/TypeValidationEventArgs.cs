//
// TypeValidationEventArgs.cs
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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

namespace System.Windows.Forms
{
	public class TypeValidationEventArgs : EventArgs
	{
		private bool cancel;
		private bool is_valid_input;
		private string message;
		private object return_value;
		private Type validating_type;

		#region Public Constructors
		public TypeValidationEventArgs (Type validatingType, bool isValidInput, object returnValue, string message) : base ()
		{
			this.is_valid_input = isValidInput;
			this.message = message;
			this.return_value = returnValue;
			this.validating_type = validatingType;
			this.cancel = false;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public bool Cancel {
			get { return this.cancel; }
			set { this.cancel = value; }
		}
		
		public bool IsValidInput {
			get { return this.is_valid_input; }
		}
		
		public string Message {
			get { return this.message; }
		}
		
		public object ReturnValue {
			get { return this.return_value; }
		}
		
		public Type ValidatingType {
			get { return this.validating_type; }
		}
		
		#endregion	// Public Instance Properties
	}
}
