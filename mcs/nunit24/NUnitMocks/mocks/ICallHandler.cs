// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;

namespace NUnit.Mocks
{
	/// <summary>
	/// The ICallHandler interface dispatches calls to methods or
	/// other objects implementing the ICall interface.
	/// </summary>
	public interface ICallHandler
	{		
		/// <summary>
		/// Simulate a method call on the mocked object.
		/// </summary>
		/// <param name="methodName">The name of the method</param>
		/// <param name="args">Arguments for this call</param>
		/// <returns>Previously specified object or null</returns>
		object Call( string methodName, params object[] args );
	}
}
