// *********************************************************************
// Copyright 2007, Andreas Schlapsi
// This is free software licensed under the MIT license. 
// *********************************************************************
using System;
using System.Reflection;
using NUnit.Core;

namespace NUnit.Core.Extensions.RowTest
{
	public class RowTestFactory
	{
		public RowTestFactory()
		{
		}
		
		public RowTestSuite CreateRowTestSuite(MethodInfo method)
		{
			if (method == null)
				throw new ArgumentNullException("method");
			
			RowTestSuite testSuite = new RowTestSuite(method);
			NUnitFramework.ApplyCommonAttributes(method, testSuite);
			
			return testSuite;
		}
		
		public RowTestCase CreateRowTestCase(Attribute row, MethodInfo method)
		{
			if (row == null)
				throw new ArgumentNullException("row");
			
			if (method == null)
				throw new ArgumentNullException("method");
			
			object[] rowArguments = RowTestFramework.GetRowArguments(row);
			rowArguments = FilterSpecialValues(rowArguments);
			
			string testName = RowTestFramework.GetTestName(row);
			Type expectedExceptionType = RowTestFramework.GetExpectedExceptionType(row);
			
			RowTestCase testCase = new RowTestCase(method, testName, rowArguments);
			if (expectedExceptionType != null)
			{
				testCase.ExceptionExpected = true;
				testCase.ExpectedExceptionType = expectedExceptionType;
				testCase.ExpectedMessage = RowTestFramework.GetExpectedExceptionMessage(row);
			}

			return testCase;
		}
		
		private object[] FilterSpecialValues(object[] arguments)
		{
			if (arguments == null)
				return null;
			
			for (int i = 0; i < arguments.Length; i++)
			{
				if (RowTestFramework.IsSpecialValue(arguments[i]))
					arguments[i] = MapSpecialValue(arguments[i]);
			}
			
			return arguments;
		}
		
		private object MapSpecialValue(object specialValue)
		{
			switch (specialValue.ToString())
			{
				case "Null":
					return null;
				
				default:
					return specialValue;
			}
		}
	}
}
