//
// System.WeakReference.cs
//
// Author:
//   Ajay kumar Dwivedi (adwiv@yahoo.com)
//

using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace System
{
	[Serializable]
	public class WeakReference : ISerializable
	{
		//Fields
		private bool isLongReference;
		private GCHandle gcHandle;

		// Helper method for constructors
		//Should not be called from any other method.
		private void AllocateHandle (Object target)
		{
			if (isLongReference) {
				gcHandle = GCHandle.Alloc (target, GCHandleType.WeakTrackResurrection);
			}
			else {
				gcHandle = GCHandle.Alloc (target, GCHandleType.Weak);
			}
		}

		//Constructors
		public WeakReference (object target)
			: this (target, false)
		{
		}

		public WeakReference (object target, bool trackResurrection)
		{
			isLongReference = trackResurrection;
			AllocateHandle (target);
		}

		protected WeakReference (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			isLongReference = info.GetBoolean ("TrackResurrection");
			Object target = info.GetValue ("TrackedObject", typeof (System.Object));

			AllocateHandle (target);
		}

		// Properties
		public virtual bool IsAlive {
			get {
				//Target property takes care of the exception
				return (Target != null);		
			}
		}

		public virtual object Target {
			get {
				//Exception is thrown by gcHandle's Target
				return gcHandle.Target;
			}
			set
			{
				gcHandle.Target = value;
			}
		}

		public virtual bool TrackResurrection {
			get {
				return isLongReference;
			}
		}

		//Methods
		~WeakReference ()
		{
			gcHandle.Free ();
		}

		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			info.AddValue ("TrackResurrection", TrackResurrection);

			try {
				info.AddValue ("TrackedObject", Target);
			} catch (Exception) {
				info.AddValue ("TrackedObject", null);
			}
		}
	}
}
