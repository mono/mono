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
using System.Configuration;
using System.Xml;
using System.Collections;
using System.Net;
using System.IO;
#if NUNIT
using NUnit.Framework;
using NUnit.Core;
#endif

namespace MonoTests.stand_alone.WebHarness
{
	public class Harness
	{
		static string _ignoreListFile = "";
		static string _catalogFile = "";
		static string _outputPath = "";
		static string _baseUrlExp = "";
		static string _baseUrlTst = "";
		static bool _disableAlmost = false;
		static bool _runExcluded = false;

		static void Main(string[] args)
		{
			ParseCommandLineAgrs(args);

			if (_baseUrlExp != "")
			{
				//baseUrl = "http://localhost:80/System_Web_dll";
				CreateExpectedResults(_baseUrlExp);
			}

			if (_baseUrlTst != "")
			{
				//baseUrl = "http://localhost:8080/System_Web_dll";
				RunTests(_baseUrlTst);
			}
		}

#if NUNIT
		[Suite]
		static public TestSuite Suite 
		{
			get 
			{
				ParseAppConfigFile();

				TestSuite suite = new TestSuite ("SystemWebTests");
				TestsCatalog tc = new TestsCatalog();
				foreach (TestInfo ti in tc)
				{
					suite.Add(new SingleWebTest(ti, _baseUrlTst));
				}
				
				return suite;
			}
		}
#endif

		#region "Cmd line"

		static string GetParameterByName(string name, string[] args)
		{
			int i = Array.IndexOf(args, name);
			if (i >= 0)
			{
				if (i < args.Length - 1)
				{
					return args[i+1];
				}
			}
			return "";
		}

		static bool IsParameterSet(string name, string[] args)
		{
			int i = Array.IndexOf(args, name);
			return (i >= 0);
		}

		static void ParseAppConfigFile()
		{
			_disableAlmost = Convert.ToBoolean(ConfigurationSettings.AppSettings["DisableAlmost"]);
			_runExcluded = Convert.ToBoolean(ConfigurationSettings.AppSettings["RunExcluded"]);
			_ignoreListFile = ConfigurationSettings.AppSettings["AlmostList"];
			_catalogFile = ConfigurationSettings.AppSettings["TestCatalog"];
			_outputPath = ConfigurationSettings.AppSettings["OutputDir"];
			if ((!_outputPath.EndsWith("\\")) && (!_outputPath.EndsWith("/")))
			{
				_outputPath += Path.DirectorySeparatorChar;
			}

			_baseUrlExp = ConfigurationSettings.AppSettings["ExpResBaseUrl"];
			_baseUrlTst = ConfigurationSettings.AppSettings["TestBaseUrl"];
		}

		static void ParseCommandLineAgrs(string [] args)
		{
			_disableAlmost = IsParameterSet("-na", args);
			_runExcluded = IsParameterSet("-x", args);
			
			// specifies the almost config xml file
			// default is almost_config.xml in current folder
			_ignoreListFile = GetParameterByName("-i", args);
			if (_ignoreListFile == "")
			{
				_ignoreListFile = "almost_config.xml";
			}
			
			// specifies tests catalog xml file
			// default is test_catalog.xml in current folder
			_catalogFile = GetParameterByName("-c", args);
			if (_catalogFile == "")
			{
				_catalogFile = "test_catalog.xml";
			}
			
			// specifies the folder where expected results will be stored
			// by default is current folder
			_outputPath = GetParameterByName("-o", args);
			if (_outputPath != "")
			{
				if ((!_outputPath.EndsWith("\\")) && (!_outputPath.EndsWith("/")))
				{
					_outputPath += Path.DirectorySeparatorChar;
				}
			}

			// specifies the base url for all tests
			// no default value
			_baseUrlExp = GetParameterByName("-e", args);
			_baseUrlTst = GetParameterByName("-t", args);
		}
		#endregion

		#region "Tests run routines"

		static void CreateExpectedResults(string baseUrl)
		{
			TestsCatalog tc = new TestsCatalog(_catalogFile, _runExcluded);
			HtmlDiff wt = new HtmlDiff();
			wt.TestsBaseUrl = baseUrl;
			wt.IgnoreListFile = _ignoreListFile;

			if ((_outputPath != "") && (!Directory.Exists(_outputPath)))
			{
				Directory.CreateDirectory(_outputPath);
			}

			Console.WriteLine("Running expected results...");
			foreach (TestInfo ti in tc)
			{
				Console.WriteLine("Running...  " + ti.Url);
				XmlDocument d = wt.GetTestXml( ti );
				d.Save(_outputPath + ti.Url.Replace("/", "_") + ".xml");
			}
		}

		static void RunTests(string baseUrl)
		{
			TestsCatalog tc = new TestsCatalog(_catalogFile, _runExcluded);
			foreach (TestInfo ti in tc)
			{
				try {
					RunSingleTest(baseUrl, ti);
				}
				catch (Exception e) {
					Console.WriteLine(e.Message);
				}
			}
		}

		public static bool RunSingleTest(string baseUrl, TestInfo ti)
		{
			HtmlDiff wt = new HtmlDiff();
			wt.TestsBaseUrl = baseUrl;
			wt.IgnoreListFile = _ignoreListFile;

			XmlDocument d1 = new XmlDocument();
			d1.Load(_outputPath + ti.Url.Replace("/", "_") + ".xml");
			
			XmlDocument d2 = wt.GetTestXml( ti );
			bool fp = wt.XmlCompare(d1, d2, _disableAlmost);
			if (fp == false) {
				throw new Exception("Url: " + ti.Url + "\nCompare failed:\n" + wt.CompareStatus + "\n");
			}
			return fp;
		}

		#endregion
	}

	#region "NUnit"

#if NUNIT
	public class SingleWebTest : NUnit.Core.TestCase 
	{
		TestInfo _testInfo = null;
		string _baseUrl = "";
		public SingleWebTest (TestInfo testInfo, string baseUrl) : base (null, testInfo.Url) 
		{
			_testInfo = testInfo;
			_baseUrl = baseUrl;
		}

		public override void Run (TestCaseResult res) 
		{
			try
			{
				Harness.RunSingleTest(_baseUrl, _testInfo);
			}
			catch (Exception e)
			{
				res.Failure(e.Message, null);
				return;
			}
			res.Success();
		}
	}
#endif

	#endregion

}
