// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;

namespace NUnit.Core.Extensibility
{
	/// <summary>
	/// The ExtensionType enumeration is used to indicate the
	/// kinds of extensions provided by an Addin. The addin
	/// is only installed by hosts supporting one of its
	/// extension types.
	/// </summary>
	[Flags]
	public enum ExtensionType
	{
		/// <summary>
		/// A Core extension is installed by the CoreExtensions
		/// host in each test domain.
		/// </summary>
		Core=1,

		/// <summary>
		/// A Client extension is installed by all clients
		/// </summary>
		Client=2,

		/// <summary>
		/// A Gui extension is installed by the gui client
		/// </summary>
		Gui=4
	}
}
