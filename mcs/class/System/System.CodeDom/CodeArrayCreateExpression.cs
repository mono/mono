//
// System.CodeDOM CodeArrayCreateExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//
namespace System.CodeDOM {

	public class CodeArrayCreateExpression : CodeExpression {
		string createType;
		CodeExpressionCollection initializers;
		CodeExpression sizeExpression;
		int size;
		
		//
		// Constructors
		//
		public CodeArrayCreateExpression ()
		{
			
		}

		public CodeArrayCreateExpression (string createType, CodeExpression size)
		{
			this.createType = createType;
			this.sizeExpression = size;
		}

		public CodeArrayCreateExpression (string createType, int size)
		{
			this.createType = createType;
			this.size = size;
		}

		public CodeArrayCreateExpression (string createType, CodeExpression [] initializers)
		{
			this.createType = createType;
			this.initializers = new CodeExpressionCollection ();

			this.initializers.AddRange (initializers);
		}

		//
		// Properties
		//
		public CodeExpression SizeExpression {
			get {
				return sizeExpression;
			}

			set {
				sizeExpression = value;
			}
		}

		public int Size {
			get {
				return size;
			}

			set {
				size = value;
			}
		}

		public CodeExpressionCollection Initializers {
			get {
				return initializers;
			}

			set {
				initializers = value;
			}
		}

		public string CreateType {
			get {
				return createType;
			}

			set {
				createType = value;
			}
		}
	}
}

