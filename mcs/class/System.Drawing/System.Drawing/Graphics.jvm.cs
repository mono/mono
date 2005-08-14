using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using awt = java.awt;
using geom = java.awt.geom;

namespace System.Drawing {
	[ComVisible(false)]
	public sealed class Graphics : MarshalByRefObject, IDisposable {
		#region Variables

		readonly awt.Graphics2D _nativeObject;
		readonly Image _image;
		
		Matrix _transform;
		GraphicsUnit _pageUnit = GraphicsUnit.Display;
		float _pageScale = 1.0f;

		static readonly float [] _unitConversion = {
													   1,								// World
													   1,								// Display
													   1,								// Pixel
													   DefaultScreenResolution / 72.0f,	// Point
													   DefaultScreenResolution,			// Inch
													   DefaultScreenResolution / 300.0f,// Document
													   DefaultScreenResolution / 25.4f	// Millimeter
												   };

		static internal readonly bool IsHeadless;
	
		#endregion

#if INTPTR_SUPPORT
		[ComVisible(false)]
		public delegate bool EnumerateMetafileProc (EmfPlusRecordType recordType,
							    int flags,
							    int dataSize,
							    IntPtr data,
							    PlayRecordCallback callbackData);
		[ComVisible (false)]
			public delegate bool DrawImageAbort (IntPtr callbackData);		
#endif			

		#region Constr. and Destr.
		private Graphics (Image image) {
			_nativeObject = (awt.Graphics2D)image.NativeObject.getGraphics();
			_image = image;
			_transform = new Matrix ();
			_nativeObject.setTransform( _transform.NativeObject );

//			NativeObject.setRenderingHint(awt.RenderingHints.KEY_TEXT_ANTIALIASING,awt.RenderingHints.VALUE_TEXT_ANTIALIAS_ON);
//
//							((java.awt.Graphics2D)NativeObject).setRenderingHint(java.awt.RenderingHints.KEY_ANTIALIASING,java.awt.RenderingHints.VALUE_ANTIALIAS_ON);
//							((java.awt.Graphics2D)NativeObject).setRenderingHint(java.awt.RenderingHints.KEY_STROKE_CONTROL,java.awt.RenderingHints.VALUE_STROKE_NORMALIZE);
//							((java.awt.Graphics2D)NativeObject).setRenderingHint(java.awt.RenderingHints.KEY_FRACTIONALMETRICS,java.awt.RenderingHints.VALUE_FRACTIONALMETRICS_ON);
//							((java.awt.Graphics2D)NativeObject).setRenderingHint(java.awt.RenderingHints.KEY_RENDERING,java.awt.RenderingHints.VALUE_RENDER_QUALITY);
//							((java.awt.Graphics2D)NativeObject).setRenderingHint(java.awt.RenderingHints.KEY_TEXT_ANTIALIASING,java.awt.RenderingHints.VALUE_TEXT_ANTIALIAS_ON);
//							//((java.awt.Graphics2D)NativeObject).setRenderingHint(java.awt.RenderingHints.KEY_TEXT_ANTIALIASING,java.awt.RenderingHints.VALUE_TEXT_ANTIALIAS_ON);
//							((java.awt.Graphics2D)NativeObject).setComposite(java.awt.AlphaComposite.SrcOver);
//							((java.awt.Graphics2D)NativeObject).setRenderingHint(java.awt.RenderingHints.KEY_INTERPOLATION,java.awt.RenderingHints.VALUE_INTERPOLATION_BILINEAR);
//							((java.awt.Graphics2D)NativeObject).setRenderingHint(java.awt.RenderingHints.KEY_ALPHA_INTERPOLATION,java.awt.RenderingHints.VALUE_ALPHA_INTERPOLATION_QUALITY);
//							((java.awt.Graphics2D)NativeObject).setRenderingHint(java.awt.RenderingHints.KEY_DITHERING,java.awt.RenderingHints.VALUE_DITHER_DISABLE);
//							((java.awt.Graphics2D)NativeObject).setBackground(java.awt.Color.WHITE);

		}

		//		~Graphics ()
		//		{
		//			nativeObject.finalize();
		//		}
		#endregion
		
		#region Internal Accessors

		static Graphics() {
			bool isHeadless = awt.GraphicsEnvironment.isHeadless();
			if (!isHeadless) {
				try {
					awt.Toolkit.getDefaultToolkit();
				}
				catch{
					isHeadless = true;
				}
			}

			IsHeadless = isHeadless;
		}

		static internal int DefaultScreenResolution {
			get {
				return IsHeadless ? 96 :
					awt.Toolkit.getDefaultToolkit().getScreenResolution();
			}
		}
		
		internal java.awt.Graphics2D NativeObject {
			get {
				return _nativeObject;
			}
		}
		#endregion

		#region FromImage (static accessor)
		public static Graphics FromImage (Image image) {		
			return new Graphics(image);
		}
		#endregion

		#region Workers [INTERNAL]
		void InternalSetBrush(Brush b) {
			java.awt.Graphics2D g = NativeObject;
			g.setPaint (b);
			SolidBrush sb = b as SolidBrush;
			if(sb != null) {
				g.setColor(sb.Color.NativeObject);
			}
			else if(b is LinearGradientBrush) {
#if DEBUG_GRADIENT_BRUSH
				if(((LinearGradientBrush)b).dPoints != null)
				{
					PointF []pts = ((LinearGradientBrush)b).dPoints;
					java.awt.Shape s = g.getClip();
					g.setClip(0,0,99999,99999);
					g.setPaint(new java.awt.Color(255,0,0));
					g.drawLine((int)pts[0].X,(int)pts[0].Y,(int)pts[1].X,(int)pts[1].Y);
					g.setPaint(new java.awt.Color(0,255,0));
					g.drawLine((int)pts[1].X,(int)pts[1].Y,(int)pts[2].X,(int)pts[2].Y);
					g.setPaint(new java.awt.Color(255,0,0));
					g.drawLine((int)pts[2].X,(int)pts[2].Y,(int)pts[3].X,(int)pts[3].Y);
					g.setPaint(new java.awt.Color(0,255,0));
					g.drawLine((int)pts[3].X,(int)pts[3].Y,(int)pts[0].X,(int)pts[0].Y);
					
					g.setClip(s);
				}
#endif
			}
		}

		void DrawShape(Pen pen, awt.Shape shape) {
			if (pen == null)
				throw new ArgumentNullException("pen");

			shape = ((awt.Stroke)pen).createStrokedShape(shape);
			FillShape(pen.Brush, shape);
		}

		void FillShape(awt.Paint paint, awt.Shape shape) {
			if (paint == null)
				throw new ArgumentNullException("brush");

			awt.Paint old = NativeObject.getPaint();
			NativeObject.setPaint(paint);
			try {
				NativeObject.fill(shape);
			}
			finally {
				NativeObject.setPaint(old);
			}
		}

		internal SizeF MeasureDraw (string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format, bool fDraw) {			
			SizeF retVal = new SizeF(0,0);
			awt.Graphics2D g = NativeObject;
				
			java.awt.Font fnt = font.NativeObject;
			if(s != null && s.Length != 0 && fnt != null) {
				float size = fnt.getSize();
				float wid = layoutRectangle.Width;
				float hei = layoutRectangle.Height;
				float x = layoutRectangle.X;
				float y = layoutRectangle.Y;
				java.text.AttributedString astr = new java.text.AttributedString(s);
				astr.addAttribute(java.awt.font.TextAttribute.FONT, fnt);
				java.text.AttributedCharacterIterator prg = astr.getIterator();
				java.awt.font.TextLayout textlayout = new java.awt.font.TextLayout(prg, g.getFontRenderContext());
				int prgStart = prg.getBeginIndex();
				int prgEnd = prg.getEndIndex();
				java.awt.font.LineBreakMeasurer lineMeasurer = new java.awt.font.LineBreakMeasurer(
					prg, new java.awt.font.FontRenderContext(null, false, false));
				lineMeasurer.setPosition(prgStart);
				float formatWidth = wid;
				
				//some vertical layout magic - should be reviewed
				//				if(format != null)
				//				{
				//					StringFormatFlags formatflag = format.FormatFlags;
				//					if(formatflag != StringFormatFlags.DirectionVertical)
				//					{
				//						if(size > (float)8 && wid > (float)13)
				//							formatWidth = wid - (float)12;
				//						if(formatflag ==  StringFormatFlags.DirectionRightToLeft && size == (float)6)
				//							formatWidth = wid - (float)10;
				//					}
				//				}

				//now calculate number of lines and full layout height
				//this is required for LineAlignment calculations....
				int lines=0;
				float layHeight=0;
				float layPrevHeight=0;
				float layWidth=0;
				float drawPosY = y;
				//bool fSkipLastLine = false;
				java.awt.font.TextLayout layout;
				java.awt.geom.Rectangle2D bnds = new java.awt.geom.Rectangle2D.Float();
				while(lineMeasurer.getPosition() < prgEnd) {
					layout = lineMeasurer.nextLayout(formatWidth);
					lines++;
					bnds = bnds.createUnion(layout.getBounds());
					layPrevHeight = layHeight;
					layHeight += layout.getDescent() + layout.getLeading() + layout.getAscent();
					float advance;
					if((format != null) && 
						((format.FormatFlags & StringFormatFlags.MeasureTrailingSpaces) != 0))
						advance = layout.getAdvance();
					else
						advance = layout.getVisibleAdvance();
					if(layWidth < advance)
						layWidth = advance;
					if((format != null) && ((format.FormatFlags & StringFormatFlags.NoWrap) != 0))
						break;
				}
				//Overhanging parts of glyphs, and unwrapped text reaching outside 
				//the formatting rectangle are allowed to show. By default all text 
				//and glyph parts reaching outside the formatting rectangle are clipped.
				if((lines == 1) && 
					(format != null) && 
					((format.FormatFlags & StringFormatFlags.NoClip) != 0)) {
					formatWidth = layWidth;					
				}

				//Only entire lines are laid out in the formatting rectangle. By default layout 
				//continues until the end of the text, or until no more lines are visible as a 
				//result of clipping, whichever comes first. Note that the default settings allow 
				//the last line to be partially obscured by a formatting rectangle that is not a 
				//whole multiple of the line height. To ensure that only whole lines are seen, specify
				//this value and be careful to provide a formatting rectangle at least as tall as the 
				//height of one line.
				if(format != null && ((format.FormatFlags & StringFormatFlags.LineLimit) != 0) && 
					layHeight > hei && layPrevHeight < hei) {
					layHeight = layPrevHeight;
					lines--;
				}

				retVal.Height = layHeight;
				retVal.Width = layWidth+size/2.5f;
				if(!fDraw)
					return retVal;

				InternalSetBrush(brush);

				//end measurment

				//for draw we should probably update origins

				//Vertical...
				if(format != null) {
					StringAlignment align = format.LineAlignment;
					if(align == StringAlignment.Center) {
						drawPosY = y + (float)hei/2 - layHeight/2;
					}
					else if(align == StringAlignment.Far) {
						drawPosY = (float)y + (float)hei - layHeight;
					}
					//in both cases if string is not fit - switch to near
					if(drawPosY < y)
						drawPosY = y;
				}

				//Horisontal... on the fly
				lineMeasurer.setPosition(prgStart);
				float drawPosX = x;				
				for(int line = 0;line < lines /*lineMeasurer.getPosition() < prgEnd*/;line++, drawPosY += layout.getDescent() + layout.getLeading()) {
					layout = lineMeasurer.nextLayout(formatWidth);
					drawPosX = x + size / (float)5;//???
					drawPosY += layout.getAscent();
					if(format != null) {
						float advance;
						if((format.FormatFlags & StringFormatFlags.MeasureTrailingSpaces) != 0)
							advance = layout.getAdvance();
						else
							advance = layout.getVisibleAdvance();
						
						if(format.Alignment == StringAlignment.Center) {
							drawPosX = (float)((double)x + ((double)formatWidth)/2 - advance/2);
						}
						else if(format.Alignment == StringAlignment.Far) {
							drawPosX = (float)(drawPosX + formatWidth) - advance;
						}
						if((format.FormatFlags & StringFormatFlags.DirectionVertical ) != 0) {
							java.awt.geom.AffineTransform at1 = java.awt.geom.AffineTransform.getTranslateInstance(
								drawPosX +  size / (float)5, (drawPosY - layout.getAscent()) + size / (float)5);

							at1.rotate(Math.PI/2);
							awt.Shape sha = textlayout.getOutline(at1);
							//g.setRenderingHint(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON);
							g.fill(sha);
							continue;
						}
						if((format.FormatFlags & StringFormatFlags.DirectionRightToLeft)  != 0) {
							drawPosX = ((drawPosX + formatWidth) - advance) + (float)9;
							layout.draw(g, drawPosX, drawPosY);
						} 
					}					
					//Draw current line
					layout.draw(g, drawPosX, drawPosY);					
				}
			} //not nulls

			return retVal;
		}
		#endregion

		#region Dispose
		public void Dispose() {			
			NativeObject.dispose();
		}
		#endregion
		
		#region Clear
		public void Clear (Color color) {
			geom.AffineTransform old = NativeObject.getTransform();
			NativeObject.setTransform(new geom.AffineTransform());
			try {
				FillShape(color.NativeObject, new awt.Rectangle(0,0,_image.Width,_image.Height));
			}
			finally {
				NativeObject.setTransform(old);
			}
		}
		#endregion

		#region DrawArc
		public void DrawArc (Pen pen, Rectangle rect, float startAngle, float sweepAngle) {
			DrawArc (pen, 
				rect.X, 
				rect.Y, 
				rect.Width, 
				rect.Height, 
				startAngle, 
				sweepAngle);
		}

		
		public void DrawArc (Pen pen, RectangleF rect, float startAngle, float sweepAngle) {
			DrawArc (pen, 
				rect.X, 
				rect.Y, 
				rect.Width, 
				rect.Height, 
				startAngle, 
				sweepAngle);
		}

		public void DrawArc (Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle) {
			DrawArc(pen,
				(float)x,
				(float)y,
				(float)width,
				(float)height,
				(float)startAngle,
				(float)sweepAngle);
		}

		public void DrawArc (Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle) {
			GraphicsPath path = new GraphicsPath();
			path.AddArc(x, y, width, height, startAngle, sweepAngle);
			DrawPath(pen, path);
		}
		#endregion

		#region DrawBezier(s)
		public void DrawBezier (Pen pen, PointF pt1, PointF pt2, PointF pt3, PointF pt4) {
			DrawBezier(pen, pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
		}

		public void DrawBezier (Pen pen, Point pt1, Point pt2, Point pt3, Point pt4) {
			DrawBezier(pen, pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
		}

		public void DrawBezier (Pen pen, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4) {
			geom.GeneralPath path = new geom.GeneralPath();
			path.moveTo(x1,y1);
			path.curveTo(x2,y2,x3,y3,x4,y4);
			DrawShape(pen, path);
		}

		public void DrawBeziers (Pen pen, Point [] points) {
			GraphicsPath path = new GraphicsPath();
			path.AddBeziers(points);
			DrawPath(pen, path);
		}

		public void DrawBeziers (Pen pen, PointF [] points) {
			GraphicsPath path = new GraphicsPath();
			path.AddBeziers(points);
			DrawPath(pen, path);
		}
		#endregion 

		#region DrawClosedCurve
		public void DrawClosedCurve (Pen pen, PointF [] points) {
			DrawClosedCurve(pen, points, 0.5f, FillMode.Alternate);
		}
		
		public void DrawClosedCurve (Pen pen, Point [] points) {
			DrawClosedCurve(pen, points, 0.5f, FillMode.Alternate);
		}
 			
		public void DrawClosedCurve (Pen pen, Point [] points, float tension, FillMode fillmode) {
			GraphicsPath path = new GraphicsPath(fillmode);
			path.AddClosedCurve(points, tension);
			DrawPath(pen, path);
		}
		
		public void DrawClosedCurve (Pen pen, PointF [] points, float tension, FillMode fillmode) {
			GraphicsPath path = new GraphicsPath(fillmode);
			path.AddClosedCurve(points, tension);
			DrawPath(pen, path);
		}
		#endregion

		#region DrawCurve
		public void DrawCurve (Pen pen, Point [] points) {
			DrawCurve(pen, points, 0.5f);
		}
		
		public void DrawCurve (Pen pen, PointF [] points) {
			DrawCurve(pen, points, 0.5f);
		}
		
		public void DrawCurve (Pen pen, PointF [] points, float tension) {
			DrawCurve(pen, points, 0, points.Length-1, tension);
		}
		
		public void DrawCurve (Pen pen, Point [] points, float tension) {
			DrawCurve(pen, points, 0, points.Length-1, tension);
		}
		
		
		public void DrawCurve (Pen pen, PointF [] points, int offset, int numberOfSegments) {
			DrawCurve(pen, points, offset, numberOfSegments, 0.5f);
		}

		public void DrawCurve (Pen pen, Point [] points, int offset, int numberOfSegments, float tension) {
			GraphicsPath path = new GraphicsPath();
			path.AddCurve(points, offset, numberOfSegments, tension);
			DrawPath(pen, path);
		}

		
		public void DrawCurve (Pen pen, PointF [] points, int offset, int numberOfSegments, float tension) {
			GraphicsPath path = new GraphicsPath();
			path.AddCurve(points, offset, numberOfSegments, tension);
			DrawPath(pen, path);
		}
		#endregion

		#region DrawEllipse
		public void DrawEllipse (Pen pen, Rectangle rect) {
			DrawEllipse (pen, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public void DrawEllipse (Pen pen, RectangleF rect) {
			DrawEllipse (pen, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public void DrawEllipse (Pen pen, int x, int y, int width, int height) {
			DrawEllipse(pen,(float)x,(float)y,(float)width,(float)height);
		}

		public void DrawEllipse (Pen pen, float x, float y, float width, float height) {
			DrawShape(pen, new geom.Ellipse2D.Float(x,y,width,height));
		}
		#endregion

		#region DrawIcon
		public void DrawIcon (Icon icon, Rectangle targetRect) {
			Bitmap b = icon.ToBitmap ();
			this.DrawImage (b, targetRect);
		}

		public void DrawIcon (Icon icon, int x, int y) {
			Bitmap b = icon.ToBitmap ();
			this.DrawImage (b, x, y);
		}

		public void DrawIconUnstretched (Icon icon, Rectangle targetRect) {
			Bitmap b = icon.ToBitmap ();
			this.DrawImageUnscaled (b, targetRect);
		}
		#endregion

		#region DrawImage
		public void DrawImage (Image image, RectangleF rect) {
			geom.AffineTransform at = geom.AffineTransform.getTranslateInstance(rect.X, rect.Y);
			at.scale(rect.Width/image.Width, rect.Height/image.Height);
			NativeObject.drawImage(image.NativeObject,at,null);
		}

		
		public void DrawImage (Image image, PointF point) {
			geom.AffineTransform at = geom.AffineTransform.getTranslateInstance(point.X, point.Y);
			NativeObject.drawImage(image.NativeObject,at,null);
		}

		
		public void DrawImage (Image image, Point [] destPoints) {
			Matrix m = new Matrix(new Rectangle(0, 0, image.Width, image.Height), destPoints);
			NativeObject.drawImage(image.NativeObject,m.NativeObject,null);
		}

		
		public void DrawImage (Image image, Point point) {
			java.awt.Graphics2D g = NativeObject;
			g.drawImage(image.NativeObject,point.X,point.Y,null);
		}

		
		public void DrawImage (Image image, Rectangle rect) {
			java.awt.Graphics2D g = NativeObject;
			g.drawImage(image.NativeObject,rect.X,rect.Y,rect.Width,rect.Height,null);
		}

		
		public void DrawImage (Image image, PointF [] destPoints) {
			Matrix m = new Matrix(new RectangleF(0, 0, image.Width, image.Height), destPoints);
			NativeObject.drawImage(image.NativeObject,m.NativeObject,null);
		}

		
		public void DrawImage (Image image, int x, int y) {
			java.awt.Graphics2D g =  NativeObject;
			g.drawImage(image.NativeObject,x,y,null);
		}

		
		public void DrawImage (Image image, float x, float y) {
			java.awt.Graphics2D g = NativeObject;
			g.drawImage(image.NativeObject,(int)x,(int)y,null);
		}

		
		public void DrawImage (Image image, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit) {
			if (srcUnit != GraphicsUnit.Pixel)
				throw new NotImplementedException();
				// Like in .NET http://dotnet247.com/247reference/msgs/45/227979.aspx

			java.awt.Graphics2D g = NativeObject;
			g.drawImage(image.NativeObject,
				destRect.X,
				destRect.Y,
				destRect.X + destRect.Width,
				destRect.Y + destRect.Height,
				srcRect.X,
				srcRect.Y,
				srcRect.X + srcRect.Width,
				srcRect.Y + srcRect.Height,
				null);
		}
		
		public void DrawImage (Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit) {			
			if (srcUnit != GraphicsUnit.Pixel)
				throw new NotImplementedException();
			// Like in .NET http://dotnet247.com/247reference/msgs/45/227979.aspx

			java.awt.Graphics2D g = NativeObject;
			g.drawImage(image.NativeObject,
				(int)destRect.X,
				(int)destRect.Y,
				(int)destRect.X + (int)destRect.Width,
				(int)destRect.Y + (int)destRect.Height,
				(int)srcRect.X,
				(int)srcRect.Y,
				(int)srcRect.X + (int)srcRect.Width,
				(int)srcRect.Y + (int)srcRect.Height,
				null);
		}

		
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit) {
			DrawImage(image, destPoints, srcRect, srcUnit, null);
		}

		
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit) {
			
			DrawImage(image, destPoints, srcRect, srcUnit, null);
		}

		
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, 
			ImageAttributes imageAttr) {
			
			if (srcUnit != GraphicsUnit.Pixel)
				throw new NotImplementedException();
			// Like in .NET http://dotnet247.com/247reference/msgs/45/227979.aspx
			
			//TBD: ImageAttributes
			Matrix m = new Matrix(srcRect, destPoints);
			NativeObject.drawImage(image.NativeObject,m.NativeObject,null);
		}
		
		public void DrawImage (Image image, float x, float y, float width, float height) {
			java.awt.Graphics2D g = NativeObject;
			g.drawImage(image.NativeObject,(int)x,(int)y,(int)width,(int)height,null);
		}

		
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, 
			ImageAttributes imageAttr) {

			if (srcUnit != GraphicsUnit.Pixel)
				throw new NotImplementedException();
			// Like in .NET http://dotnet247.com/247reference/msgs/45/227979.aspx

			//TBD: ImageAttributes
			Matrix m = new Matrix(srcRect, destPoints);
			NativeObject.drawImage(image.NativeObject,m.NativeObject,null);
		}

		
		public void DrawImage (Image image, int x, int y, Rectangle srcRect, GraphicsUnit srcUnit) {			
			if (srcUnit != GraphicsUnit.Pixel)
				throw new NotImplementedException();
			// Like in .NET http://dotnet247.com/247reference/msgs/45/227979.aspx

			java.awt.Graphics2D g = NativeObject;
			g.drawImage(image.NativeObject,x,y,srcRect.Width,srcRect.Height,srcRect.X,srcRect.Y,srcRect.Width,srcRect.Height,null);
		}
		
		public void DrawImage (Image image, int x, int y, int width, int height) {
			java.awt.Graphics2D g = NativeObject;
			g.drawImage(image.NativeObject,x,y,width,height,null);
		}

		public void DrawImage (Image image, float x, float y, RectangleF srcRect, GraphicsUnit srcUnit) {	
			if (srcUnit != GraphicsUnit.Pixel)
				throw new NotImplementedException();
			// Like in .NET http://dotnet247.com/247reference/msgs/45/227979.aspx
			
			//TBD: casts
			java.awt.Graphics2D g = NativeObject;
			g.drawImage(image.NativeObject,
				(int)x,
				(int)y,
				(int)srcRect.Width,
				(int)srcRect.Height,
				(int)srcRect.X,
				(int)srcRect.Y,
				(int)srcRect.Width,
				(int)srcRect.Height,null);
		}

#if INTPTR_SUPPORT
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
		{
			throw new NotImplementedException();
		}

		
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
		{
			
			throw new NotImplementedException();
		}

		
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, int callbackData)
		{
			throw new NotImplementedException();
		}
#endif
		
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit) {
			if (srcUnit != GraphicsUnit.Pixel)
				throw new NotImplementedException();
			// Like in .NET http://dotnet247.com/247reference/msgs/45/227979.aspx

			java.awt.Graphics2D g = NativeObject;
			g.drawImage(image.NativeObject,
				(int)destRect.X,
				(int)destRect.Y,
				(int)destRect.Width,
				(int)destRect.Height,
				(int)srcX,
				(int)srcY,
				(int)srcWidth,
				(int)srcHeight,null);
		}
		
#if INTPTR_SUPPORT		
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, int callbackData)
		{
			throw new NotImplementedException();
		}
#endif
		
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit) {
			if (srcUnit != GraphicsUnit.Pixel)
				throw new NotImplementedException();
			// Like in .NET http://dotnet247.com/247reference/msgs/45/227979.aspx

			java.awt.Graphics2D g = NativeObject;
			g.drawImage(image.NativeObject,destRect.X,destRect.Y,destRect.Width,destRect.Height,srcX,srcY,srcWidth,srcHeight,null);
		}

		
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs) {
			if (srcUnit != GraphicsUnit.Pixel)
				throw new NotImplementedException();
			// Like in .NET http://dotnet247.com/247reference/msgs/45/227979.aspx

			java.awt.Graphics2D g = NativeObject;
			g.drawImage(image.NativeObject,
				(int)destRect.X,
				(int)destRect.Y,
				(int)destRect.Width,
				(int)destRect.Height,
				(int)srcX,
				(int)srcY,
				(int)srcWidth,
				(int)srcHeight,null);
		}
		
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr) {			
			if (srcUnit != GraphicsUnit.Pixel)
				throw new NotImplementedException();
			// Like in .NET http://dotnet247.com/247reference/msgs/45/227979.aspx
			
			//TBD: attributes
			java.awt.Graphics2D g = NativeObject;
			g.drawImage(image.NativeObject,
				destRect.X,
				destRect.Y,
				destRect.X + destRect.Width,
				destRect.Y+destRect.Height,
				srcX,
				srcY,
				srcX+srcWidth,
				srcX+srcHeight,null);
		}
		
#if INTPTR_SUPPORT		
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
		{
			//TBD:units,attributes, callback
			java.awt.Graphics2D g = (java.awt.Graphics2D)nativeObject;
			g.drawImage(image.NativeObject,destRect.X,destRect.Y,destRect.Width,destRect.Height,srcX,srcY,srcWidth,srcHeight,null);
		}
		
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
		{
			//TBD:units,attributes, callback
			java.awt.Graphics2D g = (java.awt.Graphics2D)nativeObject;
			g.drawImage(image.NativeObject,
				(int)destRect.X,
				(int)destRect.Y,
				(int)destRect.Width,
				(int)destRect.Height,
				(int)srcX,
				(int)srcY,
				(int)srcWidth,
				(int)srcHeight,null);
		}

		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, IntPtr callbackData)
		{
			//TBD:units,attributes, callback
			java.awt.Graphics2D g = (java.awt.Graphics2D)nativeObject;
			g.drawImage(image.NativeObject,
				(int)destRect.X,
				(int)destRect.Y,
				(int)destRect.Width,
				(int)destRect.Height,
				(int)srcX,
				(int)srcY,
				(int)srcWidth,
				(int)srcHeight,null);
		}
		
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, IntPtr callbackData)
		{
			//TBD:units,attributes, callback
			java.awt.Graphics2D g = (java.awt.Graphics2D)nativeObject;
			g.drawImage(image.NativeObject,
				destRect.X,
				destRect.Y,
				destRect.Width,
				destRect.Height,
				srcX,
				srcY,
				srcWidth,
				srcHeight,null);
		}		
#endif
		
		public void DrawImageUnscaled (Image image, Point point) {
			DrawImageUnscaled (image, point.X, point.Y);
		}
		
		public void DrawImageUnscaled (Image image, Rectangle rect) {
			DrawImageUnscaled (image, rect.X, rect.Y, rect.Width, rect.Height);
		}
		
		public void DrawImageUnscaled (Image image, int x, int y) {
			DrawImage (image, x, y, image.Width, image.Height);
		}

		public void DrawImageUnscaled (Image image, int x, int y, int width, int height) {
			Image tmpImg = new Bitmap (width, height);
			Graphics g = FromImage (tmpImg);
			g.DrawImage (image, 0, 0, image.Width, image.Height);
			this.DrawImage (tmpImg, x, y, width, height);
			tmpImg.Dispose ();
			g.Dispose ();		
		}
		#endregion

		#region DrawLine
		public void DrawLine (Pen pen, PointF pt1, PointF pt2) {
			DrawLine(pen,pt1.X,pt1.Y,pt2.X,pt2.Y);
		}

		public void DrawLine (Pen pen, Point pt1, Point pt2) {
			DrawLine(pen,(float)pt1.X,(float)pt1.Y,(float)pt2.X,(float)pt2.Y);
		}

		public void DrawLine (Pen pen, int x1, int y1, int x2, int y2) {
			DrawLine(pen,(float)x1,(float)y1,(float)x2,(float)y2);
		}

		public void DrawLine (Pen pen, float x1, float y1, float x2, float y2) {
			DrawShape(pen, new geom.Line2D.Float(x1,y1,x2,y2));
		}

		public void DrawLines (Pen pen, PointF [] points) {
			GraphicsPath path = new GraphicsPath();
			path.AddLines(points);
			DrawShape(pen, path);
		}

		public void DrawLines (Pen pen, Point [] points) {
			GraphicsPath path = new GraphicsPath();
			path.AddLines(points);
			DrawShape(pen, path);
		}
		#endregion

		#region DrawPath
		public void DrawPath (Pen pen, GraphicsPath path) {
			DrawShape(pen, path);
		}
		#endregion
		
		#region DrawPie
		public void DrawPie (Pen pen, Rectangle rect, float startAngle, float sweepAngle) {
			DrawPie (pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
		}
		
		public void DrawPie (Pen pen, RectangleF rect, float startAngle, float sweepAngle) {
			DrawPie (pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
		}
		
		public void DrawPie (Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle) {
			GraphicsPath path = new GraphicsPath();
			path.AddPie(x, y, width, height, startAngle, sweepAngle);
			DrawPath(pen, path);
		}
		
		public void DrawPie (Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle) {
			DrawPie(pen,(float)x,(float)y,(float)width,(float)height,(float)startAngle,(float)sweepAngle);
		}
		#endregion

		#region DrawPolygon
		public void DrawPolygon (Pen pen, Point [] points) {
			GraphicsPath path = new GraphicsPath();
			path.AddPolygon(points);
			DrawPath(pen, path);
		}

		public void DrawPolygon (Pen pen, PointF [] points) {
			GraphicsPath path = new GraphicsPath();
			path.AddPolygon(points);
			DrawPath(pen, path);
		}
		#endregion

		#region DrawRectangle(s)
		internal void DrawRectangle (Pen pen, RectangleF rect) {
			DrawRectangle (pen, rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public void DrawRectangle (Pen pen, Rectangle rect) {
			DrawRectangle (pen, rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public void DrawRectangle (Pen pen, float x, float y, float width, float height) {
			DrawShape(pen, new geom.Rectangle2D.Float(x,y,width,height));
		}

		public void DrawRectangle (Pen pen, int x, int y, int width, int height) {
			DrawRectangle (pen,(float) x,(float) y,(float) width,(float) height);
		}

		public void DrawRectangles (Pen pen, RectangleF [] rects) {
			foreach(RectangleF r in rects)
				DrawRectangle (pen, r.Left, r.Top, r.Width, r.Height);
		}

		public void DrawRectangles (Pen pen, Rectangle [] rects) {
			foreach(Rectangle r in rects)
				DrawRectangle (pen, r.Left, r.Top, r.Width, r.Height);
		}
		#endregion

		#region DrawString
		public void DrawString (string s, Font font, Brush brush, RectangleF layoutRectangle) {			
			MeasureDraw(s,font,brush,layoutRectangle,null,true);
		}
		
		public void DrawString (string s, Font font, Brush brush, PointF point) {
			MeasureDraw(s,font,brush,new RectangleF(point.X,point.Y,99999f,99999f),null,true);
		}
		
		public void DrawString (string s, Font font, Brush brush, PointF point, StringFormat format) {
			MeasureDraw(s,font,brush,new RectangleF(point.X,point.Y,99999f,99999f),format,true);
		}

		public void DrawString (string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format) {
			MeasureDraw(s,font,brush,layoutRectangle,format,true);
		}
	
#if false		
		public void DrawString (string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format)
		{
			java.awt.Graphics2D g = (java.awt.Graphics2D)nativeObject;			
			//set
			java.awt.Paint oldpaint = g.getPaint();
			g.setPaint(brush.NativeObject);
			java.awt.Font oldfont = g.getFont();
			g.setFont(font.NativeObject);
			java.awt.Shape oldb = g.getClip();
			g.setClip(new java.awt.geom.Rectangle2D.Float(layoutRectangle.X,layoutRectangle.Y,layoutRectangle.Width,layoutRectangle.Height));
			//draw
			g.drawString(s,layoutRectangle.X,layoutRectangle.Y+layoutRectangle.Height);
			//restore
			g.setPaint(oldpaint);
			g.setClip(oldb);
			g.setFont(oldfont);
		}
#endif
		public void DrawString (string s, Font font, Brush brush, float x, float y) {
			MeasureDraw(s,font,brush,new RectangleF(x,y,99999f,99999f),null,true);
		}

		public void DrawString (string s, Font font, Brush brush, float x, float y, StringFormat format) {
			MeasureDraw(s,font,brush,new RectangleF(x,y,99999f,99999f),format,true);
		}
		#endregion

		#region Container [TODO]
		public void EndContainer (GraphicsContainer container) {
			throw new NotImplementedException ();
		}

		public GraphicsContainer BeginContainer () {
			throw new NotImplementedException ();
		}
		
		public GraphicsContainer BeginContainer (Rectangle dstrect, Rectangle srcrect, GraphicsUnit unit) {
			throw new NotImplementedException ();
		}

		
		public GraphicsContainer BeginContainer (RectangleF dstrect, RectangleF srcrect, GraphicsUnit unit) {
			throw new NotImplementedException ();
		}

		#endregion

		#region Metafiles Staff [TODO NotSupp]
		public void AddMetafileComment (byte [] data) {
			throw new NotImplementedException ();
		}

#if INTPTR_SUPPORT
		public void EnumerateMetafile (Metafile metafile, Point [] destPoints, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, RectangleF destRect, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, PointF [] destPoints, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Rectangle destRect, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Point destPoint, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, PointF destPoint, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, PointF destPoint, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Rectangle destRect, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, PointF [] destPoints, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Point destPoint, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Point [] destPoints, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, RectangleF destRect, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, RectangleF destRect, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Point destPoint, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, PointF destPoint, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Point [] destPoints, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, PointF [] destPoints, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Rectangle destRect, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Point [] destPoints, Rectangle srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, PointF [] destPoints, RectangleF srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}
#endif 	
		#endregion	

		#region ExcludeClip
		public void ExcludeClip (Rectangle rect) {
			geom.Area clip = GetNativeClip();
			clip.subtract(new geom.Area(rect.NativeObject));
			SetNativeClip(clip);
		}

		public void ExcludeClip (Region region) {
			geom.Area clip = GetNativeClip();
			clip.subtract(region.NativeObject);
			SetNativeClip(clip);
		}
		#endregion 

		#region FillClosedCurve
		public void FillClosedCurve (Brush brush, PointF [] points) {
			FillClosedCurve (brush, points, FillMode.Alternate);
		}

		
		public void FillClosedCurve (Brush brush, Point [] points) {
			FillClosedCurve (brush, points, FillMode.Alternate);
		}

		
		public void FillClosedCurve (Brush brush, PointF [] points, FillMode fillmode) {
			FillClosedCurve (brush, points, fillmode, 0.5f);
		}
		
		public void FillClosedCurve (Brush brush, Point [] points, FillMode fillmode) {
			FillClosedCurve (brush, points, fillmode, 0.5f);
		}

		public void FillClosedCurve (Brush brush, PointF [] points, FillMode fillmode, float tension) {
			GraphicsPath path = new GraphicsPath(fillmode);
			path.AddClosedCurve(points, tension);
			FillPath(brush, path);
		}

		public void FillClosedCurve (Brush brush, Point [] points, FillMode fillmode, float tension) {
			GraphicsPath path = new GraphicsPath(fillmode);
			path.AddClosedCurve(points, tension);
			FillPath(brush, path);
		}
		#endregion

		#region FillEllipse
		public void FillEllipse (Brush brush, Rectangle rect) {
			FillEllipse (brush, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public void FillEllipse (Brush brush, RectangleF rect) {
			FillEllipse (brush, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public void FillEllipse (Brush brush, float x, float y, float width, float height) {
			FillShape(brush,new java.awt.geom.Ellipse2D.Float(x,y,width,height));
		}

		public void FillEllipse (Brush brush, int x, int y, int width, int height) {
			FillEllipse (brush,(float)x,(float)y,(float)width,(float)height);
		}
		#endregion

		#region FillPath
		public void FillPath (Brush brush, GraphicsPath path) {
			FillShape(brush,path);
		}
		#endregion

		#region FillPie
		public void FillPie (Brush brush, Rectangle rect, float startAngle, float sweepAngle) {
			FillPie(brush,(float)rect.X,(float)rect.Y,(float)rect.Width,(float)rect.Height,(float)startAngle,(float)sweepAngle);
		}

		public void FillPie (Brush brush, int x, int y, int width, int height, int startAngle, int sweepAngle) {
			FillPie(brush,(float)x,(float)y,(float)width,(float)height,(float)startAngle,(float)sweepAngle);
		}

		public void FillPie (Brush brush, float x, float y, float width, float height, float startAngle, float sweepAngle) {
			GraphicsPath path = new GraphicsPath();
			path.AddPie(x, y, width, height, startAngle, sweepAngle);
			FillPath(brush, path);
		}
		#endregion

		#region FillPolygon
		public void FillPolygon (Brush brush, PointF [] points) {
			FillPolygon(brush, points, FillMode.Alternate);
		}

		public void FillPolygon (Brush brush, Point [] points) {
			FillPolygon(brush, points, FillMode.Alternate);
		}

		public void FillPolygon (Brush brush, Point [] points, FillMode fillMode) {
			GraphicsPath path = new GraphicsPath(fillMode);
			path.AddPolygon(points);
			FillPath(brush,path);
		}

		public void FillPolygon (Brush brush, PointF [] points, FillMode fillMode) {
			GraphicsPath path = new GraphicsPath(fillMode);
			path.AddPolygon(points);
			FillPath(brush,path);
		}
		#endregion

		#region FillRectangle
		public void FillRectangle (Brush brush, RectangleF rect) {
			FillRectangle (brush, rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public void FillRectangle (Brush brush, Rectangle rect) {
			FillRectangle (brush, rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public void FillRectangle (Brush brush, int x, int y, int width, int height) {
			FillRectangle(brush,(float)x,(float)y,(float)width,(float)height);
		}

		public void FillRectangle (Brush brush, float x, float y, float width, float height) {
			FillShape(brush,new java.awt.geom.Rectangle2D.Float(x,y,width,height));
		}

		public void FillRectangles (Brush brush, Rectangle [] rects) {
			GraphicsPath path = new GraphicsPath();
			path.AddRectangles(rects);
			FillPath(brush,path);
		}

		public void FillRectangles (Brush brush, RectangleF [] rects) {
			GraphicsPath path = new GraphicsPath();
			path.AddRectangles(rects);
			FillPath(brush,path);
		}
		#endregion

		#region FillRegion
		public void FillRegion (Brush brush, Region region) {
			FillShape(brush,region);
		}

		#endregion

		public void Flush () {
			Flush (FlushIntention.Flush);
		}

		
		public void Flush (FlushIntention intention) {
			if (_image != null)
				_image.NativeObject.flush();
		}

#if INTPTR_SUPPORTED
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public void ReleaseHdc (IntPtr hdc)
		{
			throw new NotImplementedException();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public void ReleaseHdcInternal (IntPtr hdc)
		{
			throw new NotImplementedException ();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]		
		public static Graphics FromHdc (IntPtr hdc)
		{
			throw new NotImplementedException();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static Graphics FromHdc (IntPtr hdc, IntPtr hdevice)
		{
			throw new NotImplementedException ();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static Graphics FromHdcInternal (IntPtr hdc)
		{
			throw new NotImplementedException ();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]		
		public static Graphics FromHwnd (IntPtr hwnd)
		{
			throw new NotImplementedException();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static Graphics FromHwndInternal (IntPtr hwnd)
		{
			throw new NotImplementedException ();
		}

		internal static Graphics FromXDrawable (IntPtr drawable, IntPtr display)
		{
			throw new NotImplementedException();
		}

		public static IntPtr GetHalftonePalette ()
		{
			throw new NotImplementedException ();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public IntPtr GetHdc ()
		{
			throw new NotImplementedException();
		}
#endif
		
		#region GetNearestColor [TODO]
		public Color GetNearestColor (Color color) {
			throw new NotImplementedException();
		}
		#endregion

		#region IntersectClip
		void IntersectClip (geom.Area area) {
			geom.Area clip = GetNativeClip();
			clip.intersect(area);
			SetNativeClip(clip);
		}

		public void IntersectClip (Region region) {
			if (region == null)
				throw new ArgumentNullException("region");

			IntersectClip(region.NativeObject);
		}
		
		public void IntersectClip (RectangleF rect) {
			IntersectClip(new geom.Area(rect.NativeObject));
		}

		public void IntersectClip (Rectangle rect) {			
			IntersectClip(new geom.Area(rect.NativeObject));
		}
		#endregion

		#region IsVisible
		public bool IsVisible (Point point) {
			return IsVisible(point.X,point.Y);
		}

		
		public bool IsVisible (RectangleF rect) {
			return IsVisible ((float)rect.X,(float)rect.Y,(float)rect.Width,(float)rect.Height);
		}

		public bool IsVisible (PointF point) {
			return IsVisible(point.X,point.Y);
		}
		
		public bool IsVisible (Rectangle rect) {
			return IsVisible ((float)rect.X,(float)rect.Y,(float)rect.Width,(float)rect.Height);
		}
		
		public bool IsVisible (float x, float y) {
			java.awt.Graphics2D g = NativeObject;
			java.awt.Shape s = g.getClip();
			if (s == null)
				return true;
			else				
				return s.contains(x,y);
		}
		
		public bool IsVisible (int x, int y) {
			return IsVisible ((float)x,(float)y);
		}
		
		public bool IsVisible (float x, float y, float width, float height) {
			java.awt.Graphics2D g = NativeObject;
			java.awt.Shape s = g.getClip();
			if (s == null)
				return true;
			else				
				return s.contains(x,y,width,height);
		}

		
		public bool IsVisible (int x, int y, int width, int height) {
			return IsVisible ((float)x,(float)y,(float)width,(float)height);
		}
		#endregion

		#region MeasureCharacterRanges [TODO]
		public Region [] MeasureCharacterRanges (string text, Font font, RectangleF layoutRect, StringFormat stringFormat) {
			throw new NotImplementedException();
		}
		#endregion
		
		#region MeasureString [1 method still TODO]
		public SizeF MeasureString (string text, Font font) {
			return MeasureDraw(text,font,null,new RectangleF(0,0,99999f,99999f),null,false);
		}

		
		public SizeF MeasureString (string text, Font font, SizeF layoutArea) {
			return MeasureDraw(text,font,null,new RectangleF(0,0,layoutArea.Width,layoutArea.Height),null,false);
		}

		
		public SizeF MeasureString (string text, Font font, int width) {
			return MeasureDraw(text,font,null,new RectangleF(0,0,(float)width,99999f),null,false);
		}


		public SizeF MeasureString (string text, Font font, SizeF layoutArea, StringFormat format) {
			return MeasureDraw(text,font,null,new RectangleF(0,0,layoutArea.Width,layoutArea.Height),format,false);
		}

		
		public SizeF MeasureString (string text, Font font, int width, StringFormat format) {
			return MeasureDraw(text,font,null,new RectangleF(0,0,(float)width,99999f),format,false);
		}

		
		public SizeF MeasureString (string text, Font font, PointF origin, StringFormat format) {
			//TBD: MeasureDraw still not dealing with clipping region of dc
			return MeasureDraw(text,font,null,new RectangleF(origin.X,origin.Y,99999f,99999f),format,false);
		}

		
		public SizeF MeasureString (string text, Font font, SizeF layoutArea, StringFormat stringFormat, out int charactersFitted, out int linesFilled) {	
			//TBD: charcount
			throw new NotImplementedException();
		}
		#endregion

		#region MultiplyTransform
		public void MultiplyTransform (Matrix matrix) {
			MultiplyTransform (matrix, MatrixOrder.Prepend);
		}

		public void MultiplyTransform (Matrix matrix, MatrixOrder order) {
			ConcatenateTransform(matrix.NativeObject, order);
		}
		#endregion

		#region Reset (Clip and Transform)
		public void ResetClip () {
			java.awt.Graphics2D g = NativeObject;
			g.setClip(null);

		}

		public void ResetTransform () {
			NativeObject.setTransform(new java.awt.geom.AffineTransform(1,0,0,1,0,0));
		}
		#endregion

		public GraphicsState Save () {
			throw new NotImplementedException();
		}

		public void Restore (GraphicsState gstate) {
			throw new NotImplementedException();
		}

		#region RotateTransform
		public void RotateTransform (float angle) {
			RotateTransform (angle, MatrixOrder.Prepend);
		}

		public void RotateTransform (float angle, MatrixOrder order) {
			ConcatenateTransform(
				geom.AffineTransform.getRotateInstance(java.lang.Math.toRadians(angle)),
				order);
		}
		#endregion

		#region ScaleTransform
		public void ScaleTransform (float sx, float sy) {
			ScaleTransform (sx, sy, MatrixOrder.Prepend);
		}

		public void ScaleTransform (float sx, float sy, MatrixOrder order) {
			ConcatenateTransform(
				geom.AffineTransform.getScaleInstance(sx, sy),
				order);
		}
		#endregion

		#region SetClip [Must be reviewed - more abstraction needed]
		public void SetClip (RectangleF rect) {
			SetClip (rect, CombineMode.Replace);
		}

		public void SetClip (GraphicsPath path) {
			SetClip (path, CombineMode.Replace);
		}

		public void SetClip (Rectangle rect) {
			SetClip (rect, CombineMode.Replace);
		}

		public void SetClip (Graphics g) {
			SetClip (g, CombineMode.Replace);
		}

		public void SetClip (Graphics g, CombineMode combineMode) {
			if(g == null)
				throw new NullReferenceException();
			
			SetNativeClip(
				CombineClipArea(g.GetNativeClip(),
				combineMode));
		}

		public void SetClip (Rectangle rect, CombineMode combineMode) {
			SetClip(rect.X,rect.Y,rect.Width,rect.Height,combineMode);			
		}		
		public void SetClip (RectangleF rect, CombineMode combineMode) {			
			SetClip(rect.X,rect.Y,rect.Width,rect.Height,combineMode);			
		}
		
		public void SetClip (Region region, CombineMode combineMode) {
			if(region == null)
				throw new ArgumentNullException("region");

			if(region == Region.InfiniteRegion) {
				SetNativeClip(null);
				return;
			}

			SetNativeClip(CombineClipArea(region.NativeObject,combineMode));
		}
		
		public void SetClip (GraphicsPath path, CombineMode combineMode) {
			if(path == null)
				throw new ArgumentNullException("path");

			SetNativeClip(CombineClipArea(new java.awt.geom.Area(path.NativeObject),combineMode));
		}
		#endregion

		#region Clipping Staff [INTERNAL]
		internal void SetClip(float x,float y,float width,float height,CombineMode combineMode) {					
			SetNativeClip(CombineClipArea(new geom.Area(
				new geom.Rectangle2D.Float(x,y,width,height)),combineMode));
		}

		geom.Area CombineClipArea(java.awt.geom.Area area, CombineMode combineMode) {
			if (combineMode == CombineMode.Replace)
				return area;

			geom.Area curClip = GetNativeClip();
			switch(combineMode) {
				case CombineMode.Complement:
					curClip.add(area);
					break;
				case CombineMode.Exclude:
					curClip.subtract(area);
					break;
				case CombineMode.Intersect:
					curClip.intersect(area);
					break;
				case CombineMode.Union:
					curClip.add(area);
					break;
				case CombineMode.Xor:
					curClip.exclusiveOr(area);
					break;
				default:
					throw new ArgumentOutOfRangeException();					
			}
			
			return curClip;
		}
		
		void SetNativeClip(awt.Shape s) {
			NativeObject.setClip(s);
		}

		geom.Area GetNativeClip() {
			awt.Shape clip = NativeObject.getClip();
			return clip != null ? new geom.Area(clip) : Region.InfiniteRegion.NativeObject;
		}
		#endregion
		
		#region TransformPoints
		public void TransformPoints (CoordinateSpace destSpace, CoordinateSpace srcSpace, PointF [] pts) {
			//TBD:CoordinateSpace
			java.awt.Graphics2D g = NativeObject;
			java.awt.geom.AffineTransform tr = g.getTransform();
			float[] fpts = new float[2];
			for(int i = 0; i< pts.Length; i++) {
				fpts[0] = pts[i].X;
				fpts[1] = pts[i].Y;
				tr.transform(fpts, 0, fpts, 0, 1);
				pts[i].X = fpts[0];
				pts[i].Y = fpts[1];
			}
		}

		public void TransformPoints (CoordinateSpace destSpace, CoordinateSpace srcSpace, Point [] pts) {						
			//TBD:CoordinateSpace
			java.awt.Graphics2D g = NativeObject;
			java.awt.geom.AffineTransform tr = g.getTransform();
			float[] fpts = new float[2];
			for(int i = 0; i< pts.Length; i++) {
				fpts[0] = pts[i].X;
				fpts[1] = pts[i].Y;
				tr.transform(fpts, 0, fpts, 0, 1);
				pts[i].X = (int)fpts[0];
				pts[i].Y = (int)fpts[1];
			}
		}
		#endregion

		#region TranslateClip
		public void TranslateClip (int dx, int dy) {
			TranslateClip((float)dx, (float)dy);
		}

		
		public void TranslateClip (float dx, float dy) {
			geom.Area clip = GetNativeClip();
			clip.transform(geom.AffineTransform.getTranslateInstance(dx, dy));
			SetNativeClip(clip);
		}
		#endregion

		#region TranslateTransform
		public void TranslateTransform (float dx, float dy) {
			TranslateTransform (dx, dy, MatrixOrder.Prepend);
		}

		
		public void TranslateTransform (float dx, float dy, MatrixOrder order) {
			ConcatenateTransform(
				geom.AffineTransform.getTranslateInstance(dx, dy), 
				order);
		}
		#endregion

		#region Properties [Partial TODO]
		public Region Clip {
			get {
				java.awt.Graphics2D g = NativeObject;
				java.awt.Shape s = g.getClip();				
				if(s != null) {
					return new Region(new java.awt.geom.Area(s));
				}
				else {
					return Region.InfiniteRegion.Clone();
				}
			}
			set {
				SetClip (value, CombineMode.Replace);
			}
		}

		public RectangleF ClipBounds {
			get {
				java.awt.Graphics2D g = NativeObject;
				java.awt.Rectangle r = g.getClipBounds();
				RectangleF rect = new RectangleF ((float)r.getX(),(float)r.getY(),(float)r.getWidth(),(float)r.getHeight());
				return rect;
			}
		}

		public CompositingMode CompositingMode {
			//TBD:check this carefully
			get {
				return (NativeObject.getComposite() == awt.AlphaComposite.SrcOver) ?
					CompositingMode.SourceOver : CompositingMode.SourceCopy;
			}
			set {
				NativeObject.setComposite(
					(value == CompositingMode.SourceOver) ?
					awt.AlphaComposite.SrcOver : awt.AlphaComposite.Src);
			}

		}

		public CompositingQuality CompositingQuality {
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		public float DpiX {
			get {				
				return DefaultScreenResolution;
			}
		}

		public float DpiY {
			get {
				//TBD: assume 72 (screen) for now
				return DpiX;
			}
		}

		public InterpolationMode InterpolationMode {
			get {				
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		public bool IsClipEmpty {
			get {
				java.awt.Graphics2D g = NativeObject;
				awt.Shape clip = g.getClip();
				if (clip == null)
					return false;
				return new geom.Area(clip).isEmpty();
			}
		}

		public bool IsVisibleClipEmpty {
			get {
				//TBD:correct this
				return IsClipEmpty;
			}
		}

		public float PageScale {
			get {
				return _pageScale;
			}
			set {
				_pageScale = value;
				UpdateInternalTransform();
			}
		}

		public GraphicsUnit PageUnit {
			get {
				return _pageUnit;
			}
			set {
				_pageUnit = value;
				UpdateInternalTransform();
			}
		}

		internal void UpdateInternalTransform()
		{
			float new_scale = _pageScale * _unitConversion[ (int)PageUnit ];
			Matrix mx = _transform.Clone();

			mx.Scale( new_scale, new_scale, MatrixOrder.Prepend );
			mx.Translate(
				mx.OffsetX * new_scale - mx.OffsetX,
				mx.OffsetY * new_scale - mx.OffsetY, MatrixOrder.Append);

			java.awt.Graphics2D g = NativeObject;
			g.setTransform( mx.NativeObject );
		}

		public PixelOffsetMode PixelOffsetMode {
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		public Point RenderingOrigin {
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		public SmoothingMode SmoothingMode {
			get {
				java.awt.Graphics2D g = NativeObject;

				awt.RenderingHints hints = g.getRenderingHints();
				if(hints.containsKey(java.awt.RenderingHints.KEY_ANTIALIASING) &&
					hints.get(java.awt.RenderingHints.KEY_ANTIALIASING) == 
					java.awt.RenderingHints.VALUE_ANTIALIAS_ON) {
					

					if(hints.containsKey(java.awt.RenderingHints.KEY_RENDERING)) {
						if(hints.get(java.awt.RenderingHints.KEY_RENDERING) == 
							java.awt.RenderingHints.VALUE_RENDER_QUALITY)
							return SmoothingMode.HighQuality;
						if(hints.get(java.awt.RenderingHints.KEY_RENDERING) == 
							java.awt.RenderingHints.VALUE_RENDER_SPEED)
							return SmoothingMode.HighSpeed;
						if(hints.get(java.awt.RenderingHints.KEY_RENDERING) == 
							java.awt.RenderingHints.VALUE_RENDER_DEFAULT)
							return SmoothingMode.AntiAlias;
					}
					return SmoothingMode.AntiAlias;
				}
				return SmoothingMode.Default;

			}

			set {
				java.awt.Graphics2D g = NativeObject;

				awt.RenderingHints hints = g.getRenderingHints();
				if(value ==  SmoothingMode.AntiAlias)
					g.setRenderingHint(java.awt.RenderingHints.KEY_ANTIALIASING,java.awt.RenderingHints.VALUE_ANTIALIAS_ON);
				else
					g.setRenderingHint(java.awt.RenderingHints.KEY_ANTIALIASING,java.awt.RenderingHints.VALUE_ANTIALIAS_OFF);
				//TBD:make it good
			}
		}

		public int TextContrast {
			get {
				//TBD:implement this
				throw new NotImplementedException();
			}

			set {
				//TBD:implement this
				throw new NotImplementedException();
			}
		}

		public TextRenderingHint TextRenderingHint {
			get {
				java.awt.Graphics2D g = NativeObject;

				awt.RenderingHints hints = g.getRenderingHints();
				if(hints.containsKey(java.awt.RenderingHints.KEY_TEXT_ANTIALIASING)) {
					if(hints.get(java.awt.RenderingHints.KEY_TEXT_ANTIALIASING) == 
						java.awt.RenderingHints.VALUE_TEXT_ANTIALIAS_ON)
						return TextRenderingHint.AntiAlias;
					if(hints.get(java.awt.RenderingHints.KEY_TEXT_ANTIALIASING) == 
						java.awt.RenderingHints.VALUE_TEXT_ANTIALIAS_OFF)
						return TextRenderingHint.SingleBitPerPixel;
				}
				return TextRenderingHint.SystemDefault;
			}

			set {
				//TBD: implement this
				throw new NotImplementedException();
			}
		}

		public Matrix Transform {
			get {
				return _transform;
			}
			set {
				_transform = value.Clone();
				UpdateInternalTransform();
			}
		}

		public RectangleF VisibleClipBounds {
			get {
				//TBD: implement this
				throw new NotImplementedException();
			}
		}

		void ConcatenateTransform(geom.AffineTransform transform, MatrixOrder order) {
			geom.AffineTransform at = _transform.NativeObject;
			Matrix.Multiply(at, transform, order);

			UpdateInternalTransform();
		}
		#endregion
	}
}

