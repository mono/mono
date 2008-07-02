using System;
using System.Configuration;
using System.IO;

class Program : MarshalByRefObject
{
	static void Main (string [] args)
	{
		AppDomainSetup setup = new AppDomainSetup ();

		string basedir = AppDomain.CurrentDomain.BaseDirectory;
		setup.ConfigurationFile = Path.Combine (AppDomain.CurrentDomain.BaseDirectory,
			"t46.exe.config2");

		AppDomain domain = AppDomain.CreateDomain ("test",
			AppDomain.CurrentDomain.Evidence, setup);

		Program p;
		Configuration c;

		p = GetRemote (domain);
		Assert.AreEqual (Path.Combine (basedir, "t46.exe.config2"),
			p.GetFilePath (string.Empty), "#A1");
		Assert.AreEqual ("Hello World2!",
			p.GetSettingValue (string.Empty, "hithere"), "#A2");

		p = new Program ();

		c = p.OpenExeConfiguration (string.Empty);
		Assert.AreEqual (Path.Combine (basedir, "t46.exe.config"),
			c.FilePath, "#B1");
		Assert.AreEqual ("Hello World!",
			c.AppSettings.Settings ["hithere"].Value, "#B2");

		Foo f;
		
		f = Foo.GetRemote (domain);
		Assert.AreEqual (Path.Combine (basedir, "t46.exe.config2"),
			f.GetFilePath (string.Empty), "#C1");
		Assert.AreEqual ("Hello World2!",
			f.GetSettingValue (string.Empty, "hithere"), "#C2");

		f = new Foo ();
		c = f.OpenExeConfiguration (string.Empty);
		Assert.AreEqual (Path.Combine (basedir, "t46.exe.config"),
			c.FilePath, "#D1");
		Assert.AreEqual ("Hello World!",
			c.AppSettings.Settings ["hithere"].Value, "#D2");

		Console.WriteLine ("configuration OK");
	}

	static Program GetRemote (AppDomain domain)
	{
		Program test = (Program) domain.CreateInstanceAndUnwrap (
			typeof (Program).Assembly.FullName,
			typeof (Program).FullName, new object [0]);
		return test;
	}

	public string GetFilePath (string exePath)
	{
		Configuration config = ConfigurationManager.OpenExeConfiguration (exePath);
		return config.FilePath;
	}

	public Configuration OpenExeConfiguration (string exePath)
	{
		return ConfigurationManager.OpenExeConfiguration (exePath);
	}

	public string GetSettingValue (string exePath, string key)
	{
		Configuration config = OpenExeConfiguration (exePath);
		return config.AppSettings.Settings [key].Value;
	}
}
