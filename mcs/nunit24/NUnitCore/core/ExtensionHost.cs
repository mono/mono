// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;
using NUnit.Core.Extensibility;

namespace NUnit.Core
{
	/// <summary>
	/// ExtensionHost is the abstract base class used for
	/// all extension hosts. It provides an array of 
	/// extension points and a FrameworkRegistry and
	/// implements the IExtensionHost interface. Derived
	/// classes must initialize the extension points.
	/// </summary>
	public abstract class ExtensionHost : IExtensionHost
	{
		protected FrameworkRegistry frameworks;

		protected IExtensionPoint[] extensions;

		protected ExtensionType supportedTypes;

		public ExtensionHost()
		{
			frameworks = new FrameworkRegistry();
		}

		#region IExtensionHost Interface
		public IExtensionPoint[] ExtensionPoints
		{
			get { return extensions; }
		}

		public IFrameworkRegistry FrameworkRegistry
		{
			get { return frameworks; }
		}

		public IExtensionPoint GetExtensionPoint( string name )
		{
			foreach ( IExtensionPoint extensionPoint in extensions )
				if ( extensionPoint.Name == name )
					return extensionPoint;

			return null;
		}

		public ExtensionType ExtensionTypes
		{
			get { return supportedTypes; }
		}
		#endregion
	}
}
