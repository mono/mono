//
// System.Runtime.Remoting.Contexts.SynchronizationAttribute..cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Activation;

namespace System.Runtime.Remoting.Contexts {

	[Serializable]
	[AttributeUsage (AttributeTargets.Class)]
	public class SynchronizationAttribute : ContextAttribute, IContributeClientContextSink, IContributeServerContextSink
	{
		public const int NOT_SUPPORTED = 1;
		public const int SUPPORTED = 2;
		public const int REQUIRED = 4;
		public const int REQUIRES_NEW = 8;

		private bool _fReentrant = false;
		private int _nBehavior = REQUIRED;
		private bool _fLocked = false;

		public SynchronizationAttribute ()
		{
		}

		public SynchronizationAttribute (bool fReentrant)
		{
			_fReentrant = fReentrant;
		}

		public SynchronizationAttribute (int nBehavior)
		{
			if (nBehavior != NOT_SUPPORTED &&
				nBehavior != SUPPORTED &&
				nBehavior != REQUIRED &&
				nBehavior != REQUIRES_NEW)
			{
				throw new ArgumentException ("Invalid Flag");
			}
			_nBehavior = nBehavior;
		}

		public SynchronizationAttribute (int nBehavior, bool fReentrant)
		{
			if (nBehavior != NOT_SUPPORTED &&
				nBehavior != SUPPORTED &&
				nBehavior != REQUIRED &&
				nBehavior != REQUIRES_NEW)
			{
				throw new ArgumentException ("Invalid Flag");
			}
			_nBehavior = nBehavior;
			_fReentrant = fReentrant;
		}
		
		public virtual bool IsReEntrant
		{
			get { return _fReentrant; }
		}
		
		public virtual bool Locked
		{
			get { return _fLocked; }
			set { _fLocked = value; }
		}

		[MonoTODO]
		public virtual IMessageSink GetClientContextSink (IMessageSink nextSink)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void GetPropertiesForNewContext (IConstructionCallMessage ctorMsg)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual IMessageSink GetServerContextSink (IMessageSink nextSink)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool IsContextOK (Context ctx, IConstructionCallMessage msg)
		{
			throw new NotImplementedException ();
		}
	}
}
