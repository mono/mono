//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//
// Authors:
//	Vladimir Krasnov <vladimirk@mainsoft.com>
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

#if NET_2_0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace System.Web.UI.WebControls
{
	[ThemeableAttribute (true)]
	[BindableAttribute (false)]
	public class TemplatedWizardStep : WizardStepBase
	{
		private ITemplate _contentTemplate = null;
		private Control _contentTemplateContainer = null;
		private ITemplate _customNavigationTemplate = null;
		private Control _customNavigationTemplateContainer = null;

		[TemplateContainerAttribute (typeof (System.Web.UI.WebControls.Wizard))]
		public virtual ITemplate ContentTemplate
		{
			get { return _contentTemplate; }
			set { _contentTemplate = value; }
		}

		public Control ContentTemplateContainer
		{
			get { return _contentTemplateContainer; }
			internal set { _contentTemplateContainer = value; }
		}

		[TemplateContainerAttribute (typeof (System.Web.UI.WebControls.Wizard))]
		public virtual ITemplate CustomNavigationTemplate
		{
			get { return _customNavigationTemplate; }
			set { _customNavigationTemplate = value; }
		}

		[BindableAttribute (false)]
		public Control CustomNavigationTemplateContainer
		{
			get { return _customNavigationTemplateContainer; }
			internal set { _customNavigationTemplateContainer = value; }
		}

		public override string SkinID
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		internal virtual ITemplate DefaultContentTemplate
		{
			get { return new DefaultTemplate (); }
		}

		internal virtual BaseWizardContainer DefaultContentContainer
		{
			get { return new BaseWizardContainer (); }
		}

		internal virtual void InstantiateInContainer ()
		{
			if (ContentTemplate == null)
				ContentTemplate = DefaultContentTemplate;

			if (ContentTemplateContainer == null)
				ContentTemplateContainer = DefaultContentContainer;

			if (ContentTemplateContainer is BaseWizardContainer)
				((BaseWizardContainer) ContentTemplateContainer).InstatiateTemplate (ContentTemplate);
			else
				ContentTemplate.InstantiateIn (ContentTemplateContainer);

			Controls.Clear ();
			Controls.Add (ContentTemplateContainer);

			if (CustomNavigationTemplate != null) {
				if (CustomNavigationTemplateContainer == null)
					CustomNavigationTemplateContainer = new Control ();

				CustomNavigationTemplate.InstantiateIn (CustomNavigationTemplateContainer);
			}
		}
	}

	sealed class DefaultTemplate : ITemplate
	{
		public void InstantiateIn (Control container)
		{
		}
	}

	internal class BaseWizardContainer : Table, INamingContainer
	{
		internal BaseWizardContainer ()
		{
			InitTable ();
		}

		internal void InstatiateTemplate (ITemplate template)
		{
			TableCell defaultCell = this.Rows [0].Cells [0];
			template.InstantiateIn (defaultCell);
		}

		private void InitTable ()
		{
			TableRow row = new TableRow ();
			TableCell cell = new TableCell ();

			cell.ControlStyle.Width = Unit.Percentage (100);
			cell.ControlStyle.Height = Unit.Percentage (100);

			row.Cells.Add (cell);

			this.ControlStyle.Width = Unit.Percentage (100);
			this.ControlStyle.Height = Unit.Percentage (100);
			this.CellPadding = 0;
			this.CellSpacing = 0;

			this.Rows.Add (row);
		}
	}
}

#endif