//
// System.ComponentModel.IContainer.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.InteropServices;
namespace System.ComponentModel {

	[ComVisible (true)]
	public interface IContainer : IDisposable 
	{

		ComponentCollection Components {
			get;
		}

		void Add (IComponent component);

		void Add (IComponent component, string name);

		void Remove (IComponent component);
	}
}
