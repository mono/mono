//
// System.Drawing.ComIStreamMarshaler.cs
//
// Author: Kornél Pál <http://www.kornelpal.hu/>
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
// Mono does not support native to managed wrappers
// #define RECURSIVE_WRAPPING

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Drawing
{
	// Mono does not implement COM interface marshaling
	// This custom marshaler should be replaced with UnmanagedType.Interface
	// Provides exact behaviour under Mono and .NET Framework
	internal class ComIStreamMarshaler : ICustomMarshaler
	{
		private const int S_OK = 0x00000000;
		private const int E_NOINTERFACE = unchecked((int)0x80004002);

		private delegate int QueryInterfaceDelegate(IntPtr @this, [In()] ref Guid riid, IntPtr ppvObject);
		private delegate int AddRefDelegate(IntPtr @this);
		private delegate int ReleaseDelegate(IntPtr @this);
		// Mono does not implement OutAttribute() with UnmanagedType.LPArray
		private delegate int ReadDelegate(IntPtr @this, IntPtr pv, int cb, IntPtr pcbRead);
		private delegate int WriteDelegate(IntPtr @this, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pv, int cb, IntPtr pcbWritten);
		private delegate int SeekDelegate(IntPtr @this, long dlibMove, int dwOrigin, IntPtr plibNewPosition);
		private delegate int SetSizeDelegate(IntPtr @this, long libNewSize);
		private delegate int CopyToDelegate(IntPtr @this, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(ComIStreamMarshaler))] UCOMIStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten);
		private delegate int CommitDelegate(IntPtr @this, int grfCommitFlags);
		private delegate int RevertDelegate(IntPtr @this);
		private delegate int LockRegionDelegate(IntPtr @this, long libOffset, long cb, int dwLockType);
		private delegate int UnlockRegionDelegate(IntPtr @this, long libOffset, long cb, int dwLockType);
		private delegate int StatDelegate(IntPtr @this, out STATSTG pstatstg, int grfStatFlag);
		// Mono does not implement custom marshaling for ref (and out) parameters
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
		protected sealed class ManagedToNativeWrapper
		{
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
			private static readonly IStreamVtbl managedVtable;
			private static readonly IntPtr comVtable;
			private static readonly VtableDestructor vtableDestructor;

			private readonly UCOMIStream managedInterface;
			// Keeps delegates alive while they are marshaled
			private readonly IntPtr comInterface;
			// Keeps the object alive when it has no managed references
			private readonly GCHandle gcHandle;
			private int comRefCount;
			private int managedRefCount;

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

			private ManagedToNativeWrapper(UCOMIStream managedInterface, bool outParam)
			{
				IStreamInterface newInterface;

				if (outParam)
					comRefCount = 1;
				else
					managedRefCount = 1;

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

			internal static IntPtr CreateInterface(UCOMIStream managedInterface, bool outParam)
			{
				if (managedInterface == null)
					return IntPtr.Zero;
				else
					return new ManagedToNativeWrapper(managedInterface, outParam).comInterface;
			}

			internal static void DisposeInterface(IntPtr @this)
			{
				if (@this != IntPtr.Zero)
				{
					ManagedToNativeWrapper thisObject = GetObject(@this);

					lock (thisObject)
					{
						if (thisObject.managedRefCount != 0)
						{
							thisObject.managedRefCount = 0;
							if (thisObject.comRefCount == 0)
								thisObject.Dispose();
						}
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
						return ++thisObject.comRefCount + thisObject.managedRefCount;
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
						if ((thisObject.comRefCount != 0) && (--thisObject.comRefCount == 0) && (thisObject.managedRefCount == 0))
							thisObject.Dispose();

						return thisObject.comRefCount + thisObject.managedRefCount;
					}
#if MAP_EX_TO_HR
				}
				catch
				{
					return 0;
				}
#endif
			}

			private static int Read(IntPtr @this, IntPtr pv, int cb, IntPtr pcbRead)
			{
#if MAP_EX_TO_HR
				try
				{
#endif
					byte[] buffer = new byte[cb];

					GetObject(@this).managedInterface.Read(buffer, cb, pcbRead);
					Marshal.Copy(buffer, 0, pv, cb);

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

			private static int CopyTo(IntPtr @this, UCOMIStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
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
					UCOMIStream newInterface;
					IntPtr newWrapper;

					ppstm = IntPtr.Zero;

					GetObject(@this).managedInterface.Clone(out newInterface);

					newWrapper = ManagedToNativeWrapper.CreateInterface(newInterface, true);
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
		protected sealed class NativeToManagedWrapper : UCOMIStream
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

			internal static UCOMIStream CreateInterface(IntPtr comInterface, bool outParam)
			{
				if (comInterface == IntPtr.Zero)
					return null;
				else
					return (UCOMIStream)new NativeToManagedWrapper(comInterface, outParam);
			}

			internal static void DisposeInterface(UCOMIStream managedInterface)
			{
				if (managedInterface != null)
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
				IntPtr buffer = Marshal.AllocHGlobal(cb);

				try
				{
					CheckHResult(managedVtable.Read(comInterface, buffer, cb, pcbRead));
					Marshal.Copy(buffer, pv, 0, cb);
				}
				finally
				{
					Marshal.FreeHGlobal(buffer);
				}
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

			public void CopyTo(UCOMIStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
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

			public void Clone(out UCOMIStream ppstm)
			{
				IntPtr newInterface;

				CheckHResult(managedVtable.Clone(comInterface, out newInterface));
				ppstm = NativeToManagedWrapper.CreateInterface(newInterface, true);
			}
		}

		private static readonly ComIStreamMarshaler defaultInstance = new ComIStreamMarshaler();
		
		protected ComIStreamMarshaler()
		{
		}

		private static ICustomMarshaler GetInstance(string cookie)
		{
			return defaultInstance;
		}

		// Mono calls this for null objects as well
		public virtual IntPtr MarshalManagedToNative(object managedObj)
		{
#if RECURSIVE_WRAPPING
			managedObj = NativeToManagedWrapper.CreateInterface(ManagedToNativeWrapper.CreateInterface((UCOMIStream)managedObj, true), true);
#endif
			return ManagedToNativeWrapper.CreateInterface((UCOMIStream)managedObj, false);
		}

		// Mono calls this for IntPtr.Zero objects as well
		public virtual void CleanUpNativeData(IntPtr pNativeData)
		{
			ManagedToNativeWrapper.DisposeInterface(pNativeData);
		}

		// Mono calls this for IntPtr.Zero objects as well
		// Mono does not implement function pointer to Delegate marshaling
		// This cannot be used on Mono
		public virtual object MarshalNativeToManaged(IntPtr pNativeData)
		{
#if RECURSIVE_WRAPPING
			pNativeData = ManagedToNativeWrapper.CreateInterface(NativeToManagedWrapper.CreateInterface(pNativeData, true), true);
#endif
			return NativeToManagedWrapper.CreateInterface(pNativeData, false);
		}

		// Mono calls this for null objects as well
		public virtual void CleanUpManagedData(object managedObj)
		{
			NativeToManagedWrapper.DisposeInterface((UCOMIStream)managedObj);
		}

		public virtual int GetNativeDataSize()
		{
			return -1;
		}
	}

	// Mono does not support custom marshalers for ref (and out) parameters
	internal sealed class ComIStreamOutMarshaler : ComIStreamMarshaler
	{
		private static readonly ComIStreamMarshaler defaultInstance = new ComIStreamOutMarshaler();
		
		private ComIStreamOutMarshaler()
		{
		}

		private static ICustomMarshaler GetInstance(string cookie)
		{
			return defaultInstance;
		}

		// Mono calls this for null objects as well
		public override IntPtr MarshalManagedToNative(object managedObj)
		{
#if RECURSIVE_WRAPPING
			managedObj = NativeToManagedWrapper.CreateInterface(ManagedToNativeWrapper.CreateInterface((UCOMIStream)managedObj, true), true);
#endif
			return ManagedToNativeWrapper.CreateInterface((UCOMIStream)managedObj, true);
		}

		// Mono calls this for IntPtr.Zero objects as well
		public override void CleanUpNativeData(IntPtr pNativeData)
		{
		}

		// Mono calls this for IntPtr.Zero objects as well
		// Mono does not implement function pointer to Delegate marshaling
		// This cannot be used on Mono
		public override object MarshalNativeToManaged(IntPtr pNativeData)
		{
#if RECURSIVE_WRAPPING
			pNativeData = ManagedToNativeWrapper.CreateInterface(NativeToManagedWrapper.CreateInterface(pNativeData, true), true);
#endif
			return NativeToManagedWrapper.CreateInterface(pNativeData, true);
		}

		// Mono calls this for null objects as well
		public override void CleanUpManagedData(object managedObj)
		{
			NativeToManagedWrapper.DisposeInterface((UCOMIStream)managedObj);
		}

		public override int GetNativeDataSize()
		{
			return -1;
		}
	}
}