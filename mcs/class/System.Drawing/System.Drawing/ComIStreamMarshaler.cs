//
// System.Drawing.ComIStreamMarshaler.cs
//
// Author:
//   Kornél Pál <http://www.kornelpal.hu/>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

// Undefine to debug the protected blocks
#define MAP_EX_TO_HR

// Define to debug wrappers recursively
// #define RECURSIVE_WRAPPING

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
#if NET_2_0
using System.Runtime.InteropServices.ComTypes;
using STATSTG = System.Runtime.InteropServices.ComTypes.STATSTG;
#endif

namespace System.Drawing
{
	// Mono does not implement COM interface marshaling
	// This custom marshaler should be replaced with UnmanagedType.Interface
	// Provides identical behaviour under Mono and .NET Framework
	internal sealed class ComIStreamMarshaler : ICustomMarshaler
	{
		private const int S_OK = 0x00000000;
		private const int E_NOINTERFACE = unchecked((int)0x80004002);

		private delegate int QueryInterfaceDelegate(IntPtr @this, [In()] ref Guid riid, IntPtr ppvObject);
		private delegate int AddRefDelegate(IntPtr @this);
		private delegate int ReleaseDelegate(IntPtr @this);
		private delegate int ReadDelegate(IntPtr @this, [Out(), MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pv, int cb, IntPtr pcbRead);
		private delegate int WriteDelegate(IntPtr @this, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pv, int cb, IntPtr pcbWritten);
		private delegate int SeekDelegate(IntPtr @this, long dlibMove, int dwOrigin, IntPtr plibNewPosition);
		private delegate int SetSizeDelegate(IntPtr @this, long libNewSize);
		private delegate int CopyToDelegate(IntPtr @this, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(ComIStreamMarshaler))]
#if NET_2_0
			IStream
#else
			UCOMIStream
#endif
			pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten);
		private delegate int CommitDelegate(IntPtr @this, int grfCommitFlags);
		private delegate int RevertDelegate(IntPtr @this);
		private delegate int LockRegionDelegate(IntPtr @this, long libOffset, long cb, int dwLockType);
		private delegate int UnlockRegionDelegate(IntPtr @this, long libOffset, long cb, int dwLockType);
		private delegate int StatDelegate(IntPtr @this, out STATSTG pstatstg, int grfStatFlag);
		private delegate int CloneDelegate(IntPtr @this, out IntPtr ppstm);

		[StructLayout(LayoutKind.Sequential)]
		private sealed class IStreamInterface
		{
			internal IntPtr lpVtbl;
			internal IntPtr gcHandle;
		}

		[StructLayout(LayoutKind.Sequential)]
		private sealed class IStreamVtbl
		{
			internal QueryInterfaceDelegate QueryInterface;
			internal AddRefDelegate AddRef;
			internal ReleaseDelegate Release;
			internal ReadDelegate Read;
			internal WriteDelegate Write;
			internal SeekDelegate Seek;
			internal SetSizeDelegate SetSize;
			internal CopyToDelegate CopyTo;
			internal CommitDelegate Commit;
			internal RevertDelegate Revert;
			internal LockRegionDelegate LockRegion;
			internal UnlockRegionDelegate UnlockRegion;
			internal StatDelegate Stat;
			internal CloneDelegate Clone;
		}

		// Managed COM Callable Wrapper implementation
		// Reference counting is thread safe
		private sealed class ManagedToNativeWrapper
		{
			// Mono does not implement Marshal.Release
			[StructLayout(LayoutKind.Sequential)]
			private sealed class ReleaseSlot
			{
				internal ReleaseDelegate Release;
			}

			private sealed class VtableDestructor
			{
				~VtableDestructor()
				{
					Marshal.DestroyStructure(comVtable, typeof(IStreamVtbl));
					Marshal.FreeHGlobal(comVtable);
				}
			}

			private static readonly Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");
			private static readonly Guid IID_IStream = new Guid("0000000C-0000-0000-C000-000000000046");
			private static readonly MethodInfo exceptionGetHResult = typeof(Exception).GetProperty("HResult", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.ExactBinding, null, typeof(int), new Type[] {}, null).GetGetMethod(true);
			// Keeps delegates alive while they are marshaled
			private static readonly IStreamVtbl managedVtable;
			private static readonly IntPtr comVtable;
			private static readonly VtableDestructor vtableDestructor;

			private readonly
#if NET_2_0
				IStream
#else
				UCOMIStream
#endif
				managedInterface;
			private readonly IntPtr comInterface;
			// Keeps the object alive when it has no managed references
			private readonly GCHandle gcHandle;
			private int refCount = 1;

			static ManagedToNativeWrapper()
			{
				IStreamVtbl newVtable;

				newVtable = new IStreamVtbl();
				newVtable.QueryInterface = new QueryInterfaceDelegate(QueryInterface);
				newVtable.AddRef = new AddRefDelegate(AddRef);
				newVtable.Release = new ReleaseDelegate(Release);
				newVtable.Read = new ReadDelegate(Read);
				newVtable.Write = new WriteDelegate(Write);
				newVtable.Seek = new SeekDelegate(Seek);
				newVtable.SetSize = new SetSizeDelegate(SetSize);
				newVtable.CopyTo = new CopyToDelegate(CopyTo);
				newVtable.Commit = new CommitDelegate(Commit);
				newVtable.Revert = new RevertDelegate(Revert);
				newVtable.LockRegion = new LockRegionDelegate(LockRegion);
				newVtable.UnlockRegion = new UnlockRegionDelegate(UnlockRegion);
				newVtable.Stat = new StatDelegate(Stat);
				newVtable.Clone = new CloneDelegate(Clone);
				comVtable = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IStreamVtbl)));
				Marshal.StructureToPtr(newVtable, comVtable, false);
				managedVtable = newVtable;

				vtableDestructor = new VtableDestructor();
			}

			private ManagedToNativeWrapper(
#if NET_2_0
				IStream
#else
				UCOMIStream
#endif
				managedInterface)
			{
				IStreamInterface newInterface;

				this.managedInterface = managedInterface;
				gcHandle = GCHandle.Alloc(this);

				newInterface = new IStreamInterface();
				newInterface.lpVtbl = comVtable;
				newInterface.gcHandle = (IntPtr)gcHandle;
				comInterface = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IStreamInterface)));
				Marshal.StructureToPtr(newInterface, comInterface, false);
			}

			~ManagedToNativeWrapper()
			{
				Dispose();
			}

			private void Dispose()
			{
				Marshal.FreeHGlobal(comInterface);
				gcHandle.Free();
				GC.SuppressFinalize(this);
			}

			internal static
#if NET_2_0
				IStream
#else
				UCOMIStream
#endif
				GetUnderlyingInterface(IntPtr comInterface, bool outParam)
			{
				if (Marshal.ReadIntPtr(comInterface) == comVtable)
				{

#if NET_2_0
					IStream
#else
					UCOMIStream
#endif
						managedInterface = GetObject(comInterface).managedInterface;

					if (outParam)
						Release(comInterface);

					return managedInterface;
				}
				else
					return null;
			}

			internal static IntPtr CreateInterface(
#if NET_2_0
				IStream
#else
				UCOMIStream
#endif
				managedInterface)
			{
				IntPtr comInterface;

				if (managedInterface == null)
					return IntPtr.Zero;
#if !RECURSIVE_WRAPPING
				else if ((comInterface = NativeToManagedWrapper.GetUnderlyingInterface(managedInterface)) == IntPtr.Zero)
#endif
					comInterface = new ManagedToNativeWrapper(managedInterface).comInterface;

				return comInterface;
			}

			internal static void DisposeInterface(IntPtr comInterface)
			{
				if (comInterface != IntPtr.Zero)
				{
					IntPtr vtable = Marshal.ReadIntPtr(comInterface);

					if (vtable == comVtable)
						ManagedToNativeWrapper.Release(comInterface);
					else
					{
						ReleaseSlot releaseSlot = (ReleaseSlot)Marshal.PtrToStructure((IntPtr)((long)vtable + (long)(IntPtr.Size * 2)), typeof(ReleaseSlot));
						releaseSlot.Release(comInterface);
					}
				}
			}

			// Mono does not implement Marshal.GetHRForException
			private static int GetHRForException(Exception e)
			{
				return (int)exceptionGetHResult.Invoke(e, null);
			}

			private static ManagedToNativeWrapper GetObject(IntPtr @this)
			{
				return (ManagedToNativeWrapper)((GCHandle)Marshal.ReadIntPtr(@this, IntPtr.Size)).Target;
			}

			private static int QueryInterface(IntPtr @this, ref Guid riid, IntPtr ppvObject)
			{
#if MAP_EX_TO_HR
				try
				{
#endif
					if (IID_IUnknown.Equals(riid) || IID_IStream.Equals(riid))
					{
						Marshal.WriteIntPtr(ppvObject, @this);
						AddRef(@this);
						return S_OK;
					}
					else
					{
						Marshal.WriteIntPtr(ppvObject, IntPtr.Zero);
						return E_NOINTERFACE;
					}
#if MAP_EX_TO_HR
				}
				catch (Exception e)
				{
					return GetHRForException(e);
				}
#endif
			}

			private static int AddRef(IntPtr @this)
			{
#if MAP_EX_TO_HR
				try
				{
#endif
					ManagedToNativeWrapper thisObject = GetObject(@this);

					lock (thisObject)
					{
						return ++thisObject.refCount;
					}
#if MAP_EX_TO_HR
				}
				catch
				{
					return 0;
				}
#endif
			}

			private static int Release(IntPtr @this)
			{
#if MAP_EX_TO_HR
				try
				{
#endif
					ManagedToNativeWrapper thisObject = GetObject(@this);

					lock (thisObject)
					{
						if ((thisObject.refCount != 0) && (--thisObject.refCount == 0))
							thisObject.Dispose();

						return thisObject.refCount;
					}
#if MAP_EX_TO_HR
				}
				catch
				{
					return 0;
				}
#endif
			}

			private static int Read(IntPtr @this, byte[] pv, int cb, IntPtr pcbRead)
			{
#if MAP_EX_TO_HR
				try
				{
#endif
					GetObject(@this).managedInterface.Read(pv, cb, pcbRead);
					return S_OK;
#if MAP_EX_TO_HR
				}
				catch (Exception e)
				{
					return GetHRForException(e);
				}
#endif
			}

			private static int Write(IntPtr @this, byte[] pv, int cb, IntPtr pcbWritten)
			{
#if MAP_EX_TO_HR
				try
				{
#endif
					GetObject(@this).managedInterface.Write(pv, cb, pcbWritten);
					return S_OK;
#if MAP_EX_TO_HR
				}
				catch (Exception e)
				{
					return GetHRForException(e);
				}
#endif
			}

			private static int Seek(IntPtr @this, long dlibMove, int dwOrigin, IntPtr plibNewPosition)
			{
#if MAP_EX_TO_HR
				try
				{
#endif
					GetObject(@this).managedInterface.Seek(dlibMove, dwOrigin, plibNewPosition);
					return S_OK;
#if MAP_EX_TO_HR
				}
				catch (Exception e)
				{
					return GetHRForException(e);
				}
#endif
			}

			private static int SetSize(IntPtr @this, long libNewSize)
			{
#if MAP_EX_TO_HR
				try
				{
#endif
					GetObject(@this).managedInterface.SetSize(libNewSize);
					return S_OK;
#if MAP_EX_TO_HR
				}
				catch (Exception e)
				{
					return GetHRForException(e);
				}
#endif
			}

			private static int CopyTo(IntPtr @this,
#if NET_2_0
				IStream
#else
				UCOMIStream
#endif
				pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
			{
#if MAP_EX_TO_HR
				try
				{
#endif
					GetObject(@this).managedInterface.CopyTo(pstm, cb, pcbRead, pcbWritten);
					return S_OK;
#if MAP_EX_TO_HR
				}
				catch (Exception e)
				{
					return GetHRForException(e);
				}
#endif
			}

			private static int Commit(IntPtr @this, int grfCommitFlags)
			{
#if MAP_EX_TO_HR
				try
				{
#endif
					GetObject(@this).managedInterface.Commit(grfCommitFlags);
					return S_OK;
#if MAP_EX_TO_HR
				}
				catch (Exception e)
				{
					return GetHRForException(e);
				}
#endif
			}

			private static int Revert(IntPtr @this)
			{
#if MAP_EX_TO_HR
				try
				{
#endif
					GetObject(@this).managedInterface.Revert();
					return S_OK;
#if MAP_EX_TO_HR
				}
				catch (Exception e)
				{
					return GetHRForException(e);
				}
#endif
			}

			private static int LockRegion(IntPtr @this, long libOffset, long cb, int dwLockType)
			{
#if MAP_EX_TO_HR
				try
				{
#endif
					GetObject(@this).managedInterface.LockRegion(libOffset, cb, dwLockType);
					return S_OK;
#if MAP_EX_TO_HR
				}
				catch (Exception e)
				{
					return GetHRForException(e);
				}
#endif
			}

			private static int UnlockRegion(IntPtr @this, long libOffset, long cb, int dwLockType)
			{
#if MAP_EX_TO_HR
				try
				{
#endif
					GetObject(@this).managedInterface.UnlockRegion(libOffset, cb, dwLockType);
					return S_OK;
#if MAP_EX_TO_HR
				}
				catch (Exception e)
				{
					return GetHRForException(e);
				}
#endif
			}

			private static int Stat(IntPtr @this, out STATSTG pstatstg, int grfStatFlag)
			{
#if MAP_EX_TO_HR
				try
				{
#endif
					GetObject(@this).managedInterface.Stat(out pstatstg, grfStatFlag);
					return S_OK;
#if MAP_EX_TO_HR
				}
				catch (Exception e)
				{
					pstatstg = new STATSTG();
					return GetHRForException(e);
				}
#endif
			}

			private static int Clone(IntPtr @this, out IntPtr ppstm)
			{
#if MAP_EX_TO_HR
				try
				{
#endif
#if NET_2_0
					IStream
#else
					UCOMIStream
#endif
						newInterface;
					IntPtr newWrapper;

					ppstm = IntPtr.Zero;

					GetObject(@this).managedInterface.Clone(out newInterface);

					newWrapper = ManagedToNativeWrapper.CreateInterface(newInterface);
					ppstm = newWrapper;
					return S_OK;
#if MAP_EX_TO_HR
				}
				catch (Exception e)
				{
					ppstm = IntPtr.Zero;
					return GetHRForException(e);
				}
#endif
			}
		}

		// Managed Runtime Callable Wrapper implementation
		private sealed class NativeToManagedWrapper :
#if NET_2_0
			IStream
#else
			UCOMIStream
#endif
		{
			private readonly IntPtr comInterface;
			private readonly IStreamVtbl managedVtable;

			private NativeToManagedWrapper(IntPtr comInterface, bool outParam)
			{
				IntPtr comVtable = Marshal.ReadIntPtr(comInterface);

				this.comInterface = comInterface;
				managedVtable = (IStreamVtbl)Marshal.PtrToStructure(comVtable, typeof(IStreamVtbl));
				if (!outParam)
					managedVtable.AddRef(comInterface);
			}

			~NativeToManagedWrapper()
			{
				Dispose();
			}

			private void Dispose()
			{
				managedVtable.Release(comInterface);
				GC.SuppressFinalize(this);
			}

			internal static IntPtr GetUnderlyingInterface(
#if NET_2_0
				IStream
#else
				UCOMIStream
#endif
				managedInterface)
			{
				if (managedInterface is NativeToManagedWrapper)
				{
					NativeToManagedWrapper wrapper = (NativeToManagedWrapper)managedInterface;

					wrapper.managedVtable.AddRef(wrapper.comInterface);
					return wrapper.comInterface;
				}
				else
					return IntPtr.Zero;
			}

			internal static
#if NET_2_0
				IStream
#else
				UCOMIStream
#endif
				CreateInterface(IntPtr comInterface, bool outParam)
			{
#if NET_2_0
				IStream
#else
				UCOMIStream
#endif
					managedInterface;

				if (comInterface == IntPtr.Zero)
					return null;
#if !RECURSIVE_WRAPPING
				else if ((managedInterface = ManagedToNativeWrapper.GetUnderlyingInterface(comInterface, outParam)) == null)
#endif
					managedInterface = (
#if NET_2_0
						IStream
#else
						UCOMIStream
#endif
						)new NativeToManagedWrapper(comInterface, outParam);

				return managedInterface;
			}

			internal static void DisposeInterface(
#if NET_2_0
				IStream
#else
				UCOMIStream
#endif
				managedInterface)
			{
				if (managedInterface != null && managedInterface is NativeToManagedWrapper)
					((NativeToManagedWrapper)managedInterface).Dispose();
			}

			// Mono does not implement Marshal.ThrowExceptionForHR
			private static void CheckHResult(int result)
			{
				if (result != S_OK)
					throw new COMException(null, result);
			}

			public void Read(byte[] pv, int cb, IntPtr pcbRead)
			{
				CheckHResult(managedVtable.Read(comInterface, pv, cb, pcbRead));
			}

			public void Write(byte[] pv, int cb, IntPtr pcbWritten)
			{
				CheckHResult(managedVtable.Write(comInterface, pv, cb, pcbWritten));
			}

			public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
			{
				CheckHResult(managedVtable.Seek(comInterface, dlibMove, dwOrigin, plibNewPosition));
			}

			public void SetSize(long libNewSize)
			{
				CheckHResult(managedVtable.SetSize(comInterface, libNewSize));
			}

			public void CopyTo(
#if NET_2_0
				IStream
#else
				UCOMIStream
#endif
				pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
			{
				CheckHResult(managedVtable.CopyTo(comInterface, pstm, cb, pcbRead, pcbWritten));
			}

			public void Commit(int grfCommitFlags)
			{
				CheckHResult(managedVtable.Commit(comInterface, grfCommitFlags));
			}

			public void Revert()
			{
				CheckHResult(managedVtable.Revert(comInterface));
			}

			public void LockRegion(long libOffset, long cb, int dwLockType)
			{
				CheckHResult(managedVtable.LockRegion(comInterface, libOffset, cb, dwLockType));
			}

			public void UnlockRegion(long libOffset, long cb, int dwLockType)
			{
				CheckHResult(managedVtable.UnlockRegion(comInterface, libOffset, cb, dwLockType));
			}

			public void Stat(out STATSTG pstatstg, int grfStatFlag)
			{
				CheckHResult(managedVtable.Stat(comInterface, out pstatstg, grfStatFlag));
			}

			public void Clone(out
#if NET_2_0
				IStream
#else
				UCOMIStream
#endif
				ppstm)
			{
				IntPtr newInterface;

				CheckHResult(managedVtable.Clone(comInterface, out newInterface));
				ppstm = NativeToManagedWrapper.CreateInterface(newInterface, true);
			}
		}

		private static readonly ComIStreamMarshaler defaultInstance = new ComIStreamMarshaler();

		private ComIStreamMarshaler()
		{
		}

		private static ICustomMarshaler GetInstance(string cookie)
		{
			return defaultInstance;
		}

		public IntPtr MarshalManagedToNative(object managedObj)
		{
#if RECURSIVE_WRAPPING
			managedObj = NativeToManagedWrapper.CreateInterface(ManagedToNativeWrapper.CreateInterface((
#if NET_2_0
				IStream
#else
				UCOMIStream
#endif
				)managedObj), true);
#endif
			return ManagedToNativeWrapper.CreateInterface((
#if NET_2_0
				IStream
#else
				UCOMIStream
#endif
				)managedObj);
		}

		public void CleanUpNativeData(IntPtr pNativeData)
		{
			ManagedToNativeWrapper.DisposeInterface(pNativeData);
		}

		public object MarshalNativeToManaged(IntPtr pNativeData)
		{
#if RECURSIVE_WRAPPING
			pNativeData = ManagedToNativeWrapper.CreateInterface(NativeToManagedWrapper.CreateInterface(pNativeData, true));
#endif
			return NativeToManagedWrapper.CreateInterface(pNativeData, false);
		}

		public void CleanUpManagedData(object managedObj)
		{
			NativeToManagedWrapper.DisposeInterface((
#if NET_2_0
				IStream
#else
				UCOMIStream
#endif
				)managedObj);
		}

		public int GetNativeDataSize()
		{
			return -1;
		}
	}
}