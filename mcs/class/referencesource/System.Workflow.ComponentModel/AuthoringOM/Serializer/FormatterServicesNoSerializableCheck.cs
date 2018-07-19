// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: SerializationFieldInfo
**
**
** Purpose: Provides a methods of representing imaginary fields
** which are unique to serialization.  In this case, what we're
** representing is the private members of parent classes.  We
** aggregate the FieldInfo associated with this member 
** and return a managled form of the name.  The name that we
** return is .parentname.fieldname
**
**
============================================================*/
using System;
using System.Reflection;
using System.Threading;
using System.Globalization;
using System.Security.Permissions;
using System.Collections;
using System.Collections.Generic;
using System.Workflow.ComponentModel;


namespace System.Runtime.Serialization
{
    internal static class FormatterServicesNoSerializableCheck
    {
        private struct MemberInfoName
        {
            public MemberInfo[] MemberInfo;
            public string[] Names;
        }
        private static Dictionary<Type, MemberInfoName> m_MemberInfoTable = new Dictionary<Type, MemberInfoName>(32);
        internal static readonly String FakeNameSeparatorString = "+";

        private static Object s_FormatterServicesSyncObject = null;

        private static Object formatterServicesSyncObject
        {
            get
            {
                if (s_FormatterServicesSyncObject == null)
                {
                    Object o = new Object();
                    Interlocked.CompareExchange(ref s_FormatterServicesSyncObject, o, null);
                }
                return s_FormatterServicesSyncObject;
            }
        }

        private static MemberInfo[] GetSerializableMembers2(Type type)
        {
            // get the list of all fields
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            int countProper = 0;
            for (int i = 0; i < fields.Length; i++)
            {
                if ((fields[i].Attributes & FieldAttributes.NotSerialized) == FieldAttributes.NotSerialized)
                    continue;
                countProper++;
            }
            if (countProper != fields.Length)
            {
                FieldInfo[] properFields = new FieldInfo[countProper];
                countProper = 0;
                for (int i = 0; i < fields.Length; i++)
                {
                    if ((fields[i].Attributes & FieldAttributes.NotSerialized) == FieldAttributes.NotSerialized)
                        continue;
                    properFields[countProper] = fields[i];
                    countProper++;
                }
                return properFields;
            }
            else
                return fields;
        }
        private static bool CheckSerializable(Type type)
        {
            return true;
            /*
            if (type.IsSerializable)
            {
                return true;
            }
            return false;
            */
        }
        private static MemberInfo[] InternalGetSerializableMembers(Type type, out string[] typeNames)
        {
            typeNames = null;

            ArrayList allMembers = null;
            ArrayList allNames = null;
            MemberInfo[] typeMembers;
            FieldInfo[] typeFields;
            Type parentType;

            //<

            if (type.IsInterface)
            {
                return new MemberInfo[0];
            }

            //Get all of the serializable members in the class to be serialized.
            typeMembers = GetSerializableMembers2(type);
            if (typeMembers != null)
            {
                typeNames = new string[typeMembers.Length];
                for (int index = 0; index < typeMembers.Length; index++)
                    typeNames[index] = typeMembers[index].Name;
            }

            //If this class doesn't extend directly from object, walk its hierarchy and 
            //get all of the private and assembly-access fields (e.g. all fields that aren't
            //virtual) and include them in the list of things to be serialized.  
            parentType = (Type)(type.BaseType);
            if (parentType != null && parentType != typeof(Object))
            {
                Type[] parentTypes = null;
                int parentTypeCount = 0;
                bool classNamesUnique = GetParentTypes(parentType, out parentTypes, out parentTypeCount);
                if (parentTypeCount > 0)
                {
                    allMembers = new ArrayList();
                    allNames = new ArrayList();
                    for (int i = 0; i < parentTypeCount; i++)
                    {
                        parentType = (Type)parentTypes[i];
                        if (!CheckSerializable(parentType))
                        {
                            throw new SerializationException(); //String.Format(Environment.GetResourceString("Serialization_NonSerType"), parentType.FullName, parentType.Module.Assembly.FullName));
                        }

                        typeFields = parentType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                        String typeName = classNamesUnique ? parentType.Name : parentType.FullName;
                        foreach (FieldInfo field in typeFields)
                        {
                            // Family and Assembly fields will be gathered by the type itself.
                            if (field.IsPrivate && !field.IsNotSerialized)
                            {
                                allMembers.Add(field);
                                allNames.Add(String.Concat(typeName, FakeNameSeparatorString, field.Name));
                                //allMembers.Add(new SerializationFieldInfo(field, typeName));
                            }
                        }
                    }
                    //If we actually found any new MemberInfo's, we need to create a new MemberInfo array and
                    //copy all of the members which we've found so far into that.
                    if (allMembers != null && allMembers.Count > 0)
                    {
                        MemberInfo[] membersTemp = new MemberInfo[allMembers.Count + typeMembers.Length];
                        Array.Copy(typeMembers, membersTemp, typeMembers.Length);
                        allMembers.CopyTo(membersTemp, typeMembers.Length);
                        typeMembers = membersTemp;

                        string[] namesTemp = new string[allNames.Count + typeNames.Length];
                        Array.Copy(typeNames, namesTemp, typeNames.Length);
                        allNames.CopyTo(namesTemp, typeNames.Length);
                        typeNames = namesTemp;
                    }
                }
            }
            return typeMembers;
        }

        private static bool GetParentTypes(Type parentType, out Type[] parentTypes, out int parentTypeCount)
        {
            //Check if there are any dup class names. Then we need to include as part of
            //typeName to prefix the Field names in SerializationFieldInfo
            /*out*/
            parentTypes = null;
            /*out*/
            parentTypeCount = 0;
            bool unique = true;
            for (Type t1 = parentType; t1 != typeof(object); t1 = t1.BaseType)
            {
                if (t1.IsInterface) continue;
                string t1Name = t1.Name;
                for (int i = 0; unique && i < parentTypeCount; i++)
                {
                    string t2Name = parentTypes[i].Name;
                    if (t2Name.Length == t1Name.Length && t2Name[0] == t1Name[0] && t1Name == t2Name)
                    {
                        unique = false;
                        break;
                    }
                }
                //expand array if needed
                if (parentTypes == null || parentTypeCount == parentTypes.Length)
                {
                    Type[] tempParentTypes = new Type[Math.Max(parentTypeCount * 2, 12)];
                    if (parentTypes != null)
                        Array.Copy(parentTypes, 0, tempParentTypes, 0, parentTypeCount);
                    parentTypes = tempParentTypes;
                }
                parentTypes[parentTypeCount++] = t1;
            }
            return unique;
        }

        // Get all of the Serializable members for a particular class.  For all practical intents and
        // purposes, this is the non-transient, non-static members (fields and properties).  In order to
        // be included, properties must have both a getter and a setter.  N.B.: A class
        // which implements ISerializable or has a serialization surrogate may not use all of these members
        // (or may have additional members).
        public static MemberInfo[] GetSerializableMembers(Type type, out string[] names)
        {
            names = null;

            MemberInfoName members;
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            lock (formatterServicesSyncObject)
            {
                //If we've already gathered the members for this type, just return them
                if (m_MemberInfoTable.TryGetValue(type, out members))
                {
                    names = members.Names;
                    return members.MemberInfo;
                }
            }
            members.MemberInfo = InternalGetSerializableMembers(type, out members.Names);
            lock (formatterServicesSyncObject)
            {
                //If we've already gathered the members for this type, just return them.
                MemberInfoName insertedMembers;
                if (m_MemberInfoTable.TryGetValue(type, out insertedMembers))
                {
                    names = insertedMembers.Names;
                    return insertedMembers.MemberInfo;
                }
                m_MemberInfoTable[type] = members;
            }
            names = members.Names;
            return members.MemberInfo;
        }
    }
}
