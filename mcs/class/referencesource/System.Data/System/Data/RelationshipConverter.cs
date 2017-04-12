//------------------------------------------------------------------------------
// <copyright file="RelationshipConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="false" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;

    sealed internal class RelationshipConverter : ExpandableObjectConverter {

        // converter classes should have public ctor
        public RelationshipConverter() {
        }


        /// <devdoc>
        ///    <para>Gets a value indicating whether this converter can
        ///       convert an object to the given destination type using the context.</para>
        /// </devdoc>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            if (destinationType == typeof(InstanceDescriptor)) {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        /// <devdoc>
        ///      Converts the given object to another type.  The most common types to convert
        ///      are to and from a string object.  The default implementation will make a call
        ///      to ToString on the object if the object is valid and if the destination
        ///      type is string.  If this cannot convert to the desitnation type, this will
        ///      throw a NotSupportedException.
        /// </devdoc>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == null) {
                throw new ArgumentNullException("destinationType");
            }

            System.Reflection.ConstructorInfo ctor = null;
            object[] values = null;

            if (destinationType == typeof(InstanceDescriptor) && value is DataRelation) {
                DataRelation rel = (DataRelation) value;
                DataTable parentTable = rel.ParentKey.Table;
                DataTable childTable = rel.ChildKey.Table;

                if (System.Data.Common.ADP.IsEmpty(parentTable.Namespace) && System.Data.Common.ADP.IsEmpty(childTable.Namespace)) {
                    ctor = typeof(DataRelation).GetConstructor(new Type[] { typeof(string) /*relationName*/, typeof(string) /*parentTableName*/, typeof(string) /*childTableName */, 
                        typeof(string[]) /*parentColumnNames */, typeof(string[])  /*childColumnNames*/, typeof(bool) /*nested*/ } );
                    
                    values = new object[] { rel.RelationName, rel.ParentKey.Table.TableName, rel.ChildKey.Table.TableName,rel.ParentColumnNames, rel.ChildColumnNames, rel.Nested };
                }
                else {
                    ctor = typeof(DataRelation).GetConstructor(new Type[] { typeof(string)/*relationName*/, typeof(string)/*parentTableName*/, typeof(string)/*parentTableNamespace*/,
                        typeof(string)/*childTableName */, typeof(string)/*childTableNamespace */, 
                        typeof(string[])/*parentColumnNames */, typeof(string[]) /*childColumnNames*/, typeof(bool) /*nested*/} );

                    values = new object[] { rel.RelationName, rel.ParentKey.Table.TableName, rel.ParentKey.Table.Namespace, rel.ChildKey.Table.TableName, 
                        rel.ChildKey.Table.Namespace, rel.ParentColumnNames, rel.ChildColumnNames, rel.Nested };
                }
             
                return new InstanceDescriptor(ctor, values);
            }
            
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

