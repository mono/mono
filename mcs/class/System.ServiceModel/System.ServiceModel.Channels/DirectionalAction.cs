//
// DirectionalAction.cs
//
// Author: Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Runtime.Serialization;
using System.ServiceModel.Description;

namespace System.ServiceModel.Channels
{
	internal class DirectionalAction : IComparable<DirectionalAction>
	{
		public DirectionalAction (MessageDirection direction, string action)
		{
			this.dir = direction;
			this.action = action;
		}

		MessageDirection dir;
		string action;

		public MessageDirection Direction {
			get { return dir; }
		}

		public string Action {
			get { return action; }
		}

		public int CompareTo (DirectionalAction other)
		{
			int diff = (int) Direction - (int) other.Direction;
			return diff != 0 ? diff :
				String.CompareOrdinal (Action, other.Action);
		}

		public bool Equals (DirectionalAction other)
		{
			return other != null && Direction ==
				other.Direction &&
				Action == other.Action;
		}

		public override bool Equals (object other)
		{
			DirectionalAction d = other as DirectionalAction;
			return d != null && Equals (d);
		}

		public override int GetHashCode ()
		{
			return Action.GetHashCode () ^ ((int) Direction << 24);
		}
	}
}
