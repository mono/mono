#region Copyright (c) 2002, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Philip A. Craig
/************************************************************************************
'
' Copyright © 2002 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov
' Copyright © 2000-2002 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright © 2002 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov 
' or Copyright © 2000-2002 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

namespace NUnit.Core
{
	using System;
	using System.IO;
	using System.Reflection;
	using System.Collections;

	/// <summary>
	/// Summary description for TestSuiteBuilder.
	/// </summary>
	public class TestSuiteBuilder
	{
		Hashtable suites  = new Hashtable();
		TestSuite rootSuite;

		public string TrimPathAndExtension(string assemblyName) 
		{
			FileInfo info = new FileInfo(assemblyName);
			string name = info.Name;
			string extension = info.Extension;
			if (extension != String.Empty)
				name = name.Substring(0, name.IndexOf(extension));

			return name;
		}

		public Assembly Load(string assemblyName)
		{
			Assembly assembly = AppDomain.CurrentDomain.Load(TrimPathAndExtension(assemblyName));
			return assembly;
		}

		private TestSuite BuildFromNameSpace(string nameSpace)
		{
			if( nameSpace == null || nameSpace  == "" ) return rootSuite;
			TestSuite suite = (TestSuite)suites[nameSpace];
			if(suite!=null) return suite;
			int index = nameSpace.LastIndexOf(".");
			if(index==-1)
			{
				suite = new TestSuite(nameSpace);
				rootSuite.Add(suite);
				suites[nameSpace]=suite;
				return suite;
			}
			string parentNameSpace=nameSpace.Substring( 0,index);
			TestSuite parent = BuildFromNameSpace(parentNameSpace);
			string suiteName = nameSpace.Substring(index+1);
			suite = new TestSuite(parentNameSpace,suiteName);
			suites[nameSpace]=suite;
			parent.Add(suite);
			return suite;
		}

		public TestSuite Build(string assemblyName)
		{
			TestSuiteBuilder builder = new TestSuiteBuilder();

			Assembly assembly = Load(assemblyName);

			builder.rootSuite = new TestSuite(assemblyName);
			int testFixtureCount = 0;
			Type[] testTypes = assembly.GetExportedTypes();
			foreach(Type testType in testTypes)
			{
				if(IsTestFixture(testType))
				{
					testFixtureCount++;
					string namespaces = testType.Namespace;
					TestSuite suite = builder.BuildFromNameSpace(namespaces);

					try
					{
						object fixture = BuildTestFixture(testType);
						suite.Add(fixture);
					}
					catch(InvalidTestFixtureException exception)
					{
						InvalidFixture fixture = new InvalidFixture(testType, exception.Message);
						suite.Add(fixture);
					}
				}
			}

			if(testFixtureCount == 0)
				throw new NoTestFixturesException(assemblyName + " has no TestFixtures");

			return builder.rootSuite;
		}


		public TestSuite Build(string testName, string assemblyName)
		{
			TestSuite suite = null;

			Assembly assembly = Load(assemblyName);

			if(assembly != null)
			{
				Type testType = assembly.GetType(testName);
				if(testType != null)
				{
					if(IsTestFixture(testType))
					{
						suite = MakeSuiteFromTestFixtureType(testType);
					}
					else if(IsTestSuiteProperty(testType))
					{
						suite = MakeSuiteFromProperty(testType);
					}
				}
			}
			return suite;
		}

		private bool IsTestFixture(Type type)
		{
			if(type.IsAbstract) return false;

			return type.IsDefined(typeof(NUnit.Framework.TestFixtureAttribute), true);
		}

		public object BuildTestFixture(Type fixtureType)
		{
			ConstructorInfo ctor = fixtureType.GetConstructor(Type.EmptyTypes);
			if(ctor == null) throw new InvalidTestFixtureException(fixtureType.FullName + " does not have a valid constructor");

			object testFixture = ctor.Invoke(Type.EmptyTypes);
			if(testFixture == null) throw new InvalidTestFixtureException(ctor.Name + " cannot be invoked");

			if(HasMultipleSetUpMethods(testFixture))
			{
				throw new InvalidTestFixtureException(ctor.Name + " has multiple SetUp methods");
			}
			if(HasMultipleTearDownMethods(testFixture))
			{
				throw new InvalidTestFixtureException(ctor.Name + " has multiple TearDown methods");
			}
			if(HasMultipleFixtureSetUpMethods(testFixture))
			{
				throw new InvalidTestFixtureException(ctor.Name + " has multiple TestFixtureSetUp methods");
			}
			if(HasMultipleFixtureTearDownMethods(testFixture))
			{
				throw new InvalidTestFixtureException(ctor.Name + " has multiple TestFixtureTearDown methods");
			}

			return testFixture;
		}

		private int CountMethodWithGivenAttribute(object fixture, Type type)
		{
			int count = 0;
			foreach(MethodInfo method in fixture.GetType().GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.DeclaredOnly))
			{
				if(method.IsDefined(type,false)) 
					count++;
			}
			return count;

		}

		private bool HasMultipleSetUpMethods(object fixture)
		{
			return CountMethodWithGivenAttribute(fixture,typeof(NUnit.Framework.SetUpAttribute)) > 1;
		}

		private bool HasMultipleTearDownMethods(object fixture)
		{
			return CountMethodWithGivenAttribute(fixture,typeof(NUnit.Framework.TearDownAttribute)) > 1;
		}

		private bool HasMultipleFixtureSetUpMethods(object fixture)
		{
			return CountMethodWithGivenAttribute(fixture,typeof(NUnit.Framework.TestFixtureSetUpAttribute)) > 1;
		}

		private bool HasMultipleFixtureTearDownMethods(object fixture)
		{
			return CountMethodWithGivenAttribute(fixture,typeof(NUnit.Framework.TestFixtureTearDownAttribute)) > 1;
		}

		public TestSuite MakeSuiteFromTestFixtureType(Type fixtureType)
		{
			TestSuite suite = new TestSuite(fixtureType.Name);
			try
			{
				object testFixture = BuildTestFixture(fixtureType);
				suite.Add(testFixture);
			}
			catch(InvalidTestFixtureException exception)
			{
				InvalidFixture fixture = new InvalidFixture(fixtureType,exception.Message);
				suite.ShouldRun = false;
				suite.IgnoreReason = exception.Message;
				suite.Add(fixture);
			}

			return suite;
		}

		private bool IsTestSuiteProperty(Type testClass)
		{
			return (GetSuiteProperty(testClass) != null);
		}

		/// <summary>
		/// Uses reflection to obtain the suite property for the Type
		/// </summary>
		/// <param name="testClass"></param>
		/// <returns>The Suite property of the Type, or null if the property 
		/// does not exist</returns>
		private TestSuite MakeSuiteFromProperty(Type testClass) 
		{
			TestSuite suite = null;
			PropertyInfo suiteProperty = null;
			try
			{
				suiteProperty=GetSuiteProperty(testClass);
				suite = (TestSuite)suiteProperty.GetValue(null, new Object[0]);
			}
			catch(InvalidSuiteException)
			{
				return null;
			}
			return suite;
		}

		private PropertyInfo GetSuiteProperty(Type testClass)
		{
			if(testClass != null)
			{
				PropertyInfo[] properties = testClass.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
				foreach(PropertyInfo property in properties)
				{
					object[] attrributes = property.GetCustomAttributes(typeof(NUnit.Framework.SuiteAttribute),false);
					if(attrributes.Length>0)
					{
						try {
							CheckSuiteProperty(property);
						}catch(InvalidSuiteException){
							return null;
						}
						return property;
					}
				}
			}
			return null;
		}

		private void CheckSuiteProperty(PropertyInfo property)
		{
			MethodInfo method = property.GetGetMethod(true);
			if(method.ReturnType!=typeof(NUnit.Core.TestSuite))
				throw new InvalidSuiteException("Invalid suite property method signature");
			if(method.GetParameters().Length>0)
				throw new InvalidSuiteException("Invalid suite property method signature");
		}
	}
}
