//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//
//	(C) Ximian, Inc.  http://www.ximian.com
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

namespace System.Messaging 
{
	[Serializable]
	public enum Acknowledgment 
	{
		AccessDenied = 32772,
		BadDestinationQueue = 32768,
		BadEncryption = 32775,
		BadSignature = 32774,
		CouldNotEncrypt = 32776,
		HopCountExceeded = 32773,
		None = 0,
		NotTransactionalMessage = 32778,
		NotTransactionalQueue = 32777,
		Purged = 32769,
		QueueDeleted = 49152,
		QueueExceedMaximumSize = 32771,
		QueuePurged = 49153,
		ReachQueue = 2,
		ReachQueueTimeout = 32770,
		Receive = 16384,
		ReceiveTimeout = 49154
	}
}
