//
// System.Diagnostics.SymbolStore.SymbolToken.cs
//
// Authors:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (c) 2002 Duco Fijma
// (C) 2003 Andreas Nahr
// 

namespace System.Diagnostics.SymbolStore
{
	public struct SymbolToken 
	{

		private int _val;

		public SymbolToken (int val)
		{
			_val = val;
		}

		public override bool Equals (object obj) 
		{
			if (!(obj is SymbolToken))
				return false;
			return ((SymbolToken) obj).GetToken() == _val;
		}

		public override int GetHashCode()
		{
			return _val.GetHashCode(); 
		}

		public int GetToken()
		{
			return _val; 
		}
	}
}
