namespace System.Data
{

    using java.util;

    using System.Globalization;
    //using clr.System;

    public class Res
    {
        private static readonly String FILE_NAME = "SystemData";
        private static readonly ResourceBundle _resource =
            ResourceBundle.getBundle(FILE_NAME);

        public static String GetString(String name, Object[] args)
        {
            return GetString(null, name, args);
        }

        public static String GetString(CultureInfo culture, String name, Object[] args)
        {
            try
            {
                String str = _resource.getString(name);
                if (args != null && (int) args.Length > 0)
                {
                    return String.Format(str, args);
                }
                else
                {
                    return str;
                }
            }
            catch (MissingResourceException)
            {
                return null;
            }
        }

        public static String GetString(String name)
        {
            return GetString(null, name);
        }

        public static String GetString(CultureInfo culture, String name)
        {
            try
            {
                return _resource.getString(name);
            }
            catch (MissingResourceException)
            {
                return null;
            }
        }

        public static bool GetBoolean(String name)
        {
            return GetBoolean(name);
        }

        public static bool GetBoolean(CultureInfo culture, String name)
        {
            // This online demo only decompiles 10 methods in each class
            return false;
        }

        public static char GetChar(String name)
        {
            return GetChar(null, name);
        }

        public static char GetChar(CultureInfo culture, String name)
        {
            // This online demo only decompiles 10 methods in each class
            return (char)0;
        }

        public static int GetByte(String name)
        {
            return GetByte(null, name);
        }

        public static int GetByte(CultureInfo culture, String name)
        {
            return 0;
        }

        public static short GetShort(String name)
        {
            return GetShort(null, name);
        }

        public static short GetShort(CultureInfo culture, String name)
        {
            // This online demo only decompiles 10 methods in each class
            return 0;
        }

        public static int GetInt(String name)
        {
            return GetInt(null, name);
        }

        public static int GetInt(CultureInfo culture, String name)
        {
            // This online demo only decompiles 10 methods in each class
            return 0;
        }

        public static long GetLong(String name)
        {
            return GetLong(null, name);
        }

        public static long GetLong(CultureInfo culture, String name)
        {
            // This online demo only decompiles 10 methods in each class
            return 0;
        }

        public static float GetFloat(String name)
        {
            return GetFloat(null, name);
        }

        public static float GetFloat(CultureInfo culture, String name)
        {
            // This online demo only decompiles 10 methods in each class
            return 0.0f;
        }

        public static double GetDouble(String name)
        {
            return GetDouble(null, name);
        }

        public static double GetDouble(CultureInfo culture, String name)
        {
            // This online demo only decompiles 10 methods in each class
            return 0.0;
        }

        public static Object GetObject(String name)
        {
            return GetObject(null, name);
        }

        public static Object GetObject(CultureInfo culture, String name)
        {
            // This online demo only decompiles 10 methods in each class
            return null;
        }
    }
}