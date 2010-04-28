//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.


namespace System.Data.Services.Parsing
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Xml;
    using System.Data.Services.Client;

    internal static class WebConvert
    {
        private const string HexValues = "0123456789ABCDEF";

        private const string XmlHexEncodePrefix = "0x";


        internal static string ConvertByteArrayToKeyString(byte[] byteArray)
        {
            StringBuilder hexBuilder = new StringBuilder(3 + byteArray.Length * 2);
            hexBuilder.Append(XmlConstants.XmlBinaryPrefix);
            hexBuilder.Append("'");
            for (int i = 0; i < byteArray.Length; i++)
            {
                hexBuilder.Append(HexValues[byteArray[i] >> 4]);
                hexBuilder.Append(HexValues[byteArray[i] & 0x0F]);
            }

            hexBuilder.Append("'");
            return hexBuilder.ToString();
        }


        internal static bool IsKeyTypeQuoted(Type type)
        {
            Debug.Assert(type != null, "type != null");
            return type == typeof(System.Xml.Linq.XElement) || type == typeof(string);
        }

        internal static bool TryKeyPrimitiveToString(object value, out string result)
        {
            Debug.Assert(value != null, "value != null");
            if (value.GetType() == typeof(byte[]))
            {
                result = ConvertByteArrayToKeyString((byte[])value);
            }
            else
            {
                if (!TryXmlPrimitiveToString(value, out result))
                {
                    return false;
                }

                Debug.Assert(result != null, "result != null");
                if (value.GetType() == typeof(DateTime))
                {
                    result = XmlConstants.LiteralPrefixDateTime + "'" + result + "'";
                }
                else if (value.GetType() == typeof(Decimal))
                {
                    result = result + XmlConstants.XmlDecimalLiteralSuffix;
                }
                else if (value.GetType() == typeof(Guid))
                {
                    result = XmlConstants.LiteralPrefixGuid + "'" + result + "'";
                }
                else if (value.GetType() == typeof(Int64))
                {
                    result = result + XmlConstants.XmlInt64LiteralSuffix;
                }
                else if (value.GetType() == typeof(Single))
                {
                    result = result + XmlConstants.XmlSingleLiteralSuffix;
                }
                else if (value.GetType() == typeof(double))
                {
                    result = AppendDecimalMarkerToDouble(result);
                }
                else if (IsKeyTypeQuoted(value.GetType()))
                {
                    result = "'" + result.Replace("'", "''") + "'";
                }
            }

            return true;
        }

        internal static bool TryXmlPrimitiveToString(object value, out string result)
        {
            Debug.Assert(value != null, "value != null");
            result = null;

            Type valueType = value.GetType();
            valueType = Nullable.GetUnderlyingType(valueType) ?? valueType;

            if (typeof(String) == valueType)
            {
                result = (string)value;
            }
            else if (typeof(Boolean) == valueType)
            {
                result = XmlConvert.ToString((bool)value);
            }
            else if (typeof(Byte) == valueType)
            {
                result = XmlConvert.ToString((byte)value);
            }
            else if (typeof(DateTime) == valueType)
            {
                result = XmlConvert.ToString((DateTime)value, XmlDateTimeSerializationMode.RoundtripKind);
            }
            else if (typeof(Decimal) == valueType)
            {
                result = XmlConvert.ToString((decimal)value);
            }
            else if (typeof(Double) == valueType)
            {
                result = XmlConvert.ToString((double)value);
            }
            else if (typeof(Guid) == valueType)
            {
                result = value.ToString();
            }
            else if (typeof(Int16) == valueType)
            {
                result = XmlConvert.ToString((Int16)value);
            }
            else if (typeof(Int32) == valueType)
            {
                result = XmlConvert.ToString((Int32)value);
            }
            else if (typeof(Int64) == valueType)
            {
                result = XmlConvert.ToString((Int64)value);
            }
            else if (typeof(SByte) == valueType)
            {
                result = XmlConvert.ToString((SByte)value);
            }
            else if (typeof(Single) == valueType)
            {
                result = XmlConvert.ToString((Single)value);
            }
            else if (typeof(byte[]) == valueType)
            {
                byte[] byteArray = (byte[])value;
                result = Convert.ToBase64String(byteArray);
            }
            #if !ASTORIA_LIGHT
            else if (ClientConvert.IsBinaryValue(value))
            {
                return ClientConvert.TryKeyBinaryToString(value, out result);
            }
            #endif
            else if (typeof(System.Xml.Linq.XElement) == valueType)
            {
                result = ((System.Xml.Linq.XElement)value).ToString(System.Xml.Linq.SaveOptions.None);
            }
            else
            {
                result = null;
                return false;
            }

            Debug.Assert(result != null, "result != null");
            return true;
        }

        private static string AppendDecimalMarkerToDouble(string input)
        {
            foreach (char c in input)
            {
                if (!Char.IsDigit(c))
                {
                    return input;
                }
            }

            return input + ".0";
        }
    }
}
