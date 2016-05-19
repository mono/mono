//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation
{
    // WorkflowDesignerStorageService is available in the designer's primary host, Visual Studio. Developers can 
    // store/retrieve data using this service at any time. The data stored is cleared when the designer is closed
    // and re-opened manually. But if the designer is automatically reloaded by Visual Studio after build is done
    // or the reference assemblies are updated, the data stored will not be lost.
    
    // The data stored must be serialzable, otherwise exception will be thrown by SetData/AddData.
    public interface IWorkflowDesignerStorageService
    {
        void AddData(string key, object value);
        void RemoveData(string key);
        object GetData(string key);
        void SetData(string key, object value);
        bool ContainsKey(string key);
    }
}
