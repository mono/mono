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

// Compile with 
//    csc /r:C5.dll GNfaToDfa.cs

// C5 examples: RegExp -> NFA -> DFA -> Graph
// Java 2000-10-07, GC# 2001-10-23, C# 2.0 2003-09-03, C# 2.0+C5 2004-08-08

// This file contains, in order:
//   * Helper class Set<T> defined in terms of C5 classes.
//   * A class Nfa for representing an NFA (a nondeterministic finite 
//     automaton), and for converting it to a DFA (a deterministic 
//     finite automaton).  Most complexity is in this class.
//   * A class Dfa for representing a DFA, a deterministic finite 
//     automaton, and for writing a dot input file representing the DFA.
//   * Classes for representing regular expressions, and for building an 
//     NFA from a regular expression
//   * A test class that creates an NFA, a DFA, and a dot input file 
//     for a number of small regular expressions.  The DFAs are 
//     not minimized.

using System;
using System.Text;
using System.IO;
using C5;
using SCG = System.Collections.Generic;

namespace GNfaToDfa
{

  public class Set<T> : HashSet<T> {
    public Set(SCG.IEnumerable<T> enm) : base() {
      AddAll(enm);
    }

    public Set(params T[] elems) : this((SCG.IEnumerable<T>)elems) { }

    // Set union (+), difference (-), and intersection (*):

    public static Set<T> operator +(Set<T> s1, Set<T> s2) {
      if (s1 == null || s2 == null) 
        throw new ArgumentNullException("Set+Set");
      else {
        Set<T> res = new Set<T>(s1);
        res.AddAll(s2);
        return res;
      }
    }

    public static Set<T> operator -(Set<T> s1, Set<T> s2) {
      if (s1 == null || s2 == null) 
        throw new ArgumentNullException("Set-Set");
      else {
        Set<T> res = new Set<T>(s1);
        res.RemoveAll(s2);
        return res;
      }
    }

    public static Set<T> operator *(Set<T> s1, Set<T> s2) {
      if (s1 == null || s2 == null) 
        throw new ArgumentNullException("Set*Set");
      else {
        Set<T> res = new Set<T>(s1);
        res.RetainAll(s2);
        return res;
      }
    }

    // Equality of sets; take care to avoid infinite loops

    public static bool operator ==(Set<T> s1, Set<T> s2) {
      return EqualityComparer<Set<T>>.Default.Equals(s1, s2);
    }

    public static bool operator !=(Set<T> s1, Set<T> s2) {
      return !(s1 == s2);
    }

    public override bool Equals(Object that) {
      return this == (that as Set<T>);
    }

    public override int GetHashCode() {
      return EqualityComparer<Set<T>>.Default.GetHashCode(this);
    }

    // Subset (<=) and superset (>=) relation:

    public static bool operator <=(Set<T> s1, Set<T> s2) {
      if (s1 == null || s2 == null) 
        throw new ArgumentNullException("Set<=Set");
      else
        return s1.ContainsAll(s2);
    }

    public static bool operator >=(Set<T> s1, Set<T> s2) {
      if (s1 == null || s2 == null) 
        throw new ArgumentNullException("Set>=Set");
      else
        return s2.ContainsAll(s1);
    }
    
    public override String ToString() {
      StringBuilder sb = new StringBuilder();
      sb.Append("{");
      bool first = true;
      foreach (T x in this) {
        if (!first)
          sb.Append(",");
        sb.Append(x);
        first = false;
      }
      sb.Append("}");
      return sb.ToString();
    }
  }

// ----------------------------------------------------------------------

// Regular expressions, NFAs, DFAs, and dot graphs
// sestoft@dina.kvl.dk * 
// Java 2001-07-10 * C# 2001-10-22 * Gen C# 2001-10-23, 2003-09-03

// In the Generic C# 2.0 version we 
//  use Queue<int> and Queue<Set<int>> for worklists
//  use Set<int> for pre-DFA states
//  use ArrayList<Transition> for NFA transition relations
//  use HashDictionary<Set<int>, HashDictionary<String, Set<int>>>
//  and HashDictionary<int, HashDictionary<String, int>> for DFA transition relations

/* Class Nfa and conversion from NFA to DFA ---------------------------

  A nondeterministic finite automaton (NFA) is represented as a
  dictionary mapping a state number (int) to an arraylist of
  Transitions, a Transition being a pair of a label lab (a string,
  null meaning epsilon) and a target state (an int).

  A DFA is created from an NFA in two steps:

    (1) Construct a DFA whose each of whose states is composite,
        namely a set of NFA states (Set of int).  This is done by
        methods CompositeDfaTrans and EpsilonClose.

    (2) Replace composite states (Set of int) by simple states
        (int).  This is done by methods Rename and MkRenamer.

  Method CompositeDfaTrans works as follows: 

    Create the epsilon-closure S0 (a Set of ints) of the start state
    s0, and put it in a worklist (a Queue).  Create an empty DFA
    transition relation, which is a dictionary mapping a composite
    state (an epsilon-closed set of ints) to a dictionary mapping a
    label (a non-null string) to a composite state.

    Repeatedly choose a composite state S from the worklist.  If it is
    not already in the keyset of the DFA transition relation, compute
    for every non-epsilon label lab the set T of states reachable by
    that label from some state s in S.  Compute the epsilon-closure
    Tclose of every such state T and put it on the worklist.  Then add
    the transition S -lab-> Tclose to the DFA transition relation, for
    every lab.

  Method EpsilonClose works as follows: 

    Given a set S of states.  Put the states of S in a worklist.
    Repeatedly choose a state s from the worklist, and consider all
    epsilon-transitions s -eps-> s' from s.  If s' is in S already,
    then do nothing; otherwise add s' to S and the worklist.  When the
    worklist is empty, S is epsilon-closed; return S.

  Method MkRenamer works as follows: 

    Given a dictionary mapping a set of int to something, create an
    injective dictionary mapping from set of int to int, by choosing a
    fresh int for every key in the given dictionary.

  Method Rename works as follows:

    Given a dictionary mapping a set of int to a dictionary mapping a
    string to set of int, use the result of MkRenamer to replace all
    sets of ints by ints.

*/

  class Nfa {
    private readonly int startState;
    private readonly int exitState;    // This is the unique accept state
    private readonly IDictionary<int, ArrayList<Transition>> trans;

    public Nfa(int startState, int exitState) {
      this.startState = startState; this.exitState = exitState;
      trans = new HashDictionary<int, ArrayList<Transition>>();
      if (!startState.Equals(exitState))
        trans.Add(exitState, new ArrayList<Transition>());
    }

    public int Start { get { return startState; } }

    public int Exit { get { return exitState; } }

    public IDictionary<int, ArrayList<Transition>> Trans {
      get { return trans; }
    }

    public void AddTrans(int s1, String lab, int s2) {
      ArrayList<Transition> s1Trans;
      if (trans.Contains(s1))
        s1Trans = trans[s1];
      else {
        s1Trans = new ArrayList<Transition>();
        trans.Add(s1, s1Trans);
      }
      s1Trans.Add(new Transition(lab, s2));
    }

    public void AddTrans(KeyValuePair<int, ArrayList<Transition>> tr) {
      // Assumption: if tr is in trans, it maps to an empty list (end state)
      trans.Remove(tr.Key);
      trans.Add(tr.Key, tr.Value);
    }

    public override String ToString() {
      return "NFA start=" + startState + " exit=" + exitState;
    }

    // Construct the transition relation of a composite-state DFA from
    // an NFA with start state s0 and transition relation trans (a
    // dictionary mapping int to arraylist of Transition).  The start
    // state of the constructed DFA is the epsilon closure of s0, and
    // its transition relation is a dictionary mapping a composite state
    // (a set of ints) to a dictionary mapping a label (a string) to a
    // composite state (a set of ints).

    static IDictionary<Set<int>, IDictionary<String, Set<int>>>
      CompositeDfaTrans(int s0, IDictionary<int, ArrayList<Transition>> trans) {
      Set<int> S0 = EpsilonClose(new Set<int>(s0), trans);
      IQueue<Set<int>> worklist = new CircularQueue<Set<int>>();
      worklist.Enqueue(S0);
      // The transition relation of the DFA
      IDictionary<Set<int>, IDictionary<String, Set<int>>> res =
        new HashDictionary<Set<int>, IDictionary<String, Set<int>>>();
      while (!worklist.IsEmpty) {
        Set<int> S = worklist.Dequeue();
        if (!res.Contains(S)) {
          // The S -lab-> T transition relation being constructed for a given S
          IDictionary<String, Set<int>> STrans =
            new HashDictionary<String, Set<int>>();
          // For all s in S, consider all transitions s -lab-> t
          foreach (int s in S) {
            // For all non-epsilon transitions s -lab-> t, add t to T
            foreach (Transition tr in trans[s]) {
              if (tr.lab != null) {        // Non-epsilon transition
                Set<int> toState;
                if (STrans.Contains(tr.lab))   // Already a transition on lab
                  toState = STrans[tr.lab];
                else {                         // No transitions on lab yet
                  toState = new Set<int>();
                  STrans.Add(tr.lab, toState);
                }
                toState.Add(tr.target);
              }
            }
          }
          // Epsilon-close all T such that S -lab-> T, and put on worklist
          IDictionary<String, Set<int>> STransClosed =
            new HashDictionary<String, Set<int>>();
          foreach (KeyValuePair<String, Set<int>> entry in STrans) {
            Set<int> Tclose = EpsilonClose(entry.Value, trans);
            STransClosed.Add(entry.Key, Tclose);
            worklist.Enqueue(Tclose);
          }
          res.Add(S, STransClosed);
        }
      }
      return res;
    }

    // Compute epsilon-closure of state set S in transition relation trans.  

    static Set<int>
      EpsilonClose(Set<int> S, IDictionary<int, ArrayList<Transition>> trans) {
      // The worklist initially contains all S members
      IQueue<int> worklist = new CircularQueue<int>();
      S.Apply(worklist.Enqueue);
      Set<int> res = new Set<int>(S);
      while (!worklist.IsEmpty) {
        int s = worklist.Dequeue();
        foreach (Transition tr in trans[s]) {
          if (tr.lab == null && !res.Contains(tr.target)) {
            res.Add(tr.target);
            worklist.Enqueue(tr.target);
          }
        }
      }
      return res;
    }

    // Compute a renamer, which is a dictionary mapping set of int to int

    static IDictionary<Set<int>, int> MkRenamer(ICollectionValue<Set<int>> states)
    {
      IDictionary<Set<int>, int> renamer = new HashDictionary<Set<int>, int>();
      int count = 0;
      foreach (Set<int> k in states)
        renamer.Add(k, count++);
      return renamer;
    }

    // Using a renamer (a dictionary mapping set of int to int), replace
    // composite (set of int) states with simple (int) states in the
    // transition relation trans, which is a dictionary mapping set of
    // int to a dictionary mapping from string to set of int.  The
    // result is a dictionary mapping from int to a dictionary mapping
    // from string to int.

    static IDictionary<int, IDictionary<String, int>>
      Rename(IDictionary<Set<int>, int> renamer,
             IDictionary<Set<int>, IDictionary<String, Set<int>>> trans)
    {
      IDictionary<int, IDictionary<String, int>> newtrans =
        new HashDictionary<int, IDictionary<String, int>>();
      foreach (KeyValuePair<Set<int>, IDictionary<String, Set<int>>> entry
         in trans) {
        Set<int> k = entry.Key;
        IDictionary<String, int> newktrans = new HashDictionary<String, int>();
        foreach (KeyValuePair<String, Set<int>> tr in entry.Value)
          newktrans.Add(tr.Key, renamer[tr.Value]);
        newtrans.Add(renamer[k], newktrans);
      }
      return newtrans;
    }

    static Set<int> AcceptStates(ICollectionValue<Set<int>> states,
               IDictionary<Set<int>, int> renamer,
               int exit)
    {
      Set<int> acceptStates = new Set<int>();
      foreach (Set<int> state in states)
        if (state.Contains(exit))
          acceptStates.Add(renamer[state]);
      return acceptStates;
    }

    public Dfa ToDfa() {
      IDictionary<Set<int>, IDictionary<String, Set<int>>>
        cDfaTrans = CompositeDfaTrans(startState, trans);
      Set<int> cDfaStart = EpsilonClose(new Set<int>(startState), trans);
      ICollectionValue<Set<int>> cDfaStates = cDfaTrans.Keys;
      IDictionary<Set<int>, int> renamer = MkRenamer(cDfaStates);
      IDictionary<int, IDictionary<String, int>> simpleDfaTrans =
        Rename(renamer, cDfaTrans);
      int simpleDfaStart = renamer[cDfaStart];
      Set<int> simpleDfaAccept = AcceptStates(cDfaStates, renamer, exitState);
      return new Dfa(simpleDfaStart, simpleDfaAccept, simpleDfaTrans);
    }

    // Nested class for creating distinctly named states when constructing NFAs

    public class NameSource {
      private static int nextName = 0;

      public int next() {
        return nextName++;
      }
    }

    // Write an input file for the dot program.  You can find dot at
    // http://www.research.att.com/sw/tools/graphviz/

    public void WriteDot(String filename) {
      TextWriter wr =
        new StreamWriter(new FileStream(filename, FileMode.Create,
                                        FileAccess.Write));
      wr.WriteLine("// Format this file as a Postscript file with ");
      wr.WriteLine("//    dot " + filename + " -Tps -o out.ps\n");
      wr.WriteLine("digraph nfa {");
      wr.WriteLine("size=\"11,8.25\";");
      wr.WriteLine("rotate=90;");
      wr.WriteLine("rankdir=LR;");
      wr.WriteLine("start [style=invis];");    // Invisible start node
      wr.WriteLine("start -> d" + startState); // Edge into start state

      // The accept state has a double circle
      wr.WriteLine("d" + exitState + " [peripheries=2];");

      // The transitions 
      foreach (KeyValuePair<int, ArrayList<Transition>> entry in trans) {
        int s1 = entry.Key;
        foreach (Transition s1Trans in entry.Value) {
          String lab = s1Trans.lab ?? "eps";
          int s2 = s1Trans.target;
          wr.WriteLine("d" + s1 + " -> d" + s2 + " [label=\"" + lab + "\"];");
        }
      }
      wr.WriteLine("}");
      wr.Close();
    }
  }

// Class Transition, a transition from one state to another ----------

  public class Transition {
    public readonly String lab;
    public readonly int target;

    public Transition(String lab, int target) {
      this.lab = lab; this.target = target;
    }

    public override String ToString() {
      return "-" + lab + "-> " + target;
    }
  }

// Class Dfa, deterministic finite automata --------------------------

/*
   A deterministic finite automaton (DFA) is represented as a
   dictionary mapping state number (int) to a dictionary mapping label
   (a non-null string) to a target state (an int).
*/

  class Dfa {
    private readonly int startState;
    private readonly Set<int> acceptStates;
    private readonly IDictionary<int, IDictionary<String, int>> trans;

    public Dfa(int startState, Set<int> acceptStates,
	       IDictionary<int, IDictionary<String, int>> trans)
    {
      this.startState = startState;
      this.acceptStates = acceptStates;
      this.trans = trans;
    }

    public int Start { get { return startState; } }

    public Set<int> Accept { get { return acceptStates; } }

    public IDictionary<int, IDictionary<String, int>> Trans {
      get { return trans; }
    }

    public override String ToString() {
      return "DFA start=" + startState + "\naccept=" + acceptStates;
    }

    // Write an input file for the dot program.  You can find dot at
    // http://www.research.att.com/sw/tools/graphviz/

    public void WriteDot(String filename) {
      TextWriter wr =
        new StreamWriter(new FileStream(filename, FileMode.Create,
                                        FileAccess.Write));
      wr.WriteLine("// Format this file as a Postscript file with ");
      wr.WriteLine("//    dot " + filename + " -Tps -o out.ps\n");
      wr.WriteLine("digraph dfa {");
      wr.WriteLine("size=\"11,8.25\";");
      wr.WriteLine("rotate=90;");
      wr.WriteLine("rankdir=LR;");
      wr.WriteLine("start [style=invis];");    // Invisible start node
      wr.WriteLine("start -> d" + startState); // Edge into start state

      // Accept states are double circles
      foreach (int state in trans.Keys)
        if (acceptStates.Contains(state))
          wr.WriteLine("d" + state + " [peripheries=2];");

      // The transitions 
      foreach (KeyValuePair<int, IDictionary<String, int>> entry in trans) {
        int s1 = entry.Key;
        foreach (KeyValuePair<String, int> s1Trans in entry.Value) {
          String lab = s1Trans.Key;
          int s2 = s1Trans.Value;
          wr.WriteLine("d" + s1 + " -> d" + s2 + " [label=\"" + lab + "\"];");
        }
      }
      wr.WriteLine("}");
      wr.Close();
    }
  }

// Regular expressions ----------------------------------------------
//
// Abstract syntax of regular expressions
//    r ::= A | r1 r2 | (r1|r2) | r*
//

  abstract class Regex {
    abstract public Nfa MkNfa(Nfa.NameSource names);
  }

  class Eps : Regex {
    // The resulting nfa0 has form s0s -eps-> s0e

    public override Nfa MkNfa(Nfa.NameSource names) {
      int s0s = names.next();
      int s0e = names.next();
      Nfa nfa0 = new Nfa(s0s, s0e);
      nfa0.AddTrans(s0s, null, s0e);
      return nfa0;
    }
  }

  class Sym : Regex {
    String sym;

    public Sym(String sym) {
      this.sym = sym;
    }

    // The resulting nfa0 has form s0s -sym-> s0e

    public override Nfa MkNfa(Nfa.NameSource names) {
      int s0s = names.next();
      int s0e = names.next();
      Nfa nfa0 = new Nfa(s0s, s0e);
      nfa0.AddTrans(s0s, sym, s0e);
      return nfa0;
    }
  }

  class Seq : Regex {
    Regex r1, r2;

    public Seq(Regex r1, Regex r2) {
      this.r1 = r1; this.r2 = r2;
    }

    // If   nfa1 has form s1s ----> s1e 
    // and  nfa2 has form s2s ----> s2e 
    // then nfa0 has form s1s ----> s1e -eps-> s2s ----> s2e

    public override Nfa MkNfa(Nfa.NameSource names) {
      Nfa nfa1 = r1.MkNfa(names);
      Nfa nfa2 = r2.MkNfa(names);
      Nfa nfa0 = new Nfa(nfa1.Start, nfa2.Exit);
      foreach (KeyValuePair<int, ArrayList<Transition>> entry in nfa1.Trans)
        nfa0.AddTrans(entry);
      foreach (KeyValuePair<int, ArrayList<Transition>> entry in nfa2.Trans)
        nfa0.AddTrans(entry);
      nfa0.AddTrans(nfa1.Exit, null, nfa2.Start);
      return nfa0;
    }
  }

  class Alt : Regex {
    Regex r1, r2;

    public Alt(Regex r1, Regex r2) {
      this.r1 = r1; this.r2 = r2;
    }

    // If   nfa1 has form s1s ----> s1e 
    // and  nfa2 has form s2s ----> s2e 
    // then nfa0 has form s0s -eps-> s1s ----> s1e -eps-> s0e
    //                    s0s -eps-> s2s ----> s2e -eps-> s0e

    public override Nfa MkNfa(Nfa.NameSource names) {
      Nfa nfa1 = r1.MkNfa(names);
      Nfa nfa2 = r2.MkNfa(names);
      int s0s = names.next();
      int s0e = names.next();
      Nfa nfa0 = new Nfa(s0s, s0e);
      foreach (KeyValuePair<int, ArrayList<Transition>> entry in nfa1.Trans)
        nfa0.AddTrans(entry);
      foreach (KeyValuePair<int, ArrayList<Transition>> entry in nfa2.Trans)
        nfa0.AddTrans(entry);
      nfa0.AddTrans(s0s, null, nfa1.Start);
      nfa0.AddTrans(s0s, null, nfa2.Start);
      nfa0.AddTrans(nfa1.Exit, null, s0e);
      nfa0.AddTrans(nfa2.Exit, null, s0e);
      return nfa0;
    }
  }

  class Star : Regex {
    Regex r;

    public Star(Regex r) {
      this.r = r;
    }

    // If   nfa1 has form s1s ----> s1e 
    // then nfa0 has form s0s ----> s0s
    //                    s0s -eps-> s1s
    //                    s1e -eps-> s0s

    public override Nfa MkNfa(Nfa.NameSource names) {
      Nfa nfa1 = r.MkNfa(names);
      int s0s = names.next();
      Nfa nfa0 = new Nfa(s0s, s0s);
      foreach (KeyValuePair<int, ArrayList<Transition>> entry in nfa1.Trans)
        nfa0.AddTrans(entry);
      nfa0.AddTrans(s0s, null, nfa1.Start);
      nfa0.AddTrans(nfa1.Exit, null, s0s);
      return nfa0;
    }
  }

// Trying the RE->NFA->DFA translation on three regular expressions

  class TestNFA {
    public static void Main(String[] args) {
      Regex a = new Sym("A");
      Regex b = new Sym("B");
      Regex c = new Sym("C");
      Regex abStar = new Star(new Alt(a, b));
      Regex bb = new Seq(b, b);
      Regex r = new Seq(abStar, new Seq(a, b));
      // The regular expression (a|b)*ab
      BuildAndShow("ex1", r);
      // The regular expression ((a|b)*ab)*
      BuildAndShow("ex2", new Star(r));
      // The regular expression ((a|b)*ab)((a|b)*ab)
      BuildAndShow("ex3", new Seq(r, r));
      // The regular expression (a|b)*abb, from ASU 1986 p 136
      BuildAndShow("ex4", new Seq(abStar, new Seq(a, bb)));
      // SML reals: sign?((digit+(\.digit+)?))([eE]sign?digit+)?
      Regex d = new Sym("digit");
      Regex dPlus = new Seq(d, new Star(d));
      Regex s = new Sym("sign");
      Regex sOpt = new Alt(s, new Eps());
      Regex dot = new Sym(".");
      Regex dotDigOpt = new Alt(new Eps(), new Seq(dot, dPlus));
      Regex mant = new Seq(sOpt, new Seq(dPlus, dotDigOpt));
      Regex e = new Sym("e");
      Regex exp = new Alt(new Eps(), new Seq(e, new Seq(sOpt, dPlus)));
      Regex smlReal = new Seq(mant, exp);
      BuildAndShow("ex5", smlReal);
    }

    public static void BuildAndShow(String fileprefix, Regex r) {
      Nfa nfa = r.MkNfa(new Nfa.NameSource());
      Console.WriteLine(nfa);
      Console.WriteLine("Writing NFA graph to file");
      nfa.WriteDot(fileprefix + "nfa.dot");
      Console.WriteLine("---");
      Dfa dfa = nfa.ToDfa();
      Console.WriteLine(dfa);
      Console.WriteLine("Writing DFA graph to file");
      dfa.WriteDot(fileprefix + "dfa.dot");
      Console.WriteLine();
    }
  }
}
