/**
 * Namespace:   System.Web.UI.Design.WebControls
 * Class:       XmlDesigner
 *
 * Author:      Gaurav Vaish
 * Maintainer:  gvaish_mono@lycos.com
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Web.UI.WebControls;
using System.Web.UI.Design;

namespace System.Web.UI.Design.WebControls
{
	public class XmlDesigner : ControlDesigner
	{
		private Xml xml;

		public XmlDesigner()
		{
		}

		public override void Initialize(IComponent component)
		{
			if(component is Xml)
			{
				xml = (Xml)component;
			}
			base.Initialize(component);
		}

		public override string GetDesignTimeHtml()
		{
			return GetEmptyDesignTimeHtml();
		}

		[MonoTODO]
		protected override string GetEmptyDesignTimeHtml()
		{
			throw new NotImplementedException();
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing)
				xml = null;
			base.Disponse(disposing);
		}
	}
}
