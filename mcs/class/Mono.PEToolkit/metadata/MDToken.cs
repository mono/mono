/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;
using System.Runtime.InteropServices;

namespace Mono.PEToolkit.Metadata {

	[StructLayoutAttribute(LayoutKind.Sequential)]
	public struct MDToken {

		internal int token;

		/// <summary>
		/// Creates new token with specified token type and record id.
		/// </summary>
		/// <param name="type">Token type.</param>
		/// <param name="rid">Record IDentifier.</param>
		public MDToken(TokenType type, int rid)
		{
			token = (int)type | rid;
		}

		/// <summary>
		/// Creates new Nil token of a given type.
		/// </summary>
		/// <param name="type"></param>
		public MDToken(TokenType type) : this(type, 0)
		{
		}

		/// <summary>
		/// </summary>
		/// <param name="tok"></param>
		public MDToken(MDToken tok) : this(tok.Type, tok.RID)
		{
		}


		/// <summary>
		///  Gets or sets metadata token Record IDentifier (RID).
		/// </summary>
		public int RID {
			get {
				return token & (~(int)TokenType.__mask);
			}
			set {
				token &= (int)TokenType.__mask;
				token |= value;
			}
		}

		/// <summary>
		///  Gets or sets metadata token type.
		/// </summary>
		public TokenType Type {
			get {
				return (TokenType) token & (TokenType.__mask);
			}
			set {
				token &= ~(int)TokenType.__mask;
				token |= (int)value;
			}
		}

		/// <summary>
		/// Returns true if this token is a Nil token (it's RID is 0).
		/// </summary>
		public bool IsNilToken {
			get {
				return (RID == 0);
			}
		}


		/// <summary>
		/// Returns token value.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode ()
		{
			return token;
		}



		// See Metadata Unmanaged API doc (10.8)
		public int Compress(out int len)
		{
			int res = token;
			len = 4;
			int rid = this.RID;

			// Make room for type bits.
			rid <<= 2;

			TokenType type = this.Type;

			// Token type (table that this token indexes) is encoded
			// in the least significant 2 bits:
			//   TypeDef  = 0
			//   TypeRef  = 1
			//   TypeSpec = 2
			//   BaseType = 3
			switch (type) {
				case TokenType.TypeDef:
					break;
				case TokenType.TypeRef:
					rid |= 1;
					break;
				case TokenType.TypeSpec:
					rid |= 2;
					break;
				case TokenType.BaseType:
					rid |= 3;
					break;
				default:
					// Invalid operation for this type of token.
					return res;
			}

			len = MDUtils.CompressData(rid, out res);

			return res;
		}


		unsafe public static int Size {
			get {
				return sizeof (int);
			}
		}

		public static implicit operator MDToken (uint val) {
			MDToken res = new MDToken();
			res.token = (int) val;
			return res;
		}

		public static implicit operator uint (MDToken tok) {
			return (uint)tok.token;
		}

		public override string ToString()
		{
			if (this.token == 0) return "NULL";
			return String.Format("{0}[{1}]",
				((int)Type >> (int)TokenType.__shift <= (int)TableId.MAX)
				? ((TableId)((int)Type >> (int)TokenType.__shift)).ToString()
				: Type.ToString(), RID);
			//String.Format ("type = {0}, RID = {1}", Type, RID);
		}

	}

}

