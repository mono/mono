//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities.Design
{
    using System.Collections.Generic;
    using System.ServiceModel;

    abstract class NamedObjectList<T> : List<T>
    {
        int suffixGenerator;

        protected abstract string GeneratedNameFormatResource
        { get; }

        public T CreateWithUniqueName()
        {
            string generatedName;
            do
            {
                generatedName = SR2.GetString(this.GeneratedNameFormatResource, ++this.suffixGenerator);
            } while (this.Find(generatedName) != null);

            return this.CreateObject(generatedName);
        }

        public T Find(string name)
        {
            T result = default(T);
            foreach (T obj in this)
            {
                if (this.GetName(obj) == name)
                {
                    result = obj;
                    break;
                }
            }
            return result;
        }

        protected abstract T CreateObject(string name);
        protected abstract string GetName(T obj);
    }
}
