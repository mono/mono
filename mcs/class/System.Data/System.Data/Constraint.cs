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
using System.Runtime.Serialization;

namespace System.Data
{
	[Serializable]
	public abstract class Constraint {

		protected string name = null;
		protected PropertyCollection properties = null;

		[MonoTODO]
		protected Constraint() {
			properties = new PropertyCollection();
		}

		public virtual string ConstraintName {
			[MonoTODO]	
			get{
				return name;
			} 

			[MonoTODO]
			set{
				name = value;
			}
		}

		public PropertyCollection ExtendedProperties {
			[MonoTODO]
			get {
				return properties;
			}
		}

		public abstract DataTable Table {
			get;
		}

		[MonoTODO]
		public override string ToString() {
			return name;
		}

		//[MonoTODO]
		//[ClassInterface(ClassInterfaceType.AutoDual)]
		//~Constraint() {
		//}
	}
}
