/**
 * Namespace: System.Web.UI.WebControls
 * Class:     ValidatedControlConverter
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.ComponentModel;
using System.Collections;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class ValidatedControlConverter : StringConverter
	{
		public ValidatedControlConverter(): base()
		{
		}

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
					ie.Dispose();
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
	}
}
