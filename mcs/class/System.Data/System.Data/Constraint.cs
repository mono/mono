//
// System.Data.Constraint.cs
//
// Author:
//   Daniel Morgan
//
// (C) Ximian, Inc. 2002
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Data
{
	[Serializable]
	public abstract class Constraint {

		[MonoTODO]
		[Serializable]
		protected Constraint() {
		}

		[Serializable]
		public virtual string ConstraintName {
			[MonoTODO]
			get{
			} 

			[MonoTODO]
			set{
			}
		}

		[Serializable]
		public PropertyCollection ExtendedProperties {
			[MonoTODO]
			get {
			}
		}

		[Serializable]
		public abstract DataTable Table {
			get;
		}

		[MonoTODO]
		[Serializable]
		public override string ToString() {
		}

		[MonoTODO]
		[Serializable]
		[ClassInterface(ClassInterfaceType.AutoDual)]
		~Constraint() {
		}
	}
}
