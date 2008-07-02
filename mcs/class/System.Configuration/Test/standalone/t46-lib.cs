using System;
using System.Configuration;
using System.Reflection;

public class Foo : MarshalByRefObject
{
	public static Foo GetRemote (AppDomain domain)
	{
		Foo test = (Foo) domain.CreateInstanceAndUnwrap (
			typeof (Foo).Assembly.FullName,
			typeof (Foo).FullName, new object [0]);
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
