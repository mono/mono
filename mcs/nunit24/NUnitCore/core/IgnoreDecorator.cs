// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;
using System.Reflection;

namespace NUnit.Core
{
	/// <summary>
	/// Ignore Decorator is an alternative method of marking tests to
	/// be ignored. It is currently not used, since the test builders
	/// take care of the ignore attribute.
	/// </summary>
	public class IgnoreDecorator : Extensibility.ITestDecorator
	{
		public IgnoreDecorator( string ignoreAttributeType )
		{
		}

		#region ITestDecorator Members

//		public Test Decorate(Test test, MethodInfo method)
//		{
//			return DecorateTest( test, method );
//		}
//
//		public Test Decorate(Test test, Type fixtureType)
//		{
//			return DecorateTest( test, fixtureType );
//		}

		public Test Decorate( Test test, MemberInfo member )
		{
			Attribute ignoreAttribute = Reflect.GetAttribute( member, NUnitFramework.IgnoreAttribute, false );

			if ( ignoreAttribute != null )
			{
				test.RunState = RunState.Ignored;
				test.IgnoreReason = NUnitFramework.GetIgnoreReason( ignoreAttribute );
			}

			return test;
		}

		#endregion
	}
}
