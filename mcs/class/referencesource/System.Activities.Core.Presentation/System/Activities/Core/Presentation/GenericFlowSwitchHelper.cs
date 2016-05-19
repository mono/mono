//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;

    internal static class GenericFlowSwitchHelper
    {
        static readonly MethodInfo genericCopy = typeof(GenericFlowSwitchHelper).GetMethod("GenericCopy");
        static readonly MethodInfo genericCreateGenericFlowSwitchLink = typeof(GenericFlowSwitchHelper).GetMethod("CreateGenericFlowSwitchLink");
        static readonly MethodInfo genericGetCaseName = typeof(GenericFlowSwitchHelper).GetMethod("GenericGetCaseName");
        static readonly MethodInfo genericRemapFlowSwitch = typeof(GenericFlowSwitchHelper).GetMethod("GenericRemapFlowSwitch");
        const string flowSwitchCasesKeyIdentifier = "key";
        const string flowSwitchNullCaseKeyIdentifier = "(null)";
        const string flowSwitchEmptyCaseKeyIdentifier = "(empty)";

        public static string FlowSwitchCasesKeyIdentifier
        {
            get
            {
                return flowSwitchCasesKeyIdentifier;
            }
        }

        public static string FlowSwitchNullCaseKeyIdentifier
        {
            get
            {
                return flowSwitchNullCaseKeyIdentifier;
            }
        }

        public static string FlowSwitchEmptyCaseKeyIdentifier
        {
            get
            {
                return flowSwitchEmptyCaseKeyIdentifier;
            }
        }

        public static void Copy(Type genericType, FlowNode currentFlowElement, Dictionary<FlowNode, FlowNode> clonedFlowElements)
        {
            MethodInfo copy = genericCopy.MakeGenericMethod(new Type[] { genericType });
            copy.Invoke(null, new object[] { currentFlowElement, clonedFlowElements });
        }

        public static void GenericCopy<T>(FlowNode currentFlowElement, Dictionary<FlowNode, FlowNode> clonedFlowElements)
        {
            FlowSwitch<T> currentFlowSwitch = (FlowSwitch<T>)currentFlowElement;
            FlowSwitch<T> clonedFlowSwitch = (FlowSwitch<T>)clonedFlowElements[currentFlowElement];

            //Update the default case.
            FlowNode defaultCase = currentFlowSwitch.Default;
            if (defaultCase != null && clonedFlowElements.ContainsKey(defaultCase))
            {
                clonedFlowSwitch.Default = clonedFlowElements[defaultCase];
            }
            else
            {
                clonedFlowSwitch.Default = null;
            }

            //Update the Cases dictionary.
            foreach (T key in currentFlowSwitch.Cases.Keys)
            {
                if (clonedFlowElements.ContainsKey(currentFlowSwitch.Cases[key]))
                {
                    clonedFlowSwitch.Cases.Add(key, clonedFlowElements[currentFlowSwitch.Cases[key]]);
                }
            }
        }

        // This is different from GenericCopy because all the reference shuold be set
        // from property: swtich.Default = SomeValue should be 
        // switch.Properties["Default"] = SomeValue.
        public static void ReferenceCopy(Type genericType,
                FlowNode currentFlowElement,
                Dictionary<FlowNode, ModelItem> modelItems,
                Dictionary<FlowNode, FlowNode> clonedFlowElements)
        {
            ModelItem modelItem = null;
            if (modelItems.TryGetValue(currentFlowElement, out modelItem))
            {
                MethodInfo copy = genericRemapFlowSwitch.MakeGenericMethod(new Type[] { genericType });
                copy.Invoke(null, new object[] { currentFlowElement, modelItem, clonedFlowElements });
            }
            else
            {
                Fx.Assert("should not happen!");
            }
        }

        // oldNewFlowNodeMap: <OldFlowNode, NewFlowNode>
        //    sometimes, OldFlowNode == NewFlowNode, say, FlowNode is a FlowDecesion.
        //    if FlowNode is FlowStep, OldFlowNode != NewFlowNode
        public static void GenericRemapFlowSwitch<T>(FlowNode currentFlowElement,
            ModelItem modelItem, Dictionary<FlowNode, FlowNode> oldNewFlowNodeMap)
        {
            FlowSwitch<T> currentFlowSwitch = (FlowSwitch<T>)currentFlowElement;

            //Update the default case.
            FlowNode defaultCase = currentFlowSwitch.Default;
            if (defaultCase != null && oldNewFlowNodeMap.ContainsKey(defaultCase))
            {
                modelItem.Properties["Default"].SetValue(oldNewFlowNodeMap[defaultCase]);
            }
            else
            {
                modelItem.Properties["Default"].SetValue(null);
            }


            // collect all the cases that should be update
            Dictionary<object, object> keyValueMap = new Dictionary<object, object>();
            foreach (T key in currentFlowSwitch.Cases.Keys)
            {
                if (oldNewFlowNodeMap.ContainsKey(currentFlowSwitch.Cases[key]))
                {
                    keyValueMap.Add(key, oldNewFlowNodeMap[currentFlowSwitch.Cases[key]]);
                }
            }
            // Update the Cases dictionary.
            ModelProperty casesProperty = modelItem.Properties["Cases"];

            // remove all key
            foreach (ModelItem key in GenericFlowSwitchHelper.GetCaseKeys(casesProperty))
            {
                GenericFlowSwitchHelper.RemoveCase(casesProperty, key.GetCurrentValue());
            }

            // add back keys
            foreach (T key in keyValueMap.Keys)
            {
                GenericFlowSwitchHelper.AddCase(casesProperty, key, keyValueMap[key]);
            }
        }

        public static bool IsGenericFlowSwitch(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(FlowSwitch<>);
        }

        public static IFlowSwitchLink CreateFlowSwitchLink(Type flowSwitchType, ModelItem currentMI, object caseValue, bool isDefault)
        {
            Type genericType = null;
            object key = null;
            genericType = flowSwitchType.GetGenericArguments()[0];
            if (caseValue is string)
            {
                key = GetObject(caseValue as string, genericType);
            }
            else
            {
                key = caseValue;
            }
            MethodInfo method = genericCreateGenericFlowSwitchLink.MakeGenericMethod(genericType);
            return method.Invoke(null, new object[] { currentMI, key, isDefault }) as IFlowSwitchLink;
        }

        public static IFlowSwitchLink CreateGenericFlowSwitchLink<T>(ModelItem currentMI, T caseValue, bool isDefault)
        {
            if (isDefault)
            {
                return new FlowSwitchDefaultLink<T>(currentMI, caseValue, isDefault);
            }
            else
            {
                return new FlowSwitchCaseLink<T>(currentMI, caseValue, isDefault);
            }
        }

        public static string GetCaseName(ModelProperty casesProperties, Type type, out string errorMessage)
        {
            object casesDict = casesProperties.Dictionary.GetCurrentValue();
            ModelItemCollection collection = casesProperties.Value.Properties["ItemsCollection"].Collection;
            MethodInfo method = genericGetCaseName.MakeGenericMethod(type);
            object[] parameters = new object[] { collection, null };
            string result = (string)method.Invoke(null, parameters);
            errorMessage = (string)parameters[1];
            return result;
        }

        public static string GenericGetCaseName<T>(ModelItemCollection collection, out string errorMessage)
        {
            int maxName = 100000;
            Type type = typeof(T);
            errorMessage = string.Empty;
            if (typeof(string).IsAssignableFrom(type))
            {
                string caseName = "caseN";
                for (int i = 1; i <= maxName; i++)
                {
                    caseName = string.Format(CultureInfo.InvariantCulture, SR.CaseFormat, i);
                    if (!GenericFlowSwitchHelper.ContainsCaseKey(collection, caseName))
                    {
                        break;
                    }
                }
                return caseName;
            }
            else if (GenericFlowSwitchHelper.IsIntegralType(type))
            {
                if (type == typeof(sbyte))
                {
                    sbyte maxCount = (sbyte.MaxValue < maxName) ? sbyte.MaxValue : (sbyte)maxName;
                    for (sbyte i = 0; i <= maxCount; i++)
                    {
                        if (!GenericFlowSwitchHelper.ContainsCaseKey(collection, i))
                        {
                            return GenericFlowSwitchHelper.GetString(i, type);
                        }
                    }
                }
                else if (type == typeof(byte))
                {
                    byte maxCount = (byte.MaxValue < maxName) ? byte.MaxValue : (byte)maxName;
                    for (byte i = 0; i <= maxCount; i++)
                    {
                        if (!GenericFlowSwitchHelper.ContainsCaseKey(collection, i))
                        {
                            return GenericFlowSwitchHelper.GetString(i, type);
                        }
                    }
                }
                else if (type == typeof(char))
                {
                    char maxCount = unchecked((char)maxName);
                    for (char i = (char)48; i <= maxCount; i++)
                    {
                        if (!GenericFlowSwitchHelper.ContainsCaseKey(collection, i))
                        {
                            return GenericFlowSwitchHelper.GetString(i, type);
                        }
                    }
                }
                else if (type == typeof(short))
                {
                    short maxCount = (short)maxName;
                    for (short i = 0; i <= maxCount; i++)
                    {
                        if (!GenericFlowSwitchHelper.ContainsCaseKey(collection, i))
                        {
                            return GenericFlowSwitchHelper.GetString(i, type);
                        }
                    }
                }
                else if (type == typeof(ushort))
                {
                    ushort maxCount = (ushort)maxName;
                    for (ushort i = 0; i <= maxCount; i++)
                    {
                        if (!GenericFlowSwitchHelper.ContainsCaseKey(collection, i))
                        {
                            return GenericFlowSwitchHelper.GetString(i, type);
                        }
                    }
                }
                else if (type == typeof(int))
                {
                    for (int i = 0; i <= maxName; i++)
                    {
                        if (!GenericFlowSwitchHelper.ContainsCaseKey(collection, i))
                        {
                            return GenericFlowSwitchHelper.GetString(i, type);
                        }
                    }
                }
                else if (type == typeof(uint))
                {
                    for (uint i = 0; i <= (uint)maxName; i++)
                    {
                        if (!GenericFlowSwitchHelper.ContainsCaseKey(collection, i))
                        {
                            return GenericFlowSwitchHelper.GetString(i, type);
                        }
                    }
                }
                else if (type == typeof(long))
                {
                    for (long i = 0; i <= (long)maxName; i++)
                    {
                        if (!GenericFlowSwitchHelper.ContainsCaseKey(collection, i))
                        {
                            return GenericFlowSwitchHelper.GetString(i, type);
                        }
                    }
                }
                else if (type == typeof(ulong))
                {
                    for (ulong i = 0; i <= (ulong)maxName; i++)
                    {
                        if (!GenericFlowSwitchHelper.ContainsCaseKey(collection, i))
                        {
                            return GenericFlowSwitchHelper.GetString(i, type);
                        }
                    }
                }
                errorMessage = SR.InvalidFlowSwitchCaseMessage;
                return string.Empty;
            }
            else if (type.IsEnum)
            {
                Array array = type.GetEnumValues();
                foreach (object value in array)
                {
                    if (!GenericFlowSwitchHelper.ContainsCaseKey(collection, value))
                    {
                        return GenericFlowSwitchHelper.GetString(value, type);
                    }
                }
                errorMessage = SR.InvalidFlowSwitchCaseMessage;
                return string.Empty;
            }
            else if (type == typeof(bool))
            {
                if (!GenericFlowSwitchHelper.ContainsCaseKey(collection, true))
                {
                    return GenericFlowSwitchHelper.GetString(true, type);
                }
                else if (!GenericFlowSwitchHelper.ContainsCaseKey(collection, false))
                {
                    return GenericFlowSwitchHelper.GetString(false, type);
                }
                errorMessage = SR.InvalidFlowSwitchCaseMessage;
                return string.Empty;
            }
            return string.Empty;
        }

        public static bool IsIntegralType(Type type)
        {
            if (type == typeof(sbyte) || type == typeof(byte) || type == typeof(char) || type == typeof(short) ||
                type == typeof(ushort) || type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GetString(object key, Type type)
        {
            string result = null;
            if (key == null)
            {
                Fx.Assert(type == null || !type.IsValueType, "Value type should not have null value");
                result = FlowSwitchNullCaseKeyIdentifier;
            }
            else
            {
                result = GetRawString(key);
                if (result == string.Empty && typeof(string).IsAssignableFrom(type))
                {
                    result = FlowSwitchEmptyCaseKeyIdentifier;
                }
            }
            return result;
        }

        //Raw string means the null is not represented as "<null>" and string.Empty is not represented as "<empty>".
        static string GetRawString(object caseObject)
        {
            string result = null;

            if (caseObject == null)
            {
                return null;
            }
            if (!(caseObject is string))
            {
                result = XamlUtilities.GetConverter(caseObject.GetType()).ConvertToString(caseObject);
            }
            else
            {
                result = (string)caseObject;
            }
            return result;
        }

        public static object GetObject(string caseString, Type genericType)
        {
            object result;
            if (!genericType.IsValueType && caseString == FlowSwitchNullCaseKeyIdentifier)
            {
                result = null;
            }
            else if (typeof(string).IsAssignableFrom(genericType))
            {
                if (caseString == FlowSwitchEmptyCaseKeyIdentifier)
                {
                    result = string.Empty;
                }
                else
                {
                    result = caseString;
                }
            }
            else
            {
                //If target type is value type and the caseString is null, we should leave converter to process it.
                //If target type is reference type, the caseString is a non-null value here.
                result = XamlUtilities.GetConverter(genericType).ConvertFromString(caseString);
            }
            return result;
        }

        public static bool ContainsCaseKey(ModelProperty casesProp, object key)
        {
            ModelItemCollection itemsCollection = casesProp.Value.Properties["ItemsCollection"].Collection;
            return ContainsCaseKey(itemsCollection, key);
        }

        static bool ContainsCaseKey(ModelItemCollection itemsCollection, object key)
        {
            if (GenericFlowSwitchHelper.FlowSwitchNullCaseKeyIdentifier.Equals(key))
            {
                key = null;
            }

            foreach (ModelItem item in itemsCollection)
            {
                object value = item.Properties["Key"].ComputedValue;
                if (value == key || ((value != null) && item.Properties["Key"].ComputedValue.Equals(key)))
                {
                    return true;
                }
            }
            return false;
        }

        public static ModelItem GetCaseModelItem(ModelProperty casesProp, object key)
        {
            ModelItemCollection itemsCollection = casesProp.Value.Properties["ItemsCollection"].Collection;
            return GenericFlowSwitchHelper.GetCaseModelItem(itemsCollection, key);
        }

        static ModelItem GetCaseModelItem(ModelItemCollection itemsCollection, object key)
        {
            if (GenericFlowSwitchHelper.FlowSwitchNullCaseKeyIdentifier.Equals(key))
            {
                key = null;
            }

            foreach (ModelItem item in itemsCollection)
            {
                object value = item.Properties["Key"].ComputedValue;
                if (value == key || (value != null && item.Properties["Key"].ComputedValue.Equals(key)))
                {
                    return item.Properties["Value"].Value;
                }
            }
            string caseName = GetString(key, itemsCollection.ItemType);
            throw FxTrace.Exception.AsError(new KeyNotFoundException(caseName));
        }

        public static object GetCase(ModelItemCollection itemsCollection, object key)
        {
            return GenericFlowSwitchHelper.GetCaseModelItem(itemsCollection, key).GetCurrentValue();
        }

        public static ModelItem[] GetCaseKeys(ModelProperty casesProp)
        {
            ModelItemCollection itemsCollection = casesProp.Value.Properties["ItemsCollection"].Collection;
            ModelItem[] keys = new ModelItem[itemsCollection.Count];
            for (int i = 0; i < itemsCollection.Count; i++)
            {
                keys[i] = (ModelItem) itemsCollection[i].Properties["Key"].Value;
            }
            return keys;
        }

        public static void RemoveCase(ModelProperty casesProp, object key)
        {
            ModelItemCollection itemsCollection = casesProp.Value.Properties["ItemsCollection"].Collection;
            
            if (GenericFlowSwitchHelper.FlowSwitchNullCaseKeyIdentifier.Equals(key))
            {
                key = null;
            }
            foreach (ModelItem item in itemsCollection)
            {
                object value = item.Properties["Key"].ComputedValue;
                if (value == key || (value != null && item.Properties["Key"].ComputedValue.Equals(key)))
                {
                    itemsCollection.Remove(item);
                    return;
                }
                
            }
            string caseName = GetString(key, itemsCollection.ItemType.GetGenericArguments()[0]);
            throw FxTrace.Exception.AsError(new KeyNotFoundException(caseName));
        }
        
        public static void AddCase(ModelProperty casesPropperties, object newKey, object newCase)
        {
            Type propertyType = casesPropperties.PropertyType;
            Fx.Assert(propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(IDictionary<,>), "Property type should be IDictonary<T, FlowNode>");
            Type keyType = propertyType.GetGenericArguments()[0];
            ModelItemCollection itemsCollection = casesPropperties.Value.Properties["ItemsCollection"].Collection;
             
            Type caseType = typeof(ModelItemKeyValuePair<,>).MakeGenericType(new Type[] { keyType, typeof(FlowNode) });
            object mutableKVPair = Activator.CreateInstance(caseType, new object[] { newKey, newCase });
            itemsCollection.Add(mutableKVPair);
        }

        public static bool CanBeGeneratedUniquely(Type typeArgument)
        {
            return typeArgument.IsEnum || typeof(string).IsAssignableFrom(typeArgument)
                || GenericFlowSwitchHelper.IsIntegralType(typeArgument) || typeof(bool) == typeArgument;
        }

        public static bool CheckEquality(object value, Type targetType)
        {
            if (value == null)
            {
                return true;
            }
            else
            {
                string stringValue = GetString(value, targetType);
                object newValue = GetObject(stringValue, targetType);
                return value.GetHashCode() == newValue.GetHashCode() && value.Equals(newValue);
            }
        }

        public static bool ValidateCaseKey(object obj, ModelProperty casesProp, Type genericType, out string reason)
        {
            reason = string.Empty;
            string key = GenericFlowSwitchHelper.GetString(obj, genericType);
            if (GenericFlowSwitchHelper.CheckEquality(obj, genericType))
            {
                if (GenericFlowSwitchHelper.ContainsCaseKey(casesProp, obj))
                {
                    reason = string.Format(CultureInfo.CurrentCulture, SR.DuplicateCaseKey, key);
                    return false;
                }
                return true;
            }
            else
            {
                reason = string.Format(CultureInfo.CurrentUICulture, SR.EqualityError, genericType.Name);
                return false;
            }
        }
    }
}
