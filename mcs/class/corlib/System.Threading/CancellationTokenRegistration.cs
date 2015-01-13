// 
// CancellationTokenRegistration.cs
//  
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
// 
// Copyright (c) 2009 Jérémie "Garuma" Laval
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

using System;

namespace System.Threading
{
	public struct CancellationTokenRegistration: IDisposable, IEquatable<CancellationTokenRegistration>
	{
		readonly int id;
		readonly CancellationTokenSource source;
		
		internal CancellationTokenRegistration (int id, CancellationTokenSource source)
		{
			this.id = id;
			this.source = source;
		}

		#region IDisposable implementation
		public void Dispose ()
		{
			if (source != null)
				source.RemoveCallback (this);
		}
		#endregion

		#region IEquatable<CancellationTokenRegistration> implementation
		public bool Equals (CancellationTokenRegistration other)
		{
			return id == other.id && source == other.source;
		}
		
		public static bool operator== (CancellationTokenRegistration left, CancellationTokenRegistration right)
		{
			return left.Equals (right);
		}
		
		public static bool operator!= (CancellationTokenRegistration left, CancellationTokenRegistration right)
		{
			return !left.Equals (right);
		}
		#endregion
		
		public override int GetHashCode ()
		{
			return id.GetHashCode () ^ (source == null ? 0 : source.GetHashCode ());
		}

		public override bool Equals (object obj)
		{
			return (obj is CancellationTokenRegistration) && Equals ((CancellationTokenRegistration)obj);
		}
	}
}
