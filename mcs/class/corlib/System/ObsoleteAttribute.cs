//
// System.ObsoleteAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System
{
     [AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct |
				  AttributeTargets.Enum | AttributeTargets.Constructor |
				  AttributeTargets.Method | AttributeTargets.Property |
				  AttributeTargets.Field | AttributeTargets.Event |
				  AttributeTargets.Interface | AttributeTargets.Delegate)]
	[Serializable]
	public class ObsoleteAttribute : Attribute
	{
		   private string Message;
		   private bool IsError = false;
		   
		   // Constructors
		   public ObsoleteAttribute () : base ()
		   {
		   }

		   public ObsoleteAttribute (string message)
		   {
				 Message = message;
		   }

		   public ObsoleteAttribute (string message, bool error)
		   {
				 Message = message;
				 IsError = error;
		   }
	}
}
