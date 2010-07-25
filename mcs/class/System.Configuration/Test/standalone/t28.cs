using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Configuration;
using System.Web;

class T1
{
	static void Main(string[] args)
	{
		try {
			NameValueCollection AppSettings = ConfigurationManager.AppSettings;
			Assert.Fail ("#1:" + AppSettings);
		} catch (ConfigurationErrorsException ex) {
			// Configuration system failed to initialize
			Assert.AreEqual (typeof (ConfigurationErrorsException), ex.GetType (), "#2");
			Assert.IsNull (ex.Filename, "#3");
			Assert.IsNotNull (ex.InnerException, "#6");
			Assert.AreEqual (0, ex.Line, "#7");
			Assert.IsNotNull (ex.Message, "#8");

			// <location> sections are allowed only within <configuration> sections
			ConfigurationErrorsException inner = ex.InnerException as ConfigurationErrorsException;
			Assert.AreEqual (typeof (ConfigurationErrorsException), inner.GetType (), "#9");
			Assert.AreEqual (AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, inner.Filename, "#10");
			Assert.IsNull (inner.InnerException, "#11");
			Assert.AreEqual (3, inner.Line, "#12");
			Assert.IsNotNull (inner.Message, "#13");
			Assert.IsTrue (inner.Message.IndexOf ("<location>") != -1, "#14:" + inner.Message);
			Assert.IsTrue (inner.Message.IndexOf ("<configuration>") != -1, "#15:" + inner.Message);

			Console.WriteLine ("configuration exception thrown.");
		}
	}
}
