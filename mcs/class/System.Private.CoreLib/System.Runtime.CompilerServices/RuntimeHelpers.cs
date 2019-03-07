namespace System.Runtime.CompilerServices
{
	public static class RuntimeHelpers
	{
		public delegate void TryCode (Object userData);

		public delegate void CleanupCode (Object userData, bool exceptionThrown);

		public static void InitializeArray (Array array, RuntimeFieldHandle fldHandle)
		{
			if (array == null || fldHandle.Value == IntPtr.Zero)
				throw new ArgumentNullException ();

			InitializeArray (array, fldHandle.Value);
		}

		public static extern int OffsetToStringData {
			[MethodImpl (MethodImplOptions.InternalCall)]
			get;
		}

		public static int GetHashCode (object o)
		{
			return Object.InternalGetHashCode (o);
		}

		public static new bool Equals (object o1, object o2)
		{
			if (o1 == o2)
				return true;

			if (o1 == null || o2 == null)
				return false;

			if (o1 is ValueType)
				return ValueType.DefaultEquals (o1, o2);

			return false;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern object GetObjectValue (object obj);

		public static void RunClassConstructor (RuntimeTypeHandle type)
		{
			if (type.Value == IntPtr.Zero)
				throw new ArgumentException ("Handle is not initialized.", "type");

			RunClassConstructor (type.Value);
		}

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

		public static void ExecuteCodeWithGuaranteedCleanup (TryCode code, CleanupCode backoutCode, Object userData)
		{
		}

		public static void PrepareConstrainedRegions ()
		{
		}

		public static void PrepareConstrainedRegionsNoOP ()
		{
		}

		public static void ProbeForSufficientStack()
		{
		}

		public static void PrepareDelegate (Delegate d)
		{
		}

		public static void PrepareContractedDelegate(Delegate d)
		{
		}

		public static void PrepareMethod (RuntimeMethodHandle method)
		{
		}

		public static void PrepareMethod (RuntimeMethodHandle method, RuntimeTypeHandle[] instantiation)
		{
		}

		public static void RunModuleConstructor (ModuleHandle module)
		{
			if (module == ModuleHandle.EmptyHandle)
				throw new ArgumentException ("Handle is not initialized.", "module");

			RunModuleConstructor (module.Value);
		}

		[Intrinsic]
		public static bool IsReferenceOrContainsReferences<T>()
		{
			return !typeof (T).IsValueType || RuntimeTypeHandle.HasReferences ((typeof (T) as RuntimeType));
		}

		[Intrinsic]
		internal static unsafe bool ObjectHasComponentSize (object obj)
		{
			throw new NotImplementedException ();
		}

		public static object GetUninitializedObject (Type type)
		{
			throw new NotImplementedException ();
		}

		public static T[] GetSubArray<T> (T[] array, Range range)
		{
			Type elementType = array.GetType().GetElementType();
			Span<T> source = array.AsSpan(range);

			if (elementType.IsValueType)
				return source.ToArray();

			T[] newArray = (T[])Array.CreateInstance(elementType, source.Length);
			source.CopyTo(newArray);
			return newArray;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void InitializeArray (Array array, IntPtr fldHandle);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern void RunClassConstructor (IntPtr type);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void RunModuleConstructor (IntPtr module);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern bool SufficientExecutionStack ();
	}
}