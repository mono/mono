// System.Drawing.Design.ToolboxItemCreatorCallback.cs
// 
// Author:
//      Alejandro Sánchez Acosta  <raciel@es.gnu.org>
// 
// (C) Alejandro Sánchez Acosta
// 

namespace System.Drawing.Design
{
	[Serializable]
	public delegate ToolboxItem ToolboxItemCreatorCallback(
				   object serializedObject,
		        	   string format);
}
