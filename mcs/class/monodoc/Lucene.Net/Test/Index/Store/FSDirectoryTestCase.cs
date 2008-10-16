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
using FSDirectory = Lucene.Net.Store.FSDirectory;
namespace Lucene.Net.Index.Store
{
	[TestFixture]
	abstract public class FSDirectoryTestCase
	{
		private FSDirectory directory;
		
		protected internal FSDirectory GetDirectory()
		{
			return GetDirectory(false);
		}
		
		protected internal FSDirectory GetDirectory(bool create)
		{
			if (directory == null)
			{
				directory = FSDirectory.GetDirectory(SupportClass.AppSettings.Get("test.index.dir", "."), create);
			}
			
			return directory;
		}
	}
}