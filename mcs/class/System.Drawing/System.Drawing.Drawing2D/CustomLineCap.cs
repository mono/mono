//
// System.Drawing.Drawing2D.CustomLineCap.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002/3 Ximian, Inc
//
using System;

namespace System.Drawing.Drawing2D {
	/// <summary>
	/// Summary description for CustomLineCap.
	/// </summary>
	public class CustomLineCap : MarshalByRefObject, ICloneable, IDisposable {
		private LineCap baseCap;
		private float baseInset;
		private LineJoin strokeJoin;
		private float widthScale;

		// Constructors
		// Constructor with no parameters is not part of spec. It was needed to get to compile. Bug in compiler?
		protected CustomLineCap() {
		}
		public CustomLineCap(GraphicsPath fillPAth, GraphicsPath strokePath, LineCap baseCap, float baseInset) {
			this.baseInset = baseInset;
		}
		public CustomLineCap(GraphicsPath fillPAth, GraphicsPath strokePAth, LineCap baseCap) {
			this.baseCap = baseCap;
		}
		public CustomLineCap(GraphicsPath fillPAth, GraphicsPath strokePAth) {
		}

		public LineCap BaseCap{
			get {
				return baseCap;
			}
			set {
				baseCap = value;
			}
		}
		public LineJoin StrokeJoin{
			get {
				return strokeJoin;
			}
			set {
				strokeJoin = value;
			}
		}
		public float BaseInset{
			get {
				return baseInset;
			}
			set {
				baseInset = value;
			}
		}

		public float WidthScale{
			get {
				return widthScale;
			}
			set {
				widthScale = value;
			}
		}

		//Public Methods

		// Implment IConeable.Clone
		public object Clone(){
			//FIXME
			return new CustomLineCap();
		}
		
		public virtual void Dispose(){
			Dispose(true);
		}
		public virtual void Dispose(bool disposing){
		}
		
		public void GetStrokeCaps(out LineCap startCap, out LineCap endCap){
			startCap = baseCap;
			endCap = baseCap;
		}

		public void SetStrokeCaps(LineCap startCap, LineCap endCap){
		}
	}
}
