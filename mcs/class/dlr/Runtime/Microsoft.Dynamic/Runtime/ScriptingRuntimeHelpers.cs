/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Interpreter;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// These are some generally useful helper methods. Currently the only methods are those to
    /// cached boxed representations of commonly used primitive types so that they can be shared.
    /// This is useful to most dynamic languages that use object as a universal type.
    /// 
    /// The methods in RuntimeHelepers are caleld by the generated code. From here the methods may
    /// dispatch to other parts of the runtime to get bulk of the work done, but the entry points
    /// should be here.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static partial class ScriptingRuntimeHelpers {
        private const int MIN_CACHE = -100;
        private const int MAX_CACHE = 1000;
        private static readonly object[] cache = MakeCache();

        /// <summary>
        /// A singleton boxed boolean true.
        /// </summary>
        public static readonly object True = true;

        /// <summary>
        ///A singleton boxed boolean false.
        /// </summary>
        public static readonly object False = false;

        internal static readonly MethodInfo BooleanToObjectMethod = typeof(ScriptingRuntimeHelpers).GetMethod("BooleanToObject");
        internal static readonly MethodInfo Int32ToObjectMethod = typeof(ScriptingRuntimeHelpers).GetMethod("Int32ToObject");

        private static object[] MakeCache() {
            object[] result = new object[MAX_CACHE - MIN_CACHE];

            for (int i = 0; i < result.Length; i++) {
                result[i] = (object)(i + MIN_CACHE);
            }

            return result;
        }

#if DEBUG
        public static void NoteException(Exception e) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Exceptions, "LightEH Missed: " + e.GetType());
        }
#endif

        /// <summary>
        /// Gets a singleton boxed value for the given integer if possible, otherwise boxes the integer.
        /// </summary>
        /// <param name="value">The value to box.</param>
        /// <returns>The boxed value.</returns>
        public static object Int32ToObject(Int32 value) {
            // caches improves pystone by ~5-10% on MS .Net 1.1, this is a very integer intense app
            // TODO: investigate if this still helps perf. There's evidence that it's harmful on
            // .NET 3.5 and 4.0
            if (value < MAX_CACHE && value >= MIN_CACHE) {
                return cache[value - MIN_CACHE];
            }
            return (object)value;
        }

        private static readonly string[] chars = MakeSingleCharStrings();

        private static string[] MakeSingleCharStrings() {
            string[] result = new string[255];

            for (char ch = (char)0; ch < result.Length; ch++) {
                result[ch] = new string(ch, 1);
            }

            return result;
        }

        public static object BooleanToObject(bool value) {
            return value ? True : False;
        }

        public static string CharToString(char ch) {
            if (ch < 255) return chars[ch];
            return new string(ch, 1);
        }

        internal static object GetPrimitiveDefaultValue(Type type) {
            switch (type.GetTypeCode()) {
                case TypeCode.Boolean: return ScriptingRuntimeHelpers.False;
                case TypeCode.SByte: return default(SByte);
                case TypeCode.Byte: return default(Byte);
                case TypeCode.Char: return default(Char);
                case TypeCode.Int16: return default(Int16);
                case TypeCode.Int32: return ScriptingRuntimeHelpers.Int32ToObject(0);
                case TypeCode.Int64: return default(Int64);
                case TypeCode.UInt16: return default(UInt16);
                case TypeCode.UInt32: return default(UInt32);
                case TypeCode.UInt64: return default(UInt64);
                case TypeCode.Single: return default(Single);
                case TypeCode.Double: return default(Double);
#if FEATURE_DBNULL
                case TypeCode.DBNull: return default(DBNull);
#endif
                case TypeCode.DateTime: return default(DateTime);
                case TypeCode.Decimal: return default(Decimal);
                default: return null;
            }
        }

        public static ArgumentTypeException SimpleTypeError(string message) {
            return new ArgumentTypeException(message);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")] // TODO: fix
        public static Exception CannotConvertError(Type toType, object value) {
            return SimpleTypeError(String.Format("Cannot convert {0}({1}) to {2}", CompilerHelpers.GetType(value).Name, value, toType.Name));
        }

        public static Exception SimpleAttributeError(string message) {
            //TODO: localize
            return new MissingMemberException(message);
        }

        public static object ReadOnlyAssignError(bool field, string fieldName) {
            if (field) {
                throw Error.FieldReadonly(fieldName);
            } else {
                throw Error.PropertyReadonly(fieldName);
            }
        }

        /// <summary>
        /// Helper method to create an instance.  Work around for Silverlight where Activator.CreateInstance
        /// is SecuritySafeCritical.
        /// 
        /// TODO: Why can't we just emit the right thing for default(T)?
        /// It's always null for reference types and it's well defined for value types
        /// </summary>
        public static T CreateInstance<T>() {
            return default(T);
        }

        // TODO: can't we just emit a new array?
        public static T[] CreateArray<T>(int args) {
            return new T[args];
        }
        
        /// <summary>
        /// EventInfo.EventHandlerType getter is marked SecuritySafeCritical in CoreCLR
        /// This method is to get to the property without using Reflection
        /// </summary>
        /// <param name="eventInfo"></param>
        /// <returns></returns>
        public static Type GetEventHandlerType(EventInfo eventInfo) {
            ContractUtils.RequiresNotNull(eventInfo, "eventInfo");
            return eventInfo.EventHandlerType;
        }

        public static IList<string> GetStringMembers(IList<object> members) {
            List<string> res = new List<string>();
            foreach (object o in members) {
                string str = o as string;
                if (str != null) {
                    res.Add(str);
                }
            }
            return res;
        }
#if !MONO_INTERPRETER
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")] // TODO: fix
        public static void SetEvent(EventTracker eventTracker, object value) {
            EventTracker et = value as EventTracker;
            if (et != null) {
                if (et != eventTracker) {
                    throw Error.UnexpectedEvent(eventTracker.DeclaringType.Name,
                                                eventTracker.Name,
                                                et.DeclaringType.Name,
                                                et.Name);
                }
                return;
            }

            BoundMemberTracker bmt = value as BoundMemberTracker;
            if (bmt == null) {
                throw Error.ExpectedBoundEvent(CompilerHelpers.GetType(value).Name);
            }
            if (bmt.BoundTo.MemberType != TrackerTypes.Event) throw Error.ExpectedBoundEvent(bmt.BoundTo.MemberType.ToString());

            if (bmt.BoundTo != eventTracker) throw Error.UnexpectedEvent(
                eventTracker.DeclaringType.Name,
                eventTracker.Name,
                bmt.BoundTo.DeclaringType.Name,
                bmt.BoundTo.Name);
        }
#endif
        // TODO: just emit this in the generated code
        public static bool CheckDictionaryMembers(IDictionary dict, string[] names) {
            if (dict.Count != names.Length) return false;

            foreach (string name in names) {
                if (!dict.Contains(name)) {
                    return false;
                }
            }
            return true;
        }

        // TODO: just emit this in the generated code
        [Obsolete("use MakeIncorrectBoxTypeError instead")]
        public static T IncorrectBoxType<T>(object received) {
            throw Error.UnexpectedType("StrongBox<" + typeof(T).Name + ">", CompilerHelpers.GetType(received).Name);
        }

        public static Exception MakeIncorrectBoxTypeError(Type type, object received) {
            return Error.UnexpectedType("StrongBox<" + type.Name + ">", CompilerHelpers.GetType(received).Name);
        }
        
        /// <summary>
        /// Provides the test to see if an interpreted call site should switch over to being compiled.
        /// </summary>
        public static bool InterpretedCallSiteTest(bool restrictionResult, object bindingInfo) {
            if (restrictionResult) {
                CachedBindingInfo bindInfo = (CachedBindingInfo)bindingInfo;
                if (bindInfo.CompilationThreshold >= 0) {
                    // still interpreting...
                    bindInfo.CompilationThreshold--;
                    return true;
                }
#if SILVERLIGHT
                if (PlatformAdaptationLayer.IsCompactFramework) {
                    bindInfo.CompilationThreshold = Int32.MaxValue;
                    return true;
                }
#endif
                return bindInfo.CheckCompiled();
            }
            return false;
        }
    }
}
