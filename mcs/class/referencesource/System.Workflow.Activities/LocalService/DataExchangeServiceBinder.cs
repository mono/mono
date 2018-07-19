
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;
using System.Security.Permissions;
using System.Globalization;


namespace System.Workflow.Activities
{
    internal sealed class ExternalDataExchangeBinder : Binder
    {
        Binder defltBinder;

        internal ExternalDataExchangeBinder()
        {
            defltBinder = Type.DefaultBinder;
        }

        public override MethodBase BindToMethod(BindingFlags bindingAttr,
                                                MethodBase[] match,
                                                ref object[] args,
                                                ParameterModifier[] modifiers,
                                                System.Globalization.CultureInfo culture,
                                                string[] names,
                                                out object state)
        {
            Object[] argsCopy = new Object[args.Length];
            args.CopyTo(argsCopy, 0);
            state = null;

            try
            {
                return defltBinder.BindToMethod(bindingAttr, match, ref args, modifiers, culture, names, out state);
            }
            catch (MissingMethodException) //5% case where when passed null for params.
            {

                if (match != null && match.Length != 0)
                {
                    for (int i = 0; i < match.Length; ++i)
                    {
                        ParameterInfo[] methodParams = match[i].GetParameters();

                        if (methodParams.Length == argsCopy.Length)
                        {
                            for (int j = 0; j < methodParams.Length; ++j)
                            {
                                if (!methodParams[j].ParameterType.IsInstanceOfType(argsCopy[j]))
                                {
                                    if (!(methodParams[j].ParameterType.IsArray && argsCopy[j] == null))
                                        break;
                                }

                                if (j + 1 == methodParams.Length)
                                    return match[i];
                            }
                        }
                    }
                }
            }

            return null;
        }

        public override FieldInfo BindToField(BindingFlags bindingAttr,
                                               FieldInfo[] match,
                                               object value,
                                               CultureInfo culture)
        {
            return defltBinder.BindToField(bindingAttr, match, value, culture);
        }

        public override MethodBase SelectMethod(BindingFlags bindingAttr,
                                                MethodBase[] match,
                                                Type[] types,
                                                ParameterModifier[] modifiers)
        {
            return defltBinder.SelectMethod(bindingAttr, match, types, modifiers);
        }

        public override PropertyInfo SelectProperty(BindingFlags bindingAttr,
                                                    PropertyInfo[] match,
                                                    Type returnType,
                                                    Type[] indexes,
                                                    ParameterModifier[] modifiers
        )
        {
            return defltBinder.SelectProperty(bindingAttr, match, returnType, indexes, modifiers);
        }

        public override object ChangeType(object value,
                                          Type type,
                                          CultureInfo culture
        )
        {
            return defltBinder.ChangeType(value, type, culture);
        }

        public override void ReorderArgumentArray(ref object[] args,
                                                  object state
        )
        {
            defltBinder.ReorderArgumentArray(ref args, state);
        }
    }
}
