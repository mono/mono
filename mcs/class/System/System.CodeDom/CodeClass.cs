//
// System.CodeDOM CodeClass Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDOM {
	using System.Reflection;
	using System.Collections;
	
	public class CodeClass : CodeClassMember {
		CodeClassMemberCollection members;		
		TypeAttributes attrs;
		StringCollection baseTypes;
		bool isClass, isEnum, isInterface, isStruct;
			
		string name;
		
		//
		// Constructors
		//
		public CodeClass ()
		{
		}
		
		public CodeClass (string name)
		{
			this.name = name;
		}

		//
		// Properties
		//
		public TypeAttributes attributes {
			get {
				return attrs;
			}

			set {
				attrs = value;
			}
		}

		public StringCollection BaseTypes {
			get {
				return baseTypes;
			}

			set {
				baseTypes = value;
			}
		}

		public bool IsClass {
			get {
				return isClass;
			}

			set {
				isClass = value;
			}
		}
		public bool IsEnum {
			get {
				return isEnum;
			}

			set {
				isEnum = value;
			}
		}
		
		public bool IsInterface {
			get {
				return isInterface;
			}

			set {
				isInterface = value;
			}
		}
		public bool IsStruct {
			get {
				return isStruct;
			}

			set {
				isStruct = value;
			}
		}

		public CodeClassMemberCollection Members {
			get {
				return members;
			}

			set {
				members = value;
			}
		}
	}
}
