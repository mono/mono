//
// System.Drawing.Design.PropertyValueItem.cs
// 
// Authors:
//  Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
// 
// (C) Alejandro Sánchez Acosta
// (C) 2003 Andreas Nahr
// 

using System.Drawing;

namespace System.Drawing.Design
{
	public class PropertyValueUIItem
	{

		private Image uiItemImage;
		private PropertyValueUIItemInvokeHandler handler;
		private string tooltip;

		public PropertyValueUIItem (Image uiItemImage,
			PropertyValueUIItemInvokeHandler handler, string tooltip)
		{
			this.uiItemImage = uiItemImage;
			this.handler = handler;
			this.tooltip = tooltip;
		}

		public virtual Image Image 
		{
			get
			{
				return uiItemImage;
			}
		}

		public virtual PropertyValueUIItemInvokeHandler InvokeHandler
		{
			get
			{
				return handler;
			}
		}

		public virtual string ToolTip 
		{
			get
			{
				return tooltip;
			}
		}

		public virtual void Reset()
		{
			// To be overriden in child classes
		}
	}
}
