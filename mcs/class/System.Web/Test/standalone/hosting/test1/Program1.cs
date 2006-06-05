using System;
using System.Web.UI;
using System.Web;
using System.Web.Hosting;
using ClassLibrary1;
using System.IO;

namespace ConsoleApplication1
{

	class Program
	{
		static void Main (string[] args)
		{
			string master = @"<%@	Master Language=""C#"" %>
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"" xml:lang=""en"" >
<head id=""Head1"" runat=""server"">
<title></title>
</head>
<body>
<asp:contentplaceholder id=""Main"" runat=""server"" />
</body>
</html>";
			string page = @"<%@ Page MasterPageFile=""My.master"" %>
<asp:content id=""Content1"" contentplaceholderid=""Main"" runat=""server"">
<form id=""form1"" runat=""server"" />
</asp:content>";
			string physDir = Directory.GetCurrentDirectory ();
			if (!Directory.Exists ("bin"))
				Directory.CreateDirectory ("bin");
			string masterPath = "My.master";
			if (!File.Exists (masterPath))
				using (StreamWriter sw = new StreamWriter (masterPath))
					sw.Write (master);
			string pagePath = "PageWithMaster.aspx";
			if (!File.Exists (pagePath))
				using (StreamWriter sw = new StreamWriter (pagePath))
					sw.Write (page);
			Class1 c1 = (Class1) ApplicationHost.CreateApplicationHost (
					typeof (Class1), "/", physDir);
			c1.Run ();
		}
	}
}

