// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;
using System.Reflection;
using NUnit.Core.Extensibility;

namespace NUnit.Core.Extensions
{
	/// <summary>
	/// Summary description for RepeatedTestDecorator.
	/// </summary>
	[NUnitAddin(Description="Runs a test case multiple times")]
	public class RepeatedTestDecorator : ITestDecorator, IAddin
	{
		private static readonly string RepeatAttributeType = "NUnit.Framework.Extensions.RepeatAttribute";

		#region IAddin Members
		public bool Install(IExtensionHost host)
		{
			IExtensionPoint decorators = host.GetExtensionPoint( "TestDecorators" );
			if ( decorators == null )
				return false;
				
			decorators.Install( this );
			return true;
		}
		#endregion

		#region ITestDecorator Members
		public Test Decorate(Test test, MemberInfo member)
		{
			if ( member == null )
				return test;

			TestCase testCase = test as TestCase;
			if ( testCase == null )
				return test;

			Attribute repeatAttr = Reflect.GetAttribute( member, RepeatAttributeType, true );
			if ( repeatAttr == null )
				return test;		

			object propVal = Reflect.GetPropertyValue( repeatAttr, "Count", 
				BindingFlags.Public | BindingFlags.Instance );

			if ( propVal == null )
				return test;

			int count = (int)propVal;

			return new RepeatedTestCase( testCase, count );
		}

//		public Test Decorate( Test test, Type fixtureType )
//		{
//			return test;
//		}
		#endregion
	}
}
