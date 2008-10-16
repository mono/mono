/*
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
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
namespace Monodoc.Lucene.Net.Store
{
	
	/// <summary> A memory-resident {@link OutputStream} implementation.
	/// 
	/// </summary>
	/// <version>  $Id: RAMOutputStream.java,v 1.2 2004/03/29 22:48:05 cutting Exp $
	/// </version>
	
	public class RAMOutputStream:OutputStream
	{
		private RAMFile file;
		private int pointer = 0;
		
		/// <summary>Construct an empty output buffer. </summary>
		public RAMOutputStream():this(new RAMFile())
		{
		}
		
		internal RAMOutputStream(RAMFile f)
		{
			file = f;
		}
		
		/// <summary>Copy the current contents of this buffer to the named output. </summary>
		public virtual void  WriteTo(OutputStream out_Renamed)
		{
			Flush();
			long end = file.length;
			long pos = 0;
			int buffer = 0;
			while (pos < end)
			{
				int length = BUFFER_SIZE;
				long nextPos = pos + length;
				if (nextPos > end)
				{
					// at the last buffer
					length = (int) (end - pos);
				}
				out_Renamed.WriteBytes((byte[]) file.buffers[buffer++], length);
				pos = nextPos;
			}
		}
		
		/// <summary>Resets this to an empty buffer. </summary>
		public virtual void  Leset()
		{
			try
			{
				Seek(0);
			}
			catch (System.IO.IOException e)
			{
				// should never happen
				throw new System.SystemException(e.ToString());
			}
			
			file.length = 0;
		}
		
		public override void  FlushBuffer(byte[] src, int len)
		{
			int bufferNumber = pointer / BUFFER_SIZE;
			int bufferOffset = pointer % BUFFER_SIZE;
			int bytesInBuffer = BUFFER_SIZE - bufferOffset;
			int bytesToCopy = bytesInBuffer >= len?len:bytesInBuffer;
			
			if (bufferNumber == file.buffers.Count)
				file.buffers.Add(new byte[BUFFER_SIZE]);
			
			byte[] buffer = (byte[]) file.buffers[bufferNumber];
			Array.Copy(src, 0, buffer, bufferOffset, bytesToCopy);
			
			if (bytesToCopy < len)
			{
				// not all in one buffer
				int srcOffset = bytesToCopy;
				bytesToCopy = len - bytesToCopy; // remaining bytes
				bufferNumber++;
				if (bufferNumber == file.buffers.Count)
					file.buffers.Add(new byte[BUFFER_SIZE]);
				buffer = (byte[]) file.buffers[bufferNumber];
				Array.Copy(src, srcOffset, buffer, 0, bytesToCopy);
			}
			pointer += len;
			if (pointer > file.length)
				file.length = pointer;
			
			file.lastModified = (System.DateTime.Now.Ticks - 621355968000000000) / 10000;
		}
		
		public override void  Close()
		{
			base.Close();
		}
		
		public override void  Seek(long pos)
		{
			base.Seek(pos);
			pointer = (int) pos;
		}
		public override long Length()
		{
			return file.length;
		}
	}
}