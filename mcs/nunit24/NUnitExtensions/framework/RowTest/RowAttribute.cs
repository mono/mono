// *********************************************************************
// Copyright 2007, Andreas Schlapsi
// This is free software licensed under the MIT license. 
// *********************************************************************
using System;

namespace NUnit.Framework.Extensions
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public sealed class RowAttribute : Attribute
	{
		private string _testName;
		private object[] _arguments;
		private string _description;
		private Type _expectedExceptionType;
		private string _exceptionMessage;
		
		public RowAttribute(object argument1)
		{
			_arguments = new object[] { argument1 };
		}
		
		public RowAttribute(object argument1, object argument2)
		{
			_arguments = new object[] { argument1, argument2 };
		}
		
		public RowAttribute(object argument1, object argument2, object argument3)
		{
			_arguments = new object[] { argument1, argument2, argument3 };
		}
		
		public RowAttribute(params object[] arguments)
		{
			_arguments = arguments;
		}
		
		public string TestName
		{
			get { return _testName; }
			set { _testName = value; }
		}
		
		public object[] Arguments
		{
			get { return _arguments; }
		}
		
		public string Description
		{
			get { return _description; }
			set { _description = value; }
		}
		
		public Type ExpectedException
		{
			get { return _expectedExceptionType; }
			set { _expectedExceptionType = value; }
		}
		
		public string ExceptionMessage
		{
			get { return _exceptionMessage; }
			set { _exceptionMessage = value; }
		}
	}
}
