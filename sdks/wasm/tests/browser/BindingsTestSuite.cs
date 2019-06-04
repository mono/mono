using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAssembly;
using WebAssembly.Core;

namespace TestSuite
{
    public class Program
    {
        private static Uint8ClampedArray Uint8ClampedArrayFrom ()
        {
            var clamped = new byte[50];
            return Uint8ClampedArray.From(clamped);
        }

        private static Uint8Array Uint8ArrayFrom ()
        {
            var array = new byte[50];
            return Uint8Array.From(array);
        }
        private static Uint16Array Uint16ArrayFrom ()
        {
            var array = new ushort[50];
            return Uint16Array.From(array);
        }
        private static Uint32Array Uint32ArrayFrom ()
        {
            var array = new uint[50];
            return Uint32Array.From(array);
        }
        private static Int8Array Int8ArrayFrom ()
        {
            var array = new sbyte[50];
            return Int8Array.From(array);
        }
        private static Int16Array Int16ArrayFrom ()
        {
            var array = new short[50];
            return Int16Array.From(array);
        }
        private static Int32Array Int32ArrayFrom ()
        {
            var array = new int[50];
            return Int32Array.From(array);
        }
        private static Float32Array Float32ArrayFrom ()
        {
            var array = new float[50];
            return Float32Array.From(array);
        }
        private static Float64Array Float64ArrayFrom ()
        {
            var array = new double[50];
            return Float64Array.From(array);
        }
        private static TypedArrayTypeCode TypedArrayType (ITypedArray arr)
        {
            return arr.GetTypedArrayType();
        }

        private static Uint8ClampedArray Uint8ClampedArrayFromSharedArrayBuffer (SharedArrayBuffer sab)
        {
            return new Uint8ClampedArray(sab);
        }

        private static Uint8Array Uint8ArrayFromSharedArrayBuffer (SharedArrayBuffer sab)
        {
            return new Uint8Array(sab);
        }
        private static Uint16Array Uint16ArrayFromSharedArrayBuffer (SharedArrayBuffer sab)
        {
            return new Uint16Array(sab);
        }
        private static Uint32Array Uint32ArrayFromSharedArrayBuffer (SharedArrayBuffer sab)
        {
            return new Uint32Array(sab);
        }
        private static Int8Array Int8ArrayFromSharedArrayBuffer (SharedArrayBuffer sab)
        {
            return new Int8Array(sab);
        }
        private static Int16Array Int16ArrayFromSharedArrayBuffer (SharedArrayBuffer sab)
        {
            return new Int16Array(sab);
        }
        private static Int32Array Int32ArrayFromSharedArrayBuffer (SharedArrayBuffer sab)
        {
            return new Int32Array(sab);
        }
        private static Float32Array Float32ArrayFromSharedArrayBuffer (SharedArrayBuffer sab)
        {
            return new Float32Array(sab);
        }
        private static Float64Array Float64ArrayFromSharedArrayBuffer (SharedArrayBuffer sab)
        {
            return new Float64Array(sab);
        }

        private static int FunctionSumCall (int a, int b) 
        {
            var sum = new Function("a", "b", "return a + b");
            return (int)sum.Call(null, a, b);
        }

        private static double FunctionSumCallD (double a, double b) 
        {
            var sum = new Function("a", "b", "return a + b");
            return Math.Round((double)sum.Call(null, a, b), 2);
        }
        private static int FunctionSumApply (int a, int b) 
        {
            var sum = new Function("a", "b", "return a + b");
            return (int)sum.Apply(null, new object[] { a, b });
        }

        private static double FunctionSumApplyD (double a, double b) 
        {
            var sum = new Function("a", "b", "return a + b");
            return Math.Round((double)sum.Apply(null, new object[] { a, b }), 2);
        }

        private static object FunctionMathMin (WebAssembly.Core.Array array) 
        {
            object[] parms = new object[array.Length];
            for (int x = 0; x < array.Length; x++)
                parms[x] = array[x];

            var math = (JSObject)Runtime.GetGlobalObject("Math");
            var min = (Function)math.GetObjectProperty("min");
            return min.Apply(null, parms);
        }

    }
}
