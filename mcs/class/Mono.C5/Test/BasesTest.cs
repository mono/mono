/*
 Copyright (c) 2003-2006 Niels Kokholm and Peter Sestoft
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
using SCG = System.Collections.Generic;


namespace C5UnitTests.support
{
  namespace bases
  {
    [TestFixture]
    public class ArrayBaseTest
    {
      class ABT : ArrayBase<string>
      {
        public ABT() : base(8,NaturalEqualityComparer<string>.Default) { }

        public override string Choose() { if (size > 0) return array[0]; throw new NoSuchItemException(); }

        public string this[int i] { get { return array[i]; } set { array[i] = value; } }


        public int thesize { get { return size; } set { size = value; } }
      }


      [Test]
      public void Check()
      {
        ABT abt = new ABT();

        abt.thesize = 3;
        abt[2] = "aaa";
        // Assert.IsFalse(abt.Check());
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
      class dbl : IComparable<dbl>
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
      [ExpectedException(typeof(NotComparableException))]
      public void NotComparable()
      {
        SCG.IComparer<object> foo = Comparer<object>.Default;
      }

      [Test]
      public void GenericC()
      {
        SCG.IComparer<dbl> h = new NaturalComparer<dbl>();
        dbl s = new dbl(3.4);
        dbl t = new dbl(3.4);
        dbl u = new dbl(7.4);

        Assert.AreEqual(0, h.Compare(s, t));
        Assert.IsTrue(h.Compare(s, u) < 0);
      }


      [Test]
      public void OrdinaryC()
      {
        SCG.IComparer<string> h = new NaturalComparerO<string>();
        string s = "bamse";
        string t = "bamse";
        string u = "bimse";

        Assert.AreEqual(0, h.Compare(s, t));
        Assert.IsTrue(h.Compare(s, u) < 0);
      }


      [Test]
      public void GenericCViaBuilder()
      {
        SCG.IComparer<dbl> h = Comparer<dbl>.Default;
        dbl s = new dbl(3.4);
        dbl t = new dbl(3.4);
        dbl u = new dbl(7.4);

        Assert.AreEqual(0, h.Compare(s, t));
        Assert.IsTrue(h.Compare(s, u) < 0);
        Assert.AreSame(h, Comparer<dbl>.Default);
      }


      [Test]
      public void OrdinaryCViaBuilder()
      {
        SCG.IComparer<string> h = Comparer<string>.Default;
        string s = "bamse";
        string t = "bamse";
        string u = "bimse";

        Assert.AreEqual(0, h.Compare(s, t));
        Assert.IsTrue(h.Compare(s, u) < 0);
        Assert.AreSame(h, Comparer<string>.Default);

      }


      [Test]
      public void ICViaBuilder()
      {
        SCG.IComparer<int> h = Comparer<int>.Default;
        int s = 4;
        int t = 4;
        int u = 5;

        Assert.AreEqual(0, h.Compare(s, t));
        Assert.IsTrue(h.Compare(s, u) < 0);
        Assert.AreSame(h, Comparer<int>.Default);

      }

      [Test]
      public void Nulls()
      {
        Assert.IsTrue(Comparer<string>.Default.Compare(null, "abe") < 0);
        Assert.IsTrue(Comparer<string>.Default.Compare(null, null) == 0);
        Assert.IsTrue(Comparer<string>.Default.Compare("abe", null) > 0);
      }
    }

    [TestFixture]
    public class EqualityComparers
    {
      [Test]
      public void ReftypeequalityComparer()
      {
        SCG.IEqualityComparer<string> h = NaturalEqualityComparer<string>.Default;
        string s = "bamse";
        string t = "bamse";
        string u = "bimse";

        Assert.AreEqual(s.GetHashCode(), h.GetHashCode(s));
        Assert.IsTrue(h.Equals(s, t));
        Assert.IsFalse(h.Equals(s, u));
      }


      [Test]
      public void ValuetypeequalityComparer()
      {
        SCG.IEqualityComparer<double> h = NaturalEqualityComparer<double>.Default;
        double s = 3.4;
        double t = 3.4;
        double u = 5.7;

        Assert.AreEqual(s.GetHashCode(), h.GetHashCode(s));
        Assert.IsTrue(h.Equals(s, t));
        Assert.IsFalse(h.Equals(s, u));
      }

      internal class REHTest { public override int GetHashCode() { return 37; } }

      [Test]
      public void ReferenceEqualityEqualityComparerTest()
      {
        REHTest rehtest = new REHTest();
        SCG.IEqualityComparer<REHTest> equalityComparer = ReferenceEqualityComparer<REHTest>.Default;
        Assert.AreEqual(37, rehtest.GetHashCode());
        Assert.IsFalse(equalityComparer.GetHashCode(rehtest) == 37);
      }

      [Test]
      public void ReftypeequalityComparerViaBuilder()
      {
        SCG.IEqualityComparer<string> h = EqualityComparer<string>.Default;
        string s = "bamse";
        string t = "bamse";
        string u = "bimse";

        Assert.AreEqual(s.GetHashCode(), h.GetHashCode(s));
        Assert.IsTrue(h.Equals(s, t));
        Assert.IsFalse(h.Equals(s, u));
        Assert.AreSame(h, EqualityComparer<string>.Default);
      }


      [Test]
      public void ValuetypeequalityComparerViaBuilder()
      {
        SCG.IEqualityComparer<double> h = EqualityComparer<double>.Default;
        double s = 3.4;
        double t = 3.4;
        double u = 5.7;

        Assert.AreEqual(s.GetHashCode(), h.GetHashCode(s));
        Assert.IsTrue(h.Equals(s, t));
        Assert.IsFalse(h.Equals(s, u));
        Assert.AreSame(h, EqualityComparer<double>.Default);
      }


      [Test]
      public void IntequalityComparerViaBuilder()
      {
        SCG.IEqualityComparer<int> h = EqualityComparer<int>.Default;
        int s = 3;
        int t = 3;
        int u = 5;

        Assert.AreEqual(s.GetHashCode(), h.GetHashCode(s));
        Assert.IsTrue(h.Equals(s, t));
        Assert.IsFalse(h.Equals(s, u));
        Assert.AreSame(h, EqualityComparer<int>.Default);
      }

      [Test]
      public void DoubleequalityComparerViaBuilder()
      {
        SCG.IEqualityComparer<double> h = EqualityComparer<double>.Default;
        double s = 3.1;
        double t = 3.1;
        double u = 5.2;

        Assert.AreEqual(s.GetHashCode(), h.GetHashCode(s));
        Assert.IsTrue(h.Equals(s, t));
        Assert.IsFalse(h.Equals(s, u));
        Assert.AreSame(h, EqualityComparer<double>.Default);
      }

      [Test]
      public void CharequalityComparerViaBuilder()
      {
        SCG.IEqualityComparer<char> h = EqualityComparer<char>.Default;
        char s = 'å';
        char t = 'å';
        char u = 'r';

        Assert.AreEqual(s.GetHashCode(), h.GetHashCode(s));
        Assert.IsTrue(h.Equals(s, t));
        Assert.IsFalse(h.Equals(s, u));
        Assert.AreSame(h, EqualityComparer<char>.Default);
      }

      [Test]
      public void ByteequalityComparerViaBuilder()
      {
        SCG.IEqualityComparer<byte> h = EqualityComparer<byte>.Default;
        byte s = 3;
        byte t = 3;
        byte u = 5;

        Assert.AreEqual(s.GetHashCode(), h.GetHashCode(s));
        Assert.IsTrue(h.Equals(s, t));
        Assert.IsFalse(h.Equals(s, u));
        Assert.AreSame(h, EqualityComparer<byte>.Default);
      }

      [Test]
      public void UnseqequalityComparerViaBuilder()
      {
        SCG.IEqualityComparer<ICollection<int>> h = EqualityComparer<ICollection<int>>.Default;
        ICollection<int> s = new LinkedList<int>();
        ICollection<int> t = new LinkedList<int>();
        ICollection<int> u = new LinkedList<int>();
        s.Add(1); s.Add(2); s.Add(3);
        t.Add(3); t.Add(2); t.Add(1);
        u.Add(3); u.Add(2); u.Add(4);
        Assert.AreEqual(s.GetUnsequencedHashCode(), h.GetHashCode(s));
        Assert.IsTrue(h.Equals(s, t));
        Assert.IsFalse(h.Equals(s, u));
        Assert.AreSame(h, EqualityComparer<ICollection<int>>.Default);
      }

      [Test]
      public void SeqequalityComparerViaBuilder2()
      {
        SCG.IEqualityComparer<LinkedList<int>> h = EqualityComparer<LinkedList<int>>.Default;
        LinkedList<int> s = new LinkedList<int>();
        s.Add(1); s.Add(2); s.Add(3);
        Assert.AreEqual(CHC.sequencedhashcode(1, 2, 3), h.GetHashCode(s));
      }

      [Test]
      public void UnseqequalityComparerViaBuilder2()
      {
        SCG.IEqualityComparer<HashSet<int>> h = EqualityComparer<HashSet<int>>.Default;
        HashSet<int> s = new HashSet<int>();
        s.Add(1); s.Add(2); s.Add(3);
        Assert.AreEqual(CHC.unsequencedhashcode(1, 2, 3), h.GetHashCode(s));
      }

      //generic types implementing collection interfaces
      [Test]
      public void SeqequalityComparerViaBuilder3()
      {
        SCG.IEqualityComparer<IList<int>> h = EqualityComparer<IList<int>>.Default;
        IList<int> s = new LinkedList<int>();
        s.Add(1); s.Add(2); s.Add(3);
        Assert.AreEqual(CHC.sequencedhashcode(1, 2, 3), h.GetHashCode(s));
      }

      interface IFoo<T> : ICollection<T> { void Bamse();      }

      class Foo<T> : HashSet<T>, IFoo<T>
      {
        internal Foo() : base() { }
        public void Bamse() { }
      }

      [Test]
      public void UnseqequalityComparerViaBuilder3()
      {
        SCG.IEqualityComparer<IFoo<int>> h = EqualityComparer<IFoo<int>>.Default;
        IFoo<int> s = new Foo<int>();
        s.Add(1); s.Add(2); s.Add(3);
        Assert.AreEqual(CHC.unsequencedhashcode(1, 2, 3), h.GetHashCode(s));
      }

      //Nongeneric types implementing collection types:
      interface IBaz : ISequenced<int> { void Bamse(); }

      class Baz : LinkedList<int>, IBaz
      {
        internal Baz() : base() { }
        public void Bamse() { }
        //int ISequenced<int>.GetHashCode() { return sequencedhashcode(); }
        //bool ISequenced<int>.Equals(ISequenced<int> that) { return sequencedequals(that); }
      }

#warning This test fails because of an error in .Net 2.0
      //[Test]
      public void SeqequalityComparerViaBuilder4()
      {
        SCG.IEqualityComparer<IBaz> h = EqualityComparer<IBaz>.Default;
        IBaz s = new Baz();
        s.Add(1); s.Add(2); s.Add(3);
        Assert.AreEqual(CHC.sequencedhashcode(1, 2, 3), h.GetHashCode(s));
      }

      interface IBar : ICollection<int>
      {
        void Bamse();
      }

      class Bar : HashSet<int>, IBar
      {
        internal Bar() : base() { }
        public void Bamse() { }

        //TODO: remove all this workaround stuff:
 
        bool ICollection<int>.ContainsAll<U>(System.Collections.Generic.IEnumerable<U> items) 
        {
          throw new NotImplementedException();
        }
 
        void ICollection<int>.RemoveAll<U>(System.Collections.Generic.IEnumerable<U> items) 
        {
          throw new NotImplementedException();
        }

        void ICollection<int>.RetainAll<U>(System.Collections.Generic.IEnumerable<U> items) 
        {
          throw new NotImplementedException();
        }

        void IExtensible<int>.AddAll<U>(System.Collections.Generic.IEnumerable<U> items) 
        {
          throw new NotImplementedException();
        }

      }

      [Test]
      public void UnseqequalityComparerViaBuilder4()
      {
        SCG.IEqualityComparer<IBar> h = EqualityComparer<IBar>.Default;
        IBar s = new Bar();
        s.Add(1); s.Add(2); s.Add(3);
        Assert.AreEqual(CHC.unsequencedhashcode(1, 2, 3), h.GetHashCode(s));
      }
    }
  }
}
