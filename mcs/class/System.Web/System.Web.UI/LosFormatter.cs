//
// System.Web.UI.LosFormatter
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web.UI;
using System.Web.Util;

namespace System.Web.UI
{
	public sealed class LosFormatter
	{
		delegate void WriteObject (LosFormatter formatter, TextWriter writer, object value);
		static char [] specialChars = new char [] {'<', '>', ';'};

		const char booleanID = 'o';
		const char stringID = 's';
		const char charID = 'c';
		const char int16ID = 'i';
		const char int32ID = 'I';
		const char int64ID = 'l';
		const char colorID = 'C';
		const char pairID = 'p';
		const char tripletID = 't';
		const char arrayListID = 'L';
		const char hashtableID = 'h';
		const char binaryID = 'b';
		const char arrayID = 'a';
		const char dateTimeID = 'd';
		
		static Hashtable specialTypes;
		static Hashtable idToType;
		
		static LosFormatter ()
		{
			specialTypes = new Hashtable ();
			specialTypes.Add (typeof (Boolean), new WriteObject (WriteBoolean));
			specialTypes.Add (typeof (Pair), new WriteObject (WritePair));
			specialTypes.Add (typeof (Triplet), new WriteObject (WriteTriplet));
			specialTypes.Add (typeof (Color), new WriteObject (WriteColor));
			specialTypes.Add (typeof (ArrayList), new WriteObject (WriteArrayList));
			specialTypes.Add (typeof (Hashtable), new WriteObject (WriteHashtable));
			specialTypes.Add (typeof (Array), new WriteObject (WriteArray));
			specialTypes.Add (typeof (DateTime), new WriteObject (WriteDateTime));

			idToType = new Hashtable ();
			idToType.Add (typeof (string), stringID);
			idToType.Add (typeof (char), charID);
			idToType.Add (typeof (Int16), int16ID);
			idToType.Add (typeof (Int32), int32ID);
			idToType.Add (typeof (Int64), int64ID);
			idToType.Add (typeof (Boolean), booleanID);
			idToType.Add (typeof (Pair), pairID);
			idToType.Add (typeof (Triplet), tripletID);
			idToType.Add (typeof (Color), colorID);
			idToType.Add (typeof (ArrayList), arrayListID);
			idToType.Add (typeof (Hashtable), hashtableID);
			idToType.Add (typeof (Array), arrayID);
		}
		
		public object Deserialize (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			return Deserialize (new StreamReader (stream));
		}

		public object Deserialize (TextReader input)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			return Deserialize (input.ReadToEnd ());
		}

		public object Deserialize (string input)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			string real_input = WebEncoding.Encoding.GetString (Convert.FromBase64String (input));
			return DeserializeObject (real_input);
		}

		private static string UnEscapeSpecialChars (string str)
		{
			if (str.IndexOf ('\\') == -1)
				return str;

			string result = str.Replace ("\\;", ";");
			result = result.Replace ("\\>", ">");
			result = result.Replace ("\\<", "<");
			result = result.Replace ("\\\\", "\\");
			return result;
		}
		
		private static string GetEnclosedString (string input)
		{
			if (input [0] != '<')
				throw new ArgumentException (input);

			int count = 1;
			bool escaped = false;
			StringBuilder result = new StringBuilder ();
			for (int i = 1; count != 0 && i < input.Length; i++) {
				char c = input [i];
				if (escaped)
					escaped = false;
				else if (c == '\\')
					escaped = true;
				else if (c == '<')
					count++;
				else if (c == '>')
					count--;

				result.Append (c);
			}

			result.Length--;
			return result.ToString ();
		}
		
		private static string [] GetStringValues (string input)
		{
			if (input == null || input.Length == 0)
				return new string [0];

			int length = input.Length;
			bool escaped = false;
			int opened = 0;
			ArrayList list = new ArrayList ();
			StringBuilder builder = new StringBuilder ();
			for (int i = 0; i < length; i++) {
				char c = input [i];
				if (escaped)
					escaped = false;
				else if (c == '\\')
					escaped = true;
				else if (c == '<')
					opened++;
				else if (c == '>')
					opened--;
				else if (c == ';' && opened == 0) {
					list.Add (builder.ToString ());
					builder = new StringBuilder ();
					continue;
				}

				builder.Append (c);
			}

			list.Add (builder.ToString ());

			string [] result = new string [list.Count];
			list.CopyTo (result, 0);
			return result;
		}

		private object DeserializeObject (string input)
		{
			if (input == null || input.Length < 2)
				return null;

			object obj;
			string enclosed = GetEnclosedString (input.Substring (1));
			string [] splitted;

			switch (input [0]) {
			case booleanID:
				obj = enclosed.Length == 1;
				break;
			case stringID:
				obj = UnEscapeSpecialChars (enclosed);
				break;
			case int16ID:
				obj = Int16.Parse (enclosed);
				break;
			case int32ID:
				obj = Int32.Parse (enclosed);
				break;
			case int64ID:
				obj = Int64.Parse (enclosed);
				break;
			case colorID:
				obj = Color.FromArgb (Int32.Parse (enclosed));
				break;
			case pairID:
				Pair pair = new Pair ();
				obj = pair;
				splitted = GetStringValues (enclosed);
				if (splitted.Length > 0) {
					pair.First = DeserializeObject (splitted [0]);
					if (splitted.Length > 1)
						pair.Second = DeserializeObject (splitted [1]);
				}
				break;
			case tripletID:
				Triplet triplet = new Triplet ();
				obj = triplet;
				splitted = GetStringValues (enclosed);
				if (splitted.Length == 0)
					break;
				triplet.First = DeserializeObject (splitted [0]);
				if (splitted.Length < 1)
					break;
				triplet.Second = DeserializeObject (splitted [1]);
				if (splitted.Length < 2)
					break;
				triplet.Third = DeserializeObject (splitted [2]);
				break;
			case arrayListID:
			case arrayID:
				ArrayList list = new ArrayList ();
				obj = list;
				splitted = GetStringValues (enclosed);
				foreach (string s in splitted) {
					object o = DeserializeObject (s);
					list.Add (o);
				}

				if (input [0] == arrayID)
					obj = list.ToArray (typeof (object));

				break;
			case hashtableID:
				object key;
				object value;
				Hashtable hash = new Hashtable ();
				obj = hash;
				splitted = GetStringValues (enclosed);
				int length = splitted.Length;
				for (int i = 0; i < length; i++) {
					key = DeserializeObject (splitted [i++]);
					if (i < length)
						value = DeserializeObject (splitted [i]);
					else
						value = null;

					hash.Add (key, value);
				}
				break;
			case binaryID:
				byte [] buffer = Convert.FromBase64String (enclosed);
				MemoryStream ms = new MemoryStream (buffer);
				BinaryFormatter fmt = new BinaryFormatter ();
				obj = fmt.Deserialize (ms);
				break;
			case dateTimeID:
				obj = new DateTime (Int64.Parse (enclosed));
				break;
			default:
				throw new ArgumentException ("input");
			}

			return obj;
		}

		public void Serialize (Stream stream, object value)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			if (value == null)
				throw new ArgumentNullException ("value");

			StreamWriter writer = new StreamWriter (stream);
			Serialize (writer, value);
			writer.Flush ();
		}

		public void Serialize (TextWriter output, object value)
		{
			if (value == null)
				return;

			if (output == null)
				throw new ArgumentNullException ("output");

			StringBuilder builder = new StringBuilder ();
			StringWriter writer = new StringWriter (builder);
			SerializeObject (writer, value);
			byte [] bytes = WebEncoding.Encoding.GetBytes (builder.ToString ());
			output.Write (Convert.ToBase64String (bytes));
		}

		private static void WriteBoolean (LosFormatter formatter, TextWriter output, object value)
		{
			if (value == null)
				return;
			
			output.Write (booleanID);
			bool b = (bool) value;
			output.Write (b ? "<t>" : "<>");
		}
		
		private static void WritePair (LosFormatter formatter, TextWriter output, object value)
		{
			if (value == null)
				return;
			
			output.Write (pairID);
			Pair pair = (Pair) value;
			output.Write ('<');
			formatter.SerializeObject (output, pair.First);
			output.Write (';');
			formatter.SerializeObject (output, pair.Second);
			output.Write ('>');
		}

		private static void WriteTriplet (LosFormatter formatter, TextWriter output, object value)
		{
			if (value == null)
				return;
			
			output.Write (tripletID);
			Triplet triplet = (Triplet) value;
			output.Write ('<');
			formatter.SerializeObject (output, triplet.First);
			output.Write (';');
			formatter.SerializeObject (output, triplet.Second);
			output.Write (';');
			formatter.SerializeObject (output, triplet.Third);
			output.Write ('>');
		}

		private static void WriteColor (LosFormatter formatter, TextWriter output, object value)
		{
			if (value == null)
				return;
			
			Color c = (Color) value;
			output.Write (String.Format ("{0}<{1}>", colorID, c.ToArgb ()));
		}

		private static void WriteArrayList (LosFormatter formatter, TextWriter output, object value)
		{
			if (value == null)
				return;
			
			output.Write (arrayListID);
			output.Write ('<');
			ArrayList list = (ArrayList) value;
			for (int i = 0; i < list.Count; i++) {
				formatter.SerializeObject (output, list [i]);
				if (i != list.Count - 1)
					output.Write (';');
			}
			output.Write('>');
		}

		private static void WriteArray (LosFormatter formatter, TextWriter output, object value)
		{
			if (value == null)
				return;
			
			output.Write (arrayID);
			output.Write ('<');
			Array array = (Array) value;
			for (int i = 0; i < array.Length; i++) {
				formatter.SerializeObject (output, array.GetValue (i));
				if (i != array.Length - 1)
					output.Write (';');
			}
			output.Write('>');
		}

		private static void WriteHashtable (LosFormatter formatter, TextWriter output, object value)
		{
			if (value == null)
				return;
			
			output.Write (hashtableID);
			output.Write ('<');
			Hashtable hash = (Hashtable) value;
			int i = 0;
			foreach (DictionaryEntry entry in hash) {
				formatter.SerializeObject (output, entry.Key);
				output.Write (';');
				formatter.SerializeObject (output, entry.Value);
				if (i != hash.Count - 1)
					output.Write (';');
				i++;
			}
			output.Write('>');
		}

		private static void WriteDateTime (LosFormatter formatter, TextWriter output, object value)
		{
			if (value == null)
				return;
			
			output.Write (dateTimeID);
			output.Write ('<');
			output.Write (((DateTime) value).Ticks);
			output.Write('>');
		}

		private static string EscapeSpecialChars (string str)
		{
			if (str.IndexOfAny (specialChars) == -1)
				return str;

			string result = str.Replace ("\\", "\\\\");
			result = result.Replace ("<", "\\<");
			result = result.Replace (">", "\\>");
			result = result.Replace (";", "\\;");
			return result;
		}
		
		private void SerializeBinary (TextWriter output, object value)
		{
			WebTrace.PushContext ("LosFormatter.SerializeBinary");
			/* This is just for debugging purposes */
			/*if (value is Array) {
				Array array = (Array) value;
				for (int i = 0; i < array.Length; i++) {
					object o = array.GetValue (i);
					if (o == null)
						WebTrace.WriteLine ("\t{0} is null", i);
					else
						WebTrace.WriteLine ("\t{0} {1} {2}", i, o.GetType (), o);
				}
			}
			*/
			
			BinaryFormatter fmt = new BinaryFormatter ();
			MemoryStream stream = new MemoryStream ();

			fmt.Serialize (stream, value);
			output.Write (binaryID);
			output.Write ('<');
			byte [] buffer = stream.GetBuffer ();
			output.Write (Convert.ToBase64String (buffer));
			output.Write ('>');
			
			WebTrace.PopContext ();
		}

		private void SerializeObject (TextWriter output, object value)
		{
			WebTrace.PushContext ("LosFormatter.SerializeObject");
			if (value == null) {
				WebTrace.WriteLine ("value is null");
				WebTrace.PopContext ();
				return;
			}

			Type t = value.GetType ();
			if (t.IsArray)
				t = typeof (Array);

			if (specialTypes.Contains (t)) {
				WriteObject w = (WriteObject) specialTypes [t];
				w (this, output, value);
				WebTrace.WriteLine ("special type: {0}", value.GetType ());
				WebTrace.PopContext ();
				return;
			}

			if (idToType.Contains (t)) {
				char c = (char) idToType [t];
				string s = EscapeSpecialChars (value.ToString ());
				output.Write (String.Format ("{0}<{1}>", c, value.ToString ()));
				WebTrace.WriteLine ("regular type: {0}", value.GetType ());
				WebTrace.PopContext ();
				return;
			}

			SerializeBinary (output, value);
			WebTrace.PopContext ();
		}
	}
}

