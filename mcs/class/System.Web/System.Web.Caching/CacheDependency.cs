// 
// System.Web.Caching
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
// (C) Copyright Patrik Torstensson, 2001
//
namespace System.Web.Caching
{
	/// <summary>
	/// Class to handle cache dependency, right now this class is only a mookup
	/// </summary>
	public sealed class CacheDependency : System.IDisposable
	{
		private bool _boolDisposed;

		public CacheDependency() 
		{
			_boolDisposed = false;
		}

		/// <remarks>
		/// Added by gvaish@iitk.ac.in
		/// </remarks>
		[MonoTODO("Constructor")]
		public CacheDependency(string filename)
		{
		}
		
		/// <remarks>
		/// Added by gvaish@iitk.ac.in
		/// </remarks>
		[MonoTODO("Constructor")]
		public CacheDependency(string[] filenames, string[] cachekeys)
		{
		}

		public delegate void CacheDependencyCallback(CacheDependency objDependency);
		
		public event CacheDependencyCallback Changed;

		public void OnChanged()
		{
			if (_boolDisposed)
			{
				throw new System.ObjectDisposedException("System.Web.CacheDependency");
			}

			if (Changed != null)
			{
				Changed(this);
			}
		}

		public bool IsDisposed
		{
			get 
			{
				return _boolDisposed;
			}
		}

		public bool HasEvents
		{
			get 
			{
				if (_boolDisposed)
				{
					throw new System.ObjectDisposedException("System.Web.CacheDependency");
				}

				if (Changed != null)
				{
					return true;
				}

				return false;
			}
		}

		public void Dispose()
		{
			_boolDisposed = true;
		}

		/// <summary>
		/// Used in testing.
		/// </summary>
		public void Signal()
		{
			OnChanged();
		}
	}
}
