//
// System.CodeDom CodeArrayCreateExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2001 Ximian, Inc.
//

using System.Runtime.InteropServices;

namespace System.CodeDom {

	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeArrayCreateExpression
		: CodeExpression 
	{
		private CodeTypeReference createType;
		private CodeExpressionCollection initializers;
		private CodeExpression sizeExpression;
		private int size;
		
		//
		// Constructors
		//
		public CodeArrayCreateExpression ()
		{
		}


		public CodeArrayCreateExpression (CodeTypeReference createType, 
						  CodeExpression size )
		{
			this.createType = createType;
			this.sizeExpression = size;
		}

		public CodeArrayCreateExpression (CodeTypeReference createType, 
						  params CodeExpression[] initializers )
		{
			this.createType = createType;
			this.Initializers.AddRange( initializers );
		}

		public CodeArrayCreateExpression (CodeTypeReference createType, 
						  int size)
		{
			this.createType = createType;
			this.size = size;
		}


		public CodeArrayCreateExpression (string createType, 
						  CodeExpression size)
		{
			this.createType = new CodeTypeReference( createType );
			this.sizeExpression = size;
		}

		public CodeArrayCreateExpression (string createType, 
						  params CodeExpression[] initializers)
		{
			this.createType = new CodeTypeReference( createType );
			this.Initializers.AddRange( initializers );
		}

		public CodeArrayCreateExpression (string createType, 
						  int size)
		{
			this.createType = new CodeTypeReference( createType );
			this.size = size;
		}


		public CodeArrayCreateExpression (Type createType, 
						  CodeExpression size)
		{
			this.createType = new CodeTypeReference( createType );
			this.sizeExpression = size;
		}
			
		public CodeArrayCreateExpression (Type createType, 
						  params CodeExpression[] initializers)
		{
			this.createType = new CodeTypeReference( createType );
			this.Initializers.AddRange( initializers );
		}

		public CodeArrayCreateExpression (Type createType, 
						  int size)
		{
			this.createType = new CodeTypeReference( createType );
			this.size = size;
		}

		//
		// Properties
		//
		public CodeTypeReference CreateType {
			get {
				return createType;
			}
			set {
				createType = value;
			}
		}

		public CodeExpressionCollection Initializers {
			get {
				if ( initializers == null )
					initializers = new CodeExpressionCollection();
					
				return initializers;
			}
		}

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
				// NOTE: Setting Size in ms.Net does
				// not supersede SizeExpression
				// values. Instead, the CodeGenerator
				// seems to always prefer
				// SizeExpression if set to a value !=
				// null.
			}
		}
	}
}

