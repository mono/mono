#if NET_2_0
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


namespace nunit.support
{
	namespace bases
	{
		[TestFixture]
		public class ArrayBaseTest
		{
			class ABT: ArrayBase<string>
			{
				public ABT() : base(8,new DefaultReferenceTypeHasher<string>()) { }


				public string this[int i] { get { return array[i]; } set { array[i] = value; } }


				public int thesize { get { return size; } set { size = value; } }
			}


			[Test]
			public void Check()
			{
				ABT abt = new ABT();

				abt.thesize = 3;
				abt[2] = "aaa";
				Assert.IsFalse(abt.Check());
				abt[0] = "##";
				abt[1] = "##";
				Assert.IsTrue(abt.Check());
			}
		}
	}




	namespace itemops
	{
		[TestFixture]
		public class Comparers
		{
			class dbl: IComparable<dbl>
			{
				double d;

				public dbl(double din) { d = din; }

				public int CompareTo(dbl that)
				{
					return d < that.d ? -1 : d == that.d ? 0 : 1;
				}
                public bool Equals(dbl that) { return d == that.d; }
            }

			[Test]
			public void GenericC()
			{
				IComparer<dbl> h = new NaturalComparer<dbl>();
				dbl s = new dbl(3.4);
				dbl t = new dbl(3.4);
				dbl u = new dbl(7.4);

				Assert.AreEqual(0, h.Compare(s, t));
				Assert.IsTrue(h.Compare(s, u) < 0);
			}


			[Test]
			public void OrdinaryC()
			{
				IComparer<string> h = new NaturalComparerO<string>();
				string s = "bamse";
				string t = "bamse";
				string u = "bimse";

				Assert.AreEqual(0, h.Compare(s, t));
				Assert.IsTrue(h.Compare(s, u) < 0);
			}


			[Test]
			public void GenericCViaBuilder()
			{
				IComparer<dbl> h = C5.ComparerBuilder.FromComparable<dbl>.Examine();
				dbl s = new dbl(3.4);
				dbl t = new dbl(3.4);
				dbl u = new dbl(7.4);

				Assert.AreEqual(0, h.Compare(s, t));
				Assert.IsTrue(h.Compare(s, u) < 0);
			}

			[Test]
			public void OrdinaryCViaBuilder()
			{
				IComparer<string> h = C5.ComparerBuilder.FromComparable<string>.Examine();
				string s = "bamse";
				string t = "bamse";
				string u = "bimse";

				Assert.AreEqual(0, h.Compare(s, t));
				Assert.IsTrue(h.Compare(s, u) < 0);
			}

			[Test]
			public void ICViaBuilder()
			{
				IComparer<int> h = C5.ComparerBuilder.FromComparable<int>.Examine();
				int s = 4;
				int t = 4;
				int u = 5;

				Assert.AreEqual(0, h.Compare(s, t));
				Assert.IsTrue(h.Compare(s, u) < 0);
			}

		}

		[TestFixture]
		public class Hashers
		{
			[Test]
			public void Reftypehasher()
			{
				IHasher<string> h = new DefaultReferenceTypeHasher<string>();
				string s = "bamse";
				string t = "bamse";
				string u = "bimse";

				Assert.AreEqual(s.GetHashCode(), h.GetHashCode(s));
				Assert.IsTrue(h.Equals(s, t));
				Assert.IsFalse(h.Equals(s, u));
			}


			[Test]
			public void Valuetypehasher()
			{
				IHasher<double> h = new DefaultValueTypeHasher<double>();
				double s = 3.4;
				double t = 3.4;
				double u = 5.7;

				Assert.AreEqual(s.GetHashCode(), h.GetHashCode(s));
				Assert.IsTrue(h.Equals(s, t));
				Assert.IsFalse(h.Equals(s, u));
			}


			[Test]
			public void ReftypehasherViaBuilder()
			{
				IHasher<string> h = C5.HasherBuilder.ByPrototype<string>.Examine();
				string s = "bamse";
				string t = "bamse";
				string u = "bimse";

				Assert.AreEqual(s.GetHashCode(), h.GetHashCode(s));
				Assert.IsTrue(h.Equals(s, t));
				Assert.IsFalse(h.Equals(s, u));
			}


			[Test]
			public void ValuetypehasherViaBuilder()
			{
				IHasher<double> h = C5.HasherBuilder.ByPrototype<double>.Examine();
				double s = 3.4;
				double t = 3.4;
				double u = 5.7;

				Assert.AreEqual(s.GetHashCode(), h.GetHashCode(s));
				Assert.IsTrue(h.Equals(s, t));
				Assert.IsFalse(h.Equals(s, u));
			}


			[Test]
			public void InthasherViaBuilder()
			{
				IHasher<int> h = C5.HasherBuilder.ByPrototype<int>.Examine();
				int s = 3;
				int t = 3;
				int u = 5;

				Assert.AreEqual(s.GetHashCode(), h.GetHashCode(s));
				Assert.IsTrue(h.Equals(s, t));
				Assert.IsFalse(h.Equals(s, u));
			}

			[Test]
			public void UnseqhasherViaBuilder()
			{
				IHasher<ICollection<int>> h = C5.HasherBuilder.ByPrototype<ICollection<int>>.Examine();
				ICollection<int> s = new LinkedList<int>();
				ICollection<int> t = new LinkedList<int>();
				ICollection<int> u = new LinkedList<int>();
				s.Add(1);s.Add(2);s.Add(3);
				t.Add(3);t.Add(2);t.Add(1);
				u.Add(3);u.Add(2);u.Add(4);
				Assert.AreEqual(s.GetHashCode(), h.GetHashCode(s));
				Assert.IsTrue(h.Equals(s, t));
				Assert.IsFalse(h.Equals(s, u));
			}


			[Test]
			public void SeqhasherViaBuilder()
			{
				IHasher<ISequenced<int>> h = C5.HasherBuilder.ByPrototype<ISequenced<int>>.Examine();
				ISequenced<int> s = new LinkedList<int>();
				ISequenced<int> t = new LinkedList<int>();
				ISequenced<int> u = new LinkedList<int>();
				s.Add(1);s.Add(2);s.Add(3);
				t.Add(1);t.Add(2);t.Add(3);
				u.Add(3);u.Add(2);u.Add(1);
				Assert.AreEqual(s.GetHashCode(), h.GetHashCode(s));
				Assert.IsTrue(h.Equals(s, t));
				Assert.IsFalse(h.Equals(s, u));
			}


		}


	}
}
#endif
