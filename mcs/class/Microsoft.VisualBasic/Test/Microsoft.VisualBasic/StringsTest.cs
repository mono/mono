//
// StringsTest.cs
//
// Authors:
//   Jochen Wezel (jwezel@compumaster.de)
//
// (C) 2003 Jochen Wezel, CompuMaster GmbH (http://www.compumaster.de/)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
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

using Microsoft.VisualBasic;
using System;
using System.Text;
using System.Globalization;
using NUnit.Framework;

namespace MonoTests.Microsoft.VisualBasic
{
	[TestFixture]
	public class StringsTest : Assertion 
	{
		private string TextStringOfMultipleLanguages;
		private string TextStringUninitialized;
		const string TextStringEmpty = "";

		//Disclaimer: I herewith distance me and the whole Mono project of text written in this test strings - they are really only for testing purposes and are copy and pasted of randomly found test parts of several suriously looking websites
		const string MSWebSiteContent_English = "Choose the location for which you want contact information:";
		const string MSWebSiteContent_Japanese = "電子メールをはじめ、予定表やアドレス帳などの個人情報、さらには Office v. X の各アプリケーションで作成した書類やその他のファイルをリンクさせて統合的に管理することができるコミュニケーション／個人情報管理ソフトウェアです。インターフェースが一新され、高度で多彩な Entourage X の機能がより直感的に利用できるようになりました。以前お使いになっていた電子メール アプリケーションからの情報のインポートも容易にできます。";
		const string MSWebSiteContent_Russian = "будете автоматически перенаправлены";
		const string MSWebSiteContent_Slovakian = "čníci náročných používateľovNové portfólio Microsoft Hardware. špičkové optické bezdrôtové myšky s novou technológiou - nakláňacím kolieskom. Nezáleží na tom, aký stôl máte, pokiaľ je na ňom elegantná";
		const string MSWebSiteContent_Korean = "보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!";
		const string ArabynetComWebSiteContent_Arabic = "كل الحقوق محفوظة ليديعوت إنترنت";
		const string GermanUmlauts_German = "äöüÄÖÜß";

		private char Letter_Empty;
		private char Letter_English;
		private char Letter_Japanese;
		private char Letter_Russian;
		private char Letter_Slovakian;
		private char Letter_Korean;
		private char Letter_Arabic;
		private char Letter_German;

		[SetUp]
		public void Setup()
		{		
			TextStringOfMultipleLanguages = 
				MSWebSiteContent_Japanese + 
				MSWebSiteContent_Russian +
				MSWebSiteContent_Slovakian +
				MSWebSiteContent_Korean +
				ArabynetComWebSiteContent_Arabic +
				GermanUmlauts_German;
			Letter_English = MSWebSiteContent_English[0];
			Letter_Japanese = MSWebSiteContent_Japanese[0];
			Letter_Russian = MSWebSiteContent_Russian[0];
			Letter_Slovakian = MSWebSiteContent_Slovakian[0];
			Letter_Korean = MSWebSiteContent_Korean[0];
			Letter_Arabic = ArabynetComWebSiteContent_Arabic[0];
			Letter_German = GermanUmlauts_German[0];
		}


		//TODO: additional tests with other system/languages (especially other Asian ones)
		//		pay attention to
		//		1. Byte count
		//		2. Little or big endian
        [Test]
		public void Asc()
		{
			Asc_Char();
			Asc_String();
		}
		
		public void Asc_Char() 
		{
			// buffer current culture
			System.Globalization.CultureInfo CurCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

			Encoding defencoding;
			defencoding = Encoding.Default;

			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			NUnit.Framework.Assertion.AssertEquals ("JW*Asc#US29", 1252, System.Globalization.CultureInfo.CurrentCulture.TextInfo.ANSICodePage);
			NUnit.Framework.Assertion.AssertEquals ("JW*Asc#US26", 0, Strings.Asc(Letter_Empty));
			NUnit.Framework.Assertion.AssertEquals ("JW*Asc#US27 - Quotation mark test", 34, Strings.Asc("\""[0]));
			if (defencoding.GetMaxByteCount(1) == 1)
			{
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#US28 - JapaneseCharacter", 63, Strings.Asc(MSWebSiteContent_Japanese[0]));
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#US28a - GermanCharacter", 228, Strings.Asc(Letter_German));
			}
			else
			{
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#US28W - JapaneseCharacter", -27804, Strings.Asc(MSWebSiteContent_Japanese[0]));
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#US28aW - GermanCharacter", 97, Strings.Asc(Letter_German));
			}

			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
			NUnit.Framework.Assertion.AssertEquals ("JW*Asc#DE29", 1252, System.Globalization.CultureInfo.CurrentCulture.TextInfo.ANSICodePage);
			NUnit.Framework.Assertion.AssertEquals ("JW*Asc#DE26", 0, Strings.Asc(Letter_Empty));
			NUnit.Framework.Assertion.AssertEquals ("JW*Asc#DE27 - Quotation mark test", 34, Strings.Asc("\""[0]));
			if (defencoding.GetMaxByteCount(1) == 1)
			{
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#US28 - JapaneseCharacter", 63, Strings.Asc(MSWebSiteContent_Japanese[0]));
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#DE28a - GermanCharacter", 228, Strings.Asc(Letter_German));
			}
			else
			{
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#DE28W - JapaneseCharacter", -27804, Strings.Asc(MSWebSiteContent_Japanese[0]));
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#DE28aW - GermanCharacter", 97, Strings.Asc(Letter_German));
			}
			
			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("ja-JP");
			NUnit.Framework.Assertion.AssertEquals ("JW*Asc#JA29", 932, System.Globalization.CultureInfo.CurrentCulture.TextInfo.ANSICodePage);
			NUnit.Framework.Assertion.AssertEquals ("JW*Asc#JA26", 0, Strings.Asc(Letter_Empty));
			NUnit.Framework.Assertion.AssertEquals ("JW*Asc#JA27 - Quotation mark test", 34, Strings.Asc("\""[0]));
			if (defencoding.GetMaxByteCount(1) == 1)
			{
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#JA28 - JapaneseCharacter", 63, Strings.Asc(MSWebSiteContent_Japanese[0]));
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#JA28a - GermanCharacter", 228, Strings.Asc(Letter_German));
			}
			else
			{
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#JA28W - JapaneseCharacter", -27804, Strings.Asc(MSWebSiteContent_Japanese[0]));
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#JA28aW - GermanCharacter", 97, Strings.Asc(Letter_German));
			}

			// restore buffered culture
			System.Threading.Thread.CurrentThread.CurrentCulture = CurCulture;
		}


		public void Asc_String() 
		{
			Encoding defencoding;
			defencoding = Encoding.Default;

			if (defencoding.GetMaxByteCount(1) == 1)
			{
				//single byte systems
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#01", 63, Strings.Asc(this.TextStringOfMultipleLanguages));
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#02", 99, Strings.Asc(MSWebSiteContent_Slovakian));
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#03", 63, Strings.Asc(MSWebSiteContent_Japanese));
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#04", 63, Strings.Asc(ArabynetComWebSiteContent_Arabic));
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#05", 63, Strings.Asc(MSWebSiteContent_Korean));
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#06", 63, Strings.Asc(MSWebSiteContent_Russian));
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#07", 67, Strings.Asc(MSWebSiteContent_English));
			}
			else
			{
				//double byte charsets (wide)
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#01W", -27804, Strings.Asc(this.TextStringOfMultipleLanguages));
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#02W", 63, Strings.Asc(MSWebSiteContent_Slovakian));
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#03W", -27804, Strings.Asc(MSWebSiteContent_Japanese));
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#04W", 63, Strings.Asc(ArabynetComWebSiteContent_Arabic));
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#05W", 63, Strings.Asc(MSWebSiteContent_Korean));
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#06W", -31631, Strings.Asc(MSWebSiteContent_Russian));
				NUnit.Framework.Assertion.AssertEquals ("JW*Asc#07W", 67, Strings.Asc(MSWebSiteContent_English));
			}
			try
			{
				object buffer = Strings.Asc(TextStringEmpty);
				NUnit.Framework.Assertion.Fail ("JW*Asc#08 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW*Asc#08", true);
			}
			try
			{
				object buffer = Strings.Asc(null);
				NUnit.Framework.Assertion.Fail ("JW*Asc#09 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW*Asc#09", true);
			}
			try
			{
				object buffer = Strings.Asc(TextStringUninitialized);
				NUnit.Framework.Assertion.Fail ("JW*Asc#10 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW*Asc#10", true);
			}
			NUnit.Framework.Assertion.AssertEquals ("JW*Asc#11 - Quotation mark test", 34, Strings.Asc("\""));
		}


		[Test]
		public void AscW()
		{
			AscW_Char();
			AscW_String();
		}

		public void AscW_Char() 
		{
			NUnit.Framework.Assertion.AssertEquals ("JW#12", 0, Strings.AscW(Letter_Empty));
			NUnit.Framework.Assertion.AssertEquals ("JW#13 - Quotation mark test", 34, Strings.AscW("\""[0]));
			NUnit.Framework.Assertion.AssertEquals ("JW#14 - JapaneseCharacter", 38651, Strings.AscW(Letter_Japanese));
			NUnit.Framework.Assertion.AssertEquals ("JW#14a - ArabicCharacter", 1603, Strings.AscW(Letter_Arabic));
			NUnit.Framework.Assertion.AssertEquals ("JW#14b - GermanCharacter", 228, Strings.AscW(Letter_German));
		}
		
		public void AscW_String() 
		{
			NUnit.Framework.Assertion.AssertEquals ("JW#15", 38651, Strings.AscW(this.TextStringOfMultipleLanguages));
			NUnit.Framework.Assertion.AssertEquals ("JW#16", 269, Strings.AscW(MSWebSiteContent_Slovakian));
			NUnit.Framework.Assertion.AssertEquals ("JW#17", 38651, Strings.AscW(MSWebSiteContent_Japanese));
			NUnit.Framework.Assertion.AssertEquals ("JW#18", 1603, Strings.AscW(ArabynetComWebSiteContent_Arabic));
			NUnit.Framework.Assertion.AssertEquals ("JW#19", 48372, Strings.AscW(MSWebSiteContent_Korean));
			NUnit.Framework.Assertion.AssertEquals ("JW#20", 1073, Strings.AscW(MSWebSiteContent_Russian));
			NUnit.Framework.Assertion.AssertEquals ("JW#21", 67, Strings.AscW(MSWebSiteContent_English));
			try
			{
				object buffer = Strings.AscW(TextStringEmpty);
				NUnit.Framework.Assertion.Fail ("JW#22 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#22", true);
			}
			try
			{
				object buffer = Strings.AscW(null);
				NUnit.Framework.Assertion.Fail ("JW#23 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#23", true);
			}
			try
			{
				object buffer = Strings.AscW(TextStringUninitialized);
				NUnit.Framework.Assertion.Fail ("JW#24 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#24", true);
			}
			NUnit.Framework.Assertion.AssertEquals ("JW#25 - Quotation mark test", 34, Strings.AscW("\""));
		}

		[Test]
		public void Chr() 
		{
			NUnit.Framework.Assertion.AssertEquals ("JW#29", "@"[0], Strings.Chr(64));
			try
			{
				object buffer = Strings.Chr(38651);
				NUnit.Framework.Assertion.Fail ("JW#30 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#30", true);
			}
			NUnit.Framework.Assertion.AssertEquals ("JW#31 - Quotation mark test", "\""[0], Strings.Chr(34));
		}

		[Test]
		public void ChrW() 
		{
			NUnit.Framework.Assertion.AssertEquals ("JW#32", "@"[0], Strings.ChrW(64));
			NUnit.Framework.Assertion.AssertEquals ("JW#33", "電"[0], Strings.ChrW(38651));
			NUnit.Framework.Assertion.AssertEquals ("JW#34 - Quotation mark test", "\""[0], Strings.ChrW(34));
		}


		//TODO: [Test]
		public void Filter()
		{
			Filter_Objects();
			Filter_Strings();
		}
		
        public void Filter_Objects()
		{
			/*
			if (Source +AD0APQ- null)
				throw new ArgumentException("Argument 'Source' can not be null.", "Source");
			if (Source.Rank +AD4- 1)
				throw new ArgumentException("Argument 'Source' can have only one dimension.", "Source");

			string+AFsAXQ- strings;
			strings = new string[Source.Length+AF0AOw-

			Source.CopyTo(strings, 0);
			return Filter(strings, Match, Include, Compare);
			*/
		}

		public void Filter_Strings()
		{
			/*
			if (Source +AD0APQ- null)
				throw new ArgumentException("Argument 'Source' can not be null.", "Source");
			if (Source.Rank +AD4- 1)
				throw new ArgumentException("Argument 'Source' can have only one dimension.", "Source");

			 //* Well, I don't like it either. But I figured that two iterations
			 //* on the array would be better than many aloocations. Besides, this
			 //* way I can isolate the special cases.
			 //* I'd love to hear from a different approach.
			 
			int count = Source.Length;
			bool+AFsAXQ- matches = new bool[count+AF0AOw-
			int matchesCount = 0;

			for (int i = 0; i +ADw- count; i++)
			{
				if (InStr(1, Match, Source[i], Compare) +ACEAPQ- 0)
				{
					//found one more
					matches[i] = true;
					matchesCount ++;
				}
				else
				{
					matches[i] = false;
				}
			}

			if (matchesCount +AD0APQ- 0)
			{
				if (Include)
					return new string[0+AF0AOw-
				else
					return Source;
			}
			else
			{
				if (matchesCount +AD0APQ- count)
				{
					if (Include)
						return Source;
					else
						return new string[0+AF0AOw-
				}
				else
				{
					string+AFsAXQ- ret;
					int j = 0;
					if (Include)
						ret = new string [matchesCount+AF0AOw-
					else
						ret = new string [count - matchesCount+AF0AOw-

					for (int i=0; i +ADw- count; i++)
					{
						if ((matches[i] +ACYAJg- Include) +AHwAfA- +ACE-(matches[i] +AHwAfA- Include))
						{
							ret[j] = Source[i+AF0AOw-
							j++;
						}
					}
					return ret;
				}
			}
			*/
		}

		//[Test]
		public void Format()
		{
			FormatCurrency();
			FormatDateTime();
			FormatNumber();
			FormatPercent();
		}

		// [MonoToDo("Not implemented")]
		//TODO: [Test]
		public void Format_Original()
		{
			/*
			string returnstr=null;
			string expstring=expression.GetType().ToString()+ADsAOw-
			switch(expstring)
			{
				case "System.Char":
					if ( style+ACEAPQAiACI-)
						throw new System.ArgumentException("'expression' argument has a not valid value");
					returnstr=Convert.ToChar(expression).ToString();
					break;
				case "System.String":
					if (style +AD0APQ- +ACIAIg-)
						returnstr=expression.ToString();
					else
					{
						switch ( style.ToLower ())
						{
							case "yes/no":
							case "on/off":
							switch (expression.ToString().ToLower())
							{
								case "true":
								case "On":
									if (style.ToLower ()+AD0APQAi-yes/no")
										returnstr+AD0AIg-Yes+ACIAOw- // TODO : must be translated
									else
										returnstr+AD0AIg-On+ACIAOw- // TODO : must be translated
									break;
								case "false":
								case "off":
									if (style.ToLower ()+AD0APQAi-yes/no")
										returnstr+AD0AIg-No+ACIAOw- // TODO : must be translated
									else
										returnstr+AD0AIg-Off+ACIAOw- // TODO : must be translated
									break;
								default:
									throw new System.ArgumentException();

							}
								break;
							default:
								returnstr=style.ToString();
								break;
						}
					}
					break;
				case "System.Boolean":
					if ( style+AD0APQAiACI-)
					{
						if ( Convert.ToBoolean(expression)+AD0APQ-true)
							returnstr+AD0AIg-True+ACIAOw- // must not be translated
						else
							returnstr+AD0AIg-False+ACIAOw- // must not be translated
					}
					else
						returnstr=style;
					break;
				case "System.DateTime":
					returnstr=Convert.ToDateTime(expression).ToString (style) ;
					break;
				case "System.Decimal":	case "System.Byte":	case "System.SByte":
				case "System.Int16":	case "System.Int32":	case "System.Int64":
				case "System.Double":	case "System.Single":	case "System.UInt16":
				case "System.UInt32":	case "System.UInt64":
				switch (style.ToLower ())
				{
					case "yes/no": case "true":	case "false": case "on/off":
						style=style.ToLower();
						double dblbuffer=Convert.ToDouble(expression);
						if (dblbuffer +AD0APQ- 0)
						{
							switch (style)
							{
								case "on/off":
									returnstr= "Off+ACIAOw-break; // TODO : must be translated
								case "yes/no":
									returnstr= "No+ACIAOw-break; // TODO : must be translated
								case "true":
								case "false":
									returnstr= "False+ACIAOw-break; // must not be translated
							}
						}
						else
						{
							switch (style)
							{
								case "on/off":
									returnstr+AD0AIg-On+ACIAOw-break; // TODO : must be translated
								case "yes/no":
									returnstr+AD0AIg-Yes+ACIAOw-break; // TODO : must be translated
								case "true":
								case "false":
									returnstr+AD0AIg-True+ACIAOw-break; // must not be translated
							}
						}
						break;
					default:
					switch (expstring)
					{
						case "System.Byte": returnstr=Convert.ToByte(expression).ToString (style);break;
						case "System.SByte": returnstr=Convert.ToSByte(expression).ToString (style);break;
						case "System.Int16": returnstr=Convert.ToInt16(expression).ToString (style);break;
						case "System.UInt16": returnstr=Convert.ToUInt16(expression).ToString (style);break;
						case "System.Int32":  returnstr=Convert.ToInt32(expression).ToString (style);break;
						case "System.UInt32":  returnstr=Convert.ToUInt32(expression).ToString (style);break;
						case "System.Int64":  returnstr=Convert.ToUInt64(expression).ToString (style);break;
						case "System.UInt64":returnstr=Convert.ToUInt64(expression).ToString (style);break;
						case "System.Single": returnstr=Convert.ToSingle(expression).ToString (style);break;
						case "System.Double":  returnstr=Convert.ToDouble(expression).ToString (style);break;
						case "System.Decimal": returnstr=Convert.ToDecimal(expression).ToString (style);break;

					}
						break;
				}
					break;
			}
			if (returnstr+AD0APQ-null)
				throw new System.ArgumentException();
			return returnstr;
			*/
		}

		// [MonoToDo("Not implemented")]
		//TODO: [Test]
		public void FormatCurrency()
		{
			/*
			//FIXME
			throw new NotImplementedException();
			//throws InvalidCastException
			//throws ArgumentException
			*/
		}

		// [MonoToDo("Not implemented")]
		//TODO: [Test]
		public void FormatDateTime()
		{
			/*
			switch(NamedFormat)
			{
				case DateFormat.GeneralDate:
					//FIXME: WTF should I do with it?
					throw new NotImplementedException(); 	
				case DateFormat.LongDate:  
					return Expression.ToLongDateString();
				case DateFormat.ShortDate:
					return Expression.ToShortDateString();
				case DateFormat.LongTime:
					return Expression.ToLongTimeString();
				case DateFormat.ShortTime:
					return Expression.ToShortTimeString();
				default:
					throw new ArgumentException("Argument 'NamedFormat' must be a member of DateFormat", "NamedFormat");
			}
			*/
		}

		[Test]
		public void FormatNumber()
		{
			// buffer current culture
			System.Globalization.CultureInfo CurCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

			// do testings
			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
			NUnit.Framework.Assertion.AssertEquals ("JW#60", "1.000", Strings.FormatNumber(1000,0,TriState.False,TriState.False,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#61", "1000", Strings.FormatNumber(1000,0,TriState.False,TriState.False,TriState.False));
			NUnit.Framework.Assertion.AssertEquals ("JW#62", "1.000", Strings.FormatNumber(1000,0,TriState.True,TriState.False,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#63", "1.000", Strings.FormatNumber(1000,0,TriState.False,TriState.True,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#64", "1.000", Strings.FormatNumber(1000,0,TriState.True,TriState.True,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#65", "1000", Strings.FormatNumber(1000,0,TriState.False,TriState.False,TriState.False));
			NUnit.Framework.Assertion.AssertEquals ("JW#66", "1.000", Strings.FormatNumber(1000,0,TriState.False,TriState.False,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#67", "-1000", Strings.FormatNumber(-1000,0,TriState.False,TriState.False,TriState.False));
			NUnit.Framework.Assertion.AssertEquals ("JW#68", "-1.000", Strings.FormatNumber(-1000,0,TriState.True,TriState.False,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#69", "(1.000)", Strings.FormatNumber(-1000,0,TriState.False,TriState.True,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#70", "(1.000)", Strings.FormatNumber(-1000,0,TriState.True,TriState.True,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#71", "-1000", Strings.FormatNumber(-1000,0,TriState.False,TriState.False,TriState.False));
			NUnit.Framework.Assertion.AssertEquals ("JW#72", "(1.000,0000)", Strings.FormatNumber(-1000,4,TriState.True,TriState.True,TriState.True));

			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			NUnit.Framework.Assertion.AssertEquals ("JW#80", "1,000", Strings.FormatNumber(1000,0,TriState.False,TriState.False,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#81", "1000", Strings.FormatNumber(1000,0,TriState.False,TriState.False,TriState.False));
			NUnit.Framework.Assertion.AssertEquals ("JW#82", "1,000", Strings.FormatNumber(1000,0,TriState.True,TriState.False,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#83", "1,000", Strings.FormatNumber(1000,0,TriState.False,TriState.True,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#84", "1,000", Strings.FormatNumber(1000,0,TriState.True,TriState.True,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#85", "1000", Strings.FormatNumber(1000,0,TriState.False,TriState.False,TriState.False));
			NUnit.Framework.Assertion.AssertEquals ("JW#86", "1,000", Strings.FormatNumber(1000,0,TriState.False,TriState.False,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#87", "-1000", Strings.FormatNumber(-1000,0,TriState.False,TriState.False,TriState.False));
			NUnit.Framework.Assertion.AssertEquals ("JW#88", "-1,000", Strings.FormatNumber(-1000,0,TriState.True,TriState.False,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#89", "(1,000)", Strings.FormatNumber(-1000,0,TriState.False,TriState.True,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#90", "(1,000)", Strings.FormatNumber(-1000,0,TriState.True,TriState.True,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#91", "-1000", Strings.FormatNumber(-1000,0,TriState.False,TriState.False,TriState.False));
			NUnit.Framework.Assertion.AssertEquals ("JW#92", "(1,000.0000)", Strings.FormatNumber(-1000,4,TriState.True,TriState.True,TriState.True));

			// restore buffered culture
			System.Threading.Thread.CurrentThread.CurrentCulture = CurCulture;

			/*
			//FIXME
			throw new NotImplementedException();
			//throws InvalidCastException
			*/
		}

		// [MonoToDo("Not implemented")]
		//TODO: [Test]
		public void FormatPercent()
		{
			/*
			//FIXME
			throw new NotImplementedException();
			//throws InvalidCastException
			*/
		}

		// [MonoToDo("Not implemented")]
		//TODO: [Test]
		public void GetChar()
		{
			/*
			if ((Str +AD0APQ- null) +AHwAfA- (Str.Length +AD0APQ- 0))
				throw new ArgumentException("Length of argument 'Str' must be greater than zero.", "Sre");
			if (Index +ADw- 1) 
				throw new ArgumentException("Argument 'Index' must be greater than or equal to 1.", "Index");
			if (Index +AD4- Str.Length)
				throw new ArgumentException("Argument 'Index' must be less than or equal to the length of argument 'String'.", "Index");

			return Str.ToCharArray(Index -1, 1)[0+AF0AOw-
			*/
		}

		//[Test]
		public void InStr()
		{
			InStr_WithOutStartParameter();
			InStr_WithStartParameter();
		}

		// [MonoToDo("Not implemented")]
		//TODO: [Test]
		public void InStr_WithOutStartParameter()
		{
			/*
			return InStr(1, String1, String2, Compare);
			*/
		}
		
		// [MonoToDo("Not implemented")]
		//TODO: [Test]
		public void InStr_WithStartParameter()
		{
			/*
			if (Start +ADw- 1)
				throw new ArgumentException("Argument 'Start' must be non-negative.", "Start");

			 //* FIXME: ms-help://MS.VSCC/MS.MSDNVS/vblr7/html/vafctinstr.htm
			 //* If Compare is omitted, the Option Compare setting determines the type of comparison. Specify 
			 //* a valid LCID (LocaleID) to use locale-specific rules in the comparison.
			 //* How do I do this?
			
			 //* If									InStr returns 
			 //*
			 //* String1 is zero length or Nothing	0 
			 //* String2 is zero length or Nothing	start 
			 //* String2 is not found					0 
			 //* String2 is found within String1		Position where match begins 
			 //* Start +AD4- String2						0 
			 

			//FIXME: someone with a non US setup should test this.
			switch (Compare)
			{
				case CompareMethod.Text:
					return System.Globalization.CultureInfo.CurrentCulture.CompareInfo.IndexOf(String2, String1, Start - 1) + 1;

				case CompareMethod.Binary:
					return String1.IndexOf(String2, Start - 1) + 1;
				default:
					throw new System.ArgumentException("Argument 'Compare' must be CompareMethod.Binary or CompareMethod.Text.", "Compare");
			}
			*/
		}

		//[Test]
		public void InStrRev()
		{
			InStrRev_4Parameters();
			InStrRev_5Parameters();
		}

		// [MonoToDo("Not implemented")]
		//TODO: [Test]
		public void InStrRev_4Parameters()
		{

			// 2 InStrRev functions exists+ACEAIQ- Create tests for both versions+ACE-

			/*
			if ((Start +AD0APQ- 0) +AHwAfA- (Start +ADw- -1))
				throw new ArgumentException("Argument 'Start' must be greater than 0 or equal to -1", "Start");
 
			//FIXME: Use LastIndexOf()
			throw new NotImplementedException();
			*/
		}

		// [MonoToDo("Not implemented")]
		//TODO: [Test]
		public void InStrRev_5Parameters()
		{
		}
			
		// [MonoToDo("Not implemented")]
		//TODO: [Test]
		public void Join_Strings()
		{
			/*
			if (SourceArray +AD0APQ- null)
				throw new ArgumentException("Argument 'SourceArray' can not be null.", "SourceArray");
			if (SourceArray.Rank +AD4- 1)
				throw new ArgumentException("Argument 'SourceArray' can have only one dimension.", "SourceArray");

			return string.Join(Delimiter, SourceArray);
			*/
		}

		// [MonoToDo("Not implemented")]
		//TODO: [Test]
		public void Join_Objects()
		{
			/*
			if (SourceArray +AD0APQ- null)
				throw new ArgumentException("Argument 'SourceArray' can not be null.", "SourceArray");
			if (SourceArray.Rank +AD4- 1)
				throw new ArgumentException("Argument 'SourceArray' can have only one dimension.", "SourceArray");

			string+AFsAXQ- dest;
			dest = new string[SourceArray.Length+AF0AOw-

			SourceArray.CopyTo(dest, 0);
			return string.Join(Delimiter, dest);
			*/
		}

		public void LCase_Char() 
		{
			// buffer current culture
			System.Globalization.CultureInfo CurCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

			// do testings
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/10c", 0, Strings.LCase(Letter_Empty));

			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/10", Strings.AscW("c"), Strings.LCase(Letter_English));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/11", Strings.AscW("電"), Strings.LCase(Letter_Japanese));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/12", 1073, Strings.LCase(Letter_Russian));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/13", 269, Strings.LCase(Letter_Slovakian));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/14", Strings.AscW("보"), Strings.LCase(Letter_Korean));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/15", 1603, Strings.LCase(Letter_Arabic));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/16", Strings.AscW("ä"), Strings.LCase(Letter_German));

			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/20", Strings.AscW("c"), Strings.LCase(Letter_English));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/21", Strings.AscW("電"), Strings.LCase(Letter_Japanese));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/22", 1073, Strings.LCase(Letter_Russian));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/23", 269, Strings.LCase(Letter_Slovakian));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/24", Strings.AscW("보"), Strings.LCase(Letter_Korean));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/25", 1603, Strings.LCase(Letter_Arabic));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/26", Strings.AscW("ä"), Strings.LCase(Letter_German));

			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("ja-JP");
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/30", Strings.AscW("c"), Strings.LCase(Letter_English));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/31", Strings.AscW("電"), Strings.LCase(Letter_Japanese));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/32", 1073, Strings.LCase(Letter_Russian));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/33", 269, Strings.LCase(Letter_Slovakian));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/34", Strings.AscW("보"), Strings.LCase(Letter_Korean));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/35", 1603, Strings.LCase(Letter_Arabic));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/36", Strings.AscW("ä"), Strings.LCase(Letter_German));

			// restore buffered culture
			System.Threading.Thread.CurrentThread.CurrentCulture = CurCulture;
		}

		[Test]
		public void LCase()
		{
			LCase_Char();
			LCase_String();
		}

		public void LCase_String() 
		{
			// buffer current culture
			System.Globalization.CultureInfo CurCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

			// do testings
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/64", null, Strings.LCase(TextStringUninitialized));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/65", null, Strings.LCase(TextStringUninitialized));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/67", String.Empty, Strings.LCase(TextStringEmpty));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/68", "", Strings.LCase(TextStringEmpty));

			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/70", "choose the location for which you want contact information:", Strings.LCase(MSWebSiteContent_English));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/71", "電子メールをはじめ、予定表やアドレス帳などの個人情報、さらには office v. x の各アプリケーションで作成した書類やその他のファイルをリンクさせて統合的に管理することができるコミュニケーション／個人情報管理ソフトウェアです。インターフェースが一新され、高度で多彩な entourage x の機能がより直感的に利用できるようになりました。以前お使いになっていた電子メール アプリケーションからの情報のインポートも容易にできます。", Strings.LCase(MSWebSiteContent_Japanese));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/72", "будете автоматически перенаправлены", Strings.LCase(MSWebSiteContent_Russian));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/73", "čníci náročných používateľovnové portfólio microsoft hardware. špičkové optické bezdrôtové myšky s novou technológiou - nakláňacím kolieskom. nezáleží na tom, aký stôl máte, pokiaľ je na ňom elegantná", Strings.LCase(MSWebSiteContent_Slovakian));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/74", "보안 캠페인 - 스스로 지킨 당신의 pc! 더욱 안전해집니다!", Strings.LCase(MSWebSiteContent_Korean));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/75", "كل الحقوق محفوظة ليديعوت إنترنت", Strings.LCase(ArabynetComWebSiteContent_Arabic));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/76", "äöüäöüß", Strings.LCase(GermanUmlauts_German));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/77", "電子メールをはじめ、予定表やアドレス帳などの個人情報、さらには office v. x の各アプリケーションで作成した書類やその他のファイルをリンクさせて統合的に管理することができるコミュニケーション／個人情報管理ソフトウェアです。インターフェースが一新され、高度で多彩な entourage x の機能がより直感的に利用できるようになりました。以前お使いになっていた電子メール アプリケーションからの情報のインポートも容易にできます。будете автоматически перенаправленыčníci náročných používateľovnové portfólio microsoft hardware. špičkové optické bezdrôtové myšky s novou technológiou - nakláňacím kolieskom. nezáleží na tom, aký stôl máte, pokiaľ je na ňom elegantná보안 캠페인 - 스스로 지킨 당신의 pc! 더욱 안전해집니다!كل الحقوق محفوظة ليديعوت إنترنتäöüäöüß", Strings.LCase(TextStringOfMultipleLanguages));

			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/80", "choose the location for which you want contact information:", Strings.LCase(MSWebSiteContent_English));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/81", "電子メールをはじめ、予定表やアドレス帳などの個人情報、さらには office v. x の各アプリケーションで作成した書類やその他のファイルをリンクさせて統合的に管理することができるコミュニケーション／個人情報管理ソフトウェアです。インターフェースが一新され、高度で多彩な entourage x の機能がより直感的に利用できるようになりました。以前お使いになっていた電子メール アプリケーションからの情報のインポートも容易にできます。", Strings.LCase(MSWebSiteContent_Japanese));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/82", "будете автоматически перенаправлены", Strings.LCase(MSWebSiteContent_Russian));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/83", "čníci náročných používateľovnové portfólio microsoft hardware. špičkové optické bezdrôtové myšky s novou technológiou - nakláňacím kolieskom. nezáleží na tom, aký stôl máte, pokiaľ je na ňom elegantná", Strings.LCase(MSWebSiteContent_Slovakian));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/84", "보안 캠페인 - 스스로 지킨 당신의 pc! 더욱 안전해집니다!", Strings.LCase(MSWebSiteContent_Korean));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/85", "كل الحقوق محفوظة ليديعوت إنترنت", Strings.LCase(ArabynetComWebSiteContent_Arabic));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/86", "äöüäöüß", Strings.LCase(GermanUmlauts_German));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/87", "電子メールをはじめ、予定表やアドレス帳などの個人情報、さらには office v. x の各アプリケーションで作成した書類やその他のファイルをリンクさせて統合的に管理することができるコミュニケーション／個人情報管理ソフトウェアです。インターフェースが一新され、高度で多彩な entourage x の機能がより直感的に利用できるようになりました。以前お使いになっていた電子メール アプリケーションからの情報のインポートも容易にできます。будете автоматически перенаправленыčníci náročných používateľovnové portfólio microsoft hardware. špičkové optické bezdrôtové myšky s novou technológiou - nakláňacím kolieskom. nezáleží na tom, aký stôl máte, pokiaľ je na ňom elegantná보안 캠페인 - 스스로 지킨 당신의 pc! 더욱 안전해집니다!كل الحقوق محفوظة ليديعوت إنترنتäöüäöüß", Strings.LCase(TextStringOfMultipleLanguages));

			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("ja-JP");
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/90", "choose the location for which you want contact information:", Strings.LCase(MSWebSiteContent_English));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/91", "電子メールをはじめ、予定表やアドレス帳などの個人情報、さらには office v. x の各アプリケーションで作成した書類やその他のファイルをリンクさせて統合的に管理することができるコミュニケーション／個人情報管理ソフトウェアです。インターフェースが一新され、高度で多彩な entourage x の機能がより直感的に利用できるようになりました。以前お使いになっていた電子メール アプリケーションからの情報のインポートも容易にできます。", Strings.LCase(MSWebSiteContent_Japanese));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/92", "будете автоматически перенаправлены", Strings.LCase(MSWebSiteContent_Russian));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/93", "čníci náročných používateľovnové portfólio microsoft hardware. špičkové optické bezdrôtové myšky s novou technológiou - nakláňacím kolieskom. nezáleží na tom, aký stôl máte, pokiaľ je na ňom elegantná", Strings.LCase(MSWebSiteContent_Slovakian));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/94", "보안 캠페인 - 스스로 지킨 당신의 pc! 더욱 안전해집니다!", Strings.LCase(MSWebSiteContent_Korean));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/95", "كل الحقوق محفوظة ليديعوت إنترنت", Strings.LCase(ArabynetComWebSiteContent_Arabic));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/96", "äöüäöüß", Strings.LCase(GermanUmlauts_German));
			NUnit.Framework.Assertion.AssertEquals ("JW#LCase/97", "電子メールをはじめ、予定表やアドレス帳などの個人情報、さらには office v. x の各アプリケーションで作成した書類やその他のファイルをリンクさせて統合的に管理することができるコミュニケーション／個人情報管理ソフトウェアです。インターフェースが一新され、高度で多彩な entourage x の機能がより直感的に利用できるようになりました。以前お使いになっていた電子メール アプリケーションからの情報のインポートも容易にできます。будете автоматически перенаправленыčníci náročných používateľovnové portfólio microsoft hardware. špičkové optické bezdrôtové myšky s novou technológiou - nakláňacím kolieskom. nezáleží na tom, aký stôl máte, pokiaľ je na ňom elegantná보안 캠페인 - 스스로 지킨 당신의 pc! 더욱 안전해집니다!كل الحقوق محفوظة ليديعوت إنترنتäöüäöüß", Strings.LCase(TextStringOfMultipleLanguages));

			// restore buffered culture
			System.Threading.Thread.CurrentThread.CurrentCulture = CurCulture;
		}

		
		[Test]
		public void Left()
		{
			NUnit.Framework.Assertion.AssertEquals ("JW#40", "ä電電", Strings.Left("ä電電jklmeh",3));
			NUnit.Framework.Assertion.AssertEquals ("JW#41", "jk", Strings.Left("jklmeh",2));
			NUnit.Framework.Assertion.AssertEquals ("JW#42", "", Strings.Left("jklmeh",0));
			try
			{
				object buffer = Strings.Left("jklmeh",-1);
				NUnit.Framework.Assertion.Fail ("JW#43 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#43", true);
			}
			NUnit.Framework.Assertion.AssertEquals ("JW#44", "j", Strings.Left("j",2));
		}

		[Test]
		public void Len()
		{
			Len_Object();
			Len_Bool();
			Len_Byte();
			Len_Char();
			Len_Double();
			Len_Int();
			Len_Long();
			Len_Short();
			Len_Float();
			Len_String();
			Len_DateTime();
			Len_Decimal();
		}

		public void Len_Bool()
		{
			try
			{
				object buffer = Strings.Len(null);
				NUnit.Framework.Assertion.Fail ("JW#Len/50 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#Len/50", true);
			}
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/51", 2, Strings.Len(true));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/52", 2, Strings.Len(false));
			/*
			return 2; //sizeof(bool)
			*/
		}

		public void Len_Byte()
		{
			byte MyByte1a = 3;
			const byte MyByte1 = 3;
			const byte MyByte2 = 13;
			const byte MyByte3 = 123;
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/60", 1, Strings.Len(MyByte1a));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/61", 1, Strings.Len(MyByte1));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/62", 1, Strings.Len(MyByte2));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/63", 1, Strings.Len(MyByte3));
		}
		
		public void Len_Char()
		{
			char MyChar0 = (char)0;
			char MyChar1 = (char)73;
			char MyChar1a = (char)1024;
			char MyChar2 = (char)65000;
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/71", 2, Strings.Len(MyChar0));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/72", 2, Strings.Len(MyChar1));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/73", 2, Strings.Len(MyChar1a));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/74", 2, Strings.Len(MyChar2));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/74", 2, Strings.Len(Letter_Empty));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/74", 2, Strings.Len(Letter_German));
			/*
			return 2; //sizeof(char)
			*/
		}
		
		public void Len_Double()
		{
			double MyChar0 = (double)0;
			double MyChar1 = (double)73;
			double MyChar1a = (double)-1024;
			double MyChar2 = (double)65000.398721733;
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/81", 8, Strings.Len(MyChar0));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/82", 8, Strings.Len(MyChar1));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/83", 8, Strings.Len(MyChar1a));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/84", 8, Strings.Len(MyChar2));
			/*
			return 8; //sizeof(double)
			*/
		}
		
		public void Len_Int()
		{
			int MyChar0 = (int)0;
			int MyChar1 = (int)73;
			int MyChar1a = (int)-1024;
			int MyChar2 = (int)65000;
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/91", 4, Strings.Len(MyChar0));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/92", 4, Strings.Len(MyChar1));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/93", 4, Strings.Len(MyChar1a));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/94", 4, Strings.Len(MyChar2));
			/*
			return 4; //sizeof(int)
			*/
		}
		
		public void Len_Long()
		{
			long MyChar0 = (long)0;
			long MyChar1 = (long)73;
			long MyChar1a = (long)-1024;
			long MyChar2 = (long)65000.398721733;
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/101", 8, Strings.Len(MyChar0));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/102", 8, Strings.Len(MyChar1));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/103", 8, Strings.Len(MyChar1a));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/104", 8, Strings.Len(MyChar2));
			/*
			return 8; //sizeof(long)
			*/
		}

		class TMyLenTestClass
		{
			public void esss()
			{
			}

			int dummy = 16;
			const int me34 = 23;
		}

		class TMyLenTestClass2
		{
			public void esss()
			{
				string dddd = "kjdfhdjfh";
			}

			public void esss2()
			{
				string dddd = "kjdfhdjfh";
			}

			int dummy = 16;
			const int me34 = 23;
		}

		struct TMyLenTestStruct
		{
			public int dummy1;
			public double dummy2;
			double dummy3;
		}

		struct TMyLenTestStruct2
		{
			public int dummy1;
			double dummy3;
		}

		public void Len_Object()
		{
			object ObjectOfTextStringEmpty = TextStringEmpty;
			object ObjectOfTextStringUninitialized = TextStringUninitialized;
			object ObjectOfTextStringOfMultipleLanguages = TextStringOfMultipleLanguages;
			object ObjectOfInt = (int)0;
			object ObjectOfEmptyObject = null;
			TMyLenTestStruct MyLenTestStruct;
			MyLenTestStruct = new TMyLenTestStruct();
			MyLenTestStruct.dummy1 = 34;
			MyLenTestStruct.dummy2 = 34.343;
			object ObjectOfTMyLenTestStruct = MyLenTestStruct;
			TMyLenTestClass MyLenTestClass = new TMyLenTestClass();
			object ObjectOfTMyLenTestClass = MyLenTestClass;
			TMyLenTestStruct2 MyLenTestStruct2;
			MyLenTestStruct2 = new TMyLenTestStruct2();
			MyLenTestStruct2.dummy1 = 34;
			object ObjectOfTMyLenTestStruct2 = MyLenTestStruct2;
			TMyLenTestClass2 MyLenTestClass2 = new TMyLenTestClass2();
			object ObjectOfTMyLenTestClass2 = MyLenTestClass2;
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/111", 0, Strings.Len(ObjectOfTextStringEmpty));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/112", 0, Strings.Len(ObjectOfTextStringUninitialized));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/113", 525, Strings.Len(ObjectOfTextStringOfMultipleLanguages));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/114", 4, Strings.Len(ObjectOfInt));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/115", 0, Strings.Len(ObjectOfEmptyObject));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/116", 12, Strings.Len(ObjectOfTMyLenTestStruct));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/118", 4, Strings.Len(ObjectOfTMyLenTestStruct2));
			try
			{
				int buffer = Strings.Len(ObjectOfTMyLenTestClass);
				NUnit.Framework.Assertion.Fail ("JW#Len/117 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#Len/117", true);
			}
			try
			{
				int buffer = Strings.Len(ObjectOfTMyLenTestClass2);
				NUnit.Framework.Assertion.Fail ("JW#Len/119 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#Len/119", true);
			}
			/*
			// FIXME: 
			// With user-defined types and Object variables, the Len function returns the size as it will 
			// be written to the file. If an Object contains a String, it will return the length of the string. 
			// If an Object contains any other type, it will return the size of the object as it will be written 
			// to the file.
			throw new NotImplementedException(); 
			*/
		}
		
		public void Len_Short()
		{
			short MyChar0 = (short)0;
			short MyChar1 = (short)73;
			short MyChar1a = (short)-1024;
			short MyChar2 = short.MaxValue;
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/121", 2, Strings.Len(MyChar0));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/122", 2, Strings.Len(MyChar1));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/123", 2, Strings.Len(MyChar1a));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/124", 2, Strings.Len(MyChar2));
		}
		
		public void Len_Float()
		{
			float MyChar0 = (float)0;
			float MyChar1 = (float)73;
			float MyChar1a = (float)-1024;
			float MyChar2 = (float)-465000.398721733;
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/131", 4, Strings.Len(MyChar0));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/132", 4, Strings.Len(MyChar1));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/133", 4, Strings.Len(MyChar1a));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/134", 4, Strings.Len(MyChar2));
		}
		
		public void Len_String()
		{
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/161", 0, Strings.Len(TextStringEmpty));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/162", 0, Strings.Len(TextStringUninitialized));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/163", 525, Strings.Len(TextStringOfMultipleLanguages));
		}
		
		public void Len_DateTime()
		{
			System.DateTime MyChar0 = new System.DateTime(1000,1,1);
			System.DateTime MyChar1 = new System.DateTime(2003,12,29);
			System.DateTime MyChar1a = System.DateTime.Now;
			System.DateTime MyChar2 = new System.DateTime(9999,12,31,23,59,59,999);
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/141", 8, Strings.Len(MyChar0));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/142", 8, Strings.Len(MyChar1));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/143", 8, Strings.Len(MyChar1a));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/144", 8, Strings.Len(MyChar2));
		}
		
		public void Len_Decimal()
		{
			decimal MyChar0 = (decimal)0;
			decimal MyChar1 = (decimal)-3840;
			decimal MyChar1a = (decimal)-29843433.23984723894333333;
			decimal MyChar2 = (decimal)2934838384323432333;
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/151", 8, Strings.Len(MyChar0));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/152", 8, Strings.Len(MyChar1));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/153", 8, Strings.Len(MyChar1a));
			NUnit.Framework.Assertion.AssertEquals ("JW#Len/154", 8, Strings.Len(MyChar2));
		}

		[Test]
		public void LSet()
		{
			NUnit.Framework.Assertion.AssertEquals ("JW#LSet/10", "  lf", Strings.LSet("  lfkdfkd  ", 4));
			NUnit.Framework.Assertion.AssertEquals ("JW#LSet/11", "", Strings.LSet("  lfkdfkd ", 0));
			try
			{
				string buffer = Strings.LSet("  lfkdfkd  ", -1);
				NUnit.Framework.Assertion.Fail ("JW#LSet/12 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#LSet/12", true);
			}
			NUnit.Framework.Assertion.AssertEquals ("JW#LSet/11", "  lfkdfkd           ", Strings.LSet("  lfkdfkd ", 20));
		}

		[Test]
		public void LTrim()
		{
			NUnit.Framework.Assertion.AssertEquals ("JW#LTrim/10", "dkfÄ   ", Strings.LTrim("    dkfÄ   "));
			NUnit.Framework.Assertion.AssertEquals ("JW#LTrim/11", "", Strings.LTrim("       "));
			NUnit.Framework.Assertion.AssertEquals ("JW#LTrim/12", "", Strings.LTrim(null));
			NUnit.Framework.Assertion.AssertEquals ("JW#LTrim/13", "", Strings.LTrim(String.Empty));
		}

		[Test]
		public void RTrim()
		{
			NUnit.Framework.Assertion.AssertEquals ("JW#RTrim/10", "    dkfÄ", Strings.RTrim("    dkfÄ   "));
			NUnit.Framework.Assertion.AssertEquals ("JW#RTrim/11", "", Strings.RTrim("       "));
			NUnit.Framework.Assertion.AssertEquals ("JW#RTrim/12", "", Strings.RTrim(null));
			NUnit.Framework.Assertion.AssertEquals ("JW#RTrim/13", "", Strings.RTrim(String.Empty));
		}
	
		[Test]
		public void Trim() 
		{
			NUnit.Framework.Assertion.AssertEquals ("JW#Trim/10", "dkfÄ", Strings.Trim("    dkfÄ   "));
			NUnit.Framework.Assertion.AssertEquals ("JW#Trim/11", "", Strings.Trim("       "));
			NUnit.Framework.Assertion.AssertEquals ("JW#Trim/12", "", Strings.Trim(null));
			NUnit.Framework.Assertion.AssertEquals ("JW#Trim/13", "", Strings.Trim(String.Empty));
		}

		[Test]
		public void Mid()
		{
			Mid_WithLengthParameter();
			Mid_WithOutLengthParameter();
		}

		public void Mid_WithLengthParameter()
		{
			try
			{
				string buffer = Strings.Mid(String.Empty,0,0);
				NUnit.Framework.Assertion.Fail ("JW#Mid/10 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#Mid/10", true);
			}
			try
			{
				string buffer = Strings.Mid(String.Empty,0,1);
				NUnit.Framework.Assertion.Fail ("JW#Mid/11 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#Mid/11", true);
			}
			NUnit.Framework.Assertion.AssertEquals ("JW#Mid/12", "", Strings.Mid(String.Empty,1,0));
			NUnit.Framework.Assertion.AssertEquals ("JW#Mid/13", "", Strings.Mid(String.Empty,1,1));
			try
			{
				string buffer = Strings.Mid(String.Empty,-1,0);
				NUnit.Framework.Assertion.Fail ("JW#Mid/14 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#Mid/14", true);
			}
			try
			{
				string buffer = Strings.Mid(String.Empty,1,-1);
				NUnit.Framework.Assertion.Fail ("JW#Mid/15 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#Mid/15", true);
			}
			
			NUnit.Framework.Assertion.AssertEquals ("JW#Mid/22", "", Strings.Mid(GermanUmlauts_German,1,0));
			NUnit.Framework.Assertion.AssertEquals ("JW#Mid/23", "ä", Strings.Mid(GermanUmlauts_German,1,1));
			NUnit.Framework.Assertion.AssertEquals ("JW#Mid/27", "öüÄÖ", Strings.Mid(GermanUmlauts_German,2,4));
			NUnit.Framework.Assertion.AssertEquals ("JW#Mid/28", "öüÄÖÜß", Strings.Mid(GermanUmlauts_German,2,400));

			NUnit.Framework.Assertion.AssertEquals ("JW#Mid/32", "", Strings.Mid(ArabynetComWebSiteContent_Arabic,1,0));
			NUnit.Framework.Assertion.AssertEquals ("JW#Mid/33", "ك", Strings.Mid(ArabynetComWebSiteContent_Arabic,1,1));
			NUnit.Framework.Assertion.AssertEquals ("JW#Mid/37", "ل ال", Strings.Mid(ArabynetComWebSiteContent_Arabic,2,4));
			NUnit.Framework.Assertion.AssertEquals ("JW#Mid/38", "ل الحقوق محفوظة ليديعوت إنترنت", Strings.Mid(ArabynetComWebSiteContent_Arabic,2,400));
		}

		public void Mid_WithOutLengthParameter ()
		{
			try
			{
				string buffer = Strings.Mid(String.Empty,0);
				NUnit.Framework.Assertion.Fail ("JW#Mid/60 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#Mid/60", true);
			}
			NUnit.Framework.Assertion.AssertEquals ("JW#Mid/62", "", Strings.Mid(String.Empty,1));
			try
			{
				string buffer = Strings.Mid(String.Empty,-1);
				NUnit.Framework.Assertion.Fail ("JW#Mid/64 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#Mid/64", true);
			}
			
			NUnit.Framework.Assertion.AssertEquals ("JW#Mid/73", "äöüÄÖÜß", Strings.Mid(GermanUmlauts_German,1));
			NUnit.Framework.Assertion.AssertEquals ("JW#Mid/78", "öüÄÖÜß", Strings.Mid(GermanUmlauts_German,2));

			NUnit.Framework.Assertion.AssertEquals ("JW#Mid/82", "كل الحقوق محفوظة ليديعوت إنترنت", Strings.Mid(ArabynetComWebSiteContent_Arabic,1));
			NUnit.Framework.Assertion.AssertEquals ("JW#Mid/88", "ل الحقوق محفوظة ليديعوت إنترنت", Strings.Mid(ArabynetComWebSiteContent_Arabic,2));
		}

		[Test]
		public void Replace()
		{
			string buffer;

			buffer = GermanUmlauts_German + GermanUmlauts_German + GermanUmlauts_German + GermanUmlauts_German;

			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/10", null, Strings.Replace(String.Empty, "ÄÖ", "deee",1,-1,CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/11", null, Strings.Replace(String.Empty, "ÄÖ", "deee",1,-1,CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/12", null, Strings.Replace(null, "ÄÖ", "deee",1,-1,CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/13", null, Strings.Replace(null, "ÄÖ", "deee",1,-1,CompareMethod.Text));

			try
			{
				string buffer2 = Strings.Replace(buffer, "ÄÖ", "ÄÖ",0,0,CompareMethod.Binary);
				NUnit.Framework.Assertion.Fail ("JW#Replace/20 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#Replace/20", true);
			}
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/21", "ßäöüÄÖÜßäöüÄÖÜßäöüÄÖÜß", Strings.Replace(buffer, "ÄÖ", "ÄÖ",7,-1,CompareMethod.Binary));
			try
			{
				string buffer2 = Strings.Replace(buffer, "ÄÖ", "ÄÖ",7,-2,CompareMethod.Binary);
				NUnit.Framework.Assertion.Fail ("JW#Replace/22 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#Replace/22", true);
			}
			try
			{
				string buffer2 = Strings.Replace(buffer, "ÄÖ", "ÄÖ",-1,0,CompareMethod.Binary);
				NUnit.Framework.Assertion.Fail ("JW#Replace/23 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#Replace/23", true);
			}
			try
			{
				string buffer2 = Strings.Replace(buffer, "ÄÖ", "ÄÖ",-2,0,CompareMethod.Binary);
				NUnit.Framework.Assertion.Fail ("JW#Replace/24 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#Replace/24", true);
			}
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/25", "äöüÄÖÜßäöüÄÖÜßäöüÄÖÜßäöüÄÖÜß", Strings.Replace(buffer, "ÄÖ", "ÄÖ",1,0,CompareMethod.Binary));
			try
			{
				string buffer2 = Strings.Replace(buffer, "ÄÖ", "ÄÖ",0,1,CompareMethod.Binary);
				NUnit.Framework.Assertion.Fail ("JW#Replace/26 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#Replace/26", true);
			}
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/27", "äöüÄÖÜßäöüÄÖÜßäöüÄÖÜßäöüÄÖÜß", Strings.Replace(buffer, "ÄÖ", "ÄÖ",1,1,CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/28", "ßäöüÄÖÜßäöüÄÖÜßäöüÄÖÜß", Strings.Replace(buffer, "ÄÖ", "ÄÖ",7,1,CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/29", "ßäöüÄÖÜßäöüÄÖÜßäöüÄÖÜß", Strings.Replace(buffer, "ÄÖ", "ÄÖ",7,2,CompareMethod.Binary));
			
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/31", "ßäöüudddÜßäöüudddÜßäöüudddÜß", Strings.Replace(buffer, "ÄÖ", "uddd",7,-1,CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/35", "äöüÄÖÜßäöüÄÖÜßäöüÄÖÜßäöüÄÖÜß", Strings.Replace(buffer, "ÄÖ", "uddd",1,0,CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/37", "äöüudddÜßäöüÄÖÜßäöüÄÖÜßäöüÄÖÜß", Strings.Replace(buffer, "ÄÖ", "uddd",1,1,CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/38", "ßäöüudddÜßäöüÄÖÜßäöüÄÖÜß", Strings.Replace(buffer, "ÄÖ", "uddd",7,1,CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/39", "ßäöüudddÜßäöüudddÜßäöüÄÖÜß", Strings.Replace(buffer, "ÄÖ", "uddd",7,2,CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/39a", "", Strings.Replace(buffer, "äöüÄÖÜß", "",1,-1,CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/39b", String.Empty, Strings.Replace(buffer, "äöüÄÖÜß", "",1,-1,CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/39c", null, Strings.Replace(buffer, "äöüÄÖÜß", "",400,-1,CompareMethod.Binary));
			
			buffer = ArabynetComWebSiteContent_Arabic + ArabynetComWebSiteContent_Arabic;
			//ArabynetComWebSiteContent_Arabic = "كل الحقوق محفوظة ليديعوت إنترنت";
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/41", "وق محفوظة ليديعوت إنترنتكل الحقوق محفوظة ليديعوت إنترنت", Strings.Replace(buffer, "ÄÖ", "uddd",8,-1,CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/45", "كل الحقوق محفوظة ليديعوت إنترنتكل الحقوق محفوظة ليديعوت إنترنت", Strings.Replace(buffer, "ÄÖ", "uddd",1,0,CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/47", "كل الحقوق محفوظة ليديعوت إنترنتكل الحقوق محفوظة ليديعوت إنترنت", Strings.Replace(buffer, "ÄÖ", "uddd",1,1,CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/48", "وق محفوظة ليديعوت إنترنتكل الحقوق محفوظة ليديعوت إنترنت", Strings.Replace(buffer, "ÄÖ", "uddd",8,1,CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/49", "وق محفوظة ليديعوت إنترنتكل الحقوق محفوظة ليديعوت إنترنت", Strings.Replace(buffer, "ÄÖ", "uddd",8,2,CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/49a", "", Strings.Replace(buffer, "كل الحقوق محفوظة ليديعوت إنترنت", "",1,-1,CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/49b", String.Empty, Strings.Replace(buffer, "كل الحقوق محفوظة ليديعوت إنترنت", "",1,-1,CompareMethod.Binary));

			buffer = MSWebSiteContent_Korean + MSWebSiteContent_Korean;
			//MSWebSiteContent_Korean = "보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!";
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/51", " - 스스로 지킨 당신의 PC! 더욱 안전해집니다!보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!", Strings.Replace(buffer, "ÄÖ", "uddd",7,-1,CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/55", "보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!", Strings.Replace(buffer, "ÄÖ", "uddd",1,0,CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/57", "보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!", Strings.Replace(buffer, "ÄÖ", "uddd",1,1,CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/58", " - 스스로 지킨 당신의 PC! 더욱 안전해집니다!보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!", Strings.Replace(buffer, "ÄÖ", "uddd",7,1,CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/59", " - 스스로 지킨 당신의 PC! 더욱 안전해집니다!보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!", Strings.Replace(buffer, "ÄÖ", "uddd",7,2,CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/59a", "", Strings.Replace(buffer, "보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!", "",1,-1,CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/59b", String.Empty, Strings.Replace(buffer, "보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!", "",1,-1,CompareMethod.Binary));

			buffer = GermanUmlauts_German + GermanUmlauts_German + GermanUmlauts_German + GermanUmlauts_German;
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/61", "ßudddüudddÜßudddüudddÜßudddüudddÜß", Strings.Replace(buffer, "ÄÖ", "uddd",7,-1,CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/65", "äöüÄÖÜßäöüÄÖÜßäöüÄÖÜßäöüÄÖÜß", Strings.Replace(buffer, "ÄÖ", "uddd",1,0,CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/67", "udddüÄÖÜßäöüÄÖÜßäöüÄÖÜßäöüÄÖÜß", Strings.Replace(buffer, "ÄÖ", "uddd",1,1,CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/68", "ßudddüÄÖÜßäöüÄÖÜßäöüÄÖÜß", Strings.Replace(buffer, "ÄÖ", "uddd",7,1,CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/69", "ßudddüudddÜßäöüÄÖÜßäöüÄÖÜß", Strings.Replace(buffer, "ÄÖ", "uddd",7,2,CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/69a", "ßßßß", Strings.Replace(buffer, "äöü", "",1,-1,CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/69b", String.Empty, Strings.Replace(buffer, "äöüÄÖÜß", "",1,-1,CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/69c", null, Strings.Replace(buffer, "äöüÄÖÜß", "",400,-1,CompareMethod.Text));

			buffer = ArabynetComWebSiteContent_Arabic + ArabynetComWebSiteContent_Arabic;
			//ArabynetComWebSiteContent_Arabic = "كل الحقوق محفوظة ليديعوت إنترنت";
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/71", "وق محفوظة ليديعوت إنترنتكل الحقوق محفوظة ليديعوت إنترنت", Strings.Replace(buffer, "ÄÖ", "uddd",8,-1,CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/75", "كل الحقوق محفوظة ليديعوت إنترنتكل الحقوق محفوظة ليديعوت إنترنت", Strings.Replace(buffer, "ÄÖ", "uddd",1,0,CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/77", "كل الحقوق محفوظة ليديعوت إنترنتكل الحقوق محفوظة ليديعوت إنترنت", Strings.Replace(buffer, "ÄÖ", "uddd",1,1,CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/78", "وق محفوظة ليديعوت إنترنتكل الحقوق محفوظة ليديعوت إنترنت", Strings.Replace(buffer, "ÄÖ", "uddd",8,1,CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/79", "وق محفوظة ليديعوت إنترنتكل الحقوق محفوظة ليديعوت إنترنت", Strings.Replace(buffer, "ÄÖ", "uddd",8,2,CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/79a", "", Strings.Replace(buffer, "كل الحقوق محفوظة ليديعوت إنترنت", "",1,-1,CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/79b", String.Empty, Strings.Replace(buffer, "كل الحقوق محفوظة ليديعوت إنترنت", "",1,-1,CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/79c", "uddd", Strings.Replace(buffer, Strings.UCase("وق محفوظة ليديعوت إنترنتكل الحقوق محفوظة ليديعوت إنترنت"), "uddd",8,2,CompareMethod.Text));

			buffer = MSWebSiteContent_Korean + MSWebSiteContent_Korean;
			//MSWebSiteContent_Korean = "보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!";
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/81", " - 스스로 지킨 당신의 PC! 더욱 안전해집니다!보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!", Strings.Replace(buffer, "ÄÖ", "uddd",7,-1,CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/85", "보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!", Strings.Replace(buffer, "ÄÖ", "uddd",1,0,CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/87", "보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!", Strings.Replace(buffer, "ÄÖ", "uddd",1,1,CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/88", " - 스스로 지킨 당신의 PC! 더욱 안전해집니다!보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!", Strings.Replace(buffer, "ÄÖ", "uddd",7,1,CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/89", " - 스스로 지킨 당신의 PC! 더욱 안전해집니다!보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!", Strings.Replace(buffer, "ÄÖ", "uddd",7,2,CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/89a", "", Strings.Replace(buffer, "보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!", "",1,-1,CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW#Replace/89b", String.Empty, Strings.Replace(buffer, "보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!", "",1,-1,CompareMethod.Text));
		}
		
		[Test]
		public void Right()
		{
			try
			{
				string buffer2 = Strings.Right(TextStringOfMultipleLanguages, 0);
				NUnit.Framework.Assertion.Fail ("JW#Right/20 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#Right/20", true);
			}

			NUnit.Framework.Assertion.AssertEquals ("JW#Right/21", "يديعوت إنترنتäöüÄÖÜß", Strings.Right(TextStringOfMultipleLanguages, 20));
			NUnit.Framework.Assertion.AssertEquals ("JW#Right/22", TextStringOfMultipleLanguages, Strings.Right(TextStringOfMultipleLanguages, 20000));
		
		}

		[Test]
		public void RSet()
		{
			NUnit.Framework.Assertion.AssertEquals ("JW#RSet/10", "  lf", Strings.RSet("  lfkdfkd  ", 4));
			NUnit.Framework.Assertion.AssertEquals ("JW#RSet/11", "", Strings.RSet("  lfkdfkd ", 0));
			try
			{
				string buffer = Strings.RSet("  lfkdfkd  ", -1);
				NUnit.Framework.Assertion.Fail ("JW#RSet/12 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#RSet/12", true);
			}
			NUnit.Framework.Assertion.AssertEquals ("JW#RSet/11", "            lfkdfkd ", Strings.RSet("  lfkdfkd ", 20));
		}

		[Test]
		public void Space() 
		{
			NUnit.Framework.Assertion.AssertEquals ("JW*Space#01", "", Strings.Space(0));
			NUnit.Framework.Assertion.AssertEquals ("JW*Space#02", "  ", Strings.Space(2));
			NUnit.Framework.Assertion.AssertEquals ("JW*Space#03", Strings.Chr(32).ToString() + Strings.Chr(32).ToString(), Strings.Space(2));
			NUnit.Framework.Assertion.AssertEquals ("JW*Space#03a", String.Concat(Strings.Chr(32).ToString(), Strings.Chr(32).ToString()), Strings.Space(2));
			NUnit.Framework.Assertion.AssertEquals ("JW*Space#04", "        ", Strings.Space(8));
			try
			{
				string buf = Strings.Space(-1);
				NUnit.Framework.Assertion.Fail ("JW*Space#10");
			}
			catch
			{
			}
		}

		[Test]
		public void Split()
		{
			string buffer = GermanUmlauts_German + GermanUmlauts_German + GermanUmlauts_German + GermanUmlauts_German;
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#01", 1, Strings.Split("äö","ü", 0, CompareMethod.Binary).Length);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#01a", "", Strings.Split(String.Empty," ", -1, CompareMethod.Binary)[0]);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#01a", 1, Strings.Split(String.Empty," ", -1, CompareMethod.Binary).Length);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#01b", 1, Strings.Split(String.Empty,"", -1, CompareMethod.Binary).Length);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#01b", "", Strings.Split(String.Empty,"", -1, CompareMethod.Binary)[0]);
			try
			{
				string buffer2 = Strings.Split(buffer,"ü", 0, CompareMethod.Binary).Length.ToString();
				NUnit.Framework.Assertion.Fail ("JW#Split#01c hasn't thrown an error");
			}
			catch
			{
			}
			try
			{
				string buffer2 = Strings.Split(buffer,"ü", -2, CompareMethod.Binary).Length.ToString();
				NUnit.Framework.Assertion.Fail ("JW#Split#02 hasn't thrown an error");
			}
			catch
			{
			}
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#10", 5, Strings.Split(buffer,"ü", -1, CompareMethod.Binary).Length);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#11", 2, Strings.Split(buffer,"ü", 2, CompareMethod.Binary).Length);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#12", 9, Strings.Split(buffer,"ü", -1, CompareMethod.Text).Length);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#13", 1, Strings.Split(buffer,"ü", 1, CompareMethod.Text).Length);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#14", 1, Strings.Split(buffer,"", -1, CompareMethod.Binary).Length);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#15", 1, Strings.Split(buffer,"", 2, CompareMethod.Binary).Length);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#16", 1, Strings.Split(buffer,"", -1, CompareMethod.Text).Length);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#17", 1, Strings.Split(buffer,"", 1, CompareMethod.Text).Length);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#14a", 5, Strings.Split(buffer,"äö", -1, CompareMethod.Binary).Length);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#15a", 2, Strings.Split(buffer,"äö", 2, CompareMethod.Binary).Length);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#16a", 9, Strings.Split(buffer,"äö", -1, CompareMethod.Text).Length);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#17a", 1, Strings.Split(buffer,"äö", 1, CompareMethod.Text).Length);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#14b", "üÄÖÜß", Strings.Split(buffer,"äö", -1, CompareMethod.Binary)[4]);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#15b", "üÄÖÜßäöüÄÖÜßäöüÄÖÜßäöüÄÖÜß", Strings.Split(buffer,"äö", 2, CompareMethod.Binary)[1]);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#16b", "Üß", Strings.Split(buffer,"äö", -1, CompareMethod.Text)[8]);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#17b", "äöüÄÖÜßäöüÄÖÜßäöüÄÖÜßäöüÄÖÜß", Strings.Split(buffer,"äö", 1, CompareMethod.Text)[0]);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#17c", "", Strings.Split(buffer,"äö", 2, CompareMethod.Text)[0]);

			string myString = "Look at these!";
			string[] myReturn = {"Look", "at", "these!"};
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#18", 3, Strings.Split(myString, " ", -1, CompareMethod.Binary).Length);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#18a", myReturn[0], Strings.Split(myString, " ", -1, CompareMethod.Binary)[0]);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#18b", myReturn[1], Strings.Split(myString, " ", -1, CompareMethod.Binary)[1]);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#18c", myReturn[2], Strings.Split(myString, " ", -1, CompareMethod.Binary)[2]);

			NUnit.Framework.Assertion.AssertEquals ("JW*Split#20", "äöüÄÖ", Strings.Split(buffer,"Ü", -1, CompareMethod.Binary)[0]);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#21", "äöüÄÖ", Strings.Split(buffer,"Ü", 2, CompareMethod.Binary)[0]);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#22", "äö", Strings.Split(buffer,"Ü", -1, CompareMethod.Text)[0]);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#23", "äöüÄÖÜßäöüÄÖÜßäöüÄÖÜßäöüÄÖÜß", Strings.Split(buffer,"Ü", 1, CompareMethod.Text)[0]);
			NUnit.Framework.Assertion.AssertEquals ("JW*Split#24", "äö", Strings.Split(buffer,"Ü", 2, CompareMethod.Text)[0]);

		}

		[Test]
		public void StrComp()
		{
			NUnit.Framework.Assertion.AssertEquals ("JW*StrComp#01", -1, Strings.StrComp(Strings.UCase(TextStringOfMultipleLanguages),TextStringOfMultipleLanguages, CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrComp#02", 1, Strings.StrComp(Strings.LCase(TextStringOfMultipleLanguages),TextStringOfMultipleLanguages, CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrComp#03", 0, Strings.StrComp(TextStringOfMultipleLanguages,TextStringOfMultipleLanguages, CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrComp#04", -1, Strings.StrComp(String.Empty,TextStringOfMultipleLanguages, CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrComp#05", -1, Strings.StrComp(null,TextStringOfMultipleLanguages, CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrComp#06", -1, Strings.StrComp(" " + TextStringOfMultipleLanguages,TextStringOfMultipleLanguages, CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrComp#06a", -1, Strings.StrComp(Strings.ChrW(6444) + TextStringOfMultipleLanguages,TextStringOfMultipleLanguages, CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrComp#07", 1, Strings.StrComp(TextStringOfMultipleLanguages + " ",TextStringOfMultipleLanguages, CompareMethod.Binary));

			NUnit.Framework.Assertion.AssertEquals ("JW*StrComp#11", 0, Strings.StrComp(Strings.UCase(TextStringOfMultipleLanguages),TextStringOfMultipleLanguages, CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrComp#12", 0, Strings.StrComp(Strings.LCase(TextStringOfMultipleLanguages),TextStringOfMultipleLanguages, CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrComp#13", 0, Strings.StrComp(TextStringOfMultipleLanguages,TextStringOfMultipleLanguages, CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrComp#14", -1, Strings.StrComp(String.Empty,TextStringOfMultipleLanguages, CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrComp#15", -1, Strings.StrComp(null,TextStringOfMultipleLanguages, CompareMethod.Binary));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrComp#06", -1, Strings.StrComp(" " + TextStringOfMultipleLanguages,TextStringOfMultipleLanguages, CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrComp#06a", 0, Strings.StrComp(Strings.ChrW(10654) + TextStringOfMultipleLanguages,TextStringOfMultipleLanguages, CompareMethod.Text)); //seems to be a MS bug?
			NUnit.Framework.Assertion.AssertEquals ("JW*StrComp#06b", -1, Strings.StrComp(Strings.ChrW(88) + TextStringOfMultipleLanguages,TextStringOfMultipleLanguages, CompareMethod.Text));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrComp#07", 1, Strings.StrComp(TextStringOfMultipleLanguages + " ",TextStringOfMultipleLanguages, CompareMethod.Text));
		}

		// TODO: Chinese testings
		[Test]
		public void StrConv ()
		{
			// buffer current culture
			System.Globalization.CultureInfo CurCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

			// do testings

			// ***********
			// ** EN-US **
			// ***********

			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			
			//requires japanese systems
			//NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#02", 1, Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.Hiragana, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			//NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#03", 1, Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.Katakana, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			
			//TODO: how to write VbStrConv.LinguisticCasing + VbStrConv.UpperCase correctly?
			//NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#04", 1, Strings.StrConv(TextStringOfMultipleLanguages, (VbStrConv.LinguisticCasing + VbStrConv.UpperCase), System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			//NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#04a", 1, Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.LinguisticCasing || VbStrConv.LowerCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			
			//not supported
			//NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#05", 1, Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.Narrow, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			//NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#11", 1, Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.Wide, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));

			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#06", TextStringOfMultipleLanguages, Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.None, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#01", "電子メールをはじめ、予定表やアドレス帳などの個人情報、さらには office v. x の各アプリケーションで作成した書類やその他のファイルをリンクさせて統合的に管理することができるコミュニケーション／個人情報管理ソフトウェアです。インターフェースが一新され、高度で多彩な entourage x の機能がより直感的に利用できるようになりました。以前お使いになっていた電子メール アプリケーションからの情報のインポートも容易にできます。будете автоматически перенаправленыčníci náročných používateľovnové portfólio microsoft hardware. špičkové optické bezdrôtové myšky s novou technológiou - nakláňacím kolieskom. nezáleží na tom, aký stôl máte, pokiaľ je na ňom elegantná보안 캠페인 - 스스로 지킨 당신의 pc! 더욱 안전해집니다!كل الحقوق محفوظة ليديعوت إنترنتäöüäöüß", Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.LowerCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#07", "電子メールをはじめ、予定表やアドレス帳などの個人情報、さらには Office V. X の各アプリケーションで作成した書類やその他のファイルをリンクさせて統合的に管理することができるコミュニケーション／個人情報管理ソフトウェアです。インターフェースが一新され、高度で多彩な Entourage X の機能がより直感的に利用できるようになりました。以前お使いになっていた電子メール アプリケーションからの情報のインポートも容易にできます。Будете Автоматически Перенаправленыčníci Náročných Používateľovnové Portfólio Microsoft Hardware. Špičkové Optické Bezdrôtové Myšky S Novou Technológiou - Nakláňacím Kolieskom. Nezáleží Na Tom, Aký Stôl Máte, Pokiaľ Je Na Ňom Elegantná보안 캠페인 - 스스로 지킨 당신의 Pc! 더욱 안전해집니다!كل الحقوق محفوظة ليديعوت إنترنتäöüäöüß", Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.ProperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#10", "電子メールをはじめ、予定表やアドレス帳などの個人情報、さらには OFFICE V. X の各アプリケーションで作成した書類やその他のファイルをリンクさせて統合的に管理することができるコミュニケーション／個人情報管理ソフトウェアです。インターフェースが一新され、高度で多彩な ENTOURAGE X の機能がより直感的に利用できるようになりました。以前お使いになっていた電子メール アプリケーションからの情報のインポートも容易にできます。БУДЕТЕ АВТОМАТИЧЕСКИ ПЕРЕНАПРАВЛЕНЫČNÍCI NÁROČNÝCH POUŽÍVATEĽOVNOVÉ PORTFÓLIO MICROSOFT HARDWARE. ŠPIČKOVÉ OPTICKÉ BEZDRÔTOVÉ MYŠKY S NOVOU TECHNOLÓGIOU - NAKLÁŇACÍM KOLIESKOM. NEZÁLEŽÍ NA TOM, AKÝ STÔL MÁTE, POKIAĽ JE NA ŇOM ELEGANTNÁ보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!كل الحقوق محفوظة ليديعوت إنترنتÄÖÜÄÖÜß", Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.UpperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));

			//TODO: implementation for Chinese characters
			//NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#08", 1, Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.SimplifiedChinese, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			//NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#09", 1, Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.TraditionalChinese, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));

			//ProperCase word delimiters
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#13", "Lksdjfda Älkjdf  Ükjlhj" + Strings.Chr(0) + "Ldkfjd", Strings.StrConv("lksdjfda älkjdf  ükjlhj" + Strings.Chr(0) + "ldkfjd", VbStrConv.ProperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#14", "Lksdjfda Älkjdf  Ükjlhj" + Strings.Chr(9) + "Ldkfjd", Strings.StrConv("lksdjfda älkjdf  ükjlhj" + Strings.Chr(9) + "ldkfjd", VbStrConv.ProperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#15", "Lksdjfda Älkjdf  Ükjlhj" + Strings.Chr(10) + "Ldkfjd", Strings.StrConv("lksdjfda älkjdf  ükjlhj" + Strings.Chr(10) + "ldkfjd", VbStrConv.ProperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#16", "Lksdjfda Älkjdf  Ükjlhj" + Strings.Chr(11) + "Ldkfjd", Strings.StrConv("lksdjfda älkjdf  ükjlhj" + Strings.Chr(11) + "ldkfjd", VbStrConv.ProperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#17", "Lksdjfda Älkjdf  Ükjlhj" + Strings.Chr(12) + "Ldkfjd", Strings.StrConv("lksdjfda älkjdf  ükjlhj" + Strings.Chr(12) + "ldkfjd", VbStrConv.ProperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#18", "Lksdjfda Älkjdf  Ükjlhj" + Strings.Chr(13) + "Ldkfjd", Strings.StrConv("lksdjfda älkjdf  ükjlhj" + Strings.Chr(13) + "ldkfjd", VbStrConv.ProperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#19", "Lksdjfda Älkjdf  Ükjlhj" + Strings.Chr(32) + "Ldkfjd", Strings.StrConv("lksdjfda älkjdf  ükjlhj" + Strings.Chr(32) + "ldkfjd", VbStrConv.ProperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));

			// ***********
			// ** DE-DE **
			// ***********

			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
			
			//requires japanese systems
			//NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#22", 1, Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.Hiragana, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			//NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#23", 1, Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.Katakana, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));

			//TODO: how to write VbStrConv.LinguisticCasing + VbStrConv.UpperCase correctly?
			//NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#24", 1, Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.LinguisticCasing, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			
			//not supported
			//NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#25", 1, Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.Narrow, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			//NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#31", 1, Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.Wide, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#26", TextStringOfMultipleLanguages, Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.None, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#21", "電子メールをはじめ、予定表やアドレス帳などの個人情報、さらには office v. x の各アプリケーションで作成した書類やその他のファイルをリンクさせて統合的に管理することができるコミュニケーション／個人情報管理ソフトウェアです。インターフェースが一新され、高度で多彩な entourage x の機能がより直感的に利用できるようになりました。以前お使いになっていた電子メール アプリケーションからの情報のインポートも容易にできます。будете автоматически перенаправленыčníci náročných používateľovnové portfólio microsoft hardware. špičkové optické bezdrôtové myšky s novou technológiou - nakláňacím kolieskom. nezáleží na tom, aký stôl máte, pokiaľ je na ňom elegantná보안 캠페인 - 스스로 지킨 당신의 pc! 더욱 안전해집니다!كل الحقوق محفوظة ليديعوت إنترنتäöüäöüß", Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.LowerCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#27", "電子メールをはじめ、予定表やアドレス帳などの個人情報、さらには Office V. X の各アプリケーションで作成した書類やその他のファイルをリンクさせて統合的に管理することができるコミュニケーション／個人情報管理ソフトウェアです。インターフェースが一新され、高度で多彩な Entourage X の機能がより直感的に利用できるようになりました。以前お使いになっていた電子メール アプリケーションからの情報のインポートも容易にできます。Будете Автоматически Перенаправленыčníci Náročných Používateľovnové Portfólio Microsoft Hardware. Špičkové Optické Bezdrôtové Myšky S Novou Technológiou - Nakláňacím Kolieskom. Nezáleží Na Tom, Aký Stôl Máte, Pokiaľ Je Na Ňom Elegantná보안 캠페인 - 스스로 지킨 당신의 Pc! 더욱 안전해집니다!كل الحقوق محفوظة ليديعوت إنترنتäöüäöüß", Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.ProperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#30", "電子メールをはじめ、予定表やアドレス帳などの個人情報、さらには OFFICE V. X の各アプリケーションで作成した書類やその他のファイルをリンクさせて統合的に管理することができるコミュニケーション／個人情報管理ソフトウェアです。インターフェースが一新され、高度で多彩な ENTOURAGE X の機能がより直感的に利用できるようになりました。以前お使いになっていた電子メール アプリケーションからの情報のインポートも容易にできます。БУДЕТЕ АВТОМАТИЧЕСКИ ПЕРЕНАПРАВЛЕНЫČNÍCI NÁROČNÝCH POUŽÍVATEĽOVNOVÉ PORTFÓLIO MICROSOFT HARDWARE. ŠPIČKOVÉ OPTICKÉ BEZDRÔTOVÉ MYŠKY S NOVOU TECHNOLÓGIOU - NAKLÁŇACÍM KOLIESKOM. NEZÁLEŽÍ NA TOM, AKÝ STÔL MÁTE, POKIAĽ JE NA ŇOM ELEGANTNÁ보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!كل الحقوق محفوظة ليديعوت إنترنتÄÖÜÄÖÜß", Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.UpperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));

			//TODO: implementation for Chinese characters
			//NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#28", 1, Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.SimplifiedChinese, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			//NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#29", 1, Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.TraditionalChinese, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			
			//ProperCase word delimiters
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#33", "Lksdjfda Älkjdf  Ükjlhj" + Strings.Chr(0) + "Ldkfjd", Strings.StrConv("lksdjfda älkjdf  ükjlhj" + Strings.Chr(0) + "ldkfjd", VbStrConv.ProperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#34", "Lksdjfda Älkjdf  Ükjlhj" + Strings.Chr(9) + "Ldkfjd", Strings.StrConv("lksdjfda älkjdf  ükjlhj" + Strings.Chr(9) + "ldkfjd", VbStrConv.ProperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#35", "Lksdjfda Älkjdf  Ükjlhj" + Strings.Chr(10) + "Ldkfjd", Strings.StrConv("lksdjfda älkjdf  ükjlhj" + Strings.Chr(10) + "ldkfjd", VbStrConv.ProperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#36", "Lksdjfda Älkjdf  Ükjlhj" + Strings.Chr(11) + "Ldkfjd", Strings.StrConv("lksdjfda älkjdf  ükjlhj" + Strings.Chr(11) + "ldkfjd", VbStrConv.ProperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#37", "Lksdjfda Älkjdf  Ükjlhj" + Strings.Chr(12) + "Ldkfjd", Strings.StrConv("lksdjfda älkjdf  ükjlhj" + Strings.Chr(12) + "ldkfjd", VbStrConv.ProperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#38", "Lksdjfda Älkjdf  Ükjlhj" + Strings.Chr(13) + "Ldkfjd", Strings.StrConv("lksdjfda älkjdf  ükjlhj" + Strings.Chr(13) + "ldkfjd", VbStrConv.ProperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#39", "Lksdjfda Älkjdf  Ükjlhj" + Strings.Chr(32) + "Ldkfjd", Strings.StrConv("lksdjfda älkjdf  ükjlhj" + Strings.Chr(32) + "ldkfjd", VbStrConv.ProperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));

			// ***********
			// ** JA-JP **
			// ***********

			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("ja-JP");
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#42", "電子めーるをはじめ、予定表やあどれす帳などの個人情報、さらには Office v. X の各あぷりけーしょんで作成した書類やその他のふぁいるをりんくさせて統合的に管理することができるこみゅにけーしょん／個人情報管理そふとうぇあです。いんたーふぇーすが一新され、高度で多彩な Entourage X の機能がより直感的に利用できるようになりました。以前お使いになっていた電子めーる あぷりけーしょんからの情報のいんぽーとも容易にできます。будете автоматически перенаправлены?nici naro?nych pou?ivate?ovNove portfolio Microsoft Hardware. ?pi?kove opticke bezdrotove my?ky s novou technologiou - nakla?acim kolieskom. Nezale?i na tom, aky stol mate, pokia? je na ?om elegantna?? ??? - ??? ?? ??? PC! ?? ??????!?? ?????? ?????? ??????? ??????aouAOUs", Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.Hiragana, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#43", "電子メールヲハジメ、予定表ヤアドレス帳ナドノ個人情報、サラニハ Office v. X ノ各アプリケーションデ作成シタ書類ヤソノ他ノファイルヲリンクサセテ統合的ニ管理スルコトガデキルコミュニケーション／個人情報管理ソフトウェアデス。インターフェースガ一新サレ、高度デ多彩ナ Entourage X ノ機能ガヨリ直感的ニ利用デキルヨウニナリマシタ。以前オ使イニナッテイタ電子メール アプリケーションカラノ情報ノインポートモ容易ニデキマス。будете автоматически перенаправлены?nici naro?nych pou?ivate?ovNove portfolio Microsoft Hardware. ?pi?kove opticke bezdrotove my?ky s novou technologiou - nakla?acim kolieskom. Nezale?i na tom, aky stol mate, pokia? je na ?om elegantna?? ??? - ??? ?? ??? PC! ?? ??????!?? ?????? ?????? ??????? ??????aouAOUs", Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.Katakana, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));

			//TODO: how to write VbStrConv.LinguisticCasing + VbStrConv.UpperCase correctly?
			//NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#44", 1, Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.LinguisticCasing, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			
			//not supported
			//NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#45", 1, Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.Narrow, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			//NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#51", 1, Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.Wide, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));

			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#46", TextStringOfMultipleLanguages, Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.None, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#41", "電子メールをはじめ、予定表やアドレス帳などの個人情報、さらには office v. x の各アプリケーションで作成した書類やその他のファイルをリンクさせて統合的に管理することができるコミュニケーション／個人情報管理ソフトウェアです。インターフェースが一新され、高度で多彩な entourage x の機能がより直感的に利用できるようになりました。以前お使いになっていた電子メール アプリケーションからの情報のインポートも容易にできます。будете автоматически перенаправленыčníci náročných používateľovnové portfólio microsoft hardware. špičkové optické bezdrôtové myšky s novou technológiou - nakláňacím kolieskom. nezáleží na tom, aký stôl máte, pokiaľ je na ňom elegantná보안 캠페인 - 스스로 지킨 당신의 pc! 더욱 안전해집니다!كل الحقوق محفوظة ليديعوت إنترنتäöüäöüß", Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.LowerCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#47", "電子メールをはじめ、予定表やアドレス帳などの個人情報、さらには Office V. X の各アプリケーションで作成した書類やその他のファイルをリンクさせて統合的に管理することができるコミュニケーション／個人情報管理ソフトウェアです。インターフェースが一新され、高度で多彩な Entourage X の機能がより直感的に利用できるようになりました。以前お使いになっていた電子メール アプリケーションからの情報のインポートも容易にできます。Будете Автоматически Перенаправлены?Nici Naro?Nych Pou?Ivate?Ovnove Portfolio Microsoft Hardware. ?Pi?Kove Opticke Bezdrotove My?Ky S Novou Technologiou - Nakla?Acim Kolieskom. Nezale?I Na Tom, Aky Stol Mate, Pokia? Je Na ?Om Elegantna?? ??? - ??? ?? ??? Pc! ?? ??????!?? ?????? ?????? ??????? ??????Aouaous", Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.ProperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#50", "電子メールをはじめ、予定表やアドレス帳などの個人情報、さらには OFFICE V. X の各アプリケーションで作成した書類やその他のファイルをリンクさせて統合的に管理することができるコミュニケーション／個人情報管理ソフトウェアです。インターフェースが一新され、高度で多彩な ENTOURAGE X の機能がより直感的に利用できるようになりました。以前お使いになっていた電子メール アプリケーションからの情報のインポートも容易にできます。БУДЕТЕ АВТОМАТИЧЕСКИ ПЕРЕНАПРАВЛЕНЫČNÍCI NÁROČNÝCH POUŽÍVATEĽOVNOVÉ PORTFÓLIO MICROSOFT HARDWARE. ŠPIČKOVÉ OPTICKÉ BEZDRÔTOVÉ MYŠKY S NOVOU TECHNOLÓGIOU - NAKLÁŇACÍM KOLIESKOM. NEZÁLEŽÍ NA TOM, AKÝ STÔL MÁTE, POKIAĽ JE NA ŇOM ELEGANTNÁ보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!كل الحقوق محفوظة ليديعوت إنترنتÄÖÜÄÖÜß", Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.UpperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));

			//TODO: implementation for Chinese characters
			//NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#48", 1, Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.SimplifiedChinese, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			//NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#49", 1, Strings.StrConv(TextStringOfMultipleLanguages, VbStrConv.TraditionalChinese, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));

			//ProperCase word delimiters
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#53", "Lksdjfda Alkjdf  Ukjlhj" + Strings.Chr(0) + "Ldkfjd", Strings.StrConv("lksdjfda älkjdf  ükjlhj" + Strings.Chr(0) + "ldkfjd", VbStrConv.ProperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#54", "Lksdjfda Alkjdf  Ukjlhj" + Strings.Chr(9) + "Ldkfjd", Strings.StrConv("lksdjfda älkjdf  ükjlhj" + Strings.Chr(9) + "ldkfjd", VbStrConv.ProperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#55", "Lksdjfda Alkjdf  Ukjlhj" + Strings.Chr(10) + "Ldkfjd", Strings.StrConv("lksdjfda älkjdf  ükjlhj" + Strings.Chr(10) + "ldkfjd", VbStrConv.ProperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#56", "Lksdjfda Alkjdf  Ukjlhj" + Strings.Chr(11) + "Ldkfjd", Strings.StrConv("lksdjfda älkjdf  ükjlhj" + Strings.Chr(11) + "ldkfjd", VbStrConv.ProperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#57", "Lksdjfda Alkjdf  Ukjlhj" + Strings.Chr(12) + "Ldkfjd", Strings.StrConv("lksdjfda älkjdf  ükjlhj" + Strings.Chr(12) + "ldkfjd", VbStrConv.ProperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#58", "Lksdjfda Alkjdf  Ukjlhj" + Strings.Chr(13) + "Ldkfjd", Strings.StrConv("lksdjfda älkjdf  ükjlhj" + Strings.Chr(13) + "ldkfjd", VbStrConv.ProperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrConv#59", "Lksdjfda Alkjdf  Ukjlhj" + Strings.Chr(32) + "Ldkfjd", Strings.StrConv("lksdjfda älkjdf  ükjlhj" + Strings.Chr(32) + "ldkfjd", VbStrConv.ProperCase, System.Threading.Thread.CurrentThread.CurrentCulture.LCID));

			// ***********
			// ** Chinese **
			// ***********

			//TODO: implementation for Chinese locales
			//System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("ja-JP");
			// .......

			// restore buffered culture
			System.Threading.Thread.CurrentThread.CurrentCulture = CurCulture;
			
		}

		[Test]
		public void StrDup()
		{
			string aString = "Wow! What a string!";
			object aObject = new object();
			aObject = "This is a String contained within an Object";
			NUnit.Framework.Assertion.AssertEquals ("JW*StrDup#01", "PPPPP", Strings.StrDup(5, (char)Strings.Asc("P")));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrDup#01", "WWWWWWWWWW", Strings.StrDup(10, aString));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrDup#01", "TTTTTT", Strings.StrDup(6, aObject));
		}

		[Test]
		public void StrReverse()
		{
			NUnit.Framework.Assertion.AssertEquals ("JW*StrReverse#01", "", Strings.StrReverse(String.Empty));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrReverse#02", "ßÜÖÄüöä", Strings.StrReverse(GermanUmlauts_German));
			NUnit.Framework.Assertion.AssertEquals ("JW*StrReverse#03", "", Strings.StrReverse(null));
		}

		public void UCase_Char()
		{
			// buffer current culture
			System.Globalization.CultureInfo CurCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

			// do testings
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/10c", 0, Strings.UCase(Letter_Empty));

			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/10", Strings.AscW("C"), Strings.UCase(Letter_English));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/11", Strings.AscW("電"), Strings.UCase(Letter_Japanese));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/12", 1041, Strings.UCase(Letter_Russian));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/13", 268, Strings.UCase(Letter_Slovakian));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/14", Strings.AscW("보"), Strings.UCase(Letter_Korean));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/15", 1603, Strings.UCase(Letter_Arabic));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/16", Strings.AscW("Ä"), Strings.UCase(Letter_German));

			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/20", Strings.AscW("C"), Strings.UCase(Letter_English));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/21", Strings.AscW("電"), Strings.UCase(Letter_Japanese));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/22", 1041, Strings.UCase(Letter_Russian));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/23", 268, Strings.UCase(Letter_Slovakian));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/24", Strings.AscW("보"), Strings.UCase(Letter_Korean));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/25", 1603, Strings.UCase(Letter_Arabic));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/26", Strings.AscW("Ä"), Strings.UCase(Letter_German));

			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("ja-JP");
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/30", Strings.AscW("C"), Strings.UCase(Letter_English));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/31", Strings.AscW("電"), Strings.UCase(Letter_Japanese));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/32", 1041, Strings.UCase(Letter_Russian));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/33", 268, Strings.UCase(Letter_Slovakian));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/34", Strings.AscW("보"), Strings.UCase(Letter_Korean));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/35", 1603, Strings.UCase(Letter_Arabic));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/36", Strings.AscW("Ä"), Strings.UCase(Letter_German));

			// restore buffered culture
			System.Threading.Thread.CurrentThread.CurrentCulture = CurCulture;
		}

		[Test]
		public void UCase()
		{
			UCase_Char();
			UCase_String();
		}

		public void UCase_String()
		{
			// buffer current culture
			System.Globalization.CultureInfo CurCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

			// do testings
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/64", String.Empty, Strings.UCase(TextStringUninitialized));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/65", "", Strings.UCase(TextStringUninitialized));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/67", String.Empty, Strings.UCase(TextStringEmpty));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/68", "", Strings.UCase(TextStringEmpty));

			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/70", "CHOOSE THE LOCATION FOR WHICH YOU WANT CONTACT INFORMATION:", Strings.UCase(MSWebSiteContent_English));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/71", "電子メールをはじめ、予定表やアドレス帳などの個人情報、さらには OFFICE V. X の各アプリケーションで作成した書類やその他のファイルをリンクさせて統合的に管理することができるコミュニケーション／個人情報管理ソフトウェアです。インターフェースが一新され、高度で多彩な ENTOURAGE X の機能がより直感的に利用できるようになりました。以前お使いになっていた電子メール アプリケーションからの情報のインポートも容易にできます。", Strings.UCase(MSWebSiteContent_Japanese));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/72", "БУДЕТЕ АВТОМАТИЧЕСКИ ПЕРЕНАПРАВЛЕНЫ", Strings.UCase(MSWebSiteContent_Russian));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/73", "ČNÍCI NÁROČNÝCH POUŽÍVATEĽOVNOVÉ PORTFÓLIO MICROSOFT HARDWARE. ŠPIČKOVÉ OPTICKÉ BEZDRÔTOVÉ MYŠKY S NOVOU TECHNOLÓGIOU - NAKLÁŇACÍM KOLIESKOM. NEZÁLEŽÍ NA TOM, AKÝ STÔL MÁTE, POKIAĽ JE NA ŇOM ELEGANTNÁ", Strings.UCase(MSWebSiteContent_Slovakian));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/74", "보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!", Strings.UCase(MSWebSiteContent_Korean));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/75", "كل الحقوق محفوظة ليديعوت إنترنت", Strings.UCase(ArabynetComWebSiteContent_Arabic));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/76", "ÄÖÜÄÖÜß", Strings.UCase(GermanUmlauts_German));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/77", "電子メールをはじめ、予定表やアドレス帳などの個人情報、さらには OFFICE V. X の各アプリケーションで作成した書類やその他のファイルをリンクさせて統合的に管理することができるコミュニケーション／個人情報管理ソフトウェアです。インターフェースが一新され、高度で多彩な ENTOURAGE X の機能がより直感的に利用できるようになりました。以前お使いになっていた電子メール アプリケーションからの情報のインポートも容易にできます。БУДЕТЕ АВТОМАТИЧЕСКИ ПЕРЕНАПРАВЛЕНЫČNÍCI NÁROČNÝCH POUŽÍVATEĽOVNOVÉ PORTFÓLIO MICROSOFT HARDWARE. ŠPIČKOVÉ OPTICKÉ BEZDRÔTOVÉ MYŠKY S NOVOU TECHNOLÓGIOU - NAKLÁŇACÍM KOLIESKOM. NEZÁLEŽÍ NA TOM, AKÝ STÔL MÁTE, POKIAĽ JE NA ŇOM ELEGANTNÁ보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!كل الحقوق محفوظة ليديعوت إنترنتÄÖÜÄÖÜß", Strings.UCase(TextStringOfMultipleLanguages));

			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/80", "CHOOSE THE LOCATION FOR WHICH YOU WANT CONTACT INFORMATION:", Strings.UCase(MSWebSiteContent_English));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/81", "電子メールをはじめ、予定表やアドレス帳などの個人情報、さらには OFFICE V. X の各アプリケーションで作成した書類やその他のファイルをリンクさせて統合的に管理することができるコミュニケーション／個人情報管理ソフトウェアです。インターフェースが一新され、高度で多彩な ENTOURAGE X の機能がより直感的に利用できるようになりました。以前お使いになっていた電子メール アプリケーションからの情報のインポートも容易にできます。", Strings.UCase(MSWebSiteContent_Japanese));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/82", "БУДЕТЕ АВТОМАТИЧЕСКИ ПЕРЕНАПРАВЛЕНЫ", Strings.UCase(MSWebSiteContent_Russian));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/83", "ČNÍCI NÁROČNÝCH POUŽÍVATEĽOVNOVÉ PORTFÓLIO MICROSOFT HARDWARE. ŠPIČKOVÉ OPTICKÉ BEZDRÔTOVÉ MYŠKY S NOVOU TECHNOLÓGIOU - NAKLÁŇACÍM KOLIESKOM. NEZÁLEŽÍ NA TOM, AKÝ STÔL MÁTE, POKIAĽ JE NA ŇOM ELEGANTNÁ", Strings.UCase(MSWebSiteContent_Slovakian));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/84", "보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!", Strings.UCase(MSWebSiteContent_Korean));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/85", "كل الحقوق محفوظة ليديعوت إنترنت", Strings.UCase(ArabynetComWebSiteContent_Arabic));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/86", "ÄÖÜÄÖÜß", Strings.UCase(GermanUmlauts_German));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/87", "電子メールをはじめ、予定表やアドレス帳などの個人情報、さらには OFFICE V. X の各アプリケーションで作成した書類やその他のファイルをリンクさせて統合的に管理することができるコミュニケーション／個人情報管理ソフトウェアです。インターフェースが一新され、高度で多彩な ENTOURAGE X の機能がより直感的に利用できるようになりました。以前お使いになっていた電子メール アプリケーションからの情報のインポートも容易にできます。БУДЕТЕ АВТОМАТИЧЕСКИ ПЕРЕНАПРАВЛЕНЫČNÍCI NÁROČNÝCH POUŽÍVATEĽOVNOVÉ PORTFÓLIO MICROSOFT HARDWARE. ŠPIČKOVÉ OPTICKÉ BEZDRÔTOVÉ MYŠKY S NOVOU TECHNOLÓGIOU - NAKLÁŇACÍM KOLIESKOM. NEZÁLEŽÍ NA TOM, AKÝ STÔL MÁTE, POKIAĽ JE NA ŇOM ELEGANTNÁ보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!كل الحقوق محفوظة ليديعوت إنترنتÄÖÜÄÖÜß", Strings.UCase(TextStringOfMultipleLanguages));

			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("ja-JP");
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/90", "CHOOSE THE LOCATION FOR WHICH YOU WANT CONTACT INFORMATION:", Strings.UCase(MSWebSiteContent_English));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/91", "電子メールをはじめ、予定表やアドレス帳などの個人情報、さらには OFFICE V. X の各アプリケーションで作成した書類やその他のファイルをリンクさせて統合的に管理することができるコミュニケーション／個人情報管理ソフトウェアです。インターフェースが一新され、高度で多彩な ENTOURAGE X の機能がより直感的に利用できるようになりました。以前お使いになっていた電子メール アプリケーションからの情報のインポートも容易にできます。", Strings.UCase(MSWebSiteContent_Japanese));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/92", "БУДЕТЕ АВТОМАТИЧЕСКИ ПЕРЕНАПРАВЛЕНЫ", Strings.UCase(MSWebSiteContent_Russian));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/93", "ČNÍCI NÁROČNÝCH POUŽÍVATEĽOVNOVÉ PORTFÓLIO MICROSOFT HARDWARE. ŠPIČKOVÉ OPTICKÉ BEZDRÔTOVÉ MYŠKY S NOVOU TECHNOLÓGIOU - NAKLÁŇACÍM KOLIESKOM. NEZÁLEŽÍ NA TOM, AKÝ STÔL MÁTE, POKIAĽ JE NA ŇOM ELEGANTNÁ", Strings.UCase(MSWebSiteContent_Slovakian));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/94", "보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!", Strings.UCase(MSWebSiteContent_Korean));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/95", "كل الحقوق محفوظة ليديعوت إنترنت", Strings.UCase(ArabynetComWebSiteContent_Arabic));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/96", "ÄÖÜÄÖÜß", Strings.UCase(GermanUmlauts_German));
			NUnit.Framework.Assertion.AssertEquals ("JW#UCase/97", "電子メールをはじめ、予定表やアドレス帳などの個人情報、さらには OFFICE V. X の各アプリケーションで作成した書類やその他のファイルをリンクさせて統合的に管理することができるコミュニケーション／個人情報管理ソフトウェアです。インターフェースが一新され、高度で多彩な ENTOURAGE X の機能がより直感的に利用できるようになりました。以前お使いになっていた電子メール アプリケーションからの情報のインポートも容易にできます。БУДЕТЕ АВТОМАТИЧЕСКИ ПЕРЕНАПРАВЛЕНЫČNÍCI NÁROČNÝCH POUŽÍVATEĽOVNOVÉ PORTFÓLIO MICROSOFT HARDWARE. ŠPIČKOVÉ OPTICKÉ BEZDRÔTOVÉ MYŠKY S NOVOU TECHNOLÓGIOU - NAKLÁŇACÍM KOLIESKOM. NEZÁLEŽÍ NA TOM, AKÝ STÔL MÁTE, POKIAĽ JE NA ŇOM ELEGANTNÁ보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!كل الحقوق محفوظة ليديعوت إنترنتÄÖÜÄÖÜß", Strings.UCase(TextStringOfMultipleLanguages));

			// restore buffered culture
			System.Threading.Thread.CurrentThread.CurrentCulture = CurCulture;
		}
	}
}
