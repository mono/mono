// System.Runtime.CompilerServices.RuntimeHelpers
//
// Sean MacIsaac (macisaac@ximian.com)
// Paolo Molaro (lupus@ximian.com)
//
// (C) Ximian, Inc. 2001

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Runtime.Serialization;
using System.Runtime.Versioning;
using System.Runtime.ConstrainedExecution;
using System.Reflection;

namespace System.Runtime.CompilerServices
{
	public static partial class RuntimeHelpers
	{
		public delegate void TryCode (Object userData);

		public delegate void CleanupCode (Object userData, bool exceptionThrown);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern void InitializeArray (Array array, IntPtr fldHandle);

		public static void InitializeArray (Array array, RuntimeFieldHandle fldHandle)
		{
			if ((array == null) || (fldHandle.Value == IntPtr.Zero))
				throw new ArgumentNullException ();

			InitializeArray (array, fldHandle.Value);
		}

		public static extern int OffsetToStringData {
			[MethodImpl (MethodImplOptions.InternalCall)]
			get;
		}

		public static int GetHashCode (object o) {
			return Object.InternalGetHashCode (o);
		}

		public static new bool Equals (object o1, object o2) {
			// LAMESPEC: According to MSDN, this is equivalent to 
			// Object::Equals (). But the MS version of Object::Equals()
			// includes the functionality of ValueType::Equals(), while
			// our version does not.
			if (o1 == o2)
				return true;
			if ((o1 == null) || (o2 == null))
				return false;
			if (o1 is ValueType)
				return ValueType.DefaultEquals (o1, o2);
			else
				return Object.Equals (o1, o2);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern object GetObjectValue (object obj);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern void RunClassConstructor (IntPtr type);

		public static void RunClassConstructor (RuntimeTypeHandle type)
		{
			if (type.Value == IntPtr.Zero)
				throw new ArgumentException ("Handle is not initialized.", "type");

			RunClassConstructor (type.Value);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern bool SufficientExecutionStack ();

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public static void EnsureSufficientExecutionStack ()
		{
			if (SufficientExecutionStack ())
				return;
			throw new InsufficientExecutionStackException ();
		}

		public static bool TryEnsureSufficientExecutionStack ()
		{
			return SufficientExecutionStack ();
		}

//		[MonoTODO("Currently a no-op")]
		public static void ExecuteCodeWithGuaranteedCleanup (TryCode code, CleanupCode backoutCode, Object userData)
		{
		}

//		[MonoTODO("Currently a no-op")]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.MayFail)]
		public static void PrepareConstrainedRegions ()
		{
		}

//		[MonoTODO("Currently a no-op")]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.MayFail)]
		public static void PrepareConstrainedRegionsNoOP ()
		{
		}

//		[MonoTODO("Currently a no-op")]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.MayFail)]
		public static void ProbeForSufficientStack()
		{
		}

		// This method triggers a given delegate to be prepared.  This involves preparing the
		// delegate's Invoke method and preparing the target of that Invoke.  In the case of
		// a multi-cast delegate, we rely on the fact that each individual component was prepared
		// prior to the Combine.  In other words, this service does not navigate through the
		// entire multicasting list.
		// If our own reliable event sinks perform the Combine (for example AppDomain.DomainUnload),
		// then the result is fully prepared.  But if a client calls Combine himself and then
		// then adds that combination to e.g. AppDomain.DomainUnload, then the client is responsible
		// for his own preparation.
		[System.Security.SecurityCritical]  // auto-generated_required
//		[MonoTODO("Currently a no-op")]
		public static void PrepareDelegate (Delegate d)
		{
		}

		// extracted from ../../../../external/referencesource/mscorlib/system/runtime/compilerservices/runtimehelpers.cs
		//
		// See comment above for PrepareDelegate
		//
		// PrepareContractedDelegate weakens this a bit by only assuring that we prepare 
		// delegates which also have a ReliabilityContract. This is useful for services that
		// want to provide opt-in reliability, generally some random event sink providing
		// always reliable semantics to random event handlers that are likely to have not
		// been written with relability in mind is a lost cause anyway.
		//
		// NOTE: that for the NGen case you can sidestep the required ReliabilityContract
		// by using the [PrePrepareMethod] attribute.
		[System.Security.SecurityCritical]  // auto-generated_required
//		[MonoTODO("Currently a no-op")]
		public static void PrepareContractedDelegate(Delegate d)
		{
		}

//		[MonoTODO("Currently a no-op")]
		public static void PrepareMethod (RuntimeMethodHandle method)
		{
		}

//		[MonoTODO("Currently a no-op")]
		public static void PrepareMethod (RuntimeMethodHandle method, RuntimeTypeHandle[] instantiation)
		{
		}

		public static void RunModuleConstructor (ModuleHandle module)
		{
			if (module == ModuleHandle.EmptyHandle)
				throw new ArgumentException ("Handle is not initialized.", "module");

			RunModuleConstructor (module.Value);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void RunModuleConstructor (IntPtr module);

		// This is implemented as a JIT intrinsic
		public static bool IsReferenceOrContainsReferences<T>()
		{
			return !typeof (T).IsValueType || RuntimeTypeHandle.HasReferences ((typeof (T) as RuntimeType));
		}

#if !NETCORE
		public static object GetUninitializedObject (Type type)
		{
			return FormatterServices.GetUninitializedObject (type);
		}
#endif

        /// <summary>
        /// GetSubArray helper method for the compiler to slice an array using a range.
        /// </summary>
        public static T[] GetSubArray<T> (T[] array, Range range)
        {
            Type elementType = array.GetType ().GetElementType ();
            Span<T> source = array.AsSpan (range);

            if (elementType.IsValueType)
            {
                return source.ToArray ();
            }
            else
            {
                T[] newArray = (T[]) Array.CreateInstance (elementType, source.Length);
                source.CopyTo (newArray);
                return newArray;
            }
        }
	}
}
