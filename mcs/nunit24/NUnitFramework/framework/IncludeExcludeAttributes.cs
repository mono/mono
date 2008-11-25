// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;

namespace NUnit.Framework
{
	/// <summary>
	/// Abstract base for Attributes that are used to include tests
	/// in the test run based on environmental settings.
	/// </summary>
	public abstract class IncludeExcludeAttribute : Attribute
	{
		private string include;
		private string exclude;
		private string reason;

		/// <summary>
		/// Constructor with no included items specified, for use
		/// with named property syntax.
		/// </summary>
		public IncludeExcludeAttribute() { }

		/// <summary>
		/// Constructor taking one or more included items
		/// </summary>
		/// <param name="include">Comma-delimited list of included items</param>
		public IncludeExcludeAttribute( string include )
		{
			this.include = include;
		}

		/// <summary>
		/// Name of the item that is needed in order for
		/// a test to run. Multiple itemss may be given,
		/// separated by a comma.
		/// </summary>
		public string Include
		{
			get { return this.include; }
			set { include = value; }
		}

		/// <summary>
		/// Name of the item to be excluded. Multiple items
		/// may be given, separated by a comma.
		/// </summary>
		public string Exclude
		{
			get { return this.exclude; }
			set { this.exclude = value; }
		}

		/// <summary>
		/// The reason for including or excluding the test
		/// </summary>
		public string Reason
		{
			get { return reason; }
			set { reason = value; }
		}
	}

	/// <summary>
	/// PlatformAttribute is used to mark a test fixture or an
	/// individual method as applying to a particular platform only.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method|AttributeTargets.Assembly, AllowMultiple=true)]
	public class PlatformAttribute : IncludeExcludeAttribute
	{
		/// <summary>
		/// Constructor with no platforms specified, for use
		/// with named property syntax.
		/// </summary>
		public PlatformAttribute() { }

		/// <summary>
		/// Constructor taking one or more platforms
		/// </summary>
		/// <param name="platforms">Comma-deliminted list of platforms</param>
		public PlatformAttribute( string platforms ) : base( platforms ) { }
	}

	/// <summary>
	/// CultureAttribute is used to mark a test fixture or an
	/// individual method as applying to a particular Culture only.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method|AttributeTargets.Assembly, AllowMultiple=false)]
	public class CultureAttribute : IncludeExcludeAttribute
	{
		/// <summary>
		/// Constructor with no cultures specified, for use
		/// with named property syntax.
		/// </summary>
		public CultureAttribute() { }

		/// <summary>
		/// Constructor taking one or more cultures
		/// </summary>
		/// <param name="cultures">Comma-deliminted list of cultures</param>
		public CultureAttribute( string cultures ) : base( cultures ) { }
	}
}
