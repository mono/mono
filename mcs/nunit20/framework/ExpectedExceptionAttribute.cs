#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
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
' Portions Copyright © 2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright © 2000-2003 Philip A. Craig
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
	/// ExpectedAttributeException.
	/// </summary>
	/// 
	[AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
	public sealed class ExpectedExceptionAttribute : Attribute
	{
		private Type expectedException;
		private string expectedMessage;

		/// <summary>
		/// Constructor for a given type of exception
		/// </summary>
		/// <param name="exceptionType"></param>
		public ExpectedExceptionAttribute(Type exceptionType)
		{
			expectedException = exceptionType;
		}

		/// <summary>
		/// Constructor for a given type of exception and expected message text
		/// </summary>
		/// <param name="exceptionType"></param>
		/// <param name="expectedMessage"></param>
		public ExpectedExceptionAttribute(Type exceptionType, string expectedMessage)
		{
			expectedException = exceptionType;
			this.expectedMessage = expectedMessage;
		}

		/// <summary>
		/// The expected exception type
		/// </summary>
		public Type ExceptionType 
		{
			get{ return expectedException; }
			set{ expectedException = value; }
		}

		/// <summary>
		/// The expected message
		/// </summary>
		public string ExpectedMessage 
		{
			get { return expectedMessage; }
			set { expectedMessage = value; }
		}
	}
}
