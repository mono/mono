#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
/************************************************************************************
'
' Copyright © 2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright © 2000-2003 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright © 2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, 
' Charlie Poole or Copyright © 2000-2003 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

namespace NUnit.Framework
{
	using System;

	/// <summary>
	/// Adding this attribute to a method within a <seealso cref="TestFixtureAttribute"/> 
	/// class makes the method callable from the NUnit test runner. There is a property 
	/// called Description which is optional which you can provide a more detailed test
	/// description. This class cannot be inherited.
	/// </summary>
	/// 
	/// <example>
	/// [TestFixture]
	/// public class Fixture
	/// {
	///   [Test]
	///   public void MethodToTest()
	///   {}
	///   
	///   [Test(Description = "more detailed description")]
	///   publc void TestDescriptionMethod()
	///   {}
	/// }
	/// </example>
	/// 
	[AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
	public sealed class TestAttribute : Attribute
	{
		private string description;

		/// <summary>
		/// Descriptive text for this test
		/// </summary>
		public string Description
		{
			get { return description; }
			set { description = value; }
		}
	}
}
