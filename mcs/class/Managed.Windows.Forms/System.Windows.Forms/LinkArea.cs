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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//
//	Dennis Hayes, dennish@raytek.com
//	Andreas Nahr, ClassDevelopment@A-SoftTech.com
//	Jordi Mas i Hernandez, jordi@ximian.com
//
// $Revision: 1.2 $
// $Modtime: $
// $Log: LinkArea.cs,v $
// Revision 1.2  2004/08/11 22:20:59  pbartok
// - Signature fixes
//
// Revision 1.1  2004/07/21 16:19:17  jordi
// LinkLabel control implementation
//
//

// COMPLETE

namespace System.Windows.Forms
{
	[Serializable]
	public struct LinkArea
	{
		private int start;
		private int length;
	
		public LinkArea (int start, int length)
		{
			this.start = start;
			this.length = length;
		}
		
		#region Public Properties
		
		public int Start {
			get { return start; }
			set { start = value; }
		}

		public int Length {
			get { return length; }
			set { length = value; }
		}				
		
		public bool IsEmpty {
			get {
				if (start == 0 && length == 0)
					return true;
				else
					return false;			
				
			}
		}
		
		#endregion //Public Properties
		
		#region Methods

		public override bool Equals (object o)
		{
			if (!(o is LinkArea)) 
				return false;			

			LinkArea comp = (LinkArea) o;
			return (comp.Start == start && comp.Length == length);
		}

		public override int GetHashCode ()
		{
			return start << 4 | length;
		}
		
		#endregion //Methods
		
	}
}
