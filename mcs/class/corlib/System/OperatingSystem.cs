//------------------------------------------------------------------------------
// 
// System.Environment.cs 
//
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// 
// Author:         Jim Richardson, develop@wtfo-guru.com
// Created:        Saturday, August 11, 2001 
//
//------------------------------------------------------------------------------

using System;

namespace Mono.System
{
	// this seemed like a logical place to put this enumeration
	public enum PlatformID
	{	// TODO: determine what definitions to incorporate
		//       possibilities are quite varied
		minPlatform,
		i386Linux = minPlatform,
		i686Linux,
		maxPlatform
	}

	/// <summary>
	/// Class representing a specific operating system version for a specific platform
	/// </summary>
	public sealed class OperatingSystem : ICloneable
	{
		private PlatformID itsPlatform;
		private Version itsVersion;

		public OperatingSystem(PlatformID platform, Version version)
		{
			if(version == null)
			{
				throw new ArgumentNullException();
			}
			
			// the doc doesn't say this, but I would
			//if(platform < minPlatform || platform >= maxPlatform)
			//{
				// throw new ArgumentOutOfRangeException();
				// TODO: find out if C# has assertion mechanism
				//       isn't learning new languages fun? :)
			//}

			itsPlatform = platform;
			itsVersion = version;
		}

		/// <summary>
		/// Get the PlatformID
		/// </summary>
		public PlatformID Platform
		{
			get
			{
				return itsPlatform;
			}
		}

		/// <summary>
		/// Gets the version object
		/// </summary>
		public Version Version
		{
			get
			{
				return itsVersion;
			}
		}

		/// <summary>
		/// Return a clone of this object
		/// </summary>
		public object Clone()
		{
			return new OperatingSystem(itsPlatform, itsVersion);
		}

		/// <summary>
		/// Return true if obj equals this object
		/// </summary>
		public override bool Equals(object obj)
		{
			//Check for null and compare run-time types.
			if (obj == null || GetType() != obj.GetType()) return false;
			OperatingSystem os = (OperatingSystem)obj;
			return (itsPlatform == os.itsPlatform) && 
				(os.itsVersion.Equals(itsVersion));
		}

		/// <summary>
		/// Return hash code
		/// </summary>
		public override int GetHashCode()
		{
			return ((int)itsPlatform << 24) | itsVersion.GetHashCode() >> 8;
		}

		/// <summary>
		/// Return a string reprentation of this instance
		/// </summary>
		public override string ToString()
		{
			return itsPlatform.ToString() + ", " + itsVersion.ToString();
		}
	}
}
