//
// System.Drawing.Drawing2D.GraphicsPathIterator.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002/3 Ximian, Inc
//
using System;

namespace System.Drawing.Drawing2D {
	/// <summary>
	/// Summary description for GraphicsPathIterator.
	/// </summary>
	/// 

	public sealed class GraphicsPathIterator : MarshalByRefObject {

		protected GraphicsPath path;
		
		// Constructors
		public GraphicsPathIterator(GraphicsPath path) {
			this.path = path;
		}

		//Public Properites
		public int Count {
			get {
				throw new NotImplementedException ();
			}
		}

		public int SubpathCount {
			get {
				throw new NotImplementedException ();
			}
		}

		//Public Methods.
		public int CopyData( ref PointF [] points, ref byte [] types, int startIndex, int endIndex){
			throw new NotImplementedException ();
		}

		public void Dispose(){
		}

		public int Enumerate(ref PointF [] points, ref byte [] types){
			throw new NotImplementedException ();
		}

		public bool HasCurve(){
			throw new NotImplementedException ();
		}

		public int NextMarker(GraphicsPath path){
			throw new NotImplementedException ();
		}

		public int NextMarker(out int startIndex, out int endIndex){
			throw new NotImplementedException ();
		}

		public int NextPathType(out byte pathType, out int startIndex, int endIndex){
			throw new NotImplementedException ();
		}

		public int NextSubpath(GraphicsPath path, out bool isClosed){
			throw new NotImplementedException ();
		}

		public int NextSubpath(out int startIndex, out int endIndex, out bool isClosed){
			throw new NotImplementedException ();
		}

		public void Rewind(){
		}

	}
}
