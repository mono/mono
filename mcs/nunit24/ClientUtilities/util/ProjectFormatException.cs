// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;

namespace NUnit.Util
{
	/// <summary>
	/// Exception raised when loading a project file with
	/// an invalid format.
	/// </summary>
	public class ProjectFormatException : ApplicationException
	{
		#region Instance Variables

		private int lineNumber;

		private int linePosition;

		#endregion

		#region Constructors

		public ProjectFormatException() : base() {}

		public ProjectFormatException( string message )
			: base( message ) {}

		public ProjectFormatException( string message, Exception inner )
			: base( message, inner ) {}

		public ProjectFormatException( string message, int lineNumber, int linePosition )
			: base( message )
		{
			this.lineNumber = lineNumber;
			this.linePosition = linePosition;
		}

		#endregion

		#region Properties

		public int LineNumber
		{
			get { return lineNumber; }
		}

		public int LinePosition
		{
			get { return linePosition; }
		}

		#endregion
	}
}
