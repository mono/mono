//
// System.Windows.Forms.UnsafeNativeMethods.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

namespace System.Windows.Forms
{
	internal class UnsafeNativeMethods
	{
		public interface IEnumVariant
		{
			void Clone(IEnumVariant[] ppenum);
			int Next(int celt, IntPtr rgvar, int[] pceltFetched);
			void Reset();
			void Skip(int celt);
		}

		[MonoTODO]
		public interface IOleControl 
		{
		}

		[MonoTODO]
		public interface IOleObject
		{
		}

		[MonoTODO]
		public interface IOleInPlaceObject
		{
		}

		[MonoTODO]
		public interface IOleInPlaceActiveObject
		{
		}

		[MonoTODO]
		public interface IOleWindow
		{
		}

		[MonoTODO]
		public interface IViewObject
		{
		}

		[MonoTODO]
		public interface IViewObject2
		{
		}

		[MonoTODO]
		public interface IPersist
		{
		}

		[MonoTODO]
		public interface IPersistStreamInit
		{
		}

		[MonoTODO]
		public interface IPersistPropertyBag
		{
		}

		[MonoTODO]
		public interface IPersistStorage
		{
		}

		[MonoTODO]
		public interface IQuickActivate
		{
		}
	}
}
