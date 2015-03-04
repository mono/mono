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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

// COMPLETE

using System.Reflection;

namespace System.Windows.Forms {
	public abstract class FeatureSupport : IFeatureSupport {
		#region Public Constructors
		protected FeatureSupport() {
		}
		#endregion	// Public Constructors

		#region Private and Internal Methods
		private static IFeatureSupport FeatureObject(string class_name) {
			Type	class_type;

			class_type = Type.GetType(class_name);
			if ((class_type != null) && (typeof(IFeatureSupport).IsAssignableFrom(class_type))) {
				ConstructorInfo	ctor;

				ctor = class_type.GetConstructor(Type.EmptyTypes);
				if (ctor != null) {
					return ((IFeatureSupport)ctor.Invoke(new Object[0]));
				}
			}

			return null;
		}
		#endregion	// Private and Internal Methods

		#region Public Static Methods
		public static Version GetVersionPresent(string featureClassName, string featureConstName) {
			IFeatureSupport	obj;

			obj = FeatureObject(featureClassName);
			if (obj != null) {
				return obj.GetVersionPresent(featureConstName);
			}
			return null;
		}

		public static bool IsPresent(string featureClassName, string featureConstName) {
			IFeatureSupport	obj;

			obj = FeatureObject(featureClassName);
			if (obj != null) {
				return obj.IsPresent(featureConstName);
			}

			return false;
		}

		public static bool IsPresent(string featureClassName, string featureConstName, Version minimumVersion) {
			IFeatureSupport	obj;

			obj = FeatureObject(featureClassName);
			if (obj != null) {
				return obj.IsPresent(featureConstName, minimumVersion);
			}

			return false;
		}
		#endregion	// Public Static Methods

		#region Public Instance Methods
		public abstract Version GetVersionPresent(object feature);

		public virtual bool IsPresent(object feature) {
			if (GetVersionPresent(feature) != null) {
				return true;
			}

			return false;
		}

		public virtual bool IsPresent(object feature, Version minimumVersion) {
			Version	version;
			bool	retval;

			retval = false;
			version = GetVersionPresent(feature);

			if ((version != null) && (version >= minimumVersion)) {
				retval = true;
			}

			return retval;
		}
		#endregion	// Public Instance Methods
	}
}
