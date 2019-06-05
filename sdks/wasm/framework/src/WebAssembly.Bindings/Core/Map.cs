using System;
using System.Collections;

namespace WebAssembly.Core {
	/// <summary>
	/// The Map object holds key-value pairs and remembers the original insertion order of the keys.
	/// Any value (both objects and primitive values) may be used as either a key or a value.
	/// </summary>
	public class Map : CoreObject, IDictionary {

		/// <summary>
		/// Initializes a new instance of the <see cref="T:WebAssembly.Core.Map"/> class.
		/// </summary>
		/// <param name="_params">Parameters.</param>
		public Map (params object [] _params) : base (Runtime.New<Map> (_params))
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:WebAssembly.Core.Map"/> class.
		/// </summary>
		/// <param name="js_handle">Js handle.</param>
		internal Map (IntPtr js_handle) : base (js_handle)
		{ }

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:WebAssembly.Core.Map"/> object has a fixed size.
		/// </summary>
		public bool IsFixedSize => false;

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:WebAssembly.Core.Map"/> object is read-only.
		/// </summary>
		public bool IsReadOnly => false;

		/// <summary>
		/// Gets an <see cref="T:System.Collections.ICollection"/> object containing the keys of the <see cref="T:WebAssembly.Core.Map"/> object.
		/// </summary>
		public ICollection Keys {
			// TODO: Look at wrapping the Map Iterator for Keys in a ICollection implementaion
			get {
				var keyCollection = new ArrayList ();
				using (var keysIterator = (JSObject)Invoke ("keys")) {
					var done = false;
					while (!done) {
						using (var result = (JSObject)keysIterator.Invoke ("next")) {
							done = (bool)result.GetObjectProperty ("done");
							if (!done) {
								keyCollection.Add (result.GetObjectProperty ("value"));
							}
						}
					}
				}
				return keyCollection;
			}
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.ICollection"/> object containing the values of the <see cref="T:WebAssembly.Core.Map"/> object.
		/// </summary>
		public ICollection Values {
			// TODO: Look at wrapping the Map Iterator for Values in a ICollection implementaion
			get {
				var valueCollection = new ArrayList ();
				using (var valuesIterator = (JSObject)Invoke ("values")) {
					var done = false;
					while (!done) {
						using (var result = (JSObject)valuesIterator.Invoke ("next")) {
							done = (bool)result.GetObjectProperty ("done");
							if (!done) {
								valueCollection.Add (result.GetObjectProperty ("value"));
							}
						}
					}
				}
				return valueCollection;
			}

		}

		public int Count => (int)GetObjectProperty ("size");

		public bool IsSynchronized => false;

		public object SyncRoot => false;

		public void Add (object key, object value)
		{
			Invoke ("set", key, value);
		}

		public void Clear () => Invoke ("clear");

		public bool Contains (object key) => (bool)Invoke ("has", key);

		public IDictionaryEnumerator GetEnumerator ()
		{
			// Construct and return an enumerator.
			return new MapEnumerator (this);
		}

		public void Remove (object key) => Invoke ("delete", key);

		public void CopyTo (System.Array array, int index) => throw new NotImplementedException ();

		IEnumerator IEnumerable.GetEnumerator ()
		{
			// Construct and return an enumerator.
			return ((IDictionary)this).GetEnumerator ();
		}

		/// <summary>
		/// Gets or sets the <see cref="T:WebAssembly.Core.Map"/> with the key specified by <paramref name="key" />.
		/// </summary>
		/// <param name="key">The key.</param>
		public object this [object key] {
			get {
				return Invoke ("get", key);
			}
			set {
				Invoke ("set", key, value);
			}
		}

		private sealed class MapEnumerator : IDictionaryEnumerator, IDisposable {
			readonly JSObject mapIterator;

			public MapEnumerator (Map map)
			{
				try {
					mapIterator = (JSObject)map.Invoke ("entries");
				} catch (Exception ex) {
					Console.WriteLine (ex.Message);
				}

			}

			// Return the current item.
			public object Current => new DictionaryEntry (Key, Value);

			// Return the current dictionary entry.
			public DictionaryEntry Entry {
				get { return (DictionaryEntry)Current; }
			}

			// Return the key of the current item.
			public object Key { get; private set; }

			// Return the value of the current item.
			public object Value { get; private set; }

			// Advance to the next item.
			public bool MoveNext ()
			{
				if (mapIterator == null)
					return false;

				using (var result = (JSObject)mapIterator.Invoke ("next")) {
					using (var resultValue = (Array)result.GetObjectProperty ("value")) {
						if (resultValue != null) {
							Key = resultValue [0];
							Value = resultValue [1];
						} else {
							Key = null;
							Value = null;
						}
					}
					return !(bool)result.GetObjectProperty ("done");
				}
			}

			// Reset the index to restart the enumeration.
			public void Reset ()
			{
				throw new NotImplementedException ();
			}

			#region IDisposable Support
			private bool disposedValue = false; // To detect redundant calls

			void Dispose (bool disposing)
			{
				if (!disposedValue) {
					if (disposing) {
						// TODO: dispose managed state (managed objects).
					}

					// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
					// TODO: set large fields to null.
					mapIterator.Dispose ();
					disposedValue = true;
				}
			}

			//TODO: override a finalizer only if Dispose (bool disposing) above has code to free unmanaged resources.
			// ~MapEnumerator ()
			//{
			//	// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			//	Dispose (false);
			//}

			// This code added to correctly implement the disposable pattern.
			public void Dispose ()
			{
				// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
				Dispose (true);
				// TODO: uncomment the following line if the finalizer is overridden above.
				//GC.SuppressFinalize(this);
			}
			#endregion
		}

	}
}