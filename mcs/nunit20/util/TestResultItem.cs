#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright  2000-2002 Philip A. Craig
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
' Portions Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright  2000-2002 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

using System;

namespace NUnit.Util
{
	using NUnit.Core;

	/// <summary>
	/// Summary description for TestResultItem.
	/// </summary>
	public class TestResultItem
	{
		private string testName;
		private string message;
		private string stackTrace;

		public TestResultItem(TestResult result )
		{
			testName = result.Name;
			message = result.Message;
			stackTrace = result.StackTrace;
		}

		public override string ToString()
		{
			return String.Format("{0} : {1}", testName, message);
		}

		public string GetMessage()
		{
			return ToString();
		}

        public string GetToolTipMessage()   //NRG 05/28/03 - Substitute spaces for tab characters
        {
            return (ReplaceTabs(GetMessage(), 8)); // Change each tab to 8 space characters
        }

        public string ReplaceTabs(string strOriginal, int nSpaces)  //NRG 05/28/03
        {
            string strSpaces = string.Empty;
            strSpaces = strSpaces.PadRight(nSpaces, ' ');
            return(strOriginal.Replace("\t", strSpaces));
        }

		public string StackTrace
		{
			get 
			{
				string trace = "No stack trace is available";
				if(stackTrace != null)
					trace = StackTraceFilter.Filter(stackTrace);

				return stackTrace;
			}
		}
	}
}
