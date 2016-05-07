using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Globalization;

//using System.Workflow.Activities;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;
using Hosting = System.Workflow.Runtime.Hosting;

namespace System.Workflow.Runtime.Tracking
{

    internal sealed class PropertyHelper
    {
        private PropertyHelper() { }

        #region Internal Static Methods

        internal static void GetProperty(string name, Activity activity, TrackingAnnotationCollection annotations, out TrackingDataItem item)
        {
            item = null;
            object tmp = PropertyHelper.GetProperty(name, activity);

            item = new TrackingDataItem();
            item.FieldName = name;
            item.Data = tmp;
            foreach (string s in annotations)
                item.Annotations.Add(s);
        }

        internal static object GetProperty(string name, object obj)
        {
            if (null == name)
                throw new ArgumentNullException("name");

            if (null == obj)
                throw new ArgumentNullException("obj");
            //
            // Split the names
            string[] names = name.Split(new char[] { '.' });

            object currObj = obj;
            for (int i = 0; i < names.Length; i++)
            {
                if ((null == names[i]) || (0 == names[i].Length))
                    throw new InvalidOperationException(ExecutionStringManager.TrackingProfileInvalidMember);

                object tmp = null;
                PropertyHelper.GetPropertyOrField(names[i], currObj, out tmp);
                //
                // Attempt to resolve runtime values (ParameterBinding, ParameterDeclaration and Bind)
                if (currObj is Activity)
                    currObj = GetRuntimeValue(tmp, (Activity)currObj);
                else
                    currObj = tmp;
            }

            return currObj;
        }

        internal static void GetPropertyOrField(string name, object o, out object obj)
        {
            obj = null;

            if (null == name)
                throw new ArgumentNullException("name");

            if (null == o)
                throw new ArgumentNullException("o");

            Type t = o.GetType();

            string tmp = null, realName = null;
            bool isCollection = false;
            int index = -1;

            if (TryParseIndex(name, out tmp, out index))
                isCollection = true;
            else
                tmp = name;

            object val = null;
            if ((null != tmp) && (tmp.Length > 0))
            {
                if (!NameIsDefined(tmp, o, out realName))
                    throw new MissingMemberException(o.GetType().Name, tmp);
                //
                // Attempt to match default, no parameter (if overloaded)
                // Indexer accesses will fail - we do not handle indexers
                // Do case sensitive here because we have the real name of the matching member
                val = t.InvokeMember(realName,
                                        BindingFlags.Public |
                                        BindingFlags.NonPublic |
                                        BindingFlags.GetProperty |
                                        BindingFlags.GetField |
                                        BindingFlags.Instance |
                                        BindingFlags.Static,
                                        null,
                                        o,
                                        null,
                                        System.Globalization.CultureInfo.InvariantCulture);
            }
            else
                val = o;  // root object is a collection - all that is passed for name is "[1]"

            if (isCollection)
            {
                IEnumerable collection = val as IEnumerable;
                if (null != collection)
                    GetEnumerationMember(collection, index, out obj);
            }
            else
                obj = val;
        }

        internal static void GetEnumerationMember(IEnumerable collection, int index, out object obj)
        {
            obj = null;

            if (null == collection)
                throw new ArgumentNullException("collection");

            IEnumerator e = collection.GetEnumerator();
            int i = 0;
            while (e.MoveNext())
            {
                if (i++ == index)
                {
                    obj = e.Current;
                    return;
                }
            }
            throw new IndexOutOfRangeException();
        }

        internal static object GetRuntimeValue(object o, Activity activity)
        {
            if (null == o)
                return o;

            object tmp = o;
            if (o is ActivityBind)
            {
                if (null == activity)
                    throw new ArgumentNullException("activity");

                tmp = ((ActivityBind)o).GetRuntimeValue(activity);
            }
            else if (o is WorkflowParameterBinding)
            {
                tmp = ((WorkflowParameterBinding)o).Value;
            }

            return tmp;
        }

        internal static void GetAllMembers(Activity activity, IList<TrackingDataItem> items, TrackingAnnotationCollection annotations)
        {
            Type t = activity.GetType();
            //
            // Get all fields
            FieldInfo[] fields = t.GetFields(BindingFlags.Public |
                                                BindingFlags.NonPublic |
                                                BindingFlags.Instance |
                                                BindingFlags.Static |
                                                BindingFlags.GetField);

            foreach (FieldInfo f in fields)
            {
                if (!PropertyHelper.IsInternalVariable(f.Name))
                {
                    TrackingDataItem data = new TrackingDataItem();
                    data.FieldName = f.Name;
                    data.Data = GetRuntimeValue(f.GetValue(activity), activity);
                    foreach (string s in annotations)
                        data.Annotations.Add(s);
                    items.Add(data);
                }
            }
            //
            // Get all properties (except indexers)
            PropertyInfo[] properties = t.GetProperties(BindingFlags.Public |
                                                            BindingFlags.NonPublic |
                                                            BindingFlags.Instance |
                                                            BindingFlags.Static |
                                                            BindingFlags.GetProperty);

            foreach (PropertyInfo p in properties)
            {
                if (!IsInternalVariable(p.Name))
                {
                    //
                    // Skip indexers, since private data members
                    // are exposed the data is still available.
                    if (p.GetIndexParameters().Length > 0)
                        continue;

                    TrackingDataItem data = new TrackingDataItem();
                    data.FieldName = p.Name;
                    data.Data = GetRuntimeValue(p.GetValue(activity, null), activity);
                    foreach (string s in annotations)
                        data.Annotations.Add(s);
                    items.Add(data);
                }
            }
        }

        #endregion

        #region Private Methods

        private static bool IsInternalVariable(string name)
        {
            string[] vars = { "__winoe_ActivityLocks_", "__winoe_StaticActivityLocks_", "__winoe_MethodLocks_" };

            foreach (string s in vars)
            {
                if (0 == string.Compare(s, name, StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        private static bool NameIsDefined(string name, object o, out string realName)
        {
            realName = null;
            Type t = o.GetType();
            //
            // Get the member with the requested name
            // Do case specific first
            MemberInfo[] members = t.GetMember(name,
                                                BindingFlags.Public |
                                                BindingFlags.NonPublic |
                                                BindingFlags.GetProperty |
                                                BindingFlags.GetField |
                                                BindingFlags.Instance |
                                                BindingFlags.Static);

            //
            // Not found
            if ((null == members) || (0 == members.Length))
            {
                //
                // Do case insensitive
                members = t.GetMember(name,
                                        BindingFlags.Public |
                                        BindingFlags.NonPublic |
                                        BindingFlags.GetProperty |
                                        BindingFlags.GetField |
                                        BindingFlags.Instance |
                                        BindingFlags.Static |
                                        BindingFlags.IgnoreCase);
                //
                // Not found
                if ((null == members) || (0 == members.Length))
                {
                    return false;
                }
            }

            if ((null == members) || (0 == members.Length) || (null == members[0].Name) || (0 == members[0].Name.Length))
                return false;

            realName = members[0].Name;
            return true;
        }

        private static bool TryParseIndex(string fullName, out string name, out int index)
        {
            name = null;
            index = -1;

            int endPos = -1, startPos = -1;
            for (int i = fullName.Length - 1; i > 0; i--)
            {
                if ((']' == fullName[i]) && (-1 == endPos))
                {
                    endPos = i;
                }
                else if (('[' == fullName[i]) && (-1 == startPos))
                {
                    startPos = i;
                    break;
                }
            }

            if ((-1 == endPos) || (-1 == startPos))
                return false;

            string idx = fullName.Substring(startPos + 1, endPos - 1 - startPos);
            name = fullName.Substring(0, startPos);

            return int.TryParse(idx, out index);
        }

        #endregion
    }
}
