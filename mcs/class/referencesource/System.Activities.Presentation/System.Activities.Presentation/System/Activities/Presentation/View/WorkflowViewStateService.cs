//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Xaml;
    using System.Activities.Presentation.Model;
    using System.Runtime;

    //ViewState is stored as a Dictionary<string, object> on the CFx object. 
    //ModelItem is passed in StoreViewState to get a handle to the CFx object.
    [Fx.Tag.XamlVisible(false)]
    public class WorkflowViewStateService : ViewStateService
    {
        EditingContext context;

        public override event ViewStateChangedEventHandler ViewStateChanged;

        public override event ViewStateChangedEventHandler UndoableViewStateChanged;

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly AttachableMemberIdentifier ViewStateName = new AttachableMemberIdentifier(typeof(WorkflowViewStateService), "ViewState");
        
        UndoEngine UndoEngine
        {
            get
            {
                return this.context.Services.GetService<UndoEngine>();
            }
        }

        public WorkflowViewStateService(EditingContext context)
        {
            this.context = context;
        }

        public static Dictionary<string, object> GetViewState(object instance)
        {
            if (instance == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("instance"));
            }
            Dictionary<string, object> viewState;
            AttachablePropertyServices.TryGetProperty(instance, ViewStateName, out viewState);
            return viewState;
        }

        public static void SetViewState(object instance, Dictionary<string, object> value)
        {
            if (instance == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("instance"));
            }
            AttachablePropertyServices.SetProperty(instance, ViewStateName, value);
        }


        public override object RetrieveViewState(ModelItem modelItem, string key)
        {
            if (modelItem == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("modelItem"));
            }
            if (key == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("key"));
            }

            object viewStateObj = null;
            Dictionary<string, object> viewState = WorkflowViewStateService.GetViewState(modelItem.GetCurrentValue());
            if (viewState != null)
            {
                viewState.TryGetValue(key, out viewStateObj);
            }
            return viewStateObj;
        }

        public override void StoreViewState(ModelItem modelItem, string key, object value)
        {
            if (modelItem == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("modelItem"));
            }
            if (key == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("key"));
            }
          
            object oldValue = null;
            Dictionary<string, object> viewState = WorkflowViewStateService.GetViewState(modelItem.GetCurrentValue());
            if (viewState == null)
            {
                viewState = new Dictionary<string, object>();
                WorkflowViewStateService.SetViewState(modelItem.GetCurrentValue(), viewState);
            }
            viewState.TryGetValue(key, out oldValue);
            if (value != null)
            {
                viewState[key] = value;
            }
            else
            {
                RemoveViewState(modelItem, key);
            }
            if (this.ViewStateChanged != null && value != oldValue)
            {
                this.ViewStateChanged(this, new ViewStateChangedEventArgs(modelItem, key, value, oldValue));
            }
        }

        public override bool RemoveViewState(ModelItem modelItem, string key)
        {
            if (modelItem == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("modelItem"));
            }
            if (key == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("key"));
            }
            bool itemRemoved = false;
            Dictionary<string, object> viewState = WorkflowViewStateService.GetViewState(modelItem.GetCurrentValue());
            if (viewState != null && key != null && viewState.ContainsKey(key))
            {
                itemRemoved = viewState.Remove(key);
                if (viewState.Keys.Count == 0)
                {
                    AttachablePropertyServices.RemoveProperty(modelItem.GetCurrentValue(), ViewStateName);
                }
            }
            return itemRemoved;
        }

        public override Dictionary<string, object> RetrieveAllViewState(ModelItem modelItem)
        {
            if (modelItem == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("modelItem"));
            }
            return WorkflowViewStateService.GetViewState(modelItem.GetCurrentValue());
        }
    

        public override void StoreViewStateWithUndo(ModelItem modelItem, string key, object value)
        {
            object oldValue = RetrieveViewState(modelItem, key);
            ViewStateChange vsChange = new ViewStateChange(this)
                                        {
                                            Item = modelItem,
                                            Key = key,
                                            OldValue = oldValue,
                                            NewValue = value,
                                        };
            ModelTreeManager modelTreeManager = this.context.Services.GetService<ModelTreeManager>();
            if (modelTreeManager != null)
            {
                modelTreeManager.AddToCurrentEditingScope(vsChange);
            }
        }


        void RaiseUndoableViewStateChangedEvent(ModelItem modelItem, string key, object newValue, object oldValue)
        {
            if (this.UndoableViewStateChanged != null)
            {
                this.UndoableViewStateChanged(this, new ViewStateChangedEventArgs(modelItem, key, newValue, oldValue));
            }
        }


        internal class ViewStateChange : Change
        {
            protected WorkflowViewStateService viewStateService;

            public ModelItem Item { get; set; }
            public string Key { get; set; }
            public object OldValue { get; set; }
            public object NewValue { get; set; }

            public ViewStateChange(WorkflowViewStateService viewStateService) 
            {
                this.viewStateService = viewStateService;
            }


            public override string Description
            {
                get { return SR.ViewStateUndoUnitDescription; }
            }

            public override bool Apply()
            {
                viewStateService.StoreViewState(Item, Key, NewValue);
                this.viewStateService.RaiseUndoableViewStateChangedEvent(Item, Key, NewValue, OldValue);
                return true;
            }

            public override Change GetInverse()
            {
                return new ViewStateChange(this.viewStateService)
                    {
                        Item = this.Item,
                        Key = this.Key,
                        OldValue = this.NewValue,
                        NewValue = this.OldValue
                    };
            }
        }


    }

}
