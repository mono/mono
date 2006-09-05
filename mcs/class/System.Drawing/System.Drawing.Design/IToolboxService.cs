//
// System.Drawing.Design.IToolboxService.cs
//
// Authors:
//	Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Alejandro Sánchez Acosta
// (C) 2003 Andreas Nahr
// Copyright (C) 2004, 2006 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;

namespace System.Drawing.Design {

#if NET_2_0
	[ComImport]
#endif
	[Guid("4BACD258-DE64-4048-BC4E-FEDBEF9ACB76"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IToolboxService
	{
		CategoryNameCollection CategoryNames {get;}

		string SelectedCategory {get; set;}

		void AddCreator (ToolboxItemCreatorCallback creator, string format);

		void AddCreator (ToolboxItemCreatorCallback creator, string format, IDesignerHost host);

		void AddLinkedToolboxItem (ToolboxItem toolboxItem, IDesignerHost host);

		void AddLinkedToolboxItem (ToolboxItem toolboxItem, string category, IDesignerHost host);

		void AddToolboxItem (ToolboxItem toolboxItem, String category);

		void AddToolboxItem (ToolboxItem toolboxItem);

		ToolboxItem DeserializeToolboxItem (object serializedObject);

		ToolboxItem DeserializeToolboxItem (object serializedObject, IDesignerHost host);

		ToolboxItem GetSelectedToolboxItem ();

		ToolboxItem GetSelectedToolboxItem (IDesignerHost host);

		ToolboxItemCollection GetToolboxItems ();

		ToolboxItemCollection GetToolboxItems (IDesignerHost host);

		ToolboxItemCollection GetToolboxItems (String category);

		ToolboxItemCollection GetToolboxItems (String category, IDesignerHost host);

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
