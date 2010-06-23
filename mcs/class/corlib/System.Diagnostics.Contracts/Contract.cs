//
// System.Diagnostics.Contracts.Contract.cs
//
// Authors:
//    Miguel de Icaza (miguel@gnome.org)
//    Chris Bacon (chrisbacon76@gmail.com)
//    Marek Safar (marek.safar@gmail.com)
//
// Copyright 2009, 2010 Novell (http://www.novell.com)
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

#if NET_4_0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts.Internal;
using System.Runtime.ConstrainedExecution;

namespace System.Diagnostics.Contracts
{
	public static class Contract
	{
		public static event EventHandler<ContractFailedEventArgs> ContractFailed;

		// Used in test
		internal static EventHandler<ContractFailedEventArgs> InternalContractFailedEvent {
			get { return ContractFailed; }
		}

		// Used in test
		internal static Type GetContractExceptionType ()
		{
			return typeof (ContractException);
		}

		// Used in test
		internal static Type GetContractShouldAssertExceptionType ()
		{
			return typeof (ContractShouldAssertException);
		}

		static void ReportFailure (ContractFailureKind kind, string userMessage, string conditionText, Exception innerException)
		{
			string msg = ContractHelper.RaiseContractFailedEvent (kind, userMessage, conditionText, innerException);
			// if msg is null, then it has been handled already, so don't do anything here
			if (msg != null)
				ContractHelper.TriggerFailure (kind, msg, userMessage, conditionText, innerException);
		}

		static void AssertMustUseRewriter (ContractFailureKind kind, string message)
		{
			if (Environment.UserInteractive) {
				// FIXME: This should trigger an assertion.
				// But code will never get here at the moment, as Environment.UserInteractive currently
				// always returns false.
				throw new ContractShouldAssertException (message);
			} else {
				// TODO: Note that FailFast() currently throws a NotImplementedException()
				Environment.FailFast(message/*, new ExecutionEngineException()*/);
			}
		}

		[ConditionalAttribute("CONTRACTS_FULL")]
		[ConditionalAttribute("DEBUG")]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public static void Assert (bool condition)
		{
			if (condition)
				return;

			ReportFailure (ContractFailureKind.Assert, null, null, null);
		}

		[ConditionalAttribute("DEBUG")]
		[ConditionalAttribute("CONTRACTS_FULL")]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public static void Assert (bool condition, string userMessage)
		{
			if (condition)
				return;

			ReportFailure (ContractFailureKind.Assert, userMessage, null, null);
		}

		[ConditionalAttribute("DEBUG")]
		[ConditionalAttribute("CONTRACTS_FULL")]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public static void Assume(bool condition)
		{
			// At runtime, this behaves like assert
			if (condition)
				return;

			ReportFailure (ContractFailureKind.Assume, null, null, null);
		}

		[ConditionalAttribute("CONTRACTS_FULL")]
		[ConditionalAttribute("DEBUG")]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public static void Assume (bool condition, string userMessage)
		{
			// At runtime, this behaves like assert
			if (condition)
				return;

			ReportFailure (ContractFailureKind.Assume, userMessage, null, null);
		}

		[ConditionalAttribute("CONTRACTS_FULL")]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public static void EndContractBlock ()
		{
			// Marker method, no code required.
		}

		[ConditionalAttribute("CONTRACTS_FULL")]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public static void Ensures (bool condition)
		{
			AssertMustUseRewriter (ContractFailureKind.Postcondition, "Contract.Ensures");
		}

		[ConditionalAttribute("CONTRACTS_FULL")]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public static void Ensures (bool condition, string userMessage)
		{
			AssertMustUseRewriter (ContractFailureKind.Postcondition, "Contract.Ensures");
		}

		[ConditionalAttribute("CONTRACTS_FULL")]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public static void EnsuresOnThrow<TException> (bool condition) where TException : Exception
		{
			AssertMustUseRewriter (ContractFailureKind.Postcondition, "Contract.EnsuresOnThrow");
		}

		[ConditionalAttribute("CONTRACTS_FULL")]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public static void EnsuresOnThrow<TException> (bool condition, string userMessage) where TException : Exception
		{
			AssertMustUseRewriter (ContractFailureKind.Postcondition, "Contract.EnsuresOnThrow");
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
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

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
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

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
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

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
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
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public static void Invariant (bool condition)
		{
			AssertMustUseRewriter (ContractFailureKind.Invariant, "Contract.Invariant");
		}

		[ConditionalAttribute("CONTRACTS_FULL")]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public static void Invariant (bool condition, string userMessage)
		{
			AssertMustUseRewriter (ContractFailureKind.Invariant, "Contract.Invariant");
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public static T OldValue<T> (T value)
		{
			// Marker method, no code required.
			return default (T);
		}

		[ConditionalAttribute("CONTRACTS_FULL")]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public static void Requires (bool condition)
		{
			AssertMustUseRewriter (ContractFailureKind.Precondition, "Contract.Requires");
		}

		[ConditionalAttribute("CONTRACTS_FULL")]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public static void Requires (bool condition, string userMessage)
		{
			AssertMustUseRewriter (ContractFailureKind.Precondition, "Contract.Requires");
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public static void Requires<TException> (bool condition) where TException : Exception
		{
			AssertMustUseRewriter (ContractFailureKind.Precondition, "Contract.Requires<TException>");
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public static void Requires<TException> (bool condition, string userMessage) where TException : Exception
		{
			AssertMustUseRewriter (ContractFailureKind.Precondition, "Contract.Requires<TException>");
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public static T Result<T> ()
		{
			// Marker method, no code required.
			return default (T);
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public static T ValueAtReturn<T> (out T value)
		{
			// Marker method, no code required.
			return value = default (T);
		}
	}
}

#endif
