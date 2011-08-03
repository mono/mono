// Authors:
//      Marek Habersack <grendel@twistedcode.net>
//
// Copyright (C) 2010 Novell Inc. http://novell.com
//

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
using System;
using System.Web.Caching;

namespace MonoTests.System.Web.Caching
{
	public enum QueueOperation
	{
		Enqueue,
		Dequeue,
		Disable,
		Peek,
		QueueSize,
		Update
	}

	public sealed class CacheItemPriorityQueueTestItem
	{
		public int ListIndex = -1;
		public int QueueCount = -1;
		public QueueOperation Operation;
		public bool IsDisabled;
		public bool IsNull;
		public bool Disable;
		public int OperationCount = -1;
		public string Guid;
		public int PriorityQueueIndex = -1;
		public long ExpiresAt = 0;
		
		public CacheItemPriorityQueueTestItem ()
		{}
		
		public CacheItemPriorityQueueTestItem (string serialized)
		{
			if (String.IsNullOrEmpty (serialized))
				throw new ArgumentException ("serialized");

			string[] parts = serialized.Split (';');
			if (parts.Length != 10)
				throw new InvalidOperationException ("Invalid serialized data.");

			ListIndex = Int32.Parse (parts [0]);
			QueueCount = Int32.Parse (parts [1]);

			int i = Int32.Parse (parts [2]);
			if (i < (int)QueueOperation.Enqueue || i > (int)QueueOperation.Update)
				throw new InvalidOperationException ("Invalid value for Operation in the serialized data.");
			Operation = (QueueOperation)i;
			IsDisabled = GetBool (parts [3]);
			IsNull = GetBool (parts [4]);
			Disable = GetBool (parts [5]);
			OperationCount = Int32.Parse (parts [6]);
			Guid = parts [7];
			PriorityQueueIndex = Int32.Parse (parts [8]);
			ExpiresAt = Int64.Parse (parts [9]);
			
			if (Guid.Length == 0)
				Guid = null;
		}

		bool GetBool (string s)
		{
			if (s == "t")
				return true;
			if (s == "f")
				return false;

			throw new InvalidOperationException ("Invalid bool string in serialized data.");
		}
		
		public string Serialize ()
		{
			return String.Format ("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9}",
					      ListIndex,
					      QueueCount,
					      (int)Operation,
					      IsDisabled ? "t" : "f",
					      IsNull ? "t" : "f",
					      Disable ? "t" : "f",
					      OperationCount,
					      Guid == null ? "null" : Guid.ToString (),
					      PriorityQueueIndex,
					      ExpiresAt);
		}
	}
}