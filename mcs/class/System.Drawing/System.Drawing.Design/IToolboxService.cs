// System.Drawing.Design.IToolboxService.cs
//
// Author:
//	Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro Sánchez Acosta
// 

using System.Collections;
using System.Runtime.InteropServices;

namespace System.Drawing.Design
{
	//[Guid("")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IToolboxService
	{
		CategoryNameCollection CategoryNames {get;}

		string SelectedCategory {get; set;}

		void AddCreator (ToolboxItemCreatorCallback creator, string format);

		void AddCreator (ToolboxItemCreatorCallback creator, string format, IDesignerHost host);

		void AddLinkedToolboxItem (ToolboxItem toolboxItem, IDesignerHost host);

		void AddLinkedToolboxItem (ToolboxItem toolboxItem, string category, IDesignerHost host);

		ToolboxItem DeserializeToolboxItem (object serializedObject);

		ToolboxItem DeserializeToolboxItem (object serializedObject, IDesignerHost host);

		ToolboxItem GetSelectedToolboxItem ();

		ToolboxItem GetSelectedToolboxItem (IDesignerHost host);

		bool IsSupported (object serializedObject, ICollection filterAttributes);

		bool IsSupported (object serializedObject, IDesignerHost host);

		bool IsToolboxItem (object serializedObject);

		bool IsToolboxItem (object serializedObject, IDesignerHost host);

		void Refresh();

		void RemoveCreator (string format);

		void RemoveCreator (string format, IDesignerHost host);

		void RemoveToolboxItem (ToolboxItem toolboxItem);

		void RemoveToolboxItem (ToolboxItem toolboxItem, string category);

		void SelectedToolboxItemUsed ();

		object SerializeToolboxItem (ToolboxItem toolboxItem);

		bool SetCursor ();

		void SetSelectedToolboxItem (ToolboxItem toolboxItem);
	}
}
