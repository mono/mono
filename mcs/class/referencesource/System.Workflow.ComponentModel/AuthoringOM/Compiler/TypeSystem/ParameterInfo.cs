namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;

    #region DesignTimeParameterInfo
    internal sealed class DesignTimeParameterInfo : ParameterInfo
    {
        #region Members and Constructors
        private CodeTypeReference codeParameterType;
        private bool isRef = false;
        internal DesignTimeParameterInfo(CodeParameterDeclarationExpression codeParameter, int position, MemberInfo member)
        {
            this.MemberImpl = member;
            this.NameImpl = Helper.EnsureTypeName(codeParameter.Name);
            this.codeParameterType = codeParameter.Type;
            this.AttrsImpl = Helper.ConvertToParameterAttributes(codeParameter.Direction);
            this.isRef = (codeParameter.Direction == FieldDirection.Ref);
            this.PositionImpl = position;
        }

        // return param ctor
        internal DesignTimeParameterInfo(CodeTypeReference codeParameterType, MemberInfo member)
        {
            this.MemberImpl = member;
            this.NameImpl = null;
            this.codeParameterType = codeParameterType;
            this.AttrsImpl = ParameterAttributes.None;
            this.PositionImpl = -1;
        }


        #endregion

        #region Pararmeter Info overrides

        public override Type ParameterType
        {
            get
            {
                string type = DesignTimeType.GetTypeNameFromCodeTypeReference(this.codeParameterType, (this.Member.DeclaringType as DesignTimeType));
                if ((this.AttrsImpl & ParameterAttributes.Out) > 0 || this.isRef)
                    type += '&'; // Append with & for (ref & out) parameter types
                this.ClassImpl = (this.Member.DeclaringType as DesignTimeType).ResolveType(type);
                return base.ParameterType;
            }
        }
        #endregion
    }
    #endregion
}
