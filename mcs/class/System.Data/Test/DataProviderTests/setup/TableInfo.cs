//
// TableInfo.cs : A data structure to hold all information about tables
//
// Author:
//   Satya Sudha K (ksathyasudha@novell.com)
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;
using System.Xml;

namespace MonoTests.System.Data {

	public struct TableInfo {
	
		public string name;
		public string [] columns;
		public string [] types;

		public TableInfo (int n) 
		{
			name = null;
			columns = null;
			types = null;
		}
		
		public void Initialize (XmlNode doc, int tableNum) 
		{
			string tableTag = "table" + tableNum;
			name = ConfigClass.GetElement (doc, "tables", tableTag, "name");
			columns = ConfigClass.GetColumnDetails (doc, tableNum, "name");
			types = ConfigClass.GetColumnDetails (doc, tableNum, "type");
		}
	
	}

}
