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

namespace Mono.Lucene.Net.Store
{
	
	/// <summary>Writes bytes through to a primary IndexOutput, computing
	/// checksum as it goes. Note that you cannot use seek(). 
	/// </summary>
	public class ChecksumIndexInput:IndexInput
	{
		internal IndexInput main;
		internal SupportClass.Checksum digest;
		
		public ChecksumIndexInput(IndexInput main)
		{
			this.main = main;
            digest = new SupportClass.CRC32();
		}
		
		public override byte ReadByte()
		{
			byte b = main.ReadByte();
			digest.Update(b);
			return b;
		}
		
		public override void  ReadBytes(byte[] b, int offset, int len)
		{
			main.ReadBytes(b, offset, len);
			digest.Update(b, offset, len);
		}
		
		
		public virtual long GetChecksum()
		{
			return digest.GetValue();
		}
		
		public override void  Close()
		{
			main.Close();
		}
		
		public override long GetFilePointer()
		{
			return main.GetFilePointer();
		}
		
		public override void  Seek(long pos)
		{
			throw new System.SystemException("not allowed");
		}
		
		public override long Length()
		{
			return main.Length();
		}

        /*
		override public System.Object Clone()
		{
			return null;
		}
        */
	}
}
