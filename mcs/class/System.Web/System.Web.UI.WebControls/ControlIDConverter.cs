//
// System.Web.UI.WebControls.ControlIDConverter.cs
//
// Authors:
//      Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2004-2010 Novell, Inc (http://www.novell.com)
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
using System.Collections;

namespace System.Web.UI.WebControls
{
	public class ControlIDConverter : StringConverter
	{
		public ControlIDConverter ()
		{ }

		protected virtual bool FilterControl (Control control)
		{
			return true;
		}

		/*public ICollection GetStandardValues ()
		{
			return null;
		}*/

		public override TypeConverter.StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
		{
			if (context == null)
				return null;

			IContainer container = context.Container;

			if (container == null)
				return null;

			ComponentCollection ctrlCollection = container.Components;
			ArrayList arrayList = new ArrayList (0);

			foreach (Control control in ctrlCollection) {
				if (FilterControl(control))
					arrayList.Add (control.ID);
			}
			return new StandardValuesCollection (arrayList);
		}

		/*public bool GetStandardValuesExclusive ()
		{
			return false;
		}*/

		public override bool GetStandardValuesExclusive 
					(ITypeDescriptorContext context)
		{
			return false;
		}

		/*public bool GetStandardValuesSupported ()
		{
			return false;
		}*/

		public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
		{
			if (context == null)
				return false;
			
			return true;
		}
	}
}

