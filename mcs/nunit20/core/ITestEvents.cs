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

using System;

namespace NUnit.Core
{
	/// <summary>
	/// Defines events related to the loading of tests.
	/// </summary>
	public interface ILoadEvents
	{
		event TestEventHandler TestLoading;
		event TestEventHandler TestLoaded;
		event TestEventHandler TestLoadFailed;
		
		event TestEventHandler TestReloading;
		event TestEventHandler TestReloaded;
		event TestEventHandler TestReloadFailed;
		
		event TestEventHandler TestUnloading;
		event TestEventHandler TestUnloaded;
		event TestEventHandler TestUnloadFailed;
	}

	/// <summary>
	/// Defines events related to the running of tests.
	/// </summary>
	public interface IRunEvents
	{
		// Events related to a running a set of tests
		event TestEventHandler RunStarting;	
		event TestEventHandler RunFinished;

		// Events that arise while a test is running
		// These are translated from calls to the runner on the
		// EventListener interface.
		event TestEventHandler SuiteStarting;
		event TestEventHandler SuiteFinished;
		event TestEventHandler TestStarting;
		event TestEventHandler TestFinished;

		/// <summary>
		/// An unhandled exception was thrown during a test run,
		/// and it cannot be associated with a particular test failure.
		/// </summary>
		event TestEventHandler TestException;
	}

	/// <summary>
	///  The combined interface, typically implemented by test runners.
	/// </summary>
	public interface ITestEvents : ILoadEvents, IRunEvents
	{
	}
}
