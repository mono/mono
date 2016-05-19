//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Collections.Generic;

    // Service used to update Toolbox contents from a Designer.
    public interface IActivityToolboxService
    {
        void AddCategory(string categoryName);
        void RemoveCategory(string categoryName);
        void AddItem(string qualifiedTypeName, string categoryName);
        void RemoveItem(string qualifiedTypeName, string categoryName);
        IList<string> EnumCategories();
        IList<string> EnumItems(string categoryName);
    }
}
