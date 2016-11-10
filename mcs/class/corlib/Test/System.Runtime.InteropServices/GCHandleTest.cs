//
// System.Runtime.InteropServices.GCHandle Test Cases
//
// Authors:
// 	Paolo Molaro (lupus@ximian.com)
//
// Copyright (C) 2005, 2009 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Globalization;

namespace MonoTests.System.Runtime.InteropServices
{
	[TestFixture]
	public class GCHandleTest
	{
		// Expected warning, the tests that reference this handle are testing for the default values of the object
		#pragma warning disable 649
		static GCHandle handle;
		#pragma warning restore 649
		
		[Test]
		public void DefaultZeroValue_Allocated ()
		{
			Assert.IsFalse (handle.IsAllocated, "IsAllocated");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void DefaultZeroValue_Target ()
		{
			Assert.IsNull (handle.Target, "Target");
		}

		[Test]
		public void AllocNull ()
		{
			IntPtr ptr = (IntPtr) GCHandle.Alloc (null);
			Assert.IsFalse (ptr == IntPtr.Zero, "ptr");
			GCHandle gch = (GCHandle) ptr;
			Assert.IsTrue (gch.IsAllocated, "IsAllocated");
			Assert.IsNull (gch.Target, "Target");
		}

		[Test]
		public void AllocNullWeakTrack ()
		{
			GCHandle gch = GCHandle.Alloc (null, GCHandleType.WeakTrackResurrection);
			Assert.IsTrue (gch.IsAllocated, "IsAllocated");
			Assert.IsNull (gch.Target, "Target");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddrOfPinnedObjectNormal ()
		{
			GCHandle handle = GCHandle.Alloc (new Object (), GCHandleType.Normal);
			try {
				IntPtr ptr = handle.AddrOfPinnedObject();
			}
			finally {
				handle.Free();
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddrOfPinnedObjectWeak ()
		{
			GCHandle handle = GCHandle.Alloc (new Object (), GCHandleType.Weak);
			try {
				IntPtr ptr = handle.AddrOfPinnedObject();
			}
			finally {
				handle.Free();
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddrOfPinnedObjectWeakTrackResurrection ()
		{
			GCHandle handle = GCHandle.Alloc (new Object (), GCHandleType.WeakTrackResurrection);
			try {
				IntPtr ptr = handle.AddrOfPinnedObject();
			}
			finally {
				handle.Free();
			}
		}

		[Test]
		public void AddrOfPinnedObjectNull ()
		{
			GCHandle handle = GCHandle.Alloc (null, GCHandleType.Pinned);
			try {
				IntPtr ptr = handle.AddrOfPinnedObject();
				Assert.AreEqual (new IntPtr (0), ptr);
			}
			finally {
				handle.Free();
			}
		}

		[Test]
		[Ignore ("throw non-catchable ExecutionEngineException")]
		[ExpectedException (typeof (ExecutionEngineException))]
		public void AllocMinusOne ()
		{
			// -1 is a special value used by the mono runtime
			// looks like it's special too in MS CLR (since it will crash)
			GCHandle.Alloc (null, (GCHandleType) (-1));
		}

		[Test]
		public void AllocInvalidType ()
		{
			GCHandle gch = GCHandle.Alloc (null, (GCHandleType) Int32.MinValue);
			try {
				Assert.IsTrue (gch.IsAllocated, "IsAllocated");
				Assert.IsNull (gch.Target, "Target");
			}
			finally {
				gch.Free ();
			}
		}
#if !MONOTOUCH && !FULL_AOT_RUNTIME
		[Test]
		[Category("MobileNotWorking")] // SIGSEGV, probably on AppDomain.Unload
		public void WeakHandleWorksOnNonRootDomain ()
		{
			//Console.WriteLine("current app domain: " + AppDomain.CurrentDomain.Id);
			AppDomain domain = AppDomain.CreateDomain("testdomain");

			Assembly ea = Assembly.GetExecutingAssembly ();
			domain.CreateInstanceFrom (ea.CodeBase,
				typeof (AssemblyResolveHandler).FullName,
				false,
				BindingFlags.Public | BindingFlags.Instance,
				null,
				new object [] { ea.Location, ea.FullName },
				CultureInfo.InvariantCulture,
				null,
				null);


			var testerType = typeof (CrossDomainGCHandleRunner);
			var r = (CrossDomainGCHandleRunner)domain.CreateInstanceAndUnwrap (
				testerType.Assembly.FullName, testerType.FullName, false,
				BindingFlags.Public | BindingFlags.Instance, null, new object [0],
				CultureInfo.InvariantCulture, new object [0], null);


			Assert.IsTrue (r.RunTest (), "#1");
			AppDomain.Unload (domain);
		}

		public class CrossDomainGCHandleRunner : MarshalByRefObject {
			public bool RunTest () {
				object o = new object();
				GCHandle gcHandle = GCHandle.Alloc (o, GCHandleType.Weak);
				IntPtr intPtr = (IntPtr)gcHandle;
				
				try {
					object target = GCHandle.FromIntPtr(intPtr).Target;
					return true;
				} catch (Exception) {}
				return false;
			}
		}
		
		[Serializable ()]
		class AssemblyResolveHandler
		{
			public AssemblyResolveHandler (string assemblyFile, string assemblyName)
			{
				_assemblyFile = assemblyFile;
				_assemblyName = assemblyName;

				AppDomain.CurrentDomain.AssemblyResolve +=
					new ResolveEventHandler (ResolveAssembly);
			}

			private Assembly ResolveAssembly (object sender, ResolveEventArgs args)
			{
				if (args.Name == _assemblyName)
					return Assembly.LoadFrom (_assemblyFile);

				return null;
			}

			private readonly string _assemblyFile;
			private readonly string _assemblyName;
		}
#endif

		class AnyClass
		{
		}

		struct StructWithReferenceTypeInside
		{
			public string myStr;
		}

		struct GenericStruct<T>
		{
			public T myItem;
		}

		class GenericClass<T>
		{
			public T myItem;
		}

		struct StructWithIntInside
		{
			public int myInt;
		}

		[StructLayout (LayoutKind.Explicit)]
		struct StructWithIntInsideExplicitLayout
		{
			[FieldOffset (0)]
			public int myInt;
		}

		[StructLayout (LayoutKind.Auto)]
		struct StructWithIntInsideAutoLayout
		{
			public int myInt;
		}

		struct StructWithChar
		{
			public char value;
		}

		struct StructWithBool
		{
			public bool value;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CannotAllocAPinnedGCHandleToAnArrayOfStrings ()
		{
			var arrayOfStrings = new string[] { "a", "B" };
			GCHandle.Alloc (arrayOfStrings, GCHandleType.Pinned);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CannotAllocAPinnedGCHandleToAnArrayOfIntArrays ()
		{
			var arrayOfIntArrays = new int[][] { new int[] {1, 2}, new int[] {3, 4, 5} };
			GCHandle.Alloc (arrayOfIntArrays, GCHandleType.Pinned);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CannotAllocAPinnedGCHandleToAnObject ()
		{
			GCHandle.Alloc (new object (), GCHandleType.Pinned);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CannotAllocAPinnedGCHandleToAClass ()
		{
			GCHandle.Alloc (new AnyClass (), GCHandleType.Pinned);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CannotAllocAPinnedGCHandleToAGenericClass ()
		{
			GCHandle.Alloc (new GenericClass<int> (), GCHandleType.Pinned);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CannotAllocAPinnedGCHandleToANonBlittableStruct ()
		{
			var nonBlittableStruct = default (StructWithReferenceTypeInside);
			GCHandle.Alloc(nonBlittableStruct, GCHandleType.Pinned);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CannotAllocAPinnedGCHandleToAGenericStruct ()
		{
			var nonBlittableGenericStruct = default (GenericStruct<string>);
			GCHandle.Alloc (nonBlittableGenericStruct, GCHandleType.Pinned);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CannotAllocAPinnedGCHandleToADateTime ()
		{
			GCHandle.Alloc (default (DateTime), GCHandleType.Pinned);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CannotAllocAPinnedGCHandleToAnAutoLayoutStruct ()
		{
			GCHandle.Alloc (default (StructWithIntInsideAutoLayout), GCHandleType.Pinned);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CannotAllocAllocPinnedHandleForArrayOfStructWithChar ()
		{
			var array = new StructWithChar[]
			{
				new StructWithChar () { value = 'd' },
				new StructWithChar () { value = 'f' },
				new StructWithChar () { value = '2' },
				new StructWithChar () { value = '-' },
			};

			GCHandle.Alloc (array, GCHandleType.Pinned);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CannotAllocPinnedHandleForArrayOfStructWithBool ()
		{
			var array = new StructWithBool[]
			{
				new StructWithBool () { value = true },
				new StructWithBool () { value = false },
				new StructWithBool () { value = false },
				new StructWithBool () { value = true },
			};

			GCHandle.Alloc (array, GCHandleType.Pinned);
		}

		[Test]
		public void CanAllocPinnedHandleForNull ()
		{
			var gcHandle = GCHandle.Alloc (null, GCHandleType.Pinned);
			Assert.IsNotNull (gcHandle);
			gcHandle.Free ();
		}

		[Test]
		public void CanAllocPinnedHandleForArray ()
		{
			var array = new int[] { 1, 2, 3, 4567, 8416415 };
			var gcHandle = GCHandle.Alloc (array, GCHandleType.Pinned);
			Assert.IsNotNull (gcHandle);
			gcHandle.Free ();
		}

		[Test]
		public void CanAllocPinnedHandleForMultidimensionalArray ()
		{
			var multiDimensionalArray = new int[2, 3]
			{
				{ 2, 3, 4 },
				{ 2, 3, 5 }
			};

			var gcHandle = GCHandle.Alloc (multiDimensionalArray, GCHandleType.Pinned);
			Assert.IsNotNull (gcHandle);
			gcHandle.Free ();
		}

		[Test]
		public void CanAllocPinnedHandleForCharArray ()
		{
			var array = new char[] { 'a', 'b', '9', 'z' };
			var gcHandle = GCHandle.Alloc (array, GCHandleType.Pinned);
			Assert.IsNotNull (gcHandle);
			gcHandle.Free ();
		}

		[Test]
		public void CanAllocPinnedHandleForBoolArray ()
		{
			var array = new bool[] { false, true, true, false };
			var gcHandle = GCHandle.Alloc (array, GCHandleType.Pinned);
			Assert.IsNotNull (gcHandle);
			gcHandle.Free ();
		}

		[Test]
		public void CanAllocPinnedHandleForString ()
		{
			string myObj = "Hello, world!";
			var gcHandle = GCHandle.Alloc (myObj, GCHandleType.Pinned);
			Assert.IsNotNull (gcHandle);
			gcHandle.Free ();
		}

		[Test]
		public void CanAllocPinnedHandleForStruct ()
		{
			var obj = default (StructWithIntInside);
			obj.myInt = 489794165;

			var gcHandle = GCHandle.Alloc (obj, GCHandleType.Pinned);
			Assert.IsNotNull (gcHandle);
			gcHandle.Free ();
		}

		[Test]
		public void CanAllocPinnedHandleForStructWithExplicitLayout ()
		{
			var obj = default (StructWithIntInsideExplicitLayout);
			obj.myInt = 489794165;

			var gcHandle = GCHandle.Alloc (obj, GCHandleType.Pinned);
			Assert.IsNotNull (gcHandle);
			gcHandle.Free ();
		}

		[Test]
		public void CanAllocPinnedHandleForGenericStruct ()
		{
			var obj = default (GenericStruct<int>);
			obj.myItem = 489794165;

			var gcHandle = GCHandle.Alloc (obj, GCHandleType.Pinned);
			Assert.IsNotNull (gcHandle);
			gcHandle.Free ();
		}
	}
}

