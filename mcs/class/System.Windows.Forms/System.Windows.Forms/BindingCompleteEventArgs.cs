//
// BindingCompleteEventArgs.cs
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

using System.ComponentModel;

namespace System.Windows.Forms
{
	public class BindingCompleteEventArgs : CancelEventArgs
	{
		private Binding binding;
		private BindingCompleteState state;
		private BindingCompleteContext context;
		private string error_text;
		private Exception exception;

		#region Public Constructors
		public BindingCompleteEventArgs(Binding binding, BindingCompleteState state, BindingCompleteContext context)
			: this (binding, state, context, String.Empty, null, false)
		{
		}

		public BindingCompleteEventArgs(Binding binding, BindingCompleteState state, BindingCompleteContext context, string errorText)
			: this (binding, state, context, errorText, null, false)
		{
		}

		public BindingCompleteEventArgs(Binding binding, BindingCompleteState state, BindingCompleteContext context, string errorText, Exception exception)
			: this (binding, state, context, errorText, exception, false)
		{
		}

		public BindingCompleteEventArgs(Binding binding, BindingCompleteState state, BindingCompleteContext context, string errorText, Exception exception, bool cancel)
			: base (cancel)
		{
			this.binding = binding;
			this.state = state;
			this.context = context;
			this.error_text = errorText;
			this.exception = exception;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public Binding Binding {
			get { return this.binding; }
		}

		public BindingCompleteContext BindingCompleteContext {
			get { return this.context; }
		}

		public BindingCompleteState BindingCompleteState {
			get { return this.state; }
		}

		public string ErrorText {
			get { return this.error_text; }
		}

		public Exception Exception {
			get { return this.exception; }
		}
		#endregion	// Public Instance Properties

		internal void SetErrorText (string error_text)
		{
			this.error_text = error_text;
		}

		internal void SetException (Exception exception)
		{
			this.exception = exception;
		}
	}
}
