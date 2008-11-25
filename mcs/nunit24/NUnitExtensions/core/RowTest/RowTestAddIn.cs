// *********************************************************************
// Copyright 2007, Andreas Schlapsi
// This is free software licensed under the MIT license. 
// *********************************************************************
using System;
using System.Reflection;
using NUnit.Core;
using NUnit.Core.Extensibility;

namespace NUnit.Core.Extensions.RowTest
{
	[NUnitAddin(Name = "Row Test Extension")]
	public class RowTestAddIn : IAddin, ITestCaseBuilder
	{
		private RowTestFactory _testFactory;
		
		public RowTestAddIn()
		{
			_testFactory = new RowTestFactory();
		}
		
		public bool Install(IExtensionHost host)
		{
			if (host == null)
				throw new ArgumentNullException("host");
			
			IExtensionPoint testCaseBuilders = host.GetExtensionPoint("TestCaseBuilders");
			if (testCaseBuilders == null)
				return false;
			
			testCaseBuilders.Install(this);
			return true;
		}
		
		public bool CanBuildFrom(MethodInfo method)
		{
			return RowTestFramework.IsRowTest(method);
		}
		
		public Test BuildFrom(MethodInfo method)
		{
			if (method == null)
				throw new ArgumentNullException("method");
			
			RowTestSuite suite = _testFactory.CreateRowTestSuite(method);
			Attribute[] rows = RowTestFramework.GetRowAttributes(method);

			foreach (Attribute row in rows)
				suite.Add(_testFactory.CreateRowTestCase(row, method));
			
			return suite;
		}
	}
}
