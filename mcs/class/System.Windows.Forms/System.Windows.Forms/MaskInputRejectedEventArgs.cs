//
// MaskInputRejectedEventArgs.cs
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
	public class MaskInputRejectedEventArgs : EventArgs
	{
		private int position;
		private MaskedTextResultHint rejection_hint;

		#region Public Constructors
		public MaskInputRejectedEventArgs (int position, MaskedTextResultHint rejectionHint) : base ()
		{
			this.position = position;
			this.rejection_hint = rejectionHint;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public int Position {
			get { return this.position; }
		}

		public MaskedTextResultHint RejectionHint {
			get { return this.rejection_hint; }
		}
		#endregion	// Public Instance Properties
	}
}
