/* 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

using Constants = Mono.Lucene.Net.Util.Constants;

namespace Mono.Lucene.Net.Store
{
	
	/// <summary>File-based {@link Directory} implementation that uses
	/// mmap for reading, and {@link
	/// SimpleFSDirectory.SimpleFSIndexOutput} for writing.
	/// 
	/// <p/><b>NOTE</b>: memory mapping uses up a portion of the
	/// virtual memory address space in your process equal to the
	/// size of the file being mapped.  Before using this class,
	/// be sure your have plenty of virtual address space, e.g. by
	/// using a 64 bit JRE, or a 32 bit JRE with indexes that are
	/// guaranteed to fit within the address space.
	/// On 32 bit platforms also consult {@link #setMaxChunkSize}
	/// if you have problems with mmap failing because of fragmented
	/// address space. If you get an OutOfMemoryException, it is recommened
	/// to reduce the chunk size, until it works.
	/// 
	/// <p/>Due to <a href="http://bugs.sun.com/bugdatabase/view_bug.do?bug_id=4724038">
	/// this bug</a> in Sun's JRE, MMapDirectory's {@link IndexInput#close}
	/// is unable to close the underlying OS file handle.  Only when GC
	/// finally collects the underlying objects, which could be quite
	/// some time later, will the file handle be closed.
	/// 
	/// <p/>This will consume additional transient disk usage: on Windows,
	/// attempts to delete or overwrite the files will result in an
	/// exception; on other platforms, which typically have a &quot;delete on
	/// last close&quot; semantics, while such operations will succeed, the bytes
	/// are still consuming space on disk.  For many applications this
	/// limitation is not a problem (e.g. if you have plenty of disk space,
	/// and you don't rely on overwriting files on Windows) but it's still
	/// an important limitation to be aware of.
	/// 
	/// <p/>This class supplies the workaround mentioned in the bug report
	/// (disabled by default, see {@link #setUseUnmap}), which may fail on
	/// non-Sun JVMs. It forcefully unmaps the buffer on close by using
	/// an undocumented internal cleanup functionality.
	/// {@link #UNMAP_SUPPORTED} is <code>true</code>, if the workaround
	/// can be enabled (with no guarantees).
	/// </summary>
	public class MMapDirectory:FSDirectory
	{
		private class AnonymousClassPrivilegedExceptionAction // : SupportClass.IPriviligedAction   // {{Aroush-2.9}}
		{
			public AnonymousClassPrivilegedExceptionAction(byte[] buffer, MMapDirectory enclosingInstance)
			{
				InitBlock(buffer, enclosingInstance);
			}
			private void  InitBlock(byte[] buffer, MMapDirectory enclosingInstance)
			{
				this.buffer = buffer;
				this.enclosingInstance = enclosingInstance;
			}
			private byte[] buffer;
			private MMapDirectory enclosingInstance;
			public MMapDirectory Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public virtual System.Object Run()
			{
                // {{Aroush-2.9
                /*
				System.Reflection.MethodInfo getCleanerMethod = buffer.GetType().GetMethod("cleaner", (Mono.Lucene.Net.Store.MMapDirectory.NO_PARAM_TYPES == null)?new System.Type[0]:(System.Type[]) Mono.Lucene.Net.Store.MMapDirectory.NO_PARAM_TYPES);
                getCleanerMethod.SetAccessible(true);
				System.Object cleaner = getCleanerMethod.Invoke(buffer, (System.Object[]) Mono.Lucene.Net.Store.MMapDirectory.NO_PARAMS);
				if (cleaner != null)
				{
					cleaner.GetType().GetMethod("clean", (Mono.Lucene.Net.Store.MMapDirectory.NO_PARAM_TYPES == null)?new System.Type[0]:(System.Type[]) Mono.Lucene.Net.Store.MMapDirectory.NO_PARAM_TYPES).Invoke(cleaner, (System.Object[]) Mono.Lucene.Net.Store.MMapDirectory.NO_PARAMS);
				}
                */
                System.Diagnostics.Debug.Fail("Port issue:", "sun.misc.Cleaner()"); // {{Aroush-2.9}}
                // Aroush-2.9}}
				return null;
			}
		}
		private void  InitBlock()
		{
			maxBBuf = Constants.JRE_IS_64BIT?System.Int32.MaxValue:(256 * 1024 * 1024);
		}
		
		/// <summary>Create a new MMapDirectory for the named location.
		/// 
		/// </summary>
		/// <param name="path">the path of the directory
		/// </param>
		/// <param name="lockFactory">the lock factory to use, or null for the default.
		/// </param>
		/// <throws>  IOException </throws>
		[System.Obsolete("Use the constructor that takes a DirectoryInfo, this will be removed in the 3.0 release")]
		public MMapDirectory(System.IO.FileInfo path, LockFactory lockFactory):base(new System.IO.DirectoryInfo(path.FullName), lockFactory)
		{
			InitBlock();
		}
		
        /// <summary>Create a new MMapDirectory for the named location.
        /// 
        /// </summary>
        /// <param name="path">the path of the directory
        /// </param>
        /// <param name="lockFactory">the lock factory to use, or null for the default.
        /// </param>
        /// <throws>  IOException </throws>
        public MMapDirectory(System.IO.DirectoryInfo path, LockFactory lockFactory) : base(path, lockFactory)
        {
            InitBlock();
        }
		
		/// <summary>Create a new MMapDirectory for the named location and the default lock factory.
		/// 
		/// </summary>
		/// <param name="path">the path of the directory
		/// </param>
		/// <throws>  IOException </throws>
		[System.Obsolete("Use the constructor that takes a DirectoryInfo, this will be removed in the 3.0 release")]
		public MMapDirectory(System.IO.FileInfo path):base(new System.IO.DirectoryInfo(path.FullName), null)
		{
			InitBlock();
		}
		
        /// <summary>Create a new MMapDirectory for the named location and the default lock factory.
        /// 
        /// </summary>
        /// <param name="path">the path of the directory
        /// </param>
        /// <throws>  IOException </throws>
        public MMapDirectory(System.IO.DirectoryInfo path) : base(path, null)
        {
            InitBlock();
        }
		
		// back compatibility so FSDirectory can instantiate via reflection
		/// <deprecated> 
		/// </deprecated>
        [Obsolete]
		internal MMapDirectory()
		{
			InitBlock();
		}
		
		internal static readonly System.Type[] NO_PARAM_TYPES = new System.Type[0];
		internal static readonly System.Object[] NO_PARAMS = new System.Object[0];
		
		private bool useUnmapHack = false;
		private int maxBBuf;
		
		/// <summary> <code>true</code>, if this platform supports unmapping mmaped files.</summary>
		public static bool UNMAP_SUPPORTED;
		
		/// <summary> This method enables the workaround for unmapping the buffers
		/// from address space after closing {@link IndexInput}, that is
		/// mentioned in the bug report. This hack may fail on non-Sun JVMs.
		/// It forcefully unmaps the buffer on close by using
		/// an undocumented internal cleanup functionality.
		/// <p/><b>NOTE:</b> Enabling this is completely unsupported
		/// by Java and may lead to JVM crashs if <code>IndexInput</code>
		/// is closed while another thread is still accessing it (SIGSEGV).
		/// </summary>
		/// <throws>  IllegalArgumentException if {@link #UNMAP_SUPPORTED} </throws>
		/// <summary> is <code>false</code> and the workaround cannot be enabled.
		/// </summary>
		public virtual void  SetUseUnmap(bool useUnmapHack)
		{
			if (useUnmapHack && !UNMAP_SUPPORTED)
				throw new System.ArgumentException("Unmap hack not supported on this platform!");
			this.useUnmapHack = useUnmapHack;
		}
		
		/// <summary> Returns <code>true</code>, if the unmap workaround is enabled.</summary>
		/// <seealso cref="setUseUnmap">
		/// </seealso>
		public virtual bool GetUseUnmap()
		{
			return useUnmapHack;
		}
		
		/// <summary> Try to unmap the buffer, this method silently fails if no support
		/// for that in the JVM. On Windows, this leads to the fact,
		/// that mmapped files cannot be modified or deleted.
		/// </summary>
		internal void  CleanMapping(System.IO.MemoryStream buffer)
		{
			if (useUnmapHack)
			{
				try
				{
                    // {{Aroush-2.9}} Not converted: java.security.AccessController.doPrivileged()
                    System.Diagnostics.Debug.Fail("Port issue:", "java.security.AccessController.doPrivileged()"); // {{Aroush-2.9}}
					// AccessController.DoPrivileged(new AnonymousClassPrivilegedExceptionAction(buffer, this));
				}
				catch (System.Exception e)
				{
					System.IO.IOException ioe = new System.IO.IOException("unable to unmap the mapped buffer", e.InnerException);
					throw ioe;
				}
			}
		}
		
		/// <summary> Sets the maximum chunk size (default is {@link Integer#MAX_VALUE} for
		/// 64 bit JVMs and 256 MiBytes for 32 bit JVMs) used for memory mapping.
		/// Especially on 32 bit platform, the address space can be very fragmented,
		/// so large index files cannot be mapped.
		/// Using a lower chunk size makes the directory implementation a little
		/// bit slower (as the correct chunk must be resolved on each seek)
		/// but the chance is higher that mmap does not fail. On 64 bit
		/// Java platforms, this parameter should always be {@link Integer#MAX_VALUE},
		/// as the adress space is big enough.
		/// </summary>
		public virtual void  SetMaxChunkSize(int maxBBuf)
		{
			if (maxBBuf <= 0)
				throw new System.ArgumentException("Maximum chunk size for mmap must be >0");
			this.maxBBuf = maxBBuf;
		}
		
		/// <summary> Returns the current mmap chunk size.</summary>
		/// <seealso cref="setMaxChunkSize">
		/// </seealso>
		public virtual int GetMaxChunkSize()
		{
			return maxBBuf;
		}
		
		private class MMapIndexInput:IndexInput, System.ICloneable
		{
			private void  InitBlock(MMapDirectory enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private MMapDirectory enclosingInstance;
			public MMapDirectory Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			
			private System.IO.MemoryStream buffer;
			private long length;
			private bool isClone = false;
			
			internal MMapIndexInput(MMapDirectory enclosingInstance, System.IO.FileStream raf)
			{
                byte[] data = new byte[raf.Length];
                raf.Read(data, 0, (int) raf.Length);

				InitBlock(enclosingInstance);
				this.length = raf.Length;
				this.buffer = new System.IO.MemoryStream(data);
			}
			
			public override byte ReadByte()
			{
				try
				{
					return (byte) buffer.ReadByte();
				}
				catch (ObjectDisposedException e)
				{
					throw new System.IO.IOException("read past EOF");
				}
			}
			
			public override void  ReadBytes(byte[] b, int offset, int len)
			{
				try
				{
					buffer.Read(b, offset, len);
				}
				catch (ObjectDisposedException e)
				{
					throw new System.IO.IOException("read past EOF");
				}
			}
			
			public override long GetFilePointer()
			{
				return buffer.Position;;
			}
			
			public override void  Seek(long pos)
			{
				buffer.Seek(pos, System.IO.SeekOrigin.Begin);
			}
			
			public override long Length()
			{
				return length;
			}
			
			public override System.Object Clone()
			{
                if (buffer == null)
                    throw new AlreadyClosedException("MMapIndexInput already closed");
				MMapIndexInput clone = (MMapIndexInput) base.Clone();
				clone.isClone = true;
				// clone.buffer = buffer.duplicate();   // {{Aroush-1.9}}
				return clone;
			}
			
			public override void  Close()
			{
				if (isClone || buffer == null)
					return ;
				// unmap the buffer (if enabled) and at least unset it for GC
				try
				{
					Enclosing_Instance.CleanMapping(buffer);
				}
				finally
				{
					buffer = null;
				}
			}
		}
		
		// Because Java's ByteBuffer uses an int to address the
		// values, it's necessary to access a file >
		// Integer.MAX_VALUE in size using multiple byte buffers.
		private class MultiMMapIndexInput:IndexInput, System.ICloneable
		{
			private void  InitBlock(MMapDirectory enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private MMapDirectory enclosingInstance;
			public MMapDirectory Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			
			private System.IO.MemoryStream[] buffers;
			private int[] bufSizes; // keep here, ByteBuffer.size() method is optional
			
			private long length;
			
			private int curBufIndex;
			private int maxBufSize;
			
			private System.IO.MemoryStream curBuf; // redundant for speed: buffers[curBufIndex]
			private int curAvail; // redundant for speed: (bufSizes[curBufIndex] - curBuf.position())
			
			private bool isClone = false;
			
			public MultiMMapIndexInput(MMapDirectory enclosingInstance, System.IO.FileStream raf, int maxBufSize)
			{
				InitBlock(enclosingInstance);
				this.length = raf.Length;
				this.maxBufSize = maxBufSize;
				
				if (maxBufSize <= 0)
					throw new System.ArgumentException("Non positive maxBufSize: " + maxBufSize);
				
				if ((length / maxBufSize) > System.Int32.MaxValue)
				{
					throw new System.ArgumentException("RandomAccessFile too big for maximum buffer size: " + raf.ToString());
				}
				
				int nrBuffers = (int) (length / maxBufSize);
				if (((long) nrBuffers * maxBufSize) < length)
					nrBuffers++;
				
				this.buffers = new System.IO.MemoryStream[nrBuffers];
				this.bufSizes = new int[nrBuffers];
				
				long bufferStart = 0;
				System.IO.FileStream rafc = raf;
				for (int bufNr = 0; bufNr < nrBuffers; bufNr++)
				{
                    byte[] data = new byte[rafc.Length];
                    raf.Read(data, 0, (int) rafc.Length);

					int bufSize = (length > (bufferStart + maxBufSize))?maxBufSize:(int) (length - bufferStart);
					this.buffers[bufNr] = new System.IO.MemoryStream(data);
					this.bufSizes[bufNr] = bufSize;
					bufferStart += bufSize;
				}
				Seek(0L);
			}
			
			public override byte ReadByte()
			{
				// Performance might be improved by reading ahead into an array of
				// e.g. 128 bytes and readByte() from there.
				if (curAvail == 0)
				{
					curBufIndex++;
					if (curBufIndex >= buffers.Length)
						throw new System.IO.IOException("read past EOF");
					curBuf = buffers[curBufIndex];
					curBuf.Seek(0, System.IO.SeekOrigin.Begin);
					curAvail = bufSizes[curBufIndex];
				}
				curAvail--;
				return (byte) curBuf.ReadByte();
			}
			
			public override void  ReadBytes(byte[] b, int offset, int len)
			{
				while (len > curAvail)
				{
					curBuf.Read(b, offset, curAvail);
					len -= curAvail;
					offset += curAvail;
					curBufIndex++;
					if (curBufIndex >= buffers.Length)
						throw new System.IO.IOException("read past EOF");
					curBuf = buffers[curBufIndex];
					curBuf.Seek(0, System.IO.SeekOrigin.Begin);
					curAvail = bufSizes[curBufIndex];
				}
				curBuf.Read(b, offset, len);
				curAvail -= len;
			}
			
			public override long GetFilePointer()
			{
				return ((long) curBufIndex * maxBufSize) + curBuf.Position;
			}
			
			public override void  Seek(long pos)
			{
				curBufIndex = (int) (pos / maxBufSize);
				curBuf = buffers[curBufIndex];
				int bufOffset = (int) (pos - ((long) curBufIndex * maxBufSize));
				curBuf.Seek(bufOffset, System.IO.SeekOrigin.Begin);
				curAvail = bufSizes[curBufIndex] - bufOffset;
			}
			
			public override long Length()
			{
				return length;
			}
			
			public override System.Object Clone()
			{
				MultiMMapIndexInput clone = (MultiMMapIndexInput) base.Clone();
				clone.isClone = true;
				clone.buffers = new System.IO.MemoryStream[buffers.Length];
				// No need to clone bufSizes.
				// Since most clones will use only one buffer, duplicate() could also be
				// done lazy in clones, e.g. when adapting curBuf.
				for (int bufNr = 0; bufNr < buffers.Length; bufNr++)
				{
					clone.buffers[bufNr] = buffers[bufNr];    // clone.buffers[bufNr] = buffers[bufNr].duplicate();   // {{Aroush-1.9}} how do we clone?!
				}
				try
				{
					clone.Seek(GetFilePointer());
				}
				catch (System.IO.IOException ioe)
				{
					System.SystemException newException = new System.SystemException(ioe.Message, ioe);
					throw newException;
				}
				return clone;
			}
			
			public override void  Close()
			{
				if (isClone || buffers == null)
					return ;
				try
				{
					for (int bufNr = 0; bufNr < buffers.Length; bufNr++)
					{
						// unmap the buffer (if enabled) and at least unset it for GC
						try
						{
							Enclosing_Instance.CleanMapping(buffers[bufNr]);
						}
						finally
						{
							buffers[bufNr] = null;
						}
					}
				}
				finally
				{
					buffers = null;
				}
			}
		}
		
		/// <summary>Creates an IndexInput for the file with the given name. </summary>
		public override IndexInput OpenInput(System.String name, int bufferSize)
		{
			EnsureOpen();
			System.String path = System.IO.Path.Combine(GetDirectory().FullName, name);
			System.IO.FileStream raf = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
			try
			{
				return (raf.Length <= (long) maxBBuf)?(IndexInput) new MMapIndexInput(this, raf):(IndexInput) new MultiMMapIndexInput(this, raf, maxBBuf);
			}
			finally
			{
				raf.Close();
			}
		}
		
		/// <summary>Creates an IndexOutput for the file with the given name. </summary>
		public override IndexOutput CreateOutput(System.String name)
		{
			InitOutput(name);
			return new SimpleFSDirectory.SimpleFSIndexOutput(new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, name)));
		}
		static MMapDirectory()
		{
			{
				bool v;
				try
				{
                    // {{Aroush-2.9
					/*
                    System.Type.GetType("sun.misc.Cleaner"); // {{Aroush-2.9}} port issue?
					System.Type.GetType("java.nio.DirectByteBuffer").GetMethod("cleaner", (NO_PARAM_TYPES == null)?new System.Type[0]:(System.Type[]) NO_PARAM_TYPES);
                    */
                    System.Diagnostics.Debug.Fail("Port issue:", "sun.misc.Cleaner.clean()"); // {{Aroush-2.9}}
                    // Aroush-2.9}}
					v = true;
				}
				catch (System.Exception e)
				{
					v = false;
				}
				UNMAP_SUPPORTED = v;
			}
		}
	}
}
