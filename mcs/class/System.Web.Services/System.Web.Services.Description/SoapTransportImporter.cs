// 
// System.Web.Services.Description.SoapTransportImporter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
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

using System.Collections;

namespace System.Web.Services.Description {
	public abstract class SoapTransportImporter {

		#region Fields
#if !TARGET_JVM
		static ArrayList transportImporters;
#else
		static ArrayList transportImporters {
			get {
				return (ArrayList)AppDomain.CurrentDomain.GetData("SoapTransportImporter.transportImporters");
			}
			set {
				AppDomain.CurrentDomain.SetData("SoapTransportImporter.transportImporters", value);
			}
		}
#endif
		SoapProtocolImporter importContext;

		#endregion // Fields

		#region Constructors
		
		static SoapTransportImporter ()
		{
			transportImporters = new ArrayList ();
			transportImporters.Add (new SoapHttpTransportImporter ());
		}
	
		protected SoapTransportImporter ()
		{
			importContext = null;
		}
		
		#endregion // Constructors

		#region Properties

		public SoapProtocolImporter ImportContext {
			get { return importContext; }
			set { importContext = value; }
		}

		#endregion // Properties

		#region Methods

		public abstract void ImportClass ();
		public abstract bool IsSupportedTransport (string transport);
		
		internal static SoapTransportImporter FindTransportImporter (string uri)
		{
			foreach (SoapHttpTransportImporter imp in transportImporters)
				if (imp.IsSupportedTransport (uri)) return imp;
			return null;
		}

		#endregion
	}
}
