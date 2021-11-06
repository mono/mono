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
//	Loïc Rebmeister (fox2code@gmail.com)
//

namespace System.Windows {

	public abstract class Freezable : DependencyObject {
		bool frozen = false;

		protected Freezable () {
		}

		internal delegate Freezable cloneDelegate (Freezable freezable);

		internal void cloneImpl (Freezable source, cloneDelegate cloneFreezable,bool freeze)
		{
			var e = source.GetLocalValueEnumerator ();
			while (e.MoveNext ()) {
				var value = e.Current.Value;
				if (e.Current.Property.ReadOnly || value == DependencyProperty.UnsetValue || value == null)
					continue;
				if (value is Freezable && !(freeze && (value as Freezable).frozen))
					value = cloneFreezable (value as Freezable);
				SetValue (e.Current.Property, value);
			}
			if (freeze)
				Freeze ();
		}

		public Freezable Clone ()
		{
			var freezable = this.CreateInstance ();
			freezable.CloneCore (this);
			return freezable;
		}

		protected virtual void CloneCore (Freezable sourceFreezable)
		{
			cloneImpl (sourceFreezable, freezable => freezable.Clone (), false);
		}

		public Freezable CloneCurrentValue ()
		{
			var freezable = this.CreateInstance ();
			freezable.CloneCurrentValueCore (this);
			return freezable;
		}

		protected virtual void CloneCurrentValueCore (Freezable sourceFreezable)
		{
			cloneImpl (sourceFreezable, freezable => freezable.CloneCurrentValue (), false);
		}

		protected Freezable CreateInstance ()
		{
			return CreateInstanceCore ();
		}

		protected abstract Freezable CreateInstanceCore ();

		public void Freeze ()
		{
			if (!FreezeCore (false))
				throw new InvalidOperationException ("The Freezable cannot be made unmodifiable.");
		}

		protected static bool Freeze (Freezable freezable,
					      bool isChecking)
		{
			if (freezable.frozen)
				return true;
			LocalValueEnumerator e = freezable.GetLocalValueEnumerator ();
			while (e.MoveNext ()) {
				var value = e.Current.Value;
				if (value is Freezable) {
					if(!(value as Freezable).FreezeCore (isChecking))
						return false;
				}
			}
			if (!isChecking)
				freezable.frozen = true;
			return true;
		}

		protected virtual bool FreezeCore (bool isChecking)
		{
			return Freeze (this, isChecking);
		}

		public Freezable GetAsFrozen ()
		{
			var freezable = this.CreateInstance ();
			freezable.GetAsFrozenCore (this);
			return freezable;
		}

		protected virtual void GetAsFrozenCore (Freezable sourceFreezable)
		{
			cloneImpl (sourceFreezable, freezable => freezable.GetAsFrozen (), true);
		}

		public Freezable GetCurrentValueAsFrozen ()
		{
			var freezable = this.CreateInstance ();
			freezable.GetCurrentValueAsFrozenCore (this);
			return freezable;
		}

		protected virtual void GetCurrentValueAsFrozenCore (Freezable sourceFreezable)
		{
			cloneImpl (sourceFreezable, freezable => freezable.GetCurrentValueAsFrozen (), true);
		}

		protected virtual void OnChanged ()
		{
		}

		[MonoTODO]
		protected void OnFreezablePropertyChanged (DependencyObject oldValue,
							   DependencyObject newValue)

		{
		}

		[MonoTODO]
		protected void OnFreezablePropertyChanged (DependencyObject oldValue,
							   DependencyObject newValue,
							   DependencyProperty property)
		{
		}

		protected override void OnPropertyChanged (DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged (e);
			// Note: Handler need to be called on all DependencyObject properties
			if (typeof (DependencyObject).IsAssignableFrom (e.Property.PropertyType)) {
				var oldValue = e.OldValue as DependencyObject;
				var newValue = e.NewValue as DependencyObject;
				OnFreezablePropertyChanged (oldValue, newValue, e.Property);
				OnFreezablePropertyChanged (oldValue, newValue);
			}
			Changed?.Invoke (this, EventArgs.Empty);
		}

		protected void ReadPreamble ()
		{
			VerifyAccess ();
		}

		protected void WritePostscript ()
		{
			Changed?.Invoke (this, EventArgs.Empty);
			OnChanged ();
		}

		protected void WritePreamble ()
		{
			VerifyAccess ();
			if (frozen)
				throw new InvalidOperationException ("The Freezable instance is frozen and cannot have its members written to.");
		}

		public bool CanFreeze {
			get { return FreezeCore (true); }
		}

		public bool IsFrozen {
			get { return frozen; }
		}

		public event EventHandler Changed;
	}

}
	
