//
// System.ComponentModel.EditorBrowsableAttribute.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//
//

using System.ComponentModel;

namespace System.ComponentModel 
{

	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Delegate |
	AttributeTargets.Enum | AttributeTargets.Event | AttributeTargets.Field |
	AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Property |
	AttributeTargets.Struct)]
	public sealed class EditorBrowsableAttribute : Attribute 
	{
		private EditorBrowsableState state;

		public EditorBrowsableAttribute ()
		{
			this.state = EditorBrowsableState.Always;
		}

		public EditorBrowsableAttribute (System.ComponentModel.EditorBrowsableState state)
		{
			this.state = state;
		}
			
		public EditorBrowsableState State {
        		get {
        			return state;
        		}
		}

		public override bool Equals (object obj)
		{
			if (!(obj is EditorBrowsableAttribute))
				return false;
			if (obj == this)
				return true;
			return ((EditorBrowsableAttribute) obj).State == state;
		}

		public override int GetHashCode ()
		{
			return state.GetHashCode ();
		}
	}
}
