/* 
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
namespace Lucene.Net.Demo.Html
{
	
	public class Entities
	{
		internal static readonly System.Collections.Hashtable decoder = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable(300));
		internal static readonly System.String[] encoder = new System.String[0x100];
		
		internal static System.String Decode(System.String entity)
		{
			if (entity[entity.Length - 1] == ';')
			// remove trailing semicolon
				entity = entity.Substring(0, (entity.Length - 1) - (0));
			if (entity[1] == '#')
			{
				int start = 2;
				int radix = 10;
				if (entity[2] == 'X' || entity[2] == 'x')
				{
					start++;
					radix = 16;
				}
				System.Char c = (char) System.Convert.ToInt32(entity.Substring(start), radix);
				return c.ToString();
			}
			else
			{
				System.String s = (System.String) decoder[entity];
				if (s != null)
					return s;
				else
					return "";
			}
		}
		
		public static System.String Encode(System.String s)
		{
			int length = s.Length;
			System.Text.StringBuilder buffer = new System.Text.StringBuilder(length * 2);
			for (int i = 0; i < length; i++)
			{
				char c = s[i];
				int j = (int) c;
				if (j < 0x100 && encoder[j] != null)
				{
					buffer.Append(encoder[j]); // have a named encoding
					buffer.Append(';');
				}
				else if (j < 0x80)
				{
					buffer.Append(c); // use ASCII value
				}
				else
				{
					buffer.Append("&#"); // use numeric encoding
					buffer.Append((int) c);
					buffer.Append(';');
				}
			}
			return buffer.ToString();
		}
		
		internal static void  Add(System.String entity, int value_Renamed)
		{
			decoder[entity] = ((char) value_Renamed).ToString();
			if (value_Renamed < 0x100)
				encoder[value_Renamed] = entity;
		}
		static Entities()
		{
			{
				Add("&nbsp", 160);
				Add("&iexcl", 161);
				Add("&cent", 162);
				Add("&pound", 163);
				Add("&curren", 164);
				Add("&yen", 165);
				Add("&brvbar", 166);
				Add("&sect", 167);
				Add("&uml", 168);
				Add("&copy", 169);
				Add("&ordf", 170);
				Add("&laquo", 171);
				Add("&not", 172);
				Add("&shy", 173);
				Add("&reg", 174);
				Add("&macr", 175);
				Add("&deg", 176);
				Add("&plusmn", 177);
				Add("&sup2", 178);
				Add("&sup3", 179);
				Add("&acute", 180);
				Add("&micro", 181);
				Add("&para", 182);
				Add("&middot", 183);
				Add("&cedil", 184);
				Add("&sup1", 185);
				Add("&ordm", 186);
				Add("&raquo", 187);
				Add("&frac14", 188);
				Add("&frac12", 189);
				Add("&frac34", 190);
				Add("&iquest", 191);
				Add("&Agrave", 192);
				Add("&Aacute", 193);
				Add("&Acirc", 194);
				Add("&Atilde", 195);
				Add("&Auml", 196);
				Add("&Aring", 197);
				Add("&AElig", 198);
				Add("&Ccedil", 199);
				Add("&Egrave", 200);
				Add("&Eacute", 201);
				Add("&Ecirc", 202);
				Add("&Euml", 203);
				Add("&Igrave", 204);
				Add("&Iacute", 205);
				Add("&Icirc", 206);
				Add("&Iuml", 207);
				Add("&ETH", 208);
				Add("&Ntilde", 209);
				Add("&Ograve", 210);
				Add("&Oacute", 211);
				Add("&Ocirc", 212);
				Add("&Otilde", 213);
				Add("&Ouml", 214);
				Add("&times", 215);
				Add("&Oslash", 216);
				Add("&Ugrave", 217);
				Add("&Uacute", 218);
				Add("&Ucirc", 219);
				Add("&Uuml", 220);
				Add("&Yacute", 221);
				Add("&THORN", 222);
				Add("&szlig", 223);
				Add("&agrave", 224);
				Add("&aacute", 225);
				Add("&acirc", 226);
				Add("&atilde", 227);
				Add("&auml", 228);
				Add("&aring", 229);
				Add("&aelig", 230);
				Add("&ccedil", 231);
				Add("&egrave", 232);
				Add("&eacute", 233);
				Add("&ecirc", 234);
				Add("&euml", 235);
				Add("&igrave", 236);
				Add("&iacute", 237);
				Add("&icirc", 238);
				Add("&iuml", 239);
				Add("&eth", 240);
				Add("&ntilde", 241);
				Add("&ograve", 242);
				Add("&oacute", 243);
				Add("&ocirc", 244);
				Add("&otilde", 245);
				Add("&ouml", 246);
				Add("&divide", 247);
				Add("&oslash", 248);
				Add("&ugrave", 249);
				Add("&uacute", 250);
				Add("&ucirc", 251);
				Add("&uuml", 252);
				Add("&yacute", 253);
				Add("&thorn", 254);
				Add("&yuml", 255);
				Add("&fnof", 402);
				Add("&Alpha", 913);
				Add("&Beta", 914);
				Add("&Gamma", 915);
				Add("&Delta", 916);
				Add("&Epsilon", 917);
				Add("&Zeta", 918);
				Add("&Eta", 919);
				Add("&Theta", 920);
				Add("&Iota", 921);
				Add("&Kappa", 922);
				Add("&Lambda", 923);
				Add("&Mu", 924);
				Add("&Nu", 925);
				Add("&Xi", 926);
				Add("&Omicron", 927);
				Add("&Pi", 928);
				Add("&Rho", 929);
				Add("&Sigma", 931);
				Add("&Tau", 932);
				Add("&Upsilon", 933);
				Add("&Phi", 934);
				Add("&Chi", 935);
				Add("&Psi", 936);
				Add("&Omega", 937);
				Add("&alpha", 945);
				Add("&beta", 946);
				Add("&gamma", 947);
				Add("&delta", 948);
				Add("&epsilon", 949);
				Add("&zeta", 950);
				Add("&eta", 951);
				Add("&theta", 952);
				Add("&iota", 953);
				Add("&kappa", 954);
				Add("&lambda", 955);
				Add("&mu", 956);
				Add("&nu", 957);
				Add("&xi", 958);
				Add("&omicron", 959);
				Add("&pi", 960);
				Add("&rho", 961);
				Add("&sigmaf", 962);
				Add("&sigma", 963);
				Add("&tau", 964);
				Add("&upsilon", 965);
				Add("&phi", 966);
				Add("&chi", 967);
				Add("&psi", 968);
				Add("&omega", 969);
				Add("&thetasym", 977);
				Add("&upsih", 978);
				Add("&piv", 982);
				Add("&bull", 8226);
				Add("&hellip", 8230);
				Add("&prime", 8242);
				Add("&Prime", 8243);
				Add("&oline", 8254);
				Add("&frasl", 8260);
				Add("&weierp", 8472);
				Add("&image", 8465);
				Add("&real", 8476);
				Add("&trade", 8482);
				Add("&alefsym", 8501);
				Add("&larr", 8592);
				Add("&uarr", 8593);
				Add("&rarr", 8594);
				Add("&darr", 8595);
				Add("&harr", 8596);
				Add("&crarr", 8629);
				Add("&lArr", 8656);
				Add("&uArr", 8657);
				Add("&rArr", 8658);
				Add("&dArr", 8659);
				Add("&hArr", 8660);
				Add("&forall", 8704);
				Add("&part", 8706);
				Add("&exist", 8707);
				Add("&empty", 8709);
				Add("&nabla", 8711);
				Add("&isin", 8712);
				Add("&notin", 8713);
				Add("&ni", 8715);
				Add("&prod", 8719);
				Add("&sum", 8721);
				Add("&minus", 8722);
				Add("&lowast", 8727);
				Add("&radic", 8730);
				Add("&prop", 8733);
				Add("&infin", 8734);
				Add("&ang", 8736);
				Add("&and", 8743);
				Add("&or", 8744);
				Add("&cap", 8745);
				Add("&cup", 8746);
				Add("&int", 8747);
				Add("&there4", 8756);
				Add("&sim", 8764);
				Add("&cong", 8773);
				Add("&asymp", 8776);
				Add("&ne", 8800);
				Add("&equiv", 8801);
				Add("&le", 8804);
				Add("&ge", 8805);
				Add("&sub", 8834);
				Add("&sup", 8835);
				Add("&nsub", 8836);
				Add("&sube", 8838);
				Add("&supe", 8839);
				Add("&oplus", 8853);
				Add("&otimes", 8855);
				Add("&perp", 8869);
				Add("&sdot", 8901);
				Add("&lceil", 8968);
				Add("&rceil", 8969);
				Add("&lfloor", 8970);
				Add("&rfloor", 8971);
				Add("&lang", 9001);
				Add("&rang", 9002);
				Add("&loz", 9674);
				Add("&spades", 9824);
				Add("&clubs", 9827);
				Add("&hearts", 9829);
				Add("&diams", 9830);
				Add("&quot", 34);
				Add("&amp", 38);
				Add("&lt", 60);
				Add("&gt", 62);
				Add("&OElig", 338);
				Add("&oelig", 339);
				Add("&Scaron", 352);
				Add("&scaron", 353);
				Add("&Yuml", 376);
				Add("&circ", 710);
				Add("&tilde", 732);
				Add("&ensp", 8194);
				Add("&emsp", 8195);
				Add("&thinsp", 8201);
				Add("&zwnj", 8204);
				Add("&zwj", 8205);
				Add("&lrm", 8206);
				Add("&rlm", 8207);
				Add("&ndash", 8211);
				Add("&mdash", 8212);
				Add("&lsquo", 8216);
				Add("&rsquo", 8217);
				Add("&sbquo", 8218);
				Add("&ldquo", 8220);
				Add("&rdquo", 8221);
				Add("&bdquo", 8222);
				Add("&dagger", 8224);
				Add("&Dagger", 8225);
				Add("&permil", 8240);
				Add("&lsaquo", 8249);
				Add("&rsaquo", 8250);
				Add("&euro", 8364);
			}
		}
	}
}