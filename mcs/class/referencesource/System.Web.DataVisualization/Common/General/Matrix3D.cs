//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		Matrix3D.cs
//
//  Namespace:	DataVisualization.Charting
//
//	Classes:	Matrix3D
//
//  Purpose:	Matrix3D class is used during the 3D drawings to 
//              transform plotting area 3D coordinates into the 2D 
//              projection coordinates based on rotation and 
//              perspective settings.
//
//	Reviewed:	AG - Dec 4, 2002
//              AG - Microsoft 14, 2007
//
//===================================================================

#region Used namespaces

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Collections;

#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting;
	using System.Windows.Forms.DataVisualization.Charting.Data;
	using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
	using System.Windows.Forms.DataVisualization.Charting.Borders3D;

#else
	//using System.Web.UI.DataVisualization.Charting.Utilities;
	//using System.Web.UI.DataVisualization.Charting.Borders3D;
#endif

#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{
	/// <summary>
	/// This class is responsible for all 3D coordinates transformations: Translation, 
	/// Rotation, Scale, Perspective and RightAngle Projection. Translation 
	/// and rotation are stored in composite matrix (mainMatrix), and scaling, 
	/// projection and non-composite translation are stored in private fields. 
	/// Matrix is initialized with Chart Area 3D cube, which is invisible boundary 
	/// cube of 3D Chart area. The matrix has to be initialized every time 
	/// when angles, position or perspective parameters are changed. Method 
	/// TransformPoints will apply 3D Transformation on points using 
	/// Initialization values: Main matrix and other initialization values.
	/// </summary>
	internal class Matrix3D
	{
		#region Enumerations

		/// <summary>
		/// 3D Axis used for rotation
		/// </summary>
		private enum RotationAxis
		{
			/// <summary>
			/// Rotation around X axis.
			/// </summary>
			X,

			/// <summary>
			/// Rotation around Y axis.
			/// </summary>
			Y,

			/// <summary>
			/// Rotation around Z axis.
			/// </summary>
			Z
		}

		#endregion // Enumerations

		#region Fields

		/// <summary>
		/// Composite matrix.
		/// </summary>
		private float [][] _mainMatrix;
		
		/// <summary>
		/// Default translation for chart area cube ( without composition ).
		/// </summary>
		private float _translateX;

		/// <summary>
		/// Default translation for chart area cube ( without composition ).
		/// </summary>
		private float _translateY;

		/// <summary>
		/// Default translation for chart area cube ( without composition ).
		/// </summary>
		private float _translateZ;

		/// <summary>
		/// The value, which is used to rescale chart area.
		/// </summary>
		private float _scale;

		/// <summary>
		/// The value used for Isometric Shift.
		/// </summary>
		private float _shiftX;

		/// <summary>
		/// The value used for Isometric Shift.
		/// </summary>
		private float _shiftY;

		/// <summary>
		/// Perspective value.
		/// </summary>
		internal float _perspective;

		/// <summary>
		/// Isometric projection.
		/// </summary>
		private bool _rightAngleAxis;

		/// <summary>
		/// The value, which is used for perspective.
		/// </summary>
		private float _perspectiveFactor = float.NaN;

		/// <summary>
		/// The value, which is used to set projection plane.
		/// </summary>
		private float _perspectiveZ;

		/// <summary>
		/// X Angle.
		/// </summary>
		private float _angleX;

		/// <summary>
		/// Y Angle.
		/// </summary>
		private float _angleY;

		/// <summary>
		/// Private fields used for lighting
		/// </summary>
		Point3D [] _lightVectors = new Point3D[7];

		/// <summary>
		/// LightStyle Style
		/// </summary>
		LightStyle _lightStyle;

		#endregion // Fields

        #region Properties

        /// <summary>
        /// Gets the X Angle.
        /// </summary>
        internal float AngleX
        {
            get { return _angleX; }
        }

        /// <summary>
        /// Gets the Y Angle.
        /// </summary>
        internal float AngleY
        {
            get { return _angleY; }
        }

        /// <summary>
        /// Get perspective value.
        /// </summary>
        internal float Perspective
        {
            get { return _perspective; } 
        }

        #endregion // Properties

        #region Internal and Public Methods

        /// <summary>
		/// Constructor for Matrix 3D
		/// </summary>
		public Matrix3D()
		{
		}

		/// <summary>
		/// Checks if 3D matrix was initialized.
		/// </summary>
		/// <returns>True if matrix was initialized.</returns>
		public bool IsInitialized()
		{
			return (this._mainMatrix != null);
		}

		/// <summary>
		/// Initialize Matrix 3D. This method calculates how much a chart area 
		/// cube has to be resized to fit Inner Plotting Area rectangle. Order 
		/// of operation is following: Translation for X and Y axes, Rotation 
		/// by X-axis, Rotation by Y-axis and same scaling for all axes. All 
		/// other elements, which belongs to this chart area cube (Data points, 
		/// grid lines etc.) has to follow same order. Translation and rotation 
		/// form composite matrix mainMatrix. Scale has to be allied separately.
		/// </summary>
		/// <param name="innerPlotRectangle">Inner Plotting Area position. Chart area cube has to be inside this rectangle</param>
		/// <param name="depth">Depth of chart area cube</param>
		/// <param name="angleX">Angle of rotation by X axis.</param>
		/// <param name="angleY">Angle of rotation by Y axis.</param>
		/// <param name="perspective">Perspective in percentages</param>
		/// <param name="rightAngleAxis">Right angle flag.</param>
		internal void Initialize( 
			RectangleF innerPlotRectangle, 
			float depth, 
			float angleX, 
			float angleY, 
			float perspective, 
			bool rightAngleAxis )
		{
			// Initialization for mainMatrix
			Reset();

			// Remember non-composite translation
			_translateX = innerPlotRectangle.X+innerPlotRectangle.Width/2;
			_translateY = innerPlotRectangle.Y+innerPlotRectangle.Height/2;
			_translateZ = depth / 2F;
			float width = innerPlotRectangle.Width;
			float height = innerPlotRectangle.Height;
			this._perspective = perspective;
			this._rightAngleAxis = rightAngleAxis;

			// Remember Angles
			this._angleX = angleX;
			this._angleY = angleY;
			
			// Change Degrees to radians.
			angleX = angleX / 180F * (float)Math.PI;
			angleY = angleY / 180F * (float)Math.PI;

			// Set points for 3D Bar which represents 3D Chart Area Cube.
			Point3D [] points = Set3DBarPoints( width, height, depth );
			
			// Translate Chart Area Cube WITH CENTER OF ROTATION - COMPOSITE TRANSLATION.
			Translate( _translateX, _translateY, 0 );

			// Non Isometric projection
			if( !rightAngleAxis )
			{
				// Rotate Chart Area Cube by X axis. 
				Rotate( angleX, RotationAxis.X );

				// Rotate Chart Area Cube by Y axis. 
				Rotate( angleY, RotationAxis.Y );
			}
			else
			{
				if( this._angleY >= 45 )
				{
					// Rotate Chart Area Cube by Y axis. 
					Rotate( Math.PI / 2, RotationAxis.Y );
				}
				else if( this._angleY <= -45 )
				{
					// Rotate Chart Area Cube by Y axis. 
					Rotate( -Math.PI / 2, RotationAxis.Y );
				}
			}
			
			// Apply composed transformation ( Translation and rotation ).
			GetValues( points );

			float maxZ = float.MinValue;

			if( perspective != 0F || rightAngleAxis )
			{
				// Find projection plane
				foreach( Point3D point in points )
				{
					if( point.Z > maxZ )
						maxZ = point.Z;
				}

				// Set Projection plane
				_perspectiveZ = maxZ;
			}

			if( perspective != 0F )
			{
				_perspectiveFactor = perspective / 2000F;

				// Apply perspective
				ApplyPerspective( points );
			}
				
			// Isometric projection is active
			if( rightAngleAxis )
			{
				RightAngleProjection( points );

				float minX = 0F;
				float minY = 0F;
				float maxX = 0F;
				float maxY = 0F;

				// Point loop
				foreach( Point3D point in points )
				{
					if( point.X - _translateX < 0F  && Math.Abs( point.X - _translateX ) > minX )
						minX = Math.Abs( point.X - _translateX );
					
					if( point.X - _translateX >=0F  && Math.Abs( point.X - _translateX ) > maxX )
						maxX = Math.Abs( point.X - _translateX );

					if( point.Y - _translateY < 0F  && Math.Abs( point.Y - _translateY ) > minY )
						minY = Math.Abs( point.Y - _translateY );
					
					if( point.Y - _translateY >=0F  && Math.Abs( point.Y - _translateY ) > maxY )
						maxY = Math.Abs( point.Y - _translateY );
				}

				_shiftX = (maxX - minX)/2F;
				_shiftY = (maxY - minY)/2F;
				RightAngleShift( points );
			}
								
			// This code searches for value, which will be used for scaling.
			float maxXScale = float.MinValue;
			float maxYScale = float.MinValue;

			foreach( Point3D point in points )
			{
				// Find maximum relative distance for X axis.
				// Relative distance is (distance from the center of plotting area 
				// position) / (distance from the edge of rectangle to 
				// the center of the rectangle).
				if( maxXScale < Math.Abs(point.X - _translateX) / width * 2 )
					maxXScale = Math.Abs(point.X - _translateX) / width * 2;
				
				// Find maximum relative distance for Y axis.
				if( maxYScale < Math.Abs(point.Y - _translateY) / height * 2 )
					maxYScale = Math.Abs(point.Y - _translateY) / height * 2;
			}

			// Remember scale factor
			_scale = (maxYScale > maxXScale ) ? maxYScale : maxXScale;

			// Apply scaling
			Scale( points );
			
		}

		/// <summary>
		/// Apply transformations on array od 3D Points. Order of operation is 
		/// following: Translation ( Set coordinate system for 0:100 to -50:50 
		/// Center of rotation is always 0), Composite Translation for X and Y 
		/// axes ( Moving center of rotation ), Rotation by X-axis, Rotation 
		/// by Y-axis, perspective and same scaling for all axes.
		/// </summary>
		/// <param name="points">3D Points array.</param>
		public void TransformPoints( Point3D[] points )
		{
			TransformPoints( points, true );
		}
#if RS_DEADCODE
		/// <summary>
		/// This Method returns scale factor
		/// </summary>
		/// <returns></returns>
		internal float GetScale()
		{
			return scale;
		}
#endif //RS_DEADCODE
		#endregion // Internal and Public Methods

		#region Private Methods

		/// <summary>
		/// Apply transformations on array od 3D Points. Order of operation is 
		/// following: Translation ( Set coordinate system for 0:100 to -50:50 
		/// Center of rotation is always 0), Composite Translation for X and Y 
		/// axes ( Moving center of rotation ), Rotation by X-axis, Rotation 
		/// by Y-axis, perspective and same scaling for all axes.
		/// </summary>
		/// <param name="points">3D Points array.</param>
		/// <param name="withPerspective">Applay Perspective</param>
		private void TransformPoints( Point3D[] points, bool withPerspective )
		{
			// Matrix is not initialized.
			if( _mainMatrix == null )
			{
                throw new InvalidOperationException(SR.ExceptionMatrix3DNotinitialized);
			}

			// Translate point. CENTER OF ROTATION is 0 and that center is in 
			// the middle of chart area 3D CUBE. Translate method cannot 
			// be used because composite translation WILL MOVE 
			// CENTER OF ROTATION.
			foreach( Point3D point in points )
			{
				point.X -= _translateX;
				point.Y -= _translateY;
				point.Z -= _translateZ;
			}
		
			// Transform points using composite mainMatrix. (Translation of points together with 
			// Center of rotation and rotations by X and Y axes).
			GetValues( points );

			// Apply perspective
			if( _perspective != 0F && withPerspective )
			{
				ApplyPerspective( points );
			}
			
			// RightAngle Projection
			if( _rightAngleAxis )
			{
				RightAngleProjection( points );
				RightAngleShift( points );
			}

			// Scales data points. Scaling has to be performed SEPARATELY from 
			// composite matrix. If scale is used with composite matrix after 
			// rotation, scaling will deform object.
			Scale( points );
		}
		
		/// <summary>
		/// This method adjusts a position of 3D Chart Area cube. This 
		/// method will translate chart for better use of the inner 
		/// plotting area. Center of rotation is shifted for 
		/// right Angle projection.
		/// </summary>
		/// <param name="points">3D Points array.</param>
		private void RightAngleShift( Point3D [] points )
		{
			foreach( Point3D point in points )
			{
				point.X = point.X - _shiftX;   
				point.Y = point.Y - _shiftY;   
			}
		}

		/// <summary>
		/// Method used to calculate right Angle projection.
		/// </summary>
		/// <param name="points">3D points array.</param>
		private void RightAngleProjection( Point3D [] points )
		{
			float coorectionAngle = 45F;
		
			float xFactor = this._angleX / 45;

			float yFactor;
			
			if( this._angleY >= 45 )
			{
				yFactor = (this._angleY - 90) / coorectionAngle;
			}
			else if ( this._angleY <= -45 )
			{
				yFactor = ( this._angleY + 90 ) / coorectionAngle;
			}
			else
			{
				yFactor = this._angleY / coorectionAngle;
			}
			
			// Projection formula
			// perspectiveZ - Position of perspective plain.
			// Perspective Factor - Intensity of projection.
			foreach( Point3D point in points )
			{
				point.X = point.X + ( _perspectiveZ - point.Z ) * yFactor;   
				point.Y = point.Y - ( _perspectiveZ - point.Z ) * xFactor;  
			}
		}

		/// <summary>
		/// Method is used for Planar Geometric projection. 
		/// </summary>
		/// <param name="points">3D Points array.</param>
		private void ApplyPerspective( Point3D [] points )
		{
			// Projection formula
			// perspectiveZ - Position of perspective plain.
			// perspectiveFactor - Intensity of projection.
			foreach( Point3D point in points )
			{
				point.X = _translateX + (point.X - _translateX) / ( 1 + (_perspectiveZ - point.Z) * _perspectiveFactor);   
				point.Y = _translateY + (point.Y - _translateY) / ( 1 + (_perspectiveZ - point.Z) * _perspectiveFactor); 
			}
		}

		/// <summary>
		/// Scales data points. Scaling has to be performed SEPARATELY from 
		/// composite matrix. If scale is used with composite matrix after 
		/// rotation, scaling will deform object.
		/// </summary>
		/// <param name="points">3D Points array.</param>
		private void Scale( Point3D [] points )
		{
			foreach( Point3D point in points )
			{
				point.X = _translateX + (point.X - _translateX) / _scale; 
				point.Y = _translateY + (point.Y - _translateY) / _scale; 
			}
		}

		/// <summary>
		/// Prepend to this Matrix object a translation. This method is used 
		/// only if CENTER OF ROTATION HAS TO BE MOVED.
		/// </summary>
		/// <param name="dx">Translate in x axis direction.</param>
		/// <param name="dy">Translate in y axis direction.</param>
		/// <param name="dz">Translate in z axis direction.</param>
		private void Translate( float dx, float dy, float dz )
		{
			float [][] translationMatrix = new float[4][];
			translationMatrix[0] = new float[4];
			translationMatrix[1] = new float[4];
			translationMatrix[2] = new float[4];
			translationMatrix[3] = new float[4];

			// Matrix initialization
			// Row loop
			for( int row = 0; row < 4; row ++ )
			{
				// Column loop
				for( int column = 0; column < 4; column ++ )
				{
					// For initialization: Diagonal matrix elements are equal to one 
					// and all other elements are equal to zero.
					if( row == column )
					{
						translationMatrix[row][column] = 1F;
					}
					else
					{
						translationMatrix[row][column] = 0F;
					}
				}
			}
		
			// Set translation values to the matrix
			translationMatrix[0][3] = dx;
			translationMatrix[1][3] = dy;
			translationMatrix[2][3] = dz;
		
			// Translate main Matrix
			Multiply( translationMatrix, MatrixOrder.Prepend, true );
		
		}

		/// <summary>
		/// This method initialize and set default values for mainMatrix ( there is no rotation and translation )
		/// </summary>
		private void Reset()
		{
			// First element is row and second element is column !!!
			_mainMatrix = new float[4][];
			_mainMatrix[0] = new float[4];
			_mainMatrix[1] = new float[4];
			_mainMatrix[2] = new float[4];
			_mainMatrix[3] = new float[4];

			// Matrix initialization
			// Row loop
			for( int row = 0; row < 4; row ++ )
			{
				// Column loop
				for( int column = 0; column < 4; column ++ )
				{
					// For initialization: Diagonal matrix elements are equal to one 
					// and all other elements are equal to zero.
					if( row == column )
					{
						_mainMatrix[row][column] = 1F;
					}
					else
					{
						_mainMatrix[row][column] = 0F;
					}
				}
			}
		}


		/// <summary>
		/// Multiplies this Matrix object by the matrix specified in the 
        /// matrix parameter, and in the order specified in the order parameter.
		/// </summary>
		/// <param name="mulMatrix">The Matrix object by which this Matrix object is to be multiplied.</param>
		/// <param name="order">The MatrixOrder enumeration that represents the order of the multiplication. If the specified order is MatrixOrder.Prepend, this Matrix object is multiplied by the specified matrix in a prepended order. If the specified order is MatrixOrder.Append, this Matrix object is multiplied by the specified matrix in an appended order.</param>
		/// <param name="setMainMatrix">Set main matrix to be result of multiplication</param>
		/// <returns>Matrix multiplication result.</returns>
		private float[][] Multiply( float [][] mulMatrix, MatrixOrder order, bool setMainMatrix )
		{
			// A matrix which is result of matrix multiplication
			// of mulMatrix and mainMatrix
			float [][] resultMatrix = new float[4][];
			resultMatrix[0] = new float[4];
			resultMatrix[1] = new float[4];
			resultMatrix[2] = new float[4];
			resultMatrix[3] = new float[4];

			// Row loop
			for( int row = 0; row < 4; row ++ )
			{
				// Column loop
				for( int column = 0; column < 4; column ++ )
				{
					// Initialize element
					resultMatrix[row][column ] = 0F;
					for( int sumIndx = 0; sumIndx < 4; sumIndx ++ )
					{
						// Find matrix element
						if( order == MatrixOrder.Prepend )
						{
							// Order of matrix multiplication
							resultMatrix[row][column ] += _mainMatrix[row][sumIndx ] * mulMatrix[sumIndx][column];
						}
						else
						{
							// Order of matrix multiplication
							resultMatrix[row][column] += mulMatrix[row][sumIndx] * _mainMatrix[sumIndx][column];
						}
					}
				}
			}

			// Set result matrix to be main matrix
			if( setMainMatrix )
			{
				_mainMatrix = resultMatrix;
			}

			return resultMatrix;
		}


		/// <summary>
		/// Multiplies this Matrix object by the Vector specified in the 
		/// vector parameter.
		/// </summary>
		/// <param name="mulVector">The vector object by which this Matrix object is to be multiplied.</param>
		/// <param name="resultVector">Vector which is result of matrix and vector multiplication.</param>
		private void MultiplyVector( float [] mulVector, ref float [] resultVector )
		{
			// Row loop
			for( int row = 0; row < 3; row ++ )
			{
				// Initialize element
				resultVector[ row ] = 0F;

				// Column loop
				for( int column = 0; column < 4; column ++ )
				{
					// Find matrix element
					resultVector[ row ] += _mainMatrix[row][column] * mulVector[ column ];
				}
			}
		}

		/// <summary>
		/// Prepend to this Matrix object a clockwise rotation, around the axis and by the specified angle.
		/// </summary>
		/// <param name="angle">Angle to rotate</param>
		/// <param name="axis">Axis used for rotation</param>
		private void Rotate( double angle, RotationAxis axis )
		{
			float [][] rotationMatrix = new float[4][];
			rotationMatrix[0] = new float[4];
			rotationMatrix[1] = new float[4];
			rotationMatrix[2] = new float[4];
			rotationMatrix[3] = new float[4];

			// Change angle direction
			angle = -1F * angle;

			// Matrix initialization
			// Row loop
			for( int row = 0; row < 4; row ++ )
			{
				// Column loop
				for( int column = 0; column < 4; column ++ )
				{
					// For initialization: Diagonal matrix elements are equal to one 
					// and all other elements are equal to zero.
					if( row == column )
					{
						rotationMatrix[row][column] = 1F;
					}
					else
					{
						rotationMatrix[row][column] = 0F;
					}
				}
			}

			// Rotation about axis
			switch( axis )
			{
					// Rotation about X axis
				case RotationAxis.X:
					rotationMatrix[1][1] = (float)Math.Cos( angle );
					rotationMatrix[1][2] = (float)-Math.Sin( angle );
					rotationMatrix[2][1] = (float)Math.Sin( angle );
					rotationMatrix[2][2] = (float)Math.Cos( angle );
					break;

					// Rotation about Y axis
				case RotationAxis.Y:
					rotationMatrix[0][0] = (float)Math.Cos( angle );
					rotationMatrix[0][2] = (float)Math.Sin( angle );
					rotationMatrix[2][0] = (float)-Math.Sin( angle );
					rotationMatrix[2][2] = (float)Math.Cos( angle );
					break;

					// Rotation about Z axis
				case RotationAxis.Z:
					rotationMatrix[0][0] = (float)Math.Cos( angle );
					rotationMatrix[0][1] = (float)-Math.Sin( angle );
					rotationMatrix[1][0] = (float)Math.Sin( angle );
					rotationMatrix[1][1] = (float)Math.Cos( angle );
					break;

			}

			// Rotate Main matrix
			Multiply( rotationMatrix, MatrixOrder.Prepend, true );
		
		}

		/// <summary>
		/// Returns transformed x and y values from x, y and z values 
		/// and composed main matrix values (All rotations, 
		/// translations and scaling).
		/// </summary>
		/// <param name="points">Array of 3D points.</param>
		private void GetValues( Point3D [] points )
		{
			// Create one dimensional matrix (vector)
			float [] inputVector = new float[4];

			// A vector which is result of matrix and vector multiplication
			float [] resultVector = new float[4];
		
			foreach( Point3D point in points )
			{
				// Fill input vector with x, y and z coordinates
				inputVector[0] = point.X;
				inputVector[1] = point.Y;
				inputVector[2] = point.Z;
				inputVector[3] = 1;
		
				// Apply 3D transformations.
				MultiplyVector( inputVector, ref resultVector );

				// Return x and y coordinates.
				point.X = resultVector[0];
				point.Y = resultVector[1];
				point.Z = resultVector[2];
			}
		}


		/// <summary>
		/// Set points for 3D Bar which represents 3D Chart Area.
		/// </summary>
		/// <param name="dx">Width of the bar 3D.</param>
		/// <param name="dy">Height of the bar 3D.</param>
		/// <param name="dz">Depth of the bar 3D.</param>
		/// <returns>Collection of Points 3D.</returns>
		private Point3D [] Set3DBarPoints( float dx, float dy, float dz )
		{
			Point3D [] points = new Point3D[8];

			// ********************************************
			// 3D Bar side: Front
			// ********************************************
			points[0] = new Point3D(-dx/2, -dy/2, dz/2);
			points[1] = new Point3D(dx/2, -dy/2, dz/2);
			points[2] = new Point3D(dx/2, dy/2, dz/2);
			points[3] = new Point3D(-dx/2, dy/2, dz/2);
			
			// ********************************************
			// 3D Bar side: Back
			// ********************************************
			points[4] = new Point3D(-dx/2, -dy/2, -dz/2);
			points[5] = new Point3D(dx/2, -dy/2, -dz/2);
			points[6] = new Point3D(dx/2, dy/2, -dz/2);
			points[7] = new Point3D(-dx/2, dy/2, -dz/2);

			return points;
		}

		#endregion // Private Methods
		
		#region Lighting Methods

		/// <summary>
		/// Initial Lighting. Use matrix transformation only once 
		/// for Normal vectors.
		/// </summary>
		/// <param name="lightStyle">LightStyle Style</param>
		internal void InitLight( LightStyle lightStyle )
		{
			// Set LightStyle Style
			this._lightStyle = lightStyle;
										
			// Center of rotation
			_lightVectors[0] = new Point3D( 0F, 0F, 0F );

			// Front side normal Vector.
			_lightVectors[1] = new Point3D( 0F, 0F, 1F );

			// Back side normal Vector.
			_lightVectors[2] = new Point3D( 0F, 0F, -1F );

			// Left side normal Vector.
			_lightVectors[3] = new Point3D( -1F, 0F, 0F );

			// Right side normal Vector.
			_lightVectors[4] = new Point3D( 1F, 0F, 0F );

			// Top side normal Vector.
			_lightVectors[5] = new Point3D( 0F, -1F, 0F );

			// Bottom side normal Vector.
			_lightVectors[6] = new Point3D( 0F, 1F, 0F );

			// Apply matrix transformations
			TransformPoints( _lightVectors, false );

			// ********************************************************
			// LightStyle Vector and normal vectors have to have same center. 
			// Shift Normal vectors.
			// ********************************************************

			// Front Side shift
			_lightVectors[1].X -= _lightVectors[0].X;
			_lightVectors[1].Y -= _lightVectors[0].Y;
			_lightVectors[1].Z -= _lightVectors[0].Z;

			// Back Side shift
			_lightVectors[2].X -= _lightVectors[0].X;
			_lightVectors[2].Y -= _lightVectors[0].Y;
			_lightVectors[2].Z -= _lightVectors[0].Z;

			// Left Side shift
			_lightVectors[3].X -= _lightVectors[0].X;
			_lightVectors[3].Y -= _lightVectors[0].Y;
			_lightVectors[3].Z -= _lightVectors[0].Z;

			// Right Side shift
			_lightVectors[4].X -= _lightVectors[0].X;
			_lightVectors[4].Y -= _lightVectors[0].Y;
			_lightVectors[4].Z -= _lightVectors[0].Z;

			// Top Side shift
			_lightVectors[5].X -= _lightVectors[0].X;
			_lightVectors[5].Y -= _lightVectors[0].Y;
			_lightVectors[5].Z -= _lightVectors[0].Z;

			// Bottom Side shift
			_lightVectors[6].X -= _lightVectors[0].X;
			_lightVectors[6].Y -= _lightVectors[0].Y;
			_lightVectors[6].Z -= _lightVectors[0].Z;

		}

		/// <summary>
		/// Return intensity of lightStyle for 3D Cube. There are tree types of lights: None, 
		/// Simplistic and Realistic. None Style have same lightStyle intensity on 
		/// all polygons. Normal vector doesn’t have influence on this type 
		/// of lighting. Simplistic style have lightStyle source, which is 
		/// rotated together with scene. Realistic lighting have fixed lightStyle 
		/// source and intensity of lightStyle is change when scene is rotated.
		/// </summary>
		/// <param name="surfaceColor">Color used for polygons without lighting</param>
		/// <param name="front">Color corrected with intensity of lightStyle for Front side of the 3D Rectangle</param>
		/// <param name="back">Color corrected with intensity of lightStyle for Back side of the 3D Rectangle</param>
		/// <param name="left">Color corrected with intensity of lightStyle for Left side of the 3D Rectangle</param>
		/// <param name="right">Color corrected with intensity of lightStyle for Right side of the 3D Rectangle</param>
		/// <param name="top">Color corrected with intensity of lightStyle for Top side of the 3D Rectangle</param>
		/// <param name="bottom">Color corrected with intensity of lightStyle for Bottom side of the 3D Rectangle</param>
		internal void GetLight( Color surfaceColor, out Color front, out Color back, out Color left, out Color right, out Color top, out Color bottom )
		{
			switch( _lightStyle )
			{
				// LightStyle style is None
				case  LightStyle.None:
				{
					front = surfaceColor;
					left = surfaceColor;
					top = surfaceColor;
					back = surfaceColor;
					right = surfaceColor;
					bottom = surfaceColor;
					break;
				}
				// LightStyle style is Simplistic
				case  LightStyle.Simplistic:
				{
					front = surfaceColor;
					left = ChartGraphics.GetGradientColor( surfaceColor, Color.Black, 0.25);
					top = ChartGraphics.GetGradientColor( surfaceColor, Color.Black, 0.15);
					back = surfaceColor;
					right = ChartGraphics.GetGradientColor( surfaceColor, Color.Black, 0.25);
					bottom = ChartGraphics.GetGradientColor( surfaceColor, Color.Black, 0.15);
					break;
				}
				// LightStyle style is Realistic
				default:
				{
										
					// For Right Axis angle Realistic lightStyle should be different
					if( _rightAngleAxis )
					{
						// LightStyle source Vector
						Point3D lightSource = new Point3D( 0F, 0F, -1F );
						Point3D [] rightPRpoints = new Point3D[1];
						rightPRpoints[0] = lightSource;
						RightAngleProjection(rightPRpoints);

						// ******************************************************************
						// Color correction. Angle between Normal vector of polygon and 
						// vector of lightStyle source is used.
						// ******************************************************************
						if( this._angleY >= 45 || this._angleY <= -45 )
						{
							front = ChartGraphics.GetGradientColor( surfaceColor, Color.Black, GetAngle(lightSource,_lightVectors[1])/Math.PI );

							back = ChartGraphics.GetGradientColor( surfaceColor, Color.Black, GetAngle(lightSource,_lightVectors[2])/Math.PI );
							
							left = ChartGraphics.GetGradientColor( surfaceColor, Color.Black, 0 );

							right = ChartGraphics.GetGradientColor( surfaceColor, Color.Black, 0 );
						}
						else
						{
							front = ChartGraphics.GetGradientColor( surfaceColor, Color.Black, 0 );

							back = ChartGraphics.GetGradientColor( surfaceColor, Color.Black, 1 );

							left = ChartGraphics.GetGradientColor( surfaceColor, Color.Black, GetAngle(lightSource,_lightVectors[3])/Math.PI );

							right = ChartGraphics.GetGradientColor( surfaceColor, Color.Black, GetAngle(lightSource,_lightVectors[4])/Math.PI );
						}
			
						top = ChartGraphics.GetGradientColor( surfaceColor, Color.Black, GetAngle(lightSource,_lightVectors[5])/Math.PI );

						bottom = ChartGraphics.GetGradientColor( surfaceColor, Color.Black, GetAngle(lightSource,_lightVectors[6])/Math.PI );
					}
					else
					{
						// LightStyle source Vector
						Point3D lightSource = new Point3D( 0F, 0F, 1F );

						// ******************************************************************
						// Color correction. Angle between Normal vector of polygon and 
						// vector of lightStyle source is used.
						// ******************************************************************
						front = GetBrightGradientColor( surfaceColor, GetAngle(lightSource,_lightVectors[1])/Math.PI );

						back = GetBrightGradientColor( surfaceColor, GetAngle(lightSource,_lightVectors[2])/Math.PI );

						left = GetBrightGradientColor( surfaceColor, GetAngle(lightSource,_lightVectors[3])/Math.PI );

						right = GetBrightGradientColor( surfaceColor, GetAngle(lightSource,_lightVectors[4])/Math.PI );
			
						top = GetBrightGradientColor( surfaceColor, GetAngle(lightSource,_lightVectors[5])/Math.PI );

						bottom = GetBrightGradientColor( surfaceColor, GetAngle(lightSource,_lightVectors[6])/Math.PI );
					}

					break;
				}
			}
		}
		

		/// <summary>
		/// Return intensity of lightStyle for Polygons. There are tree types of lights: None, 
		/// Simplistic and Realistic. None Style have same lightStyle intensity on 
		/// all polygons. Normal vector doesn’t have influence on this type 
		/// of lighting. Simplistic style have lightStyle source, which is 
		/// rotated together with scene. Realistic lighting have fixed lightStyle 
		/// source and intensity of lightStyle is change when scene is rotated.
		/// </summary>
		/// <param name="points">Points of the polygon</param>
		/// <param name="surfaceColor">Color used for polygons without lighting</param>
		/// <param name="visiblePolygon">This flag gets information if  polygon is visible or not.</param>
		/// <param name="rotation">Y angle ( from -90 to 90 ) Should be used width switchSeriesOrder to get from -180 to 180</param>
		/// <param name="surfaceName">Used for lighting of front - back and left - right sides</param>
		/// <param name="switchSeriesOrder">Used to calculate real y angle</param>
		/// <returns>Color corrected with intensity of lightStyle</returns>
        internal Color GetPolygonLight(Point3D[] points, Color surfaceColor, bool visiblePolygon, float rotation, SurfaceNames surfaceName, bool switchSeriesOrder)
		{
			// Corrected color
			Color color = surfaceColor;

			// Direction of lightStyle source
			Point3D lightSource;
			lightSource = new Point3D( 0F, 0F, 1F );

			// There are tree different lightStyle styles: None, Simplistic and realistic.
			switch( _lightStyle )
			{
				// LightStyle style is None
				case  LightStyle.None:
				{
					// Use same color
					break;
				}
				// LightStyle style is Simplistic
				case  LightStyle.Simplistic:
				{
					// Find two vectors of polygon
					Point3D firstVector = new Point3D();
					firstVector.X = points[0].X - points[1].X;
					firstVector.Y = points[0].Y - points[1].Y;
					firstVector.Z = points[0].Z - points[1].Z;

					Point3D secondVector = new Point3D();
					secondVector.X = points[2].X - points[1].X;
					secondVector.Y = points[2].Y - points[1].Y;
					secondVector.Z = points[2].Z - points[1].Z;

					// Find Normal vector for Polygon
					Point3D normalVector = new Point3D();
					normalVector.X = firstVector.Y * secondVector.Z - firstVector.Z * secondVector.Y;
					normalVector.Y = firstVector.Z * secondVector.X - firstVector.X * secondVector.Z;
					normalVector.Z = firstVector.X * secondVector.Y - firstVector.Y * secondVector.X;
					
					// Polygon is left side ( like side of area chart )
					if( surfaceName == SurfaceNames.Left )
					{
						color = ChartGraphics.GetGradientColor( surfaceColor, Color.Black, 0.15);
					}
					// Polygon is right side ( like side of area chart )
					else if( surfaceName == SurfaceNames.Right )
					{
						color = ChartGraphics.GetGradientColor( surfaceColor, Color.Black, 0.15);
					}
					// Polygon is front side ( like side of area chart )
					else if( surfaceName == SurfaceNames.Front )
					{
						color = surfaceColor;
					}
					// Polygon is back side ( like side of area chart )
					else if( surfaceName == SurfaceNames.Back )
					{
						color = surfaceColor;
					}
					// Polygon has angle with bottom side ( Line chart or top of area chart )
					else
					{
						float angleLeft;
						float angleRight;

						// Find angles between lightStyle and polygon for different y-axis angles.
						if( switchSeriesOrder )
						{
                            if (rotation > 0 && rotation <= 90)
							{
								angleLeft = GetAngle( normalVector, _lightVectors[3] );
								angleRight = GetAngle( normalVector, _lightVectors[4] );
							}
							else
							{
								angleLeft = GetAngle( normalVector, _lightVectors[4] );
								angleRight = GetAngle( normalVector, _lightVectors[3] );
							}
						}
						else
						{
                            if (rotation > 0 && rotation <= 90)
							{
								angleLeft = GetAngle( normalVector, _lightVectors[4] );
								angleRight = GetAngle( normalVector, _lightVectors[3] );
							}
							else
							{
								angleLeft = GetAngle( normalVector, _lightVectors[3] );
								angleRight = GetAngle( normalVector, _lightVectors[4] );
							}
						}

						if( Math.Abs( angleLeft - angleRight ) < 0.01 )
						{
							color = ChartGraphics.GetGradientColor( surfaceColor, Color.Black, 0.25);
						}
						else if( angleLeft < angleRight )
						{
							color = ChartGraphics.GetGradientColor( surfaceColor, Color.Black, 0.25);
						}
						else
						{
							color = ChartGraphics.GetGradientColor( surfaceColor, Color.Black, 0.15);
						}
					}

					break;
				}
				// LightStyle style is Realistic
				default:
				{

					// Find two vectors of polygon
					Point3D firstVector = new Point3D();
					firstVector.X = points[0].X - points[1].X;
					firstVector.Y = points[0].Y - points[1].Y;
					firstVector.Z = points[0].Z - points[1].Z;

					Point3D secondVector = new Point3D();
					secondVector.X = points[2].X - points[1].X;
					secondVector.Y = points[2].Y - points[1].Y;
					secondVector.Z = points[2].Z - points[1].Z;

					// Find Normal vector for Polygon
					Point3D normalVector = new Point3D();
					normalVector.X = firstVector.Y * secondVector.Z - firstVector.Z * secondVector.Y;
					normalVector.Y = firstVector.Z * secondVector.X - firstVector.X * secondVector.Z;
					normalVector.Z = firstVector.X * secondVector.Y - firstVector.Y * secondVector.X;

					// ******************************************************************
					// Color correction. Angle between Normal vector of polygon and 
					// vector of lightStyle source is used.
					// ******************************************************************
					if( surfaceName == SurfaceNames.Front )
					{
						lightSource.Z *= -1;
						color = GetBrightGradientColor( surfaceColor, GetAngle(lightSource,_lightVectors[2])/Math.PI );
					}
					else if( surfaceName == SurfaceNames.Back )
					{
						lightSource.Z *= -1;
						color = GetBrightGradientColor( surfaceColor, GetAngle(lightSource,_lightVectors[1])/Math.PI );
					}
					else
					{
						if( visiblePolygon )
						{
							lightSource.Z *= -1;
						}

						color = GetBrightGradientColor( surfaceColor, GetAngle(lightSource,normalVector)/Math.PI );
					}

					break;
				}
			}
			return color;
			
		}

		/// <summary>
		/// This method creates gradien color with brightnes.
		/// </summary>
		/// <param name="beginColor">Start color for gradient.</param>
		/// <param name="position">Position used between Start and end color.</param>
		/// <returns>Calculated Gradient color from gradient position</returns>
		private Color GetBrightGradientColor( Color beginColor, double position )
		{
			position = position * 2;
			double brightness = 0.5;
			if( position < brightness )
			{
				return ChartGraphics.GetGradientColor( Color.FromArgb(beginColor.A,255,255,255), beginColor, 1 - brightness + position );
			}
			else if( -brightness + position < 1 )
			{
				return ChartGraphics.GetGradientColor( beginColor, Color.Black, -brightness + position );
			}
			else
			{
				return Color.FromArgb( beginColor.A, 0, 0, 0 );
			}
		}

		/// <summary>
		/// Returns the angle between two 3D vectors (a and b); 
		/// </summary>
		/// <param name="a">First vector</param>
		/// <param name="b">Second Vector</param>
		/// <returns>Angle between vectors</returns>
		private float GetAngle(Point3D a,Point3D b)
		{
			double angle;

			angle = Math.Acos( ( a.X * b.X + a.Y * b.Y + a.Z * b.Z ) / ( Math.Sqrt( a.X * a.X + a.Y * a.Y + a.Z * a.Z ) * Math.Sqrt( b.X * b.X + b.Y * b.Y + b.Z * b.Z ) ) );

			return (float)angle;
		}

		#endregion
	}
}
