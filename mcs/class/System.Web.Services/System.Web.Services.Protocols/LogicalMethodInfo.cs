// 
// System.Web.Services.Protocols.LogicalMethodInfo.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
// Copyright (C) Ximian, Inc,  2003
//
// TODO:
//    BeginInvoke, EndInvoke are missing.
//    AsyncResultParameter
//
// WILD GUESS:
//   The reason for this class is so that it can cluster method/begin/end methods
//   together, as the begin/end methods in generated files from WSDL does *NOT*
//   contain all the information required to make a request.
//
//   Either that, or the Begin*/End* versions probe the attributes on the regular
//   method (which seems simpler). 
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Reflection;
using System.Collections;
using System.Text;
using System.Web.Services;

namespace System.Web.Services.Protocols {
	public sealed class LogicalMethodInfo {
                #region Fields

		MethodInfo method_info, end_method_info;
		ParameterInfo [] parameters;
		ParameterInfo [] out_parameters;
		ParameterInfo [] in_parameters;
		WebMethodAttribute attribute;

		#endregion // Fields.
		
		#region Constructors
	
		public LogicalMethodInfo (MethodInfo method_info)
		{
			if (method_info == null)
				throw new ArgumentNullException ("method_info should be non-null");
			if (method_info.IsStatic)
				throw new InvalidOperationException ("method is static");
			
			this.method_info = method_info;
		}

		//
		// Only an internal contructor, called from "Create"
		//
		LogicalMethodInfo (MethodInfo method_info, MethodInfo end_method_info)
		{
			if (method_info == null)
				throw new ArgumentNullException ("method_info should be non-null");
			if (method_info.IsStatic)
				throw new InvalidOperationException ("method is static");
			
			this.method_info = method_info;
			this.end_method_info = end_method_info;
		}
		
		#endregion // Constructors

		#region Properties

		//
		// Signatures for Begin/End methods:
		//
		//        public System.IAsyncResult BeginHelloWorld(ARG1, ARG2, System.AsyncCallback callback, object asyncState) {
		//        public string EndHelloWorld(System.IAsyncResult asyncResult) {

		public ParameterInfo AsyncCallbackParameter {
			get {
				ParameterInfo [] pi = method_info.GetParameters ();
				return pi [pi.Length-2];
			}
		}

		public ParameterInfo AsyncResultParameter {
			get {
				ParameterInfo [] pi = end_method_info.GetParameters ();
				return pi [pi.Length-1];
			}
		}

		public ParameterInfo AsyncStateParameter {
			get {
				ParameterInfo [] pi = method_info.GetParameters ();
				return pi [pi.Length-1];
			}
		}

		public MethodInfo BeginMethodInfo {
			get {
				if (IsBeginMethod (method_info))
					return method_info;
				return null;
			}
		}

		public ICustomAttributeProvider CustomAttributeProvider {
			get {
				return method_info;
			}
		}

		public Type DeclaringType {
			get {
				return method_info.DeclaringType;
			}
		}

		public MethodInfo EndMethodInfo {
			get {
				return end_method_info;
			}
		}

		public ParameterInfo[] InParameters {
			get {
				if (parameters == null)
					ComputeParameters ();
				return in_parameters;
			}
		}

		public bool IsAsync {
			get {
				return end_method_info != null;
			}
		}

		public bool IsVoid {
			get {
				return ReturnType == typeof (void);
			}
		}

		public MethodInfo MethodInfo {
			get {
				if (IsBeginMethod (method_info))
					return null;
				return method_info;
			}
		}

		public string Name {
			get {
				return method_info.Name;
			}
		}

		internal MethodInfo ActualMethodInfo {
			get { return method_info; }
		}

		void ComputeParameters ()
		{
			ParameterInfo[] pars = method_info.GetParameters ();
			if (IsAsync)
			{
				parameters = new ParameterInfo [pars.Length - 2];
				Array.Copy (pars, 0, parameters, 0, pars.Length - 2);
				in_parameters = new ParameterInfo [parameters.Length];
				parameters.CopyTo (in_parameters, 0);
				
				ParameterInfo[] outPars = end_method_info.GetParameters ();
				out_parameters = new ParameterInfo [outPars.Length - 1];
				Array.Copy (outPars, 0, out_parameters, 0, out_parameters.Length);
			}
			else
			{
				parameters = pars;
				int out_count = 0;
				int in_count = 0;
				
				foreach (ParameterInfo p in parameters){
					Type ptype = p.ParameterType;
					if (ptype.IsByRef){
						out_count++;
						if (!p.IsOut)
							in_count++;
					} else
						in_count++;
				}
				out_parameters = new ParameterInfo [out_count];
				int i = 0;
				for (int j = 0; j < parameters.Length; j++){
					if (parameters [j].ParameterType.IsByRef)
						out_parameters [i++] = parameters [j];
				}
				in_parameters = new ParameterInfo [in_count];
				i = 0;
				for (int j = 0; j < parameters.Length; j++){
					if (parameters [j].ParameterType.IsByRef){
						if (!parameters [j].IsOut)
							in_parameters [i++] = parameters [j];
					} else
						in_parameters [i++] = parameters [j];
				}
			}
		}
		
		public ParameterInfo[] OutParameters {
			get {
				if (parameters == null)
					ComputeParameters ();
				return out_parameters;
			}
		}

		public ParameterInfo[] Parameters {
			get {
				if (parameters == null)
					ComputeParameters ();
				return parameters;
			}
		}

		public Type ReturnType {
			get {
				if (IsAsync)
					return end_method_info.ReturnType;
				else
					return method_info.ReturnType;
			}
		}

		public ICustomAttributeProvider ReturnTypeCustomAttributeProvider {
			get {
				return method_info.ReturnTypeCustomAttributes;
			}
		}

		internal bool EnableSession {
			get {
				if (method_info == null)
					return false;

				if (attribute == null) {
					object [] o = method_info.GetCustomAttributes (false);
					foreach (Attribute att in o) {
						if (att is WebMethodAttribute) {
							attribute = (WebMethodAttribute) att;
							break;
						}
					}
				}

				return (attribute != null) ? attribute.EnableSession : false;
			}
		}
		internal int CacheDuration {
			get {
				if (method_info == null)
					return -1;

				if (attribute == null) {
					object [] o = method_info.GetCustomAttributes (false);
					foreach (Attribute att in o) {
						if (att is WebMethodAttribute) {
							attribute = (WebMethodAttribute) att;
							break;
						}
					}
				}

				return (attribute != null) ? attribute.CacheDuration : -1;
			}
		}
		#endregion // Properties

		#region Methods

		public IAsyncResult BeginInvoke (object target, object[] values, AsyncCallback callback, object asyncState)
		{
			int len = (values!=null) ? values.Length : 0;
			object[] pars = new object [len + 2];
			
			if (len > 0)
				values.CopyTo (pars, 0);
			
			pars [len] = callback;
			pars [len+1] = asyncState;
				
			return (IAsyncResult) method_info.Invoke (target, pars);
		}

		public static LogicalMethodInfo[] Create (MethodInfo[] method_infos)
		{
			return Create (method_infos, LogicalMethodTypes.Sync | LogicalMethodTypes.Async);
		}

		public static LogicalMethodInfo[] Create (MethodInfo[] method_infos, LogicalMethodTypes types)
		{
			ArrayList sync = ((types & LogicalMethodTypes.Sync) != 0) ? new ArrayList () : null;
			ArrayList begin, end;

			if ((types & LogicalMethodTypes.Async) != 0){
				begin = new ArrayList ();
				end = new ArrayList ();
			} else 
				begin = end = null;

			foreach (MethodInfo mi in method_infos){
				if (IsBeginMethod (mi) && begin != null)
					begin.Add (mi);
				else if (IsEndMethod (mi) && end != null)
					end.Add (mi);
				else if (sync != null)
					sync.Add (mi);
			}

			int bcount = 0, count = 0;
			if (begin != null){
				bcount = count = begin.Count;
				if (count != end.Count)
					throw new InvalidOperationException ("Imbalance of begin/end methods");
			}
			if (sync != null)
				count += sync.Count;

			LogicalMethodInfo [] res = new LogicalMethodInfo [count];
			int dest = 0;
			if (begin != null){
				foreach (MethodInfo bm in begin){
					string end_name = "End" + bm.Name.Substring (5);

					for (int i = 0; i < bcount; i++){
						MethodInfo em = (MethodInfo) end [i];
						if (em.Name == end_name){
							res [dest++] = new LogicalMethodInfo (bm, em);
							break;
						}
						throw new InvalidOperationException ("Imbalance of begin/end methods");
					}
				}
			}


			if (sync != null)
				foreach (MethodInfo mi in sync){
					res [dest++] = new LogicalMethodInfo (mi);
				}
			
			return res;
		}

		public object[] EndInvoke (object target, IAsyncResult asyncResult)
		{
			if (parameters == null)
				ComputeParameters ();

			object[] values = new object [out_parameters.Length + 1];
			values [values.Length - 1] = asyncResult;
			object res = end_method_info.Invoke (target, values);
			
			int retc = IsVoid ? 0 : 1;
			object [] ret = new object [retc + out_parameters.Length];
			
			if (retc == 1) ret [0] = res;
			
			Array.Copy (values, 0, ret, retc, out_parameters.Length);
			return ret;
		}

		public object GetCustomAttribute (Type type)
		{
			return Attribute.GetCustomAttribute (method_info, type, false);
		}

		public object[] GetCustomAttributes (Type type)
		{
			return method_info.GetCustomAttributes (type, false);
		}

		public object[] Invoke (object target, object[] values)
		{
			if (parameters == null)
				ComputeParameters ();

			int retc = IsVoid ? 0 : 1;
			object [] ret = new object [retc + out_parameters.Length];
			object res = method_info.Invoke (target, values);
			if (retc == 1) ret [0] = res;

			int j = retc;
			for (int i = 0; i < parameters.Length; i++){
				if (parameters [i].ParameterType.IsByRef)
					ret [j++] = values [i];
			}

			return ret;
		}

		public static bool IsBeginMethod (MethodInfo method_info)
		{
			if (method_info == null)
				throw new ArgumentNullException ("method_info can not be null");

			if (method_info.ReturnType != typeof (IAsyncResult))
				return false;

			if (method_info.Name.StartsWith ("Begin"))
				return true;

			return false;
		}

		public static bool IsEndMethod (MethodInfo method_info)
		{
			if (method_info == null)
				throw new ArgumentNullException ("method_info can not be null");

			ParameterInfo [] parameter_info = method_info.GetParameters ();
			if (parameter_info.Length != 1)
				return false;
			if (parameter_info [0].ParameterType != typeof (IAsyncResult))
				return false;
			if (method_info.Name.StartsWith ("End"))
				return true;

			return false;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			if (parameters == null)
				ComputeParameters ();
			
			for (int i = 0; i < parameters.Length; i++){
				sb.Append (parameters [i].ParameterType);
				if (parameters [i].ParameterType.IsByRef)
					sb.Append (" ByRef");
				
				if (i+1 != parameters.Length)
					sb.Append (", ");
			}
			
			return String.Format (
                        	"{0} {1} ({2})",
				method_info.ReturnType, method_info.Name,
				sb.ToString ());
		}

		#endregion // Methods
	}
}
