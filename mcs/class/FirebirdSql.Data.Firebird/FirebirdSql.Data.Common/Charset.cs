/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Text;

namespace FirebirdSql.Data.Common
{
	internal sealed class Charset
	{
		#region Static Fields

		private readonly static CharsetCollection supportedCharsets = Charset.InitializeSupportedCharsets();

		#endregion

		#region Static Properties

		public static CharsetCollection SupportedCharsets
		{
			get { return Charset.supportedCharsets; }
		}

		public static Charset DefaultCharset
		{
			get { return Charset.SupportedCharsets[0]; }
		}

		#endregion

		#region Fields

		private int		id;
		private int		bytesPerCharacter;
		private string	name;
		private string	systemName;

		#endregion

		#region Properties

		public int ID
		{
			get { return this.id; }
		}

		public string Name
		{
			get { return this.name; }
		}

		public int BytesPerCharacter
		{
			get { return this.bytesPerCharacter; }
		}

		#endregion

		#region Constructors

		public Charset(
			int id, string name, int bytesPerCharacter, string systemName)
		{
			this.id					= id;
			this.name				= name;
			this.bytesPerCharacter	= bytesPerCharacter;
			this.systemName			= systemName;
		}

		#endregion

		#region Methods

		public byte[] GetBytes(string s)
		{
			return this.GetEncoding().GetBytes(s);
		}

		public int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			return this.GetEncoding().GetBytes(s, charIndex, charCount, bytes, byteIndex);
		}

		public string GetString(byte[] buffer)
		{
			return this.GetString(buffer, 0, buffer.Length);
		}

		public string GetString(byte[] buffer, int index, int count)
		{
			return this.GetEncoding().GetString(buffer, index, count);
		}

		#endregion

		#region Private Methods

		private Encoding GetEncoding()
		{
			switch (this.systemName)
			{
				case "NONE":
					return Encoding.Default;

				case "OCTETS":
					return new BinaryEncoding();

				default:
					return Encoding.GetEncoding(this.systemName);
			}
		}

		#endregion

		#region Static Methods

		public static CharsetCollection InitializeSupportedCharsets()
		{
			CharsetCollection charsets = new CharsetCollection();

			// NONE
			Charset.Add(charsets, 0, "NONE", 1, "NONE");
			// OCTETS
			Charset.Add(charsets, 1, "OCTETS", 1, "OCTETS");
			// American Standard Code for Information Interchange	
			Charset.Add(charsets, 2, "ASCII", 1, "ascii");
			// Eight-bit Unicode Transformation Format
			Charset.Add(charsets, 3, "UNICODE_FSS", 3, "UTF-8");
			// Shift-JIS, Japanese
			Charset.Add(charsets, 5, "SJIS_0208", 2, "shift_jis");
			// JIS X 0201, 0208, 0212, EUC encoding, Japanese
			Charset.Add(charsets, 6, "EUCJ_0208", 2, "euc-jp");
			// Windows Japanese	
			Charset.Add(charsets, 7, "ISO2022-JP", 2, "iso-2022-jp");
			// MS-DOS United States, Australia, New Zealand, South Africa	
			Charset.Add(charsets, 10, "DOS437", 1, "IBM437");
			// MS-DOS Latin-1				
			Charset.Add(charsets, 11, "DOS850", 1, "ibm850");
			// MS-DOS Nordic	
			Charset.Add(charsets, 12, "DOS865", 1, "IBM865");
			// MS-DOS Portuguese	
			Charset.Add(charsets, 13, "DOS860", 1, "IBM860");
			// MS-DOS Canadian French	
			Charset.Add(charsets, 14, "DOS863", 1, "IBM863");
			// ISO 8859-1, Latin alphabet No. 1
			Charset.Add(charsets, 21, "ISO8859_1", 1, "iso-8859-1");
			// ISO 8859-2, Latin alphabet No. 2
			Charset.Add(charsets, 22, "ISO8859_2", 1, "iso-8859-2");
			// Windows Korean	
			Charset.Add(charsets, 44, "KSC_5601", 2, "ks_c_5601-1987");
			// MS-DOS Icelandic	
			Charset.Add(charsets, 47, "DOS861", 1, "ibm861");
			// Windows Eastern European
			Charset.Add(charsets, 51, "WIN1250", 1, "windows-1250");
			// Windows Cyrillic
			Charset.Add(charsets, 52, "WIN1251", 1, "windows-1251");
			// Windows Latin-1
			Charset.Add(charsets, 53, "WIN1252", 1, "windows-1252");
			// Windows Greek
			Charset.Add(charsets, 54, "WIN1253", 1, "windows-1253");
			// Windows Turkish
			Charset.Add(charsets, 55, "WIN1254", 1, "windows-1254");
			// Big5, Traditional Chinese
			Charset.Add(charsets, 56, "BIG_5", 2, "big5");
			// GB2312, EUC encoding, Simplified Chinese	
			Charset.Add(charsets, 57, "GB_2312", 2, "gb2312");
			// Windows Hebrew
			Charset.Add(charsets, 58, "WIN1255", 1, "windows-1255");
			// Windows Arabic	
			Charset.Add(charsets, 59, "WIN1256", 1, "windows-1256");
			// Windows Baltic	
			Charset.Add(charsets, 60, "WIN1257", 1, "windows-1257");

            return charsets;
		}

		private static void Add(
			CharsetCollection charsets, int id, string charset, int bytesPerCharacter, string systemName)
		{
			charsets.Add(
					id,
					charset,
					bytesPerCharacter,
					systemName);
		}

		#endregion
	}
}
