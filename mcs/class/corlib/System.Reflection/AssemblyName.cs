//
// System.Reflection/AssemblyName.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;

namespace System.Reflection {
	public class AssemblyName /* : ICloneable, ISerializable, IDeserializationCallback */ {
		private string name;
		
		public virtual string Name {
			get {return name;}
			set {name = value;}
		}

		public AssemblyName () {
			name = null;
		}

	}

}
