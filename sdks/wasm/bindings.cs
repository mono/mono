using System;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace WebAssembly
{
    /*
	TODO:
		Expose annotated C# type to JS
		Add property fetch to JSObject
		Add typed method invoke support (get a delegate?)
		Add JS helpers to fetch wrapped methods, like to Module.cwrap
		Better Wrap C# exception when passing them as object (IE, on task failure)
		Make JSObject disposable (same for js objects)
	*/
    public sealed class Runtime
    {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        static extern string InvokeJS(string str, out int exceptional_result);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern object InvokeJSWithArgs(int js_obj_handle, string method, object[] _params, out int exceptional_result);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern object GetObjectProperty(int js_obj_handle, string propertyName, out int exceptional_result);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern object SetObjectProperty(int js_obj_handle, string propertyName, object value, bool createIfNotExists, bool hasOwnProperty, out int exceptional_result);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern object GetGlobalObject(string globalName, out int exceptional_result);

        public static string InvokeJS(string str)
        {
            int exception = 0;
            var res = InvokeJS(str, out exception);
            if (exception != 0)
                throw new JSException(res);
            return res;
        }

        static Dictionary<int, JSObject> bound_objects = new Dictionary<int, JSObject>();
        static Dictionary<object, JSObject> raw_to_js = new Dictionary<object, JSObject>();

        static int BindJSObject(int js_id)
        {
            JSObject obj;
            if (bound_objects.ContainsKey(js_id))
                obj = bound_objects[js_id];
            else
                bound_objects[js_id] = obj = new JSObject(js_id);

            return (int)(IntPtr)obj.Handle;
        }

        static int UnBindJSObject(int js_id)
        {
            if (bound_objects.ContainsKey(js_id))
            {
                var obj = bound_objects[js_id];
                bound_objects.Remove(js_id);
                return (int)(IntPtr)obj.Handle;
            }

            return 0;

        }

        static int BindJSObject(JSObject obj)
        {

            int js_id = obj.JSHandle;
            if (js_id <= 0)
                throw new JSException($"Invalid JS Object Handle {js_id}");

            if (bound_objects.ContainsKey(js_id))
                obj = bound_objects[js_id];
            else
                bound_objects[js_id] = obj;

            return (int)(IntPtr)obj.Handle;
        }

        static int UnBindJSObjectAndFree(int js_id)
        {
            if (bound_objects.ContainsKey(js_id))
            {
                var obj = bound_objects[js_id];
                bound_objects.Remove(js_id);
                var gCHandle = obj.Handle;
                obj.Handle.Free();
                obj.JSHandle = -1;
                obj.RawObject = null;
                return (int)(IntPtr)gCHandle;
            }
            return 0;

        }


        static int UnBindRawJSObjectAndFree(int gcHandle)
        {

            GCHandle h = (GCHandle)(IntPtr)gcHandle;
            JSObject obj = (JSObject)h.Target;
            if (obj != null && obj.RawObject != null)
            {
                var raw_obj = obj.RawObject;
                if (raw_to_js.ContainsKey(raw_obj))
                {
                    raw_to_js.Remove(raw_obj);
                }


                obj.Dispose();

                var gCHandle = obj.Handle;
                obj.Handle.Free();
                obj.JSHandle = -1;
                obj.RawObject = null;
                return (int)(IntPtr)gCHandle;
            }

            return 0;

        }

        static object CreateTaskSource(int js_id)
        {
            return new TaskCompletionSource<object>();
        }

        static void SetTaskSourceResult(TaskCompletionSource<object> tcs, object result)
        {
            tcs.SetResult(result);
        }

        static void SetTaskSourceFailure(TaskCompletionSource<object> tcs, string reason)
        {
            tcs.SetException(new JSException(reason));
        }

        static int GetTaskAndBind(TaskCompletionSource<object> tcs, int js_id)
        {
            return BindExistingObject(tcs.Task, js_id);
        }

        static int BindExistingObject(object raw_obj, int js_id)
        {

            JSObject obj;
            if (raw_obj is JSObject)
                obj = (JSObject)raw_obj;
            else if (raw_to_js.ContainsKey(raw_obj))
                obj = raw_to_js[raw_obj];
            else
                raw_to_js[raw_obj] = obj = new JSObject(js_id, raw_obj);

            return (int)(IntPtr)obj.Handle;
        }

        static int GetJSObjectId(object raw_obj)
        {
            JSObject obj = null;
            if (raw_obj is JSObject)
                obj = (JSObject)raw_obj;
            else if (raw_to_js.ContainsKey(raw_obj))
                obj = raw_to_js[raw_obj];

            var js_handle = obj != null ? obj.JSHandle : -1;

            return js_handle;
        }

        static object GetMonoObject(int gc_handle)
        {
            GCHandle h = (GCHandle)(IntPtr)gc_handle;
            JSObject o = (JSObject)h.Target;
            if (o != null && o.RawObject != null)
                return o.RawObject;
            return o;
        }

        static object BoxInt(int i)
        {
            return i;
        }
        static object BoxDouble(double d)
        {
            return d;
        }

        static object BoxBool(int b)
        {
            return b == 0 ? false : true;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct IntPtrAndHandle
        {
            [FieldOffset(0)]
            internal IntPtr ptr;

            [FieldOffset(0)]
            internal RuntimeMethodHandle handle;
        }

        //FIXME this probably won't handle generics
        static string GetCallSignature(IntPtr method_handle)
        {
            IntPtrAndHandle tmp = default(IntPtrAndHandle);
            tmp.ptr = method_handle;

            var mb = MethodBase.GetMethodFromHandle(tmp.handle);

            string res = "";
            foreach (var p in mb.GetParameters())
            {
                var t = p.ParameterType;

                switch (Type.GetTypeCode(t))
                {
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Boolean:
                        // Enums types have the same code as their underlying numeric types
                        if (t.IsEnum)
                            res += "j";
                        else
                            res += "i";
                        break;
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        // Enums types have the same code as their underlying numeric types
                        if (t.IsEnum)
                            res += "k";
                        else
                            res += "l";
                        break;
                    case TypeCode.Single:
                        res += "f";
                        break;
                    case TypeCode.Double:
                        res += "d";
                        break;
                    case TypeCode.String:
                        res += "s";
                        break;
                    default:
                        if (t.IsValueType)
                            throw new Exception("Can't handle VT arguments");
                        res += "o";
                        break;
                }
            }
            return res;
        }

        static object ObjectToEnum(IntPtr method_handle, int parm, object obj)
        {
            IntPtrAndHandle tmp = default(IntPtrAndHandle);
            tmp.ptr = method_handle;

            var mb = MethodBase.GetMethodFromHandle(tmp.handle);
            var parmType = mb.GetParameters()[parm].ParameterType;
            if (parmType.IsEnum)
                return Runtime.EnumFromExportContract(parmType, obj);
            else
                return null;

        }


        static MethodInfo gsjsc;
        static void GenericSetupJSContinuation<T>(Task<T> task, JSObject cont_obj)
        {
            task.GetAwaiter().OnCompleted(() =>
            {
                //FIXME we should dispose cont_obj after completing the Promise
                if (task.Exception != null)
                    cont_obj.Invoke("reject", task.Exception.ToString());
                else
                {
                    cont_obj.Invoke("resolve", task.Result);
                }
            });
        }

        static void SetupJSContinuation(Task task, JSObject cont_obj)
        {
            if (task.GetType() == typeof(Task))
            {
                task.GetAwaiter().OnCompleted(() =>
                {
                    //FIXME we should dispose cont_obj after completing the Promise
                    if (task.Exception != null)
                        cont_obj.Invoke("reject", task.Exception.ToString());
                    else
                        cont_obj.Invoke("resolve", null);
                });
            }
            else
            {
                //FIXME this is horrible codegen, we can do better with per-method glue
                if (gsjsc == null)
                    gsjsc = typeof(Runtime).GetMethod("GenericSetupJSContinuation", BindingFlags.NonPublic | BindingFlags.Static);
                gsjsc.MakeGenericMethod(task.GetType().GetGenericArguments()).Invoke(null, new object[] { task, cont_obj });
            }
        }


        public static object GetGlobalObject(string str = null)
        {
            int exception = 0;
            var globalHandle = Runtime.GetGlobalObject(str, out exception);

            if (exception != 0)
                throw new JSException($"Error obtaining a handle to global {str}");

            return globalHandle;
        }

        static string ObjectToString(object o)
        {

            if (o is Enum)
                return EnumToExportContract((Enum)o).ToString();
            
            return o.ToString();
        }        
        // This is simple right now and will include FlagsAttribute later.
        public static Enum EnumFromExportContract(Type enumType, object value)
        {

            if (!enumType.IsEnum)
            {
                throw new ArgumentException("Type provided must be an Enum.", nameof(enumType));
            }

            if (value is string)
            {

                var fields = enumType.GetFields();
                foreach (var fi in fields)
                {
                    // Do not process special names
                    if (fi.IsSpecialName)
                        continue;

                    ExportAttribute[] attributes =
                        (ExportAttribute[])fi.GetCustomAttributes(typeof(ExportAttribute), false);

                    var enumConversionType = ConvertEnum.Default;

                    object contractName = null;

                    if (attributes != null && attributes.Length > 0)
                    {
                        enumConversionType = attributes[0].EnumValue;
                        if (enumConversionType != ConvertEnum.Numeric)
                            contractName = attributes[0].ContractName;

                    }

                    if (contractName == null)
                        contractName = fi.Name;

                    switch (enumConversionType)
                    {
                        case ConvertEnum.ToLower:
                            contractName = contractName.ToString().ToLower();
                            break;
                        case ConvertEnum.ToUpper:
                            contractName = contractName.ToString().ToUpper();
                            break;
                        case ConvertEnum.Numeric:
                            contractName = (int)Enum.Parse(value.GetType(), contractName.ToString());
                            break;
                        default:
                            contractName = contractName.ToString();
                            break;
                    }

                    if (contractName.ToString() == value.ToString())
                    {
                        return (Enum)Enum.Parse(enumType, fi.Name);
                    }

                }
                 
                throw new ArgumentException($"Value is a name, but not one of the named constants defined for the enum of type: {enumType}.", nameof(value));
            }
            else
            {
                return (Enum)Enum.ToObject(enumType, value);
            }

            return null;
        }

        // This is simple right now and will include FlagsAttribute later.
        public static object EnumToExportContract(Enum value)
        {

            FieldInfo fi = value.GetType().GetField(value.ToString());

            ExportAttribute[] attributes =
                (ExportAttribute[])fi.GetCustomAttributes(typeof(ExportAttribute), false);

            var enumConversionType = ConvertEnum.Default;

            object contractName = null;

            if (attributes != null && attributes.Length > 0)
            {
                enumConversionType = attributes[0].EnumValue;
                if (enumConversionType != ConvertEnum.Numeric)
                    contractName = attributes[0].ContractName;

            }

            if (contractName == null)
                contractName = value;

            switch (enumConversionType)
            {
                case ConvertEnum.ToLower:
                    contractName = contractName.ToString().ToLower();
                    break;
                case ConvertEnum.ToUpper:
                    contractName = contractName.ToString().ToUpper();
                    break;
                case ConvertEnum.Numeric:
                    contractName = (int)Enum.Parse(value.GetType(), contractName.ToString());
                    break;
                default:
                    contractName = contractName.ToString();
                    break;
            }

            return contractName;
        }

    }

    public class JSException : Exception
    {
        public JSException(string msg) : base(msg) { }
    }

    public class JSObject : IDisposable
    {
        public int JSHandle { get; internal set; }
        internal GCHandle Handle;
        internal object RawObject;

        internal JSObject(int js_handle)
        {
            this.JSHandle = js_handle;
            this.Handle = GCHandle.Alloc(this);
        }

        internal JSObject(int js_id, object raw_obj)
        {
            this.JSHandle = js_id;
            this.Handle = GCHandle.Alloc(this);
            this.RawObject = raw_obj;
        }

        public object Invoke(string method, params object[] args)
        {
            int exception = 0;
            var res = Runtime.InvokeJSWithArgs(JSHandle, method, args, out exception);
            if (exception != 0)
                throw new JSException((string)res);
            return res;
        }


        public object GetObjectProperty(string expr)
        {

            int exception = 0;
            var propertyValue = Runtime.GetObjectProperty(JSHandle, expr, out exception);

            if (exception != 0)
                throw new JSException((string)propertyValue);

            return propertyValue;

        }

        public void SetObjectProperty(string expr, object value, bool createIfNotExists = true, bool hasOwnProperty = false)
        {

            int exception = 0;
            var setPropResult = Runtime.SetObjectProperty(JSHandle, expr, value, createIfNotExists, hasOwnProperty, out exception);
            if (exception != 0)
                throw new JSException($"Error setting {expr} on (js-obj js '{JSHandle}' mono '{(IntPtr)Handle} raw '{RawObject != null})");

        }

        protected void FreeHandle()
        {

            Runtime.InvokeJS("BINDING.mono_wasm_free_handle(" + JSHandle + ");");
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return JSHandle == (obj as JSObject).JSHandle;
        }

        public override int GetHashCode()
        {
            return JSHandle;
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (JSHandle < 0)
                return;

            if (disposing)
            {

                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            FreeHandle();
        }

        public override string ToString()
        {
            return $"(js-obj js '{JSHandle}' mono '{(IntPtr)Handle} raw '{RawObject != null})";
        }

    }

    public enum ConvertEnum
    {
        Default,
        ToLower,
        ToUpper,
        Numeric
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field, 
                    AllowMultiple = true, Inherited = false)]
    public class ExportAttribute : Attribute
    {
        public ExportAttribute() : this(null, null)
        {
        }

        public ExportAttribute(Type contractType) : this(null, contractType)
        {
        }

        public ExportAttribute(string contractName) : this(contractName, null)
        {
        }

        public ExportAttribute(string contractName, Type contractType)
        {
            ContractName = contractName;
            ContractType = contractType;
        }

        public string ContractName { get; }

        public Type ContractType { get; }
        public ConvertEnum EnumValue { get; set; }
    }

}
