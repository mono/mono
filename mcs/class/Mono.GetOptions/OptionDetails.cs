//
// OptionDetails.cs
//
// Author: Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) 2002 Rafael Teixeira
//

using System;
using System.Collections;
using System.IO;
using System.Reflection;

namespace Mono.GetOptions
{
	public enum WhatToDoNext
	{
		AbandonProgram,
		GoAhead
	}

	internal class OptionDetails : IComparable 
	{
		public char ShortForm;
		public string LongForm;
		public string ShortDescription;
		public bool NeedsParameter;
		public int MaxOccurs; // negative means there is no limit
		public int Occurs;
		public Options OptionBundle;
		public MemberInfo MemberInfo;
		public ArrayList Values;
		public System.Type ParameterType;
		public string ParamName = "PARAM"; // TODO: another element to get from OptionAttribute
		
		public override string ToString()
		{
			string optionHelp;
			// TODO: Yet not that good
			optionHelp = "  ";
			optionHelp += (this.ShortForm != ' ') ? "-"+this.ShortForm+" " : "   ";
			optionHelp += (this.LongForm != string.Empty && this.LongForm != null) ? "--"+this.LongForm : "";
			if (NeedsParameter)
			{
				if (this.LongForm != string.Empty && this.LongForm != null)
					optionHelp += "="; 
				optionHelp += ParamName; 
			}
			optionHelp = optionHelp.PadRight(26) + " ";
			optionHelp += this.ShortDescription;
			return optionHelp; 
		}

		private static System.Type TypeOfMember(MemberInfo memberInfo)
		{
			if ((memberInfo.MemberType == MemberTypes.Field && memberInfo is FieldInfo))
				return ((FieldInfo)memberInfo).FieldType;

			if ((memberInfo.MemberType == MemberTypes.Property && memberInfo is PropertyInfo))
				return ((PropertyInfo)memberInfo).PropertyType;

			if ((memberInfo.MemberType == MemberTypes.Method && memberInfo is MethodInfo))
			{
				if (((MethodInfo)memberInfo).ReturnType.FullName != typeof(WhatToDoNext).FullName)
					throw new NotSupportedException("Option method must return '" + typeof(WhatToDoNext).FullName + "'");

				ParameterInfo[] parameters = ((MethodInfo)memberInfo).GetParameters();
				if ((parameters == null) || (parameters.Length == 0))
					return null;
				else
					return parameters[0].ParameterType;
			}

			throw new NotSupportedException("'" + memberInfo.MemberType + "' memberType is not supportted");
		}

		public OptionDetails(MemberInfo memberInfo, OptionAttribute option, Options optionBundle)
		{
			this.ShortForm = option.ShortForm;
			this.LongForm = (option.LongForm == string.Empty)? memberInfo.Name:option.LongForm;
			this.ShortDescription = option.ShortDescription;
			this.Occurs = 0;
			this.OptionBundle = optionBundle; 
			this.MemberInfo = memberInfo;
			this.NeedsParameter = false;
			this.Values = null;
			this.MaxOccurs = 1;
			this.ParameterType = TypeOfMember(memberInfo);

			if (this.ParameterType != null && this.ParameterType.FullName != "System.Boolean")
			{
				this.NeedsParameter = true;
				if (this.ParameterType.IsArray)
				{
					this.Values = new ArrayList();
					this.MaxOccurs = option.MaxOccurs;
				}
				else
				{
					if (this.MemberInfo is MethodInfo)
						this.MaxOccurs = option.MaxOccurs;
				}
			}
		}

		internal string Key
		{
			get { return ((this.ShortForm != ' ') ? this.ShortForm + "" : "") + this.LongForm; }
		}

		int IComparable.CompareTo(object other)
		{
			return Key.CompareTo(((OptionDetails)other).Key);
		}

		public void TransferValues()
		{
			if (Values != null)
			{
				if (MemberInfo is FieldInfo)
				{
					((FieldInfo)MemberInfo).SetValue(OptionBundle, Values.ToArray(ParameterType.GetElementType()));
					return;
				}

				if (MemberInfo is PropertyInfo) 
				{
					((PropertyInfo)MemberInfo).SetValue(OptionBundle, Values.ToArray(ParameterType.GetElementType()), null);
					return;
				}

				if ((WhatToDoNext)((MethodInfo)MemberInfo).Invoke(OptionBundle, new object[] { Values.ToArray(ParameterType.GetElementType()) }) == WhatToDoNext.AbandonProgram)
					System.Environment.Exit(1);
			}
		}

		private void DoIt(string parameter)
		{
			Occurs++;

			if (MaxOccurs > 0 && Occurs > MaxOccurs)
				throw new IndexOutOfRangeException("Option " + ShortForm + " can be used at most " + MaxOccurs + " times");
			
			if (!NeedsParameter)
			{
				if (MemberInfo is FieldInfo)
				{
					((FieldInfo)MemberInfo).SetValue(OptionBundle, true);
					return;
				}
				if (MemberInfo is PropertyInfo) 
				{
					((PropertyInfo)MemberInfo).SetValue(OptionBundle, true, null);
					return;
				}
				if ((WhatToDoNext)((MethodInfo)MemberInfo).Invoke(OptionBundle, null) == WhatToDoNext.AbandonProgram)
					System.Environment.Exit(1);

				return;
			}

			object convertedParameter = null;

			if (Values != null && parameter != null)
			{
				convertedParameter = Convert.ChangeType(parameter, ParameterType.GetElementType());
				Values.Add(convertedParameter);
				return;
			}

			if (parameter != null)
				convertedParameter = Convert.ChangeType(parameter, ParameterType);

			if (MemberInfo is FieldInfo)
			{
				((FieldInfo)MemberInfo).SetValue(OptionBundle, convertedParameter);
				return;
			}

			if (MemberInfo is PropertyInfo) 
			{
				((PropertyInfo)MemberInfo).SetValue(OptionBundle, convertedParameter, null);
				return;
			}

			if ((WhatToDoNext)((MethodInfo)MemberInfo).Invoke(OptionBundle, new object[] { convertedParameter }) == WhatToDoNext.AbandonProgram)
				System.Environment.Exit(1);
		}

		public bool ProcessArgument(string arg, string nextArg)
		{
			if (arg == ("-" + ShortForm) || arg == ("--" + LongForm) ||
				arg == ("/" + ShortForm) || arg == ("/" + LongForm)) 
			{
				DoIt(nextArg);
				return true;
			}
			string temp = (arg + ":" + nextArg).TrimStart('-','/');
			if (temp == LongForm)
			{
				DoIt(null);
				return true;
			}
			return false;
		}
	}
}
