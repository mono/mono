//
// System.Runtime.Remoting.Contexts.ContextAttribute..cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Remoting.Activation;
using System.Collections;

namespace System.Runtime.Remoting.Contexts {

	[AttributeUsage (AttributeTargets.Class)]
	[Serializable]
	public class ContextAttribute : Attribute, IContextAttribute, IContextProperty {
		protected string AttributeName;

		public ContextAttribute (string name)
		{
			AttributeName = name;
		}

		public virtual string Name {
			get {
				return AttributeName;
			}
		}

		public override bool Equals (object o)
		{
			if (o == null)
				return false;

			if (!(o is ContextAttribute))
				return false;

			ContextAttribute ca = (ContextAttribute) o;
			
			if (ca.AttributeName != AttributeName)
				return false;

			return true;
		}

		public virtual void Freeze (Context ctx)
		{
		}

		public override int GetHashCode ()
		{
			if (AttributeName == null)
				return 0;
			
			return AttributeName.GetHashCode ();
		}

		/// <summary>
		///    Adds the current context property to the IConstructionCallMessage
		/// </summary>
		public virtual void GetPropertiesForNewContext (IConstructionCallMessage msg)
		{
			if (msg == null)
				throw new ArgumentNullException ("IConstructionCallMessage");

			IList list = msg.ContextProperties;

			list.Add (this);
		}

		// <summary>
		//   True whether the context arguments satisfies the requirements
		//   of the current context.
		// </summary>
		public virtual bool IsContextOK (Context ctx, IConstructionCallMessage msg)
		{
			if (msg == null)
				throw new ArgumentNullException ("IConstructionCallMessage");
			if (ctx == null)
				throw new ArgumentNullException ("Context");

			if (!msg.ActivationType.IsContextful)
				return true;

			IContextProperty p = ctx.GetProperty (AttributeName);
			if (p == null)
				return false;

			if (this != p)
				return false;
				
			return true;
		}

		public virtual bool IsNewContextOK (Context ctx)
		{
			return true;
		}
	}
}
