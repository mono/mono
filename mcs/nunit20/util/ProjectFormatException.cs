#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright  2000-2002 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright  2000-2002 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

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
