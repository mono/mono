#region Copyright (c) 2002, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Philip A. Craig
/************************************************************************************
'
' Copyright © 2002 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov
' Copyright © 2000-2002 Philip A. Craig
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
' Portions Copyright © 2002 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov 
' or Copyright © 2000-2002 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

namespace NUnit.Core
{
	using System;
	using System.IO;
	using System.Reflection;
	using System.Runtime.Remoting;

	/// <summary>
	/// Summary description for RemoteTestRunner.
	/// </summary>
	/// 
	[Serializable]
	public class RemoteTestRunner : LongLivingMarshalByRefObject
	{
		private TestSuite suite;
		private string fullName;
		private string assemblyName;

		public void Initialize(string assemblyName)
		{
			this.assemblyName = assemblyName;
		}

		public void Initialize(string fixtureName, string assemblyName)
		{
			TestName = fixtureName;
			Initialize(assemblyName);
		}

		public void BuildSuite() 
		{
			TestSuiteBuilder builder = new TestSuiteBuilder();
			if(fullName == null) 
				suite = builder.Build(assemblyName);
			else
				suite = builder.Build(fullName, assemblyName);

			if(suite != null) TestName = suite.FullName;
		}

		public TestResult Run(NUnit.Core.EventListener listener, TextWriter outText, TextWriter errorText)
		{
			Console.SetOut(new StringTextWriter(outText));
			Console.SetError(new StringTextWriter(errorText));

			Test test = FindByName(suite, fullName);

			TestResult result = test.Run(listener);

			return result;
		}

		/// <summary>
		/// Use this wrapper to ensure that only strings get passed accross the AppDomain
		/// boundry.  Otherwise tests will break when non-remotable objecs are passed to
		/// Console.Write/WriteLine.
		/// </summary>
		private class StringTextWriter : TextWriter
		{
			public StringTextWriter(TextWriter aTextWriter)
			{
				theTextWriter = aTextWriter;
			}
			private TextWriter theTextWriter;

			override public void Write(char aChar)
			{
				theTextWriter.Write(aChar);
			}

			override public void Write(string aString)
			{
				theTextWriter.Write(aString);
			}

			override public void WriteLine(string aString)
			{
				theTextWriter.WriteLine(aString);
			}

			override public System.Text.Encoding Encoding
			{
				get { return theTextWriter.Encoding; }
			}
		}

		private Test FindByName(Test test, string fullName)
		{
			if(test.FullName.Equals(fullName)) return test;
			
			Test result = null;
			if(test is TestSuite)
			{
				TestSuite suite = (TestSuite)test;
				foreach(Test testCase in suite.Tests)
				{
					result = FindByName(testCase, fullName);
					if(result != null) break;
				}
			}

			return result;
		}

		public string TestName 
		{
			get { return fullName; }
			set { fullName = value; }
		}
			
		public Test Test
		{
			get 
			{ return suite; }
		}
	}
}

