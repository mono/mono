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
using System.Runtime.Serialization;

namespace System.Reflection {

	public class AssemblyName /* : ICloneable, ISerializable, IDeserializationCallback */ {
		string name;
		string codebase;
		Version version;
		
		public AssemblyName ()
		{
			name = null;
		}

		public AssemblyName (SerializationInfo si, StreamingContext sc)
		{
		}

		public virtual string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}

		public virtual string CodeBase {
			get {
				return codebase;
			}

			set {
				codebase = value;
			}
		}

		public virtual Version Version {
			get {
				return version;
			}

			set {
				version = value;
			}
		}
		
		public override int GetHashCode ()
		{
			return name.GetHashCode ();
		}

		public override bool Equals (object o)
		{
			if (!(o is System.Reflection.AssemblyName))
				return false;

			AssemblyName an = (AssemblyName)o;

			if (an.name == this.name)
				return true;
			
			return false;
		}

	}

}
