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
    }
}
