// System.Security.Policy.ApplicationDirectory
//
// Author:
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2001 Jackson Harper, All rights reserved.

using System;

namespace System.Security.Policy {

	[MonoTODO("This class should use a URLString like class instead of just a string")]
	public sealed class ApplicationDirectory {
		
		private string directory;

		//
		// Public Constructors
		//
		
		public ApplicationDirectory(string name)
		{
			if (null == name)
				throw new ArgumentNullException ();		
			directory = name;
		}

		//
		// Public Properties
		//
		
		public string Directory {
			get { return directory;	}
		}
		
		//
		// Public Methods
		//
		
		public object Copy()
		{	
			return new ApplicationDirectory (Directory);
		}
		
		[MonoTODO("This needs to check for security subsets")]
		public override bool Equals(object other)
		{
			if (null != other && (other is ApplicationDirectory)) {
				ApplicationDirectory compare = (ApplicationDirectory)other;
				return compare.directory.Equals(directory);
			}
			return false;
		}
		
		/// <summary>
		///   This does not return the exact same results as the MS version
		/// </summary>
		public override int GetHashCode()
		{
			return directory.GetHashCode ();
		}
		
		public override string ToString()
		{
			return ToXml ().ToString ();
		}

		private SecurityElement ToXml()
		{
			SecurityElement element = new SecurityElement (GetType().FullName);
			element.AddAttribute ("version", "1");
			element.AddAttribute ("Directory", Directory);

			return element;
		}

	}
}

