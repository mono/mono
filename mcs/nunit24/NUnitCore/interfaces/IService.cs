// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;

namespace NUnit.Core
{
	/// <summary>
	/// The IService interface is implemented by all Services.
	/// </summary>
	public interface IService
	{
		/// <summary>
		/// Initialize the Service
		/// </summary>
		void InitializeService();

		/// <summary>
		/// Do any cleanup needed before terminating the service
		/// </summary>
		void UnloadService();
	}
}
