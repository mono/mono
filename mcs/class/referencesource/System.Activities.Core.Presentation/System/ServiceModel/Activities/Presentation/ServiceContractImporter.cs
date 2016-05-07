// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Activities.Presentation
{
    using System.Activities;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.Toolbox;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.Xaml;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.ServiceModel.Description;
    using System.Xaml;
    using System.Xml;
    using Microsoft.Activities.Presentation.Xaml;

    /// <summary>
    /// ServiceContractImporter is an activity template generator that generate Receive/ReceiveAndSendReply based on ContractDescription
    /// </summary>
    public static class ServiceContractImporter
    {
        /// <summary>
        /// The key used to store the contract type view state.
        /// </summary>
        public const string ContractTypeViewStateKey = "contractType";

        private const string ReceiveSuffix = "_Receive";
        private const string SendReplySuffix = "_SendReply";
        private const string SendFaultReply = "SendFaultReply";
        private const string SendFaultReplySuffix = "_" + SendFaultReply;
        private const string ReceiveAndSendReplySuffix = "_ReceiveAndSendReply";

        /// <summary>
        /// Gets the filter function for screening out non service contract types.
        /// </summary>
        internal static Func<Type, bool> FilterFunction
        {
            get
            {
                return t => t.IsDefined(typeof(ServiceContractAttribute), true) && !t.IsGenericType;
            }
        }

        /// <summary>
        /// Launch the user interface for developer to pick a contract type.
        /// </summary>
        /// <param name="localAssemblyName">The local assembly name.</param>
        /// <param name="referencedAssemblies">The list of referenced assembly names.</param>
        /// <param name="editingContext">The editing context.</param>
        /// <returns>The contract type selected by user or null if user cancels.</returns>
        public static Type SelectContractType(AssemblyName localAssemblyName, IList<AssemblyName> referencedAssemblies, EditingContext editingContext)
        {
            AssemblyContextControlItem assemblyContextControlItem = new AssemblyContextControlItem { LocalAssemblyName = localAssemblyName, ReferencedAssemblyNames = referencedAssemblies };
            TypeBrowser typeBrowser = new TypeBrowser(assemblyContextControlItem, editingContext, FilterFunction);
            bool? dialogResult = typeBrowser.ShowDialog(/* owner = */ null);
            if (dialogResult.HasValue && dialogResult.Value)
            {
                return typeBrowser.ConcreteType;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Generate activity templates based on contract
        /// </summary>
        /// <param name="contractType">The contract type</param>
        /// <returns>The list of all activity templates, each represent an operation.</returns>
        public static IEnumerable<ActivityTemplateFactoryBuilder> GenerateActivityTemplates(Type contractType)
        {
            if (contractType == null)
            {
                throw FxTrace.Exception.ArgumentNull("contractType");
            }

            ContractDescription contract = null;
            try
            {
                contract = ContractDescription.GetContract(contractType);
            }
            catch (InvalidOperationException e)
            {
                if (e.InnerException != null)
                {
                    throw new InvalidOperationException(e.Message + Environment.NewLine + Environment.NewLine + e.InnerException.GetType().ToString() + Environment.NewLine + e.InnerException.Message, e);
                }
                else
                {
                    throw;
                }
            }
            
            // 
            if (contract.Operations != null)
            {
                foreach (OperationDescription operation in contract.Operations)
                {
                    Activity generatedActivity = GenerateActivity(operation);

                    WorkflowViewStateService.SetViewState(
                        generatedActivity,
                        new Dictionary<string, object>
                        {
                             { ContractTypeViewStateKey, contractType },
                        });
                    yield return new ActivityTemplateFactoryBuilder
                    {
                        Name = contractType.Name + "." + generatedActivity.DisplayName,
                        TargetType = generatedActivity.GetType(),
                        Implementation = generatedActivity,
                    };
                }
            }
        }

        /// <summary>
        /// Save the activity template into its XAML representation.
        /// </summary>
        /// <param name="activityTemplate">The activity template to be saved.</param>
        /// <returns>The XAML representation of the activity template.</returns>
        public static string SaveActivityTemplate(ActivityTemplateFactoryBuilder activityTemplate)
        {
            XamlSchemaContext xsc = new XamlSchemaContext();
            using (XamlReader reader = new XamlObjectReader(activityTemplate, xsc))
            {
                using (StringWriter textWriter = new StringWriter(CultureInfo.InvariantCulture))
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, new XmlWriterSettings { Indent = true }))
                    {
                        using (XamlWriter xamlWriter = new XamlXmlWriter(xmlWriter, xsc))
                        {
                            using (ActivityTemplateFactoryBuilderWriter builderWriter = new ActivityTemplateFactoryBuilderWriter(xamlWriter, xsc))
                            {
                                XamlServices.Transform(reader, builderWriter);
                            }
                        }
                    }

                    return textWriter.ToString();
                }
            }
        }

        internal static Activity GenerateActivity(OperationDescription operation)
        {
            if (operation == null)
            {
                throw FxTrace.Exception.ArgumentNull("operation");
            }

            Receive receive = Receive.FromOperationDescription(operation);
            receive.DisplayName = receive.OperationName + ReceiveSuffix;
            IEnumerable<SendReply> faultReplies;
            SendReply reply = SendReply.FromOperationDescription(operation, out faultReplies);
            if (reply != null)
            {
                reply.DisplayName = receive.OperationName + SendReplySuffix;
                Variable<CorrelationHandle> handle = new Variable<CorrelationHandle> { Name = "__handle" };
                receive.CorrelationInitializers.Add(new RequestReplyCorrelationInitializer { CorrelationHandle = new InArgument<CorrelationHandle>(handle) });
                reply.Request = receive;

                Activity replyActivity = reply;

                Switch<string> replySelector = null;
                foreach (SendReply faultReply in faultReplies)
                {
                    if (replySelector == null)
                    {
                        replySelector = new Switch<string>
                        {
                            Default = reply
                        };
                        replyActivity = replySelector;
                    }

                    faultReply.Request = receive;
                    string faultName = faultReply.DisplayName.Substring(0, faultReply.DisplayName.Length - SendFaultReply.Length);
                    faultReply.DisplayName = receive.OperationName + SendFaultReplySuffix;
                    replySelector.Cases.Add(faultName, faultReply);
                }

                return new Sequence
                {
                    DisplayName = receive.OperationName + ReceiveAndSendReplySuffix,
                    Variables = { handle },
                    Activities = { receive, replyActivity },
                };
            }
            else
            {
                return receive;
            }
        }
    }
}
