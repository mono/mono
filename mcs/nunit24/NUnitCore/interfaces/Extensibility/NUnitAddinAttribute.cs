// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;

namespace NUnit.Core.Extensibility
{
	/// <summary>
	/// NUnitAddinAttribute is used to mark all add-ins. The marked class
	/// must implement the IAddin interface.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
	public sealed class NUnitAddinAttribute : Attribute
	{
        /// <summary>
        /// The name of this addin
        /// </summary>
		public string Name;

        /// <summary>
        /// A description for the addin
        /// </summary>
		public string Description;

        /// <summary>
        /// The type of extension provided
        /// </summary>
		public ExtensionType Type;

        /// <summary>
        /// Default Constructor
        /// </summary>
		public NUnitAddinAttribute()
		{
			this.Type = ExtensionType.Core;
		}
	}
}
