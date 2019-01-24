using System.Reflection;
using System.Collections.Generic;

namespace System
{
	static class MonoCustomAttrs
	{
		internal static object[] GetCustomAttributes (ICustomAttributeProvider obj, Type attributeType, bool inherit) => throw new NotImplementedException ();

		internal static bool IsDefined (ICustomAttributeProvider obj, Type attributeType, bool inherit) => throw new NotImplementedException ();

		internal static IList<CustomAttributeData> GetCustomAttributesData (ICustomAttributeProvider obj, Type attributeType, bool inherit) => throw new NotImplementedException ();

		internal static IList<CustomAttributeData> GetCustomAttributesData (ICustomAttributeProvider obj, bool inherit = false) => throw new NotImplementedException ();
	}
}

namespace System.Threading
{
	public sealed class Timer : System.MarshalByRefObject, System.IDisposable
	{
		public Timer(System.Threading.TimerCallback callback) { }
		public Timer(System.Threading.TimerCallback callback, object state, int dueTime, int period) { }
		public Timer(System.Threading.TimerCallback callback, object state, long dueTime, long period) { }
		public Timer(System.Threading.TimerCallback callback, object state, System.TimeSpan dueTime, System.TimeSpan period) { }
		[System.CLSCompliantAttribute(false)]
		public Timer(System.Threading.TimerCallback callback, object state, uint dueTime, uint period) { }
		public bool Change(int dueTime, int period) { throw null; }
		public bool Change(long dueTime, long period) { throw null; }
		public bool Change(System.TimeSpan dueTime, System.TimeSpan period) { throw null; }
		[System.CLSCompliantAttribute(false)]
		public bool Change(uint dueTime, uint period) { throw null; }
		public void Dispose() { }
		public bool Dispose(System.Threading.WaitHandle notifyObject) { throw null; }
		public System.Threading.Tasks.ValueTask DisposeAsync() { throw null; }
	}

	internal class TimerQueueTimer
	{
		public TimerQueueTimer (TimerCallback timerCallback, object state, uint dueTime, uint period, bool flowExecutionContext)
		{
		}

		internal bool Change(uint dueTime, uint period)
		{
			throw new NotImplementedException ();
		}

		public void Close ()
		{
		}
	}
}

namespace System.Reflection
{
	abstract class MonoField : RuntimeFieldInfo
	{
		internal object UnsafeGetValue (object obj) => throw new NotImplementedException ();
	}

	abstract class MonoMethod : MethodInfo
	{
		internal MethodInfo GetBaseMethod () => throw new NotImplementedException ();
	}

	abstract class RuntimeModule
	{
	}

	abstract class RuntimeFieldInfo : FieldInfo
	{
		internal RuntimeType GetDeclaringTypeInternal () => throw new NotImplementedException ();
	}

	public partial class CustomAttributeData
	{
		protected CustomAttributeData() { }
		public virtual System.Type AttributeType { get { throw null; } }
		public virtual System.Reflection.ConstructorInfo Constructor { get { throw null; } }
		public virtual System.Collections.Generic.IList<System.Reflection.CustomAttributeTypedArgument> ConstructorArguments { get { throw null; } }
		public virtual System.Collections.Generic.IList<System.Reflection.CustomAttributeNamedArgument> NamedArguments { get { throw null; } }
		public override bool Equals(object obj) { throw null; }
		public static System.Collections.Generic.IList<System.Reflection.CustomAttributeData> GetCustomAttributes(System.Reflection.Assembly target) { throw null; }
		public static System.Collections.Generic.IList<System.Reflection.CustomAttributeData> GetCustomAttributes(System.Reflection.MemberInfo target) { throw null; }
		public static System.Collections.Generic.IList<System.Reflection.CustomAttributeData> GetCustomAttributes(System.Reflection.Module target) { throw null; }
		public static System.Collections.Generic.IList<System.Reflection.CustomAttributeData> GetCustomAttributes(System.Reflection.ParameterInfo target) { throw null; }
		public override int GetHashCode() { throw null; }
		public override string ToString() { throw null; }
	}
}
