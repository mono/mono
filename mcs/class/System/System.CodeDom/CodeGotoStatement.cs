//
// System.CodeDom CodeGotoStatement Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2002 Ximian, Inc.
//

using System.Runtime.InteropServices;

namespace System.CodeDom
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeGotoStatement
		: CodeStatement
	{
		private string label;

		//
		// Constructors
		//
		public CodeGotoStatement( string label )
		{
			Label = label;
		}

		//
		// Properties
		//
		public string Label {
			get {
				return label;
			}
			set {
				label = value;
			}
		}
	}
}
