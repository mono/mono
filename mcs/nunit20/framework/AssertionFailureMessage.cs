#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig, Douglas de la Torre
/************************************************************************************
'
' Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright  2000-2002 Philip A. Craig
' Copyright  2001 Douglas de la Torre
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
' Portions Copyright  2002 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov 
' Copyright  2000-2002 Philip A. Craig, or Copyright  2001 Douglas de la Torre
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

using System;
using System.Text;

namespace NUnit.Framework
{
	/// <summary>
	/// Summary description for AssertionFailureMessage.
	/// </summary>
	public class AssertionFailureMessage
	{
		/// <summary>
		/// Protected constructor, used since this class is only used via
		/// static methods
		/// </summary>
		protected AssertionFailureMessage() 
		{}

		/// <summary>
		/// Number of characters before a highlighted position before
		/// clipping will occur.  Clipped text is replaced with an
		/// elipses "..."
		/// </summary>
		static protected int PreClipLength
		{
			get
			{
				return 35;
			}
		}

		/// <summary>
		/// Number of characters after a highlighted position before
		/// clipping will occur.  Clipped text is replaced with an
		/// elipses "..."
		/// </summary>
		static protected int PostClipLength
		{
			get
			{
				return 35;
			}
		}   

		/// <summary>
		/// Called to test if the position will cause clipping
		/// to occur in the early part of a string.
		/// </summary>
		/// <param name="iPosition"></param>
		/// <returns></returns>
		static private bool IsPreClipped( int iPosition )
		{
			if( iPosition > PreClipLength )
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Called to test if the position will cause clipping
		/// to occur in the later part of a string past the
		/// specified position.
		/// </summary>
		/// <param name="sString"></param>
		/// <param name="iPosition"></param>
		/// <returns></returns>
		static private bool IsPostClipped( string sString, int iPosition )
		{
			if( sString.Length - iPosition > PostClipLength )
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Property called to insert newline characters into a string
		/// </summary>
		static private string NewLine
		{
			get
			{
				return "\r\n\t";
			}
		}

		/// <summary>
		/// Renders up to M characters before, and up to N characters after
		/// the specified index position.  If leading or trailing text is
		/// clipped, and elipses "..." is added where the missing text would
		/// be.
		/// 
		/// Clips strings to limit previous or post newline characters,
		/// since these mess up the comparison
		/// </summary>
		/// <param name="sString"></param>
		/// <param name="iPosition"></param>
		/// <returns></returns>
		static protected string ClipAroundPosition( string sString, int iPosition )
		{
			if( null == sString || 0 == sString.Length )
			{
				return "";
			}

			return BuildBefore( sString, iPosition ) + BuildAfter(  sString, iPosition );
		}

		/// <summary>
		/// Clips the string before the specified position, and appends
		/// ellipses (...) to show that clipping has occurred 
		/// </summary>
		/// <param name="sString"></param>
		/// <param name="iPosition"></param>
		/// <returns></returns>
		static protected string PreClip( string sString, int iPosition )
		{
			return "..." + sString.Substring( iPosition - PreClipLength, PreClipLength );
		}

		/// <summary>
		/// Clips the string after the specified position, and appends
		/// ellipses (...) to show that clipping has occurred 
		/// </summary>
		/// <param name="sString"></param>
		/// <param name="iPosition"></param>
		/// <returns></returns>
		static protected string PostClip( string sString, int iPosition )
		{
			return sString.Substring( iPosition, PostClipLength ) + "...";
		}

		/// <summary>
		/// Builds the first half of a string, limiting the number of
		/// characters before the position, and removing newline
		/// characters.  If the leading string is truncated, the
		/// ellipses (...) characters are appened.
		/// </summary>
		/// <param name="sString"></param>
		/// <param name="iPosition"></param>
		/// <returns></returns>
		static private string BuildBefore( string sString, int iPosition )
		{
			if( IsPreClipped(iPosition) )
			{
				return PreClip( sString, iPosition );
			}
			return sString.Substring( 0, iPosition );
		}

		/// <summary>
		/// Builds the last half of a string, limiting the number of
		/// characters after the position, and removing newline
		/// characters.  If the string is truncated, the
		/// ellipses (...) characters are appened.
		/// </summary>
		/// <param name="sString"></param>
		/// <param name="iPosition"></param>
		/// <returns></returns>
		static private string BuildAfter( string sString, int iPosition )
		{
			if( IsPostClipped(sString, iPosition) )
			{
				return PostClip( sString, iPosition );
			}
			return sString.Substring( iPosition );
		}

		/// <summary>
		/// Text that is rendered for the expected value
		/// </summary>
		/// <returns></returns>
		static protected string ExpectedText()
		{
			return "expected:<";
		}

		/// <summary>
		/// Text rendered for the actual value.  This text should
		/// be the same length as the Expected text, so leading
		/// spaces should pad this string to ensure they match.
		/// </summary>
		/// <returns></returns>
		static protected string ButWasText()
		{
			return " but was:<";
		}

		/// <summary>
		/// Raw line that communicates the expected value, and the actual value
		/// </summary>
		/// <param name="sbOutput"></param>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		static protected void AppendExpectedAndActual( StringBuilder sbOutput, Object expected, Object actual )
		{
			sbOutput.Append( NewLine );
			sbOutput.Append( ExpectedText() );
			sbOutput.Append( DisplayString( expected ) );
			sbOutput.Append( ">" );
			sbOutput.Append( NewLine );
			sbOutput.Append( ButWasText() );
			sbOutput.Append( DisplayString( actual ) );
			sbOutput.Append( ">" );
		}

		/// <summary>
		/// Display an object as a string
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		static protected string DisplayString( object  obj )
		{
			if ( obj == null ) 
				return "(null)";
			else if ( obj is string )
				return Quoted( (string)obj );
			else
				return obj.ToString();
		}

		/// <summary>
		/// Quote a string
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		static protected string Quoted( string text )
		{
			return string.Format( "\"{0}\"", text );
		}

		/// <summary>
		/// Draws a marker under the expected/actual strings that highlights
		/// where in the string a mismatch occurred.
		/// </summary>
		/// <param name="sbOutput"></param>
		/// <param name="iPosition"></param>
		static protected void AppendPositionMarker( StringBuilder sbOutput, int iPosition )
		{
			sbOutput.Append( new String( '-', ButWasText().Length + 1 ) );
			if( iPosition > 0 )
			{
				sbOutput.Append( new string( '-', iPosition ) );
			}
			sbOutput.Append( "^" );
		}

		/// <summary>
		/// Tests two objects to determine if they are strings.
		/// </summary>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		/// <returns></returns>
		static protected bool InputsAreStrings( Object expected, Object actual )
		{
			if( null != expected  &&
				null != actual    &&
				expected is string &&
				actual   is string )
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Tests if two strings are different lengths.
		/// </summary>
		/// <param name="sExpected"></param>
		/// <param name="sActual"></param>
		/// <returns>True if string lengths are different</returns>
		static protected bool LengthsDifferent( string sExpected, string sActual )
		{
			if( sExpected.Length != sActual.Length )
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Tests if two arrays are different lengths.
		/// </summary>
		/// <param name="sExpected"></param>
		/// <param name="sActual"></param>
		/// <returns>True if array lengths are different</returns>
		static protected bool LengthsDifferent( object[] expected, object[] actual )
		{
			if( expected.Length != actual.Length )
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Used to construct a message when the lengths of two strings are
		/// different.  Also includes the strings themselves, to allow them
		/// to be compared visually.
		/// </summary>
		/// <param name="sbOutput"></param>
		/// <param name="sExpected"></param>
		/// <param name="sActual"></param>
		static protected void BuildLengthsDifferentMessage( StringBuilder sbOutput, string sExpected, string sActual )
		{
			BuildContentDifferentMessage( sbOutput, sExpected, sActual );
		}

		/// <summary>
		/// Reports the length of two strings that are different lengths
		/// </summary>
		/// <param name="sbOutput"></param>
		/// <param name="sExpected"></param>
		/// <param name="sActual"></param>
		static protected void BuildStringLengthDifferentReport( StringBuilder sbOutput, string sExpected, string sActual )
		{
			sbOutput.Append( "String lengths differ.  Expected length=" );
			sbOutput.Append( sExpected.Length );
			sbOutput.Append( ", but was length=" );
			sbOutput.Append( sActual.Length );
			sbOutput.Append( "." );
			sbOutput.Append( NewLine );
		}

		/// <summary>
		/// Reports the length of two strings that are the same length
		/// </summary>
		/// <param name="sbOutput"></param>
		/// <param name="sExpected"></param>
		/// <param name="sActual"></param>
		static protected void BuildStringLengthSameReport(  StringBuilder sbOutput, string sExpected, string sActual )
		{
			sbOutput.Append( "String lengths are both " );
			sbOutput.Append( sExpected.Length );
			sbOutput.Append( "." );
			sbOutput.Append( NewLine );
		}

		/// <summary>
		/// Reports whether the string lengths are the same or different, and
		/// what the string lengths are.
		/// </summary>
		/// <param name="sbOutput"></param>
		/// <param name="sExpected"></param>
		/// <param name="sActual"></param>
		static protected void BuildStringLengthReport( StringBuilder sbOutput, string sExpected, string sActual )
		{
			if( sExpected.Length != sActual.Length )
			{
				BuildStringLengthDifferentReport( sbOutput, sExpected, sActual );
			}
			else
			{
				BuildStringLengthSameReport( sbOutput, sExpected, sActual );
			}
		}

		/// <summary>
		/// Reports the length of two arrays that are different lengths
		/// </summary>
		/// <param name="sbOutput"></param>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		static protected void BuildArrayLengthDifferentReport( StringBuilder sbOutput, Array expected, Array actual )
		{
			sbOutput.Append( "Array lengths differ.  Expected length=" );
			sbOutput.Append( expected.Length );
			sbOutput.Append( ", but was length=" );
			sbOutput.Append( actual.Length );
			sbOutput.Append( "." );
			sbOutput.Append( NewLine );
		}

		/// <summary>
		/// Reports the length of two arrays that are the same length
		/// </summary>
		/// <param name="sbOutput"></param>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		static protected void BuildArrayLengthSameReport(  StringBuilder sbOutput, Array expected, Array actual )
		{
			sbOutput.Append( "Array lengths are both " );
			sbOutput.Append( expected.Length );
			sbOutput.Append( "." );
			sbOutput.Append( NewLine );
		}

		/// <summary>
		/// Reports whether the array lengths are the same or different, and
		/// what the array lengths are.
		/// </summary>
		/// <param name="sbOutput"></param>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		static protected void BuildArrayLengthReport( StringBuilder sbOutput, Array expected, Array actual )
		{
			if( expected.Length != actual.Length )
			{
				BuildArrayLengthDifferentReport( sbOutput, expected, actual );
			}
			else
			{
				BuildArrayLengthSameReport( sbOutput, expected, actual );
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sbOutput"></param>
		/// <param name="sExpected"></param>
		/// <param name="sActual"></param>
		/// <param name="iPosition"></param>
		static private void BuildContentDifferentAtPosition( StringBuilder sbOutput, string sExpected, string sActual, int iPosition )
		{
			BuildStringLengthReport( sbOutput, sExpected, sActual );

			sbOutput.Append( "Strings differ at index " );
			sbOutput.Append( iPosition );
			sbOutput.Append( "." );
			sbOutput.Append( NewLine );

			//
			// Clips the strings, then turns any hidden whitespace into visible
			// characters
			//
			string sClippedExpected = ConvertWhitespace(ClipAroundPosition( sExpected, iPosition ));
			string sClippedActual   = ConvertWhitespace(ClipAroundPosition( sActual,   iPosition ));

			AppendExpectedAndActual( 
				sbOutput, 
				sClippedExpected, 
				sClippedActual );
			sbOutput.Append( NewLine );

			// Add a line showing where they differ.  If the string lengths are
			// different, they start differing just past the length of the 
			// shorter string
			AppendPositionMarker( 
				sbOutput, 
				FindMismatchPosition( sClippedExpected, sClippedActual, 0 ) );
			sbOutput.Append( NewLine );
		}

		/// <summary>
		/// Turns CR, LF, or TAB into visual indicator to preserve visual marker 
		/// position.   This is done by replacing the '\r' into '\\' and 'r' 
		/// characters, and the '\n' into '\\' and 'n' characters, and '\t' into
		/// '\\' and 't' characters.  
		/// 
		/// Thus the single character becomes two characters for display.
		/// </summary>
		/// <param name="sInput"></param>
		/// <returns></returns>
		static protected string ConvertWhitespace( string sInput )
		{
			if( null != sInput )
			{
				sInput = sInput.Replace( "\r", "\\r" );
				sInput = sInput.Replace( "\n", "\\n" );
				sInput = sInput.Replace( "\t", "\\t" );
			}
			return sInput;
		}

		/// <summary>
		/// Shows the position two strings start to differ.  Comparison 
		/// starts at the start index.
		/// </summary>
		/// <param name="sExpected"></param>
		/// <param name="sActual"></param>
		/// <param name="iStart"></param>
		/// <returns>-1 if no mismatch found, or the index where mismatch found</returns>
		static private int FindMismatchPosition( string sExpected, string sActual, int iStart )
		{
			int iLength = Math.Min( sExpected.Length, sActual.Length );
			for( int i=iStart; i<iLength; i++ )
			{
				//
				// If they mismatch at a specified position, report the
				// difference.
				//
				if( sExpected[i] != sActual[i] )
				{
					return i;
				}
			}
			//
			// Strings have same content up to the length of the shorter string.
			// Mismatch occurs because string lengths are different, so show
			// that they start differing where the shortest string ends
			//
			if( sExpected.Length != sActual.Length )
			{
				return iLength;
			}
            
			//
			// Same strings
			//
			Assert.IsTrue( sExpected.Equals( sActual ) );
			return -1;
		}

		/// <summary>
		/// Constructs a message that can be displayed when the content of two
		/// strings are different, but the string lengths are the same.  The
		/// message will clip the strings to a reasonable length, centered
		/// around the first position where they are mismatched, and draw 
		/// a line marking the position of the difference to make comparison
		/// quicker.
		/// </summary>
		/// <param name="sbOutput"></param>
		/// <param name="sExpected"></param>
		/// <param name="sActual"></param>
		static protected void BuildContentDifferentMessage( StringBuilder sbOutput, string sExpected, string sActual )
		{
			//
			// If they mismatch at a specified position, report the
			// difference.
			//
			int iMismatch = FindMismatchPosition( sExpected, sActual, 0 );
			if( -1 != iMismatch )
			{
				BuildContentDifferentAtPosition( 
					sbOutput, 
					sExpected, 
					sActual, 
					iMismatch );
				return;
			}

			//
			// If the lengths differ, but they match up to the length,
			// show the difference just past the length of the shorter
			// string
			//
			if( sExpected.Length != sActual.Length )
			{
				BuildContentDifferentAtPosition( 
					sbOutput, 
					sExpected, 
					sActual, 
					Math.Min(sExpected.Length, sActual.Length) );
			}
		}

		/// <summary>
		/// Called to append a message when the input strings are different.
		/// A different message is rendered when the lengths are mismatched,
		/// and when the lengths match but content is mismatched.
		/// </summary>
		/// <param name="sbOutput"></param>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		static private void BuildStringsDifferentMessage( StringBuilder sbOutput, string expected, string actual )
		{
			sbOutput.Append( NewLine );
			if( LengthsDifferent( expected, actual ) )
			{
				BuildLengthsDifferentMessage( sbOutput, expected, actual );
			}
			else
			{
				BuildContentDifferentMessage( sbOutput, expected, actual );
			}
		}

		/// <summary>
		/// Called to append a message when the input arrays are different.
		/// A different message is rendered when the lengths are mismatched,
		/// and when the lengths match but content is mismatched.
		/// </summary>
		/// <param name="sbOutput"></param>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		static private void BuildArraysDifferentMessage( StringBuilder sbOutput, int index, Array expected, Array actual )
		{
			sbOutput.Append( NewLine );

			BuildArrayLengthReport( sbOutput, expected, actual );
			
			sbOutput.Append( "Arrays differ at index " );
			sbOutput.Append( index );
			sbOutput.Append( "." );
			sbOutput.Append( NewLine );
				
			if ( index < expected.Length && index < actual.Length )
			{
				if( InputsAreStrings( expected.GetValue(index), actual.GetValue(index) ) )
				{
					BuildStringsDifferentMessage( 
						sbOutput, 
						(string)expected.GetValue(index), 
						(string)actual.GetValue(index) );
				}
				else
				{
					AppendExpectedAndActual( sbOutput, expected.GetValue(index), actual.GetValue(index) );
				}
			}
			else if( expected.Length < actual.Length )
			{
				sbOutput.Append( NewLine );
				sbOutput.Append( "   extra:<" );
				DisplayElements( sbOutput, actual, index, 3 );
				sbOutput.Append( ">" );
			}
			else
			{
				sbOutput.Append( NewLine );
				sbOutput.Append( " missing:<" );
				DisplayElements( sbOutput, expected, index, 3 );
				sbOutput.Append( ">" );
			}

			return;
		}

		static private void DisplayElements( StringBuilder sbOutput, Array array, int index, int max )
		{
			for( int i = 0; i < max; i++ )
			{
				sbOutput.Append( DisplayString( array.GetValue(index++) ) );
				
				if ( index >= array.Length )
					return;

				sbOutput.Append( "," );
			}

			sbOutput.Append( "..." );
		}

		/// <summary>
		/// Used to create a StringBuilder that is used for constructing
		/// the output message when text is different.  Handles initialization
		/// when a message is provided.  If message is null, an empty
		/// StringBuilder is returned.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		static protected StringBuilder CreateStringBuilder( string message, params object[] args )
		{
			StringBuilder sbOutput;
			if (message != null) 
			{
				if ( args != null && args.Length > 0 )
					sbOutput = new StringBuilder( string.Format( message, args ) );
				else
					sbOutput = new StringBuilder( message );
			}
			else
			{
				sbOutput = new StringBuilder();
			}
			return sbOutput;
		}

		/// <summary>
		/// Called to create a message when two objects have been found to
		/// be unequal.  If the inputs are strings, a special message is
		/// rendered that can help track down where the strings are different,
		/// based on differences in length, or differences in content.
		/// 
		/// If the inputs are not strings, the ToString method of the objects
		/// is used to show what is different about them.
		/// </summary>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		/// <param name="message"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		static public string FormatMessageForFailNotEquals(Object expected, Object actual,
			string message, params object[] args) 
		{
			StringBuilder sbOutput = CreateStringBuilder( message, args );
			if( null != message )
			{
				if( message.Length > 0 )
				{
					sbOutput.Append( " " );
				}
			}

			if( InputsAreStrings( expected, actual ) )
			{
				BuildStringsDifferentMessage( 
					sbOutput, 
					(string)expected, 
					(string)actual );
			}
			else
			{
				AppendExpectedAndActual( sbOutput, expected, actual );
			}
			return sbOutput.ToString();
		}

		/// <summary>
		/// Called to create a message when two arrays are not equal. 
		/// </summary>
		/// <param name="message"></param>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		/// <returns></returns>
		static public string FormatMessageForFailArraysNotEqual(int index, Array expected, Array actual, 
			string message, params object[] args) 
		{
			StringBuilder sbOutput = CreateStringBuilder( message, args );
			if( null != message )
			{
				if( message.Length > 0 )
				{
					sbOutput.Append( " " );
				}
			}

			BuildArraysDifferentMessage(
				sbOutput, 
				index,
				expected, 
				actual );

			return sbOutput.ToString();
		}
	}
}
