// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;

namespace NUnit.Mocks
{
	/// <summary>
	/// The ICall interface is implemented by objects that can be called
	/// with an array of arguments and return a value.
	/// </summary>
	public interface ICall
	{
		/// <summary>
		/// Process a call with a possibly empty set of arguments.
		/// </summary>
		/// <param name="args">Arguments for this call</param>
		/// <returns>An implementation-defined return value</returns>
		object Call( object[] args );
	}
}
