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
using System.Reflection;

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
		private TypeAttributes typeAttributes = TypeAttributes.Public;
		private bool isEnum;
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

		/* by default, it's a class */

		//
		// Properties
		//
		public CodeTypeReferenceCollection BaseTypes {
			get {
				if ( baseTypes == null ) {
					baseTypes = new CodeTypeReferenceCollection();
					if ( PopulateBaseTypes != null )
						PopulateBaseTypes( this, EventArgs.Empty );
				}
				return baseTypes;
			}
		}

		public bool IsClass {
			get {
				if ( (typeAttributes & TypeAttributes.Interface) != 0 )
					return false;
				if ( isEnum )
					return false;
				if ( isStruct )
					return false;
				return true;
			}
			set {
				if ( value ) {
					typeAttributes &= ~TypeAttributes.Interface;
					isEnum = false;
					isStruct = false;
				}
			}
		}
		
		public bool IsEnum {
			get {
				return isEnum;
			}
			set {
				if ( value ) {
					typeAttributes &= ~TypeAttributes.Interface;
					isEnum = true;
					isStruct = false;
				}
			}
		}

		public bool IsInterface {
			get {
				return (typeAttributes & TypeAttributes.Interface) != 0;
			}
			set {
				if ( value ) {
					typeAttributes |= TypeAttributes.Interface;
					isEnum = false;
					isStruct = false;
				}
			}
		}

		public bool IsStruct {
			get {
				return isStruct;
			}
			set {
				if ( value ) {
					typeAttributes &= ~TypeAttributes.Interface;
					isEnum = false;
					isStruct = true;
				}
			}
		}

		public CodeTypeMemberCollection Members {
			get {
				if ( members == null ) {
					members = new CodeTypeMemberCollection();
					if ( PopulateMembers != null )
						PopulateMembers( this, EventArgs.Empty );
				}
				return members;
			}
		}

		public TypeAttributes TypeAttributes {
			get {
				return typeAttributes;
			}
			set {
				typeAttributes = value;
#if FALSE
				/* MS does not seem to do this, so don't I */
				if ( (typeAttributes & TypeAttributes.Interface) != 0 ) {
					isEnum = false;
					isStruct = false;
				}
#endif
			}
		}

		//
		// Events
		// 
		public event EventHandler PopulateBaseTypes;

		public event EventHandler PopulateMembers;
	}
}
