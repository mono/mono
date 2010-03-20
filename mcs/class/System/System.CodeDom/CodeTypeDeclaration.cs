//
// System.CodeDom CodeTypeDeclaration Class implementation
//
// Author:
//   Sean MacIsaac (macisaac@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
//   Marek Safar (marek.safar@seznam.cz)
//
// (C) 2001 Ximian, Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
		private TypeAttributes attributes = TypeAttributes.Public;
		private bool isEnum;
		private bool isStruct;
		//int populated;

#if NET_2_0
		bool isPartial;
		CodeTypeParameterCollection typeParameters;
#endif

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
				if ( (attributes & TypeAttributes.Interface) != 0 )
					return false;
				if ( isEnum )
					return false;
				if ( isStruct )
					return false;
				return true;
			}
			set {
				if ( value ) {
					attributes &= ~TypeAttributes.Interface;
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
					attributes &= ~TypeAttributes.Interface;
					isEnum = true;
					isStruct = false;
				}
			}
		}

		public bool IsInterface {
			get {
				return (attributes & TypeAttributes.Interface) != 0;
			}
			set {
				if ( value ) {
					attributes |= TypeAttributes.Interface;
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
					attributes &= ~TypeAttributes.Interface;
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
				return attributes;
			}
			set {
				attributes = value;
#if FALSE
				/* MS does not seem to do this, so don't I */
				if ( (attributes & TypeAttributes.Interface) != 0 ) {
					isEnum = false;
					isStruct = false;
				}
#endif
			}
		}

#if NET_2_0
		public bool IsPartial {
			get {
				return isPartial;
			}
			set {
				isPartial = value;
			}
		}

		[ComVisible (false)]
		public CodeTypeParameterCollection TypeParameters {
			get {
				if (typeParameters == null)
					typeParameters = new CodeTypeParameterCollection ();
				return typeParameters;
			}
		}
#endif

		//
		// Events
		// 
		public event EventHandler PopulateBaseTypes;

		public event EventHandler PopulateMembers;
	}
}
