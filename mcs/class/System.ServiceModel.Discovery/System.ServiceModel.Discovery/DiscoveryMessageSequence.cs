//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Discovery
{
	public class DiscoveryMessageSequence : IComparable<DiscoveryMessageSequence>, IEquatable<DiscoveryMessageSequence>
	{
		internal DiscoveryMessageSequence (long instanceId, Uri sequenceId, long messageNumber)
		{
			InstanceId = instanceId;
			SequenceId = sequenceId;
			MessageNumber = messageNumber;
		}

		public long InstanceId { get; private set; }
		public long MessageNumber { get; private set; }
		public Uri SequenceId { get; private set; }

		public bool CanCompareTo (DiscoveryMessageSequence other)
		{
			return other != null; // I cannot find any other conditions that return false.
		}

		public int CompareTo (DiscoveryMessageSequence other)
		{
			return CanCompareTo (other) ? GetHashCode () - other.GetHashCode () : -1;
		}

		public bool Equals (DiscoveryMessageSequence other)
		{
			if (other == null)
				return false;
			return  InstanceId == other.InstanceId &&
				(SequenceId == null && other.SequenceId == null || SequenceId.Equals (other.SequenceId)) &&
				MessageNumber == other.MessageNumber;
		}

		public override bool Equals (object obj)
		{
			var s = obj as DiscoveryMessageSequence;
			return s != null && Equals (s);
		}

		public override int GetHashCode ()
		{
			return (int) ((InstanceId * (SequenceId != null ? SequenceId.GetHashCode () : 1) << 17) + MessageNumber);
		}

		public override string ToString ()
		{
			return String.Format ("InstanceId={0}, SequenceId={1}, MessageNumber={2}", InstanceId, SequenceId, MessageNumber);
		}

		public static bool operator == (DiscoveryMessageSequence messageSequence1, DiscoveryMessageSequence messageSequence2)
		{
			return messageSequence1 != null ? messageSequence1.Equals (messageSequence2) : messageSequence2 == null;
		}

		public static bool operator != (DiscoveryMessageSequence messageSequence1, DiscoveryMessageSequence messageSequence2)
		{
			return messageSequence1 == null ? messageSequence2 != null : !messageSequence1.Equals (messageSequence2);
		}
	}
}
