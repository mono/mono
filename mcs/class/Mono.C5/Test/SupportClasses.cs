/*
 Copyright (c) 2003-2004 Niels Kokholm <kokholm@itu.dk> and Peter Sestoft <sestoft@dina.kvl.dk>
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
*/

using System;
using C5;
using NUnit.Framework;
using MSG = System.Collections.Generic;


namespace nunit
{
	using System;
	using C5;
	using NUnit.Framework;
	class SC: IComparer<string>
	{
		public int Compare(string a, string b)
		{
			return a.CompareTo(b);
		}


		public void appl(String s)
		{
			System.Console.WriteLine("--{0}", s);
		}
	}



	class IC: IComparer<int>, IComparable<int>, IComparer<IC>, IComparable<IC>
	{
		public int Compare(int a, int b)
		{
			return a > b ? 1 : a < b ? -1 : 0;
		}


		public int Compare(IC a, IC b)
		{
			return a._i > b._i ? 1 : a._i < b._i ? -1 : 0;
		}


		private int _i;


		public int i
		{
			get { return _i; }
			set { _i = value; }
		}


		public IC() { }


		public IC(int i) { _i = i; }


		public int CompareTo(int that) 		{			return _i > that ? 1 : _i < that ? -1 : 0;		}

        public bool Equals(int that) { return _i == that; }


        public int CompareTo(IC that) { return _i > that._i ? 1 : _i < that._i ? -1 : 0; }
        public bool Equals(IC that) { return _i == that._i; }


        public static bool eq(MSG.IEnumerable<int> me, params int[] that)
		{
			int i = 0, maxind = that.Length - 1;

			foreach (int item in me)
				if (i > maxind || item != that[i++])
					return false;

			return i == maxind + 1;
		}
		public static bool seq(ICollection<int> me, params int[] that)
		{
			int[] me2 = me.ToArray();

			Array.Sort(me2);

			int i = 0, maxind = that.Length - 1;

			foreach (int item in me2)
				if (i > maxind || item != that[i++])
					return false;

			return i == maxind + 1;
		}
	}

	
	class RevIC: IComparer<int>
	{
		public int Compare(int a, int b)
		{
			return a > b ? -1 : a < b ? 1 : 0;
		}
	}



	public delegate int Int2Int(int i);


	public class FunEnumerable: MSG.IEnumerable<int>
	{
		int size;

		Int2Int f;


		public FunEnumerable(int size, Int2Int f)
		{
			this.size = size; this.f = f;
		}


		public bool Exists(Filter<int> filter) { return false; }


		public bool All(Filter<int> filter) { return false; }


		public MSG.IEnumerator<int> GetEnumerator()
		{
			for (int i = 0; i < size; i++)
				yield return f(i);
		}

		public void Apply(Applier<int> a) { }
	}

}
