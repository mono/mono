//
// System.ComponentModel.ComponentCollection.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) Tim Coleman, 2002
//

using System.Collections;
using System.Runtime.InteropServices;

namespace System.ComponentModel {
	[ComVisible (true)]
	public class ComponentCollection : ReadOnlyCollectionBase {

		#region Constructors

		[MonoTODO]
		public ComponentCollection (IComponent[] components)
		{
			throw new NotImplementedException ();
		}

		#endregion // Constructors

		#region Properties

		public virtual IComponent this [string name] {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public virtual IComponent this [int index] {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public void CopyTo (IComponent[] array, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("this probably shouldn't be here.")]
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
		
	}
}
			
