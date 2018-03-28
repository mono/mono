namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Diagnostics;

    internal class CodeDomLoader : IDisposable
    {
        private TypeProvider typeProvider = null;
        private CodeCompileUnit codeCompileUnit = null;
        private List<Type> types = new List<Type>();

        internal CodeDomLoader(TypeProvider typeProvider, CodeCompileUnit codeCompileUnit)
        {
            this.typeProvider = typeProvider;
            this.codeCompileUnit = codeCompileUnit;
            AddTypes();
        }

        internal void Refresh(EventHandler refresher)
        {
            RemoveTypes();
            refresher(this.typeProvider, EventArgs.Empty);
            AddTypes();
        }

        private void AddTypes()
        {
            if (this.typeProvider != null && this.types != null)
            {
                this.types.Clear();
                foreach (CodeNamespace codeNamespace in this.codeCompileUnit.Namespaces)
                {
                    foreach (CodeTypeDeclaration codeTypeDeclaration in codeNamespace.Types)
                    {
                        // Look for partial type
                        string typename = Helper.EnsureTypeName(codeTypeDeclaration.Name);

                        if (codeNamespace.Name.Length > 0)
                            typename = (Helper.EnsureTypeName(codeNamespace.Name) + "." + typename);

                        DesignTimeType partialType = this.typeProvider.GetType(typename, false) as DesignTimeType;
                        if (partialType == null)
                        {
                            partialType = new DesignTimeType(null, codeTypeDeclaration.Name, codeNamespace.Imports, codeNamespace.Name, this.typeProvider);
                            this.types.Add(partialType);
                            this.typeProvider.AddType(partialType);
                        }
                        partialType.AddCodeTypeDeclaration(codeTypeDeclaration);
                    }
                }

                Queue nestedQueue = new Queue(this.types);
                while (nestedQueue.Count != 0)
                {
                    Type type = nestedQueue.Dequeue() as Type;
                    if (type.DeclaringType != null)
                        this.types.Add(type);
                    foreach (Type nestedType2 in type.GetNestedTypes(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                        nestedQueue.Enqueue(nestedType2);
                }
            }
        }

        private void RemoveTypes()
        {
            if (this.typeProvider != null && this.types != null)
            {
                this.typeProvider.RemoveTypes(this.types.ToArray());
                this.types.Clear();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            RemoveTypes();
            this.typeProvider = null;
            this.types = null;
        }

        #endregion
    }
}
