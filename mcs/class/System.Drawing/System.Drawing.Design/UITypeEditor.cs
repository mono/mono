//
// System.Drawing.Design.UITypeEditor.cs
// 
// Authors:
//  Alan Tam Siu Lung <Tam@SiuLung.com>
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
// 
// (C) 2003 Alan Tam Siu Lung <Tam@SiuLung.com>
// (C) 2003 Andreas Nahr
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;
using System.Security.Permissions;
using System.Collections;

namespace System.Drawing.Design
{
	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	[PermissionSet (SecurityAction.InheritanceDemand, Unrestricted = true)]
	public class UITypeEditor {

		static UITypeEditor ()
		{
			Hashtable editors = new Hashtable ();
			editors [typeof (Array)] = "System.ComponentModel.Design.ArrayEditor, " + Consts.AssemblySystem_Design;
			editors [typeof (byte [])] = "System.ComponentModel.Design.BinaryEditor, " + Consts.AssemblySystem_Design;
			editors [typeof (DateTime)] = "System.ComponentModel.Design.DateTimeEditor, " + Consts.AssemblySystem_Design;
			editors [typeof (IList)] = "System.ComponentModel.Design.CollectionEditor, " + Consts.AssemblySystem_Design;
			editors [typeof (ICollection)] = "System.ComponentModel.Design.CollectionEditor, " + Consts.AssemblySystem_Design;
			editors [typeof (string[])] = "System.Windows.Forms.Design.StringArrayEditor, " + Consts.AssemblySystem_Design;
			TypeDescriptor.AddEditorTable (typeof (UITypeEditor), editors);
		}
		
		public UITypeEditor()
		{
		}

		public virtual object EditValue (ITypeDescriptorContext context,
			IServiceProvider provider, object value)
		{
			// We already stated that we can't edit ;)
			return value;
		}

		public object EditValue (IServiceProvider provider, object value)
		{
			return EditValue (null, provider, value);
		}

		public virtual UITypeEditorEditStyle GetEditStyle (ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.None;
		}

		public UITypeEditorEditStyle GetEditStyle ()
		{
			return GetEditStyle (null);
		}

		public bool GetPaintValueSupported ()
		{
			return GetPaintValueSupported (null);
		}

		public virtual bool GetPaintValueSupported (ITypeDescriptorContext context)
		{
			return false;
		}
#if !TARGET_JVM
		public void PaintValue (object value, Graphics canvas, Rectangle rectangle)
		{
			PaintValue (new PaintValueEventArgs (null, value, canvas, rectangle));
		}

		public virtual void PaintValue (PaintValueEventArgs e)
		{
			// LAMESPEC: Did not find info in the docs if this should do something here.
			// Usually you would expect, that this class gets inherited and this overridden, 
			// but on the other hand the class is not abstract. Could never observe it did paint anything
			return;
		}
#endif
#if NET_2_0
		public virtual bool IsDropDownResizable {
			get { return false; }
		}
#endif
	}
}
