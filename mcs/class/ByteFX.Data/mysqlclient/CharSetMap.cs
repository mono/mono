using System;
using System.Text;
using System.Collections;

namespace ByteFX.Data.MySqlClient
{
	/// <summary>
	/// Summary description for CharSetMap.
	/// </summary>
	internal class CharSetMap
	{
		private static Hashtable mapping;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="MySqlCharSetName"></param>
		/// <returns></returns>
		public static Encoding GetEncoding( string MySqlCharSetName ) 
		{
			if (mapping == null )
				InitializeMapping();
			try 
			{
				int cpid = (int)mapping[ MySqlCharSetName ];
				return Encoding.GetEncoding( cpid );
			}
			catch (System.NotSupportedException) 
			{
				return Encoding.GetEncoding(0);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private static void InitializeMapping() 
		{
			mapping = new Hashtable();

			// relatively sure about
			mapping.Add( "default", 0 );
			mapping.Add( "big5", 950 );			// Traditional Chinese
			mapping.Add( "latin1", 28591 );		// Latin alphabet #1
			mapping.Add( "utf8", 65001 );
			mapping.Add( "ucs2", 1200 );
			mapping.Add( "latin2", 28592 );
			mapping.Add( "latin4", 28594 );
			mapping.Add( "latin3", 28593 );
			mapping.Add( "latin5", 1254 );
			mapping.Add( "cp1251", 1251 );		// Russian
			mapping.Add( "win1251", 1251 );
			mapping.Add( "hebrew", 1255 );		// Hebrew
			mapping.Add( "greek", 1253 );		// Greek
			mapping.Add( "sjis", 932 );			// Shift-JIS
			mapping.Add( "gbk", 936 );			// Simplified Chinese
			mapping.Add( "cp866", 866 );
			mapping.Add( "euc_kr", 949 );

			// maybe, maybe not...
			mapping.Add( "win1250", 1250 );		// Central Eurpoe
			mapping.Add( "win1251ukr", 1251 );
			mapping.Add( "latin1_de", 1252 );	// Latin1 German
			mapping.Add( "german1", 1252 );		// German
			mapping.Add( "danish", 1252 );		// Danish
			mapping.Add( "dos", 437 );			// Dos
			mapping.Add( "pclatin2", 852 );		
			mapping.Add( "win1250ch", 1250 );
			mapping.Add( "cp1257", 1257 );
			mapping.Add( "usa7", 646 );
			mapping.Add( "czech", 912 );
			mapping.Add( "hungarian", 912 );
			mapping.Add( "croat", 912 );

/*			("gb2312", "EUC_CN");
			("ujis", "EUC_JP");
			("latvian", "ISO8859_13");
			("latvian1", "ISO8859_13");
			("estonia", "ISO8859_13");
			("koi8_ru", "KOI8_R");
			("tis620", "TIS620");
			("macroman", "MacRoman");
			("macce", "MacCentralEurope");
*/

		}
	}
}
