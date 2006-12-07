//
// Authors:
//   Rafael Mizrahi   <rafim@mainsoft.com>
//   Erez Lotan       <erezl@mainsoft.com>
//   Vladimir Krasnov <vladimirk@mainsoft.com>
//   
// 
// Copyright (c) 2002-2005 Mainsoft Corporation.
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
using System.Net;
using System.Text;
using System.Collections;
using System.Reflection;
using NUnit.Framework;

namespace MonoTests.stand_alone.WebHarness
{
	public class HtmlDiff
	{
		public const string BEGIN_TAG = "begint";
		public const string END_TAG = "endt";

		private XmlDocument _xmlIgnoreList = null;
		private string _compareStatus = "";
		private static string _compareActual = "";
		private static string _compareExpect = "";
		private string _ignoreListFile = "";

		

		public HtmlDiff()
		{
		}

		public string IgnoreListFile
		{
			get {return _ignoreListFile;}
			set {_ignoreListFile = value;}
		}

		public string CompareStatus
		{
			get {return _compareStatus.ToString();}
		}

		public static string GetControlFromPageHtml (string str)
		{
			if (str == null || str == string.Empty)
				throw new ArgumentException ("internal error: str is null or empty");
			int beginPos = str.IndexOf (BEGIN_TAG);
			int endPos = str.IndexOf (END_TAG);
			if (beginPos == -1)
				throw new Exception ("internal error: BEGIN_TAG is missing. Full source: "+str);
			if (endPos == -1)
				throw new Exception ("internal error: END_TAG is missing. Full source: "+str);
				
			StringBuilder sb = new StringBuilder ();
			sb.Append (str.Substring (beginPos + BEGIN_TAG.Length, endPos - beginPos - BEGIN_TAG.Length));
			return sb.ToString ();
		}

		public static void AssertAreEqual (string origin, string derived, string msg)
		{
			bool test = false;
			try {
				test = HtmlComparer (origin, derived);
			}
			catch (Exception e) {
				//swallow e when there is XML error and fallback
				//to the text comparison
				Assert.AreEqual (origin, derived, msg);
			}
			if (!test) {
				Assert.AreEqual (_compareExpect, _compareActual, msg);
			}
		}

		private static bool HtmlComparer (string origin, string derived)
		{
			XmlDocument or = new XmlDocument ();
			MonoTests.stand_alone.WebHarness.HtmlDiff helper = new MonoTests.stand_alone.WebHarness.HtmlDiff ();
			or.LoadXml (helper.HtmltoXml (origin));
			XmlDocument dr = new XmlDocument ();
			dr.LoadXml (helper.HtmltoXml (derived));
			return helper.XmlCompare (or, dr, false);
		}

		private bool XmlCompare(XmlDocument expected, XmlDocument actual, bool ignoreAlmost)
		{
			XmlComparer comparer = new XmlComparer();
			if (ignoreAlmost == false)
			{
				DoAlmost(expected);
				DoAlmost(actual);
			}
			bool c = comparer.AreEqual(expected, actual);
			_compareStatus = comparer.LastCompare;
			_compareActual = comparer.Actual;
			_compareExpect = comparer.Expected;
			return c;
		}

		public string HtmltoXml (string html) //throws XmlException
		{
			HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument ();
			doc.LoadHtml (html.Trim (new char[] { '\r', '\n', ' ' })); // bug in HtmlAgilityPack

			StringBuilder fixedxml = new StringBuilder ();
			StringWriter sw = new StringWriter (fixedxml);

			StringBuilder tempxml = new StringBuilder ();
			StringWriter tsw = new StringWriter (tempxml);

			doc.OptionOutputAsXml = true;
			doc.Save (tsw);

			// fix style attribute
			// the reason is that style attribute name-value pairs come in different order
			// in .NET and GH
			// Here I will sort the values of style attribute
			XmlDocument tempDoc = new XmlDocument ();
			tempDoc.LoadXml (tempxml.ToString ());

			XmlNodeList allNodes = tempDoc.SelectNodes ("//*");
			foreach (XmlNode n in allNodes) {
				if (n.Attributes["style"] != null) {
					string att = n.Attributes["style"].Value;
					string[] style = att.Trim (new char[] { ' ', ';' }).Split (';');

					for (int styleIndex = 0; styleIndex < style.Length; styleIndex++) {
						style[styleIndex] = FixStyleNameValue (style[styleIndex]);
					}
					Array.Sort (style);
					n.Attributes["style"].Value = string.Join (";", style);
				}
			}
			tempDoc.Save (sw);
			return fixedxml.ToString ();
		}

		private string FixStyleNameValue(string nameValue)
		{
			string [] nv = nameValue.Split(':');
			// value may contain spaces in case of
			// multiple values for one key
			string [] nvalue = nv[1].Trim().Split(' ');
			Array.Sort(nvalue);
			nv[1] = string.Join(" ", nvalue);
			return nv[0].Trim().ToLower() + ":" + nv[1].Trim().ToLower();
		}

		private void DoAlmost(XmlDocument xmlDocument)
		{
			XmlNode XmlIgnoreNode;
			IEnumerator xmlIgnoreEnum;


			if (_xmlIgnoreList == null)
			{
				_xmlIgnoreList = new XmlDocument();
				string xml;

				Stream source = Assembly.GetExecutingAssembly ()
					.GetManifestResourceStream ("HtmlCompare.nunitweb_config.xml");
				if (source == null) {
					source = Assembly.GetExecutingAssembly ()
					.GetManifestResourceStream ("nunitweb_config.xml");
				}
								
				try {
					using (StreamReader sr = new StreamReader (source))
						xml = sr.ReadToEnd ();
				}
				finally {
					source.Close ();
				}
				
				_xmlIgnoreList.LoadXml (xml);
			}
			// Remove by Id or Name
			// search by tag and if id or name match, remove all attributes
			// must be the first almost since the following almost delete the id and name
			
			xmlIgnoreEnum = _xmlIgnoreList.SelectSingleNode("Almost/RemoveById").GetEnumerator();
			while (xmlIgnoreEnum.MoveNext())
			{
				XmlNodeList DocNodeList;
				XmlIgnoreNode = (XmlNode)xmlIgnoreEnum.Current;
				DocNodeList = xmlDocument.GetElementsByTagName("*");
				if (DocNodeList != null)
				{
					foreach (XmlElement tmpXmlElement in DocNodeList)
					{
						foreach (XmlAttribute tmpIgnoreAttr in XmlIgnoreNode.Attributes)
						{
							if (tmpXmlElement.Name.ToLower() == XmlIgnoreNode.Name.ToLower()) 
							{
								if (tmpXmlElement.Attributes[tmpIgnoreAttr.Name] != null )
								{
									if (tmpXmlElement.Attributes[tmpIgnoreAttr.Name].Value.ToLower() == tmpIgnoreAttr.Value.ToLower())
									{
										tmpXmlElement.RemoveAllAttributes();
									}
								}
							}
						}
					}
				}	
			}
			// remove ignored attributes
			// search for tag and remove it's attributes
			xmlIgnoreEnum = _xmlIgnoreList.SelectSingleNode("Almost/IgnoreList").GetEnumerator(); //FirstChild.GetEnumerator
			while (xmlIgnoreEnum.MoveNext())
			{
				XmlIgnoreNode = (XmlNode)xmlIgnoreEnum.Current;
				XmlNodeList DocNodeList;
				//clean specific element

				DocNodeList = xmlDocument.GetElementsByTagName("*");
				if (DocNodeList != null)
				{
					foreach (XmlElement tmpXmlElement in DocNodeList)
					{
						if (tmpXmlElement.Name.ToLower() == XmlIgnoreNode.Name.ToLower()) 
						{
							foreach (XmlAttribute tmpIgnoreAttr in XmlIgnoreNode.Attributes)
							{
								tmpXmlElement.RemoveAttribute(tmpIgnoreAttr.Name);
							}
						}
					}
				}
			}

			// clean javascript attribute value
			xmlIgnoreEnum = _xmlIgnoreList.SelectSingleNode("Almost/CleanJavaScriptValueList").GetEnumerator(); //FirstChild.GetEnumerator
			while (xmlIgnoreEnum.MoveNext())
			{
				XmlIgnoreNode = (XmlNode)xmlIgnoreEnum.Current;
				XmlNodeList DocNodeList;
				//clean Java Script attribute values
				DocNodeList = xmlDocument.GetElementsByTagName("*");
				if (DocNodeList != null)
				{
					foreach (XmlElement tmpXmlElement in DocNodeList)
					{
						if (tmpXmlElement.Name.ToLower() == XmlIgnoreNode.Name.ToLower()) 
						{
							foreach (XmlAttribute tmpIgnoreAttr in XmlIgnoreNode.Attributes)
							{
								if (tmpXmlElement.Attributes[tmpIgnoreAttr.Name] != null )
								{
									if (tmpXmlElement.Attributes[tmpIgnoreAttr.Name].Value.ToLower().IndexOf("javascript") >= 0 )
									{
										tmpXmlElement.SetAttribute(tmpIgnoreAttr.Name, "");
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
