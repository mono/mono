// ILToken.cs
// Author: Sergey Chaban (serge@wildwestsoftware.com)


using System;

namespace Mono.ILASM {

        public class ILToken : ICloneable {
                internal int token;
                internal object val;

                public static readonly ILToken Invalid;
                public static readonly ILToken EOF;

                public static readonly ILToken Dot;

                public static readonly ILToken OpenBrace;
                public static readonly ILToken CloseBrace;
                public static readonly ILToken OpenBracket;
                public static readonly ILToken CloseBracket;
                public static readonly ILToken OpenParens;
                public static readonly ILToken CloseParens;
                public static readonly ILToken Comma;
                public static readonly ILToken Colon;
                public static readonly ILToken DoubleColon;
                public static readonly ILToken Semicolon;
                public static readonly ILToken Assign;
                public static readonly ILToken Star;
                public static readonly ILToken Ampersand;
                public static readonly ILToken Plus;
                public static readonly ILToken Slash;
                public static readonly ILToken Bang;
                public static readonly ILToken Ellipsis;
                public static readonly ILToken Dash;
                public static readonly ILToken OpenAngleBracket;
                public static readonly ILToken CloseAngleBracket;

                private static readonly ILToken [] punctuations;

                /// <summary>
                /// </summary>
                static ILToken ()
                {
                        Invalid = new ILToken (-1, "invalid");
                        EOF = new ILToken (Token.EOF, "eof");

                        Dot = new ILToken (Token.DOT, ".");
                        OpenBrace = new ILToken (Token.OPEN_BRACE, "{");
                        CloseBrace = new ILToken (Token.CLOSE_BRACE, "}");
                        OpenBracket = new ILToken (Token.OPEN_BRACKET, "[");
                        CloseBracket = new ILToken (Token.CLOSE_BRACKET, "]");
                        OpenParens = new ILToken (Token.OPEN_PARENS, "(");
                        CloseParens = new ILToken (Token.CLOSE_PARENS, ")");
                        Comma = new ILToken (Token.COMMA, ",");
                        Colon = new ILToken (Token.COLON, ":");
                        DoubleColon = new ILToken (Token.DOUBLE_COLON, "::");
                        Semicolon = new ILToken (Token.SEMICOLON, ";");
                        Assign = new ILToken (Token.ASSIGN, "=");
                        Star = new ILToken (Token.STAR, "*");
                        Ampersand = new ILToken (Token.AMPERSAND, "&");
                        Plus = new ILToken (Token.PLUS, "+");
                        Slash = new ILToken (Token.SLASH, "/");
                        Bang = new ILToken (Token.BANG, "!");
                        Ellipsis = new ILToken (Token.ELLIPSIS, "...");
                        Dash = new ILToken (Token.DASH, "-");
                        OpenAngleBracket = new ILToken (Token.OPEN_ANGLE_BRACKET, "<");
                        CloseAngleBracket = new ILToken (Token.CLOSE_ANGLE_BRACKET, ">");

                        punctuations = new ILToken [] {
                                OpenBrace, CloseBrace,
                                OpenBracket, CloseBracket,
                                OpenParens, CloseParens,
                                Comma, Colon, Semicolon,
                                Assign, Star, Ampersand,
                                Plus, Slash, Bang,
                                OpenAngleBracket, CloseAngleBracket
                        };
                }

                /// <summary>
                /// </summary>
                public ILToken ()
                {
                }

                /// <summary>
                /// </summary>
                /// <param name="token"></param>
                /// <param name="val"></param>
                public ILToken (int token, object val)
                {
                        this.token = token;
                        this.val = val;
                }


                /// <summary>
                /// </summary>
                /// <param name="that"></param>
                public ILToken (ILToken that)
                {
                        this.token = that.token;
                        this.val = that.val;
                }



                /// <summary>
                /// </summary>
                public int TokenId {
                        get {
                                return token;
                        }
                }

                /// <summary>
                /// </summary>
                public object Value {
                        get {
                                return val;
                        }
                }


                /// <summary>
                /// </summary>
                /// <param name="that"></param>
                public virtual void CopyFrom (ILToken that)
                {
                        this.token = that.token;
                        this.val = that.val;
                }


                /// <summary>
                /// </summary>
                /// <returns></returns>
                public virtual object Clone ()
                {
                        return new ILToken (this);
                }


                /// <summary>
                /// </summary>
                /// <returns></returns>
                public override int GetHashCode ()
                {
                        int h = token;
                        if (val != null) h ^= val.GetHashCode ();
                        return h;
                }


                /// <summary>
                /// </summary>
                /// <returns></returns>
                public override string ToString ()
                {
                        return (token.ToString() + " : " + (val != null ? val.ToString () : "<null>"));
                }


                /// <summary>
                /// </summary>
                /// <param name="o"></param>
                /// <returns></returns>
                public override bool Equals (object o)
                {
                        bool res = (o != null);

                        if (res) {
                                res = Object.ReferenceEquals (this, o);
                                if (!res) {
                                        res = o is ILToken;
                                        if (res) {
                                                ILToken that = o as ILToken;
                                                res = (this.token == that.token) && (this.val.Equals (that.val));
                                        }
                                }
                        }

                        return res;
                }


                private static bool EqImpl (ILToken t1, ILToken t2)
                {
                        bool res = false;
                        if ((t1 as object) != null) {
                                res = t1.Equals (t2);
                        } else {
                                res = ((t2 as object) == null);
                        }

                        return res;
                }


                /// <summary>
                /// </summary>
                /// <param name="t1"></param>
                /// <param name="t2"></param>
                /// <returns></returns>
                public static bool operator == (ILToken t1, ILToken t2)
                {
                        return EqImpl (t1, t2);
                }

                /// <summary>
                /// </summary>
                /// <param name="t1"></param>
                /// <param name="t2"></param>
                /// <returns></returns>
                public static bool operator != (ILToken t1, ILToken t2)
                {
                        return !EqImpl (t1, t2);
                }



                /// <summary>
                /// </summary>
                /// <param name="ch"></param>
                /// <returns></returns>
                public static ILToken GetPunctuation (int ch)
                {
                        int id = "{}[](),:;=*&+/!<>".IndexOf ((char) ch);
                        ILToken res = null;

                        if (id != -1) {
                                res = punctuations [id];
                        }

                        return res;
                }


        }
}
