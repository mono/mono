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
	[PersistChildren (false)]
	[ParseChildren (true)]
	[ToolboxItem (false)]
	[ControlBuilder (typeof (WizardStepControlBuilder))]
	public class TemplatedWizardStep : WizardStepBase
	{
		ITemplate _contentTemplate = null;
		Control _contentTemplateContainer = null;
		ITemplate _customNavigationTemplate = null;
		Control _customNavigationTemplateContainer = null;

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		[TemplateContainerAttribute (typeof (System.Web.UI.WebControls.Wizard))]
		public virtual ITemplate ContentTemplate {
			get { return _contentTemplate; }
			set { _contentTemplate = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Control ContentTemplateContainer {
			get { return _contentTemplateContainer; }
			internal set { _contentTemplateContainer = value; }
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		[TemplateContainerAttribute (typeof (System.Web.UI.WebControls.Wizard))]
		public virtual ITemplate CustomNavigationTemplate
		{
			get { return _customNavigationTemplate; }
			set { _customNavigationTemplate = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Bindable (false)]
		public Control CustomNavigationTemplateContainer {
			get { return _customNavigationTemplateContainer; }
			internal set { _customNavigationTemplateContainer = value; }
		}

		[Browsable (true)]
		[MonoTODO("Why override?")]
		public override string SkinID {
			get { return base.SkinID; }
			set { base.SkinID = value; }
		}
	}
}

#endif
