//
// System.CodeDom CodeTypeDeclaration Class implementation
//
// Author:
//   Sean MacIsaac (macisaac@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2001 Ximian, Inc.
//

using System.Runtime.InteropServices;

namespace System.CodeDom
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeTypeDeclaration
		: CodeTypeMember
	{
		private CodeTypeReferenceCollection baseTypes;
		private CodeTypeMemberCollection members;
		private bool isClass;
		private bool isEnum;
		private bool isInterface;
		private bool isStruct;

		//
		// Constructors
		//
		public CodeTypeDeclaration()
		{
		}
		
		public CodeTypeDeclaration( string name )
		{
			this.Name = name;
		}

		//
		// Properties
		//
		public CodeTypeReferenceCollection BaseTypes {
			get {
				if ( baseTypes == null ) {
					baseTypes = new CodeTypeReferenceCollection();
					PopulateBaseTypes( this, EventArgs.Empty );
				}
				return baseTypes;
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

		public CodeTypeMemberCollection Members {
			get {
				if ( members == null ) {
					members = new CodeTypeMemberCollection();
					PopulateMembers( this, EventArgs.Empty );
				}
				return members;
			}
		}

		//
		// Events
		// 
		public event EventHandler PopulateBaseTypes;

		public event EventHandler PopulateMembers;
	}
}
