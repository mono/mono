//
// DbConfig.cs : Defines a class 'ConfigClass' that is used to read the config file
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
using System.IO;
using System.Xml;

namespace MonoTests.System.Data {

	public class ConfigClass {
	
		public static String GetElement(XmlNode doc, params String [] args) 
		{
			String element = "";
			if (args.Length > 0) {
				XmlNode xmlNode = doc.SelectSingleNode(args [0]);
				for (int i = 1; i < args.Length; i++)
					xmlNode = xmlNode.SelectSingleNode(args [i]);

				element = xmlNode.InnerText.Trim ();
			}
			return element;
		}
	
		public static String [] GetColumnDetails (XmlNode doc, int tableNum, string attr) 
		{
			String tagName = "table" + tableNum;
			int numColumns = Convert.ToInt32 (ConfigClass.GetElement (doc, "tables", tagName, "numColumns"));
			String [] columns = new String [numColumns];

			for (int col = 1; col <= numColumns; col++) {
				XmlNodeList nodelist = doc.SelectNodes ("//tables/" + tagName + "/column" + col + "/" + attr);
				if (nodelist.Count == 0)
					columns [col - 1] = "";
				else
					columns [col - 1] = nodelist [0].InnerText; 
			}
			return columns;
		}
	}
}

