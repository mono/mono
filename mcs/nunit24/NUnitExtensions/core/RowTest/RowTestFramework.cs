// *********************************************************************
// Copyright 2007, Andreas Schlapsi
// This is free software licensed under the MIT license. 
// *********************************************************************
using System;
using System.Reflection;
using NUnit.Core;

namespace NUnit.Core.Extensions.RowTest
{
	public sealed class RowTestFramework
	{
		public const string RowTestAttribute = "NUnit.Framework.Extensions.RowTestAttribute";
		public const string RowAttribute = "NUnit.Framework.Extensions.RowAttribute";
		public const string SpecialValueEnum = "NUnit.Framework.Extensions.SpecialValue";
		
		private RowTestFramework()
		{
		}
		
		public static bool IsRowTest(MethodInfo method)
		{
			if (method == null)
				return false;
			
			return Reflect.HasAttribute(method, RowTestAttribute, false);;
		}
		
		public static Attribute[] GetRowAttributes(MethodInfo method)
		{
			if (method == null)
				throw new ArgumentNullException("method");
			
			return Reflect.GetAttributes(method, RowAttribute, false);
		}
		
		public static object[] GetRowArguments(Attribute attribute)
		{
			return Reflect.GetPropertyValue(attribute, "Arguments") as object[];
		}
		
		public static bool IsSpecialValue(object argument)
		{
			if (argument == null)
				return false;
			
			return argument.GetType().FullName == SpecialValueEnum;
		}
		
		public static Type GetExpectedExceptionType(Attribute attribute)
		{
			return Reflect.GetPropertyValue(attribute, "ExpectedException") as Type;
		}
		
		public static string GetExpectedExceptionMessage(Attribute attribute)
		{
			return Reflect.GetPropertyValue(attribute, "ExceptionMessage") as string;
		}
		
		public static string GetTestName(Attribute attribute)
		{
			return Reflect.GetPropertyValue(attribute, "TestName") as string;
		}
	}
}
