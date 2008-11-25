// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;

namespace NUnit.Mocks
{
	/// <summary>
	/// The IVerify interface is implemented by objects capable of self-verification.
	/// </summary>
	public interface IVerify
	{
		void Verify();
	}
}
