// System.Drawing.Design.PropertyValueUIItemInvokeHandler.cs
// 
// Author:
//      Alejandro Sánchez Acosta  <raciel@es.gnu.org>
// 
// (C) Alejandro Sánchez Acosta
//  

using System.Drawing;
using System.ComponentModel;

namespace System.Drawing.Design
{
	[Serializable]
	public delegate void PropertyValueUIItemInvokeHandler (
				   ITypeDescriptorContext context,
    	                           PropertyDescriptor descriptor,
			           PropertyValueUIItem invokedItem);
}
