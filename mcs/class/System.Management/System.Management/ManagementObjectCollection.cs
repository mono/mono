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
using System.Runtime;
using System.Runtime.InteropServices;

namespace System.Management
{
	public class ManagementObjectCollection : ICollection, IEnumerable, IDisposable
	{
		private readonly static string name;

		internal ManagementScope scope;

		internal EnumerationOptions options;

		private IEnumWbemClassObject enumWbem;

		private bool isDisposed;

		public int Count
		{
			get
			{
				if (!this.isDisposed)
				{
					int num = 0;
					IEnumerator enumerator = this.GetEnumerator();
					while (enumerator.MoveNext())
					{
						num++;
					}
					return num;
				}
				else
				{
					throw new ObjectDisposedException(ManagementObjectCollection.name);
				}
			}
		}

		public bool IsSynchronized
		{
			get
			{
				if (!this.isDisposed)
				{
					return false;
				}
				else
				{
					throw new ObjectDisposedException(ManagementObjectCollection.name);
				}
			}
		}

		public object SyncRoot
		{
			get
			{
				if (!this.isDisposed)
				{
					return this;
				}
				else
				{
					throw new ObjectDisposedException(ManagementObjectCollection.name);
				}
			}
		}

		static ManagementObjectCollection()
		{
			ManagementObjectCollection.name = typeof(ManagementObjectCollection).FullName;
		}

		internal ManagementObjectCollection(ManagementScope scope, EnumerationOptions options, IEnumWbemClassObject enumWbem)
		{
			if (options == null)
			{
				this.options = new EnumerationOptions();
			}
			else
			{
				this.options = (EnumerationOptions)options.Clone();
			}
			if (scope == null)
			{
				this.scope = ManagementScope._Clone(null);
			}
			else
			{
				this.scope = scope.Clone();
			}
			this.enumWbem = enumWbem;
		}

		public void CopyTo(Array array, int index)
		{
			if (!this.isDisposed)
			{
				if (array != null)
				{
					if (index < array.GetLowerBound(0) || index > array.GetUpperBound(0))
					{
						throw new ArgumentOutOfRangeException("index");
					}
					else
					{
						int length = array.Length - index;
						int num = 0;
						ArrayList arrayLists = new ArrayList();
						ManagementObjectCollection.ManagementObjectEnumerator enumerator = this.GetEnumerator();
						while (enumerator.MoveNext())
						{
							ManagementBaseObject current = enumerator.Current;
							arrayLists.Add(current);
							num++;
							if (num <= length)
							{
								continue;
							}
							throw new ArgumentException(null, "index");
						}
						arrayLists.CopyTo(array, index);
						return;
					}
				}
				else
				{
					throw new ArgumentNullException("array");
				}
			}
			else
			{
				throw new ObjectDisposedException(ManagementObjectCollection.name);
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void CopyTo(ManagementBaseObject[] objectCollection, int index)
		{
			this.CopyTo(objectCollection, index);
		}

		public void Dispose()
		{
			if (!this.isDisposed)
			{
				this.Dispose(true);
			}
		}

		private void Dispose (bool disposing)
		{
			if (disposing) {
				GC.SuppressFinalize (this);
				this.isDisposed = true;
			}
			if (Marshal.IsComObject (this.enumWbem)) {
				Marshal.ReleaseComObject (this.enumWbem);
			}
		}

		~ManagementObjectCollection()
		{
			try
			{
				this.Dispose(false);
			}
			finally
			{
				//this.Finalize();
			}
		}

		public ManagementObjectCollection.ManagementObjectEnumerator GetEnumerator()
		{
			if (!this.isDisposed)
			{
				if (!this.options.Rewindable)
				{
					return new ManagementObjectCollection.ManagementObjectEnumerator(this, this.enumWbem);
				}
				else
				{
					IEnumWbemClassObject enumWbemClassObject = null;
					int num = 0;
					try
					{
						num = this.scope.GetSecuredIEnumWbemClassObjectHandler(this.enumWbem).Clone_(ref enumWbemClassObject);
						if (((long)num & (long)-2147483648) == (long)0)
						{
							num = this.scope.GetSecuredIEnumWbemClassObjectHandler(enumWbemClassObject).Reset_();
						}
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						ManagementException.ThrowWithExtendedInfo(cOMException);
					}
					if (((long)num & (long)-4096) != (long)-2147217408)
					{
						if (((long)num & (long)-2147483648) != (long)0)
						{
							Marshal.ThrowExceptionForHR(num);
						}
					}
					else
					{
						ManagementException.ThrowWithExtendedInfo((ManagementStatus)num);
					}
					return new ManagementObjectCollection.ManagementObjectEnumerator(this, enumWbemClassObject);
				}
			}
			else
			{
				throw new ObjectDisposedException(ManagementObjectCollection.name);
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public class ManagementObjectEnumerator : IEnumerator, IDisposable
		{
			private readonly static string name;

			private IEnumWbemClassObject enumWbem;

			private ManagementObjectCollection collectionObject;

			private uint cachedCount;

			private int cacheIndex;

			private IWbemClassObjectFreeThreaded[] cachedObjects;

			private bool atEndOfCollection;

			private bool isDisposed;

			public ManagementBaseObject Current
			{
				get
				{
					if (!this.isDisposed)
					{
						if (this.cacheIndex >= 0)
						{
							return ManagementBaseObject.GetBaseObject(this.cachedObjects[this.cacheIndex], this.collectionObject.scope);
						}
						else
						{
							throw new InvalidOperationException();
						}
					}
					else
					{
						throw new ObjectDisposedException(ManagementObjectCollection.ManagementObjectEnumerator.name);
					}
				}
			}

			object System.Collections.IEnumerator.Current
			{
				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				get
				{
					return this.Current;
				}
			}

			static ManagementObjectEnumerator()
			{
				ManagementObjectCollection.ManagementObjectEnumerator.name = typeof(ManagementObjectCollection.ManagementObjectEnumerator).FullName;
			}

			internal ManagementObjectEnumerator(ManagementObjectCollection collectionObject, IEnumWbemClassObject enumWbem)
			{
				this.enumWbem = enumWbem;
				this.collectionObject = collectionObject;
				this.cachedObjects = new IWbemClassObjectFreeThreaded[collectionObject.options.BlockSize];
				this.cachedCount = 0;
				this.cacheIndex = -1;
				this.atEndOfCollection = false;
			}

			public void Dispose()
			{
				if (!this.isDisposed)
				{
					if (this.enumWbem != null)
					{
						if (Marshal.IsComObject (this.enumWbem)) {
							Marshal.ReleaseComObject(this.enumWbem);
						}
						this.enumWbem = null;
					}
					this.cachedObjects = null;
					this.collectionObject = null;
					this.isDisposed = true;
					GC.SuppressFinalize(this);
				}
			}

			public bool MoveNext()
			{
				int totalMilliseconds;
				if (!this.isDisposed)
				{
					if (!this.atEndOfCollection)
					{
						ManagementObjectCollection.ManagementObjectEnumerator managementObjectEnumerator = this;
						managementObjectEnumerator.cacheIndex = managementObjectEnumerator.cacheIndex + 1;
						if ((long)this.cachedCount - (long)this.cacheIndex == (long)0)
						{
							TimeSpan timeout = this.collectionObject.options.Timeout;
							if (timeout.Ticks == 0x7fffffffffffffffL)
							{
								totalMilliseconds = -1;
							}
							else
							{
								TimeSpan timeSpan = this.collectionObject.options.Timeout;
								totalMilliseconds = (int)timeSpan.TotalMilliseconds;
							}
							int num = totalMilliseconds;
							SecurityHandler securityHandler = this.collectionObject.scope.GetSecurityHandler();
							IWbemClassObject_DoNotMarshal[] wbemClassObjectDoNotMarshalArray = new IWbemClassObject_DoNotMarshal[this.collectionObject.options.BlockSize];
							int num1 = this.collectionObject.scope.GetSecuredIEnumWbemClassObjectHandler(this.enumWbem).Next_(num, this.collectionObject.options.BlockSize, wbemClassObjectDoNotMarshalArray, ref this.cachedCount);
							securityHandler.Reset();
							if (num1 >= 0)
							{
								for (int i = 0; (long)i < (long)this.cachedCount; i++)
								{
									IntPtr ptr = Marshal.GetIUnknownForObject(wbemClassObjectDoNotMarshalArray[i].NativeObject);
									this.cachedObjects[i] = new IWbemClassObjectFreeThreaded(ptr);
								}
							}
							if (num1 >= 0)
							{
								if (num1 == 0x40004 && this.cachedCount == 0)
								{
									ManagementException.ThrowWithExtendedInfo((ManagementStatus)num1);
								}
								if (num1 == 1 && this.cachedCount == 0)
								{
									this.atEndOfCollection = true;
									ManagementObjectCollection.ManagementObjectEnumerator managementObjectEnumerator1 = this;
									managementObjectEnumerator1.cacheIndex = managementObjectEnumerator1.cacheIndex - 1;
									return false;
								}
							}
							else
							{
								if (((long)num1 & (long)-4096) != (long)-2147217408)
								{
									Marshal.ThrowExceptionForHR(num1);
								}
								else
								{
									ManagementException.ThrowWithExtendedInfo((ManagementStatus)num1);
								}
							}
							this.cacheIndex = 0;
						}
						return true;
					}
					else
					{
						return false;
					}
				}
				else
				{
					throw new ObjectDisposedException(ManagementObjectCollection.ManagementObjectEnumerator.name);
				}
			}

			public void Reset()
			{
				int num;
				if (!this.isDisposed)
				{
					if (this.collectionObject.options.Rewindable)
					{
						SecurityHandler securityHandler = this.collectionObject.scope.GetSecurityHandler();
						int num1 = 0;
						try
						{
							try
							{
								num1 = this.collectionObject.scope.GetSecuredIEnumWbemClassObjectHandler(this.enumWbem).Reset_();
							}
							catch (COMException cOMException1)
							{
								COMException cOMException = cOMException1;
								ManagementException.ThrowWithExtendedInfo(cOMException);
							}
						}
						finally
						{
							securityHandler.Reset();
						}
						if (((long)num1 & (long)-4096) != (long)-2147217408)
						{
							if (((long)num1 & (long)-2147483648) != (long)0)
							{
								Marshal.ThrowExceptionForHR(num1);
							}
						}
						else
						{
							ManagementException.ThrowWithExtendedInfo((ManagementStatus)num1);
						}
						if (this.cacheIndex >= 0)
						{
							num = this.cacheIndex;
						}
						else
						{
							num = 0;
						}
						for (int i = num; (long)i < (long)this.cachedCount; i++)
						{
							Marshal.ReleaseComObject((IWbemClassObject_DoNotMarshal)Marshal.GetObjectForIUnknown(this.cachedObjects[i]));
						}
						this.cachedCount = 0;
						this.cacheIndex = -1;
						this.atEndOfCollection = false;
						return;
					}
					else
					{
						throw new InvalidOperationException();
					}
				}
				else
				{
					throw new ObjectDisposedException(ManagementObjectCollection.ManagementObjectEnumerator.name);
				}
			}
		}
	}
}