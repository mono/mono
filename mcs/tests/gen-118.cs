//-- ex-gen-class-polynomial

using System;

// A type implements AddMul<A,R> if one can add an A to it, giving an R:

interface AddMul<A,R> {
  R Add(A e);                   // Addition with A, giving R
  R Mul(A e);                   // Multiplication with A, giving R
}

// Polynomials over E, Polynomial<E>:

// The base type E of the polynomial must support addition,
// multiplication and zero (via the nullary constructor).  That's what
// the type parameter constraint on E says.

// In return, one can add an E or a polynomial over E to a polynomial
// over E.  Similarly, a polynomial over E can be multiplied by an E
// or by a polynomial over E.  That's what the interface clauses say.

class Polynomial<E> : AddMul<E,Polynomial<E>>,
                      AddMul<Polynomial<E>,Polynomial<E>>
  where E : AddMul<E,E>, new() {
  // cs contains coefficients of x^0, x^1, ...; absent coefficients are zero.
  // Invariant: cs != null && cs.Length >= 0; cs.Length==0 represents zero.
  private readonly E[] cs;  

  public Polynomial() { 
    this.cs = new E[0];
  }

  public Polynomial(E[] cs) { 
    this.cs = cs;
  }

  public Polynomial<E> Add(Polynomial<E> that) {
    int newlen = Math.Max(this.cs.Length, that.cs.Length);
    int minlen = Math.Min(this.cs.Length, that.cs.Length);
    E[] newcs = new E[newlen];
    if (this.cs.Length <= that.cs.Length) {
      for (int i=0; i<minlen; i++)
        newcs[i] = this.cs[i].Add(that.cs[i]);
      for (int i=minlen; i<newlen; i++)
        newcs[i] = that.cs[i];
    } else {
      for (int i=0; i<minlen; i++)
        newcs[i] = this.cs[i].Add(that.cs[i]);
      for (int i=minlen; i<newlen; i++)
        newcs[i] = this.cs[i];
    }
    return new Polynomial<E>(newcs);
  }

  public Polynomial<E> Add(E that) {
    return this.Add(new Polynomial<E>(new E[] { that }));
  } 

  public Polynomial<E> Mul(E that) {
    E[] newcs = new E[cs.Length];
    for (int i=0; i<cs.Length; i++)
      newcs[i] = that.Mul(cs[i]);
    return new Polynomial<E>(newcs);
  }

  public Polynomial<E> Mul(Polynomial<E> that) {
    int newlen = Math.Max(1, this.cs.Length + that.cs.Length - 1);
    E[] newcs = new E[newlen];
    for (int i=0; i<newlen; i++) {
      E sum = new E();                     // Permitted by constraint E : new()
      int start = Math.Max(0, i-that.cs.Length+1);
      int stop  = Math.Min(i, this.cs.Length-1);
      for (int j=start; j<=stop; j++) {
        // assert 0<=j && j<this.cs.Length && 0<=i-j && i-j<that.cs.Length;
        sum = sum.Add(this.cs[j].Mul(that.cs[i-j]));
      }
      newcs[i] = sum;
    }
    return new Polynomial<E>(newcs);
  }

  public E Eval(E x) {
    E res = new E();                       // Permitted by constraint E : new()
    for (int j=cs.Length-1; j>=0; j--) 
      res = res.Mul(x).Add(cs[j]);
    return res;
  }
}  

struct Int : AddMul<Int,Int> {
  private readonly int i;
  public Int(int i) {
    this.i = i;
  }
  public Int Add(Int that) {
    return new Int(this.i + that.i);
  }
  public Int Mul(Int that) {
    return new Int(this.i * that.i);
  }
  public override String ToString() {
    return i.ToString();
  }
}

class TestPolynomial {
  public static void Main(String[] args) {
    // The integer polynomial 2 + 5x + x^2
    Polynomial<Int> ip = 
      new Polynomial<Int>(new Int[] { new Int(2), new Int(5), new Int(1) });
    Console.WriteLine(ip.Eval(new Int(10)));            // 152
    Console.WriteLine(ip.Add(ip).Eval(new Int(10)));    // 304 = 152 + 152
    Console.WriteLine(ip.Mul(ip).Eval(new Int(10)));    // 23104 = 152 * 152
  }
}

