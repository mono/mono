using System;
using System.Collections;
using NUnit.Core.Extensibility;

namespace NUnit.Core
{
	/// <summary>
	/// ExtensionPoint is used as a base class for all 
	/// extension points.
	/// </summary>
	public abstract class ExtensionPoint : IExtensionPoint
	{
		private string name;
		private IExtensionHost host;

		protected ArrayList extensions = new ArrayList();

		#region Constructor
		public ExtensionPoint(string name, IExtensionHost host)
		{
			this.name = name;
			this.host = host;
		}
		#endregion

		#region IExtensionPoint Members
		/// <summary>
		/// Get the name of this extension point
		/// </summary>
		public string Name
		{
			get { return this.name; }
		}

		/// <summary>
		/// Get the host that provides this extension point
		/// </summary>
		public IExtensionHost Host
		{
			get { return this.host; }
		}

		/// <summary>
		/// Install an extension at this extension point. If the
		/// extension object does not meet the requirements for
		/// this extension point, an exception is thrown.
		/// </summary>
		/// <param name="extension">The extension to install</param>
		public void Install(object extension)
		{
			if ( !ValidExtension( extension ) )
				throw new ArgumentException( 
					extension.GetType().FullName + " is not {0} extension point", "extension" );

			extensions.Add( extension );
		}

		/// <summary>
		/// Removes an extension from this extension point. If the
		/// extension object is not present, the method returns
		/// without error.
		/// </summary>
		/// <param name="extension"></param>
		public void Remove(object extension)
		{
			extensions.Remove( extension );
		}
		#endregion

		#region Abstract Methods
		protected abstract bool ValidExtension(object extension);
		#endregion
	}
}
