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
		static GCHandle handle;

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

		[Test]
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
	}

}

