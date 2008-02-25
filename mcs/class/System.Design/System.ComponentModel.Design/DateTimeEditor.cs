//
// System.ComponentModel.Design.DateTimeEditor
//
// Authors:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
// (C) 2007 Andreas Nahr
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
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace System.ComponentModel.Design
{
	public class DateTimeEditor : UITypeEditor
	{
		private class EditorControl : MonthCalendar
		{
			public EditorControl ()
			{
				MaxSelectionCount = 1;
			}
		}

		private IWindowsFormsEditorService editorService;
		private EditorControl control = new EditorControl ();
		private DateTime editContent;

		public DateTimeEditor ()
		{
			control.DateSelected += new DateRangeEventHandler (control_DateSelected);
		}

		public override object EditValue (ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && provider != null)
			{
				editorService = (IWindowsFormsEditorService)provider.GetService (typeof (IWindowsFormsEditorService));
				if (editorService != null)
				{
					if (!(value is DateTime))
						return value;

					editContent = (DateTime)value;
					if (editContent > control.MaxDate || editContent < control.MinDate)
						control.SelectionStart = DateTime.Today;
					else
						control.SelectionStart = editContent;

					editorService.DropDownControl (control);

					return editContent;
				}
			}
			return base.EditValue (context, provider, value);
		}

		void control_DateSelected (object sender, DateRangeEventArgs e)
		{
			editContent = e.Start;
			editorService.CloseDropDown ();
		}

		public override UITypeEditorEditStyle GetEditStyle (ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}
	}
}
