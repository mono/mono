// System.Drawing.Design.IToolboxUser.cs
// 
// Author:
//      Alejandro Sánchez Acosta  <raciel@es.gnu.org>
// 
// (C) Alejandro Sánchez Acosta
// 

namespace System.Drawing.Design
{
	public interface IToolboxUser
	{
		bool GetToolSupported (ToolboxItem tool);

		void ToolPicked (ToolboxItem tool);
	}
}
