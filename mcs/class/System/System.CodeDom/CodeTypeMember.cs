//
// System.CodeDom CodeTypeMember Class implementation
//
// Author:
//   Sean MacIsaac (macisaac@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom
{
	public class CodeTypeMember : CodeObject {
		private string name;

		public string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}
	}
}
