// *********************************************************************
// Copyright 2007, Andreas Schlapsi
// This is free software licensed under the MIT license. 
// *********************************************************************
using System;
using System.Reflection;
using NUnit.Core;

namespace NUnit.Core.Extensions.RowTest
{
	public class RowTestSuite : TestSuite
	{
		private static string GetParentName(MethodInfo method)
		{
			if (method == null)
				throw new ArgumentNullException("method");
			
			return method.DeclaringType.ToString();
		}
		
		private static string GetTestName(MethodInfo method)
		{
			if (method == null)
				throw new ArgumentNullException("method");
			
			return method.Name;
		}
		
		public RowTestSuite(MethodInfo method)
			: base (GetParentName(method), GetTestName(method))
		{
		}
		
		public override TestResult Run(EventListener listener, ITestFilter filter)
		{
			if (this.Parent != null)
				this.Fixture = this.Parent.Fixture;
			
			return base.Run(listener, filter);
		}
		
		protected override void DoOneTimeSetUp(TestResult suiteResult)
		{
		}
		
		protected override void DoOneTimeTearDown(TestResult suiteResult)
		{
		}
	}
}
