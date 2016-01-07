// ***********************************************************************
// Copyright (c) 2008 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using NUnit.Framework;

namespace NUnit.TestData.AttributeInheritanceData
{
	// Sample Test from a post by Scott Bellware

	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
	class ConcernAttribute : TestFixtureAttribute
	{
		private Type typeOfConcern;

		public ConcernAttribute( Type typeOfConcern )
		{
			this.typeOfConcern = typeOfConcern;
		}
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
	class SpecAttribute : TestAttribute
	{
	}

	/// <summary>
	/// Summary description for AttributeInheritance.
	/// </summary>
	[Concern(typeof(ClassUnderTest))]
	public class When_collecting_test_fixtures
	{
		[Spec]
		public void should_include_classes_with_an_attribute_derived_from_TestFixtureAttribute()
		{
		}
	}

    class ClassUnderTest { }
}
