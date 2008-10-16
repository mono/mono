namespace Monodoc {
using System;
using System.Text;
using System.Xml;

class HtmlHelper {

	public static void Header (StringBuilder sb, string s)
	{
		sb.Append ("<b>");
		sb.Append (s);
		sb.Append ("</b><br>");
	}

	public static void BodyText (StringBuilder sb, string s)
	{
		sb.Append ("<table border=0 cellpading=0 cellspacing=0 width=100%><tr><td></td><td width=\"95%\">");
		sb.Append (s);
		sb.Append ("<td></td></td></tr></table>");
	}

	public static void RenderCaption (StringBuilder sb, string text)
	{
		sb.Append (String.Format (
				   "<table width=\"100%\">" +
				   "<tr bgcolor=#b0c4de><td><i>Mono Class Library</i><h3>{0}</h3></td></tr></table>", text));
	}

	public static void Signature (StringBuilder sb, string s)
	{
		sb.Append (String.Format ("<table bgcolor=#c0c0c0 cellspacing=0 width=100%><tr><td>"+
					  "  <table cellpadding=10 cellspacing=0 width=100%><tr bgcolor=#f2f2f2><td>{0}</td></tr></table>" +
					  "</td></tr></table>", s));
	}

}
}
