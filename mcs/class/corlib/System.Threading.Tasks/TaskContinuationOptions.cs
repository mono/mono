#if NET_4_0
// TaskContinuationKind.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System;

namespace System.Threading.Tasks
{
	[System.FlagsAttribute, System.SerializableAttribute]
	public enum TaskContinuationOptions
	{
		None = 0x0,
		PreferFairness = 0x1,
		RespectParentCancellation = 0x2,
		LongRunning = 0x4,
		DetachedFromParent = 0x8,
		ExecuteSynchronously = 0x10,
		NotOnRanToCompletion = 0x20,
		NotOnFaulted = 0x40,
		NotOnCanceled = 0x80,
		OnlyOnRanToCompletion = 0x100,
		OnlyOnFaulted = 0x200,
		OnlyOnCanceled = 0x400
	}
}
#endif
