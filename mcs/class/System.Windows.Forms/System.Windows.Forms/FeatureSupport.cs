//
// System.Windows.Forms.FeatureSupport.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
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

namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

    public abstract class FeatureSupport : IFeatureSupport {

		//
		//  --- Public Methods
		//
		//IFeatureSupport

		[MonoTODO]
		Version IFeatureSupport.GetVersionPresent(object feature) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		bool IFeatureSupport.IsPresent(object feature){
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		bool IFeatureSupport.IsPresent(object feature, Version minimumVersion){
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		public abstract Version GetVersionPresent(object feature);

		[MonoTODO]
		public static Version GetVersionPresent(string featureClassName, string featureConstName){
			throw new NotImplementedException ();
		}


		[MonoTODO]
		public virtual bool IsPresent(object feature){
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool IsPresent(object feature, Version minimumVersion){
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool IsPresent(string featureClassName, string featureConstName){
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool IsPresent(string featureClassName, string featureConstName, Version minimumVersion){
			throw new NotImplementedException ();
		}

		//
		// --- Protected Constructors
		//
		[MonoTODO]
		protected FeatureSupport()
		{
		}
	 }
}
