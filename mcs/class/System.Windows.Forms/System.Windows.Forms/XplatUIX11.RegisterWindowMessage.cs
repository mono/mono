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
// Copyright (c) 2019 AxxonSoft.
//
// Authors:
//	Nikita Voronchev <nikita.voronchev@ru.axxonsoft.com>
//

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Mono.Unix;
using Mono.Unix.Native;

namespace System.Windows.Forms {
	internal partial class XplatUIX11
	{
		// Simplest realisation: list.
		// TODO: One may use balanced tree (for exapmle red-black tree or AVL tree) to speed up search.
		public class WindowMessagesStorage
		{
			public const uint MSG_RANGE_MIN = 0xC000;
			public const uint MSG_RANGE_MAX = 0xFFFF;

			private const string SHARED_MEMORY_NAME = "/_Mono_SWF_RegisterWindowMessage_WindowMessagesStore";

			public WindowMessagesStorage ()
			{
				using (var shm = new SharedMemory(SHARED_MEMORY_NAME))
				{
					if (shm.GetSize () == 0)
						InitializeSharedStorage (shm);
				}
			}

			public uint RegisterWindowMessage (string lpString)
			{
				if (string.IsNullOrEmpty(lpString))
					throw new ArgumentException ("Empty string passed as message name");

				using (var shm = new SharedMemory(SHARED_MEMORY_NAME)) {
					if ( GetItemsCountInStorage(shm) >= (MSG_RANGE_MAX - MSG_RANGE_MIN + 1))
						throw new Exception ("There is no free mesage id");

					bool found = SearchNode (shm, lpString, out StorageNode node);
					if (!found) {
						node = MakeNewStorageNode (shm, lpString);
						PlaceNewNodeInLinkedList (shm, node);
					}

					return node.MessageId;
				}
			}

			public static void Clear ()
			{
				using (var shm = new SharedMemory(SHARED_MEMORY_NAME)) {
					shm.Clear ();
				}
			}

			private void InitializeSharedStorage (SharedMemory shm)
			{
				var header = new StorageHeader {
					ItemsCount = 0,
					LastNodeOffset = -1
				};
				shm.GrowSize (Convert.ToUInt32 (StorageHeader.GetSize ()));  // Got an exception if we define extremely huge header.
				SetHeader (shm, header);
			}

			private bool SearchNode (SharedMemory shm, string lpString, out StorageNode node)
			{
				if (GetItemsCountInStorage (shm) == 0) {
					node = new StorageNode ();
					return false;
				}

				var header = GetHeader (shm);
				int nodeOffset = StorageHeader.GetSize ();
				while (true) {
					var nodePtr = IntPtr.Add (shm.MemoryBegining, nodeOffset);
					node = StorageNode.Load (nodePtr);

					if (String.Equals(node.Key, lpString))
						return true;
					
					if (nodeOffset >= header.LastNodeOffset)
						return false;
					else
						nodeOffset += node.GetSize ();
				}
			}

			private StorageNode MakeNewStorageNode (SharedMemory shm, string lpString)
			{
				uint nodesCount = GetItemsCountInStorage (shm);

				var node = new StorageNode {
					Key = lpString,
					MessageId = MSG_RANGE_MIN + nodesCount,
				};

				return node;
			}

			private void PlaceNewNodeInLinkedList (SharedMemory shm, StorageNode newNode)
			{
				var initialSize = Helpers.UintToInt (shm.GetSize (), "Shared memory");
				
				var newNodeSize = Helpers.LongSizeToUint (newNode.GetSize (), "Storage node size");
				var freeMemBegining = IntPtr.Add (shm.MemoryBegining, initialSize);
				shm.GrowSize (newNodeSize);
				StorageNode.Store (freeMemBegining, newNode);
				
				var header = GetHeader (shm);
				header.ItemsCount += 1;
				header.LastNodeOffset = initialSize;
				SetHeader (shm, header);
			}

			private uint GetItemsCountInStorage (SharedMemory shm)
			{
				var header = GetHeader (shm);
				return header.ItemsCount;
			}

			private StorageHeader GetHeader (SharedMemory shm)
			{
				return StorageHeader.Load (shm.MemoryBegining);
			}

			private void SetHeader (SharedMemory shm, StorageHeader header)
			{
				StorageHeader.Store (shm.MemoryBegining, header);
			}

			[StructLayout(LayoutKind.Sequential)]
			internal struct StorageHeader 
			{
				public uint ItemsCount;
				
				public int LastNodeOffset;

				public static int GetSize ()
				{
					return Marshal.SizeOf<StorageHeader> ();
				}

				public static void Store (IntPtr ptr, StorageHeader header)
				{
					Marshal.StructureToPtr<StorageHeader> (header, ptr, false);
				}

				public static StorageHeader Load (IntPtr ptr)
				{
					return Marshal.PtrToStructure<StorageHeader> (ptr);
				}

				
				public override string ToString ()
				{
					return string.Format("{{StorageHeader: ItemsCount={0}, LastNodeOffset={1}}}", ItemsCount, LastNodeOffset);
				}
			}

			// We need to place string in shared memory as characters arrays right behind two fileds.
			// Meanwhile classical marshalling
			//     Marshal.PtrToStructure<StorageNode> (ptr);
			//     Marshal.StructureToPtr<StorageNode> (node, ptr, false);
			// places in shared memory a pointer to the string. This pointer will be invalid for the 
			// any other process. Hence, we should marshall this struct manually.
			internal class StorageNode
			{
				public uint MessageId;

				public string Key;

				public static void Store (IntPtr ptr, StorageNode node)
				{
					var keyAsArr = node.Key.ToCharArray ();
					var nodeHeader = MakeHeaderFromNode (node);
					Marshal.StructureToPtr<NodeHeader> (nodeHeader, ptr, false);
					Marshal.Copy(keyAsArr, 0, GetUnmanagedCharArrPtr (ptr), keyAsArr.Length);
				}

				public static StorageNode Load (IntPtr ptr)
				{
					var nodeHeader = Marshal.PtrToStructure<NodeHeader> (ptr);
					char[] key = new char [nodeHeader.KeyLen];
					Marshal.Copy(GetUnmanagedCharArrPtr (ptr), key, 0, nodeHeader.KeyLen);
					return MakeNodeFormHeaderAndKey (nodeHeader, key);
				}

				public int GetSize ()
				{
					return GetNodeHeaderSize () + GetUnmanagedCharArrSize ();
				}

				private int GetUnmanagedCharArrSize ()
				{
					return Key.ToCharArray ().Length * sizeof (char);
				}

				private static IntPtr GetUnmanagedCharArrPtr (IntPtr structPtr)
				{
					return IntPtr.Add (structPtr, GetNodeHeaderSize ());
				}

				private static int GetNodeHeaderSize ()
				{
					return Marshal.SizeOf<NodeHeader> ();
				}

				private static NodeHeader MakeHeaderFromNode (StorageNode node)
				{
					return new NodeHeader {
						MessageId = node.MessageId,
						KeyLen = node.Key.Length
					};
				}

				private static StorageNode MakeNodeFormHeaderAndKey (NodeHeader uvp, char[] key)
				{
					return new StorageNode {
						MessageId = uvp.MessageId,
						Key = new string (key)
					};
				}
				
				public override string ToString ()
				{
					return string.Format("{{StorageNode: MessageId=0x{0:X}, Key='{1}'}}", MessageId, Key);
				}

				[StructLayout(LayoutKind.Sequential)]
				struct NodeHeader
				{
					public uint MessageId;
					public int KeyLen;
				}
			}
		}

		internal class SharedMemory : IDisposable
		{
			private const int POSIX_ERR_RETCODE = -1;
			private const int OK_RET_CODE = 0;

			private bool disposed = false;
			private int shmfd = POSIX_ERR_RETCODE;
			private IntPtr shmptr = IntPtr.Zero;
			private uint cachedSize = 0;
			private object interThreadLock = new object ();

			public SharedMemory (string name)
			{
				OpenFileAndLock (name);
				if (GetSize () != 0)
					MapMemory ();
			}

			public void Dispose()
			{
				Dispose (true);
				GC.SuppressFinalize (this);
			}

			protected virtual void Dispose(bool disposing)
			{
				if (disposed)
					return;
				CloseFileAndUnlock ();
				disposed = true;
			}

			~SharedMemory()
			{
				Dispose (false);
			}

			public void Clear ()
			{
				UnmapMemory ();
				SetFileSize (0);
			}

			public void GrowSize (uint growOn)
			{
				UnmapMemory ();
				SetFileSize (GetSize () + growOn);
				MapMemory ();
			}

			// This function uses file descriptor of shared memory for inter-process licking. But POSIX `lockf`
			// function shares lock with all threads of the process. Hence we need to use additional lock to
			// achieve thread-safety -- lets employ `Monitor` сlass.
			private void OpenFileAndLock (string name)
			{
				Monitor.Enter (interThreadLock);
				
				shmfd = Syscall.shm_open (name, OpenFlags.O_CREAT | OpenFlags.O_RDWR, FilePermissions.ACCESSPERMS);
				if (shmfd == POSIX_ERR_RETCODE)
					Helpers.ThrowExceptionFromErrno ("Shared memory open error");

				int ret = Syscall.lockf (shmfd, LockfCommand.F_LOCK, 0);
				if (ret == POSIX_ERR_RETCODE)
					Helpers.ThrowExceptionFromErrno ("Shared memory locking error");
			}

			private void CloseFileAndUnlock ()
			{
				if (shmptr != IntPtr.Zero && shmptr != Syscall.MAP_FAILED)
					Syscall.munmap (shmptr, GetSize ());

				if (shmfd != POSIX_ERR_RETCODE)
					Syscall.close (shmfd);  // Remove lock automatically.

				// Locking at the very begining of the .ctor. Therefore there will not be a `SynchronizationLockException`.
				Monitor.Exit (interThreadLock);
			}

			private void SetFileSize (uint newSize)
			{
				int ret = Syscall.ftruncate (shmfd, newSize);
				if (ret == POSIX_ERR_RETCODE)
					Helpers.ThrowExceptionFromErrno (string.Format("Shared memory size changing error (newSize={0})", newSize));
			}

			private void MapMemory ()
			{
				shmptr = Syscall.mmap (new IntPtr(0), GetSize (), MmapProts.PROT_WRITE | MmapProts.PROT_READ, MmapFlags.MAP_SHARED, shmfd, 0);
				if (shmptr == Syscall.MAP_FAILED)
					Helpers.ThrowExceptionFromErrno ("Shared memory map error");
			}

			private void UnmapMemory ()
			{
				if (shmptr == IntPtr.Zero)
					return;
				int ret = Syscall.munmap (shmptr, GetSize ());
				if (ret == POSIX_ERR_RETCODE)
					Helpers.ThrowExceptionFromErrno ("Shared memory unmap fails");
			}

			public uint GetSize ()
			{
				int ret = Syscall.fstat (shmfd, out Stat buf);
				if (ret != OK_RET_CODE)
					Helpers.ThrowExceptionFromErrno ("Shared memory size getting error");
				cachedSize = Helpers.LongSizeToUint (buf.st_size, "Shared memory size");
				return cachedSize;
			}

			public IntPtr MemoryBegining
			{
				get { return shmptr; }
			}

			public override string ToString()
			{
				return string.Format("{{SharedMemory: shmfd={0}, shmptr=0x{1}, cachedSize={2}}}", shmfd, shmptr.ToString("X"), cachedSize);
			}
		}

		internal class Helpers
		{
			public static uint LongSizeToUint (long longVal, string errMsgPrefix)
			{
				if (longVal < 0)
					ThrowExceptionFromErrno (errMsgPrefix + " is negative");
				if (longVal > uint.MaxValue)
					ThrowExceptionFromErrno (errMsgPrefix + " is too big");
				return (uint)longVal;
			}

			public static int UintToInt (uint uintVal, string errMsgPrefix)
			{
				if (uintVal > int.MaxValue)
					ThrowExceptionFromErrno (errMsgPrefix + " is too big");
				return (int)uintVal;
			}

			public static void ThrowExceptionFromErrno (string msg)
			{
				var lastErr = Stdlib.GetLastError ();
				var lastErrMsg = UnixMarshal.GetErrorDescription (lastErr);
				var exText = string.Format ("{0}: {1} ({2})", msg, lastErrMsg, lastErr);
				throw new Exception (exText);
			}
		}
	}
}