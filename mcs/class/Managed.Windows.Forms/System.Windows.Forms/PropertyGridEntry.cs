using System;
using System.ComponentModel;

namespace System.Windows.Forms.PropertyGridInternal {
	/// <summary>
	/// Summary description for PropertyGridEntry.
	/// </summary>
	internal class PropertyGridEntry : GridEntry {
		public PropertyGridEntry() {
			//
			// TODO: Add constructor logic here
			//
		}

		public PropertyGridEntry(object obj, PropertyDescriptor prop_desc) : base(obj, prop_desc) {
		}
	}
}
