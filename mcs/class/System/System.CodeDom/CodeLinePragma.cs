//
// System.CodeDom CodeLinePragma Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

using System.Runtime.InteropServices;

namespace System.CodeDom 
{
	// <summary>
	//    Use objects of this class to keep track of locations where
	//    statements are defined
	// </summary>
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeLinePragma 
	{
		private string fileName;
		private int lineNumber;
		
		//
		// Constructors
		//
		public CodeLinePragma (string fileName, int lineNumber)
		{
			this.fileName = fileName;
			this.lineNumber = lineNumber;
		}
		
		//
		// Properties
		//
		public string FileName {
			get {
				return fileName;
			}
			set {
				fileName = value;
			}
		}
		
		public int LineNumber {
			get {
				return lineNumber;
			}
			set {
				lineNumber = value;
			}
		}
	}
}
