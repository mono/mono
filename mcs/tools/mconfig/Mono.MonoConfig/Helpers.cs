//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Novell, Inc
//

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
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Mono.MonoConfig
{
	public static class Helpers
	{
		public const string FakeRootName = "F_a_K_e_R_o_O_t_M_o_N_o_C_o_N_f_I_g_N_o_D_e";
		
		public static EnumType ConvertEnum <EnumType> (string value, string attrName)
		{
			try {
				EnumConverter cvt = new EnumConverter (typeof (EnumType));
				return (EnumType) cvt.ConvertFromInvariantString (value);
			} catch (Exception) {
				throw new ApplicationException (
					String.Format ("Failed to parse the '{0}' attribute '{1}'", attrName, value));
			}
		}

		public static string BreakLongLine (string line, string indent, int maxLineWidth)
		{
			StringBuilder sb = new StringBuilder ();

			int lineLen = line.Length;
			int segments = lineLen / maxLineWidth;
			int segmentStart = 0;
			int segmentLen = maxLineWidth - 1;
			int idx;

			while (segments-- >= 0) {
				idx = line.LastIndexOf (' ', segmentStart + segmentLen);
				if (idx > 0)
					segmentLen = idx - segmentStart;
				else
					idx = segmentLen - 1;

				sb.AppendFormat ("{0}{1}\n", indent, line.Substring (segmentStart, segmentLen));
				segmentStart = idx + 1;
				if (lineLen - segmentStart > maxLineWidth)
					segmentLen = maxLineWidth;
				else
					segmentLen = lineLen - segmentStart - 1;
			}

			return sb.ToString ();
		}
		
		public static string GetRequiredNonEmptyAttribute (XPathNavigator node, string name)
		{
			string val = GetOptionalAttribute (node, name);
			
			if (String.IsNullOrEmpty (val))
				ThrowMissingRequiredAttribute (node, name);
			return val;
		}
		
		public static string GetOptionalAttribute (XPathNavigator node, string name)
		{
			return node.GetAttribute (name, String.Empty);
		}
		
		static void ThrowMissingRequiredAttribute (XPathNavigator node, string name)
		{
			throw new ApplicationException (String.Format ("Element '{0}' is missing required attribute '{1}'",
								       node.LocalName, name));
		}

		public static void BuildSectionTree (XPathNodeIterator iter, Section section)
		{
			XPathNavigator nav, tmp;
			XPathNodeIterator children;
			List <Section> sectionChildren = section.Children;
			Section newSection, curSection;
			
			while (iter.MoveNext ()) {
				nav = iter.Current;
				children = nav.Select ("section[string-length(@name) > 0]");
				curSection = new Section (nav);
				
				while (children.MoveNext ()) {
					tmp = children.Current;
					newSection = new Section (tmp);
					BuildSectionTree (tmp.Select ("section[string-length(@name) > 0]"), newSection);
					curSection.Children.Add (newSection);
				}
				sectionChildren.Add (curSection);
			}
		}

		public static XmlDocument FindDefault (IDefaultContainer[] defaults, string name, FeatureTarget target)
		{
			int len;
			
			if (defaults == null || (len = defaults.Length) == 0)
				return null;
			
			IDefaultContainer cur;
			string text = null;
			
			for (int i = 0; i < len; i++) {
				cur = defaults [i];
				text = cur.FindDefault (name, target);
				if (text != null)
					break;
			}

			if (text == null)
				return null;

			XmlDocument ret = new XmlDocument ();
			ret.LoadXml (String.Format ("<{0}>{1}</{0}>", FakeRootName, text));

			return ret;
		}

		public static ConfigBlockBlock FindConfigBlock (IConfigBlockContainer[] configBlocks, string name)
		{
			int len;

			if (configBlocks == null || (len = configBlocks.Length) == 0)
				return null;
			
			IConfigBlockContainer cur;
			ConfigBlockBlock ret = null;

			for (int i = 0; i < len; i++) {
				cur = configBlocks [i];
				ret = cur.FindConfigBlock (name);
				if (ret != null)
					break;
			}

			return ret;
		}

		public static void SaveXml (XmlDocument doc, string targetFile)
		{
			XmlWriterSettings settings = new XmlWriterSettings ();
			settings.CloseOutput = true;
			settings.CheckCharacters = true;
			settings.Indent = true;
			settings.Encoding = Encoding.UTF8;
			settings.IndentChars = "\t";
			settings.NewLineHandling = NewLineHandling.Replace;

			XmlWriter writer = null;
			try {
				writer = XmlWriter.Create (targetFile, settings);
				doc.Save (writer);
				writer.Flush ();
			} catch (Exception ex) {
				throw new ApplicationException (
					String.Format ("Failed to write XML file {1}", targetFile), ex);
			} finally {
				if (writer != null)
					writer.Close ();
			}
		}
	}
}
