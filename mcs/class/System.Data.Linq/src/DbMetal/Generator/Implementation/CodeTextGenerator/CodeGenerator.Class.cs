#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Linq;
using DbLinq.Schema.Dbml;
using DbLinq.Schema.Dbml.Adapter;
using DbLinq.Util;
using DbMetal.Generator.EntityInterface;

#if MONO_STRICT
using System.Data.Linq;
#else
using DbLinq.Data.Linq;
#endif

namespace DbMetal.Generator.Implementation.CodeTextGenerator
{
#if !MONO_STRICT
    public
#endif
    partial class CodeGenerator
    {
        protected virtual void WriteClasses(CodeWriter writer, Database schema, GenerationContext context)
        {
            IEnumerable<Table> tables = schema.Tables;

            var types = context.Parameters.GenerateTypes;
            if (types.Count > 0)
                tables = tables.Where(t => types.Contains(t.Type.Name));

            foreach (var table in tables)
                WriteClass(writer, table, schema, context);
        }

        protected virtual void WriteClass(CodeWriter writer, Table table, Database schema, GenerationContext context)
        {
            writer.WriteLine();

            string entityBase = context.Parameters.EntityBase;
            if (string.IsNullOrEmpty(entityBase))
                entityBase = schema.EntityBase;

            var specifications = SpecificationDefinition.Partial;
            if (table.Type.AccessModifierSpecified)
                specifications |= GetSpecificationDefinition(table.Type.AccessModifier);
            else
                specifications |= SpecificationDefinition.Public;
            if (table.Type.ModifierSpecified)
                specifications |= GetSpecificationDefinition(table.Type.Modifier);

            var tableAttribute = NewAttributeDefinition<TableAttribute>();
            tableAttribute["Name"] = table.Name;
            //using (WriteAttributes(writer, context.Parameters.EntityExposedAttributes))
            using (WriteAttributes(writer, GetAttributeNames(context, context.Parameters.EntityExposedAttributes)))
            using (writer.WriteAttribute(tableAttribute))
            using (writer.WriteClass(specifications,
                                     table.Type.Name, entityBase, context.Parameters.EntityInterfaces))
            {
                WriteClassHeader(writer, table, context);
                WriteCustomTypes(writer, table, schema, context);
                WriteClassExtensibilityDeclarations(writer, table, context);
                WriteClassProperties(writer, table, context);
                if (context.Parameters.GenerateEqualsAndHash)
                    WriteClassEqualsAndHash(writer, table, context);
                WriteClassChildren(writer, table, schema, context);
                WriteClassParents(writer, table, schema, context);
                WriteClassChildrenAttachment(writer, table, schema, context);
                WriteClassCtor(writer, table, schema, context);
            }
        }

        protected virtual void WriteClassEqualsAndHash(CodeWriter writer, Table table, GenerationContext context)
        {
            List<DbLinq.Schema.Dbml.Column> primaryKeys = table.Type.Columns.Where(c => c.IsPrimaryKey).ToList();
            if (primaryKeys.Count == 0)
            {
                writer.WriteLine("#warning L189 table {0} has no primary key. Multiple C# objects will refer to the same row.",
                                 table.Name);
                return;
            }

            using (writer.WriteRegion(string.Format("GetHashCode(), Equals() - uses column {0} to look up objects in liveObjectMap",
                                                    string.Join(", ", primaryKeys.Select(pk => pk.Member).ToList().ToArray()))))
            {
                // GetHashCode
                using (writer.WriteMethod(SpecificationDefinition.Public | SpecificationDefinition.Override,
                                          "GetHashCode", typeof(int)))
                {
                    string hashCode = null;

                    foreach (var primaryKey in primaryKeys)
                    {
                        var member = writer.GetVariableExpression(primaryKey.Storage);
                        string primaryKeyHashCode = writer.GetMethodCallExpression(writer.GetMemberExpression(member, "GetHashCode"));
                        if (primaryKey.CanBeNull
                        || primaryKey.ExtendedType == null
                        || GetType(primaryKey.Type, false).IsClass) // this patch to ensure that even if DB does not allow nulls,
                        // our in-memory object won't generate a fault
                        {
                            var isNullExpression = writer.GetEqualExpression(member, writer.GetNullExpression());
                            var nullExpression = writer.GetLiteralValue(0);
                            primaryKeyHashCode = writer.GetTernaryExpression(isNullExpression, nullExpression, primaryKeyHashCode);
                        }
                        if (string.IsNullOrEmpty(hashCode))
                            hashCode = primaryKeyHashCode;
                        else
                            hashCode = writer.GetXOrExpression(hashCode, primaryKeyHashCode);
                    }
                    writer.WriteLine(writer.GetReturnStatement(hashCode));
                }
                writer.WriteLine();

                // Equals
                string otherAsObject = "o";
                using (writer.WriteMethod(SpecificationDefinition.Public | SpecificationDefinition.Override,
                                          "Equals", typeof(bool), new ParameterDefinition { Type = typeof(object), Name = otherAsObject }))
                {
                    string other = "other";
                    writer.WriteLine(writer.GetStatement(writer.GetAssignmentExpression(
                                                             writer.GetDeclarationExpression(other, table.Type.Name),
                                                             writer.GetCastExpression(otherAsObject, table.Type.Name,
                                                                                      false))));
                    using (writer.WriteIf(writer.GetEqualExpression(other, writer.GetNullExpression())))
                    {
                        writer.WriteLine(writer.GetReturnStatement(writer.GetLiteralValue(false)));
                    }
                    string andExpression = null;
                    foreach (var primaryKey in primaryKeys)
                    {
                        var member = writer.GetVariableExpression(primaryKey.Storage);
                        string primaryKeyTest = writer.GetMethodCallExpression(writer.GetMemberExpression(writer.GetLiteralType(typeof(object)), "Equals"),
                                                                               member,
                                                                               writer.GetMemberExpression(other, member));
                        if (string.IsNullOrEmpty(andExpression))
                            andExpression = primaryKeyTest;
                        else
                            andExpression = writer.GetAndExpression(andExpression, primaryKeyTest);
                    }
                    writer.WriteLine(writer.GetReturnStatement(andExpression));
                }
            }
        }

        /// <summary>
        /// Class headers are written at top of class
        /// They consist in specific headers writen by interface implementors
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="table"></param>
        /// <param name="context"></param>
        private void WriteClassHeader(CodeWriter writer, Table table, GenerationContext context)
        {
            foreach (IImplementation implementation in context.Implementations())
                implementation.WriteClassHeader(writer, table, context);
        }

        protected virtual void WriteClassExtensibilityDeclarations(CodeWriter writer, Table table, GenerationContext context)
        {
            using (writer.WriteRegion("Extensibility Method Definitions"))
            {
                writer.WriteLine("partial void OnCreated();");
                foreach (var c in table.Type.Columns)
                {
                    writer.WriteLine("partial void On{0}Changed();", c.Member);
                    writer.WriteLine("partial void On{0}Changing({1} value);", c.Member, GetTypeOrExtendedType(writer, c));
                }
            }
        }

        /// <summary>
        /// Writes all properties, depending on the use (simple property or FK)
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="table"></param>
        /// <param name="context"></param>
        protected virtual void WriteClassProperties(CodeWriter writer, Table table, GenerationContext context)
        {
            foreach (var property in table.Type.Columns)
            {
                var property1 = property;
                var relatedAssociations = from a in table.Type.Associations
                                          where a.IsForeignKey
                                          && a.TheseKeys.Contains(property1.Name)
                                          select a;
                WriteClassProperty(writer, property, relatedAssociations, context);
            }
        }

        protected virtual string GetTypeOrExtendedType(CodeWriter writer, Column property)
        {
            object extendedType = property.ExtendedType;
            var enumType = extendedType as EnumType;
            if (enumType != null)
                return writer.GetEnumType(enumType.Name);
            return writer.GetLiteralType(GetType(property.Type, property.CanBeNull));
        }

        /// <summary>
        /// Writes class property
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="property"></param>
        /// <param name="relatedAssociations">non null if property is a FK</param>
        /// <param name="context"></param>
        protected virtual void WriteClassProperty(CodeWriter writer, Column property, IEnumerable<Association> relatedAssociations, GenerationContext context)
        {
            using (writer.WriteRegion(string.Format("{0} {1}", GetTypeOrExtendedType(writer, property), property.Member)))
            {
                WriteClassPropertyBackingField(writer, property, context);
                WriteClassPropertyAccessors(writer, property, relatedAssociations, context);
            }
        }

        protected virtual void WriteClassPropertyBackingField(CodeWriter writer, Column property, GenerationContext context)
        {
            //AttributeDefinition autoGenAttribute = null;
            //if (property.IsDbGenerated)
            //    autoGenAttribute = NewAttributeDefinition<AutoGenIdAttribute>();
            //using (writer.WriteAttribute(autoGenAttribute))
            // for auto-properties, we just won't generate a private field
            if (property.Storage != null)
                writer.WriteField(SpecificationDefinition.Private, property.Storage, GetTypeOrExtendedType(writer, property));
        }

        /// <summary>
        /// Returns a name from a given fullname
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        protected virtual string GetName(string fullName)
        {
            var namePartIndex = fullName.LastIndexOf('.');
            // if we have a dot, we have a namespace
            if (namePartIndex > 0)
                return fullName.Substring(namePartIndex + 1);
            // otherwise, it's just a name, that we keep as is
            return fullName;
        }

        /// <summary>
        /// Returns name for given list of attributes
        /// </summary>
        /// <param name="context"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        protected virtual string[] GetAttributeNames(GenerationContext context, string[] attributes)
        {
            return (from a in attributes select GetName(a)).ToArray();
        }

        private class EnumFullname
        {
            private string _EnumName;
            private object _EnumValue;

            public EnumFullname(string enumName, object enumValue)
            {
                _EnumName = enumName;
                _EnumValue = enumValue;
            }

            public override string ToString()
            {
                return string.Format("{0}.{1}", _EnumName, _EnumValue.ToString());
            }
        }

        /// <summary>
        /// Writes property accessor
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="property"></param>
        /// <param name="relatedAssociations"></param>
        /// <param name="context"></param>
        protected virtual void WriteClassPropertyAccessors(CodeWriter writer, Column property, IEnumerable<Association> relatedAssociations, GenerationContext context)
        {
            //generate [Column(...)] attribute
            var column = NewAttributeDefinition<ColumnAttribute>();
            column["Storage"] = property.Storage;
            column["Name"] = property.Name;
            column["DbType"] = property.DbType;
            // be smart: we only write attributes when they differ from the default values
            var columnAttribute = new ColumnAttribute();
            if (property.IsPrimaryKey != columnAttribute.IsPrimaryKey)
                column["IsPrimaryKey"] = property.IsPrimaryKey;
            if (property.IsDbGenerated != columnAttribute.IsDbGenerated)
                column["IsDbGenerated"] = property.IsDbGenerated;
            if (property.AutoSync != DbLinq.Schema.Dbml.AutoSync.Default)
                column["AutoSync"] = new EnumFullname("AutoSync", property.AutoSync);
            if (property.CanBeNull != columnAttribute.CanBeNull)
                column["CanBeNull"] = property.CanBeNull;
            if (property.Expression != null)
                column["Expression"] = property.Expression;

            var specifications = property.AccessModifierSpecified
                                     ? GetSpecificationDefinition(property.AccessModifier)
                                     : SpecificationDefinition.Public;
            if (property.ModifierSpecified)
                specifications |= GetSpecificationDefinition(property.Modifier);

            //using (WriteAttributes(writer, context.Parameters.MemberExposedAttributes))
            using (WriteAttributes(writer, GetAttributeNames(context, context.Parameters.MemberExposedAttributes)))
            using (writer.WriteAttribute(NewAttributeDefinition<DebuggerNonUserCodeAttribute>()))
            using (writer.WriteAttribute(column))
            using (writer.WriteProperty(specifications, property.Member, GetTypeOrExtendedType(writer, property)))
            {
                // on auto storage, we're just lazy
                if (property.Storage == null)
                    writer.WriteAutomaticPropertyGetSet();
                else
                {
                    using (writer.WritePropertyGet())
                    {
                        writer.WriteLine(writer.GetReturnStatement(writer.GetVariableExpression(property.Storage)));
                    }
                    using (writer.WritePropertySet())
                    {
                        WriteClassPropertyAccessorSet(writer, property, relatedAssociations, context);
                    }
                }
            }
        }

        /// <summary>
        /// Writes property setter, for FK properties
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="property"></param>
        /// <param name="relatedAssociations"></param>
        /// <param name="context"></param>
        private void WriteClassPropertyAccessorSet(CodeWriter writer, Column property, IEnumerable<Association> relatedAssociations, GenerationContext context)
        {
            // if new value if different from old one
            using (writer.WriteIf(writer.GetDifferentExpression(writer.GetPropertySetValueExpression(),
                                                                writer.GetVariableExpression(property.Storage))))
            {
                // if the property is used as a FK, we ensure that it hasn't been already loaded or assigned
                foreach (var relatedAssociation in relatedAssociations)
                {
                    // first thing to check: ensure the association backend isn't already affected.
                    // if it is the case, then the property must not be manually set

                    // R# considers the following as an error, but the csc doesn't
                    //var memberName = ReflectionUtility.GetMemberInfo<DbLinq.Data.Linq.EntityRef<object>>(e => e.HasLoadedOrAssignedValue).Name;
                    var memberName = "HasLoadedOrAssignedValue";
                    using (writer.WriteIf(writer.GetMemberExpression(relatedAssociation.Storage, memberName)))
                    {
                        writer.WriteLine(writer.GetThrowStatement(writer.GetNewExpression(
                                                                      writer.GetMethodCallExpression(
                                                                          writer.GetLiteralFullType(
                                                                              typeof(
                                                                                  System.Data.Linq.
                                                                                  ForeignKeyReferenceAlreadyHasValueException
                                                                                  )))
                                                                      )));
                    }
                }
                // the before and after are used by extensions related to interfaces
                // for example INotifyPropertyChanged
                // here the code before the change
                foreach (IImplementation implementation in context.Implementations())
                    implementation.WritePropertyBeforeSet(writer, property, context);
                // property assignment
                writer.WriteLine(
                    writer.GetStatement(
                        writer.GetAssignmentExpression(writer.GetVariableExpression(property.Storage),
                                                       writer.GetPropertySetValueExpression())));
                // here the code after change
                foreach (IImplementation implementation in context.Implementations())
                    implementation.WritePropertyAfterSet(writer, property, context);
            }
        }

        /// <summary>
        /// Returns all children (ie members of EntitySet)
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        protected virtual IEnumerable<Association> GetClassChildren(Table table)
        {
            return table.Type.Associations.Where(a => !a.IsForeignKey);
        }

        /// <summary>
        /// Returns all parents (ie member referenced as EntityRef)
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        protected virtual IEnumerable<Association> GetClassParents(Table table)
        {
            return table.Type.Associations.Where(a => a.IsForeignKey);
        }

        protected virtual void WriteClassChildren(CodeWriter writer, Table table, Database schema, GenerationContext context)
        {
            var children = GetClassChildren(table).ToList();
            if (children.Count > 0)
            {
                using (writer.WriteRegion("Children"))
                {
                    foreach (var child in children)
                    {
                        bool hasDuplicates = (from c in children where c.Member == child.Member select c).Count() > 1;
                        WriteClassChild(writer, child, hasDuplicates, schema, context);
                    }
                }
            }
        }

        private void WriteClassChild(CodeWriter writer, Association child, bool hasDuplicates, Database schema, GenerationContext context)
        {
            // the following is apparently useless
            DbLinq.Schema.Dbml.Table targetTable = schema.Tables.FirstOrDefault(t => t.Type.Name == child.Type);
            if (targetTable == null)
            {
                //Logger.Write(Level.Error, "ERROR L143 target table class not found:" + child.Type);
                return;
            }

            var storageAttribute = NewAttributeDefinition<AssociationAttribute>();
            storageAttribute["Storage"] = child.Storage;
            storageAttribute["OtherKey"] = child.OtherKey;
            storageAttribute["ThisKey"] = child.ThisKey;
            storageAttribute["Name"] = child.Name;

            SpecificationDefinition specifications;
            if (child.AccessModifierSpecified)
                specifications = GetSpecificationDefinition(child.AccessModifier);
            else
                specifications = SpecificationDefinition.Public;
            if (child.ModifierSpecified)
                specifications |= GetSpecificationDefinition(child.Modifier);

            var propertyName = hasDuplicates
                                   ? child.Member + "_" + string.Join("", child.OtherKeys.ToArray())
                                   : child.Member;

            var propertyType = writer.GetGenericName(TypeExtensions.GetShortName(typeof(EntitySet<>)), child.Type);

            if (child.Storage != null)
                writer.WriteField(SpecificationDefinition.Private, child.Storage, propertyType);

            using (writer.WriteAttribute(storageAttribute))
            using (writer.WriteAttribute(NewAttributeDefinition<DebuggerNonUserCodeAttribute>()))
            using (writer.WriteProperty(specifications, propertyName,
                                        writer.GetGenericName(TypeExtensions.GetShortName(typeof(EntitySet<>)), child.Type)))
            {
                // if we have a backing field, use it
                if (child.Storage != null)
                {
                    // the getter returns the field
                    using (writer.WritePropertyGet())
                    {
                        writer.WriteLine(writer.GetReturnStatement(
                            child.Storage
                            ));
                    }
                    // the setter assigns the field
                    using (writer.WritePropertySet())
                    {
                        writer.WriteLine(writer.GetStatement(
                            writer.GetAssignmentExpression(
                            child.Storage,
                            writer.GetPropertySetValueExpression())
                            ));
                    }
                }
                // otherwise, use automatic property
                else
                    writer.WriteAutomaticPropertyGetSet();
            }
            writer.WriteLine();
        }

        protected virtual void WriteClassParents(CodeWriter writer, Table table, Database schema, GenerationContext context)
        {
            var parents = GetClassParents(table).ToList();
            if (parents.Count > 0)
            {
                using (writer.WriteRegion("Parents"))
                {
                    foreach (var parent in parents)
                    {
                        bool hasDuplicates = (from p in parents where p.Member == parent.Member select p).Count() > 1;
                        WriteClassParent(writer, parent, hasDuplicates, schema, context);
                    }
                }
            }
        }

        protected virtual void WriteClassParent(CodeWriter writer, Association parent, bool hasDuplicates, Database schema, GenerationContext context)
        {
            // the following is apparently useless
            DbLinq.Schema.Dbml.Table targetTable = schema.Tables.FirstOrDefault(t => t.Type.Name == parent.Type);
            if (targetTable == null)
            {
                //Logger.Write(Level.Error, "ERROR L191 target table type not found: " + parent.Type + "  (processing " + parent.Name + ")");
                return;
            }

            string member = parent.Member;
            string storageField = parent.Storage;
            // TODO: remove this
            if (member == parent.ThisKey)
            {
                member = parent.ThisKey + targetTable.Type.Name; //repeat name to prevent collision (same as Linq)
                storageField = "_x_" + parent.Member;
            }

            writer.WriteField(SpecificationDefinition.Private, storageField,
                              writer.GetGenericName(TypeExtensions.GetShortName(typeof(EntityRef<>)),
                                                    targetTable.Type.Name));

            var storageAttribute = NewAttributeDefinition<AssociationAttribute>();
			storageAttribute["Storage"] = storageField;
			storageAttribute["OtherKey"] = parent.OtherKey;
            storageAttribute["ThisKey"] = parent.ThisKey;
            storageAttribute["Name"] = parent.Name;
            storageAttribute["IsForeignKey"] = parent.IsForeignKey;

            SpecificationDefinition specifications;
            if (parent.AccessModifierSpecified)
                specifications = GetSpecificationDefinition(parent.AccessModifier);
            else
                specifications = SpecificationDefinition.Public;
            if (parent.ModifierSpecified)
                specifications |= GetSpecificationDefinition(parent.Modifier);

            var propertyName = hasDuplicates
                                   ? member + "_" + string.Join("", parent.TheseKeys.ToArray())
                                   : member;

            using (writer.WriteAttribute(storageAttribute))
            using (writer.WriteAttribute(NewAttributeDefinition<DebuggerNonUserCodeAttribute>()))
            using (writer.WriteProperty(specifications, propertyName, targetTable.Type.Name))
            {
                string storage = writer.GetMemberExpression(storageField, "Entity");
                using (writer.WritePropertyGet())
                {
                    writer.WriteLine(writer.GetReturnStatement(storage));
                }
                using (writer.WritePropertySet())
                {
                    // algorithm is:
                    // 1.1. must be different than previous value
                    // 1.2. or HasLoadedOrAssignedValue is false (but why?)
                    // 2. implementations before change
                    // 3. if previous value not null
                    // 3.1. place parent in temp variable
                    // 3.2. set [Storage].Entity to null
                    // 3.3. remove it from parent list
                    // 4. assign value to [Storage].Entity
                    // 5. if value is not null
                    // 5.1. add it to parent list
                    // 5.2. set FK members with entity keys
                    // 6. else
                    // 6.1. set FK members to defaults (null or 0)
                    // 7. implementationas after change

                    //writer.WriteLine(writer.GetStatement(writer.GetAssignmentExpression(storage, writer.GetPropertySetValueExpression())));
                    var entityMember = writer.GetMemberExpression(parent.Storage, "Entity");
                    // 1.1
                    using (writer.WriteIf(writer.GetDifferentExpression(writer.GetPropertySetValueExpression(),
                                                                        entityMember)))
                    {
                        var otherAssociation = schema.GetReverseAssociation(parent);
                        // 2. code before the change
                        // TODO change interface to require a member instead of a column
                        //foreach (IImplementation implementation in context.Implementations())
                        //    implementation.WritePropertyBeforeSet(writer, ???, context);
                        // 3.
                        using (writer.WriteIf(writer.GetDifferentExpression(entityMember, writer.GetNullExpression())))
                        {
                            var previousEntityRefName = "previous" + parent.Type;
                            // 3.1.
                            writer.WriteLine(writer.GetStatement(
                                writer.GetVariableDeclarationInitialization(parent.Type, previousEntityRefName, entityMember)
                                ));
                            // 3.2.
                            writer.WriteLine(writer.GetStatement(
                                writer.GetAssignmentExpression(entityMember, writer.GetNullExpression())
                                ));
                            // 3.3.
                            writer.WriteLine(writer.GetStatement(
                                writer.GetMethodCallExpression(
                                    writer.GetMemberExpression(writer.GetMemberExpression(previousEntityRefName, otherAssociation.Member), "Remove"),
                                    writer.GetThisExpression())
                                ));
                        }
                        // 4.
                        writer.WriteLine(writer.GetStatement(
                            writer.GetAssignmentExpression(entityMember, writer.GetPropertySetValueExpression())
                            ));

                        // 5. if value is null or not
                        writer.WriteRawIf(writer.GetDifferentExpression(writer.GetPropertySetValueExpression(), writer.GetNullExpression()));
                        // 5.1.
                        writer.WriteLine(writer.GetStatement(
                            writer.GetMethodCallExpression(
                                writer.GetMemberExpression(writer.GetMemberExpression(writer.GetPropertySetValueExpression(), otherAssociation.Member), "Add"),
                                writer.GetThisExpression())
                            ));

                        // 5.2
                        var table = schema.Tables.Single(t => t.Type.Associations.Contains(parent));
                        var childKeys = parent.TheseKeys.ToArray();
                        var childColumns = (from ck in childKeys select table.Type.Columns.Single(c => c.Member == ck))
                                            .ToArray();
                        var parentKeys = parent.OtherKeys.ToArray();

                        for (int keyIndex = 0; keyIndex < parentKeys.Length; keyIndex++)
                        {
                            writer.WriteLine(writer.GetStatement(writer.GetAssignmentExpression(
                                childColumns[keyIndex].Storage ?? childColumns[keyIndex].Member,
                                writer.GetMemberExpression(writer.GetPropertySetValueExpression(), parentKeys[keyIndex])
                                )));
                        }

                        // 6.
                        writer.WriteRawElse();

                        // 6.1.
                        for (int keyIndex = 0; keyIndex < parentKeys.Length; keyIndex++)
                        {
                            var column = table.Type.Columns.Single(c => c.Member == childKeys[keyIndex]);
                            var columnType = System.Type.GetType(column.Type);
                            var columnLiteralType = columnType != null ? writer.GetLiteralType(columnType) : column.Type;
                            writer.WriteLine(writer.GetStatement(writer.GetAssignmentExpression(
                                childColumns[keyIndex].Storage ?? childColumns[keyIndex].Member,
                                column.CanBeNull ? writer.GetNullExpression() : writer.GetNullValueExpression(columnLiteralType)
                                )));
                        }

                        writer.WriteRawEndif();

                        // 7. code after change
                        // TODO change interface to require a member instead of a column
                        //foreach (IImplementation implementation in context.Implementations())
                        //    implementation.WritePropertyAfterSet(writer, ???, context);

                    }
                }
            }
            writer.WriteLine();
        }

        /// <summary>
        /// Returns event method name related to a child
        /// </summary>
        /// <param name="child"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        protected virtual string GetChildMethodName(Association child, string method)
        {
            return string.Format("{0}_{1}", child.Member, method);
        }

        /// <summary>
        /// Returns child attach method name
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        protected virtual string GetChildAttachMethodName(Association child)
        {
            return GetChildMethodName(child, "Attach");
        }

        /// <summary>
        /// Returns child detach method name
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        protected virtual string GetChildDetachMethodName(Association child)
        {
            return GetChildMethodName(child, "Detach");
        }

        /// <summary>
        /// Writes attach/detach method
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="table"></param>
        /// <param name="schema"></param>
        /// <param name="context"></param>
        protected virtual void WriteClassChildrenAttachment(CodeWriter writer, Table table, Database schema, GenerationContext context)
        {
            var children = GetClassChildren(table).ToList();
            if (children.Count > 0)
            {
                using (writer.WriteRegion("Attachement handlers"))
                {
                    foreach (var child in children)
                    {
                        // the reverse child is the association seen from the child
                        // we're going to use it...
                        var reverseChild = schema.GetReverseAssociation(child);
                        // ... to get the parent name
                        var memberName = reverseChild.Member;
                        var entityParameter = new ParameterDefinition { Name = "entity", LiteralType = child.Type };
                        // the Attach event handler sets the child entity parent to "this"
                        using (writer.WriteMethod(SpecificationDefinition.Private, GetChildAttachMethodName(child),
                                                  null, entityParameter))
                        {
                            writer.WriteLine(
                                writer.GetStatement(
                                    writer.GetAssignmentExpression(
                                        writer.GetMemberExpression(entityParameter.Name, memberName),
                                        writer.GetThisExpression())));
                        }
                        writer.WriteLine();
                        // the Detach event handler sets the child entity parent to null
                        using (writer.WriteMethod(SpecificationDefinition.Private, GetChildDetachMethodName(child),
                                                  null, entityParameter))
                        {
                            writer.WriteLine(
                                writer.GetStatement(
                                    writer.GetAssignmentExpression(
                                        writer.GetMemberExpression(entityParameter.Name, memberName),
                                        writer.GetNullExpression())));
                        }
                        writer.WriteLine();
                    }
                }
            }
        }

        /// <summary>
        /// Writes class ctor.
        /// EntitySet initializations
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="table"></param>
        /// <param name="schema"></param>
        /// <param name="context"></param>
        protected virtual void WriteClassCtor(CodeWriter writer, Table table, Database schema, GenerationContext context)
        {
            using (writer.WriteRegion("ctor"))
            using (writer.WriteCtor(SpecificationDefinition.Public, table.Type.Name, new ParameterDefinition[0], null))
            {
                // children are EntitySet
                foreach (var child in GetClassChildren(table))
                {
                    // if the association has a storage, we use it. Otherwise, we use the property name
                    var entitySetMember = child.Storage ?? child.Member;
                    writer.WriteLine(writer.GetStatement(
                        writer.GetAssignmentExpression(
                            entitySetMember,
                            writer.GetNewExpression(writer.GetMethodCallExpression(
                                writer.GetGenericName(TypeExtensions.GetShortName(typeof(EntitySet<>)), child.Type),
                                GetChildAttachMethodName(child),
                                GetChildDetachMethodName(child)
                            ))
                        )
                        ));
                }
                // the parents are the entities referenced by a FK. So a "parent" is an EntityRef
                foreach (var parent in GetClassParents(table))
                {
                    var entityRefMember = parent.Storage;
                    writer.WriteLine(writer.GetStatement(
                        writer.GetAssignmentExpression(
                            entityRefMember,
                            writer.GetNewExpression(writer.GetMethodCallExpression(
                            writer.GetGenericName(TypeExtensions.GetShortName(typeof(EntityRef<>)), parent.Type)
                            ))
                        )
                    ));
                }
                writer.WriteLine(writer.GetStatement(writer.GetMethodCallExpression("OnCreated")));
            }
        }
    }
}
