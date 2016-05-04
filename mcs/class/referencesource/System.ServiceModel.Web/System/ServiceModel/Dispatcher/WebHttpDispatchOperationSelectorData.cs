//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Collections.Generic;
    using System.Text;

    class WebHttpDispatchOperationSelectorData
    {
        internal List<string> AllowedMethods { get; set; }

        internal string AllowHeader
        {
            get
            {
                if (this.AllowedMethods != null)
                {
                    int allowedHeadersCount = this.AllowedMethods.Count;
                    if (allowedHeadersCount > 0)
                    {
                        StringBuilder stringBuilder = new StringBuilder(AllowedMethods[0]);
                        for (int x = 1; x < allowedHeadersCount; x++)
                        {
                            stringBuilder.Append(", " + this.AllowedMethods[x]);
                        }

                        return stringBuilder.ToString();
                    }
                }
                return null;
            }
        }
    }
}
