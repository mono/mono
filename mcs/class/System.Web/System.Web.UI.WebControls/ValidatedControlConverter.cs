
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
/**
 * Namespace: System.Web.UI.WebControls
 * Class:     ValidatedControlConverter
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 * Modifier: Sanjay Gupta
 * Contact: gsanjay@novell.com
 * (C) Gaurav Vaish (2002)
 * (C) 2004 Novell, Inc. (http://www.novell.com)
 */

using System;
using System.ComponentModel;
using System.Collections;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class ValidatedControlConverter : 
#if NET_2_0
	ControlIDConverter
#else
	StringConverter
#endif
	{
		public ValidatedControlConverter(): base()
		{
		}
#if NET_2_0
		protected override bool FilterControl(Control control)
		{
			//GetCustomAttributes (false);
			//What shall we use above method or the one used
			object [] attribs = control.GetType ().GetCustomAttributes 
					(typeof (ValidationPropertyAttribute), false);
			
			foreach (Attribute attr in attribs) 
				if (attr is ValidationPropertyAttribute)
					return true;
			
			return false;										
		}
#else
		private object[] GetValues(IContainer container)
		{
			ArrayList values = new ArrayList();
			IEnumerator ie = container.Components.GetEnumerator();
			try
			{
				foreach(IComponent current in container.Components)
				{
					Control ctrl = (Control)current;
					if(ctrl == null || ctrl.ID == null || ctrl.ID.Length == 0)
						continue;
					ValidationPropertyAttribute attrib = (ValidationPropertyAttribute)((TypeDescriptor.GetAttributes(ctrl))[typeof(ValidationPropertyAttribute)]);
					if(attrib == null || attrib.Name == null)
						continue;
					values.Add(String.Copy(ctrl.ID));
				}
			}finally
			{
				if(ie is IDisposable)
					((IDisposable)ie).Dispose();
			}
			values.Sort();
			return values.ToArray();
		}

		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			if(context != null && context.Container != null)
			{
				object[] values = GetValues(context.Container);
				if(values != null)
				{
					return new StandardValuesCollection(values);
				}
			}
			return null;
		}

		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			return false;
		}

		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return true;
		}
#endif
	}
}
