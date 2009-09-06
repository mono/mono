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
// Copyright (c) 2008 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//

using System;
using System.ComponentModel;
using System.Windows.Threading;

namespace System.Windows.Data {

	public abstract class DataSourceProvider : INotifyPropertyChanged, ISupportInitialize
	{
		protected DataSourceProvider ()
		{
			throw new NotImplementedException ();
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public object Data {
			get { throw new NotImplementedException (); }
		}

		protected Dispatcher Dispatcher {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public Exception Error {
			get { throw new NotImplementedException (); }
		}

		[DefaultValue (true)]
		public bool IsInitialLoadEnabled {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		protected bool IsRefreshDeferred {
			get { throw new NotImplementedException (); }
		}

		public event EventHandler DataChanged;

		protected virtual event PropertyChangedEventHandler PropertyChanged;

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged {
			add { PropertyChanged += value; }
			remove { PropertyChanged -= value; }
		}

		protected virtual void BeginInit ()
		{
			throw new NotImplementedException ();
		}
		void ISupportInitialize.BeginInit ()
		{
			BeginInit ();
		}

		protected virtual void EndInit ()
		{
			throw new NotImplementedException ();
		}
		void ISupportInitialize.EndInit ()
		{
			EndInit ();
		}

		protected virtual void BeginQuery ()
		{
			throw new NotImplementedException ();
		}

		public virtual IDisposable DeferRefresh ()
		{
			throw new NotImplementedException ();
		}

		public void InitialLoad ()
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnPropertyChanged (PropertyChangedEventArgs e)
		{
			throw new NotImplementedException ();
		}

		protected void OnQueryFinished (object newData)
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnQueryFinished (object newData, Exception error, DispatcherOperationCallback completionWork, object callbackArguments)
		{
			throw new NotImplementedException ();
		}

		public void Refresh ()
		{
			throw new NotImplementedException ();
		}
	}
}

