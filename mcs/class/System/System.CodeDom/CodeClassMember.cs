//
// System.CodeDom CodeClassMember Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	public class CodeClassMember : CodeStatement {
		MemberAttributes attributes;
		CodeAttributeBlock customAttributes;
		
		string name;

		//
		// Yeah, this is a strange way of defining this
		//
		public enum MemberAttributes {
			Abstract    = 0x0001,
			Final       = 0x0002,
			Override    = 0x0004,
			Const       = 0x0005,
			Assembly    = 0x1000,
			AccessMask  = 0xf000,
			FamANDAssem = 0x2000,
			Family      = 0x3000,
			FamORAssem  = 0x4000,
			New         = 0x0010,
			Private     = 0x5000,
			Public      = 0x6000,
			ScopeMask   = 0x000f,
			Static      = 0x0003,
			VTableMask  = 0x00f0
		}
		
		//
		// Constructors
		//
		public CodeClassMember ()
		{
		}

		//
		// Properties
		//

		public MemberAttributes Attributes {
			get {
				return attributes;
			}

			set {
				attributes = value;
			}
		}

		public CodeAttributeBlock CustomAttributes {
			get {
				return customAttributes;
			}

			set {
				customAttributes = value;
			}
		}

		public string Name {
			get {
				return name;
			}

			set {
				name = value;
			}
		}
	}
}
