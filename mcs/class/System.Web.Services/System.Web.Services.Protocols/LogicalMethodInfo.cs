// 
// System.Web.Services.Protocols.LogicalMethodInfo.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Reflection;
using System.Web.Services;

namespace System.Web.Services.Protocols {
	public sealed class LogicalMethodInfo {

		#region Fields

		#endregion // Fields

		#region Constructors
	
		public LogicalMethodInfo (MethodInfo methodInfo)
		{
		}
		
		#endregion // Constructors

		#region Properties

		public ParameterInfo AsyncCallbackParameter {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public ParameterInfo AsyncResultParameter {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public ParameterInfo AsyncStateParameter {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public MethodInfo BeginMethodInfo {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public ICustomAttributeProvider CustomAttributeProvider {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public Type DeclaringType {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public MethodInfo EndMethodInfo {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public ParameterInfo[] InParameters {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public bool IsAsync {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public bool IsVoid {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public MethodInfo MethodInfo {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public string Name {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public ParameterInfo[] OutParameters {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public ParameterInfo[] Parameters {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public Type ReturnType {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public ICustomAttributeProvider ReturnTypeCustomAttributeProvider {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public IAsyncResult BeginInvoke (object target, object[] values, AsyncCallback callback, object asyncState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static LogicalMethodInfo[] Create (MethodInfo[] methodInfos)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static LogicalMethodInfo[] Create (MethodInfo[] methodInfos, LogicalMethodTypes types)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object EndInvoke (object target, IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object GetCustomAttribute (Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object[] GetCustomAttributes (Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object[] Invoke (object target, object[] values)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool IsBeginMethod (MethodInfo methodInfo)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool IsEndMethod (MethodInfo methodInfo)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
