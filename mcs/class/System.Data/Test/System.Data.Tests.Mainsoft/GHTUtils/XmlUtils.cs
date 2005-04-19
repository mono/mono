// Authors:
//   Rafael Mizrahi   <rafim@mainsoft.com>
//   Erez Lotan       <erezl@mainsoft.com>
//   Oren Gurfinkel   <oreng@mainsoft.com>
//   Ofer Borstein
// 
// Copyright (c) 2004 Mainsoft Co.
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

namespace GHTUtils.Xml
{
	/// <summary>
	/// 
	/// </summary>
	public class XmlUtils
	{
		public XmlUtils()
		{
		}

		/// <summary>
		/// lecsicographic sorting of the XmlNodes child elements and attributes.
		/// </summary>
		/// <param name="a_xmlString">Xml string to sort.</param>
		/// <returns></returns>
		public static string SortXml(string a_xmlString)
		{
			XmlDocument l_toSort = new XmlDocument();
			l_toSort.LoadXml(a_xmlString);
			SortXml(l_toSort.DocumentElement);
			return l_toSort.OuterXml;
		}

		/// <summary>
		/// Inplace pre-order recursive lecsicographic sorting of the XmlNodes child elements and attributes.
		/// </summary>
		/// <param name="a_root">The root to be sorted.</param>
		public static void SortXml(XmlNode a_root)
		{
			SortAttributes(a_root.Attributes);
			SortElements(a_root);
			foreach (XmlNode l_currentChild in a_root.ChildNodes)
			{
				SortXml(l_currentChild);
			}
		}

		/// <summary>
		/// Sorts an attributes collection alphabeticlly.
		/// Uses bubble sort.
		/// </summary>
		/// <param name="a_toSort">The attribute collection to sort.</param>
		public static void SortAttributes(XmlAttributeCollection a_toSort)
		{
			if (a_toSort == null)
			{
				return;
			}

			bool l_change = true;
			while (l_change)
			{
				l_change = false;
				for (int i=1; i<a_toSort.Count; i++)
				{
					if (String.Compare(a_toSort[i].Name, a_toSort[i-1].Name, true) < 0)
					{
						//Replace
						a_toSort.InsertBefore(a_toSort[i], a_toSort[i-1]);
						l_change = true;
					}
				}
			}
			
		}

		/// <summary>
		/// Sorts a XmlNodeList alphbeticlly, by the names of the elements.
		/// Uses bubble sort.
		/// </summary>
		/// <param name="a_toSort">The node list to sort.</param>
		public static void SortElements(XmlNode a_toSort)
		{
			bool l_change = true;
			while (l_change)
			{
				l_change = false;
				for (int i=1; i<a_toSort.ChildNodes.Count; i++)
				{
					if ( String.Compare(a_toSort.ChildNodes[i].Name, a_toSort.ChildNodes[i-1].Name, true) < 0)
					{
						//Replace:
						a_toSort.InsertBefore(a_toSort.ChildNodes[i], a_toSort.ChildNodes[i-1]);
						l_change = true;
					}
				}
			}
		}
	}
}
