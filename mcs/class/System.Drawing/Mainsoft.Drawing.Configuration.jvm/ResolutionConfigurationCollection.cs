using System;
using System.Collections;

namespace Mainsoft.Drawing.Configuration {
	/// <summary>
	/// Summary description for MetadataConfigurationCollection.
	/// </summary>
	public class ResolutionConfigurationCollection  : IEnumerable, ICollection {

		ArrayList _resolutionConfigurations;

		#region ctors

		internal ResolutionConfigurationCollection(ResolutionConfigurationCollection parent) {
			_resolutionConfigurations = new ArrayList();

			if (parent != null)
				_resolutionConfigurations.AddRange(parent);
		}

		#endregion

		#region methods

		internal void Add(ResolutionConfiguration value) {
			_resolutionConfigurations.Add(value);
		}

		internal void Sort() {
			_resolutionConfigurations.Sort();
		}

		#endregion

		#region props

		public ResolutionConfiguration this[int index] {
			get {
				return (ResolutionConfiguration)_resolutionConfigurations[index];
			}
		}

		public ResolutionConfiguration this[string ImageFormat] {
			get {
				for (int i=0; i < _resolutionConfigurations.Count; i++)
					if ( ((ResolutionConfiguration)_resolutionConfigurations[i]).ImageFormat == ImageFormat )
						return (ResolutionConfiguration)_resolutionConfigurations[i];
				return null;
			}
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator() {
			// TODO:  Add ResolutionConfigurationCollection.GetEnumerator implementation
			return _resolutionConfigurations.GetEnumerator();
		}

		#endregion

		#region ICollection Members

		public bool IsSynchronized {
			get {
				// TODO:  Add ResolutionConfigurationCollection.IsSynchronized getter implementation
				return _resolutionConfigurations.IsSynchronized;
			}
		}

		public int Count {
			get {
				// TODO:  Add ResolutionConfigurationCollection.Count getter implementation
				return _resolutionConfigurations.Count;
			}
		}

		public void CopyTo(Array array, int index) {
			// TODO:  Add ResolutionConfigurationCollection.CopyTo implementation
			_resolutionConfigurations.CopyTo(array, index);
		}

		public object SyncRoot {
			get {
				// TODO:  Add ResolutionConfigurationCollection.SyncRoot getter implementation
				return _resolutionConfigurations.SyncRoot;
			}
		}

		#endregion
	}
}
