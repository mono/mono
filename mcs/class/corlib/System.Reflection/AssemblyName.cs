
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

	public class AssemblyName  : ISerializable // ICloneable, , IDeserializationCallback
	{
		string name;
		string codebase;
		Version version;
		
		public AssemblyName ()
		{
			name = null;
		}

		internal AssemblyName (SerializationInfo si, StreamingContext sc)
		{
			name = si.GetString ("_Name");
			codebase = si.GetString ("_CodeBase");
			version = (Version)si.GetValue ("_Version", typeof (Version));
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

		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("_Name", name);
			info.AddValue ("_CodeBase", codebase);
			info.AddValue ("_Version", version);
		}
	}
}
