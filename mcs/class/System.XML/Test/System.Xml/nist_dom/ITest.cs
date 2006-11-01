//**************************************************************************
//
//
//                       National Institute Of Standards and Technology
//                                     DTS Version 1.0
//         
//
//
// Ported to System.Xml by: Mizrahi Rafael rafim@mainsoft.com
// Mainsoft Corporation (c) 2003-2004
//
//**************************************************************************
using System;

namespace nist_dom
{
	/// <summary>
	/// Summary description for ITest.
	/// </summary>
	public interface ITest
	{
        testResults[] RunTests();
	}
}
