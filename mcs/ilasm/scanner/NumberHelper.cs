// NumberHelper.cs
// Author: Sergey Chaban (serge@wildwestsoftware.com)

using System;
using System.Text;
using System.Globalization;

namespace Mono.ILASM {

        /// <summary>
        /// </summary>
        internal class NumberHelper : StringHelperBase {

                private ILToken result;

                /// <summary>
                /// </summary>
                /// <param name="host"></param>
                public NumberHelper (ILTokenizer host) : base (host)
                {
                        Reset ();
                }


                private void Reset ()
                {
                        result = ILToken.Invalid.Clone() as ILToken;
                }

                /// <summary>
                /// </summary>
                /// <returns></returns>
                public override bool Start (char ch)
                {
                        bool res = (Char.IsDigit (ch) || ch == '-' || (ch == '.' && Char.IsDigit ((char) host.Reader.Peek ())));
                        Reset ();
                        return res;
                }

                bool is_hex (int e)
                {
                        return (e >= '0' && e <= '9') || (e >= 'A' && e <= 'F') || (e >= 'a' && e <= 'f');
                }

                bool is_sign (int ch)
                {
                        return ((ch == '+') || (ch == '-'));
                }

                bool is_e (int ch)
                {
                        return ((ch == 'e') || (ch == 'E'));
                }

                /// <summary>
                /// </summary>
                /// <returns></returns>
                public override string Build ()
                {
                        ILReader reader = host.Reader;
                        reader.MarkLocation ();
                        StringBuilder num_builder = new StringBuilder ();
                        string num;
                        int ch;
                        int peek;
                        bool is_real = false;
                        bool dec_found = false;

                        NumberStyles nstyles = NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint |
                                NumberStyles.AllowLeadingSign;

                        ch = reader.Read ();
                        peek = reader.Peek ();
                        reader.Unread (ch);

                        if (ch == '0' && (peek == 'x' || peek == 'X'))
                                return BuildHex ();

                        if (is_sign (reader.Peek ()))
                                num_builder.Append ((char) reader.Read ());

                        do {
                                ch = reader.Read ();
                                peek = reader.Peek ();
                                num_builder.Append ((char) ch);

                                if (is_e (ch)) {
                                        if (is_real)
                                                throw new Exception ("Bad number format, multiples e's found.");

                                        is_real = true;
                                }
                                if (ch == '.')
                                        dec_found = true;
                                if (!is_hex(peek) &&
                                    !(peek == '.' && !dec_found) && !is_e (peek) &&
                                    !(is_sign (peek) && is_real)) {
                                        break;
                                }
                        } while (ch != -1);

                        num = num_builder.ToString ();

                        // Check for hexbytes
                        if (num.Length == 2) {
                                if (Char.IsLetter (num[0]) || Char.IsLetter (num[1])) {
                                        result.token = Token.HEXBYTE;
                                        result.val = Byte.Parse (num, NumberStyles.HexNumber);
                                        return num;
                                }
                        }

                        if (ch == '.' && peek == '.') {
                                num = num.Substring (0, num.Length-1);
                                reader.Unread ('.');
                        } else if (ch == '.') {
                                num += '0';
                        }

                        try {
                                if (num.IndexOf ('.') != -1) {
                                        double d = Double.Parse (num, nstyles, NumberFormatInfo.InvariantInfo);
                                        result.token = Token.FLOAT64;
                                        result.val = d;
                                } else {
                                        long i = Int64.Parse (num, nstyles);
                                        if (i < Int32.MinValue || i > Int32.MaxValue) {
                                                result.token = Token.INT64;
                                                result.val = i;
                                        } else {
                                                result.token = Token.INT32;
                                                result.val = (int) i;
                                        }
                                }
                        } catch {
                                reader.Unread (num.ToCharArray ());
                                reader.RestoreLocation ();
                                num = String.Empty;
                                Reset ();
                                throw new ILSyntaxError ("Bad number format! '" + num_builder + "'");
                        }
                        return num;
                }

                public string BuildHex ()
                {
                        ILReader reader = host.Reader;
                        reader.MarkLocation ();
                        StringBuilder num_builder = new StringBuilder ();
                        NumberStyles nstyles = NumberStyles.HexNumber;

                        string num;
                        int ch;
                        int peek;

                        ch = reader.Read ();
                        if (ch != '0')
                                throw new Exception ("Bad hex number format, first char is not 0");

                        ch = reader.Read ();

                        if (ch != 'x' && ch != 'X')
                                throw new Exception ("Bad hex number format, second char is not x or X");

                        do {
                                ch = reader.Read ();
                                peek = reader.Peek ();
                                num_builder.Append ((char) ch);

                                if (!is_hex ((char) peek))
                                        break;

                                if (num_builder.Length == 32)
                                        throw new Exception ("Number too big.");

                        } while (ch != -1);

                        num = num_builder.ToString ();

                        try {
                                long i = Int64.Parse (num, nstyles);
                                if (i < Int32.MinValue || i > Int32.MaxValue) {
                                        result.token = Token.INT64;
                                        result.val = i;
                                } else {
                                        result.token = Token.INT32;
                                        result.val = (int) i;
                                }
                        } catch {
                                reader.Unread (num.ToCharArray ());
                                reader.RestoreLocation ();
                                num = String.Empty;
                                Reset ();
                                throw new ILSyntaxError ("Bad hex number format! '" + num + "'");
                        }

                        return num;
                }

                /// <summary>
                /// </summary>
                public ILToken ResultToken {
                        get {
                                return result;
                        }
                }


        }

}
