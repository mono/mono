using System;
using System.Reflection;

namespace System.Reflection {
	public class AssemblyName /* : ICloneable, ISerializable, IDeserializationCallback */ {
		private string name;
		
		public virtual string Name {
			get {return name;}
			set {name = value;}
		}

	}

}
