//
// System.Windows.Forms.OSFeature.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

        public class OSFeature : FeatureSupport {

		//
		//	 --- Public Fields
		//
		public static readonly object LayeredWindows;
		public static readonly object Themes;

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public static OSFeature Feature {
			get {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//
		[MonoTODO]
		public virtual bool Equals(object o)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static bool Equals(object o1, object o2)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual int GetHashCode()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Type GetType()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override Version GetVersionPresent(object feature)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual bool IsPresent(object o)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual bool IsPresent(object o, Version v)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual string ToString()
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Protected Methods
		//
		[MonoTODO]
		protected object MemberwiseClone()
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Destructor
		//
		[MonoTODO]
		~OSFeature()
		{
			throw new NotImplementedException ();
		}
	 }
}
