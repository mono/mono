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

    #region DesignTimeEventInfo
    internal sealed class DesignTimeEventInfo : EventInfo
    {

        #region Members and Constructors

        private string name;
        private DesignTimeMethodInfo addMethod = null;
        private DesignTimeMethodInfo removeMethod = null;
        private Attribute[] attributes = null;

        private MemberAttributes memberAttributes;
        private DesignTimeType declaringType;
        private CodeMemberEvent codeDomEvent;

        internal DesignTimeEventInfo(DesignTimeType declaringType, CodeMemberEvent codeDomEvent)
        {
            if (declaringType == null)
            {
                throw new ArgumentNullException("Declaring Type");
            }

            if (codeDomEvent == null)
            {
                throw new ArgumentNullException("codeDomEvent");
            }

            this.declaringType = declaringType;
            this.codeDomEvent = codeDomEvent;
            this.name = Helper.EnsureTypeName(codeDomEvent.Name);
            this.memberAttributes = codeDomEvent.Attributes;

            this.addMethod = null;
            this.removeMethod = null;
        }

        #endregion

        #region Event Info overrides

        public override MethodInfo GetAddMethod(bool nonPublic)
        {
            if (this.addMethod == null)
            {
                Type handlerType = declaringType.ResolveType(DesignTimeType.GetTypeNameFromCodeTypeReference(this.codeDomEvent.Type, declaringType));
                if (handlerType != null)
                {
                    CodeMemberMethod codeAddMethod = new CodeMemberMethod();

                    codeAddMethod.Name = "add_" + this.name;
                    codeAddMethod.ReturnType = new CodeTypeReference(typeof(void));
                    codeAddMethod.Parameters.Add(new CodeParameterDeclarationExpression(this.codeDomEvent.Type, "Handler"));
                    codeAddMethod.Attributes = this.memberAttributes;
                    this.addMethod = new DesignTimeMethodInfo(this.declaringType, codeAddMethod, true);
                }
            }
            return this.addMethod;
        }

        public override MethodInfo GetRemoveMethod(bool nonPublic)
        {
            if (this.removeMethod == null)
            {
                Type handlerType = declaringType.ResolveType(DesignTimeType.GetTypeNameFromCodeTypeReference(this.codeDomEvent.Type, declaringType));
                if (handlerType != null)
                {
                    CodeMemberMethod codeRemoveMethod = new CodeMemberMethod();

                    codeRemoveMethod.Name = "remove_" + this.name;
                    codeRemoveMethod.ReturnType = new CodeTypeReference(typeof(void));
                    codeRemoveMethod.Parameters.Add(new CodeParameterDeclarationExpression(handlerType, "Handler"));
                    codeRemoveMethod.Attributes = this.memberAttributes;
                    this.removeMethod = new DesignTimeMethodInfo(declaringType, codeRemoveMethod, true);
                }
            }
            return this.removeMethod;
        }

        public override MethodInfo GetRaiseMethod(bool nonPublic)
        {
            return null;
        }

        public override EventAttributes Attributes
        {
            //We're not interested in this flag 
            get
            {
                return default(EventAttributes);
            }
        }
        #endregion

        #region MemberInfo Overrides
        public override string Name
        {
            get
            {
                return this.name;
            }
        }
        public override Type DeclaringType
        {
            get
            {
                return this.declaringType;
            }
        }
        public override Type ReflectedType
        {
            get
            {
                return this.declaringType;
            }
        }
        public override object[] GetCustomAttributes(bool inherit)
        {
            return GetCustomAttributes(typeof(object), inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == null)
                throw new ArgumentNullException("attributeType");

            if (this.attributes == null)
                this.attributes = Helper.LoadCustomAttributes(this.codeDomEvent.CustomAttributes, this.DeclaringType as DesignTimeType);

            return Helper.GetCustomAttributes(attributeType, inherit, this.attributes, this);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            if (attributeType == null)
                throw new ArgumentNullException("attributeType");

            if (this.attributes == null)
                this.attributes = Helper.LoadCustomAttributes(this.codeDomEvent.CustomAttributes, this.DeclaringType as DesignTimeType);

            if (Helper.IsDefined(attributeType, inherit, attributes, this))
                return true;

            return false;
        }

        #endregion

        #region Helpers

        internal bool IsPublic
        {
            get
            {
                return ((memberAttributes & MemberAttributes.Public) != 0);
            }
        }
        internal bool IsStatic
        {
            get
            {
                return ((memberAttributes & MemberAttributes.Static) != 0);
            }
        }
        #endregion

    }
    #endregion
}
