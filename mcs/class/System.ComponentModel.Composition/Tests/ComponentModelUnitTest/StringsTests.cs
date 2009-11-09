// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Internal;
using System.Threading;
using System.Globalization;

namespace System
{
    [TestClass]
    public class StringsTests
    {
        [TestMethod]
        public void PropertiesAreInsyncWithResources()
        {
            var properties = GetStringProperties();

            Assert.IsTrue(properties.Length > 0, "Expected to find at least one string property in Strings.cs.");

            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(null, (object[])null);

                Assert.IsNotNull(value, "Property '{0}' does not have an associated string in Strings.resx.", property.Name);
            }
        }

        private static PropertyInfo[] GetStringProperties()
        {
            PropertyInfo[] properties = typeof(Strings).GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            return properties.Where(property => 
            {
                return !CanIgnore(property);

            }).ToArray();
        }

        private static bool CanIgnore(PropertyInfo property)
        {
            switch (property.Name)
            {
                case "Culture":
                case "ResourceManager":
                    return true;
            }

            return false;
        }

#if !SILVERLIGHT
        [TestMethod]
        public void LocalizedResourcesArePickedUpBasedOnThreadCulture()
        {
            // verify each property against an en-US resource manager
            bool wasVerified = false;
            bool areLocalizedCLRResourcesAvailable = AreLocalizedCLRResourcesAvailable();
            foreach (var stringsPropInfo in GetStringProperties())
            {
                string referenceValue = String.Empty;
                PerformOnEnUsThread(() =>
                {
                    referenceValue = (string)stringsPropInfo.GetValue(null, (object[])null);
                });
                string actualValue = (string)stringsPropInfo.GetValue(null, (object[])null);

                // determine if LangPacks are installed for the the CurrentUICulture. If not, default to english
                if (areLocalizedCLRResourcesAvailable)
                {
                    Assert.AreNotEqual(referenceValue, actualValue, "Failed to pick up localized resources for UI culture '{0}'", Thread.CurrentThread.CurrentUICulture.Name);
                }
                else
                {
                    Assert.AreEqual(referenceValue, actualValue);
                }

                wasVerified = true;
            }

            Assert.IsTrue(wasVerified);
        }


        private bool AreLocalizedCLRResourcesAvailable()
        {
            if (Thread.CurrentThread.CurrentUICulture.LCID == 1033) // en-US
            {
                return false;
            }

            object regValue = Microsoft.Win32.Registry.GetValue(
                GetRegPath(),
                "Install",
                0);

            return (regValue != null) && ((int)regValue == 1);
        }

        private string GetRegPath()
        {
            var path = String.Format(
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v{0}\Client\{1}",
                typeof(string).Assembly.GetName().Version.Major,
                Thread.CurrentThread.CurrentUICulture.LCID);

            return path;
        }
        
        private static void PerformOnEnUsThread(Action action)
        {
            Thread enUsThread = new Thread(new ThreadStart(action));
            enUsThread.CurrentUICulture = new CultureInfo("en-US");
            enUsThread.Start();
            enUsThread.Join();
        }
#endif
    }
}