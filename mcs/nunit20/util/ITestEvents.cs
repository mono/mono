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
using NUnit.Core;

//TODO: rename this file
namespace NUnit.Util
{
	/// <summary>
	/// IProjectEvents interface extends NUnit.Core.ITestEvents adding
	/// events that are fired as projects are loaded and unloaded.
	/// </summary>
	public interface IProjectEvents : ITestEvents
	{
		// Events related to the loading and unloading
		// of projects - including wrapper projects
		// created in order to load assemblies. This
		// occurs separately from the loading of tests
		// for the assemblies in the project.
		event TestProjectEventHandler ProjectLoading;
		event TestProjectEventHandler ProjectLoaded;
		event TestProjectEventHandler ProjectLoadFailed;
		event TestProjectEventHandler ProjectUnloading;
		event TestProjectEventHandler ProjectUnloaded;
		event TestProjectEventHandler ProjectUnloadFailed;
	}
}
