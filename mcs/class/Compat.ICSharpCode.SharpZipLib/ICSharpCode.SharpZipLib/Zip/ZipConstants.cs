// ZipConstants.cs
// Copyright (C) 2001 Mike Krueger
//
// This file was translated from java, it was part of the GNU Classpath
// Copyright (C) 2001 Free Software Foundation, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library.  Thus, the terms and
// conditions of the GNU General Public License cover the whole
// combination.
// 
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.

using System.Text;

namespace ICSharpCode.SharpZipLib.Zip 
{
	
	/// <summary>
	/// This class contains constants used for zip.
	/// </summary>
	[System.ObsoleteAttribute("This assembly has been deprecated. Please use https://www.nuget.org/packages/SharpZipLib/ instead.")]
	public sealed class ZipConstants
	{
		/* The local file header */
		public const int LOCHDR = 30;
		public const int LOCSIG = 'P' | ('K' << 8) | (3 << 16) | (4 << 24);
		
		public const int LOCVER =  4;
		public const int LOCFLG =  6;
		public const int LOCHOW =  8;
		public const int LOCTIM = 10;
		public const int LOCCRC = 14;
		public const int LOCSIZ = 18;
		public const int LOCLEN = 22;
		public const int LOCNAM = 26;
		public const int LOCEXT = 28;
		
		public const int SPANNINGSIG = 'P' | ('K' << 8) | (7 << 16) | (8 << 24);
		public const int SPANTEMPSIG = 'P' | ('K' << 8) | ('0' << 16) | ('0' << 24);
		
		/* The Data descriptor */
		public const int EXTSIG = 'P' | ('K' << 8) | (7 << 16) | (8 << 24);
		public const int EXTHDR = 16;
		
		public const int EXTCRC =  4;
		public const int EXTSIZ =  8;
		public const int EXTLEN = 12;
		
		/* The central directory file header */
		public const int CENSIG = 'P' | ('K' << 8) | (1 << 16) | (2 << 24);
		
		/* The central directory file header for 64bit ZIP*/
		public const int CENSIG64 = 'P' | ('K' << 8) | (6 << 16) | (6 << 24);
		
		public const int CENHDR = 46;
		
		public const int CENVEM =  4;
		public const int CENVER =  6;
		public const int CENFLG =  8;
		public const int CENHOW = 10;
		public const int CENTIM = 12;
		public const int CENCRC = 16;
		public const int CENSIZ = 20;
		public const int CENLEN = 24;
		public const int CENNAM = 28;
		public const int CENEXT = 30;
		public const int CENCOM = 32;
		public const int CENDSK = 34;
		public const int CENATT = 36;
		public const int CENATX = 38;
		public const int CENOFF = 42;
		
		/* The entries in the end of central directory */
		public const int ENDSIG = 'P' | ('K' << 8) | (5 << 16) | (6 << 24);
		public const int ENDHDR = 22;
		
		public const int HDRDIGITALSIG = 'P' | ('K' << 8) | (5 << 16) | (5 << 24);
		public const int CENDIGITALSIG = 'P' | ('K' << 8) | (5 << 16) | (5 << 24);
		
		/* The following two fields are missing in SUN JDK */
		public const int ENDNRD =  4;
		public const int ENDDCD =  6;
		
		public const int ENDSUB =  8;
		public const int ENDTOT = 10;
		public const int ENDSIZ = 12;
		public const int ENDOFF = 16;
		public const int ENDCOM = 20;
		
		
		static int defaultCodePage = 0;  // 0 gives default code page, set it to whatever you like or alternatively alter it via property at runtime for more flexibility
		                                 // Some care for compatability purposes is required as you can specify unicode code pages here.... if this way of working seems ok
		                                 // then some protection may be added to make the interface a bit more robust perhaps.
		
		public static int DefaultCodePage {
			get {
				return defaultCodePage; 
			}
			set {
				defaultCodePage = value; 
			}
		}
		
		public static string ConvertToString(byte[] data)
		{
#if COMACT_FRAMEWORK			
			return Encoding.ASCII.GetString(data,0, data.Length);
#else
			return Encoding.GetEncoding(DefaultCodePage).GetString(data, 0, data.Length);
#endif
		}
		
		public static byte[] ConvertToArray(string str)
		{
#if COMACT_FRAMEWORK			
			return Encoding.ASCII.GetBytes(str);
#else
			return Encoding.GetEncoding(DefaultCodePage).GetBytes(str);
#endif
		}
	}
}
