//
// System.CodeDom CodeStatement class implementation
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
	public class CodeStatement
		: CodeObject
	{
		private CodeLinePragma linePragma;
		
		//
		// Constructors
		//
		public CodeStatement()
		{
		}
		
		//
		// Properties
		//
		public CodeLinePragma LinePragma {
			get {
				return linePragma;
			}
			set {
				linePragma = value;
			}
		}
	}
}
