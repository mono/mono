// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;

namespace NUnit.Core.Extensibility
{
	/// <summary>
	/// Add-ins are used to extend NUnti. All add-ins must
	/// implement the IAddin interface.
	/// </summary>
	public interface IAddin
	{
		/// <summary>
		/// When called, the add-in installs itself into
		/// the host, if possible. Because NUnit uses separate
		/// hosts for the client and test domain environments,
		/// an add-in may be invited to istall itself more than
		/// once. The add-in is responsible for checking which
		/// extension points are supported by the host that is
		/// passed to it and taking the appropriate action.
		/// </summary>
		/// <param name="host">The host in which to install the add-in</param>
		/// <returns>True if the add-in was installed, otehrwise false</returns>
		bool Install( IExtensionHost host );
	}
}
