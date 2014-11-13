//------------------------------------------------------------------------------
// <copyright file="ConstraintConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;

    /// <devdoc>
    /// </devdoc>
    sealed internal class ConstraintConverter : ExpandableObjectConverter {

        // converter classes should have public ctor
        public ConstraintConverter() {
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

            if (destinationType == typeof(InstanceDescriptor) && value is Constraint) {
                if (value is UniqueConstraint) {
                    UniqueConstraint constr = (UniqueConstraint)value;
                    System.Reflection.ConstructorInfo ctor = typeof(UniqueConstraint).GetConstructor(new Type[] { typeof(string), typeof(string[]), typeof(bool) } );
                    if (ctor != null)
                        return new InstanceDescriptor(ctor, new object[] { constr.ConstraintName, constr.ColumnNames, constr.IsPrimaryKey });
                }
                else {
                    ForeignKeyConstraint constr = (ForeignKeyConstraint)value;
                    System.Reflection.ConstructorInfo ctor = typeof(ForeignKeyConstraint).GetConstructor(new Type[] { typeof(string), typeof(string), typeof(string[]), 
                        typeof(string[]), typeof(AcceptRejectRule), typeof(Rule), typeof(Rule) } );
                    if (ctor != null)
                        return new InstanceDescriptor(ctor, new object[] { constr.ConstraintName, constr.ParentKey.Table.TableName, constr.ParentColumnNames,
                        constr.ChildColumnNames, constr.AcceptRejectRule, constr.DeleteRule, constr.UpdateRule });
                }                                        
            }
            
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
