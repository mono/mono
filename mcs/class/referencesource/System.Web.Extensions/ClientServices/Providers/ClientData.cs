//------------------------------------------------------------------------------
// <copyright file="ClientData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.ClientServices.Providers
{
    using System;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.AccessControl;
    using System.Globalization;
    using System.Data;
    using System.Data.Common;
    using System.Data.OleDb;
    using System.IO;
    using System.Windows.Forms;
    using System.Data.SqlClient;
    using System.Xml.Serialization;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Schema;
    using System.Xml;
    using System.Collections.Specialized;
    using System.IO.IsolatedStorage;

    internal class ClientData
    {
        internal enum ClientDateStoreOrderEnum {
            LastLoggedInUserName = 0,
            LastLoggedInDateUtc = 1,
            PasswordHash = 2,
            PasswordSalt = 3,
            Roles = 4,
            RolesCachedDateUtc = 5,
            SettingsNames = 6,
            SettingsStoredAs = 7,
            SettingsValues = 8,
            SettingsNeedReset = 9,
            SettingsCacheIsMoreFresh = 10,
            CookieNames = 11,
            CookieValues = 12
        }

        private const int       _NumStoredValues     = 13;
        private static string[] _StoredValueNames = new string[_NumStoredValues] {
                                                "LastLoggedInUserName",
                                                "LastLoggedInDateUtc",
                                                "PasswordHash",
                                                "PasswordSalt",
                                                "Roles",
                                                "RolesCachedDateUtc",
                                                "SettingsNames",
                                                "SettingsStoredAs",
                                                "SettingsValues",
                                                "SettingsNeedReset",
                                                "SettingsCacheIsMoreFresh",
                                                "CookieNames",
                                                "CookieValues"};
        private object[] _StoredValues = new object[_NumStoredValues] {
                                                "",
                                                DateTime.UtcNow.AddYears(-1),
                                                string.Empty,
                                                string.Empty,
                                                new string[0],
                                                DateTime.UtcNow.AddYears(-1),
                                                new string[0],
                                                new string[0],
                                                new string[0],
                                                false,
                                                false,
                                                new string[0],
                                                new string[0]};

        private ClientData() { }

        private ClientData(XmlReader reader)
        {
            reader.ReadStartElement("ClientData");

            for (int iter = 0; iter < _NumStoredValues; iter++) {

                reader.ReadStartElement(_StoredValueNames[iter]);

                if (_StoredValues[iter] is string)
                    _StoredValues[iter] = reader.ReadContentAsString();
                else if (_StoredValues[iter] is DateTime) {
                    string s = reader.ReadContentAsString();
                    long l = long.Parse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    _StoredValues[iter] = DateTime.FromFileTimeUtc(l);
                } else if (_StoredValues[iter] is bool) {
                    string s = reader.ReadContentAsString();
                    _StoredValues[iter] = !(string.IsNullOrEmpty(s) || s != "1");
                } else {
                    _StoredValues[iter] = ReadStringArray(reader);
                }
                reader.ReadEndElement();
            }

            reader.ReadEndElement();
        }

        private static string[] ReadStringArray(XmlReader reader)
        {
            //string count = reader.GetAttribute("count");
            //if (string.IsNullOrEmpty(count))
            //    return new string[0];
            StringCollection sc = new StringCollection();
            while (reader.IsStartElement())
            {
                reader.ReadStartElement("item");
                sc.Add(reader.ReadContentAsString());
                reader.ReadEndElement();
            }

            string[] retValue = new string[sc.Count];
            sc.CopyTo(retValue, 0);
            return retValue;
        }

        private static void WriteStringArray(XmlWriter writer, string [] arrToWrite)
        {
            //writer.WriteAttributeString("count", arrToWrite.Length.ToString());
            if (arrToWrite.Length == 0)
                writer.WriteValue(string.Empty);
            for (int iter = 0; iter < arrToWrite.Length; iter++) {
                writer.WriteStartElement("item");
                writer.WriteValue((arrToWrite[iter]==null) ? string.Empty : arrToWrite[iter]);
                writer.WriteEndElement();
            }
        }

        // App data
        internal string LastLoggedInUserName { get { return (string)_StoredValues[(int)ClientDateStoreOrderEnum.LastLoggedInUserName]; } set { _StoredValues[(int)ClientDateStoreOrderEnum.LastLoggedInUserName] = value; } }
        internal DateTime LastLoggedInDateUtc { get { return (DateTime)_StoredValues[(int)ClientDateStoreOrderEnum.LastLoggedInDateUtc]; } set { _StoredValues[(int)ClientDateStoreOrderEnum.LastLoggedInDateUtc] = value; } }

        // Membership data
        internal string PasswordHash { get { return (string)_StoredValues[(int)ClientDateStoreOrderEnum.PasswordHash]; } set { _StoredValues[(int)ClientDateStoreOrderEnum.PasswordHash] = value; } }
        internal string PasswordSalt { get { return (string)_StoredValues[(int)ClientDateStoreOrderEnum.PasswordSalt]; } set { _StoredValues[(int)ClientDateStoreOrderEnum.PasswordSalt] = value; } }

        // Roles data
        internal string[] Roles { get { return (string[])_StoredValues[(int)ClientDateStoreOrderEnum.Roles]; } set { _StoredValues[(int)ClientDateStoreOrderEnum.Roles] = value; } }
        internal DateTime RolesCachedDateUtc { get { return (DateTime)_StoredValues[(int)ClientDateStoreOrderEnum.RolesCachedDateUtc]; } set { _StoredValues[(int)ClientDateStoreOrderEnum.RolesCachedDateUtc] = value; } }

        // Settings data
        internal string[] SettingsNames { get { return (string[])_StoredValues[(int)ClientDateStoreOrderEnum.SettingsNames]; } set { _StoredValues[(int)ClientDateStoreOrderEnum.SettingsNames] = value; } }
        internal string[] SettingsStoredAs{ get { return (string[])_StoredValues[(int)ClientDateStoreOrderEnum.SettingsStoredAs]; } set { _StoredValues[(int)ClientDateStoreOrderEnum.SettingsStoredAs] = value; } }
        internal string[] SettingsValues { get { return (string[])_StoredValues[(int)ClientDateStoreOrderEnum.SettingsValues]; } set { _StoredValues[(int)ClientDateStoreOrderEnum.SettingsValues] = value; } }

        internal bool SettingsNeedReset { get { return (bool)_StoredValues[(int)ClientDateStoreOrderEnum.SettingsNeedReset]; } set { _StoredValues[(int)ClientDateStoreOrderEnum.SettingsNeedReset] = value; } }
        internal bool SettingsCacheIsMoreFresh { get { return (bool)_StoredValues[(int)ClientDateStoreOrderEnum.SettingsCacheIsMoreFresh]; } set { _StoredValues[(int)ClientDateStoreOrderEnum.SettingsCacheIsMoreFresh] = value; } }

        // Cookie data
        internal string[] CookieNames { get { return (string[])_StoredValues[(int)ClientDateStoreOrderEnum.CookieNames]; } set { _StoredValues[(int)ClientDateStoreOrderEnum.CookieNames] = value; } }
        internal string[] CookieValues { get { return (string[])_StoredValues[(int)ClientDateStoreOrderEnum.CookieValues]; } set { _StoredValues[(int)ClientDateStoreOrderEnum.CookieValues] = value; } }


        // Filename
        private string FileName = string.Empty;
        private bool UsingIsolatedStorage = false;
        private const string _IsolatedDir = "System.Web.Extensions.ClientServices.ClientData";

        internal void Save()
        {
            if (!UsingIsolatedStorage) {
                using (XmlWriter writer = XmlWriter.Create(FileName))  {
                    Save(writer);
                }
            } else {
                using(IsolatedStorageFile f = IsolatedStorageFile.GetUserStoreForAssembly()) {
                    if (f.GetDirectoryNames(_IsolatedDir).Length == 0)
                        f.CreateDirectory(_IsolatedDir);
                    using(IsolatedStorageFileStream fs = new IsolatedStorageFileStream(FileName, FileMode.Create, f)) {
                        using (XmlWriter writer = XmlWriter.Create(fs)) {
                            Save(writer);
                        }
                    }
                }
            }
        }

        private void Save(XmlWriter writer)
        {
            writer.WriteStartElement("ClientData");

            for (int iter = 0; iter < _NumStoredValues; iter++) {

                writer.WriteStartElement(_StoredValueNames[iter]);

                if (_StoredValues[iter] == null) {
                    writer.WriteValue(string.Empty);
                } else if (_StoredValues[iter] is string) {
                    writer.WriteValue(_StoredValues[iter]);
                } else if (_StoredValues[iter] is bool) {
                    writer.WriteValue(((bool)_StoredValues[iter]) ? "1" : "0");
                } else if (_StoredValues[iter] is DateTime) {
                    writer.WriteValue(((DateTime)_StoredValues[iter]).ToFileTimeUtc().ToString("X", CultureInfo.InvariantCulture));
                } else {
                    WriteStringArray(writer, (string[])_StoredValues[iter]);
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.Flush();
        }

        internal static ClientData Load(string username, bool useIsolatedStorage)
        {
            ClientData cd = null;
            string fileName = null;
            if (useIsolatedStorage) {
                fileName = _IsolatedDir + "\\" + SqlHelper.GetPartialDBFileName(username, ".clientdata");
                try {
                    using(IsolatedStorageFile f = IsolatedStorageFile.GetUserStoreForAssembly()) {
                        using(IsolatedStorageFileStream fs = new IsolatedStorageFileStream(fileName, FileMode.Open, f)) {
                            using (XmlReader xr = XmlReader.Create(fs)) {
                                cd = new ClientData(xr);
                            }
                        }
                    }
                } catch {} // ignore exceptions

            } else {
                fileName = SqlHelper.GetFullDBFileName(username, ".clientdata");
                try {
                    if (File.Exists(fileName)) {
                        using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {
                            using (XmlReader xr = XmlReader.Create(fs)) {
                                cd = new ClientData(xr);
                            }
                        }
                    }
                } catch {} // ignore exceptions
            }


            if (cd == null)
                cd = new ClientData();
            cd.UsingIsolatedStorage = useIsolatedStorage;
            cd.FileName = fileName;
            return cd;
        }
    }


    internal static class ClientDataManager
    {
        static private ClientData _applicationClientData;
        static private ClientData _userClientData;
        static private string _curUserName;

        internal static ClientData GetAppClientData(bool useIsolatedStore)
        {
            if (_applicationClientData == null)
                _applicationClientData = ClientData.Load(null, useIsolatedStore);
            return _applicationClientData;
        }

        internal static ClientData GetUserClientData(string username, bool useIsolatedStore)
        {
            if (username != _curUserName) {
                _curUserName = username;
                _userClientData = ClientData.Load(username, useIsolatedStore);
            }
            return _userClientData;
        }

        internal static string GetCookie(string username, string cookieName, bool useIsolatedStore)
        {
            ClientData cd = GetUserClientData(username, useIsolatedStore);
            if (cd.CookieNames == null) {
                cd.CookieNames = new string[0];
                cd.CookieValues = new string[0];
                return null;
            }

            for(int iter=0; iter<cd.CookieNames.Length; iter++)
                if (string.Compare(cookieName, cd.CookieNames[iter], StringComparison.OrdinalIgnoreCase) == 0)
                    return cd.CookieValues[iter];
            return null;
        }

        internal static string StoreCookie(string username, string cookieName, string cookieValue, bool useIsolatedStore)
        {
            ClientData cd = GetUserClientData(username, useIsolatedStore);

            if (cd.CookieNames == null) {
                cd.CookieNames = new string[0];
                cd.CookieValues = new string[0];
            } else {
                for(int iter=0; iter<cd.CookieNames.Length; iter++) {
                    if (cd.CookieValues[iter].StartsWith(cookieName + "=", StringComparison.OrdinalIgnoreCase)) {
                        if (cd.CookieValues[iter] != cookieName + "=" + cookieValue) {
                            cd.CookieValues[iter] = cookieName + "=" + cookieValue;
                            cd.Save();
                        }
                        return cd.CookieNames[iter];
                    }
                }
            }

            string name = Guid.NewGuid().ToString("N");
            string [] names = new string[cd.CookieNames.Length+1];
            string [] vals = new string[cd.CookieNames.Length+1];
            cd.CookieNames.CopyTo(names, 0);
            cd.CookieValues.CopyTo(vals, 0);
            names[cd.CookieNames.Length] = name;
            vals[cd.CookieNames.Length] = cookieName + "=" + cookieValue;

            cd.CookieNames = names;
            cd.CookieValues = vals;
            cd.Save();
            return name;
        }

        internal static void DeleteAllCookies(string username, bool useIsolatedStore)
        {
            ClientData cd = GetUserClientData(username, useIsolatedStore);
            cd.CookieNames = new string[0];
            cd.CookieValues = new string[0];
        }
    }
}
