// System.Drawing.Design.PropertyValueUIHandler.cs
//
// Author:
// 	Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro Sánchez Acosta
//

using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace System.Drawing.Design
{
	
	[Serializable]
	public delegate void PropertyValueUIHandler (ITypeDescriptorContext context, PropertyDescriptor propDesc, ArrayList valueUIItemList);
	
}
