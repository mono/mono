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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//

namespace System.Windows {

	public abstract class Freezable : DependencyObject {
		protected Freezable () {
		}

		public Freezable Clone ()
		{
			throw new NotImplementedException ();
		}

		protected virtual void CloneCore (Freezable sourceFreezable)
		{
			throw new NotImplementedException ();
		}

		public Freezable CloneCurrentValue ()
		{
			throw new NotImplementedException ();
		}

		protected virtual void CloneCurrentValueCore (Freezable sourceFreezable)
		{
			throw new NotImplementedException ();
		}

		protected Freezable CreateInstance ()
		{
			throw new NotImplementedException ();
		}

		protected abstract Freezable CreateInstanceCore ();

		public void Freeze ()
		{
			throw new NotImplementedException ();
		}

		protected static bool Freeze (Freezable freezable,
					      bool isChecking)
		{
			throw new NotImplementedException ();
		}

		protected virtual bool FreezeCore (bool isChecking)
		{
			throw new NotImplementedException ();
		}

		public Freezable GetAsFrozen ()
		{
			throw new NotImplementedException ();
		}

		protected virtual void GetAsFrozenCore (Freezable sourceFreezable)
		{
			throw new NotImplementedException ();
		}

		public Freezable GetCurrentValueAsFrozen ()
		{
			throw new NotImplementedException ();
		}

		protected virtual void GetCurrentValueAsFrozenCore (Freezable sourceFreezable)
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnChanged ()
		{
			throw new NotImplementedException ();
		}

		protected void OnFreezablePropertyChanged (DependencyObject oldValue,
							   DependencyObject newValue)

		{
			throw new NotImplementedException ();
		}

		protected void OnFreezablePropertyChanged (DependencyObject oldValue,
							   DependencyObject newValue,
							   DependencyProperty property)
		{
			throw new NotImplementedException ();
		}

		protected override void OnPropertyChanged (DependencyPropertyChangedEventArgs e)
		{
			throw new NotImplementedException ();
		}

		protected void ReadPreamble ()
		{
			throw new NotImplementedException ();
		}

		protected void WritePostscript ()
		{
			throw new NotImplementedException ();
		}

		protected void WritePreamble ()
		{
			throw new NotImplementedException ();
		}

		public bool CanFreeze {
			get { return FreezeCore (true); }
		}

		public bool IsFrozen {
			get {
				throw new NotImplementedException ();
			}
		}

		public event EventHandler Changed;
	}

}
	
