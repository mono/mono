// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//
//  Alexander Olk	xenomorph2@onlinehome.de
//

using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Text;

// Usage:
// - for files:
//   string mimeType = Mime.GetMimeTypeForFile( string filename );
// - for byte array:
//   string mimeType = Mime.GetMimeTypeForData( byte[] data );
// - for string (maybe an email):
//   string mimeType = Mime.GetMimeTypeForString( string input );

// - get alias for mime type:
//   string alias = Mime.GetMimeAlias( string mimeType );
// - get subclass for mime type:
//   string subtype = Mime.GetMimeSubClass( string mimeType );
// - get all available mime types:
//   string[] available = Mime.AvailableMimeTypes;

// TODO:
// - optimize
// - little/big endian stuff for TypeHostXX
// - async callback ?!?
// - freedesktop org file extensions can have regular expressions also, resolve them too
// - sort match collections by magic priority ( higher = first )
// - MimeGenerated: use indexes to point to mime type name strings instead of repeating the name string each time (in match, subclass, etc.) !?!
// - buffer is currently hard coded to size 8192, value should be determined by MimeGenerated

namespace System.Windows.Forms
{
	internal class Mime
	{
		public static Mime Instance = new Mime();
		
		private string current_file_name;
		private string global_result = octet_stream;
		
		private FileStream file_stream;
		
		private byte[] buffer = new byte[ 8192 ];
		
		private const string octet_stream = "application/octet-stream";
		private const string text_plain = "text/plain";
		private const string zero_file = "application/x-zerosize";
		
		private StringDictionary mime_file_cache = new StringDictionary();
		
		private const int mime_file_cache_max_size = 5000;
		
		private string search_string;
		
		private static object lock_object = new Object();
		
		private int platform = (int) Environment.OSVersion.Platform;
		
		private bool is_zero_file = false;
		
		public Mime( )
		{
			MimeGenerated.Init( );
			
//			Console.WriteLine( "Mime Instance created..." );
		}
		
		public static string GetMimeTypeForFile( string filename )
		{
			lock ( lock_object )
			{
				Instance.StartByFileName( filename );
			}
			
			return Instance.global_result;
		}
		
		// not tested
		public static string GetMimeTypeForData( byte[] data )
		{
			lock ( lock_object )
			{
				Instance.StartDataLookup( data );
			}
			
			return Instance.global_result;
		}
		
		public static string GetMimeTypeForString( string input )
		{
			lock ( lock_object )
			{
				Instance.StartStringLookup( input );
			}
			
			return Instance.global_result;
		}
		
		public static string GetMimeAlias( string mimetype )
		{
			return MimeGenerated.Aliases[ mimetype ];
		}
		
		public static string GetMimeSubClass( string mimetype )
		{
			return MimeGenerated.SubClasses[ mimetype ];
		}
		
		public static string[] AvailableMimeTypes
		{
			get {
				string[] result = new string[ MimeGenerated.MimeTypes.Count ];
				
				MimeGenerated.MimeTypes.Keys.CopyTo( result, 0 );
				
				return result;
			}
		}
		
		private void StartByFileName( string filename )
		{
			if ( mime_file_cache.ContainsKey( filename ) )
			{
				global_result = mime_file_cache[ filename ];
				return;
			}
			
			current_file_name = filename;
			is_zero_file = false;
			
			if ( !CheckForInode( ) )
			{
				global_result = octet_stream;
				
				GoByFileName( );
			}
			
			if ( !mime_file_cache.ContainsKey( current_file_name ) )
				mime_file_cache.Add( current_file_name, global_result );
			
			// not tested
			if ( mime_file_cache.Count > mime_file_cache_max_size )
			{
				IEnumerator enumerator = mime_file_cache.GetEnumerator( );
				
				for ( int i = 0; i < mime_file_cache_max_size - 1000; i++ )
				{
					mime_file_cache.Remove( enumerator.Current.ToString( ) );
				}
			}
		}
		
		private void StartDataLookup( byte[] data )
		{
			global_result = octet_stream;
			
			System.Array.Clear( buffer, 0, buffer.Length );
			
			if ( data.Length > buffer.Length )
			{
				System.Array.Copy( data, buffer, buffer.Length );
			}
			else
			{
				System.Array.Copy( data, buffer, data.Length );
			}
			
			if ( CheckMatch80Plus( ) )
				return;
			
			if ( CheckMatchBelow80( ) )
				return;
			
			CheckForBinaryOrText( );
		}
		
		private void StartStringLookup( string input )
		{
			global_result = text_plain;
			
			search_string = input;
			
			if ( CheckForContentTypeString( ) )
				return;
		}
		
		private bool CheckForInode( )
		{
			if ( ( platform == 4 ) || ( platform == 128 ) )
			{
#if __MonoCS__
				// *nix platform
				Mono.Unix.UnixFileInfo ufi = new Mono.Unix.UnixFileInfo( current_file_name );
				
				if ( ufi.IsFile )
				{
					return false;
				}
				else
				if ( ufi.IsDirectory )
				{
					global_result = "inode/directory";
					return true;
				}
				else
				if ( ufi.IsBlockDevice )
				{
					global_result = "inode/blockdevice";
					return true;
				}
				else
				if ( ufi.IsSocket )
				{
					global_result = "inode/socket";
					return true;
				}
				else
				if ( ufi.IsSymbolicLink )
				{
					global_result = "inode/symlink";
					return true;
				}
				else
				if ( ufi.IsCharacterDevice )
				{
					global_result = "inode/chardevice";
					return true;
				}
				else
				if ( ufi.IsFIFO )
				{
					global_result = "inode/fifo";
					return true;
				}
#endif
			}
			else
			{
				// TODO!!!!
				// windows platform
			}
			
			return false;
		}
		
		private void GoByFileName( )
		{
			// check if we can open the file
			if ( !OpenFile( ) )
			{
				// couldn't open the file, check globals only
				
				CheckGlobalPatterns( );
				
				return;
			}
			
			if ( !is_zero_file )
			{
				// check for matches with a priority >= 80
				if ( CheckMatch80Plus( ) )
					return;
			}
			
			// check global patterns, aka file extensions...
			// this should be done for zero size files also,
			// for example zero size file trash.ccc~ should return
			// application/x-trash instead of application/x-zerosize
			if ( CheckGlobalPatterns( ) )
				return;
			
			// if file size is zero, no other checks are needed
			if ( is_zero_file )
				return;
			
			// ok, still nothing matches then try matches with a priority < 80
			if ( CheckMatchBelow80( ) )
				return;
			
			// wow, still nothing... return application/octet-stream for binary data, or text/plain for textual data
			CheckForBinaryOrText( );
		}
		
		private bool CheckMatch80Plus( )
		{
			foreach ( Match match in MimeGenerated.Matches80Plus )
			{
				if ( TestMatch( match ) )
				{
					global_result = match.MimeType;
					
					return true;
				}
			}
			
			return false;
		}
		
		private void CheckGlobalResult( )
		{
			int comma_index = global_result.IndexOf( "," );
			
			if ( comma_index != -1 )
			{
				global_result = global_result.Substring( 0, comma_index );
			}
		}
		
		private bool CheckGlobalPatterns( )
		{
			string filename = Path.GetFileName( current_file_name );
			string filename_lower = filename.ToLower( );
			
			// first check for literals
			
			for ( int i = 0; i < MimeGenerated.GlobalLiterals.Count; i++ )
			{
				string key = MimeGenerated.GlobalLiterals.GetKey( i );
				
				// no regex char
				if ( key.IndexOf( '[' ) == -1 )
				{
					if ( key.Equals( filename ) )
					{
						global_result = MimeGenerated.GlobalLiterals[ i ];
						CheckGlobalResult( );
						return true;
					}
				}
				else // regex it ;)
				{
					if ( Regex.IsMatch( filename, key ) )
					{
						global_result = MimeGenerated.GlobalLiterals[ i ];
						CheckGlobalResult( );
						return true;
					}
				}
			}
			
			if ( filename.IndexOf( '.' ) != -1 )
			{
				// check for double extension like .tar.gz
				
				for ( int i = 0; i < MimeGenerated.GlobalPatternsLong.Count; i++ )
				{
					string key = MimeGenerated.GlobalPatternsLong.GetKey( i );
					
					if ( filename.EndsWith( key ) )
					{
						global_result = MimeGenerated.GlobalPatternsLong[ i ];
						CheckGlobalResult( );
						return true;
					}
					else
					{
						if ( filename_lower.EndsWith( key ) )
						{
							global_result = MimeGenerated.GlobalPatternsLong[ i ];
							CheckGlobalResult( );
							return true;
						}
					}
				}
				
				// check normal extensions...
				
				string extension = Path.GetExtension( current_file_name );
				
				if ( extension.Length != 0 )
				{
					global_result = MimeGenerated.GlobalPatternsShort[ extension ];
					
					if ( global_result != null )
					{
						CheckGlobalResult( );
						return true;
					}
					
					string extension_lower = extension.ToLower( );
					
					global_result = MimeGenerated.GlobalPatternsShort[ extension_lower ];
					
					if ( global_result != null )
					{
						CheckGlobalResult( );
						return true;
					}
				}
			}
			
			// finally check if a prefix or suffix matches
			
			for ( int i = 0; i < MimeGenerated.GlobalSufPref.Count; i++ )
			{
				string key = MimeGenerated.GlobalSufPref.GetKey( i );
				
				if ( key.StartsWith( "*" ) )
				{
					if ( filename.EndsWith( key.Replace( "*", "" ) ) )
					{
						global_result = MimeGenerated.GlobalSufPref[ i ];
						CheckGlobalResult( );
						return true;
					}
				}
				else
				{
					if ( filename.StartsWith( key.Replace( "*", "" ) ) )
					{
						global_result = MimeGenerated.GlobalSufPref[ i ];
						CheckGlobalResult( );
						return true;
					}
				}
			}
			
			return false;
		}
		
		private bool CheckMatchBelow80( )
		{
			foreach ( Match match in MimeGenerated.MatchesBelow80 )
			{
				if ( TestMatch( match ) )
				{
					global_result = match.MimeType;
					
					return true;
				}
			}
			
			return false;
		}
		
		private void CheckForBinaryOrText( )
		{
			// check the first 32 bytes
			
			for ( int i = 0; i < 32; i++ )
			{
				char c = System.Convert.ToChar( buffer[ i ] );
				
				if ( c != '\t' &&  c != '\n' && c != '\r' && c != 12 && c < 32 )
				{
					global_result = octet_stream;
					return;
				}
			}
			
			global_result = text_plain;
		}
		
		private bool TestMatch( Match match )
		{
			bool found = false;
			
			//  using a simple brute force search algorithm
			// compare each (masked) value from the buffer with the (masked) value from the match
			// TODO:
			// - to find some more speed, maybe we should use unsafe code
			// - check if buffer[0] and buffer[lastmatchbyte] match ByteValue[0] and ByteValue[lastmatchbyte] in a match
			
			for ( int offset_counter = 0; offset_counter < match.OffsetLength; offset_counter++ )
			{
				if ( match.Mask == null )
				{
					if ( buffer[ match.Offset + offset_counter ] == match.ByteValue[ 0 ] )
					{
						if ( match.ByteValue.Length == 1 )
						{
							if ( match.Matches.Count > 0 )
							{
								foreach ( Match sub_match in match.Matches )
								{
									if ( TestMatch( sub_match ) )
										return true;
								}
							}
							else
								return true;
						}
						
						for ( int i = 1; i < match.ByteValue.Length; i++ )
						{
							if ( buffer[ match.Offset + offset_counter + i ] != match.ByteValue[ i ] )
							{
								found = false;
								break;
							}
							
							found = true;
						}
						
						if ( found )
						{
							found = false;
							
							if ( match.Matches.Count > 0 )
							{
								foreach ( Match sub_match in match.Matches )
								{
									if ( TestMatch( sub_match ) )
										return true;
								}
							}
							else
								return true;
						}
					}
				}
				else // with mask ( it's the same as above, only AND the byte with the corresponding mask byte
				{
					if ( ( buffer[ match.Offset + offset_counter ] & match.Mask[ 0 ] )  ==
					    ( match.ByteValue[ 0 ] & match.Mask[ 0 ] ) )
					{
						if ( match.ByteValue.Length == 1 )
						{
							if ( match.Matches.Count > 0 )
							{
								foreach ( Match sub_match in match.Matches )
								{
									if ( TestMatch( sub_match ) )
										return true;
								}
							}
							else
								return true;
						}
						
						for ( int i = 1; i < match.ByteValue.Length; i++ )
						{
							if ( ( buffer[ match.Offset + offset_counter + i ]  & match.Mask[ i ] ) !=
							    ( match.ByteValue[ i ] & match.Mask[ i ] ) )
							{
								found = false;
								break;
							}
							
							found = true;
						}
						
						if ( found )
						{
							found = false;
							
							if ( match.Matches.Count > 0 )
							{
								foreach ( Match sub_match in match.Matches )
								{
									if ( TestMatch( sub_match ) )
										return true;
								}
							}
							else
								return true;
						}
					}
				}
			}
			
			return found;
		}
		
		private bool OpenFile( )
		{
			try
			{
				System.Array.Clear( buffer, 0, buffer.Length );
				
				file_stream = new FileStream( current_file_name, FileMode.Open, FileAccess.Read ); // FileShare ??? use BinaryReader ???
				
				if ( file_stream.Length == 0 )
				{
					global_result = zero_file;
					is_zero_file = true;
				}
				else
				{
					file_stream.Read( buffer, 0, buffer.Length );
				}
				
				file_stream.Close( );
			}
			catch (Exception e)
			{
				return false;
			}
			
			return true;
		}
		
		private bool CheckForContentTypeString( )
		{
			int index = search_string.IndexOf( "Content-type:" );
			
			if ( index != -1 )
			{
				index += 13; // Length of string "Content-type:"
				
				global_result = "";
				
				while ( search_string[ index ] != ';' )
				{
					global_result += search_string[ index++ ];
				}
				
				global_result.Trim( );
				
				return true;
			}
			
			// convert string to byte array
			byte[] string_byte = ( new ASCIIEncoding( ) ).GetBytes( search_string );
			
			System.Array.Clear( buffer, 0, buffer.Length );
			
			if ( string_byte.Length > buffer.Length )
			{
				System.Array.Copy( string_byte, buffer, buffer.Length );
			}
			else
			{
				System.Array.Copy( string_byte, buffer, string_byte.Length );
			}
			
			if ( CheckMatch80Plus( ) )
				return true;
			
			if ( CheckMatchBelow80( ) )
				return true;
			
			return false;
		}
	}
	
	internal class MimeType
	{
		private string comment;
		private Hashtable commentsLanguage = new Hashtable();
		
		public string Comment
		{
			get {
				return comment;
			}
			set {
				comment = value;
			}
		}
		
		public Hashtable CommentsLanguage
		{
			get {
				return commentsLanguage;
			}
			set {
				commentsLanguage = value;
			}
		}
		public string GetCommentForLanguage( string language )
		{
			return commentsLanguage[ language ] as string;
		}
	}
	
	internal enum MatchTypes
	{
		TypeString,
		TypeHost16,
		TypeHost32,
		TypeBig16,
		TypeBig32,
		TypeLittle16,
		TypeLittle32,
		TypeByte
	}
	
	internal class Match
	{
		string mimeType;
		byte[] byteValue;
		byte[] mask = null;
		int priority;
		int offset;
		int offsetLength;
		int wordSize = 1;
		MatchTypes matchType;
		ArrayList matches = new ArrayList();
		
		public string MimeType
		{
			set {
				mimeType = value;
			}
			
			get {
				return mimeType;
			}
		}
		
		public byte[] ByteValue
		{
			set {
				byteValue = value;
			}
			
			get {
				return byteValue;
			}
		}
		
		public byte[] Mask
		{
			set {
				mask = value;
			}
			
			get {
				return mask;
			}
		}
		
		public int Priority
		{
			set {
				priority = value;
			}
			
			get {
				return priority;
			}
		}
		
		public ArrayList Matches
		{
			set {
				matches = value;
			}
			
			get {
				return matches;
			}
		}
		
		public int Offset
		{
			set {
				offset = value;
			}
			
			get {
				return offset;
			}
		}
		
		public int OffsetLength
		{
			set {
				offsetLength = value;
			}
			
			get {
				return offsetLength;
			}
		}
		
		public int WordSize
		{
			set {
				wordSize = value;
			}
			
			get {
				return wordSize;
			}
		}
		
		public MatchTypes MatchType
		{
			set {
				matchType = value;
			}
			
			get {
				return matchType;
			}
		}
	}
}

