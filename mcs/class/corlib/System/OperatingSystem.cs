//------------------------------------------------------------------------------
// 
// System.OperatingSystem.cs 
//
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// 
// Author:         Jim Richardson, develop@wtfo-guru.com
// Created:        Saturday, August 11, 2001 
//
//------------------------------------------------------------------------------

using System;
using System.Globalization;

namespace System
{
	/// <summary>
	/// Class representing a specific operating system version for a specific platform
	/// </summary>
	[Serializable]
	public sealed class OperatingSystem : ICloneable
	{
		private System.PlatformID itsPlatform;
		private Version itsVersion;

		public OperatingSystem(PlatformID platform, Version version)
		{
			if(version == null)
			{
				throw new ArgumentNullException();
			}
			
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
		{	// this leave us enuf for 256 unique platforms which should suffice for a good while
			return ((int)itsPlatform << 24) | itsVersion.GetHashCode() >> 8;
		}

		/// <summary>
		/// Return a string reprentation of this instance
		/// </summary>
		public override string ToString()
		{
			string str;
			
			switch(itsPlatform)
			{
			case System.PlatformID.Win32NT: str = "Microsoft Windows NT"; break;
			case System.PlatformID.Win32S: str = "Microsoft Win32S";  break;
			case System.PlatformID.Win32Windows: str = "Microsoft Windows 98"; break;
			case System.PlatformID.Unix: str = "Unix"; break;
			default: str = Locale.GetText ("<unknown>"); break;
			}

			return str + " " + itsVersion.ToString();
		}
	}
}
