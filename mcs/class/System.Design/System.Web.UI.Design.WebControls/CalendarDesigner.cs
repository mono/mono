/**
 * Namespace:   System.Web.UI.Design.WebControls
 * Class:       CalendarDesigner
 *
 * Author:      Gaurav Vaish
 * Maintainer:  gvaish_mono@lycos.com
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI.Design;

namespace System.Web.UI.Design.WebControls
{
	public class CalendarDesigner : ControlDesigner
	{
		private DesignerVerbCollection verbs;
		private Calendar               calendar;

		public CalendarDesigner() : base()
		{
		}

		public override DesignerVerbCollection Verbs
		{
			get
			{
				if(verbs == null)
				{
					verbs = new DesignerVerbCollection();
					//verbs.Add(new DesignerVerb(OnAutoFormat:Event_Handler)
				}
				return verbs;
			}
		}

		public override void Initialize(IComponent component)
		{
			if(component is Calendar)
			{
				base.Initialize(component);
				this.calendar = (Calendar) component;
			}
		}

		[MonoTODO]
		protected void OnAutoFormat(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}
