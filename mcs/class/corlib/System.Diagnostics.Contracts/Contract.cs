//
// System.Diagnostics.Contracts.Contract.cs
//
// Authors:
//    Miguel de Icaza (miguel@gnome.org)
//
// Copyright 2009 Novell (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

//
// Things left to do:
// 
//   * This is a blind implementation from specs, without any testing, so the escalation
//     is probably broken, and so are the messages and arguments to the eventargs properties
//
//   * How to plug the original condition into the Escalate method?   Perhaps we need
//     some injection for it?
//
//   * The "originalException" in Escalate is nowhere used
//
//   * We do not Escalate everything that needs to be, perhaps that is the role of the
//     rewriter to call Escalate with the proper values?
//
//   * I added a "new()" constraint to methods that took a TException because I needed
//     to new the exception, but this is perhaps wrong.
//
//   * Result and ValueAtReturn, I need to check what these do in .NET 4, but have not
//     installed it yet ;-)
//
using System;
using System.Collections.Generic;

namespace System.Diagnostics.Contracts {
#if NET_2_1 || NET_4_0
	public
#else
	internal
#endif

	static class Contract {

		public static event EventHandler<ContractFailedEventArgs> ContractFailed;

		static void Escalate (ContractFailureKind kind, Exception e, string text, params object [] args)
		{
			if (ContractFailed != null){
				var ea = new ContractFailedEventArgs (kind, text, "<condition>", e ?? new Exception ());
				ContractFailed (null, ea);
				if (!ea.Unwind)
					return;
			}
			Console.Error.WriteLine ("Contract.{0}: {1}", kind.ToString (), String.Format (text, args));
		}
		
		[ConditionalAttribute("CONTRACTS_FULL")]
		[ConditionalAttribute("DEBUG")]
		public static void Assert (bool condition)
		{
			if (condition)
				return;
			Escalate (ContractFailureKind.Assert, null, "failure");
		}

		[ConditionalAttribute("DEBUG")]
		[ConditionalAttribute("CONTRACTS_FULL")]
		public static void Assert (bool condition, string userMessage)
		{
			if (condition)
				return;
			
			Escalate (ContractFailureKind.Assert, null, userMessage);
		}

		[ConditionalAttribute("DEBUG")]
		[ConditionalAttribute("CONTRACTS_FULL")]
		public static void Assume(bool condition)
		{
			// At runtime, this behaves like assert
			if (condition)
				return;

			Escalate (ContractFailureKind.Assume, null, "failure");
		}

		[ConditionalAttribute("CONTRACTS_FULL")]
		[ConditionalAttribute("DEBUG")]
		public static void Assume (bool condition, string userMessage)
		{
			if (condition)
				return;
			
			Escalate (ContractFailureKind.Assume, null, userMessage);
		}

		[ConditionalAttribute("CONTRACTS_FULL")]
		public static void EndContractBlock ()
		{
			// seems to be some kind of flag, no code generated
		}

		[ConditionalAttribute("CONTRACTS_FULL")]
		public static void Ensures (bool condition)
		{
			// Requires binary rewriter to work
		}

		[ConditionalAttribute("CONTRACTS_FULL")]
		public static void Ensures (bool condition, string userMessage)
		{
			// Requires binary rewriter to work
		}

		[ConditionalAttribute("CONTRACTS_FULL")]
		public static void EnsuresOnThrow<TException> (bool condition) where TException : Exception
		{
			// Requires binary rewriter to work
		}

		[ConditionalAttribute("CONTRACTS_FULL")]
		public static void EnsuresOnThrow<TException> (bool condition, string userMessage) where TException : Exception
		{
			// Requires binary rewriter to work
		}

		public static bool Exists<T> (IEnumerable<T> collection, Predicate<T> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException ("predicate");
			if (collection == null)
				throw new ArgumentNullException ("collection");
			
			foreach (var t in collection)
				if (predicate (t))
					return true;
			return false;
		}

		public static bool Exists (int fromInclusive, int toExclusive, Predicate<int> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException ("predicate");
			if (toExclusive < fromInclusive)
				throw new ArgumentException ("toExclusitve < fromInclusive");
			
			for (int i = fromInclusive; i < toExclusive; i++)
				if (predicate (i))
					return true;

			return false;
		}

		public static bool ForAll<T> (IEnumerable<T> collection, Predicate<T> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException ("predicate");
			if (collection == null)
				throw new ArgumentNullException ("collection");
			
			foreach (var t in collection)
				if (!predicate (t))
					return false;

			return true;
		}

		public static bool ForAll (int fromInclusive, int toExclusive, Predicate<int> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException ("predicate");
			if (toExclusive < fromInclusive)
				throw new ArgumentException ("toExclusitve < fromInclusive");
			
			for (int i = fromInclusive; i < toExclusive; i++)
				if (!predicate (i))
					return false;

			return true;
		}

		[ConditionalAttribute("CONTRACTS_FULL")]
		public static void Invariant (bool condition)
		{
			// Binary rewriter required
		}

		[ConditionalAttribute("CONTRACTS_FULL")]
		public static void Invariant (bool condition, string userMessage)
		{
			// Binary rewriter required
		}

		static Exception RewriterRequired ()
		{
			return new Exception ("The rewriter is required to use this method");
		}
	
		public static T OldValue<T> (T value)
		{
			// This is really the binary rewriter that should kick-in
			throw RewriterRequired ();
		}

		[ConditionalAttribute("CONTRACTS_FULL")]
		[MonoTODO ("Currently throws Exception, needs to throw the proper exception")]
		public static void Requires (bool condition)
		{
			if (!condition){
				Escalate (ContractFailureKind.Precondition, null, "failure");
				throw new Exception ();
			}
		}

		[ConditionalAttribute("CONTRACTS_FULL")]
		[MonoTODO ("Currently throws Exception, needs to throw the proper exception")]
		public static void Requires (bool condition, string userMessage)
		{
			if (!condition){
				Escalate (ContractFailureKind.Precondition, null, userMessage);
				throw new Exception ();
			}
		}

		public static void Requires<TException> (bool condition) where TException : Exception, new ()
		{
			if (!condition){
				var e = new TException ();
				Escalate (ContractFailureKind.Precondition, e, "failure");
				throw e;
			}
		}

		public static void Requires<TException>(bool condition, string userMessage) where TException : Exception, new ()
		{
			if (!condition){
				var e = new TException ();
				Escalate (ContractFailureKind.Precondition, e, userMessage);
				throw e;
			}
		}

		public static T Result<T> ()
		{
			throw RewriterRequired ();
		}

		public static T ValueAtReturn<T> (out T value)
		{
			throw RewriterRequired ();
		}
	}
}
