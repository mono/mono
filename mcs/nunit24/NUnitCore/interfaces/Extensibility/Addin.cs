// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;
using System.Reflection;

namespace NUnit.Core.Extensibility
{
	/// <summary>
	/// The Addin class holds information about an addin.
	/// </summary>
	[Serializable]
	public class Addin
	{
		#region Private Fields
		private string typeName;
		private string name;
		private string description;
		private ExtensionType extensionType;
		private AddinStatus status;
		private string message;
		#endregion

		#region Constructor
		/// <summary>
		/// Construct an Addin for a type.
		/// </summary>
		/// <param name="type">The type to be used</param>
		public Addin( Type type )
		{
			this.typeName = type.AssemblyQualifiedName;

			object[] attrs = type.GetCustomAttributes( typeof(NUnitAddinAttribute), false );
			if ( attrs.Length == 1 )
			{
				NUnitAddinAttribute attr = (NUnitAddinAttribute)attrs[0];
				this.name = attr.Name;
				this.description = attr.Description;
				this.extensionType = attr.Type;
			}

			if ( this.name == null )
				this.name = type.Name;

			if ( this.extensionType == 0 )
				this.extensionType = ExtensionType.Core;

			this.status = AddinStatus.Enabled;
        }
		#endregion

		#region Properties
		/// <summary>
		/// The name of the Addin
		/// </summary>
		public string Name
		{
			get { return name; }
		}

		/// <summary>
		/// Brief description of what the Addin does
		/// </summary>
		public string Description
		{
			get { return description; }
		}

		/// <summary>
		/// The type or types of extension provided, using 
		/// one or more members of the ExtensionType enumeration.
		/// </summary>
		public ExtensionType ExtensionType
		{
			get { return extensionType; }
		}

		/// <summary>
		/// The AssemblyQualifiedName of the type that implements
		/// the addin.
		/// </summary>
		public string TypeName
		{
			get { return typeName; }
		}

		/// <summary>
		/// The status of the addin
		/// </summary>
		public AddinStatus Status
		{
			get { return status; }
			set { status = value; }
		}

		/// <summary>
		/// Any message that clarifies the status of the Addin,
		/// such as an error message or an explanation of why
		/// the addin is disabled.
		/// </summary>
		public string Message
		{
			get { return message; }
			set { message = value; }
		}
		#endregion
	}
}
