//
// System.ComponentModel.IComponent.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2003 Andreas Nahr
//

using System;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;

namespace System.ComponentModel
{
	[ComVisible (true), TypeConverter (typeof (System.ComponentModel.ComponentConverter))]
    	[Designer ("System.Windows.Forms.Design.ComponentDocumentDesigner, " + Consts.AssemblySystem_Design, typeof (IRootDesigner))]
	[RootDesignerSerializer ("System.ComponentModel.Design.Serialization.RootCodeDomSerializer, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.Serialization.CodeDomSerializer, " + Consts.AssemblySystem_Design, true)]
	public interface IComponent : IDisposable
	{
		ISite Site { get; set; }
		event EventHandler Disposed;
	}
}

