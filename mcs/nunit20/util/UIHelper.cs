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

namespace NUnit.Util
{
	using System;
	using NUnit.Core;

	/// <summary>
	/// Summary description for UIHelper.
	/// </summary>
	public class UIHelper
	{
		private static bool AreNodesTheSame(Test testOne, Test testTwo)
		{
			if(testOne==null && testTwo!=null) return false;
			if(testTwo==null && testOne!=null) return false;
			if(testOne.GetType().FullName != testTwo.GetType().FullName) return false;
			if(testOne.ShouldRun ^ testTwo.ShouldRun) return false;
			return testOne.FullName.Equals(testTwo.FullName);
		}

		public static bool CompareTree(Test rootTestOriginal, Test rootTestNew)
		{
			if(!AreNodesTheSame(rootTestOriginal,rootTestNew)) return false;
			if((rootTestOriginal is TestSuite) && (rootTestNew is TestSuite))
			{
				TestSuite originalSuite = (TestSuite)rootTestOriginal;
				TestSuite newSuite = (TestSuite)rootTestNew;
				int originalCount = originalSuite.Tests.Count;
				int newCount = newSuite.Tests.Count;
				if(originalCount!=newCount)
				{
					return false;
				}
				for(int i=0; i<originalSuite.Tests.Count;i++)
				{
					if(!CompareTree((Test)originalSuite.Tests[i],(Test)newSuite.Tests[i])) return false;
				}
			}
			return true;
		}

		private static bool AreNodesTheSame( UITestNode testOne, UITestNode testTwo )
		{
			if( testOne == null && testTwo != null ) return false;
			if( testTwo == null && testOne != null ) return false;
			if( testOne.IsSuite != testTwo.IsSuite ) return false;
			if( testOne.ShouldRun != testTwo.ShouldRun ) return false;

			return testOne.FullName.Equals(testTwo.FullName);
		}

		public static bool CompareTree( UITestNode rootTestOriginal, UITestNode rootTestNew )
		{
			if( !AreNodesTheSame( rootTestOriginal, rootTestNew ) ) 
				return false;

			if( rootTestOriginal.IsSuite && rootTestNew.IsSuite )
			{
				if( rootTestOriginal.Tests.Count != rootTestNew.Tests.Count )
					return false;

				for(int i=0; i< rootTestOriginal.Tests.Count; i++)
					if( !CompareTree( (UITestNode)rootTestOriginal.Tests[i], (UITestNode)rootTestNew.Tests[i] ) ) 
						return false;
			}

			return true;
		}
	}
}
