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
namespace Lucene.Net.Store
{
	
	/// <summary>This class provides access to package-level features defined in the
	/// store package. It is used for testing only.
	/// </summary>
	public class TestHelper
	{
		
		/// <summary>Returns true if the instance of the provided input stream is actually
		/// an FSInputStream.
		/// </summary>
		public static bool isFSInputStream(InputStream is_Renamed)
		{
			return is_Renamed is FSInputStream;
		}
		
		/// <summary>Returns true if the provided input stream is an FSInputStream and
		/// is a clone, that is it does not own its underlying file descriptor.
		/// </summary>
		public static bool IsFSInputStreamClone(InputStream is_Renamed)
		{
			if (isFSInputStream(is_Renamed))
			{
				return ((FSInputStream) is_Renamed).isClone;
			}
			else
			{
				return false;
			}
		}
		
		/// <summary>Given an instance of FSDirectory.FSInputStream, this method returns
		/// true if the underlying file descriptor is valid, and false otherwise.
		/// This can be used to determine if the OS file has been closed.
		/// The descriptor becomes invalid when the non-clone instance of the
		/// FSInputStream that owns this descriptor is closed. However, the
		/// descriptor may possibly become invalid in other ways as well.
		/// </summary>
		public static bool IsFSInputStreamOpen(InputStream is_Renamed)
		{
			if (isFSInputStream(is_Renamed))
			{
				FSInputStream fis = (FSInputStream) is_Renamed;
				return fis.IsFDValid();
			}
			else
			{
				return false;
			}
		}
	}
}