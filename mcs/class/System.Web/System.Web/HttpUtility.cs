// 
// System.Web.HttpUtility
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   Wictor Wilén (decode/encode functions) (wictor@ibizkit.se)
//
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace System.Web {
   public sealed class HttpUtility {
      // private stuff
      private const string _hex = "0123456789ABCDEF";
      private const string _chars = "<>;:.?=&@*+%/\\";

      public HttpUtility() {
      }

      public static void HtmlAttributeEncode(string s, TextWriter output) {
         output.Write(HtmlAttributeEncode(s));
      }

      public static string HtmlAttributeEncode(string s) {
         if (null == s) {
            return s;
         }

         StringBuilder oStr = new StringBuilder(128);

         for (int i = 0; i != s.Length; i++) {
            if (s[i] == '&') {
               oStr.Append("&amp;");
            } else 
            if (s[i] == '"') {
               oStr.Append("&quot;");
            } else {
               oStr.Append(s[i]);
            }
         }

         return oStr.ToString();
      }

      public static string UrlDecode(string str) {
         return UrlDecode(str, Encoding.UTF8);
      }

      [MonoTODO("Use Encoding")]
      public static string UrlDecode(string s, Encoding Enc) {
         if (null == s) {
            return null;
         }

         StringBuilder dest = new StringBuilder ();
         long len = s.Length;
         string tmp = "";

         for (int i = 0; i < len; i++) {
            if (s [i] == '%' && i + 2 < len) {
               tmp = s [i+1].ToString ();
               tmp += s [i+2];
               dest.Append ((char) Int32.Parse (tmp, NumberStyles.HexNumber));
	       i += 2;
            } 
            else if (s [i] == '+')
               dest.Append (' ');
            else
               dest.Append (s [i]);
         }

         return dest.ToString ();
      }



      public static string UrlEncode(string str) {
         return UrlEncode(str, Encoding.UTF8);
      }

      [MonoTODO("Use encoding")]
      public static string UrlEncode(string s, Encoding Enc) {
         if (null == s) {
            return null;
         }

         StringBuilder dest = new StringBuilder ();
         long len = s.Length;
         int h1, h2;

         for(int i = 0; i < len; i++) {
            if(s[i] == ' ') // space character is replaced with '+'
               dest.Append ('+');
            else if ( _chars.IndexOf (s [i]) >= 0 ) {
               h1 = (int)s[i] % 16;
               h2 = (int)s[i] / 16;
               dest.Append ('%');
               dest.Append (_hex [h1].ToString ());
               dest.Append (_hex [h2].ToString ());
            }
            else
               dest.Append (s [i]);

         }

         return dest.ToString ();
      }
   
      /// <summary>
      /// Decodes an HTML-encoded string and returns the decoded string.
      /// </summary>
      /// <param name="s">The HTML string to decode. </param>
      /// <returns>The decoded text.</returns>
      [MonoTODO()]
      public static string HtmlDecode(string s) {
         throw new System.NotImplementedException();
      }

      /// <summary>
      /// Decodes an HTML-encoded string and sends the resulting output to a TextWriter output stream.
      /// </summary>
      /// <param name="s">The HTML string to decode</param>
      /// <param name="output">The TextWriter output stream containing the decoded string. </param>
      [MonoTODO()]
      public static void HtmlDecode(string s, TextWriter output) {
         throw new System.NotImplementedException();
      }

      /// <summary>
      /// HTML-encodes a string and returns the encoded string.
      /// </summary>
      /// <param name="s">The text string to encode. </param>
      /// <returns>The HTML-encoded text.</returns>
      public static string HtmlEncode(string s) {
         string dest = "";
         long len = s.Length;
         int v;



         for(int i = 0; i < len; i++) {
            switch(s[i]) {
               case '>':
                  dest += "&gt;";
                  break;
               case '<':
                  dest += "&lt;";
                  break;
               case '"':
                  dest += "&quot;";
                  break;
               case '&':
                  dest += "&amp;";
                  break;
               default:
                  if(s[i] >= 128) {
                     dest += "&H";
                     v = (int) s[i];
                     dest += v.ToString() ;
							
                  }
                  else
                     dest += s[i];
                  break;
            }
         }
         return dest;
      }

      /// <summary>
      /// HTML-encodes a string and sends the resulting output to a TextWriter output stream.
      /// </summary>
      /// <param name="s">The string to encode. </param>
      /// <param name="output">The TextWriter output stream containing the encoded string. </param>
      public static void HtmlEncode(	string s, TextWriter output) {
         output.Write(HtmlEncode(s));
      }
   }
}
