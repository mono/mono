// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;

namespace NUnit.Core
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
	public class TestBuilderAttribute : Attribute
	{
		private Type builderType;

		public TestBuilderAttribute(Type builderType)
		{
			this.builderType = builderType;
		}

		public Type BuilderType
		{
			get { return builderType; }
		}
	}
}
