//
// System.Runtime.Serialization.SerializationInfo.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// TODO: This is just a skeleton to get corlib to compile.
//

namespace System.Runtime.Serialization {

	public sealed class SerializationInfo {
		Type type;
		IFormatterConverter converter;
		
		public SerializationInfo (Type type, IFormatterConverter converter)
		{
			this.type = type;
			this.converter = converter;
		}
		
		public string AssemblyName {
			get {
				return "TODO: IMPLEMENT ME";
			}
			
			set {
			}
		}
		
		public string FullTypeName {
			get {
				return "TODO: IMLEMENT ME";
			}
			
			set {
			}
		}
		
		
		public int MemberCount {
			get {
				return 0;
			}
		}
	}
}
