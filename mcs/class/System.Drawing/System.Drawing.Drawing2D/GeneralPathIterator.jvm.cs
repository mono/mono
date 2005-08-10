using System;

using java.awt.geom;

namespace System.Drawing.Drawing2D
{
	internal class GeneralPathIterator : PathIterator
	{
		#region Fields

		int typeIdx = 0;
		int pointIdx   = 0;
		ExtendedGeneralPath _path;
		AffineTransform _affine;

		private static readonly int [] curvesize = {2, 2, 4, 6, 0};

		#endregion // Fileds

		#region Constructors

		public GeneralPathIterator(ExtendedGeneralPath _path) : this (_path, null)
		{
		}

		public GeneralPathIterator(ExtendedGeneralPath _path, AffineTransform at) 
		{
			this._path = _path;
			this._affine = at;
		}

		#endregion // Constructors

		#region Methods

		public int getWindingRule() 
		{
			return _path.getWindingRule ();
		}

		public bool isDone() 
		{
			return (typeIdx >= _path.TypesCount);
		}

		public void next() 
		{
			int type = _path.Types [typeIdx++] & ExtendedGeneralPath.SEG_MASK;
			pointIdx += curvesize [type];
		}

		public int currentSegment(float [] coords) {
			int type = _path.Types [typeIdx] & ExtendedGeneralPath.SEG_MASK;
			int numCoords = curvesize [type];
			if (numCoords > 0 && _affine != null)
				_affine.transform (_path.Coords, pointIdx, coords, 0, numCoords/2);
			else
				Array.Copy (_path.Coords, pointIdx, coords, 0, numCoords);
			return type;
		}

		public int currentSegment(double [] coords) 
		{
			int type = _path.Types [typeIdx] & ExtendedGeneralPath.SEG_MASK;
			int numCoords = curvesize [type];
			if (numCoords > 0 && _affine != null)
				_affine.transform (_path.Coords, pointIdx, coords, 0, numCoords/2);
			else 
				for (int i=0; i < numCoords; i++)
					coords [i] = _path.Coords [pointIdx + i];
		
			return type;
		}

		#endregion // Methods
	}  
}
