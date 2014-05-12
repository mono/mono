using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpCompress.Archive;
using SharpCompress.Common;
using SharpCompress.IO;

namespace SharpCompress
{
    internal static class Utility
    {
        public static ReadOnlyCollection<T> ToReadOnly<T>(this IEnumerable<T> items)
        {
            return new ReadOnlyCollection<T>(items.ToList());
        }

        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        public static int URShift(int number, int bits)
        {
            if (number >= 0)
                return number >> bits;
            else
                return (number >> bits) + (2 << ~bits);
        }

        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        public static int URShift(int number, long bits)
        {
            return URShift(number, (int)bits);
        }

        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        public static long URShift(long number, int bits)
        {
            if (number >= 0)
                return number >> bits;
            else
                return (number >> bits) + (2L << ~bits);
        }

        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        public static long URShift(long number, long bits)
        {
            return URShift(number, (int)bits);
        }

        /// <summary>
        /// Fills the array with an specific value from an specific index to an specific index.
        /// </summary>
        /// <param name="array">The array to be filled.</param>
        /// <param name="fromindex">The first index to be filled.</param>
        /// <param name="toindex">The last index to be filled.</param>
        /// <param name="val">The value to fill the array with.</param>
        public static void Fill<T>(T[] array, int fromindex, int toindex, T val) where T : struct
        {
            if (array.Length == 0)
            {
                throw new NullReferenceException();
            }
            if (fromindex > toindex)
            {
                throw new ArgumentException();
            }
            if ((fromindex < 0) || ((System.Array)array).Length < toindex)
            {
                throw new IndexOutOfRangeException();
            }
            for (int index = (fromindex > 0) ? fromindex-- : fromindex; index < toindex; index++)
            {
                array[index] = val;
            }
        }

        /// <summary>
        /// Fills the array with an specific value.
        /// </summary>
        /// <param name="array">The array to be filled.</param>
        /// <param name="val">The value to fill the array with.</param>
        public static void Fill<T>(T[] array, T val) where T : struct
        {
            Fill(array, 0, array.Length, val);
        }

        public static void SetSize(this List<byte> list, int count)
        {
            if (count > list.Count)
            {
                for (int i = list.Count; i < count; i++)
                {
                    list.Add(0x0);
                }
            }
            else
            {
                byte[] temp = new byte[count];
                list.CopyTo(temp, 0);
                list.Clear();
                list.AddRange(temp);
            }
        }

        /// <summary> Read a int value from the byte array at the given position (Big Endian)
        /// 
        /// </summary>
        /// <param name="array">the array to read from
        /// </param>
        /// <param name="pos">the offset
        /// </param>
        /// <returns> the value
        /// </returns>
        public static int readIntBigEndian(byte[] array, int pos)
        {
            int temp = 0;
            temp |= array[pos] & 0xff;
            temp <<= 8;
            temp |= array[pos + 1] & 0xff;
            temp <<= 8;
            temp |= array[pos + 2] & 0xff;
            temp <<= 8;
            temp |= array[pos + 3] & 0xff;
            return temp;
        }

        /// <summary> Read a short value from the byte array at the given position (little
        /// Endian)
        /// 
        /// </summary>
        /// <param name="array">the array to read from
        /// </param>
        /// <param name="pos">the offset
        /// </param>
        /// <returns> the value
        /// </returns>
        public static short readShortLittleEndian(byte[] array, int pos)
        {
            return BitConverter.ToInt16(array, pos);
        }

        /// <summary> Read an int value from the byte array at the given position (little
        /// Endian)
        /// 
        /// </summary>
        /// <param name="array">the array to read from
        /// </param>
        /// <param name="pos">the offset
        /// </param>
        /// <returns> the value
        /// </returns>
        public static int readIntLittleEndian(byte[] array, int pos)
        {
            return BitConverter.ToInt32(array, pos);
        }

        /// <summary> Write an int value into the byte array at the given position (Big endian)
        /// 
        /// </summary>
        /// <param name="array">the array
        /// </param>
        /// <param name="pos">the offset
        /// </param>
        /// <param name="value">the value to write
        /// </param>
        public static void writeIntBigEndian(byte[] array, int pos, int value)
        {
            array[pos] = (byte)((Utility.URShift(value, 24)) & 0xff);
            array[pos + 1] = (byte)((Utility.URShift(value, 16)) & 0xff);
            array[pos + 2] = (byte)((Utility.URShift(value, 8)) & 0xff);
            array[pos + 3] = (byte)((value) & 0xff);
        }

        /// <summary> Write a short value into the byte array at the given position (little
        /// endian)
        /// 
        /// </summary>
        /// <param name="array">the array
        /// </param>
        /// <param name="pos">the offset
        /// </param>
        /// <param name="value">the value to write
        /// </param>
#if PORTABLE || NETFX_CORE
        public static void WriteLittleEndian(byte[] array, int pos, short value)
        {
            byte[] newBytes = BitConverter.GetBytes(value);
            Array.Copy(newBytes, 0, array, pos, newBytes.Length);
        }
#else
        public static unsafe void WriteLittleEndian(byte[] array, int pos, short value)
        {
            fixed (byte* numRef = &(array[pos]))
            {
                *((short*)numRef) = value;
            }
        }
#endif

        /// <summary> Increment a short value at the specified position by the specified amount
        /// (little endian).
        /// </summary>
        public static void incShortLittleEndian(byte[] array, int pos, short incrementValue)
        {
            short existingValue = BitConverter.ToInt16(array, pos);
            existingValue += incrementValue;
            WriteLittleEndian(array, pos, existingValue);
            //int c = Utility.URShift(((array[pos] & 0xff) + (dv & 0xff)), 8);
            //array[pos] = (byte)(array[pos] + (dv & 0xff));
            //if ((c > 0) || ((dv & 0xff00) != 0))
            //{
            //    array[pos + 1] = (byte)(array[pos + 1] + ((Utility.URShift(dv, 8)) & 0xff) + c);
            //}
        }

        /// <summary> Write an int value into the byte array at the given position (little
        /// endian)
        /// 
        /// </summary>
        /// <param name="array">the array
        /// </param>
        /// <param name="pos">the offset
        /// </param>
        /// <param name="value">the value to write
        /// </param>
#if PORTABLE || NETFX_CORE
        public static void WriteLittleEndian(byte[] array, int pos, int value)
        {
            byte[] newBytes = BitConverter.GetBytes(value);
            Array.Copy(newBytes, 0, array, pos, newBytes.Length);
        }
#else
        public static unsafe void WriteLittleEndian(byte[] array, int pos, int value)
        {
            fixed (byte* numRef = &(array[pos]))
            {
                *((int*)numRef) = value;
            }
        }
#endif

        public static void Initialize<T>(this T[] array, Func<T> func)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = func();
            }
        }

        public static void AddRange<T>(this ICollection<T> destination, IEnumerable<T> source)
        {
            foreach (T item in source)
            {
                destination.Add(item);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (T item in items)
            {
                action(item);
            }
        }

        public static IEnumerable<T> AsEnumerable<T>(this T item)
        {
            yield return item;
        }

        public static void CheckNotNull(this object obj, string name)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        public static void CheckNotNullOrEmpty(this string obj, string name)
        {
            obj.CheckNotNull(name);
            if (obj.Length == 0)
            {
                throw new ArgumentException("String is empty.");
            }
        }

        public static void Skip(this Stream source, long advanceAmount)
        {
            byte[] buffer = new byte[32 * 1024];
            int read = 0;
            int readCount = 0;
            do
            {
                readCount = buffer.Length;
                if (readCount > advanceAmount)
                {
                    readCount = (int)advanceAmount;
                }
                read = source.Read(buffer, 0, readCount);
                if (read < 0)
                {
                    break;
                }
                advanceAmount -= read;
                if (advanceAmount == 0)
                {
                    break;
                }
            } while (true);
        }

        public static void SkipAll(this Stream source)
        {
            byte[] buffer = new byte[32 * 1024];
            do
            {
            } while (source.Read(buffer, 0, buffer.Length) == buffer.Length);
        }


        public static byte[] UInt32ToBigEndianBytes(uint x)
        {
            return new byte[]
                       {
                           (byte) ((x >> 24) & 0xff),
                           (byte) ((x >> 16) & 0xff),
                           (byte) ((x >> 8) & 0xff),
                           (byte) (x & 0xff)
                       };
        }

        public static DateTime DosDateToDateTime(UInt16 iDate, UInt16 iTime)
        {
            int year = iDate / 512 + 1980;
            int month = iDate % 512 / 32;
            int day = iDate % 512 % 32;
            int hour = iTime / 2048;
            int minute = iTime % 2048 / 32;
            int second = iTime % 2048 % 32 * 2;

            if (iDate == UInt16.MaxValue || month == 0 || day == 0)
            {
                year = 1980;
                month = 1;
                day = 1;
            }

            if (iTime == UInt16.MaxValue)
            {
                hour = minute = second = 0;
            }

            DateTime dt;
            try
            {
                dt = new DateTime(year, month, day, hour, minute, second);
            }
            catch
            {
                dt = new DateTime();
            }
            return dt;
        }

        public static uint DateTimeToDosTime(this DateTime? dateTime)
        {
            if (dateTime == null)
            {
                return 0;
            }
            return (uint)(
                              (dateTime.Value.Second / 2) | (dateTime.Value.Minute << 5) | (dateTime.Value.Hour << 11) |
                              (dateTime.Value.Day << 16) | (dateTime.Value.Month << 21) |
                              ((dateTime.Value.Year - 1980) << 25));
        }


        public static DateTime DosDateToDateTime(UInt32 iTime)
        {
            return DosDateToDateTime((UInt16)(iTime / 65536),
                                     (UInt16)(iTime % 65536));
        }

        public static DateTime DosDateToDateTime(Int32 iTime)
        {
            return DosDateToDateTime((UInt32)iTime);
        }

        public static long TransferTo(this Stream source, Stream destination)
        {
            byte[] array = new byte[81920];
            int count;
            long total = 0;
            while ((count = source.Read(array, 0, array.Length)) != 0)
            {
                total += count;
                destination.Write(array, 0, count);
            }
            return total;
        }

        public static bool ReadFully(this Stream stream, byte[] buffer)
        {
            int total = 0;
            int read;
            while ((read = stream.Read(buffer, total, buffer.Length - total)) > 0)
            {
                total += read;
                if (total >= buffer.Length)
                {
                    return true;
                }
            }
            return (total >= buffer.Length);
        }

        public static string TrimNulls(this string source)
        {
            return source.Replace('\0', ' ').Trim();
        }

        public static bool BinaryEquals(this byte[] source, byte[] target)
        {
            if (source.Length != target.Length)
            {
                return false;
            }
            for (int i = 0; i < source.Length; ++i)
            {
                if (source[i] != target[i])
                {
                    return false;
                }
            }
            return true;
        }

#if PORTABLE || NETFX_CORE
        public static void CopyTo(this byte[] array, byte[] destination, int index)
        {
            Array.Copy(array, 0, destination, index, array.Length);
        }

        public static long HostToNetworkOrder(long host)
        {
            return (int)((long)HostToNetworkOrder((int)host)
                & unchecked((long)(unchecked((ulong)-1))) << 32
                | ((long)HostToNetworkOrder((int)((int)host >> 32)) & unchecked((long)(unchecked((ulong)-1)))));
        }
        public static int HostToNetworkOrder(int host)
        {
            return (int)((int)(HostToNetworkOrder((short)host) & -1) << 16 | (HostToNetworkOrder((short)(host >> 16)) & -1));
        }
        public static short HostToNetworkOrder(short host)
        {
            return (short)((int)(host & 255) << 8 | ((int)host >> 8 & 255));
        }
        public static long NetworkToHostOrder(long network)
        {
            return HostToNetworkOrder(network);
        }
        public static int NetworkToHostOrder(int network)
        {
            return HostToNetworkOrder(network);
        }
        public static short NetworkToHostOrder(short network)
        {
            return HostToNetworkOrder(network);
        }
#endif
    }
}