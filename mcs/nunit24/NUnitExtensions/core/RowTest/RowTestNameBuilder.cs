// *********************************************************************
// Copyright 2007, Andreas Schlapsi
// This is free software licensed under the MIT license. 
// *********************************************************************
using System;
using System.Reflection;
using System.Text;

namespace NUnit.Core.Extensions.RowTest
{
	public class RowTestNameBuilder
	{
		private MethodInfo _method;
		private string _baseTestName;
		private object[] _arguments;
		private string _argumentList;

		public RowTestNameBuilder(MethodInfo method, string baseTestName, object[] arguments)
		{
			_method = method;
			_baseTestName = baseTestName;
			_arguments = arguments;
		}
		
		public MethodInfo Method
		{
			get { return _method; }
		}
		
		public string BaseTestName
		{
			get { return _baseTestName; }
		}
		
		public object[] Arguments
		{
			get { return _arguments; }
		}
		
		public string TestName
		{
			get 
			{
				string baseTestName = _baseTestName;
				
				if (baseTestName == null || baseTestName.Length == 0)
					baseTestName = _method.Name;
					
				return baseTestName + GetArgumentList();
			}
		}
		
		public string FullTestName
		{
			get { return _method.DeclaringType.FullName + "." + TestName; }
		}
		
		private string GetArgumentList()
		{
			if (_argumentList == null)
				_argumentList = "(" + CreateArgumentList() + ")";
			
			return _argumentList;
		}
		
		private string CreateArgumentList()
		{
			if (_arguments == null)
				return "null";
			
			StringBuilder argumentListBuilder = new StringBuilder();

			for (int i = 0; i < _arguments.Length; i++)
			{
				if (i > 0)
					argumentListBuilder.Append(", ");
				
				argumentListBuilder.Append (GetArgumentString (_arguments[i]));
			}
			
			return argumentListBuilder.ToString();
		}
		
		private string GetArgumentString (object argument)
		{
			if (argument == null)
				return "null";
			
			return argument.ToString();
		}
	}
}
