//
// System.CodeDom CodeTypeMember Class implementation
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
	public class CodeTypeMember
		: CodeObject
	{
		private string name;
		private MemberAttributes attributes;
		private CodeCommentStatementCollection comments;
		private CodeAttributeDeclarationCollection customAttributes;
		private CodeLinePragma linePragma;

		//
		// Constructors
		//
		public CodeTypeMember()
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
	}
}
