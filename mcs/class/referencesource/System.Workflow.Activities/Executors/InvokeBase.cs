using System;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace System.Workflow.Activities
{
    internal static class InvokeHelper
    {
        internal static void InitializeParameters(MethodInfo methodBase, WorkflowParameterBindingCollection parameterBindings)
        {
            ParameterInfo[] parameters = methodBase.GetParameters();
            foreach (ParameterInfo parameter in parameters)
            {
                if (!parameterBindings.Contains(parameter.Name))
                    parameterBindings.Add(new WorkflowParameterBinding(parameter.Name));
            }

            if (methodBase.ReturnType != typeof(void))
            {
                if (!parameterBindings.Contains("(ReturnValue)"))
                    parameterBindings.Add(new WorkflowParameterBinding("(ReturnValue)"));
            }
        }

        internal static object[] GetParameters(MethodBase methodBase, WorkflowParameterBindingCollection parameterBindings)
        {
            ParameterInfo[] formalParameters = methodBase.GetParameters();
            object[] actualParameters = new object[formalParameters.Length];
            int index = 0;

            foreach (ParameterInfo formalParameter in formalParameters)
            {
                if (parameterBindings.Contains(formalParameter.Name))
                {
                    WorkflowParameterBinding binding = parameterBindings[formalParameter.Name];
                    actualParameters[index] = binding.Value;
                }
                index++;
            }
            return actualParameters;
        }

        internal static object[] GetParameters(MethodBase methodBase, WorkflowParameterBindingCollection parameterBindings, out ParameterModifier[] parameterModifiers)
        {
            ParameterInfo[] formalParameters = methodBase.GetParameters();
            object[] actualParameters = new object[formalParameters.Length];
            if (actualParameters.Length == 0)
            {
                parameterModifiers = new ParameterModifier[0];
                return actualParameters;
            }

            int index = 0;
            BinaryFormatter formatter = null;
            ParameterModifier parameterModifier = new ParameterModifier(actualParameters.Length);
            foreach (ParameterInfo formalParameter in formalParameters)
            {
                if (formalParameter.ParameterType.IsByRef)
                {
                    parameterModifier[index] = true;
                }
                else
                {
                    parameterModifier[index] = false;
                }
                if (parameterBindings.Contains(formalParameter.Name))
                {
                    WorkflowParameterBinding binding = parameterBindings[formalParameter.Name];

                    if (formatter == null)
                        formatter = new BinaryFormatter();
                    actualParameters[index] = CloneOutboundValue(binding.Value, formatter, formalParameter.Name);
                }
                index++;
            }

            parameterModifiers = new ParameterModifier[1] { parameterModifier };
            return actualParameters;
        }

        internal static object CloneOutboundValue(object source, BinaryFormatter formatter, string name)
        {
            if (source == null || source.GetType().IsValueType)
                return source;

            ICloneable clone = source as ICloneable;
            if (clone != null)
                return clone.Clone();

            System.IO.MemoryStream stream = new System.IO.MemoryStream(1024);
            try
            {
                formatter.Serialize(stream, source);
            }
            catch (SerializationException e)
            {
                throw new InvalidOperationException(SR.GetString(SR.Error_CallExternalMethodArgsSerializationException, name), e);
            }
            stream.Position = 0;
            object cloned = formatter.Deserialize(stream);
            return cloned;
        }

        internal static void SaveOutRefParameters(object[] actualParameters, MethodBase methodBase, WorkflowParameterBindingCollection parameterBindings)
        {
            int index = 0;
            BinaryFormatter formatter = null;
            foreach (ParameterInfo formalParameter in methodBase.GetParameters())
            {
                if (parameterBindings.Contains(formalParameter.Name))
                {
                    if (formalParameter.ParameterType.IsByRef || (formalParameter.IsIn && formalParameter.IsOut))
                    {
                        WorkflowParameterBinding binding = parameterBindings[formalParameter.Name];

                        if (formatter == null)
                            formatter = new BinaryFormatter();
                        binding.Value = CloneOutboundValue(actualParameters[index], formatter, formalParameter.Name);
                    }
                }
                index++;
            }
        }
    }
}
