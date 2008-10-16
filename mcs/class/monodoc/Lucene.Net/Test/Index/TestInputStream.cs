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
using NUnit.Framework;
using InputStream = Lucene.Net.Store.InputStream;
namespace Lucene.Net.Index
{
	[TestFixture]
	public class TestInputStream
	{
        [Test]
		public virtual void  TestRead()
		{
			InputStream is_Renamed = new MockInputStream(new byte[] 
                {
                    (byte) 0x80, (byte) 0x01, (byte) 0xFF, (byte) 0x7F, 
                    (byte) 0x80, (byte) 0x80, (byte) 0x01, (byte) 0x81, 
                    (byte) 0x80, (byte) 0x01, (byte) 0x06, (byte) 'L', 
                    (byte) 'u', (byte) 'c', (byte) 'e', (byte) 'n', 
                    (byte) 'e'}
                );
			Assert.AreEqual(128, is_Renamed.ReadVInt());
			Assert.AreEqual(16383, is_Renamed.ReadVInt());
			Assert.AreEqual(16384, is_Renamed.ReadVInt());
			Assert.AreEqual(16385, is_Renamed.ReadVInt());
			Assert.AreEqual("Lucene", is_Renamed.ReadString());
		}
	}
}