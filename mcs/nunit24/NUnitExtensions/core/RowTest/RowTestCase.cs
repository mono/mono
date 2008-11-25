// *********************************************************************
// Copyright 2007, Andreas Schlapsi
// This is free software licensed under the MIT license. 
// *********************************************************************
using System;
using System.Reflection;
using System.Text;
using NUnit.Core;

namespace NUnit.Core.Extensions.RowTest
{
	public class RowTestCase : NUnitTestMethod
	{
		private object[] _arguments;
		
		public RowTestCase(MethodInfo method, string testName, object[] arguments)
			: base(method)
		{
			RowTestNameBuilder testNameBuilder = new RowTestNameBuilder(method, testName, arguments);
			this.TestName.Name = testNameBuilder.TestName;
			this.TestName.FullName = testNameBuilder.FullTestName;
			
			_arguments = arguments;
		}
		
		public object[] Arguments
		{
			get { return _arguments; }
		}
		
		public override void RunTestMethod(TestCaseResult testResult)
		{
			object[] arguments = _arguments != null ? _arguments : new object[] { null };			
			Reflect.InvokeMethod(this.Method, this.Fixture, arguments);
		}
	}
}
