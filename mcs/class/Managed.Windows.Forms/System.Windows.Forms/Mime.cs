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
// - async callback ?!?
// - freedesktop org file extensions can have regular expressions also, resolve them too
// - sort match collections by magic priority ( higher = first ) ?
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
		
		public static NameValueCollection Aliases;
		public static NameValueCollection SubClasses;
		
		public static NameValueCollection GlobalPatternsShort;
		public static NameValueCollection GlobalPatternsLong;
		public static NameValueCollection GlobalLiterals;
		public static NameValueCollection GlobalSufPref;
		
		public static ArrayList Matches80Plus;
		public static ArrayList MatchesBelow80;
		
		private Mime( )
		{
			Aliases = new NameValueCollection (new CaseInsensitiveHashCodeProvider (), new Comparer (System.Globalization.CultureInfo.CurrentUICulture));
			SubClasses = new NameValueCollection (new CaseInsensitiveHashCodeProvider (), new Comparer (System.Globalization.CultureInfo.CurrentUICulture));
			GlobalPatternsShort = new NameValueCollection (new CaseInsensitiveHashCodeProvider (), new Comparer (System.Globalization.CultureInfo.CurrentUICulture));
			GlobalPatternsLong = new NameValueCollection (new CaseInsensitiveHashCodeProvider (), new Comparer (System.Globalization.CultureInfo.CurrentUICulture));
			GlobalLiterals = new NameValueCollection (new CaseInsensitiveHashCodeProvider (), new Comparer (System.Globalization.CultureInfo.CurrentUICulture));
			GlobalSufPref = new NameValueCollection (new CaseInsensitiveHashCodeProvider (), new Comparer (System.Globalization.CultureInfo.CurrentUICulture));
			Matches80Plus = new ArrayList ();
			MatchesBelow80 = new ArrayList ();
			
			FDOMimeConfigReader fmcr = new FDOMimeConfigReader ();
			fmcr.Init ();
		}
		
		public static string GetMimeTypeForFile( string filename )
		{
			lock ( lock_object )
			{
				Instance.StartByFileName( filename );
			}
			
			if (filename.EndsWith(".html")) {
				Console.WriteLine(filename);
				Console.WriteLine(Instance.global_result);
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
			return Aliases[ mimetype ];
		}
		
		public static string GetMimeSubClass( string mimetype )
		{
			return SubClasses[ mimetype ];
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
			
//			if ( !CheckForInode( ) )
//			{
				global_result = octet_stream;
				
				GoByFileName( );
//			}
			
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
		
//		private bool CheckForInode( )
//		{
//			if ( ( platform == 4 ) || ( platform == 128 ) )
//			{
//#if __MonoCS__
//				try
//				{
//					// *nix platform
//					Mono.Unix.UnixFileInfo ufi = new Mono.Unix.UnixFileInfo( current_file_name );
//
//					if ( ufi.IsFile )
//					{
//						return false;
//					}
//					else
//					if ( ufi.IsDirectory )
//					{
//						global_result = "inode/directory";
//						return true;
//					}
//					else
//					if ( ufi.IsBlockDevice )
//					{
//						global_result = "inode/blockdevice";
//						return true;
//					}
//					else
//					if ( ufi.IsSocket )
//					{
//						global_result = "inode/socket";
//						return true;
//					}
//					else
//					if ( ufi.IsSymbolicLink )
//					{
//						global_result = "inode/symlink";
//						return true;
//					}
//					else
//					if ( ufi.IsCharacterDevice )
//					{
//						global_result = "inode/chardevice";
//						return true;
//					}
//					else
//					if ( ufi.IsFIFO )
//					{
//						global_result = "inode/fifo";
//						return true;
//					}
//				} catch( Exception e )
//				{
//					return false;
//				}
//#endif
//			}
//			else
//			{
//				// TODO!!!!
//				// windows platform
//			}
//			
//			return false;
//		}
		
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
			foreach ( Match match in Matches80Plus )
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
			for ( int i = 0; i < GlobalLiterals.Count; i++ )
			{
				string key = GlobalLiterals.GetKey( i );
				
				// no regex char
				if ( key.IndexOf( '[' ) == -1 )
				{
					if (filename.IndexOf(key) != -1)
					{
						global_result = GlobalLiterals[ i ];
						CheckGlobalResult( );
						return true;
					}
				}
				else // regex it ;)
				{
					if ( Regex.IsMatch( filename, key ) )
					{
						global_result = GlobalLiterals[ i ];
						CheckGlobalResult( );
						return true;
					}
				}
			}
			
			if ( filename.IndexOf( '.' ) != -1 )
			{
				// check for double extension like .tar.gz
				
				for ( int i = 0; i < GlobalPatternsLong.Count; i++ )
				{
					string key = GlobalPatternsLong.GetKey( i );
					
					if ( filename.EndsWith( key ) )
					{
						global_result = GlobalPatternsLong[ i ];
						CheckGlobalResult( );
						return true;
					}
					else
					{
						if ( filename_lower.EndsWith( key ) )
						{
							global_result = GlobalPatternsLong[ i ];
							CheckGlobalResult( );
							return true;
						}
					}
				}
				
				// check normal extensions...
				
				string extension = Path.GetExtension( current_file_name );
				
				if ( extension.Length != 0 )
				{
					global_result = GlobalPatternsShort[ extension ];
					
					if ( global_result != null )
					{
						CheckGlobalResult( );
						return true;
					}
					
					string extension_lower = extension.ToLower( );
					
					global_result = GlobalPatternsShort[ extension_lower ];
					
					if ( global_result != null )
					{
						CheckGlobalResult( );
						return true;
					}
				}
			}
			
			// finally check if a prefix or suffix matches
			
			for ( int i = 0; i < GlobalSufPref.Count; i++ )
			{
				string key = GlobalSufPref.GetKey( i );
				
				if ( key.StartsWith( "*" ) )
				{
					if ( filename.EndsWith( key.Replace( "*", "" ) ) )
					{
						global_result = GlobalSufPref[ i ];
						CheckGlobalResult( );
						return true;
					}
				}
				else
				{
					if ( filename.StartsWith( key.Replace( "*", "" ) ) )
					{
						global_result = GlobalSufPref[ i ];
						CheckGlobalResult( );
						return true;
					}
				}
			}
			
			return false;
		}
		
		private bool CheckMatchBelow80( )
		{
			foreach ( Match match in MatchesBelow80 )
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
		
		private bool TestMatch (Match match)
		{
			foreach (Matchlet matchlet in match.Matchlets)
				if (TestMatchlet (matchlet))
					return true;
			
			return false;
		}
		
		private bool TestMatchlet( Matchlet matchlet )
		{
			bool found = false;
			
			//  using a simple brute force search algorithm
			// compare each (masked) value from the buffer with the (masked) value from the match
			// TODO:
			// - to find some more speed, maybe we should use unsafe code
			// - check if buffer[0] and buffer[lastmatchbyte] match ByteValue[0] and ByteValue[lastmatchbyte] in a match
			
			for ( int offset_counter = 0; offset_counter < matchlet.OffsetLength; offset_counter++ )
			{
				if ( matchlet.Mask == null )
				{
					if ( buffer[ matchlet.Offset + offset_counter ] == matchlet.ByteValue[ 0 ] )
					{
						if ( matchlet.ByteValue.Length == 1 )
						{
							if ( matchlet.Matchlets.Count > 0 )
							{
								foreach ( Matchlet sub_matchlet in matchlet.Matchlets )
								{
									if ( TestMatchlet( sub_matchlet ) )
										return true;
								}
							}
							else
								return true;
						}
						
						for ( int i = 1; i < matchlet.ByteValue.Length; i++ )
						{
							if ( buffer[ matchlet.Offset + offset_counter + i ] != matchlet.ByteValue[ i ] )
							{
								found = false;
								break;
							}
							
							found = true;
						}
						
						if ( found )
						{
							found = false;
							
							if ( matchlet.Matchlets.Count > 0 )
							{
								foreach ( Matchlet sub_matchlets in matchlet.Matchlets )
								{
									if ( TestMatchlet( sub_matchlets ) )
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
					if ( ( buffer[ matchlet.Offset + offset_counter ] & matchlet.Mask[ 0 ] )  ==
					    ( matchlet.ByteValue[ 0 ] & matchlet.Mask[ 0 ] ) )
					{
						if ( matchlet.ByteValue.Length == 1 )
						{
							if ( matchlet.Matchlets.Count > 0 )
							{
								foreach ( Matchlet sub_matchlets in matchlet.Matchlets )
								{
									if ( TestMatchlet( sub_matchlets ) )
										return true;
								}
							}
							else
								return true;
						}
						
						for ( int i = 1; i < matchlet.ByteValue.Length; i++ )
						{
							if ( ( buffer[ matchlet.Offset + offset_counter + i ]  & matchlet.Mask[ i ] ) !=
							    ( matchlet.ByteValue[ i ] & matchlet.Mask[ i ] ) )
							{
								found = false;
								break;
							}
							
							found = true;
						}
						
						if ( found )
						{
							found = false;
							
							if ( matchlet.Matchlets.Count > 0 )
							{
								foreach ( Matchlet sub_matchlets in matchlet.Matchlets )
								{
									if ( TestMatchlet( sub_matchlets ) )
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
	
	internal class FDOMimeConfigReader {
		bool fdo_mime_available = false;
		StringCollection shared_mime_paths = new StringCollection ();
		BinaryReader br;
		
		public void Init ()
		{
			CheckFDOMimePaths ();
			
			if (!fdo_mime_available)
				return;
			
			ReadMagicData ();
			
			ReadGlobsData ();
			
			ReadSubclasses ();
			
			ReadAliases ();
			
			shared_mime_paths = null;
			br = null;
		}
		
		private void CheckFDOMimePaths ()
		{
			if (Directory.Exists ("/usr/share/mime"))
				shared_mime_paths.Add ("/usr/share/mime/");
			else
			if (Directory.Exists ("/usr/local/share/mime"))
				shared_mime_paths.Add ("/usr/local/share/mime/");
			
			if (Directory.Exists (System.Environment.GetFolderPath (Environment.SpecialFolder.Personal) + "/.local/share/mime"))
				shared_mime_paths.Add (System.Environment.GetFolderPath (Environment.SpecialFolder.Personal) + "/.local/share/mime/");
			
			if (shared_mime_paths.Count == 0)
				return;
			
			fdo_mime_available = true;
		}
		
		private void ReadMagicData ()
		{
			foreach (string path in shared_mime_paths) {
				if (!File.Exists (path + "/magic"))
					continue;
				
				try {
					FileStream fs = File.OpenRead (path + "/magic");
					br = new BinaryReader (fs);
					
					if (CheckMagicHeader ()) {
						MakeMatches ();
					}
					
					br.Close ();
					fs.Close ();
				} catch (Exception ) {
				}
			}
		}
		
		private void MakeMatches ()
		{
			Matchlet[] matchlets = new Matchlet [30];
			
			while (br.PeekChar () != -1) {
				int priority = -1;
				string mime_type = ReadPriorityAndMimeType (ref priority);
				
				if (mime_type != null) {
					Match match = new Match ();
					match.Priority = priority;
					match.MimeType = mime_type;
					
					while (true) {
						int indent = 0;
						// indent
						char c;
						if (br.PeekChar () != '>') {
							string indent_string = "";
							while (true) {
								if (br.PeekChar () == '>')
									break;
								
								c = br.ReadChar ();
								indent_string += c;
							}
							indent = Convert.ToInt32 (indent_string);
						}
						
						int offset = 0;
						
						// offset
						if (br.PeekChar () == '>') {
							br.ReadChar ();
							offset = ReadValue ();
						}
						
						int value_length = 0;
						byte[] value = null;
						// value length and value
						if (br.PeekChar () == '=') {
							br.ReadChar ();
							
							// read 2 bytes value length (always big endian)
							byte first = br.ReadByte ();
							byte second = br.ReadByte ();
							
							value_length = first * 256 + second;
							
							value = br.ReadBytes (value_length);
						}
						
						// mask
						byte[] mask = null;
						
						if (br.PeekChar () == '&') {
							br.ReadChar ();
							
							mask = br.ReadBytes (value_length);
						}
						
						// word_size
						int word_size = 1;
						if (br.PeekChar () == '~') {
							br.ReadChar ();
							
							c = br.ReadChar ();
							
							word_size = Convert.ToInt32 (c - 0x30);
							
							// data is stored in big endian format. 
							if (word_size > 1 && System.BitConverter.IsLittleEndian) {
								//convert the value and, if available, the mask data to little endian
								if (word_size == 2) {
									if (value != null) {
										for (int i = 0; i < value.Length; i += 2) {
											byte one = value [i];
											byte two = value [i + 1];
											value [i] = two;
											value [i + 1] = one;
										}
									}
									if (mask != null) {
										for (int i = 0; i < mask.Length; i += 2) {
											byte one = mask [i];
											byte two = mask [i + 1];
											mask [i] = two;
											mask [i + 1] = one;
										}
									}
								} else if (word_size == 4) {
									if (value != null) {
										for (int i = 0; i < value.Length; i += 4) {
											byte one = value [i];
											byte two = value [i + 1];
											byte three = value [i + 2];
											byte four = value [i + 3];
											value [i] = four;
											value [i + 1] = three;
											value [i + 2] = two;
											value [i + 3] = one;
										}
									}
									if (mask != null) {
										for (int i = 0; i < mask.Length; i += 4) {
											byte one = mask [i];
											byte two = mask [i + 1];
											byte three = mask [i + 2];
											byte four = mask [i + 3];
											mask [i] = four;
											mask [i + 1] = three;
											mask [i + 2] = two;
											mask [i + 3] = one;
											
										}
									}
								}
							}
						}
						
						// range length
						int range_length = 0;
						if (br.PeekChar () == '+') {
							br.ReadChar ();
							range_length = ReadValue ();
						}
						
						// read \n
						br.ReadChar ();
						
						// create the matchlet
						matchlets [indent] = new Matchlet ();
						matchlets [indent].Offset = offset;
						matchlets [indent].OffsetLength = range_length;
						matchlets [indent].ByteValue = value;
						if (mask != null)
							matchlets [indent].Mask = mask;
						
						if (indent == 0) {
							match.Matchlets.Add (matchlets [indent]);
						} else {
							matchlets [indent - 1].Matchlets.Add (matchlets [indent]);
						}
						
						
						// if '[' move to next mime type
						if (br.PeekChar () == '[')
							break;
					}
					
					if (priority < 80)
						Mime.MatchesBelow80.Add (match);
					else
						Mime.Matches80Plus.Add (match);
				}
			}
		}
		
		private void ReadGlobsData ()
		{
			foreach (string path in shared_mime_paths) {
				if (!File.Exists (path + "/globs"))
					continue;
				
				try {
					StreamReader sr = new StreamReader (path + "/globs");
					
					while (sr.Peek () != -1) {
						string line = sr.ReadLine ().Trim ();
						
						if (line.StartsWith ("#"))
							continue;
						
						string[] split = line.Split (new char [] {':'});
						
						if (split [1].IndexOf ('*') > -1 && split [1].IndexOf ('.') == -1) {
							Mime.GlobalSufPref.Add (split [1], split [0]);
						} else if (split [1]. IndexOf ('*') == -1) {
							Mime.GlobalLiterals.Add (split [1], split [0]);
						} else {
							string[] split2 = split [1].Split (new char [] {'.'});
							
							if (split2.Length > 2) {
								// more than one dot
								Mime.GlobalPatternsLong.Add (split [1].Remove(0, 1), split [0]);
							} else {
								// normal
								Mime.GlobalPatternsShort.Add (split [1].Remove(0, 1), split [0]);
							}
						}
					}
					
					sr.Close ();
				} catch (Exception ) {
				}
			}
		}
		
		private void ReadSubclasses ()
		{
			foreach (string path in shared_mime_paths) {
				if (!File.Exists (path + "/subclasses"))
					continue;
				
				try {
					StreamReader sr = new StreamReader (path + "/subclasses");
					
					while (sr.Peek () != -1) {
						string line = sr.ReadLine ().Trim ();
						
						if (line.StartsWith ("#"))
							continue;
						
						string[] split = line.Split (new char [] {' '});
						
						Mime.SubClasses.Add (split [0], split [1]);
					}
					
					sr.Close ();
				} catch (Exception ) {
				}
			}
		}
		
		private void ReadAliases ()
		{
			foreach (string path in shared_mime_paths) {
				if (!File.Exists (path + "/aliases"))
					continue;
				
				try {
					StreamReader sr = new StreamReader (path + "/aliases");
					
					while (sr.Peek () != -1) {
						string line = sr.ReadLine ().Trim ();
						
						if (line.StartsWith ("#"))
							continue;
						
						string[] split = line.Split (new char [] {' '});
						
						Mime.Aliases.Add (split [0], split [1]);
					}
					
					sr.Close ();
				} catch (Exception ) {
				}
			}
		}
		
		private int ReadValue ()
		{
			string result_string = "";
			int result = 0;
			char c;
			
			while (true) {
				if (br.PeekChar () == '=' || br.PeekChar () == '\n')
					break;
				
				c = br.ReadChar ();
				result_string += c;
			}
			
			result = Convert.ToInt32 (result_string);
			
			return result;
		}
		
		private string ReadPriorityAndMimeType (ref int priority)
		{
			if (br.ReadChar () == '[') {
				string priority_string = "";
				while (true) {
					char c = br.ReadChar ();
					if (c == ':')
						break;
					priority_string += c;
				}
				
				priority = System.Convert.ToInt32 (priority_string);
				
				string mime_type_result = "";
				while (true) {
					char c = br.ReadChar ();
					if (c == ']')
						break;
					
					mime_type_result += c;
				}
				
				if (br.ReadChar () == '\n')
					return mime_type_result;
			}
			return null;
		}
		
		private bool CheckMagicHeader ()
		{
			char[] chars = br.ReadChars (10);
			string magic_header = new String (chars);
			
			if (magic_header != "MIME-Magic")
				return false;
			
			if (br.ReadByte () != 0)
				return false;
			if (br.ReadChar () != '\n')
				return false;
			
			return true;
		}
	}
	
	internal class Match {
		string mimeType;
		int priority;
		ArrayList matchlets = new ArrayList();
		
		public string MimeType {
			set {
				mimeType = value;
			}
			
			get {
				return mimeType;
			}
		}
		
		public int Priority {
			set {
				priority = value;
			}
			
			get {
				return priority;
			}
		}
		
		public ArrayList Matchlets {
			get {
				return matchlets;
			}
		}
	}
	
	internal class Matchlet {
		byte[] byteValue;
		byte[] mask = null;
		
		int offset;
		int offsetLength;
		int wordSize = 1;
		
		ArrayList matchlets = new ArrayList ();
		
		public byte[] ByteValue {
			set {
				byteValue = value;
			}
			
			get {
				return byteValue;
			}
		}
		
		public byte[] Mask {
			set {
				mask = value;
			}
			
			get {
				return mask;
			}
		}
		
		public int Offset {
			set {
				offset = value;
			}
			
			get {
				return offset;
			}
		}
		
		public int OffsetLength {
			set {
				offsetLength = value;
			}
			
			get {
				return offsetLength;
			}
		}
		
		public int WordSize {
			set {
				wordSize = value;
			}
			
			get {
				return wordSize;
			}
		}
		
		public ArrayList Matchlets {
			get {
				return matchlets;
			}
		}
	}
}

