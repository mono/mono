//
// System.ComponentModel.EditorBrowsableAttribute.cs
//
// Author:
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
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
		

        public EditorBrowsableState State
        {
        	get 
        	{
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


// Old implementation
//using System;
//
//
//
//namespace System.ComponentModel
//{
//
//	/// <summary>
//	/// Specifies that a property or method is viewable in an editor. This class cannot be inherited.
//	/// </summary>
//
//	[MonoTODO("Missing description for State. Only minimal testing.")]
//
//	[AttributeUsage(
//
//		AttributeTargets.Class|
//
//		AttributeTargets.Constructor|
//
//		AttributeTargets.Delegate|
//
//		AttributeTargets.Enum|
//
//		AttributeTargets.Event|
//
//		AttributeTargets.Field|
//
//		AttributeTargets.Interface|
//
//		AttributeTargets.Method|
//
//		AttributeTargets.Property|
//
//		AttributeTargets.Struct)]
//
//	public sealed class EditorBrowsableAttribute : Attribute
//
//	{
//
//		private System.ComponentModel.EditorBrowsableState state;
//
//
//
//		/// <summary>
//
//		/// FIXME: Summary description for State.
//
//		/// </summary>
//
//		public System.ComponentModel.EditorBrowsableState State
//
//		{
//
//			get 
//
//			{
//
//				return state;
//
//			}
//
//		}
//
//
//
//		/// <summary>
//
//		/// Initializes a new instance of the System.ComponentModel.EditorBrowsableAttribute class with an System.ComponentModel.EditorBrowsableState.
//
//		/// </summary>
//
//		/// <param name="state">The System.ComponentModel.EditorBrowsableState to set System.ComponentModel.EditorBrowsableAttribute.State to.</param>
//
//		public EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState state)
//
//		{
//
//			this.state = state;
//
//		}
//
//
//
//		/// <summary>
//
//		/// Initializes a new instance of the System.ComponentModel.EditorBrowsableAttribute class with an System.ComponentModel.EditorBrowsableState == System.ComponentModel.EditorBrowsableState.Always.
//
//		/// </summary>
//
//		public EditorBrowsableAttribute()
//
//		{
//
//			this.state = System.ComponentModel.EditorBrowsableState.Always; 
//
//		}
//
//	}
//
//}

