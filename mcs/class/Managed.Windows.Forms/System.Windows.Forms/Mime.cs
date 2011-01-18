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
// Copyright (c) 2006 Alexander Olk
//
// Authors:
//
//  Alexander Olk	alex.olk@googlemail.com
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
// - optimize even more :)
// - async callback ?!?
// - freedesktop org file extensions can have regular expressions also, resolve them too
// - sort match collections by magic priority ( higher = first ) ?

// internal test:
// looking up the mime types 20 times for 2757 files in /usr/lib without caching (mime_file_cache)
// old version: Time: 00:00:32.3791220
// new version: Time: 00:00:16.9991810

namespace System.Windows.Forms
{
	internal class Mime
	{
		public static Mime Instance = new Mime();
		
		private string current_file_name;
		private string global_result = octet_stream;
		
		private FileStream file_stream;
		
		private byte[] buffer = null;
		
		private const string octet_stream = "application/octet-stream";
		private const string text_plain = "text/plain";
		private const string zero_file = "application/x-zerosize";
		
		private StringDictionary mime_file_cache = new StringDictionary();
		
		private const int mime_file_cache_max_size = 3000;
		
		private string search_string;
		
		private static object lock_object = new Object();
		
		private bool is_zero_file = false;
		
		private int bytes_read = 0;
		
		private bool mime_available = false;
		
		public static NameValueCollection Aliases;
		public static NameValueCollection SubClasses;
		
		public static NameValueCollection GlobalPatternsShort;
		public static NameValueCollection GlobalPatternsLong;
		public static NameValueCollection GlobalLiterals;
		public static NameValueCollection GlobalSufPref;
		
		public static ArrayList Matches80Plus;
		public static ArrayList MatchesBelow80;
		
		private Mime ()
		{
			Aliases = new NameValueCollection (StringComparer.CurrentCultureIgnoreCase);
			SubClasses = new NameValueCollection (StringComparer.CurrentCultureIgnoreCase);
			GlobalPatternsShort = new NameValueCollection (StringComparer.CurrentCultureIgnoreCase);
			GlobalPatternsLong = new NameValueCollection (StringComparer.CurrentCultureIgnoreCase);
			GlobalLiterals = new NameValueCollection (StringComparer.CurrentCultureIgnoreCase);
			GlobalSufPref = new NameValueCollection (StringComparer.CurrentCultureIgnoreCase);
			
			Matches80Plus = new ArrayList ();
			MatchesBelow80 = new ArrayList ();
			
			FDOMimeConfigReader fmcr = new FDOMimeConfigReader ();
			int buffer_length = fmcr.Init ();
			
			if (buffer_length >= 32) {
				buffer = new byte [buffer_length];
				mime_available = true;
			}
		}
		
		public static bool MimeAvailable {
			get {
				return Instance.mime_available;
			}
		}
		
		public static string GetMimeTypeForFile (string filename)
		{
			lock (lock_object) {
				Instance.StartByFileName (filename);
			}
			
			return Instance.global_result;
		}
		
		// not tested
		public static string GetMimeTypeForData (byte[] data)
		{
			lock (lock_object) {
				Instance.StartDataLookup (data);
			}
			
			return Instance.global_result;
		}
		
		public static string GetMimeTypeForString (string input)
		{
			lock (lock_object) {
				Instance.StartStringLookup (input);
			}
			
			return Instance.global_result;
		}
		
		public static string GetMimeAlias (string mimetype)
		{
			return Aliases [mimetype];
		}
		
		public static string GetMimeSubClass (string mimetype)
		{
			return SubClasses [mimetype];
		}
		
		public static void CleanFileCache ()
		{
			lock (lock_object) {
				Instance.mime_file_cache.Clear ();
			}
		}
		
		private void StartByFileName (string filename)
		{
			if (mime_file_cache.ContainsKey (filename)) {
				global_result = mime_file_cache [filename];
				return;
			}
			
			current_file_name = filename;
			is_zero_file = false;
			
			global_result = octet_stream;
			
			GoByFileName ();
			
			mime_file_cache.Add (current_file_name, global_result);
			
			if (mime_file_cache.Count > mime_file_cache_max_size) {
				IEnumerator enumerator = mime_file_cache.GetEnumerator ();
				
				int counter = mime_file_cache_max_size - 500;
				
				while (enumerator.MoveNext ()) {
					mime_file_cache.Remove (enumerator.Current.ToString ());
					counter--;
					
					if (counter == 0)
						break;
				}
			}
		}
		
		private void StartDataLookup (byte[] data)
		{
			global_result = octet_stream;
			
			System.Array.Clear (buffer, 0, buffer.Length);
			
			if (data.Length > buffer.Length) {
				System.Array.Copy (data, buffer, buffer.Length);
			} else {
				System.Array.Copy (data, buffer, data.Length);
			}
			
			if (CheckMatch80Plus ())
				return;
			
			if (CheckMatchBelow80 ())
				return;
			
			CheckForBinaryOrText ();
		}
		
		private void StartStringLookup (string input)
		{
			global_result = text_plain;
			
			search_string = input;
			
			if (CheckForContentTypeString ())
				return;
		}
		
		private void GoByFileName ()
		{
			// check if we can open the file
			if (!MimeAvailable || !OpenFile ()) {
				// couldn't open the file, check globals only
				CheckGlobalPatterns ();
				
				return;
			}
			
			if (!is_zero_file) {
				// check for matches with a priority >= 80
				if (CheckMatch80Plus ())
					return;
			}
			
			// check global patterns, aka file extensions...
			// this should be done for zero size files also,
			// for example zero size file trash.ccc~ should return
			// application/x-trash instead of application/x-zerosize
			if (CheckGlobalPatterns ())
				return;
			
			// if file size is zero, no other checks are needed
			if (is_zero_file)
				return;
			
			// ok, still nothing matches then try matches with a priority < 80
			if (CheckMatchBelow80 ())
				return;
			
			// wow, still nothing... return application/octet-stream for binary data, or text/plain for textual data
			CheckForBinaryOrText ();
		}
		
		private bool CheckMatch80Plus ()
		{
			foreach (Match match in Matches80Plus) {
				if (TestMatch (match)) {
					global_result = match.MimeType;
					
					return true;
				}
			}
			
			return false;
		}
		
		// this little helper method gives us a real speed improvement
		private bool FastEndsWidth (string input, string value)
		{
			if (value.Length > input.Length)
				return false;
			
			int z = input.Length - 1;
			
			for (int i = value.Length - 1; i > -1; i--) {
				if (value [i] != input [z])
					return false;
				
				z--;
			}
			
			return true;
		}
		
		private bool FastStartsWith (string input, string value)
		{
			if (value.Length > input.Length)
				return false;
			
			for (int i = 0; i < value.Length; i++)
				if (value [i] != input [i])
					return false;
			
			return true;
		}
		
		// start always with index = 0
		private int FastIndexOf (string input, char value)
		{
			if (input.Length == 0)
				return -1;
			
			for (int i = 0; i < input.Length; i++)
				if (input [i] == value)
					return i;
			
			return -1;
		}
		
		private int FastIndexOf (string input, string value)
		{
			if (input.Length == 0)
				return -1;
			
			for (int i = 0; i < input.Length - value.Length; i++) {
				if (input [i] == value [0]) {
					int counter = 0;
					for (int z = 1; z < value.Length; z++) {
						if (input [i + z] != value [z])
							break;
						
						counter++;
					}
					if (counter == value.Length - 1) {
						return i;
					}
				}
			}
			
			return -1;
		}
		
		private void CheckGlobalResult ()
		{
			int comma_index = FastIndexOf (global_result, ',');
			
			if (comma_index != -1) {
				global_result = global_result.Substring (0, comma_index);
			}
		}
		
		private bool CheckGlobalPatterns ()
		{
			string filename = Path.GetFileName (current_file_name);
			
			// first check for literals
			for (int i = 0; i < GlobalLiterals.Count; i++) {
				string key = GlobalLiterals.GetKey (i);
				
				// no regex char
				if (FastIndexOf (key, '[') == -1) {
					if (FastIndexOf (filename, key) != -1) {
						global_result = GlobalLiterals [i];
						CheckGlobalResult ();
						return true;
					}
				} else {
					if (Regex.IsMatch (filename, key)) {
						global_result = GlobalLiterals [i];
						CheckGlobalResult ();
						return true;
					}
				}
			}
			
			if (FastIndexOf (filename, '.') != -1) {
				// check for double extension like .tar.gz
				for (int i = 0; i < GlobalPatternsLong.Count; i++) {
					string key = GlobalPatternsLong.GetKey (i);
					
					if (FastEndsWidth (filename, key)) {
						global_result = GlobalPatternsLong [i];
						CheckGlobalResult ();
						return true;
					} else {
						if (FastEndsWidth (filename.ToLower (), key)) {
							global_result = GlobalPatternsLong [i];
							CheckGlobalResult ();
							return true;
						}
					}
				}
				
				// check normal extensions...
				string extension = Path.GetExtension (current_file_name);
				
				if (extension.Length != 0) {
					string global_result_tmp = GlobalPatternsShort [extension];
					
					if (global_result_tmp != null) {
						global_result = global_result_tmp;
						CheckGlobalResult ();
						return true;
					}
					
					global_result_tmp = GlobalPatternsShort [extension.ToLower ()];
					
					if (global_result_tmp != null) {
						global_result = global_result_tmp;
						CheckGlobalResult ();
						return true;
					}
				}
			}
			
			// finally check if a prefix or suffix matches
			for (int i = 0; i < GlobalSufPref.Count; i++) {
				string key = GlobalSufPref.GetKey (i);
				
				if (key [0] == '*') {
					if (FastEndsWidth (filename, key.Replace ("*", String.Empty))) {
						global_result = GlobalSufPref [i];
						CheckGlobalResult ();
						return true;
					}
				} else {
					if (FastStartsWith (filename, key.Replace ("*", String.Empty))) {
						global_result = GlobalSufPref [i];
						CheckGlobalResult ();
						return true;
					}
				}
			}
			
			return false;
		}
		
		private bool CheckMatchBelow80 ()
		{
			foreach (Match match in MatchesBelow80) {
				if (TestMatch (match)) {
					global_result = match.MimeType;
					
					return true;
				}
			}
			
			return false;
		}
		
		private void CheckForBinaryOrText ()
		{
			// check the first 32 bytes
			
			for (int i = 0; i < 32; i++) {
				char c = System.Convert.ToChar (buffer [i]);
				
				if (c != '\t' &&  c != '\n' && c != '\r' && c != 12 && c < 32) {
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
		
		private bool TestMatchlet (Matchlet matchlet)
		{
			//  using a simple brute force search algorithm
			// compare each (masked) value from the buffer with the (masked) value from the matchlet
			
			// no need to check if the offset + the bytevalue length exceed the # bytes read
			if (matchlet.Offset + matchlet.ByteValue.Length > bytes_read)
				return false;
			
			for (int offset_counter = 0; offset_counter < matchlet.OffsetLength; offset_counter++) {
				if (matchlet.Offset + offset_counter + matchlet.ByteValue.Length > bytes_read)
					return false;
				
				if (matchlet.Mask == null) {
					if (buffer [matchlet.Offset + offset_counter] == matchlet.ByteValue [0]) {
						if (matchlet.ByteValue.Length == 1) {
							if (matchlet.Matchlets.Count > 0) {
								foreach (Matchlet sub_matchlet in matchlet.Matchlets) {
									if (TestMatchlet (sub_matchlet))
										return true;
								}
							} else
								return true;
						}
						
						int minus = 0;
						// check if the last matchlet byte value is the same as the byte value in the buffer...
						if (matchlet.ByteValue.Length > 2) {
							if (buffer [matchlet.Offset + offset_counter + matchlet.ByteValue.Length - 1] != matchlet.ByteValue [matchlet.ByteValue.Length - 1])
								return false;
							
							minus = 1;
						}
						
						for (int i = 1; i < matchlet.ByteValue.Length - minus; i++) {
							if (buffer [matchlet.Offset + offset_counter + i] != matchlet.ByteValue [i])
								return false;
						}
						
						if (matchlet.Matchlets.Count > 0) {
							foreach (Matchlet sub_matchlets in matchlet.Matchlets) {
								if (TestMatchlet (sub_matchlets))
									return true;
							}
						} else
							return true;
					}
				} else {
					if ((buffer [matchlet.Offset + offset_counter] & matchlet.Mask [0])  ==
					    (matchlet.ByteValue [0] & matchlet.Mask [0])) {
						if (matchlet.ByteValue.Length == 1) {
							if (matchlet.Matchlets.Count > 0) {
								foreach (Matchlet sub_matchlets in matchlet.Matchlets) {
									if (TestMatchlet (sub_matchlets))
										return true;
								}
							} else
								return true;
						}
						
						int minus = 0;
						// check if the last matchlet byte value is the same as the byte value in the buffer...
						if (matchlet.ByteValue.Length > 2) {
							
							if ((buffer [matchlet.Offset + offset_counter + matchlet.ByteValue.Length - 1] & matchlet.Mask [matchlet.ByteValue.Length - 1])
							    != (matchlet.ByteValue [matchlet.ByteValue.Length - 1] & matchlet.Mask [matchlet.ByteValue.Length - 1]))
								return false;
							
							minus = 1;
						}
						
						for (int i = 1; i < matchlet.ByteValue.Length - minus; i++) {
							if ((buffer [matchlet.Offset + offset_counter + i]  & matchlet.Mask [i]) !=
							    (matchlet.ByteValue [i] & matchlet.Mask [i]))
								return false;
						}
						
						if (matchlet.Matchlets.Count > 0) {
							foreach (Matchlet sub_matchlets in matchlet.Matchlets) {
								if (TestMatchlet (sub_matchlets))
									return true;
							}
						} else
							return true;
					}
				}
			}
			
			return false;
		}
		
		private bool OpenFile ()
		{
			try {
				file_stream = new FileStream (current_file_name, FileMode.Open, FileAccess.Read); // FileShare ??? use BinaryReader ???
				
				if (file_stream.Length == 0) {
					global_result = zero_file;
					is_zero_file = true;
				} else {
					bytes_read = file_stream.Read (buffer, 0, buffer.Length);
					
					// do not clear the whole buffer everytime; clear only what's needed
					if (bytes_read < buffer.Length) {
						System.Array.Clear (buffer, bytes_read, buffer.Length - bytes_read);
					}
				}
				
				file_stream.Close ();
			} catch (Exception) {
				return false;
			}
			
			return true;
		}
		
		private bool CheckForContentTypeString ()
		{
			int index = search_string.IndexOf ("Content-type:");
			
			if (index != -1) {
				index += 13; // Length of string "Content-type:"
				
				global_result = String.Empty;
				
				while (search_string [index] != ';') {
					global_result += search_string [index++];
				}
				
				global_result.Trim ();
				
				return true;
			}
			
			// convert string to byte array
			byte[] string_byte = (new ASCIIEncoding ()).GetBytes (search_string);
			
			System.Array.Clear (buffer, 0, buffer.Length);
			
			if (string_byte.Length > buffer.Length) {
				System.Array.Copy (string_byte, buffer, buffer.Length);
			} else {
				System.Array.Copy (string_byte, buffer, string_byte.Length);
			}
			
			if (CheckMatch80Plus ())
				return true;
			
			if (CheckMatchBelow80 ())
				return true;
			
			return false;
		}
	}
	
	internal class FDOMimeConfigReader
	{
		bool fdo_mime_available = false;
		StringCollection shared_mime_paths = new StringCollection ();
		BinaryReader br;
		
		int max_offset_and_range = 0;
		
		public int Init ()
		{
			CheckFDOMimePaths ();
			
			if (!fdo_mime_available)
				return -1;
			
			ReadMagicData ();
			
			ReadGlobsData ();
			
			ReadSubclasses ();
			
			ReadAliases ();
			
			shared_mime_paths = null;
			br = null;
			
			return max_offset_and_range;
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
							StringBuilder indent_string = new StringBuilder ();
							//string indent_string = String.Empty;
							while (true) {
								if (br.PeekChar () == '>')
									break;
								
								c = br.ReadChar ();
								//indent_string += c;
								indent_string.Append (c);
							}
							indent = Convert.ToInt32 (indent_string.ToString ());
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
						int range_length = 1;
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
						
						if (max_offset_and_range < matchlets [indent].Offset + matchlets [indent].OffsetLength + matchlets [indent].ByteValue.Length + 1)
							max_offset_and_range = matchlets [indent].Offset + matchlets [indent].OffsetLength + matchlets [indent].ByteValue.Length  + 1;
						
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
								Mime.GlobalPatternsLong.Add (split [1].Remove (0, 1), split [0]);
							} else {
								// normal
								Mime.GlobalPatternsShort.Add (split [1].Remove (0, 1), split [0]);
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
			StringBuilder result_string = new StringBuilder ();
			int result = 0;
			char c;
			
			while (true) {
				if (br.PeekChar () == '=' || br.PeekChar () == '\n')
					break;
				
				c = br.ReadChar ();
				result_string.Append (c);
			}
			
			result = Convert.ToInt32 (result_string.ToString ());
			
			return result;
		}
		
		private string ReadPriorityAndMimeType (ref int priority)
		{
			if (br.ReadChar () == '[') {
				StringBuilder priority_string = new StringBuilder ();
				while (true) {
					char c = br.ReadChar ();
					if (c == ':')
						break;
					priority_string.Append (c);
				}
				
				priority = System.Convert.ToInt32 (priority_string.ToString ());
				
				StringBuilder mime_type_result = new StringBuilder ();
				while (true) {
					char c = br.ReadChar ();
					if (c == ']')
						break;
					
					mime_type_result.Append (c);
				}
				
				if (br.ReadChar () == '\n')
					return mime_type_result.ToString ();
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
	
	internal class Match
	{
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
	
	internal class Matchlet
	{
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

