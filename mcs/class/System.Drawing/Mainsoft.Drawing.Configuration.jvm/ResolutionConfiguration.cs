using System;
using System.Collections;

namespace Mainsoft.Drawing.Configuration
{
	/// <summary>
	/// Summary description for ResolutionConfiguration.
	/// </summary>
	public class ResolutionConfiguration : IComparable
	{
		string _imageFormat = "";

		string _xResPath = "";
		string _yResPath = "";
		string _unitsTypePath = "";

		string _xResDefault = "";
		string _yResDefault = "";
		string _unitsTypeDefault = "";

		Hashtable _unitScale;

		public ResolutionConfiguration(
			string imageFormat,
			string xresPath, string yresPath, string unitsTypePath,
			string xresDefault, string yresDefault, string unitsTypeDefault,
			Hashtable unitScale)
		{
			_imageFormat = imageFormat;

			_xResPath = xresPath;
			_yResPath = yresPath;
			_unitsTypePath = unitsTypePath;

			_xResDefault = xresDefault;
			_yResDefault = yresDefault;
			_unitsTypeDefault = unitsTypeDefault;

			_unitScale = unitScale;
		}

		public string XResPath {
			get { return _xResPath; }
		}
		public string XResDefault {
			get { return _xResDefault; }
		}
		public string YResPath {
			get { return _yResPath; }
		}
		public string YResDefault {
			get { return _yResDefault; }
		}
		public string UnitsTypePath {
			get { return _unitsTypePath; }
		}
		public string UnitsTypeDefault {
			get { return _unitsTypeDefault; }
		}
		public string ImageFormat {
			get { return _imageFormat; }
		}
		public Hashtable UnitsScale {
			get { return _unitScale; }
		}

		#region IComparable Members

		public int CompareTo(object obj) {
			return _imageFormat.CompareTo(((ResolutionConfiguration)obj).ImageFormat);
		}

		#endregion

	}
}
