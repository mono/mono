//
// System.CodeDom CodeLinePragma Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//
using System;

namespace System.CodeDom {

	// <summary>
	//    Use objects of this class to keep track of locations where
	//    statements are defined
	// </summary>
	public class CodeLinePragma {
		string fileName;
		int    lineNumber;
		
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
