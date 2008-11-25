// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;

namespace NUnit.Util
{
	/// <summary>
	/// The TestObserver interface is implemented by a class that
	/// subscribes to the events generated in loading and running
	/// tests and uses that information in some way.
	/// </summary>
	public interface TestObserver
	{
		void Subscribe( ITestEvents events );
	}
}
