using System;
using System.IO;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.CompilerServices;

namespace Mono
{
	static class RemotingGate
	{
		static bool _initialized;
		static object _syncRoot = new object ();
		static BinaryFormatter _serializationFormatter;
		static BinaryFormatter _deserializationFormatter;
		
		[MonoLinkerConditional (MonoLinkerFeatures.Remoting, MonoLinkerConditionalAction.Remove)]
		public static void Initialize ()
		{
			lock (_syncRoot) {
				if (_initialized)
					return;
				RemotingSurrogateSelector surrogateSelector = new RemotingSurrogateSelector ();
				StreamingContext context = new StreamingContext (StreamingContextStates.Remoting, null);
				_serializationFormatter = new BinaryFormatter (surrogateSelector, context);
				_deserializationFormatter = new BinaryFormatter (null, context);
				_serializationFormatter.AssemblyFormat = FormatterAssemblyStyle.Full;
				_deserializationFormatter.AssemblyFormat = FormatterAssemblyStyle.Full;
				_initialized = true;
			}
		}

		[MonoLinkerConditional (MonoLinkerFeatures.Remoting, MonoLinkerConditionalAction.Throw)]
		internal static byte[] SerializeCallData (object obj)
		{
			var ctx = Thread.CurrentThread.GetExecutionContextReader().LogicalCallContext;
			if (!ctx.IsNull) {
				CACD cad = new CACD ();
				cad.d = obj;
				cad.c = ctx.Clone ();
				obj = cad;
			}
			
			if (obj == null) return null;

			Initialize ();
			MemoryStream ms = new MemoryStream ();
			lock (_serializationFormatter) {
				_serializationFormatter.Serialize (ms, obj);
			}
			return ms.ToArray ();
		}

		[MonoLinkerConditional (MonoLinkerFeatures.Remoting, MonoLinkerConditionalAction.Throw)]
		internal static object DeserializeCallData (byte[] array)
		{
			if (array == null) return null;
			
			Initialize ();
			MemoryStream ms = new MemoryStream (array);
			object obj;
			lock (_deserializationFormatter) {
				obj = _deserializationFormatter.Deserialize (ms);
			}
			
			if (obj is CACD) {
				CACD cad = (CACD) obj;
				obj = cad.d;

				var lcc = (LogicalCallContext) cad.c;
				if (lcc.HasInfo)
					Thread.CurrentThread.GetMutableExecutionContext().LogicalCallContext.Merge (lcc);
			}
			return obj;
		}
		
		[MonoLinkerConditional (MonoLinkerFeatures.Remoting, MonoLinkerConditionalAction.Throw)]
		internal static byte[] SerializeExceptionData (Exception ex)
		{
			Initialize ();
			byte[] result = null;
			try {
				/* empty - we're only interested in the protected block */
			} finally {
				MemoryStream ms = new MemoryStream ();
				lock (_serializationFormatter) {
					_serializationFormatter.Serialize (ms, ex);
				}
				result = ms.ToArray ();
			}
			return result;
		}

		// Holds information in xdomain calls. Names are short to minimize serialized size.
		[Serializable]
		class CACD {
			public object d;	/* call data */
			public object c;	/* call context */
		}

		[MonoLinkerConditional (MonoLinkerFeatures.Remoting, MonoLinkerConditionalAction.Throw)]
		internal static LogicalCallContext.Reader GetCallContextReader (LogicalCallContext context)
		{
			return new LogicalCallContext.Reader (context);
		}

		[MonoLinkerConditional (MonoLinkerFeatures.Remoting, MonoLinkerConditionalAction.Throw)]
		internal static LogicalCallContext CopyCallContext (LogicalCallContext context)
		{
			if (context == null || !context.HasInfo)
				return null;
			var reader = new LogicalCallContext.Reader (context);
			if (!reader.HasInfo)
				return null;
			return reader.Clone ();
		}

		[MonoLinkerConditional (MonoLinkerFeatures.Remoting, MonoLinkerConditionalAction.Throw)]
		internal static LogicalCallContext CloneCallContext (LogicalCallContext context)
		{
			return (LogicalCallContext)context.Clone ();
		}

		[MonoLinkerConditional (MonoLinkerFeatures.Remoting, MonoLinkerConditionalAction.Return)]
		internal static bool CallContextHasInfo (LogicalCallContext context)
		{
			return context.HasInfo;
		}

		[MonoLinkerConditional (MonoLinkerFeatures.Remoting, MonoLinkerConditionalAction.Throw)]
		internal static IllogicalCallContext CreateCopyIllogicalCallContext (IllogicalCallContext context)
		{
			return context.CreateCopy ();
		}

		[MonoLinkerConditional (MonoLinkerFeatures.Remoting, MonoLinkerConditionalAction.Return)]
		internal static bool IllogicalCallContextHasUserData (IllogicalCallContext context)
		{
			return context.HasUserData;
		}

		[MonoLinkerConditional (MonoLinkerFeatures.Remoting, MonoLinkerConditionalAction.Return)]
		internal static void EnterContext ()
		{
			SynchronizationAttribute.EnterContext ();
		}

		[MonoLinkerConditional (MonoLinkerFeatures.Remoting, MonoLinkerConditionalAction.Return)]
		internal static void ExitContext ()
		{
			SynchronizationAttribute.ExitContext ();
		}

		[MonoLinkerConditional (MonoLinkerFeatures.Remoting, MonoLinkerConditionalAction.Return)]
		internal static bool TypeIsConstructionCall (Type type)
		{
			return Object.ReferenceEquals (type, typeof (ConstructionCall));
		}

		[MonoLinkerConditional (MonoLinkerFeatures.Remoting, MonoLinkerConditionalAction.Return)]
		internal static bool TypeIsLogicalCallContext (Type type)
		{
			return Object.ReferenceEquals (type, typeof (LogicalCallContext));

		}

		[MonoLinkerConditional (MonoLinkerFeatures.Remoting, MonoLinkerConditionalAction.Return)]
		internal static bool TypeIsSynchronizationAttribute (Type type)
		{
			return Object.ReferenceEquals (type, typeof (SynchronizationAttribute));
		}
	}
}
