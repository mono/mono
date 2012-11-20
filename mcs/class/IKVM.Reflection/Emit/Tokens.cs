/*
  Copyright (C) 2008 Jeroen Frijters

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net
  
*/

namespace IKVM.Reflection.Emit
{
	public struct EventToken
	{
		public static readonly EventToken Empty;
		private readonly int token;

		internal EventToken(int token)
		{
			this.token = token;
		}

		public int Token
		{
			get { return token; }
		}

		public override bool Equals(object obj)
		{
			return obj as EventToken? == this;
		}

		public override int GetHashCode()
		{
			return token;
		}

		public bool Equals(EventToken other)
		{
			return this == other;
		}

		public static bool operator ==(EventToken et1, EventToken et2)
		{
			return et1.token == et2.token;
		}

		public static bool operator !=(EventToken et1, EventToken et2)
		{
			return et1.token != et2.token;
		}
	}

	public struct FieldToken
	{
		public static readonly FieldToken Empty;
		private readonly int token;

		internal FieldToken(int token)
		{
			this.token = token;
		}

		public int Token
		{
			get { return token; }
		}

		public override bool Equals(object obj)
		{
			return obj as FieldToken? == this;
		}

		public override int GetHashCode()
		{
			return token;
		}

		public bool Equals(FieldToken other)
		{
			return this == other;
		}

		public static bool operator ==(FieldToken ft1, FieldToken ft2)
		{
			return ft1.token == ft2.token;
		}

		public static bool operator !=(FieldToken ft1, FieldToken ft2)
		{
			return ft1.token != ft2.token;
		}
	}

	public struct MethodToken
	{
		public static readonly MethodToken Empty;
		private readonly int token;

		internal MethodToken(int token)
		{
			this.token = token;
		}

		public int Token
		{
			get { return token; }
		}

		public override bool Equals(object obj)
		{
			return obj as MethodToken? == this;
		}

		public override int GetHashCode()
		{
			return token;
		}

		public bool Equals(MethodToken other)
		{
			return this == other;
		}

		public static bool operator ==(MethodToken mt1, MethodToken mt2)
		{
			return mt1.token == mt2.token;
		}

		public static bool operator !=(MethodToken mt1, MethodToken mt2)
		{
			return mt1.token != mt2.token;
		}
	}

	public struct SignatureToken
	{
		public static readonly SignatureToken Empty;
		private readonly int token;

		internal SignatureToken(int token)
		{
			this.token = token;
		}

		public int Token
		{
			get { return token; }
		}

		public override bool Equals(object obj)
		{
			return obj as SignatureToken? == this;
		}

		public override int GetHashCode()
		{
			return token;
		}

		public bool Equals(SignatureToken other)
		{
			return this == other;
		}

		public static bool operator ==(SignatureToken st1, SignatureToken st2)
		{
			return st1.token == st2.token;
		}

		public static bool operator !=(SignatureToken st1, SignatureToken st2)
		{
			return st1.token != st2.token;
		}
	}

	public struct StringToken
	{
		private readonly int token;

		internal StringToken(int token)
		{
			this.token = token;
		}

		public int Token
		{
			get { return token; }
		}

		public override bool Equals(object obj)
		{
			return obj as StringToken? == this;
		}

		public override int GetHashCode()
		{
			return token;
		}

		public bool Equals(StringToken other)
		{
			return this == other;
		}

		public static bool operator ==(StringToken st1, StringToken st2)
		{
			return st1.token == st2.token;
		}

		public static bool operator !=(StringToken st1, StringToken st2)
		{
			return st1.token != st2.token;
		}
	}

	public struct TypeToken
	{
		public static readonly TypeToken Empty;
		private readonly int token;

		internal TypeToken(int token)
		{
			this.token = token;
		}

		public int Token
		{
			get { return token; }
		}

		public override bool Equals(object obj)
		{
			return obj as TypeToken? == this;
		}

		public override int GetHashCode()
		{
			return token;
		}

		public bool Equals(TypeToken other)
		{
			return this == other;
		}

		public static bool operator ==(TypeToken tt1, TypeToken tt2)
		{
			return tt1.token == tt2.token;
		}

		public static bool operator !=(TypeToken tt1, TypeToken tt2)
		{
			return tt1.token != tt2.token;
		}
	}
}
