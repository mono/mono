//
// JsonDeserializer.cs
//
// Author:
//   Marek Habersack <grendel@twistedcode.net>
//
// (C) 2008 Novell, Inc.  http://novell.com/
// Copyright 2011, Xamarin, Inc (http://xamarin.com)
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
// Code is based on JSON_checker (http://www.json.org/JSON_checker/) and JSON_parser
// (http://fara.cs.uni-potsdam.de/~jsg/json_parser/) C sources.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace System.Web.Script.Serialization
{
	internal sealed class JsonDeserializer
	{
		/* Universal error constant */
		const int __ = -1;
		const int UNIVERSAL_ERROR = __;
		
		/*
		  Characters are mapped into these 31 character classes. This allows for
		  a significant reduction in the size of the state transition table.
		*/
		const int C_SPACE = 0x00;  /* space */
		const int C_WHITE = 0x01;  /* other whitespace */
		const int C_LCURB = 0x02;  /* {  */
		const int C_RCURB = 0x03;  /* } */
		const int C_LSQRB = 0x04;  /* [ */
		const int C_RSQRB = 0x05;  /* ] */
		const int C_COLON = 0x06;  /* : */
		const int C_COMMA = 0x07;  /* , */
		const int C_QUOTE = 0x08;  /* " */
		const int C_BACKS = 0x09;  /* \ */
		const int C_SLASH = 0x0A;  /* / */
		const int C_PLUS  = 0x0B;  /* + */
		const int C_MINUS = 0x0C;  /* - */
		const int C_POINT = 0x0D;  /* . */
		const int C_ZERO  = 0x0E;  /* 0 */
		const int C_DIGIT = 0x0F;  /* 123456789 */
		const int C_LOW_A = 0x10;  /* a */
		const int C_LOW_B = 0x11;  /* b */
		const int C_LOW_C = 0x12;  /* c */
		const int C_LOW_D = 0x13;  /* d */
		const int C_LOW_E = 0x14;  /* e */
		const int C_LOW_F = 0x15;  /* f */
		const int C_LOW_L = 0x16;  /* l */
		const int C_LOW_N = 0x17;  /* n */
		const int C_LOW_R = 0x18;  /* r */
		const int C_LOW_S = 0x19;  /* s */
		const int C_LOW_T = 0x1A;  /* t */
		const int C_LOW_U = 0x1B;  /* u */
		const int C_ABCDF = 0x1C;  /* ABCDF */
		const int C_E     = 0x1D;  /* E */
		const int C_ETC   = 0x1E;  /* everything else */
		const int C_STAR  = 0x1F;  /* * */
		const int C_I     = 0x20;  /* I */
		const int C_LOW_I = 0x21;  /* i */
		const int C_LOW_Y = 0x22;  /* y */
		const int C_N     = 0x23;  /* N */
		
		/* The state codes. */
		const int GO = 0x00;  /* start    */
		const int OK = 0x01;  /* ok       */
		const int OB = 0x02;  /* object   */
		const int KE = 0x03;  /* key      */
		const int CO = 0x04;  /* colon    */
		const int VA = 0x05;  /* value    */
		const int AR = 0x06;  /* array    */
		const int ST = 0x07;  /* string   */
		const int ES = 0x08;  /* escape   */
		const int U1 = 0x09;  /* u1       */
		const int U2 = 0x0A;  /* u2       */
		const int U3 = 0x0B;  /* u3       */
		const int U4 = 0x0C;  /* u4       */
		const int MI = 0x0D;  /* minus    */
		const int ZE = 0x0E;  /* zero     */
		const int IN = 0x0F;  /* integer  */
		const int FR = 0x10;  /* fraction */
		const int E1 = 0x11;  /* e        */
		const int E2 = 0x12;  /* ex       */
		const int E3 = 0x13;  /* exp      */
		const int T1 = 0x14;  /* tr       */
		const int T2 = 0x15;  /* tru      */
		const int T3 = 0x16;  /* true     */
		const int F1 = 0x17;  /* fa       */
		const int F2 = 0x18;  /* fal      */
		const int F3 = 0x19;  /* fals     */
		const int F4 = 0x1A;  /* false    */
		const int N1 = 0x1B;  /* nu       */
		const int N2 = 0x1C;  /* nul      */
		const int N3 = 0x1D;  /* null     */
		const int FX = 0x1E;  /* *.* *eE* */
		const int IV = 0x1F;  /* invalid input */
		const int UK = 0x20;  /* unquoted key name */
		const int UI = 0x21; /* ignore during unquoted key name construction */
		const int I1 = 0x22;  /* In */
		const int I2 = 0x23;  /* Inf */
		const int I3 = 0x24;  /* Infi */
		const int I4 = 0x25;  /* Infin */
		const int I5 = 0x26;  /* Infini */
		const int I6 = 0x27;  /* Infinit */
		const int I7 = 0x28;  /* Infinity */
		const int V1 = 0x29;  /* Na */
		const int V2 = 0x2A;  /* NaN */
		
		/* Actions */
		const int FA = -10; /* false */
		const int TR = -11; /* false */
		const int NU = -12; /* null */
		const int DE = -13; /* double detected by exponent e E */
		const int DF = -14; /* double detected by fraction . */
		const int SB = -15; /* string begin */
		const int MX = -16; /* integer detected by minus */
		const int ZX = -17; /* integer detected by zero */
		const int IX = -18; /* integer detected by 1-9 */
		const int EX = -19; /* next char is escaped */
		const int UC = -20; /* Unicode character read */
		const int SE =  -4; /* string end */
		const int AB =  -5; /* array begin */
		const int AE =  -7; /* array end */
		const int OS =  -6; /* object start */
		const int OE =  -8; /* object end */
		const int EO =  -9; /* empty object */
		const int CM =  -3; /* comma */
		const int CA =  -2; /* colon action */
		const int PX = -21; /* integer detected by plus */
		const int KB = -22; /* unquoted key name begin */
		const int UE = -23; /* unquoted key name end */
		const int IF = -25; /* Infinity */
		const int NN = -26; /* NaN */
		
		
		enum JsonMode {
			NONE,
			ARRAY,
			DONE,
			KEY,
			OBJECT
		};

		enum JsonType {
			NONE = 0,
			ARRAY_BEGIN,
			ARRAY_END,
			OBJECT_BEGIN,
			OBJECT_END,
			INTEGER,
			FLOAT,
			NULL,
			TRUE,
			FALSE,
			STRING,
			KEY,
			MAX
		};
		
		/*
		  This array maps the 128 ASCII characters into character classes.
		  The remaining Unicode characters should be mapped to C_ETC.
		  Non-whitespace control characters are errors.
		*/
		static readonly int[] ascii_class = {
			__,      __,      __,      __,      __,      __,      __,      __,
			__,      C_WHITE, C_WHITE, __,      __,      C_WHITE, __,      __,
			__,      __,      __,      __,      __,      __,      __,      __,
			__,      __,      __,      __,      __,      __,      __,      __,

			C_SPACE, C_ETC,   C_QUOTE, C_ETC,   C_ETC,   C_ETC,   C_ETC,   C_QUOTE,
			C_ETC,   C_ETC,   C_STAR,   C_PLUS,  C_COMMA, C_MINUS, C_POINT, C_SLASH,
			C_ZERO,  C_DIGIT, C_DIGIT, C_DIGIT, C_DIGIT, C_DIGIT, C_DIGIT, C_DIGIT,
			C_DIGIT, C_DIGIT, C_COLON, C_ETC,   C_ETC,   C_ETC,   C_ETC,   C_ETC,

			C_ETC,   C_ABCDF, C_ABCDF, C_ABCDF, C_ABCDF, C_E,     C_ABCDF, C_ETC,
			C_ETC,   C_I,   C_ETC,   C_ETC,   C_ETC,   C_ETC,   C_N,   C_ETC,
			C_ETC,   C_ETC,   C_ETC,   C_ETC,   C_ETC,   C_ETC,   C_ETC,   C_ETC,
			C_ETC,   C_ETC,   C_ETC,   C_LSQRB, C_BACKS, C_RSQRB, C_ETC,   C_ETC,

			C_ETC,   C_LOW_A, C_LOW_B, C_LOW_C, C_LOW_D, C_LOW_E, C_LOW_F, C_ETC,
			C_ETC,   C_LOW_I, C_ETC,   C_ETC,   C_LOW_L, C_ETC,   C_LOW_N, C_ETC,
			C_ETC,   C_ETC,   C_LOW_R, C_LOW_S, C_LOW_T, C_LOW_U, C_ETC,   C_ETC,
			C_ETC,   C_LOW_Y,   C_ETC,   C_LCURB, C_ETC,   C_RCURB, C_ETC,   C_ETC
		};

		static readonly int[,] state_transition_table = {
		/*
		  The state transition table takes the current state and the current symbol,
		  and returns either a new state or an action. An action is represented as a
		  negative number. A JSON text is accepted if at the end of the text the
		  state is OK and if the mode is MODE_DONE.

		                           white                  '                   1-9                                   ABCDF  etc
		                       space |  {  }  [  ]  :  ,  "  \  /  +  -  .  0  |  a  b  c  d  e  f  l  n  r  s  t  u  |  E  |  *  I  i  y  N */
			/*start    GO*/ {GO,GO,OS,__,AB,__,__,__,SB,__,__,PX,MX,__,ZX,IX,__,__,__,__,__,FA,__,__,__,__,TR,__,__,__,__,__,I1,__,__,V1},
			/*ok       OK*/ {OK,OK,__,OE,__,AE,__,CM,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__},
			/*object   OB*/ {OB,OB,__,EO,__,__,__,__,SB,__,__,__,KB,__,KB,KB,KB,KB,KB,KB,KB,KB,KB,KB,KB,KB,KB,KB,KB,KB,KB,__,KB,KB,KB,KB},
			/*key      KE*/ {KE,KE,__,__,__,__,__,__,SB,__,__,__,KB,__,KB,KB,KB,KB,KB,KB,KB,KB,KB,KB,KB,KB,KB,KB,KB,KB,KB,__,KB,KB,KB,KB},
			/*colon    CO*/ {CO,CO,__,__,__,__,CA,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__},
			/*value    VA*/ {VA,VA,OS,__,AB,__,__,__,SB,__,__,PX,MX,__,ZX,IX,__,__,__,__,__,FA,__,NU,__,__,TR,__,__,__,__,__,I1,__,__,V1},
			/*array    AR*/ {AR,AR,OS,__,AB,AE,__,__,SB,__,__,PX,MX,__,ZX,IX,__,__,__,__,__,FA,__,NU,__,__,TR,__,__,__,__,__,I1,__,__,V1},
			/*string   ST*/ {ST,__,ST,ST,ST,ST,ST,ST,SE,EX,ST,ST,ST,ST,ST,ST,ST,ST,ST,ST,ST,ST,ST,ST,ST,ST,ST,ST,ST,ST,ST,ST,ST,ST,ST,ST},
			/*escape   ES*/ {__,__,__,__,__,__,__,__,ST,ST,ST,__,__,__,__,__,__,ST,__,__,__,ST,__,ST,ST,__,ST,U1,__,__,__,__,__,__,__,__},
			/*u1       U1*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,U2,U2,U2,U2,U2,U2,U2,U2,__,__,__,__,__,__,U2,U2,__,__,__,__,__,__},
			/*u2       U2*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,U3,U3,U3,U3,U3,U3,U3,U3,__,__,__,__,__,__,U3,U3,__,__,__,__,__,__},
			/*u3       U3*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,U4,U4,U4,U4,U4,U4,U4,U4,__,__,__,__,__,__,U4,U4,__,__,__,__,__,__},
			/*u4       U4*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,UC,UC,UC,UC,UC,UC,UC,UC,__,__,__,__,__,__,UC,UC,__,__,__,__,__,__},
			/*minus    MI*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,ZE,IN,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,I1,__,__,__},
			/*zero     ZE*/ {OK,OK,__,OE,__,AE,__,CM,__,__,__,__,__,DF,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__},
			/*int      IN*/ {OK,OK,__,OE,__,AE,__,CM,__,__,__,__,__,DF,IN,IN,__,__,__,__,DE,__,__,__,__,__,__,__,__,DE,__,__,__,__,__,__},
			/*frac     FR*/ {OK,OK,__,OE,__,AE,__,CM,__,__,__,__,__,__,FR,FR,__,__,__,__,E1,__,__,__,__,__,__,__,__,E1,__,__,__,__,__,__},
			/*e        E1*/ {__,__,__,__,__,__,__,__,__,__,__,E2,E2,__,E3,E3,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__},
			/*ex       E2*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,E3,E3,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__},
			/*exp      E3*/ {OK,OK,__,OE,__,AE,__,CM,__,__,__,__,__,__,E3,E3,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__},
			/*tr       T1*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,T2,__,__,__,__,__,__,__,__,__,__,__},
			/*tru      T2*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,T3,__,__,__,__,__,__,__,__},
			/*true     T3*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,OK,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__},
			/*fa       F1*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,F2,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__},
			/*fal      F2*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,F3,__,__,__,__,__,__,__,__,__,__,__,__,__},
			/*fals     F3*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,F4,__,__,__,__,__,__,__,__,__,__},
			/*false    F4*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,OK,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__},
			/*nu       N1*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,N2,__,__,__,__,__,__,__,__},
			/*nul      N2*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,N3,__,__,__,__,__,__,__,__,__,__,__,__,__},
			/*null     N3*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,OK,__,__,__,__,__,__,__,__,__,__,__,__,__},
			/*_.       FX*/ {OK,OK,__,OE,__,AE,__,CM,__,__,__,__,__,__,FR,FR,__,__,__,__,E1,__,__,__,__,__,__,__,__,E1,__,__,__,__,__,__},
			/*inval.   IV*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__},
			/*unq.key  UK*/ {UI,UI,__,__,__,__,UE,__,__,__,__,UK,UK,UK,UK,UK,UK,UK,UK,UK,UK,UK,UK,UK,UK,UK,UK,UK,UK,UK,UK,__,UK,UK,UK,UK},
			/*unq.ign. UI*/ {UI,UI,__,__,__,__,UE,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__},
			/*i1       I1*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,I2,__,__,__,__,__,__,__,__,__,__,__,__},
			/*i2       I2*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,I3,__,__,__,__,__,__,__,__,__,__,__,__,__,__},
			/*i3       I3*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,I4,__,__},
			/*i4       I4*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,I5,__,__,__,__,__,__,__,__,__,__,__,__},
			/*i5       I5*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,I6,__,__},
			/*i6       I6*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,I7,__,__,__,__,__,__,__,__,__},
			/*i7       I7*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,IF,__},
			/*v1       V1*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,V2,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__},
			/*v2       V2*/ {__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,__,NN},
		};

		JavaScriptSerializer serializer;
		JavaScriptTypeResolver typeResolver;
		int maxJsonLength;
		int currentPosition;
		int recursionLimit;
		int recursionDepth;
		
		Stack <JsonMode> modes;
		Stack <object> returnValue;
		JsonType jsonType;
		
		bool escaped;
		int state;
		Stack <string> currentKey;
		StringBuilder buffer;
		char quoteChar;
		
		public JsonDeserializer (JavaScriptSerializer serializer)
		{
			this.serializer = serializer;
			this.maxJsonLength = serializer.MaxJsonLength;
			this.recursionLimit = serializer.RecursionLimit;
			this.typeResolver = serializer.TypeResolver;
			this.modes = new Stack <JsonMode> ();
			this.currentKey = new Stack <string> ();
			this.returnValue = new Stack <object> ();
			this.state = GO;
			this.currentPosition = 0;
			this.recursionDepth = 0;
		}

		public object Deserialize (string input)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			return Deserialize (new StringReader (input));
		}
		
		public object Deserialize (TextReader input)
		{
			if (input == null)
				throw new ArgumentNullException ("input");
			
			int value;
			buffer = new StringBuilder ();
			
			while (true) {
				value = input.Read ();
				if (value < 0)
					break;

				currentPosition++;
				if (currentPosition > maxJsonLength)
					throw new ArgumentException ("Maximum JSON input length has been exceeded.");

				if (!ProcessCharacter ((char) value))
					throw new InvalidOperationException ("JSON syntax error.");
			}

			object topObject = PeekObject ();
			if (buffer.Length > 0) {
				object result;

				if (ParseBuffer (out result)) {
					if (topObject != null)
						StoreValue (result);
					else
						PushObject (result);
				}
			}

			if (returnValue.Count > 1)
				throw new InvalidOperationException ("JSON syntax error.");

			object ret = PopObject ();
			return ret;
		}

#if DEBUG
		void DumpObject (string indent, object obj)
		{
			if (obj is Dictionary <string, object>) {
				Console.WriteLine (indent + "{");
				foreach (KeyValuePair <string, object> kvp in (Dictionary <string, object>)obj) {
					Console.WriteLine (indent + "\t\"{0}\": ", kvp.Key);
					DumpObject (indent + "\t\t", kvp.Value);
				}
				Console.WriteLine (indent + "}");
			} else if (obj is object[]) {
				Console.WriteLine (indent + "[");
				foreach (object o in (object[])obj)
					DumpObject (indent + "\t", o);
				Console.WriteLine (indent + "]");
			} else if (obj != null)
				Console.WriteLine (indent + obj.ToString ());
			else
				Console.WriteLine ("null");
		}
#endif
		
		void DecodeUnicodeChar ()
		{
			int len = buffer.Length;
			if (len < 6)
				throw new ArgumentException ("Invalid escaped unicode character specification (" + currentPosition + ")");

			int code = Int32.Parse (buffer.ToString ().Substring (len - 4), NumberStyles.HexNumber);
			buffer.Length = len - 6;
			buffer.Append ((char)code);
		}

		string GetModeMessage (JsonMode expectedMode)
		{
			switch (expectedMode) {
				case JsonMode.ARRAY:
					return "Invalid array passed in, ',' or ']' expected (" + currentPosition + ")";

				case JsonMode.KEY:
					return "Invalid object passed in, key name or ':' expected (" + currentPosition + ")";

				case JsonMode.OBJECT:
					return "Invalid object passed in, key value expected (" + currentPosition + ")";
					
				default:
					return "Invalid JSON string";
			}
		}
		
		void PopMode (JsonMode expectedMode)
		{
			JsonMode mode = PeekMode ();
			if (mode != expectedMode)
				throw new ArgumentException (GetModeMessage (mode));

			modes.Pop ();
		}

		void PushMode (JsonMode newMode)
		{
			modes.Push (newMode);
		}

		JsonMode PeekMode ()
		{
			if (modes.Count == 0)
				return JsonMode.NONE;

			return modes.Peek ();
		}

		void PushObject (object o)
		{
			returnValue.Push (o);
		}

		object PopObject (bool notIfLast)
		{
			int count = returnValue.Count;
			if (count == 0)
				return null;

			if (notIfLast && count == 1)
				return null;
			
			return returnValue.Pop ();
		}

		object PopObject ()
		{
			return PopObject (false);
		}
		
		object PeekObject ()
		{
			if (returnValue.Count == 0)
				return null;

			return returnValue.Peek ();
		}
		
		void RemoveLastCharFromBuffer ()
		{
			int len = buffer.Length;
			if (len == 0)
				return;
			buffer.Length = len - 1;
		}

		bool ParseBuffer (out object result)
		{
			result = null;
			
			if (jsonType == JsonType.NONE) {
				buffer.Length = 0;
				return false;
			}

			string s = buffer.ToString ();
			bool converted = true;
			int intValue;
			long longValue;
			decimal decimalValue;
			double doubleValue;
			
			switch (jsonType) {
				case JsonType.INTEGER:
					/* MS AJAX.NET JSON parser promotes big integers to double */
					
					if (Int32.TryParse (s, out intValue))
						result = intValue;
					else if (Int64.TryParse (s, out longValue))
						result = longValue;
					else if (Decimal.TryParse (s, out decimalValue))
						result = decimalValue;
					else if (Double.TryParse (s, out doubleValue))
						result = doubleValue;
					else
						converted = false;
					break;

				case JsonType.FLOAT:
					if (Decimal.TryParse (s, out decimalValue))
						result = decimalValue;
					else if (Double.TryParse (s, out doubleValue))
						result = doubleValue;
					else
						converted = false;
					break;

				case JsonType.TRUE:
					if (String.Compare (s, "true", StringComparison.Ordinal) == 0)
						result = true;
					else
						converted = false;
					break;

				case JsonType.FALSE:
					if (String.Compare (s, "false", StringComparison.Ordinal) == 0)
						result = false;
					else
						converted = false;
					break;

				case JsonType.NULL:
					if (String.Compare (s, "null", StringComparison.Ordinal) != 0)
						converted = false;
					break;

				case JsonType.STRING:
					if (s.StartsWith ("/Date(", StringComparison.Ordinal) && s.EndsWith (")/", StringComparison.Ordinal)) {
						long javaScriptTicks = Convert.ToInt64 (s.Substring (6, s.Length - 8));
						result = new DateTime ((javaScriptTicks * 10000) + JsonSerializer.InitialJavaScriptDateTicks, DateTimeKind.Utc);
					} else
						result = s;
					break;

				default:
					throw new InvalidOperationException (String.Format ("Internal error: unexpected JsonType ({0})", jsonType));
					
			}

			if (!converted)
				throw new ArgumentException ("Invalid JSON primitive: " + s);

			buffer.Length = 0;
			return true;
		}
		
		bool ProcessCharacter (char ch)
		{
			int next_class, next_state;

			if (ch >= 128)
				next_class = C_ETC;
			else {
				next_class = ascii_class [ch];
				if (next_class <= UNIVERSAL_ERROR)
					return false;
			}

			if (escaped) {
				escaped = false;
				RemoveLastCharFromBuffer ();
				
				switch (ch) {
					case 'b':
						buffer.Append ('\b');
						break;
					case 'f':
						buffer.Append ('\f');
						break;
					case 'n':
						buffer.Append ('\n');
						break;
					case 'r':
						buffer.Append ('\r');
						break;
					case 't':
						buffer.Append ('\t');
						break;
					case '"':
					case '\\':
					case '/':
						buffer.Append (ch);
						break;
					case 'u':
						buffer.Append ("\\u");
						break;
					default:
						return false;
				}
			} else if (jsonType != JsonType.NONE || !(next_class == C_SPACE || next_class == C_WHITE))
				buffer.Append (ch);

			next_state = state_transition_table [state, next_class];
			if (next_state >= 0) {
				state = next_state;
				return true;
			}

			object result;
			/* An action to perform */
			switch (next_state) {
				case UC: /* Unicode character */
					DecodeUnicodeChar ();
					state = ST;
					break;
					
				case EX: /* Escaped character */
					escaped = true;
					state = ES;
					break;

				case MX: /* integer detected by minus */
					jsonType = JsonType.INTEGER;
					state = MI;
					break;

				case PX: /* integer detected by plus */
					jsonType = JsonType.INTEGER;
					state = MI;
					break;
					
				case ZX: /* integer detected by zero */
					jsonType = JsonType.INTEGER;
					state = ZE;
					break;

				case IX: /* integer detected by 1-9 */
					jsonType = JsonType.INTEGER;
					state = IN;
					break;

				case DE: /* floating point number detected by exponent*/
					jsonType = JsonType.FLOAT;
					state = E1;
					break;

				case DF: /* floating point number detected by fraction */
					jsonType = JsonType.FLOAT;
					state = FX;
					break;

				case SB: /* string begin " or ' */
					buffer.Length = 0;
					quoteChar = ch;
					jsonType = JsonType.STRING;
					state = ST;
					break;

				case KB: /* unquoted key name begin */
					jsonType = JsonType.STRING;
					state = UK;
					break;

				case UE: /* unquoted key name end ':' */
					RemoveLastCharFromBuffer ();
					if (ParseBuffer (out result))
						StoreKey (result);
					jsonType = JsonType.NONE;
					
					PopMode (JsonMode.KEY);
					PushMode (JsonMode.OBJECT);
					state = VA;
					buffer.Length = 0;
					break;
					
				case NU: /* n */
					jsonType = JsonType.NULL;
					state = N1;
					break;

				case FA: /* f */
					jsonType = JsonType.FALSE;
					state = F1;
					break;

				case TR: /* t */
					jsonType = JsonType.TRUE;
					state = T1;
					break;

				case EO: /* empty } */
					result = PopObject (true);
					if (result != null)
						StoreValue (result);
					PopMode (JsonMode.KEY);
					state = OK;
					break;

				case OE: /* } */
					RemoveLastCharFromBuffer ();

					if (ParseBuffer (out result))
						StoreValue (result);

					result = PopObject (true);
					if (result != null)
						StoreValue (result);

					PopMode (JsonMode.OBJECT);

					jsonType = JsonType.NONE;
					state = OK;
					break;

				case AE: /* ] */
					RemoveLastCharFromBuffer ();
					if (ParseBuffer (out result))
						StoreValue (result);
					PopMode (JsonMode.ARRAY);
					result = PopObject (true);
					if (result != null)
						StoreValue (result);
					
					jsonType = JsonType.NONE;
					state = OK;
					break;

				case OS: /* { */
					RemoveLastCharFromBuffer ();
					CreateObject ();
					PushMode (JsonMode.KEY);
					
					state = OB;
					break;

				case AB: /* [ */
					RemoveLastCharFromBuffer ();
					CreateArray ();
					PushMode (JsonMode.ARRAY);

					state = AR;
					break;

				case SE: /* string end " or ' */
					if (ch == quoteChar) {
						RemoveLastCharFromBuffer ();

						switch (PeekMode ()) {
							case JsonMode.KEY:
								if (ParseBuffer (out result))
									StoreKey (result);

								jsonType = JsonType.NONE;
								state = CO;
								buffer.Length = 0;
								break;

							case JsonMode.ARRAY:
							case JsonMode.OBJECT:
								if (ParseBuffer (out result))
									StoreValue (result);
							
								jsonType = JsonType.NONE;
								state = OK;
								break;

							case JsonMode.NONE: /* A stand-alone string */
								jsonType = JsonType.STRING;
								state = IV; /* the rest of input is invalid */
								if (ParseBuffer (out result))
									PushObject (result);
								break;
							
							default:
								throw new ArgumentException ("Syntax error: string in unexpected place.");
						}
					}
					break;

				case CM: /* , */
					RemoveLastCharFromBuffer ();

					// With MS.AJAX, a comma resets the recursion depth
					recursionDepth = 0;

					bool doStore = ParseBuffer (out result);
					switch (PeekMode ()) {
						case JsonMode.OBJECT:
							if (doStore)
								StoreValue (result);
							PopMode (JsonMode.OBJECT);
							PushMode (JsonMode.KEY);
							jsonType = JsonType.NONE;
							state = KE;
							break;

						case JsonMode.ARRAY:
							jsonType = JsonType.NONE;
							state = VA;
							if (doStore)
								StoreValue (result);
							break;

						default:
							throw new ArgumentException ("Syntax error: unexpected comma.");
					}
					
					break;

				case CA: /* : */
					RemoveLastCharFromBuffer ();

					// With MS.AJAX a colon increases recursion depth
					if (++recursionDepth >= recursionLimit)
						throw new ArgumentException ("Recursion limit has been reached on parsing input.");
					
					PopMode (JsonMode.KEY);
					PushMode (JsonMode.OBJECT);
					state = VA;
					break;

				case IF: /* Infinity */
				case NN: /* NaN */
					jsonType = JsonType.FLOAT;
					switch (PeekMode ()) {
						case JsonMode.ARRAY:
						case JsonMode.OBJECT:
 							if (ParseBuffer (out result))
 								StoreValue (result);
							
							jsonType = JsonType.NONE;
							state = OK;
							break;

						case JsonMode.NONE: /* A stand-alone NaN/Infinity */
							jsonType = JsonType.FLOAT;
							state = IV; /* the rest of input is invalid */
 							if (ParseBuffer (out result))
 								PushObject (result);
							break;
							
						default:
							throw new ArgumentException ("Syntax error: misplaced NaN/Infinity.");
					}
					buffer.Length = 0;
					break;
					
				default:
					throw new ArgumentException (GetModeMessage (PeekMode ()));
			}
			
			return true;
		}

		void CreateArray ()
		{
			var arr = new ArrayList ();
			PushObject (arr);
		}
		
		void CreateObject ()
		{
			var dict = new Dictionary <string, object> ();
			PushObject (dict);
		}

		void StoreKey (object o)
		{
			string key = o as string;
			if (key != null)
				key = key.Trim ();
			
			if (String.IsNullOrEmpty (key))
				throw new InvalidOperationException ("Internal error: key is null, empty or not a string.");
			
			currentKey.Push (key);
			Dictionary <string, object> dict = PeekObject () as Dictionary <string, object>;
			if (dict == null)
				throw new InvalidOperationException ("Internal error: current object is not a dictionary.");

			/* MS AJAX.NET silently overwrites existing currentKey value */
			dict [key] = null;
		}

		void StoreValue (object o)
		{
			Dictionary <string, object> dict = PeekObject () as Dictionary <string, object>;
			if (dict == null) {
				ArrayList arr = PeekObject () as ArrayList;
				if (arr == null)
					throw new InvalidOperationException ("Internal error: current object is not a dictionary or an array.");
				arr.Add (o);
				return;
			}

			string key;
			if (currentKey.Count == 0)
				key = null;
			else
				key = currentKey.Pop ();

			if (String.IsNullOrEmpty (key))
				throw new InvalidOperationException ("Internal error: object is a dictionary, but no key present.");
			
			dict [key] = o;
		}
	}
}
