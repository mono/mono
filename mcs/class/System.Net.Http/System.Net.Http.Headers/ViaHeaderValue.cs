//
// ViaHeaderValue.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Xamarin Inc (http://www.xamarin.com)
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

namespace System.Net.Http.Headers
{
	public class ViaHeaderValue : ICloneable
	{
		public ViaHeaderValue (string protocolVersion, string receivedBy)
		{
			Parser.Token.Check (protocolVersion);
			Parser.Uri.Check (receivedBy);

			ProtocolVersion = protocolVersion;
			ReceivedBy = receivedBy;
		}

		public ViaHeaderValue (string protocolVersion, string receivedBy, string protocolName)
			: this (protocolVersion, receivedBy)
		{
			if (!string.IsNullOrEmpty (protocolName)) {
				Parser.Token.Check (protocolName);
				ProtocolName = protocolName;
			}
		}

		public ViaHeaderValue (string protocolVersion, string receivedBy, string protocolName, string comment)
			: this (protocolVersion, receivedBy, protocolName)
		{
			if (!string.IsNullOrEmpty (comment)) {
				Parser.Token.CheckComment (comment);
				Comment = comment;
			}
		}

		private ViaHeaderValue ()
		{
		}

		public string Comment { get; private set; }
		public string ProtocolName { get; private set; }
		public string ProtocolVersion { get; private set; }
		public string ReceivedBy { get; private set; }

		object ICloneable.Clone ()
		{
			return MemberwiseClone ();
		}

		public override bool Equals (object obj)
		{
			var source = obj as ViaHeaderValue;
			if (source == null)
				return false;

			return string.Equals (source.Comment, Comment, StringComparison.Ordinal) &&
				string.Equals (source.ProtocolName, ProtocolName, StringComparison.OrdinalIgnoreCase) &&
				string.Equals (source.ProtocolVersion, ProtocolVersion, StringComparison.OrdinalIgnoreCase) &&
				string.Equals (source.ReceivedBy, ReceivedBy, StringComparison.OrdinalIgnoreCase);
		}

		public override int GetHashCode ()
		{
			int hc = ProtocolVersion.ToLowerInvariant ().GetHashCode ();
			hc ^= ReceivedBy.ToLowerInvariant ().GetHashCode ();

			if (!string.IsNullOrEmpty (ProtocolName)) {
				hc ^= ProtocolName.ToLowerInvariant ().GetHashCode ();
			}

			if (!string.IsNullOrEmpty (Comment)) {
				hc ^= Comment.GetHashCode ();
			}

			return hc;
		}

		public static ViaHeaderValue Parse (string input)
		{
			ViaHeaderValue value;
			if (TryParse (input, out value))
				return value;

			throw new FormatException (input);
		}
		
		public static bool TryParse (string input, out ViaHeaderValue parsedValue)
		{
			parsedValue = null;

			var lexer = new Lexer (input);

			var t = lexer.Scan ();
			if (t != Token.Type.Token)
				return false;

			var next = lexer.Scan ();
			ViaHeaderValue value = new ViaHeaderValue ();

			if (next == Token.Type.SeparatorSlash) {
				next = lexer.Scan ();
				if (next != Token.Type.Token)
					return false;

				value.ProtocolName = lexer.GetStringValue (t);
				value.ProtocolVersion = lexer.GetStringValue (next);

				next = lexer.Scan ();
			} else {
				value.ProtocolVersion = lexer.GetStringValue (t);
			}

			if (next != Token.Type.Token)
				return false;

			if (lexer.PeekChar () == ':') {
				lexer.EatChar ();

				t = lexer.Scan ();
				if (t != Token.Type.Token)
					return false;
			} else {
				t = next;
			}

			value.ReceivedBy = lexer.GetStringValue (next, t);

			string comment;
			if (!lexer.ScanCommentOptional (out comment))
				return false;

			value.Comment = comment;
			parsedValue = value;
			return true;
		}

		public override string ToString ()
		{
			string s = ProtocolName != null ?
				ProtocolName + "/" + ProtocolVersion + " " + ReceivedBy :
				ProtocolVersion + " " + ReceivedBy;

			return Comment != null ? s + " " + Comment : s;
		}
	}
}
