//
// System.ComponentModel.IComponent.cs
//
// Authors:
//  Miguel de Icaza (miguel@ximian.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2003 Andreas Nahr
//

using System;

namespace System.ComponentModel
{
	public interface IComponent : IDisposable
	{
		ISite Site { get; set; }
		event EventHandler Disposed;
	}
}

