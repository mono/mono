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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez, jordi@ximian.com
//
// TODO:
//		- Complete the implementation when the Button class is available
//
//
// $Revision: 1.1 $
// $Modtime: $
// $Log: MessageBox.cs,v $
// Revision 1.1  2004/07/26 11:41:35  jordi
// initial messagebox implementation
//
//

// INCOMPLETE

using System;
using System.Drawing;

namespace System.Windows.Forms
{

	public class MessageBox
	{
		private class MessageBoxForm : Form
		{
			
			public MessageBoxForm (IWin32Window owner, string text, string caption,
				MessageBoxButtons buttons, MessageBoxIcon icon)
			{
				
			}

			public MessageBoxForm (IWin32Window owner, string text, string caption,
				MessageBoxButtons buttons, MessageBoxIcon icon,
				MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
			{
				
			}
			
			public DialogResult RunDialog ()
			{								
				return DialogResult.OK;
			}
		}


		private MessageBox ()
		{

		}

		public static DialogResult Show (string text)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, string.Empty,
				MessageBoxButtons.OK, MessageBoxIcon.None);

			return form.RunDialog ();

		}

		public static DialogResult Show (IWin32Window owner, string text)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, string.Empty,
				MessageBoxButtons.OK, MessageBoxIcon.None);
				
			return form.RunDialog ();

		}

		public static DialogResult Show (string text, string caption)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption,
				MessageBoxButtons.OK, MessageBoxIcon.None);

			return form.RunDialog ();
		}

		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption,
				buttons, MessageBoxIcon.None);
				
			return form.RunDialog ();
		}

                public static DialogResult Show (IWin32Window owner, string text,  string caption,
			MessageBoxButtons buttons)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption,
				buttons, MessageBoxIcon.None);
				
			return form.RunDialog ();
		}

		public static DialogResult Show (IWin32Window owner, string text, string caption,
			MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption,
				buttons, icon);
				
			return form.RunDialog ();
		}


		public static DialogResult Show (IWin32Window owner, string text, string caption)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption,
				MessageBoxButtons.OK, MessageBoxIcon.None);
				
			return form.RunDialog ();
		}


		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons,
			MessageBoxIcon icon)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption,
				buttons, icon);
				
			return form.RunDialog ();
		}

		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons,
			MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{

			MessageBoxForm form = new MessageBoxForm (null, text, caption,
				buttons, icon, defaultButton, MessageBoxOptions.DefaultDesktopOnly);
				
			return form.RunDialog ();

		}

		public static DialogResult Show (IWin32Window owner, string text, string caption,
			MessageBoxButtons buttons, MessageBoxIcon icon,  MessageBoxDefaultButton defaultButton)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption,
				buttons, icon, defaultButton, MessageBoxOptions.DefaultDesktopOnly);
				
			return form.RunDialog ();
		}

		public static DialogResult
			Show (string text, string caption,
			      MessageBoxButtons buttons, MessageBoxIcon icon,
			      MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption,
				buttons, icon, defaultButton, options);
				
			return form.RunDialog ();
		}

		public static DialogResult Show (IWin32Window owner, string text, string caption,
			MessageBoxButtons buttons, MessageBoxIcon icon,
			MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption,
				buttons, icon, defaultButton, options);
				
			return form.RunDialog ();
		}
	}
}
