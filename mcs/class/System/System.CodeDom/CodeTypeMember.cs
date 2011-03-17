//
// System.CodeDom CodeTypeMember Class implementation
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

namespace System.CodeDom
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeTypeMember
		: CodeObject
	{
		private string name;
		private MemberAttributes attributes;
		private CodeCommentStatementCollection comments;
		private CodeAttributeDeclarationCollection customAttributes;
		private CodeLinePragma linePragma;
		CodeDirectiveCollection endDirectives;
		CodeDirectiveCollection startDirectives;

		//
		// Constructors
		//
		public CodeTypeMember()
		{
			attributes = (MemberAttributes.Private | MemberAttributes.Final);
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

		public CodeCommentStatementCollection Comments {
			get {
				if ( comments == null )
					comments = new CodeCommentStatementCollection();
				return comments;
			}
		}

		
		public CodeAttributeDeclarationCollection CustomAttributes {
			get {
				if ( customAttributes == null )
					customAttributes = new CodeAttributeDeclarationCollection();
				return customAttributes;
			}
			set {
				customAttributes = value;
			}
		}

		public CodeLinePragma LinePragma {
			get {
				return linePragma;
			}
			set {
				linePragma = value;
			}
		}

		public string Name {
			get {
				if (name == null)
					return String.Empty;
				return name;
			}
			set {
				name = value;
			}
		}

		public CodeDirectiveCollection EndDirectives {
			get {
				if (endDirectives == null)
					endDirectives = new CodeDirectiveCollection ();
				return endDirectives;
			}
		}

		public CodeDirectiveCollection StartDirectives {
			get {
				if (startDirectives == null)
					startDirectives = new CodeDirectiveCollection ();
				return startDirectives;
			}
		}
	}
}
