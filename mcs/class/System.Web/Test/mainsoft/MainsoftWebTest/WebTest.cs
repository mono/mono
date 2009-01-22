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

namespace MonoTests.stand_alone.WebHarness
{
	public abstract class XmlComparableTest
	{
		public abstract XmlDocument GetTestXml(TestInfo testInfo);
		public abstract bool XmlCompare(XmlDocument d1, XmlDocument d2, bool ignoreAlmost);
	}

	public class HtmlDiff : XmlComparableTest
	{
		private string _testsBaseUrl = "";
		private string _ignoreListFile = "";
		private XmlDocument _xmlIgnoreList = null;
		private string _compareStatus = "";

		public HtmlDiff()
		{
		}

		public string TestsBaseUrl
		{
			get {return _testsBaseUrl;}
			set {_testsBaseUrl = value;}
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

		public override XmlDocument GetTestXml(TestInfo testInfo)
		{
			return BuildXml( GetSubTests( GetUrl(testInfo.Url) ), testInfo );
		}

		public override bool XmlCompare(XmlDocument d1, XmlDocument d2, bool ignoreAlmost)
		{
			XmlComparer comparer = new XmlComparer();
			if (ignoreAlmost == false)
			{
				DoAlmost(d1);
				DoAlmost(d2);
			}
			bool c = comparer.AreEqual(d1, d2);
			_compareStatus = comparer.LastCompare;
			return c;
		}

		//============================================================================
		//

		private string GetUrl(string url)
		{
			try
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_testsBaseUrl + url);
				request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322)";
				request.Accept = "image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, application/x-shockwave-flash, */*";

				HttpWebResponse response = (HttpWebResponse)request.GetResponse();

				Stream responseStream = response.GetResponseStream();
				StreamReader sr = new StreamReader(responseStream);

				string s = sr.ReadToEnd();
				sr.Close();

				return s;
			}
			catch(Exception e)
			{
				Console.WriteLine("Cannot retrieve document from url " + _testsBaseUrl + url);
				Console.WriteLine(e.Message);
			}

			return "";
		}

		private ArrayList GetSubTests(string s)
		{
			ArrayList subTestList = new ArrayList();
			int startIndex = SkipViewstate (s);
			string subTest = FindSubTest(s, startIndex);
			
			while (subTest != "")
			{
				if (subTest.ToLower().IndexOf("ghtsubtest") != -1)
					subTestList.Add(subTest);
				subTest = FindSubTest(s, s.IndexOf(subTest) + subTest.Length);
			}

			return subTestList;
		}
		#region "Sub Test extraction routines"
		private string FindSubTest(string s, int startIndex)
		{
			int tagBeginCount = 0;
			int stringPosition = startIndex;
			int tagPosition = 0;

			tagPosition = GetNextDivPosition(s, stringPosition);
			if (tagPosition == -1) return "";

			if (isBeginTag(s, tagPosition))
				tagBeginCount++;
			else
				return "";

			startIndex = tagPosition;

			while (tagBeginCount > 0)
			{
				stringPosition = tagPosition + 1;
				tagPosition = GetNextDivPosition(s, stringPosition);
				if (tagPosition == -1) return "";

				if (isBeginTag(s, tagPosition))
					tagBeginCount++;
				else
					tagBeginCount--;
			}

			return s.Substring(startIndex, tagPosition - startIndex + 6);
		}

		private int GetNextDivPosition(string s, int startIndex)
		{
			int tagBeginPos = GetBeginDivPosition(s, startIndex);
			int tagEndPos = GetEndDivPosition(s, startIndex);

			if ((tagBeginPos == -1) && (tagEndPos == -1)) return -1;
			if ((tagBeginPos > 0) && (tagEndPos > 0))
			{
				if (tagBeginPos < tagEndPos)
					return tagBeginPos;
				else
					return tagEndPos;
			}
			else
			{
				if (tagBeginPos > tagEndPos)
					return tagBeginPos;
				else
					return tagEndPos;
			}
		}

		private int SkipViewstate (string s)
		{
			int start = s.IndexOf ("<div id");
			int startVS = s.IndexOf ("<div>");
			int vs = s.IndexOf ("VIEWSTATE");
			int endVS = s.IndexOf ("</div>");

			if (startVS > 0 && startVS < vs && vs < endVS && startVS < start)
				return endVS + 7;

			return 0;
		}

		private int GetBeginDivPosition(string s, int startIndex)
		{
			return s.IndexOf("<div id", startIndex);
		}

		private int GetEndDivPosition(string s, int startIndex)
		{
			return s.IndexOf("</div", startIndex);
		}

		private bool isBeginTag(string tag, int pos)
		{
			return tag.Substring(pos).StartsWith("<div");
		}
		private bool isEndTag(string tag, int pos)
		{
			return tag.Substring(pos).StartsWith("</div");
		}
		#endregion

		private XmlDocument BuildXml(ArrayList subTests, TestInfo ti)
		{
			StringBuilder xmltext = new StringBuilder();
			xmltext.Append("<TestResults name=\"" + ti.Name + "\">");
			foreach(string st in subTests)
			{
				xmltext.Append(st);
			}
			xmltext.Append("</TestResults>");

			XmlDocument r = new XmlDocument();
			r.LoadXml(HtmltoXml(xmltext.ToString()));
			return r;
		}

		private string HtmltoXml(string html)
		{
			HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
			doc.LoadHtml(html);

			StringBuilder fixedxml = new StringBuilder();
			StringWriter sw = new StringWriter(fixedxml);

			try
			{
				StringBuilder tempxml = new StringBuilder();
				StringWriter tsw = new StringWriter(tempxml);

				doc.OptionOutputAsXml = true;
				doc.Save(tsw);

				// fix style attribute
				// the reason is that style attribute name-value pairs come in different order
				// in .NET and GH
				// Here I will sort the values of style attribute
				XmlDocument tempDoc = new XmlDocument();
				tempDoc.LoadXml(tempxml.ToString());

				XmlNodeList allNodes = tempDoc.SelectNodes("//*");
				foreach (XmlNode n in allNodes)
				{
					if (n.Attributes["style"] != null)
					{
						string att = n.Attributes["style"].Value;
						string [] style = att.Trim(new char[]{' ', ';'}).Split(';');

						for (int styleIndex=0; styleIndex<style.Length; styleIndex++)
						{
							style[styleIndex] = FixStyleNameValue(style[styleIndex]);
						}
						Array.Sort(style);
						n.Attributes["style"].Value = string.Join(";", style);
					}
				}
				tempDoc.Save(sw);
			}
			catch (Exception)
			{
				Console.WriteLine("Error parsing html response...");
				Console.WriteLine("Test case aborted");
				return "<TestCaseAborted></TestCaseAborted>";
			}
			return fixedxml.ToString();
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
				_xmlIgnoreList.Load(_ignoreListFile);
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
									if ((tmpXmlElement.Attributes [tmpIgnoreAttr.Name].Value.ToLower ().IndexOf ("javascript") >= 0) ||
									(tmpXmlElement.Attributes [tmpIgnoreAttr.Name].Value.ToLower ().IndexOf ("dopostback") >= 0)) {
										tmpXmlElement.SetAttribute (tmpIgnoreAttr.Name, "");
									}
								}
							}
						}
					}
				}
			}
			// remove whole tags
			ArrayList tagsToRemove = new ArrayList ();
			xmlIgnoreEnum = _xmlIgnoreList.SelectSingleNode ("Almost/RemoveTags").GetEnumerator (); //FirstChild.GetEnumerator
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
							tagsToRemove.Add (tmpXmlElement);
							//tmpXmlElement.ParentNode.RemoveChild (tmpXmlElement);
						}
					}
				}
			}
			if (tagsToRemove.Count > 0) {
				foreach (XmlElement el in tagsToRemove) {
					try {
						el.ParentNode.RemoveChild (el);
					}
					catch { }
				}
			}
		}
	}
}
