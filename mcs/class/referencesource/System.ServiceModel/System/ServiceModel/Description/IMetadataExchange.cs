//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceContract(ConfigurationName = ServiceMetadataBehavior.MexContractName, Name = ServiceMetadataBehavior.MexContractName, Namespace = ServiceMetadataBehavior.MexContractNamespace)]
    public interface IMetadataExchange
    {
        [OperationContract(Action = MetadataStrings.WSTransfer.GetAction, ReplyAction = MetadataStrings.WSTransfer.GetResponseAction)]
        Message Get(Message request);

        [OperationContract(Action = MetadataStrings.WSTransfer.GetAction, ReplyAction = MetadataStrings.WSTransfer.GetResponseAction, AsyncPattern = true)]
        IAsyncResult BeginGet(Message request, AsyncCallback callback, object state);
        Message EndGet(IAsyncResult result);

    }
}
