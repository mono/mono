//
// System.Windows.Forms.Design.ComponentEditorForm.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//
using System;
using System.ComponentModel;
namespace System.Windows.Forms.Design
{
	/// <summary>
	/// Summary description for WindowsFormsComponentEditor.
	/// </summary>
	public abstract class ComponentEditor
	{
		public ComponentEditor()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		
		public bool EditComponent(object obj)
		{
			return false;
		}
		
		public abstract bool EditComponent(ITypeDescriptorContext ctx, object obj);
	}
	
	public class WindowsFormsComponentEditor : ComponentEditor
	{
		public WindowsFormsComponentEditor()
		{
		}
		
		public override bool EditComponent(ITypeDescriptorContext ctx, object obj)
		{
			return false;
		}
		
		public bool EditComponent(object obj, IWin32Window iwnd)
		{
			return false;
		}
		
		public virtual bool EditComponent(ITypeDescriptorContext ctx, object obj, IWin32Window iwnd)
		{
			return false;
		}
	}

}
