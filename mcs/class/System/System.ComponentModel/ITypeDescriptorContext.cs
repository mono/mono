//
// System.ComponentModel.ITypeDescriptorContext.cs
//
// Authors:
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Runtime.InteropServices;

namespace System.ComponentModel
{
	[ComVisible (true)]
	public interface ITypeDescriptorContext : IServiceProvider
	{
		IContainer Container { get; }

		object Instance { get; }

		PropertyDescriptor PropertyDescriptor { get; }

		void OnComponentChanged ();

		bool OnComponentChanging ();
	}

}
