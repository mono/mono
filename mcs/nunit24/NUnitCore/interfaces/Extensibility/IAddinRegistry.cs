// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;

namespace NUnit.Core.Extensibility
{
	/// <summary>
	/// The IAddinRegistry interface allows registering addins
	/// and retrieving information about them. It is also used
	///  to record the load status of an addin.
	/// </summary>
	public interface IAddinRegistry
	{
		/// <summary>
		/// Gets a list of all addins as Addin objects
		/// </summary>
		System.Collections.IList Addins { get; }

		/// <summary>
		/// Registers an addin
		/// </summary>
		/// <param name="addin">The addin to be registered</param>
		void Register( Addin addin );

		/// <summary>
		///  Sets the load status of an addin
		/// </summary>
		/// <param name="name">The name of the addin</param>
		/// <param name="status">The status to be set</param>
		/// <param name="message">An optional message explaining the status</param>
		void SetStatus( string name, AddinStatus status, string message );
	}
}
