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
	/// <summary>
	/// Summary description for WeakReference.
	/// </summary>
	[Serializable]
	public class WeakReference : ISerializable
	{
		//Fields
		private bool isLongReference;
		private GCHandle gcHandle;

		// Helper method for constructors
		//Should not be called from any other method.
		private void AllocateHandle(Object target)
		{
			if(this.isLongReference)
			{
				this.gcHandle = GCHandle.Alloc(target, GCHandleType.WeakTrackResurrection);
			}
			else
			{
				this.gcHandle = GCHandle.Alloc(target, GCHandleType.Weak);
			}
		}		
		
		
		//Constructors
		public WeakReference(object target)
			: this(target,false)
		{}

		
		public WeakReference(object target, bool trackResurrection)
		{
			this.isLongReference = trackResurrection;
			AllocateHandle(target);
		}

		
		protected WeakReference(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			this.isLongReference = info.GetBoolean("IsLongReference");
			//TODO: How to load the exact type?
			//Does that matter? No idea :(
			Object target = info.GetValue("TargetObject",typeof(System.Object));

			AllocateHandle(target);
		}

		
		// Properties
		public virtual bool IsAlive 
		{
			get
			{
				//Target property takes care of the exception
				return (Target != null);		
			}
		}

		public virtual object Target 
		{
			get
			{
				//Exception is thrown by gcHandle's Target
				return this.gcHandle.Target;
			}
			set
			{
				this.gcHandle.Target = value;
			}
		}

		public virtual bool TrackResurrection 
		{
			get
			{
				return this.isLongReference;
			}
		}

		//Methods
		~WeakReference()
		{
			gcHandle.Free();
		}

		//TODO
		public virtual void GetObjectData(SerializationInfo info,StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			info.AddValue("IsLongReference",this.isLongReference);
			try
			{
				info.AddValue("TargetObject",Target);
			}
			catch(Exception)
			{
				info.AddValue("TargetObject",null);
			}
		}
	}
}