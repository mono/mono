//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Internal.PropertyEditing.Editors
{
    using System.Windows.Data;
    using System.Globalization;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Collections;
    sealed class FlagStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return value.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Type flagType = parameter as Type;
            Fx.Assert(flagType != null && flagType.IsEnum, "TargetType should be enum");
            Fx.Assert(Attribute.IsDefined(flagType, typeof(FlagsAttribute)), "FlagType should have flags attribute");

            if (value == null)
            {
                return Enum.ToObject(flagType, 0);
            }

            string str = (value as string).ToUpperInvariant();
            str = str.Trim();
            if (str.Equals(string.Empty) || str.Equals("0"))
            {
                return Enum.ToObject(flagType, 0);
            }

            Dictionary<string, object> flagDictionary = GenerateFlagDictionary(flagType);
            int flagsIntValue = 0;
            string[] names = str.Split(',');
            foreach (string name in names)
            {
                string flagName = name.Trim();
                if (flagDictionary.ContainsKey(flagName))
                {
                    flagsIntValue |= (int)flagDictionary[flagName];
                    flagDictionary.Remove(flagName);
                }                
                else
                {
                    throw FxTrace.Exception.AsError(new ArgumentException(string.Format(CultureInfo.CurrentUICulture, SR.InvalidFlagName, value, flagType.Name)));
                }
            }
            return Enum.ToObject(flagType, flagsIntValue);
        }

        static Dictionary<string, object> GenerateFlagDictionary(Type flagType)
        {
            Dictionary<string, object> flagDictionary = new Dictionary<string, object>();
            string[] flagNames = flagType.GetEnumNames();
            Array flagValues = flagType.GetEnumValues();
            for (int i = 0; i < flagNames.Length; i++)
            {
                flagDictionary.Add(flagNames[i].ToUpperInvariant(), flagValues.GetValue(i));
            }
            return flagDictionary;
        }
    }
}
