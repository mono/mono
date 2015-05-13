//
// System.Management.AuthenticationLevel
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
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
using System.Collections;
using System.Collections.Specialized;
using System.Runtime;
using System.Runtime.Serialization;

namespace System.Management
{
	public class ManagementNamedValueCollection : NameObjectCollectionBase
	{
		public object this[string name]
		{
			get
			{
				return base.BaseGet(name);
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementNamedValueCollection()
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected ManagementNamedValueCollection(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public void Add(string name, object value)
		{
			try
			{
				base.BaseRemove(name);
			}
			catch
			{
			}
			base.BaseAdd(name, value);
			this.FireIdentifierChanged();
		}

		public ManagementNamedValueCollection Clone()
		{
			ManagementNamedValueCollection managementNamedValueCollection = new ManagementNamedValueCollection();
			foreach (string str in this)
			{
				object obj = base.BaseGet(str);
				if (obj == null)
				{
					managementNamedValueCollection.Add(str, null);
				}
				else
				{
					Type type = obj.GetType();
					if (!type.IsByRef)
					{
						managementNamedValueCollection.Add(str, obj);
					}
					else
					{
						try
						{
							object obj1 = ((ICloneable)obj).Clone();
							managementNamedValueCollection.Add(str, obj1);
						}
						catch
						{
							throw new NotSupportedException();
						}
					}
				}
			}
			return managementNamedValueCollection;
		}

		private void FireIdentifierChanged()
		{
			if (this.IdentifierChanged != null)
			{
				this.IdentifierChanged(this, null);
			}
		}

		internal IWbemContext GetContext()
		{
			IWbemContext wbemContext = null;
			if (0 < this.Count)
			{
				int num = 0;
				try
				{
					wbemContext = (IWbemContext)(new WbemContext());
					IEnumerator enumerator = this.GetEnumerator();
					try
					{
						do
						{
							if (!enumerator.MoveNext())
							{
								break;
							}
							string current = (string)enumerator.Current;
							object obj = base.BaseGet(current);
							num = wbemContext.SetValue_(current, 0, ref obj);
						}
						while (((long)num & (long)-2147483648) == (long)0);
					}
					finally
					{
						IDisposable disposable = enumerator as IDisposable;
						if (disposable != null)
						{
							disposable.Dispose();
						}
					}
				}
				catch
				{
				}
			}
			return wbemContext;
		}

		public void Remove(string name)
		{
			base.BaseRemove(name);
			this.FireIdentifierChanged();
		}

		public void RemoveAll()
		{
			base.BaseClear();
			this.FireIdentifierChanged();
		}

		internal event IdentifierChangedEventHandler IdentifierChanged;
	}
}