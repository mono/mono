// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;

namespace NUnit.Mocks
{
	/// <summary>
	/// The IMethod interface represents an method or other named object that 
	/// is both callable and self-verifying.
	/// </summary>
	public interface IMethod : IVerify, ICall
	{
		/// <summary>
		/// The name of the object
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Tell the object to expect a certain call.
		/// </summary>
		/// <param name="call"></param>
		void Expect( ICall call );
	}
}
