	static partial class SR
    {
        internal static string GetResourceString(string resourceKey, string defaultString)
        {
            return defaultString;
        }

        internal static string ADP_CollectionIndexString {
              get { return SR.GetResourceString("ADP_CollectionIndexString", @"An {0} with {1} '{2}' is not contained by this {3}."); }
        }
        internal static string ADP_CollectionInvalidType {
              get { return SR.GetResourceString("ADP_CollectionInvalidType", @"The {0} only accepts non-null {1} type objects, not {2} objects."); }
        }
        internal static string ADP_CollectionIsNotParent {
              get { return SR.GetResourceString("ADP_CollectionIsNotParent", @"The {0} is already contained by another {1}."); }
        }
        internal static string ADP_CollectionNullValue {
              get { return SR.GetResourceString("ADP_CollectionNullValue", @"The {0} only accepts non-null {1} type objects."); }
        }
        internal static string ADP_CollectionRemoveInvalidObject {
              get { return SR.GetResourceString("ADP_CollectionRemoveInvalidObject", @"Attempted to remove an {0} that is not contained by this {1}."); }
        }
        internal static string ADP_CollectionUniqueValue {
              get { return SR.GetResourceString("ADP_CollectionUniqueValue", @"The {0}.{1} is required to be unique, '{2}' already exists in the collection."); }
        }
        internal static string ADP_ConnectionStateMsg_Closed {
              get { return SR.GetResourceString("ADP_ConnectionStateMsg_Closed", @"The connection's current state is closed."); }
        }
        internal static string ADP_ConnectionStateMsg_Connecting {
              get { return SR.GetResourceString("ADP_ConnectionStateMsg_Connecting", @"The connection's current state is connecting."); }
        }
        internal static string ADP_ConnectionStateMsg_Open {
              get { return SR.GetResourceString("ADP_ConnectionStateMsg_Open", @"The connection's current state is open."); }
        }
        internal static string ADP_ConnectionStateMsg_OpenExecuting {
              get { return SR.GetResourceString("ADP_ConnectionStateMsg_OpenExecuting", @"The connection's current state is executing."); }
        }
        internal static string ADP_ConnectionStateMsg_OpenFetching {
              get { return SR.GetResourceString("ADP_ConnectionStateMsg_OpenFetching", @"The connection's current state is fetching."); }
        }
        internal static string ADP_ConnectionStateMsg {
              get { return SR.GetResourceString("ADP_ConnectionStateMsg", @"The connection's current state: {0}."); }
        }
        internal static string ADP_ConnectionStringSyntax {
              get { return SR.GetResourceString("ADP_ConnectionStringSyntax", @"Format of the initialization string does not conform to specification starting at index {0}."); }
        }
        internal static string ADP_DataReaderClosed {
              get { return SR.GetResourceString("ADP_DataReaderClosed", @"Invalid attempt to call {0} when reader is closed."); }
        }
        internal static string ADP_EmptyString {
              get { return SR.GetResourceString("ADP_EmptyString", @"Expecting non-empty string for '{0}' parameter."); }
        }
        internal static string ADP_InvalidEnumerationValue {
              get { return SR.GetResourceString("ADP_InvalidEnumerationValue", @"The {0} enumeration value, {1}, is invalid."); }
        }
        internal static string ADP_InvalidKey {
              get { return SR.GetResourceString("ADP_InvalidKey", @"Invalid keyword, contain one or more of 'no characters', 'control characters', 'leading or trailing whitespace' or 'leading semicolons'."); }
        }
        internal static string ADP_InvalidValue {
              get { return SR.GetResourceString("ADP_InvalidValue", @"The value contains embedded nulls (\\u0000)."); }
        }
        internal static string ADP_InvalidXMLBadVersion {
              get { return SR.GetResourceString("ADP_InvalidXMLBadVersion", @"Invalid Xml; can only parse elements of version one."); }
        }
        internal static string Xml_SimpleTypeNotSupported {
              get { return SR.GetResourceString("Xml_SimpleTypeNotSupported", @"DataSet doesn't support 'union' or 'list' as simpleType."); }
        }
        internal static string Xml_MissingAttribute {
              get { return SR.GetResourceString("Xml_MissingAttribute", @"Invalid {0} syntax: missing required '{1}' attribute."); }
        }
        internal static string Xml_ValueOutOfRange {
              get { return SR.GetResourceString("Xml_ValueOutOfRange", @"Value '{1}' is invalid for attribute '{0}'."); }
        }
        internal static string Xml_AttributeValues {
              get { return SR.GetResourceString("Xml_AttributeValues", @"The value of attribute '{0}' should be '{1}' or '{2}'."); }
        }
        internal static string Xml_RelationParentNameMissing {
              get { return SR.GetResourceString("Xml_RelationParentNameMissing", @"Parent table name is missing in relation '{0}'."); }
        }
        internal static string Xml_RelationChildNameMissing {
              get { return SR.GetResourceString("Xml_RelationChildNameMissing", @"Child table name is missing in relation '{0}'."); }
        }
        internal static string Xml_RelationTableKeyMissing {
              get { return SR.GetResourceString("Xml_RelationTableKeyMissing", @"Parent table key is missing in relation '{0}'."); }
        }
        internal static string Xml_RelationChildKeyMissing {
              get { return SR.GetResourceString("Xml_RelationChildKeyMissing", @"Child table key is missing in relation '{0}'."); }
        }
        internal static string Xml_UndefinedDatatype {
              get { return SR.GetResourceString("Xml_UndefinedDatatype", @"Undefined data type: '{0}'."); }
        }
        internal static string Xml_DatatypeNotDefined {
              get { return SR.GetResourceString("Xml_DatatypeNotDefined", @"Data type not defined."); }
        }
        internal static string Xml_InvalidField {
              get { return SR.GetResourceString("Xml_InvalidField", @"Invalid XPath selection inside field node. Cannot find: {0}."); }
        }
        internal static string Xml_InvalidSelector {
              get { return SR.GetResourceString("Xml_InvalidSelector", @"Invalid XPath selection inside selector node: {0}."); }
        }
        internal static string Xml_InvalidKey {
              get { return SR.GetResourceString("Xml_InvalidKey", @"Invalid 'Key' node inside constraint named: {0}."); }
        }
        internal static string Xml_DuplicateConstraint {
              get { return SR.GetResourceString("Xml_DuplicateConstraint", @"The constraint name {0} is already used in the schema."); }
        }
        internal static string Xml_CannotConvert {
              get { return SR.GetResourceString("Xml_CannotConvert", @" Cannot convert '{0}' to type '{1}'."); }
        }
        internal static string Xml_MissingRefer {
              get { return SR.GetResourceString("Xml_MissingRefer", @"Missing '{0}' part in '{1}' constraint named '{2}'."); }
        }
        internal static string Xml_MismatchKeyLength {
              get { return SR.GetResourceString("Xml_MismatchKeyLength", @"Invalid Relation definition: different length keys."); }
        }
        internal static string Xml_CircularComplexType {
              get { return SR.GetResourceString("Xml_CircularComplexType", @"DataSet doesn't allow the circular reference in the ComplexType named '{0}'."); }
        }
        internal static string Xml_CannotInstantiateAbstract {
              get { return SR.GetResourceString("Xml_CannotInstantiateAbstract", @"DataSet cannot instantiate an abstract ComplexType for the node {0}."); }
        }
        internal static string Xml_MultipleTargetConverterError {
              get { return SR.GetResourceString("Xml_MultipleTargetConverterError", @"An error occurred with the multiple target converter while writing an Xml Schema.  See the inner exception for details."); }
        }
        internal static string Xml_MultipleTargetConverterEmpty {
              get { return SR.GetResourceString("Xml_MultipleTargetConverterEmpty", @"An error occurred with the multiple target converter while writing an Xml Schema.  A null or empty string was returned."); }
        }
        internal static string Xml_MergeDuplicateDeclaration {
              get { return SR.GetResourceString("Xml_MergeDuplicateDeclaration", @"Duplicated declaration '{0}'."); }
        }
        internal static string Xml_MissingTable {
              get { return SR.GetResourceString("Xml_MissingTable", @"Cannot load diffGram. Table '{0}' is missing in the destination dataset."); }
        }
        internal static string Xml_MissingSQL {
              get { return SR.GetResourceString("Xml_MissingSQL", @"Cannot load diffGram. The 'sql' node is missing."); }
        }
        internal static string Xml_ColumnConflict {
              get { return SR.GetResourceString("Xml_ColumnConflict", @"Column name '{0}' is defined for different mapping types."); }
        }
        internal static string Xml_InvalidPrefix {
              get { return SR.GetResourceString("Xml_InvalidPrefix", @"Prefix '{0}' is not valid, because it contains special characters."); }
        }
        internal static string Xml_NestedCircular {
              get { return SR.GetResourceString("Xml_NestedCircular", @"Circular reference in self-nested table '{0}'."); }
        }
        internal static string Xml_FoundEntity {
              get { return SR.GetResourceString("Xml_FoundEntity", @"DataSet cannot expand entities. Use XmlValidatingReader and set the EntityHandling property accordingly."); }
        }
        internal static string Xml_PolymorphismNotSupported {
              get { return SR.GetResourceString("Xml_PolymorphismNotSupported", @"Type '{0}' does not implement IXmlSerializable interface therefore can not proceed with serialization."); }
        }
        internal static string Xml_CanNotDeserializeObjectType {
              get { return SR.GetResourceString("Xml_CanNotDeserializeObjectType", @"Unable to proceed with deserialization. Data does not implement IXMLSerializable, therefore polymorphism is not supported."); }
        }
        internal static string Xml_DataTableInferenceNotSupported {
              get { return SR.GetResourceString("Xml_DataTableInferenceNotSupported", @"DataTable does not support schema inference from Xml."); }
        }
        internal static string Xml_MultipleParentRows {
              get { return SR.GetResourceString("Xml_MultipleParentRows", @"Cannot proceed with serializing DataTable '{0}'. It contains a DataRow which has multiple parent rows on the same Foreign Key."); }
        }
        internal static string Xml_IsDataSetAttributeMissingInSchema {
              get { return SR.GetResourceString("Xml_IsDataSetAttributeMissingInSchema", @"IsDataSet attribute is missing in input Schema."); }
        }
        internal static string Xml_TooManyIsDataSetAtributeInSchema {
              get { return SR.GetResourceString("Xml_TooManyIsDataSetAtributeInSchema", @"Cannot determine the DataSet Element. IsDataSet attribute exist more than once."); }
        }
        internal static string Xml_DynamicWithoutXmlSerializable {
              get { return SR.GetResourceString("Xml_DynamicWithoutXmlSerializable", @"DataSet will not serialize types that implement IDynamicMetaObjectProvider but do not also implement IXmlSerializable."); }
        }
        internal static string Expr_NYI {
              get { return SR.GetResourceString("Expr_NYI", @"The feature not implemented. {0}."); }
        }
        internal static string Expr_MissingOperand {
              get { return SR.GetResourceString("Expr_MissingOperand", @"Syntax error: Missing operand after '{0}' operator."); }
        }
        internal static string Expr_TypeMismatch {
              get { return SR.GetResourceString("Expr_TypeMismatch", @"Type mismatch in expression '{0}'."); }
        }
        internal static string Expr_ExpressionTooComplex {
              get { return SR.GetResourceString("Expr_ExpressionTooComplex", @"Expression is too complex."); }
        }
        internal static string Expr_UnboundName {
              get { return SR.GetResourceString("Expr_UnboundName", @"Cannot find column [{0}]."); }
        }
        internal static string Expr_InvalidString {
              get { return SR.GetResourceString("Expr_InvalidString", @"The expression contains an invalid string constant: {0}."); }
        }
        internal static string Expr_UndefinedFunction {
              get { return SR.GetResourceString("Expr_UndefinedFunction", @"The expression contains undefined function call {0}()."); }
        }
        internal static string Expr_Syntax {
              get { return SR.GetResourceString("Expr_Syntax", @"Syntax error in the expression."); }
        }
        internal static string Expr_FunctionArgumentCount {
              get { return SR.GetResourceString("Expr_FunctionArgumentCount", @"Invalid number of arguments: function {0}()."); }
        }
        internal static string Expr_MissingRightParen {
              get { return SR.GetResourceString("Expr_MissingRightParen", @"The expression is missing the closing parenthesis."); }
        }
        internal static string Expr_UnknownToken {
              get { return SR.GetResourceString("Expr_UnknownToken", @"Cannot interpret token '{0}' at position {1}."); }
        }
        internal static string Expr_UnknownToken1 {
              get { return SR.GetResourceString("Expr_UnknownToken1", @"Expected {0}, but actual token at the position {2} is {1}."); }
        }
        internal static string Expr_DatatypeConvertion {
              get { return SR.GetResourceString("Expr_DatatypeConvertion", @"Cannot convert from {0} to {1}."); }
        }
        internal static string Expr_DatavalueConvertion {
              get { return SR.GetResourceString("Expr_DatavalueConvertion", @"Cannot convert value '{0}' to Type: {1}."); }
        }
        internal static string Expr_InvalidName {
              get { return SR.GetResourceString("Expr_InvalidName", @"Invalid column name [{0}]."); }
        }
        internal static string Expr_InvalidDate {
              get { return SR.GetResourceString("Expr_InvalidDate", @"The expression contains invalid date constant '{0}'."); }
        }
        internal static string Expr_NonConstantArgument {
              get { return SR.GetResourceString("Expr_NonConstantArgument", @"Only constant expressions are allowed in the expression list for the IN operator."); }
        }
        internal static string Expr_InvalidPattern {
              get { return SR.GetResourceString("Expr_InvalidPattern", @"Error in Like operator: the string pattern '{0}' is invalid."); }
        }
        internal static string Expr_InWithoutParentheses {
              get { return SR.GetResourceString("Expr_InWithoutParentheses", @"Syntax error: The items following the IN keyword must be separated by commas and be enclosed in parentheses."); }
        }
        internal static string Expr_ArgumentType {
              get { return SR.GetResourceString("Expr_ArgumentType", @"Type mismatch in function argument: {0}(), argument {1}, expected {2}."); }
        }
        internal static string Expr_ArgumentTypeInteger {
              get { return SR.GetResourceString("Expr_ArgumentTypeInteger", @"Type mismatch in function argument: {0}(), argument {1}, expected one of the Integer types."); }
        }
        internal static string Expr_TypeMismatchInBinop {
              get { return SR.GetResourceString("Expr_TypeMismatchInBinop", @"Cannot perform '{0}' operation on {1} and {2}."); }
        }
        internal static string Expr_AmbiguousBinop {
              get { return SR.GetResourceString("Expr_AmbiguousBinop", @"Operator '{0}' is ambiguous on operands of type '{1}' and '{2}'. Cannot mix signed and unsigned types. Please use explicit Convert() function."); }
        }
        internal static string Expr_InWithoutList {
              get { return SR.GetResourceString("Expr_InWithoutList", @"Syntax error: The IN keyword must be followed by a non-empty list of expressions separated by commas, and also must be enclosed in parentheses."); }
        }
        internal static string Expr_UnsupportedOperator {
              get { return SR.GetResourceString("Expr_UnsupportedOperator", @"The expression contains unsupported operator '{0}'."); }
        }
        internal static string Expr_InvalidNameBracketing {
              get { return SR.GetResourceString("Expr_InvalidNameBracketing", @"The expression contains invalid name: '{0}'."); }
        }
        internal static string Expr_MissingOperandBefore {
              get { return SR.GetResourceString("Expr_MissingOperandBefore", @"Syntax error: Missing operand before '{0}' operator."); }
        }
        internal static string Expr_TooManyRightParentheses {
              get { return SR.GetResourceString("Expr_TooManyRightParentheses", @"The expression has too many closing parentheses."); }
        }
        internal static string Expr_UnresolvedRelation {
              get { return SR.GetResourceString("Expr_UnresolvedRelation", @"The table [{0}] involved in more than one relation. You must explicitly mention a relation name in the expression '{1}'."); }
        }
        internal static string Expr_AggregateArgument {
              get { return SR.GetResourceString("Expr_AggregateArgument", @"Syntax error in aggregate argument: Expecting a single column argument with possible 'Child' qualifier."); }
        }
        internal static string Expr_AggregateUnbound {
              get { return SR.GetResourceString("Expr_AggregateUnbound", @"Unbound reference in the aggregate expression '{0}'."); }
        }
        internal static string Expr_EvalNoContext {
              get { return SR.GetResourceString("Expr_EvalNoContext", @"Cannot evaluate non-constant expression without current row."); }
        }
        internal static string Expr_ExpressionUnbound {
              get { return SR.GetResourceString("Expr_ExpressionUnbound", @"Unbound reference in the expression '{0}'."); }
        }
        internal static string Expr_ComputeNotAggregate {
              get { return SR.GetResourceString("Expr_ComputeNotAggregate", @"Cannot evaluate. Expression '{0}' is not an aggregate."); }
        }
        internal static string Expr_FilterConvertion {
              get { return SR.GetResourceString("Expr_FilterConvertion", @"Filter expression '{0}' does not evaluate to a Boolean term."); }
        }
        internal static string Expr_InvalidType {
              get { return SR.GetResourceString("Expr_InvalidType", @"Invalid type name '{0}'."); }
        }
        internal static string Expr_LookupArgument {
              get { return SR.GetResourceString("Expr_LookupArgument", @"Syntax error in Lookup expression: Expecting keyword 'Parent' followed by a single column argument with possible relation qualifier: Parent[(<relation_name>)].<column_name>."); }
        }
        internal static string Expr_InvokeArgument {
              get { return SR.GetResourceString("Expr_InvokeArgument", @"Need a row or a table to Invoke DataFilter."); }
        }
        internal static string Expr_ArgumentOutofRange {
              get { return SR.GetResourceString("Expr_ArgumentOutofRange", @"{0}() argument is out of range."); }
        }
        internal static string Expr_IsSyntax {
              get { return SR.GetResourceString("Expr_IsSyntax", @"Syntax error: Invalid usage of 'Is' operator. Correct syntax: <expression> Is [Not] Null."); }
        }
        internal static string Expr_Overflow {
              get { return SR.GetResourceString("Expr_Overflow", @"Value is either too large or too small for Type '{0}'."); }
        }
        internal static string Expr_BindFailure {
              get { return SR.GetResourceString("Expr_BindFailure", @"Cannot find the parent relation '{0}'."); }
        }
        internal static string Expr_InvalidHoursArgument {
              get { return SR.GetResourceString("Expr_InvalidHoursArgument", @"'hours' argument is out of range. Value must be between -14 and +14."); }
        }
        internal static string Expr_InvalidMinutesArgument {
              get { return SR.GetResourceString("Expr_InvalidMinutesArgument", @"'minutes' argument is out of range. Value must be between -59 and +59."); }
        }
        internal static string Expr_InvalidTimeZoneRange {
              get { return SR.GetResourceString("Expr_InvalidTimeZoneRange", @"Provided range for time one exceeds total of 14 hours."); }
        }
        internal static string Expr_MismatchKindandTimeSpan {
              get { return SR.GetResourceString("Expr_MismatchKindandTimeSpan", @"Kind property of provided DateTime argument, does not match 'hours' and 'minutes' arguments."); }
        }
        internal static string Expr_UnsupportedType {
              get { return SR.GetResourceString("Expr_UnsupportedType", @"A DataColumn of type '{0}' does not support expression."); }
        }
        internal static string Data_EnforceConstraints {
              get { return SR.GetResourceString("Data_EnforceConstraints", @"Failed to enable constraints. One or more rows contain values violating non-null, unique, or foreign-key constraints."); }
        }
        internal static string Data_CannotModifyCollection {
              get { return SR.GetResourceString("Data_CannotModifyCollection", @"Collection itself is not modifiable."); }
        }
        internal static string Data_CaseInsensitiveNameConflict {
              get { return SR.GetResourceString("Data_CaseInsensitiveNameConflict", @"The given name '{0}' matches at least two names in the collection object with different cases, but does not match either of them with the same case."); }
        }
        internal static string Data_NamespaceNameConflict {
              get { return SR.GetResourceString("Data_NamespaceNameConflict", @"The given name '{0}' matches at least two names in the collection object with different namespaces."); }
        }
        internal static string Data_InvalidOffsetLength {
              get { return SR.GetResourceString("Data_InvalidOffsetLength", @"Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."); }
        }
        internal static string Data_ArgumentOutOfRange {
              get { return SR.GetResourceString("Data_ArgumentOutOfRange", @"'{0}' argument is out of range."); }
        }
        internal static string Data_ArgumentNull {
              get { return SR.GetResourceString("Data_ArgumentNull", @"'{0}' argument cannot be null."); }
        }
        internal static string Data_ArgumentContainsNull {
              get { return SR.GetResourceString("Data_ArgumentContainsNull", @"'{0}' argument contains null value."); }
        }
        internal static string DataColumns_OutOfRange {
              get { return SR.GetResourceString("DataColumns_OutOfRange", @"Cannot find column {0}."); }
        }
        internal static string DataColumns_Add1 {
              get { return SR.GetResourceString("DataColumns_Add1", @"Column '{0}' already belongs to this DataTable."); }
        }
        internal static string DataColumns_Add2 {
              get { return SR.GetResourceString("DataColumns_Add2", @"Column '{0}' already belongs to another DataTable."); }
        }
        internal static string DataColumns_Add3 {
              get { return SR.GetResourceString("DataColumns_Add3", @"Cannot have more than one SimpleContent columns in a DataTable."); }
        }
        internal static string DataColumns_Add4 {
              get { return SR.GetResourceString("DataColumns_Add4", @"Cannot add a SimpleContent column to a table containing element columns or nested relations."); }
        }
        internal static string DataColumns_AddDuplicate {
              get { return SR.GetResourceString("DataColumns_AddDuplicate", @"A column named '{0}' already belongs to this DataTable."); }
        }
        internal static string DataColumns_AddDuplicate2 {
              get { return SR.GetResourceString("DataColumns_AddDuplicate2", @"Cannot add a column named '{0}': a nested table with the same name already belongs to this DataTable."); }
        }
        internal static string DataColumns_AddDuplicate3 {
              get { return SR.GetResourceString("DataColumns_AddDuplicate3", @"A column named '{0}' already belongs to this DataTable: cannot set a nested table name to the same name."); }
        }
        internal static string DataColumns_Remove {
              get { return SR.GetResourceString("DataColumns_Remove", @"Cannot remove a column that doesn't belong to this table."); }
        }
        internal static string DataColumns_RemovePrimaryKey {
              get { return SR.GetResourceString("DataColumns_RemovePrimaryKey", @"Cannot remove this column, because it's part of the primary key."); }
        }
        internal static string DataColumns_RemoveChildKey {
              get { return SR.GetResourceString("DataColumns_RemoveChildKey", @"Cannot remove this column, because it is part of the parent key for relationship {0}."); }
        }
        internal static string DataColumns_RemoveConstraint {
              get { return SR.GetResourceString("DataColumns_RemoveConstraint", @"Cannot remove this column, because it is a part of the constraint {0} on the table {1}."); }
        }
        internal static string DataColumn_AutoIncrementAndExpression {
              get { return SR.GetResourceString("DataColumn_AutoIncrementAndExpression", @"Cannot set AutoIncrement property for a computed column."); }
        }
        internal static string DataColumn_AutoIncrementAndDefaultValue {
              get { return SR.GetResourceString("DataColumn_AutoIncrementAndDefaultValue", @"Cannot set AutoIncrement property for a column with DefaultValue set."); }
        }
        internal static string DataColumn_DefaultValueAndAutoIncrement {
              get { return SR.GetResourceString("DataColumn_DefaultValueAndAutoIncrement", @"Cannot set a DefaultValue on an AutoIncrement column."); }
        }
        internal static string DataColumn_AutoIncrementSeed {
              get { return SR.GetResourceString("DataColumn_AutoIncrementSeed", @"AutoIncrementStep must be a non-zero value."); }
        }
        internal static string DataColumn_NameRequired {
              get { return SR.GetResourceString("DataColumn_NameRequired", @"ColumnName is required when it is part of a DataTable."); }
        }
        internal static string DataColumn_ChangeDataType {
              get { return SR.GetResourceString("DataColumn_ChangeDataType", @"Cannot change DataType of a column once it has data."); }
        }
        internal static string DataColumn_NullDataType {
              get { return SR.GetResourceString("DataColumn_NullDataType", @"Column requires a valid DataType."); }
        }
        internal static string DataColumn_DefaultValueDataType {
              get { return SR.GetResourceString("DataColumn_DefaultValueDataType", @"The DefaultValue for column {0} is of type {1} and cannot be converted to {2}."); }
        }
        internal static string DataColumn_DefaultValueDataType1 {
              get { return SR.GetResourceString("DataColumn_DefaultValueDataType1", @"The DefaultValue for the column is of type {0} and cannot be converted to {1}."); }
        }
        internal static string DataColumn_DefaultValueColumnDataType {
              get { return SR.GetResourceString("DataColumn_DefaultValueColumnDataType", @"The DefaultValue for column {0} is of type {1}, but the column is of type {2}."); }
        }
        internal static string DataColumn_ReadOnlyAndExpression {
              get { return SR.GetResourceString("DataColumn_ReadOnlyAndExpression", @"Cannot change ReadOnly property for the expression column."); }
        }
        internal static string DataColumn_UniqueAndExpression {
              get { return SR.GetResourceString("DataColumn_UniqueAndExpression", @"Cannot change Unique property for the expression column."); }
        }
        internal static string DataColumn_ExpressionAndUnique {
              get { return SR.GetResourceString("DataColumn_ExpressionAndUnique", @"Cannot create an expression on a column that has AutoIncrement or Unique."); }
        }
        internal static string DataColumn_ExpressionAndReadOnly {
              get { return SR.GetResourceString("DataColumn_ExpressionAndReadOnly", @"Cannot set expression because column cannot be made ReadOnly."); }
        }
        internal static string DataColumn_ExpressionAndConstraint {
              get { return SR.GetResourceString("DataColumn_ExpressionAndConstraint", @"Cannot set Expression property on column {0}, because it is a part of a constraint."); }
        }
        internal static string DataColumn_ExpressionInConstraint {
              get { return SR.GetResourceString("DataColumn_ExpressionInConstraint", @"Cannot create a constraint based on Expression column {0}."); }
        }
        internal static string DataColumn_ExpressionCircular {
              get { return SR.GetResourceString("DataColumn_ExpressionCircular", @"Cannot set Expression property due to circular reference in the expression."); }
        }
        internal static string DataColumn_NullKeyValues {
              get { return SR.GetResourceString("DataColumn_NullKeyValues", @"Column '{0}' has null values in it."); }
        }
        internal static string DataColumn_NullValues {
              get { return SR.GetResourceString("DataColumn_NullValues", @"Column '{0}' does not allow nulls."); }
        }
        internal static string DataColumn_ReadOnly {
              get { return SR.GetResourceString("DataColumn_ReadOnly", @"Column '{0}' is read only."); }
        }
        internal static string DataColumn_NonUniqueValues {
              get { return SR.GetResourceString("DataColumn_NonUniqueValues", @"Column '{0}' contains non-unique values."); }
        }
        internal static string DataColumn_NotInTheTable {
              get { return SR.GetResourceString("DataColumn_NotInTheTable", @"Column '{0}' does not belong to table {1}."); }
        }
        internal static string DataColumn_NotInAnyTable {
              get { return SR.GetResourceString("DataColumn_NotInAnyTable", @"Column must belong to a table."); }
        }
        internal static string DataColumn_SetFailed {
              get { return SR.GetResourceString("DataColumn_SetFailed", @"Couldn't store <{0}> in {1} Column.  Expected type is {2}."); }
        }
        internal static string DataColumn_CannotSetToNull {
              get { return SR.GetResourceString("DataColumn_CannotSetToNull", @"Cannot set Column '{0}' to be null. Please use DBNull instead."); }
        }
        internal static string DataColumn_LongerThanMaxLength {
              get { return SR.GetResourceString("DataColumn_LongerThanMaxLength", @"Cannot set column '{0}'. The value violates the MaxLength limit of this column."); }
        }
        internal static string DataColumn_HasToBeStringType {
              get { return SR.GetResourceString("DataColumn_HasToBeStringType", @"MaxLength applies to string data type only. You cannot set Column '{0}' property MaxLength to be non-negative number."); }
        }
        internal static string DataColumn_CannotSetMaxLength {
              get { return SR.GetResourceString("DataColumn_CannotSetMaxLength", @"Cannot set Column '{0}' property MaxLength to '{1}'. There is at least one string in the table longer than the new limit."); }
        }
        internal static string DataColumn_CannotSetMaxLength2 {
              get { return SR.GetResourceString("DataColumn_CannotSetMaxLength2", @"Cannot set Column '{0}' property MaxLength. The Column is SimpleContent."); }
        }
        internal static string DataColumn_CannotSimpleContentType {
              get { return SR.GetResourceString("DataColumn_CannotSimpleContentType", @"Cannot set Column '{0}' property DataType to {1}. The Column is SimpleContent."); }
        }
        internal static string DataColumn_CannotSimpleContent {
              get { return SR.GetResourceString("DataColumn_CannotSimpleContent", @"Cannot set Column '{0}' property MappingType to SimpleContent. The Column DataType is {1}."); }
        }
        internal static string DataColumn_ExceedMaxLength {
              get { return SR.GetResourceString("DataColumn_ExceedMaxLength", @"Column '{0}' exceeds the MaxLength limit."); }
        }
        internal static string DataColumn_NotAllowDBNull {
              get { return SR.GetResourceString("DataColumn_NotAllowDBNull", @"Column '{0}' does not allow DBNull.Value."); }
        }
        internal static string DataColumn_CannotChangeNamespace {
              get { return SR.GetResourceString("DataColumn_CannotChangeNamespace", @"Cannot change the Column '{0}' property Namespace. The Column is SimpleContent."); }
        }
        internal static string DataColumn_AutoIncrementCannotSetIfHasData {
              get { return SR.GetResourceString("DataColumn_AutoIncrementCannotSetIfHasData", @"Cannot change AutoIncrement of a DataColumn with type '{0}' once it has data."); }
        }
        internal static string DataColumn_NotInTheUnderlyingTable {
              get { return SR.GetResourceString("DataColumn_NotInTheUnderlyingTable", @"Column '{0}' does not belong to underlying table '{1}'."); }
        }
        internal static string DataColumn_InvalidDataColumnMapping {
              get { return SR.GetResourceString("DataColumn_InvalidDataColumnMapping", @"DataColumn with type '{0}' is a complexType. Can not serialize value of a complex type as Attribute"); }
        }
        internal static string DataColumn_CannotSetDateTimeModeForNonDateTimeColumns {
              get { return SR.GetResourceString("DataColumn_CannotSetDateTimeModeForNonDateTimeColumns", @"The DateTimeMode can be set only on DataColumns of type DateTime."); }
        }
        internal static string DataColumn_DateTimeMode {
              get { return SR.GetResourceString("DataColumn_DateTimeMode", @"Cannot change DateTimeMode from '{0}' to '{1}' once the table has data."); }
        }
        internal static string DataColumn_INullableUDTwithoutStaticNull {
              get { return SR.GetResourceString("DataColumn_INullableUDTwithoutStaticNull", @"Type '{0}' does not contain static Null property or field."); }
        }
        internal static string DataColumn_UDTImplementsIChangeTrackingButnotIRevertible {
              get { return SR.GetResourceString("DataColumn_UDTImplementsIChangeTrackingButnotIRevertible", @"Type '{0}' does not implement IRevertibleChangeTracking; therefore can not proceed with RejectChanges()."); }
        }
        internal static string DataColumn_SetAddedAndModifiedCalledOnNonUnchanged {
              get { return SR.GetResourceString("DataColumn_SetAddedAndModifiedCalledOnNonUnchanged", @"SetAdded and SetModified can only be called on DataRows with Unchanged DataRowState."); }
        }
        internal static string DataColumn_OrdinalExceedMaximun {
              get { return SR.GetResourceString("DataColumn_OrdinalExceedMaximun", @"Ordinal '{0}' exceeds the maximum number."); }
        }
        internal static string DataColumn_NullableTypesNotSupported {
              get { return SR.GetResourceString("DataColumn_NullableTypesNotSupported", @"DataSet does not support System.Nullable<>."); }
        }
        internal static string DataConstraint_NoName {
              get { return SR.GetResourceString("DataConstraint_NoName", @"Cannot change the name of a constraint to empty string when it is in the ConstraintCollection."); }
        }
        internal static string DataConstraint_Violation {
              get { return SR.GetResourceString("DataConstraint_Violation", @"Cannot enforce constraints on constraint {0}."); }
        }
        internal static string DataConstraint_ViolationValue {
              get { return SR.GetResourceString("DataConstraint_ViolationValue", @"Column '{0}' is constrained to be unique.  Value '{1}' is already present."); }
        }
        internal static string DataConstraint_NotInTheTable {
              get { return SR.GetResourceString("DataConstraint_NotInTheTable", @"Constraint '{0}' does not belong to this DataTable."); }
        }
        internal static string DataConstraint_OutOfRange {
              get { return SR.GetResourceString("DataConstraint_OutOfRange", @"Cannot find constraint {0}."); }
        }
        internal static string DataConstraint_Duplicate {
              get { return SR.GetResourceString("DataConstraint_Duplicate", @"Constraint matches constraint named {0} already in collection."); }
        }
        internal static string DataConstraint_DuplicateName {
              get { return SR.GetResourceString("DataConstraint_DuplicateName", @"A Constraint named '{0}' already belongs to this DataTable."); }
        }
        internal static string DataConstraint_UniqueViolation {
              get { return SR.GetResourceString("DataConstraint_UniqueViolation", @"These columns don't currently have unique values."); }
        }
        internal static string DataConstraint_ForeignTable {
              get { return SR.GetResourceString("DataConstraint_ForeignTable", @"These columns don't point to this table."); }
        }
        internal static string DataConstraint_ParentValues {
              get { return SR.GetResourceString("DataConstraint_ParentValues", @"This constraint cannot be enabled as not all values have corresponding parent values."); }
        }
        internal static string DataConstraint_AddFailed {
              get { return SR.GetResourceString("DataConstraint_AddFailed", @"This constraint cannot be added since ForeignKey doesn't belong to table {0}."); }
        }
        internal static string DataConstraint_RemoveFailed {
              get { return SR.GetResourceString("DataConstraint_RemoveFailed", @"Cannot remove a constraint that doesn't belong to this table."); }
        }
        internal static string DataConstraint_NeededForForeignKeyConstraint {
              get { return SR.GetResourceString("DataConstraint_NeededForForeignKeyConstraint", @"Cannot remove unique constraint '{0}'. Remove foreign key constraint '{1}' first."); }
        }
        internal static string DataConstraint_CascadeDelete {
              get { return SR.GetResourceString("DataConstraint_CascadeDelete", @"Cannot delete this row because constraints are enforced on relation {0}, and deleting this row will strand child rows."); }
        }
        internal static string DataConstraint_CascadeUpdate {
              get { return SR.GetResourceString("DataConstraint_CascadeUpdate", @"Cannot make this change because constraints are enforced on relation {0}, and changing this value will strand child rows."); }
        }
        internal static string DataConstraint_ClearParentTable {
              get { return SR.GetResourceString("DataConstraint_ClearParentTable", @"Cannot clear table {0} because ForeignKeyConstraint {1} enforces constraints and there are child rows in {2}."); }
        }
        internal static string DataConstraint_ForeignKeyViolation {
              get { return SR.GetResourceString("DataConstraint_ForeignKeyViolation", @"ForeignKeyConstraint {0} requires the child key values ({1}) to exist in the parent table."); }
        }
        internal static string DataConstraint_BadObjectPropertyAccess {
              get { return SR.GetResourceString("DataConstraint_BadObjectPropertyAccess", @"Property not accessible because '{0}'."); }
        }
        internal static string DataConstraint_RemoveParentRow {
              get { return SR.GetResourceString("DataConstraint_RemoveParentRow", @"Cannot remove this row because it has child rows, and constraints on relation {0} are enforced."); }
        }
        internal static string DataConstraint_AddPrimaryKeyConstraint {
              get { return SR.GetResourceString("DataConstraint_AddPrimaryKeyConstraint", @"Cannot add primary key constraint since primary key is already set for the table."); }
        }
        internal static string DataConstraint_CantAddConstraintToMultipleNestedTable {
              get { return SR.GetResourceString("DataConstraint_CantAddConstraintToMultipleNestedTable", @"Cannot add constraint to DataTable '{0}' which is a child table in two nested relations."); }
        }
        internal static string DataKey_TableMismatch {
              get { return SR.GetResourceString("DataKey_TableMismatch", @"Cannot create a Key from Columns that belong to different tables."); }
        }
        internal static string DataKey_NoColumns {
              get { return SR.GetResourceString("DataKey_NoColumns", @"Cannot have 0 columns."); }
        }
        internal static string DataKey_TooManyColumns {
              get { return SR.GetResourceString("DataKey_TooManyColumns", @"Cannot have more than {0} columns."); }
        }
        internal static string DataKey_DuplicateColumns {
              get { return SR.GetResourceString("DataKey_DuplicateColumns", @"Cannot create a Key when the same column is listed more than once: '{0}'"); }
        }
        internal static string DataKey_RemovePrimaryKey {
              get { return SR.GetResourceString("DataKey_RemovePrimaryKey", @"Cannot remove unique constraint since it's the primary key of a table."); }
        }
        internal static string DataKey_RemovePrimaryKey1 {
              get { return SR.GetResourceString("DataKey_RemovePrimaryKey1", @"Cannot remove unique constraint since it's the primary key of table {0}."); }
        }
        internal static string DataRelation_ColumnsTypeMismatch {
              get { return SR.GetResourceString("DataRelation_ColumnsTypeMismatch", @"Parent Columns and Child Columns don't have type-matching columns."); }
        }
        internal static string DataRelation_KeyColumnsIdentical {
              get { return SR.GetResourceString("DataRelation_KeyColumnsIdentical", @"ParentKey and ChildKey are identical."); }
        }
        internal static string DataRelation_KeyLengthMismatch {
              get { return SR.GetResourceString("DataRelation_KeyLengthMismatch", @"ParentColumns and ChildColumns should be the same length."); }
        }
        internal static string DataRelation_KeyZeroLength {
              get { return SR.GetResourceString("DataRelation_KeyZeroLength", @"ParentColumns and ChildColumns must not be zero length."); }
        }
        internal static string DataRelation_ForeignRow {
              get { return SR.GetResourceString("DataRelation_ForeignRow", @"The row doesn't belong to the same DataSet as this relation."); }
        }
        internal static string DataRelation_NoName {
              get { return SR.GetResourceString("DataRelation_NoName", @"RelationName is required when it is part of a DataSet."); }
        }
        internal static string DataRelation_ForeignTable {
              get { return SR.GetResourceString("DataRelation_ForeignTable", @"GetChildRows requires a row whose Table is {0}, but the specified row's Table is {1}."); }
        }
        internal static string DataRelation_ForeignDataSet {
              get { return SR.GetResourceString("DataRelation_ForeignDataSet", @"This relation should connect two tables in this DataSet to be added to this DataSet."); }
        }
        internal static string DataRelation_GetParentRowTableMismatch {
              get { return SR.GetResourceString("DataRelation_GetParentRowTableMismatch", @"GetParentRow requires a row whose Table is {0}, but the specified row's Table is {1}."); }
        }
        internal static string DataRelation_SetParentRowTableMismatch {
              get { return SR.GetResourceString("DataRelation_SetParentRowTableMismatch", @"SetParentRow requires a child row whose Table is {0}, but the specified row's Table is {1}."); }
        }
        internal static string DataRelation_DataSetMismatch {
              get { return SR.GetResourceString("DataRelation_DataSetMismatch", @"Cannot have a relationship between tables in different DataSets."); }
        }
        internal static string DataRelation_TablesInDifferentSets {
              get { return SR.GetResourceString("DataRelation_TablesInDifferentSets", @"Cannot create a relation between tables in different DataSets."); }
        }
        internal static string DataRelation_AlreadyExists {
              get { return SR.GetResourceString("DataRelation_AlreadyExists", @"A relation already exists for these child columns."); }
        }
        internal static string DataRelation_DoesNotExist {
              get { return SR.GetResourceString("DataRelation_DoesNotExist", @"This relation doesn't belong to this relation collection."); }
        }
        internal static string DataRelation_AlreadyInOtherDataSet {
              get { return SR.GetResourceString("DataRelation_AlreadyInOtherDataSet", @"This relation already belongs to another DataSet."); }
        }
        internal static string DataRelation_AlreadyInTheDataSet {
              get { return SR.GetResourceString("DataRelation_AlreadyInTheDataSet", @"This relation already belongs to this DataSet."); }
        }
        internal static string DataRelation_DuplicateName {
              get { return SR.GetResourceString("DataRelation_DuplicateName", @"A Relation named '{0}' already belongs to this DataSet."); }
        }
        internal static string DataRelation_NotInTheDataSet {
              get { return SR.GetResourceString("DataRelation_NotInTheDataSet", @"Relation {0} does not belong to this DataSet."); }
        }
        internal static string DataRelation_OutOfRange {
              get { return SR.GetResourceString("DataRelation_OutOfRange", @"Cannot find relation {0}."); }
        }
        internal static string DataRelation_TableNull {
              get { return SR.GetResourceString("DataRelation_TableNull", @"Cannot create a collection on a null table."); }
        }
        internal static string DataRelation_TableWasRemoved {
              get { return SR.GetResourceString("DataRelation_TableWasRemoved", @"The table this collection displays relations for has been removed from its DataSet."); }
        }
        internal static string DataRelation_ChildTableMismatch {
              get { return SR.GetResourceString("DataRelation_ChildTableMismatch", @"Cannot add a relation to this table's ParentRelation collection where this table isn't the child table."); }
        }
        internal static string DataRelation_ParentTableMismatch {
              get { return SR.GetResourceString("DataRelation_ParentTableMismatch", @"Cannot add a relation to this table's ChildRelation collection where this table isn't the parent table."); }
        }
        internal static string DataRelation_RelationNestedReadOnly {
              get { return SR.GetResourceString("DataRelation_RelationNestedReadOnly", @"Cannot set the 'Nested' property to false for this relation."); }
        }
        internal static string DataRelation_TableCantBeNestedInTwoTables {
              get { return SR.GetResourceString("DataRelation_TableCantBeNestedInTwoTables", @"The same table '{0}' cannot be the child table in two nested relations."); }
        }
        internal static string DataRelation_LoopInNestedRelations {
              get { return SR.GetResourceString("DataRelation_LoopInNestedRelations", @"The table ({0}) cannot be the child table to itself in nested relations."); }
        }
        internal static string DataRelation_CaseLocaleMismatch {
              get { return SR.GetResourceString("DataRelation_CaseLocaleMismatch", @"Cannot add a DataRelation or Constraint that has different Locale or CaseSensitive settings between its parent and child tables."); }
        }
        internal static string DataRelation_ParentOrChildColumnsDoNotHaveDataSet {
              get { return SR.GetResourceString("DataRelation_ParentOrChildColumnsDoNotHaveDataSet", @"Cannot create a DataRelation if Parent or Child Columns are not in a DataSet."); }
        }
        internal static string DataRelation_InValidNestedRelation {
              get { return SR.GetResourceString("DataRelation_InValidNestedRelation", @"Nested table '{0}' which inherits its namespace cannot have multiple parent tables in different namespaces."); }
        }
        internal static string DataRelation_InValidNamespaceInNestedRelation {
              get { return SR.GetResourceString("DataRelation_InValidNamespaceInNestedRelation", @"Nested table '{0}' with empty namespace cannot have multiple parent tables in different namespaces."); }
        }
        internal static string DataRow_NotInTheDataSet {
              get { return SR.GetResourceString("DataRow_NotInTheDataSet", @"The row doesn't belong to the same DataSet as this relation."); }
        }
        internal static string DataRow_NotInTheTable {
              get { return SR.GetResourceString("DataRow_NotInTheTable", @"Cannot perform this operation on a row not in the table."); }
        }
        internal static string DataRow_ParentRowNotInTheDataSet {
              get { return SR.GetResourceString("DataRow_ParentRowNotInTheDataSet", @"This relation and child row don't belong to same DataSet."); }
        }
        internal static string DataRow_EditInRowChanging {
              get { return SR.GetResourceString("DataRow_EditInRowChanging", @"Cannot change a proposed value in the RowChanging event."); }
        }
        internal static string DataRow_EndEditInRowChanging {
              get { return SR.GetResourceString("DataRow_EndEditInRowChanging", @"Cannot call EndEdit() inside an OnRowChanging event."); }
        }
        internal static string DataRow_BeginEditInRowChanging {
              get { return SR.GetResourceString("DataRow_BeginEditInRowChanging", @"Cannot call BeginEdit() inside the RowChanging event."); }
        }
        internal static string DataRow_CancelEditInRowChanging {
              get { return SR.GetResourceString("DataRow_CancelEditInRowChanging", @"Cannot call CancelEdit() inside an OnRowChanging event.  Throw an exception to cancel this update."); }
        }
        internal static string DataRow_DeleteInRowDeleting {
              get { return SR.GetResourceString("DataRow_DeleteInRowDeleting", @"Cannot call Delete inside an OnRowDeleting event.  Throw an exception to cancel this delete."); }
        }
        internal static string DataRow_ValuesArrayLength {
              get { return SR.GetResourceString("DataRow_ValuesArrayLength", @"Input array is longer than the number of columns in this table."); }
        }
        internal static string DataRow_NoCurrentData {
              get { return SR.GetResourceString("DataRow_NoCurrentData", @"There is no Current data to access."); }
        }
        internal static string DataRow_NoOriginalData {
              get { return SR.GetResourceString("DataRow_NoOriginalData", @"There is no Original data to access."); }
        }
        internal static string DataRow_NoProposedData {
              get { return SR.GetResourceString("DataRow_NoProposedData", @"There is no Proposed data to access."); }
        }
        internal static string DataRow_RemovedFromTheTable {
              get { return SR.GetResourceString("DataRow_RemovedFromTheTable", @"This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row."); }
        }
        internal static string DataRow_DeletedRowInaccessible {
              get { return SR.GetResourceString("DataRow_DeletedRowInaccessible", @"Deleted row information cannot be accessed through the row."); }
        }
        internal static string DataRow_InvalidVersion {
              get { return SR.GetResourceString("DataRow_InvalidVersion", @"Version must be Original, Current, or Proposed."); }
        }
        internal static string DataRow_OutOfRange {
              get { return SR.GetResourceString("DataRow_OutOfRange", @"There is no row at position {0}."); }
        }
        internal static string DataRow_RowInsertOutOfRange {
              get { return SR.GetResourceString("DataRow_RowInsertOutOfRange", @"The row insert position {0} is invalid."); }
        }
        internal static string DataRow_RowInsertMissing {
              get { return SR.GetResourceString("DataRow_RowInsertMissing", @"Values are missing in the rowOrder sequence for table '{0}'."); }
        }
        internal static string DataRow_RowOutOfRange {
              get { return SR.GetResourceString("DataRow_RowOutOfRange", @"The given DataRow is not in the current DataRowCollection."); }
        }
        internal static string DataRow_AlreadyInOtherCollection {
              get { return SR.GetResourceString("DataRow_AlreadyInOtherCollection", @"This row already belongs to another table."); }
        }
        internal static string DataRow_AlreadyInTheCollection {
              get { return SR.GetResourceString("DataRow_AlreadyInTheCollection", @"This row already belongs to this table."); }
        }
        internal static string DataRow_AlreadyDeleted {
              get { return SR.GetResourceString("DataRow_AlreadyDeleted", @"Cannot delete this row since it's already deleted."); }
        }
        internal static string DataRow_Empty {
              get { return SR.GetResourceString("DataRow_Empty", @"This row is empty."); }
        }
        internal static string DataRow_AlreadyRemoved {
              get { return SR.GetResourceString("DataRow_AlreadyRemoved", @"Cannot remove a row that's already been removed."); }
        }
        internal static string DataRow_MultipleParents {
              get { return SR.GetResourceString("DataRow_MultipleParents", @"A child row has multiple parents."); }
        }
        internal static string DataRow_InvalidRowBitPattern {
              get { return SR.GetResourceString("DataRow_InvalidRowBitPattern", @"Unrecognized row state bit pattern."); }
        }
        internal static string DataSet_SetNameToEmpty {
              get { return SR.GetResourceString("DataSet_SetNameToEmpty", @"Cannot change the name of the DataSet to an empty string."); }
        }
        internal static string DataSet_SetDataSetNameConflicting {
              get { return SR.GetResourceString("DataSet_SetDataSetNameConflicting", @"The name '{0}' is invalid. A DataSet cannot have the same name of the DataTable."); }
        }
        internal static string DataSet_UnsupportedSchema {
              get { return SR.GetResourceString("DataSet_UnsupportedSchema", @"The schema namespace is invalid. Please use this one instead: {0}."); }
        }
        internal static string DataSet_CannotChangeCaseLocale {
              get { return SR.GetResourceString("DataSet_CannotChangeCaseLocale", @"Cannot change CaseSensitive or Locale property. This change would lead to at least one DataRelation or Constraint to have different Locale or CaseSensitive settings between its related tables."); }
        }
        internal static string DataSet_CannotChangeSchemaSerializationMode {
              get { return SR.GetResourceString("DataSet_CannotChangeSchemaSerializationMode", @"SchemaSerializationMode property can be set only if it is overridden by derived DataSet."); }
        }
        internal static string DataTable_ForeignPrimaryKey {
              get { return SR.GetResourceString("DataTable_ForeignPrimaryKey", @"PrimaryKey columns do not belong to this table."); }
        }
        internal static string DataTable_CannotAddToSimpleContent {
              get { return SR.GetResourceString("DataTable_CannotAddToSimpleContent", @"Cannot add a nested relation or an element column to a table containing a SimpleContent column."); }
        }
        internal static string DataTable_NoName {
              get { return SR.GetResourceString("DataTable_NoName", @"TableName is required when it is part of a DataSet."); }
        }
        internal static string DataTable_MultipleSimpleContentColumns {
              get { return SR.GetResourceString("DataTable_MultipleSimpleContentColumns", @"DataTable already has a simple content column."); }
        }
        internal static string DataTable_MissingPrimaryKey {
              get { return SR.GetResourceString("DataTable_MissingPrimaryKey", @"Table doesn't have a primary key."); }
        }
        internal static string DataTable_InvalidSortString {
              get { return SR.GetResourceString("DataTable_InvalidSortString", @" {0} isn't a valid Sort string entry."); }
        }
        internal static string DataTable_CanNotSerializeDataTableHierarchy {
              get { return SR.GetResourceString("DataTable_CanNotSerializeDataTableHierarchy", @"Cannot serialize the DataTable. A DataTable being used in one or more DataColumn expressions is not a descendant of current DataTable."); }
        }
        internal static string DataTable_CanNotRemoteDataTable {
              get { return SR.GetResourceString("DataTable_CanNotRemoteDataTable", @"This DataTable can only be remoted as part of DataSet. One or more Expression Columns has reference to other DataTable(s)."); }
        }
        internal static string DataTable_CanNotSetRemotingFormat {
              get { return SR.GetResourceString("DataTable_CanNotSetRemotingFormat", @"Cannot have different remoting format property value for DataSet and DataTable."); }
        }
        internal static string DataTable_CanNotSerializeDataTableWithEmptyName {
              get { return SR.GetResourceString("DataTable_CanNotSerializeDataTableWithEmptyName", @"Cannot serialize the DataTable. DataTable name is not set."); }
        }
        internal static string DataTable_DuplicateName {
              get { return SR.GetResourceString("DataTable_DuplicateName", @"A DataTable named '{0}' already belongs to this DataSet."); }
        }
        internal static string DataTable_DuplicateName2 {
              get { return SR.GetResourceString("DataTable_DuplicateName2", @"A DataTable named '{0}' with the same Namespace '{1}' already belongs to this DataSet."); }
        }
        internal static string DataTable_SelfnestedDatasetConflictingName {
              get { return SR.GetResourceString("DataTable_SelfnestedDatasetConflictingName", @"The table ({0}) cannot be the child table to itself in a nested relation: the DataSet name conflicts with the table name."); }
        }
        internal static string DataTable_DatasetConflictingName {
              get { return SR.GetResourceString("DataTable_DatasetConflictingName", @"The name '{0}' is invalid. A DataTable cannot have the same name of the DataSet."); }
        }
        internal static string DataTable_AlreadyInOtherDataSet {
              get { return SR.GetResourceString("DataTable_AlreadyInOtherDataSet", @"DataTable already belongs to another DataSet."); }
        }
        internal static string DataTable_AlreadyInTheDataSet {
              get { return SR.GetResourceString("DataTable_AlreadyInTheDataSet", @"DataTable already belongs to this DataSet."); }
        }
        internal static string DataTable_NotInTheDataSet {
              get { return SR.GetResourceString("DataTable_NotInTheDataSet", @"Table {0} does not belong to this DataSet."); }
        }
        internal static string DataTable_OutOfRange {
              get { return SR.GetResourceString("DataTable_OutOfRange", @"Cannot find table {0}."); }
        }
        internal static string DataTable_InRelation {
              get { return SR.GetResourceString("DataTable_InRelation", @"Cannot remove a table that has existing relations.  Remove relations first."); }
        }
        internal static string DataTable_InConstraint {
              get { return SR.GetResourceString("DataTable_InConstraint", @"Cannot remove table {0}, because it referenced in ForeignKeyConstraint {1}.  Remove the constraint first."); }
        }
        internal static string DataTable_TableNotFound {
              get { return SR.GetResourceString("DataTable_TableNotFound", @"DataTable '{0}' does not match to any DataTable in source."); }
        }
        internal static string DataMerge_MissingDefinition {
              get { return SR.GetResourceString("DataMerge_MissingDefinition", @"Target DataSet missing definition for {0}."); }
        }
        internal static string DataMerge_MissingConstraint {
              get { return SR.GetResourceString("DataMerge_MissingConstraint", @"Target DataSet missing {0} {1}."); }
        }
        internal static string DataMerge_DataTypeMismatch {
              get { return SR.GetResourceString("DataMerge_DataTypeMismatch", @"<target>.{0} and <source>.{0} have conflicting properties: DataType property mismatch."); }
        }
        internal static string DataMerge_PrimaryKeyMismatch {
              get { return SR.GetResourceString("DataMerge_PrimaryKeyMismatch", @"<target>.PrimaryKey and <source>.PrimaryKey have different Length."); }
        }
        internal static string DataMerge_PrimaryKeyColumnsMismatch {
              get { return SR.GetResourceString("DataMerge_PrimaryKeyColumnsMismatch", @"Mismatch columns in the PrimaryKey : <target>.{0} versus <source>.{1}."); }
        }
        internal static string DataMerge_ReltionKeyColumnsMismatch {
              get { return SR.GetResourceString("DataMerge_ReltionKeyColumnsMismatch", @"Relation {0} cannot be merged, because keys have mismatch columns."); }
        }
        internal static string DataMerge_MissingColumnDefinition {
              get { return SR.GetResourceString("DataMerge_MissingColumnDefinition", @"Target table {0} missing definition for column {1}."); }
        }
        internal static string DataIndex_RecordStateRange {
              get { return SR.GetResourceString("DataIndex_RecordStateRange", @"The RowStates parameter must be set to a valid combination of values from the DataViewRowState enumeration."); }
        }
        internal static string DataIndex_FindWithoutSortOrder {
              get { return SR.GetResourceString("DataIndex_FindWithoutSortOrder", @"Find finds a row based on a Sort order, and no Sort order is specified."); }
        }
        internal static string DataIndex_KeyLength {
              get { return SR.GetResourceString("DataIndex_KeyLength", @"Expecting {0} value(s) for the key being indexed, but received {1} value(s)."); }
        }
        internal static string DataStorage_AggregateException {
              get { return SR.GetResourceString("DataStorage_AggregateException", @"Invalid usage of aggregate function {0}() and Type: {1}."); }
        }
        internal static string DataStorage_InvalidStorageType {
              get { return SR.GetResourceString("DataStorage_InvalidStorageType", @"Invalid storage type: {0}."); }
        }
        internal static string DataStorage_ProblematicChars {
              get { return SR.GetResourceString("DataStorage_ProblematicChars", @"The DataSet Xml persistency does not support the value '{0}' as Char value, please use Byte storage instead."); }
        }
        internal static string DataStorage_SetInvalidDataType {
              get { return SR.GetResourceString("DataStorage_SetInvalidDataType", @"Type of value has a mismatch with column type"); }
        }
        internal static string DataStorage_IComparableNotDefined {
              get { return SR.GetResourceString("DataStorage_IComparableNotDefined", @" Type '{0}' does not implement IComparable interface. Comparison cannot be done."); }
        }
        internal static string DataView_SetFailed {
              get { return SR.GetResourceString("DataView_SetFailed", @"Cannot set {0}."); }
        }
        internal static string DataView_SetDataSetFailed {
              get { return SR.GetResourceString("DataView_SetDataSetFailed", @"Cannot change DataSet on a DataViewManager that's already the default view for a DataSet."); }
        }
        internal static string DataView_SetRowStateFilter {
              get { return SR.GetResourceString("DataView_SetRowStateFilter", @"RowStateFilter cannot show ModifiedOriginals and ModifiedCurrents at the same time."); }
        }
        internal static string DataView_SetTable {
              get { return SR.GetResourceString("DataView_SetTable", @"Cannot change Table property on a DefaultView or a DataView coming from a DataViewManager."); }
        }
        internal static string DataView_CanNotSetDataSet {
              get { return SR.GetResourceString("DataView_CanNotSetDataSet", @"Cannot change DataSet property once it is set."); }
        }
        internal static string DataView_CanNotUseDataViewManager {
              get { return SR.GetResourceString("DataView_CanNotUseDataViewManager", @"DataSet must be set prior to using DataViewManager."); }
        }
        internal static string DataView_CanNotSetTable {
              get { return SR.GetResourceString("DataView_CanNotSetTable", @"Cannot change Table property once it is set."); }
        }
        internal static string DataView_CanNotUse {
              get { return SR.GetResourceString("DataView_CanNotUse", @"DataTable must be set prior to using DataView."); }
        }
        internal static string DataView_CanNotBindTable {
              get { return SR.GetResourceString("DataView_CanNotBindTable", @"Cannot bind to DataTable with no name."); }
        }
        internal static string DataView_SetIListObject {
              get { return SR.GetResourceString("DataView_SetIListObject", @"Cannot set an object into this list."); }
        }
        internal static string DataView_AddNewNotAllowNull {
              get { return SR.GetResourceString("DataView_AddNewNotAllowNull", @"Cannot call AddNew on a DataView where AllowNew is false."); }
        }
        internal static string DataView_NotOpen {
              get { return SR.GetResourceString("DataView_NotOpen", @"DataView is not open."); }
        }
        internal static string DataView_CreateChildView {
              get { return SR.GetResourceString("DataView_CreateChildView", @"The relation is not parented to the table to which this DataView points."); }
        }
        internal static string DataView_CanNotDelete {
              get { return SR.GetResourceString("DataView_CanNotDelete", @"Cannot delete on a DataSource where AllowDelete is false."); }
        }
        internal static string DataView_CanNotEdit {
              get { return SR.GetResourceString("DataView_CanNotEdit", @"Cannot edit on a DataSource where AllowEdit is false."); }
        }
        internal static string DataView_GetElementIndex {
              get { return SR.GetResourceString("DataView_GetElementIndex", @"Index {0} is either negative or above rows count."); }
        }
        internal static string DataView_AddExternalObject {
              get { return SR.GetResourceString("DataView_AddExternalObject", @"Cannot add external objects to this list."); }
        }
        internal static string DataView_CanNotClear {
              get { return SR.GetResourceString("DataView_CanNotClear", @"Cannot clear this list."); }
        }
        internal static string DataView_InsertExternalObject {
              get { return SR.GetResourceString("DataView_InsertExternalObject", @"Cannot insert external objects to this list."); }
        }
        internal static string DataView_RemoveExternalObject {
              get { return SR.GetResourceString("DataView_RemoveExternalObject", @"Cannot remove objects not in the list."); }
        }
        internal static string DataROWView_PropertyNotFound {
              get { return SR.GetResourceString("DataROWView_PropertyNotFound", @"{0} is neither a DataColumn nor a DataRelation for table {1}."); }
        }
        internal static string Range_Argument {
              get { return SR.GetResourceString("Range_Argument", @"Min ({0}) must be less than or equal to max ({1}) in a Range object."); }
        }
        internal static string Range_NullRange {
              get { return SR.GetResourceString("Range_NullRange", @"This is a null range."); }
        }
        internal static string RecordManager_MinimumCapacity {
              get { return SR.GetResourceString("RecordManager_MinimumCapacity", @"MinimumCapacity must be non-negative."); }
        }
        internal static string SqlConvert_ConvertFailed {
              get { return SR.GetResourceString("SqlConvert_ConvertFailed", @" Cannot convert object of type '{0}' to object of type '{1}'."); }
        }
        internal static string DataSet_DefaultDataException {
              get { return SR.GetResourceString("DataSet_DefaultDataException", @"Data Exception."); }
        }
        internal static string DataSet_DefaultConstraintException {
              get { return SR.GetResourceString("DataSet_DefaultConstraintException", @"Constraint Exception."); }
        }
        internal static string DataSet_DefaultDeletedRowInaccessibleException {
              get { return SR.GetResourceString("DataSet_DefaultDeletedRowInaccessibleException", @"Deleted rows inaccessible."); }
        }
        internal static string DataSet_DefaultDuplicateNameException {
              get { return SR.GetResourceString("DataSet_DefaultDuplicateNameException", @"Duplicate name not allowed."); }
        }
        internal static string DataSet_DefaultInRowChangingEventException {
              get { return SR.GetResourceString("DataSet_DefaultInRowChangingEventException", @"Operation not supported in the RowChanging event."); }
        }
        internal static string DataSet_DefaultInvalidConstraintException {
              get { return SR.GetResourceString("DataSet_DefaultInvalidConstraintException", @"Invalid constraint."); }
        }
        internal static string DataSet_DefaultMissingPrimaryKeyException {
              get { return SR.GetResourceString("DataSet_DefaultMissingPrimaryKeyException", @"Missing primary key."); }
        }
        internal static string DataSet_DefaultNoNullAllowedException {
              get { return SR.GetResourceString("DataSet_DefaultNoNullAllowedException", @"Null not allowed."); }
        }
        internal static string DataSet_DefaultReadOnlyException {
              get { return SR.GetResourceString("DataSet_DefaultReadOnlyException", @"Column is marked read only."); }
        }
        internal static string DataSet_DefaultRowNotInTableException {
              get { return SR.GetResourceString("DataSet_DefaultRowNotInTableException", @"Row not found in table."); }
        }
        internal static string DataSet_DefaultVersionNotFoundException {
              get { return SR.GetResourceString("DataSet_DefaultVersionNotFoundException", @"Version not found."); }
        }
        internal static string Load_ReadOnlyDataModified {
              get { return SR.GetResourceString("Load_ReadOnlyDataModified", @"ReadOnly Data is Modified."); }
        }
        internal static string DataTableReader_InvalidDataTableReader {
              get { return SR.GetResourceString("DataTableReader_InvalidDataTableReader", @"DataTableReader is invalid for current DataTable '{0}'."); }
        }
        internal static string DataTableReader_SchemaInvalidDataTableReader {
              get { return SR.GetResourceString("DataTableReader_SchemaInvalidDataTableReader", @"Schema of current DataTable '{0}' in DataTableReader has changed, DataTableReader is invalid."); }
        }
        internal static string DataTableReader_CannotCreateDataReaderOnEmptyDataSet {
              get { return SR.GetResourceString("DataTableReader_CannotCreateDataReaderOnEmptyDataSet", @"DataTableReader Cannot be created. There is no DataTable in DataSet."); }
        }
        internal static string DataTableReader_DataTableReaderArgumentIsEmpty {
              get { return SR.GetResourceString("DataTableReader_DataTableReaderArgumentIsEmpty", @"Cannot create DataTableReader. Argument is Empty."); }
        }
        internal static string DataTableReader_ArgumentContainsNullValue {
              get { return SR.GetResourceString("DataTableReader_ArgumentContainsNullValue", @"Cannot create DataTableReader. Arguments contain null value."); }
        }
        internal static string DataTableReader_InvalidRowInDataTableReader {
              get { return SR.GetResourceString("DataTableReader_InvalidRowInDataTableReader", @"Current DataRow is either in Deleted or Detached state."); }
        }
        internal static string DataTableReader_DataTableCleared {
              get { return SR.GetResourceString("DataTableReader_DataTableCleared", @"Current DataTable '{0}' is empty. There is no DataRow in DataTable."); }
        }
        internal static string RbTree_InvalidState {
              get { return SR.GetResourceString("RbTree_InvalidState", @"DataTable internal index is corrupted: '{0}'."); }
        }
        internal static string RbTree_EnumerationBroken {
              get { return SR.GetResourceString("RbTree_EnumerationBroken", @"Collection was modified; enumeration operation might not execute."); }
        }
        internal static string NamedSimpleType_InvalidDuplicateNamedSimpleTypeDelaration {
              get { return SR.GetResourceString("NamedSimpleType_InvalidDuplicateNamedSimpleTypeDelaration", @"Simple type '{0}' has already be declared with different '{1}'."); }
        }
        internal static string DataDom_Foliation {
              get { return SR.GetResourceString("DataDom_Foliation", @"Invalid foliation."); }
        }
        internal static string DataDom_TableNameChange {
              get { return SR.GetResourceString("DataDom_TableNameChange", @"Cannot change the table name once the associated DataSet is mapped to a loaded XML document."); }
        }
        internal static string DataDom_TableNamespaceChange {
              get { return SR.GetResourceString("DataDom_TableNamespaceChange", @"Cannot change the table namespace once the associated DataSet is mapped to a loaded XML document."); }
        }
        internal static string DataDom_ColumnNameChange {
              get { return SR.GetResourceString("DataDom_ColumnNameChange", @"Cannot change the column name once the associated DataSet is mapped to a loaded XML document."); }
        }
        internal static string DataDom_ColumnNamespaceChange {
              get { return SR.GetResourceString("DataDom_ColumnNamespaceChange", @"Cannot change the column namespace once the associated DataSet is mapped to a loaded XML document."); }
        }
        internal static string DataDom_ColumnMappingChange {
              get { return SR.GetResourceString("DataDom_ColumnMappingChange", @"Cannot change the ColumnMapping property once the associated DataSet is mapped to a loaded XML document."); }
        }
        internal static string DataDom_TableColumnsChange {
              get { return SR.GetResourceString("DataDom_TableColumnsChange", @"Cannot add or remove columns from the table once the DataSet is mapped to a loaded XML document."); }
        }
        internal static string DataDom_DataSetTablesChange {
              get { return SR.GetResourceString("DataDom_DataSetTablesChange", @"Cannot add or remove tables from the DataSet once the DataSet is mapped to a loaded XML document."); }
        }
        internal static string DataDom_DataSetNestedRelationsChange {
              get { return SR.GetResourceString("DataDom_DataSetNestedRelationsChange", @"Cannot add, remove, or change Nested relations from the DataSet once the DataSet is mapped to a loaded XML document."); }
        }
        internal static string DataDom_DataSetNull {
              get { return SR.GetResourceString("DataDom_DataSetNull", @"The DataSet parameter is invalid. It cannot be null."); }
        }
        internal static string DataDom_DataSetNameChange {
              get { return SR.GetResourceString("DataDom_DataSetNameChange", @"Cannot change the DataSet name once the DataSet is mapped to a loaded XML document."); }
        }
        internal static string DataDom_CloneNode {
              get { return SR.GetResourceString("DataDom_CloneNode", @"This type of node cannot be cloned: {0}."); }
        }
        internal static string DataDom_MultipleLoad {
              get { return SR.GetResourceString("DataDom_MultipleLoad", @"Cannot load XmlDataDocument if it already contains data. Please use a new XmlDataDocument."); }
        }
        internal static string DataDom_MultipleDataSet {
              get { return SR.GetResourceString("DataDom_MultipleDataSet", @"DataSet can be associated with at most one XmlDataDocument. Cannot associate the DataSet with the current XmlDataDocument because the DataSet is already associated with another XmlDataDocument."); }
        }
        internal static string DataDom_NotSupport_GetElementById {
              get { return SR.GetResourceString("DataDom_NotSupport_GetElementById", @"GetElementById() is not supported on DataDocument."); }
        }
        internal static string DataDom_NotSupport_EntRef {
              get { return SR.GetResourceString("DataDom_NotSupport_EntRef", @"Cannot create entity references on DataDocument."); }
        }
        internal static string DataDom_NotSupport_Clear {
              get { return SR.GetResourceString("DataDom_NotSupport_Clear", @"Clear function on DateSet and DataTable is not supported on XmlDataDocument."); }
        }
        internal static string ConfigProviderNotFound {
              get { return SR.GetResourceString("ConfigProviderNotFound", @"Unable to find the requested .Net Framework Data Provider.  It may not be installed."); }
        }
        internal static string ConfigProviderInvalid {
              get { return SR.GetResourceString("ConfigProviderInvalid", @"The requested .Net Framework Data Provider's implementation does not have an Instance field of a System.Data.Common.DbProviderFactory derived type."); }
        }
        internal static string ConfigProviderNotInstalled {
              get { return SR.GetResourceString("ConfigProviderNotInstalled", @"Failed to find or load the registered .Net Framework Data Provider."); }
        }
        internal static string ConfigProviderMissing {
              get { return SR.GetResourceString("ConfigProviderMissing", @"The missing .Net Framework Data Provider's assembly qualified name is required."); }
        }
        internal static string ConfigBaseElementsOnly {
              get { return SR.GetResourceString("ConfigBaseElementsOnly", @"Only elements allowed."); }
        }
        internal static string ConfigBaseNoChildNodes {
              get { return SR.GetResourceString("ConfigBaseNoChildNodes", @"Child nodes not allowed."); }
        }
        internal static string ConfigUnrecognizedAttributes {
              get { return SR.GetResourceString("ConfigUnrecognizedAttributes", @"Unrecognized attribute '{0}'."); }
        }
        internal static string ConfigUnrecognizedElement {
              get { return SR.GetResourceString("ConfigUnrecognizedElement", @"Unrecognized element."); }
        }
        internal static string ConfigSectionsUnique {
              get { return SR.GetResourceString("ConfigSectionsUnique", @"The '{0}' section can only appear once per config file."); }
        }
        internal static string ConfigRequiredAttributeMissing {
              get { return SR.GetResourceString("ConfigRequiredAttributeMissing", @"Required attribute '{0}' not found."); }
        }
        internal static string ConfigRequiredAttributeEmpty {
              get { return SR.GetResourceString("ConfigRequiredAttributeEmpty", @"Required attribute '{0}' cannot be empty."); }
        }
        internal static string ADP_EmptyArray {
              get { return SR.GetResourceString("ADP_EmptyArray", @"Expecting non-empty array for '{0}' parameter."); }
        }
        internal static string SQL_WrongType {
              get { return SR.GetResourceString("SQL_WrongType", @"Expecting argument of type {1}, but received type {0}."); }
        }
        internal static string ADP_InvalidConnectionOptionValue {
              get { return SR.GetResourceString("ADP_InvalidConnectionOptionValue", @"Invalid value for key '{0}'."); }
        }
        internal static string ADP_KeywordNotSupported {
              get { return SR.GetResourceString("ADP_KeywordNotSupported", @"Keyword not supported: '{0}'."); }
        }
        internal static string ADP_InternalProviderError {
              get { return SR.GetResourceString("ADP_InternalProviderError", @"Internal .Net Framework Data Provider error {0}."); }
        }
        internal static string ADP_NoQuoteChange {
              get { return SR.GetResourceString("ADP_NoQuoteChange", @"The QuotePrefix and QuoteSuffix properties cannot be changed once an Insert, Update, or Delete command has been generated."); }
        }
        internal static string ADP_MissingSourceCommand {
              get { return SR.GetResourceString("ADP_MissingSourceCommand", @"The DataAdapter.SelectCommand property needs to be initialized."); }
        }
        internal static string ADP_MissingSourceCommandConnection {
              get { return SR.GetResourceString("ADP_MissingSourceCommandConnection", @"The DataAdapter.SelectCommand.Connection property needs to be initialized;"); }
        }
        internal static string ADP_InvalidMultipartName {
              get { return SR.GetResourceString("ADP_InvalidMultipartName", @"{0} ""{1}""."); }
        }
        internal static string ADP_InvalidMultipartNameQuoteUsage {
              get { return SR.GetResourceString("ADP_InvalidMultipartNameQuoteUsage", @"{0} ""{1}"", incorrect usage of quotes."); }
        }
        internal static string ADP_InvalidMultipartNameToManyParts {
              get { return SR.GetResourceString("ADP_InvalidMultipartNameToManyParts", @"{0} ""{1}"", the current limit of ""{2}"" is insufficient."); }
        }
        internal static string ADP_ColumnSchemaExpression {
              get { return SR.GetResourceString("ADP_ColumnSchemaExpression", @"The column mapping from SourceColumn '{0}' failed because the DataColumn '{1}' is a computed column."); }
        }
        internal static string ADP_ColumnSchemaMismatch {
              get { return SR.GetResourceString("ADP_ColumnSchemaMismatch", @"Inconvertible type mismatch between SourceColumn '{0}' of {1} and the DataColumn '{2}' of {3}."); }
        }
        internal static string ADP_ColumnSchemaMissing1 {
              get { return SR.GetResourceString("ADP_ColumnSchemaMissing1", @"Missing the DataColumn '{0}' for the SourceColumn '{2}'."); }
        }
        internal static string ADP_ColumnSchemaMissing2 {
              get { return SR.GetResourceString("ADP_ColumnSchemaMissing2", @"Missing the DataColumn '{0}' in the DataTable '{1}' for the SourceColumn '{2}'."); }
        }
        internal static string ADP_InvalidSourceColumn {
              get { return SR.GetResourceString("ADP_InvalidSourceColumn", @"SourceColumn is required to be a non-empty string."); }
        }
        internal static string ADP_MissingColumnMapping {
              get { return SR.GetResourceString("ADP_MissingColumnMapping", @"Missing SourceColumn mapping for '{0}'."); }
        }
        internal static string ADP_NotSupportedEnumerationValue {
              get { return SR.GetResourceString("ADP_NotSupportedEnumerationValue", @"The {0} enumeration value, {1}, is not supported by the {2} method."); }
        }
        internal static string ADP_MissingTableSchema {
              get { return SR.GetResourceString("ADP_MissingTableSchema", @"Missing the '{0}' DataTable for the '{1}' SourceTable."); }
        }
        internal static string ADP_InvalidSourceTable {
              get { return SR.GetResourceString("ADP_InvalidSourceTable", @"SourceTable is required to be a non-empty string"); }
        }
        internal static string ADP_MissingTableMapping {
              get { return SR.GetResourceString("ADP_MissingTableMapping", @"Missing SourceTable mapping: '{0}'"); }
        }
        internal static string ADP_ConnectionRequired {
              get { return SR.GetResourceString("ADP_ConnectionRequired", @"{0}: Connection property has not been initialized."); }
        }
        internal static string ADP_OpenConnectionRequired {
              get { return SR.GetResourceString("ADP_OpenConnectionRequired", @"{0} requires an open and available Connection. {1}"); }
        }
        internal static string ADP_ConnectionRequired_Insert {
              get { return SR.GetResourceString("ADP_ConnectionRequired_Insert", @"Update requires the InsertCommand to have a connection object. The Connection property of the InsertCommand has not been initialized."); }
        }
        internal static string ADP_ConnectionRequired_Update {
              get { return SR.GetResourceString("ADP_ConnectionRequired_Update", @"Update requires the UpdateCommand to have a connection object. The Connection property of the UpdateCommand has not been initialized."); }
        }
        internal static string ADP_ConnectionRequired_Delete {
              get { return SR.GetResourceString("ADP_ConnectionRequired_Delete", @"Update requires the DeleteCommand to have a connection object. The Connection property of the DeleteCommand has not been initialized."); }
        }
        internal static string ADP_ConnectionRequired_Batch {
              get { return SR.GetResourceString("ADP_ConnectionRequired_Batch", @"Update requires a connection object.  The Connection property has not been initialized."); }
        }
        internal static string ADP_ConnectionRequired_Clone {
              get { return SR.GetResourceString("ADP_ConnectionRequired_Clone", @"Update requires the command clone to have a connection object. The Connection property of the command clone has not been initialized."); }
        }
        internal static string ADP_OpenConnectionRequired_Insert {
              get { return SR.GetResourceString("ADP_OpenConnectionRequired_Insert", @"Update requires the {0}Command to have an open connection object. {1}"); }
        }
        internal static string ADP_OpenConnectionRequired_Update {
              get { return SR.GetResourceString("ADP_OpenConnectionRequired_Update", @"Update requires the {0}Command to have an open connection object. {1}"); }
        }
        internal static string ADP_OpenConnectionRequired_Delete {
              get { return SR.GetResourceString("ADP_OpenConnectionRequired_Delete", @"Update requires the {0}Command to have an open connection object. {1}"); }
        }
        internal static string ADP_OpenConnectionRequired_Clone {
              get { return SR.GetResourceString("ADP_OpenConnectionRequired_Clone", @"Update requires the updating command to have an open connection object. {1}"); }
        }
        internal static string ADP_MissingSelectCommand {
              get { return SR.GetResourceString("ADP_MissingSelectCommand", @"The SelectCommand property has not been initialized before calling '{0}'."); }
        }
        internal static string ADP_UnwantedStatementType {
              get { return SR.GetResourceString("ADP_UnwantedStatementType", @"The StatementType {0} is not expected here."); }
        }
        internal static string ADP_FillSchemaRequiresSourceTableName {
              get { return SR.GetResourceString("ADP_FillSchemaRequiresSourceTableName", @"FillSchema: expected a non-empty string for the SourceTable name."); }
        }
        internal static string ADP_FillRequiresSourceTableName {
              get { return SR.GetResourceString("ADP_FillRequiresSourceTableName", @"Fill: expected a non-empty string for the SourceTable name."); }
        }
        internal static string ADP_FillChapterAutoIncrement {
              get { return SR.GetResourceString("ADP_FillChapterAutoIncrement", @"Hierarchical chapter columns must map to an AutoIncrement DataColumn."); }
        }
        internal static string ADP_MissingDataReaderFieldType {
              get { return SR.GetResourceString("ADP_MissingDataReaderFieldType", @"DataReader.GetFieldType({0}) returned null."); }
        }
        internal static string ADP_OnlyOneTableForStartRecordOrMaxRecords {
              get { return SR.GetResourceString("ADP_OnlyOneTableForStartRecordOrMaxRecords", @"Only specify one item in the dataTables array when using non-zero values for startRecords or maxRecords."); }
        }
        internal static string ADP_UpdateRequiresSourceTable {
              get { return SR.GetResourceString("ADP_UpdateRequiresSourceTable", @"Update unable to find TableMapping['{0}'] or DataTable '{0}'."); }
        }
        internal static string ADP_UpdateRequiresSourceTableName {
              get { return SR.GetResourceString("ADP_UpdateRequiresSourceTableName", @"Update: expected a non-empty SourceTable name."); }
        }
        internal static string ADP_UpdateRequiresCommandClone {
              get { return SR.GetResourceString("ADP_UpdateRequiresCommandClone", @"Update requires the command clone to be valid."); }
        }
        internal static string ADP_UpdateRequiresCommandSelect {
              get { return SR.GetResourceString("ADP_UpdateRequiresCommandSelect", @"Auto SQL generation during Update requires a valid SelectCommand."); }
        }
        internal static string ADP_UpdateRequiresCommandInsert {
              get { return SR.GetResourceString("ADP_UpdateRequiresCommandInsert", @"Update requires a valid InsertCommand when passed DataRow collection with new rows."); }
        }
        internal static string ADP_UpdateRequiresCommandUpdate {
              get { return SR.GetResourceString("ADP_UpdateRequiresCommandUpdate", @"Update requires a valid UpdateCommand when passed DataRow collection with modified rows."); }
        }
        internal static string ADP_UpdateRequiresCommandDelete {
              get { return SR.GetResourceString("ADP_UpdateRequiresCommandDelete", @"Update requires a valid DeleteCommand when passed DataRow collection with deleted rows."); }
        }
        internal static string ADP_UpdateMismatchRowTable {
              get { return SR.GetResourceString("ADP_UpdateMismatchRowTable", @"DataRow[{0}] is from a different DataTable than DataRow[0]."); }
        }
        internal static string ADP_RowUpdatedErrors {
              get { return SR.GetResourceString("ADP_RowUpdatedErrors", @"RowUpdatedEvent: Errors occurred; no additional is information available."); }
        }
        internal static string ADP_RowUpdatingErrors {
              get { return SR.GetResourceString("ADP_RowUpdatingErrors", @"RowUpdatingEvent: Errors occurred; no additional is information available."); }
        }
        internal static string ADP_ResultsNotAllowedDuringBatch {
              get { return SR.GetResourceString("ADP_ResultsNotAllowedDuringBatch", @"When batching, the command's UpdatedRowSource property value of UpdateRowSource.FirstReturnedRecord or UpdateRowSource.Both is invalid."); }
        }
        internal static string ADP_UpdateConcurrencyViolation_Update {
              get { return SR.GetResourceString("ADP_UpdateConcurrencyViolation_Update", @"Concurrency violation: the UpdateCommand affected {0} of the expected {1} records."); }
        }
        internal static string ADP_UpdateConcurrencyViolation_Delete {
              get { return SR.GetResourceString("ADP_UpdateConcurrencyViolation_Delete", @"Concurrency violation: the DeleteCommand affected {0} of the expected {1} records."); }
        }
        internal static string ADP_UpdateConcurrencyViolation_Batch {
              get { return SR.GetResourceString("ADP_UpdateConcurrencyViolation_Batch", @"Concurrency violation: the batched command affected {0} of the expected {1} records."); }
        }
        internal static string ADP_InvalidSourceBufferIndex {
              get { return SR.GetResourceString("ADP_InvalidSourceBufferIndex", @"Invalid source buffer (size of {0}) offset: {1}"); }
        }
        internal static string ADP_InvalidDestinationBufferIndex {
              get { return SR.GetResourceString("ADP_InvalidDestinationBufferIndex", @"Invalid destination buffer (size of {0}) offset: {1}"); }
        }
        internal static string ADP_StreamClosed {
              get { return SR.GetResourceString("ADP_StreamClosed", @"Invalid attempt to {0} when stream is closed."); }
        }
        internal static string ADP_InvalidSeekOrigin {
              get { return SR.GetResourceString("ADP_InvalidSeekOrigin", @"Specified SeekOrigin value is invalid."); }
        }
        internal static string ADP_DynamicSQLJoinUnsupported {
              get { return SR.GetResourceString("ADP_DynamicSQLJoinUnsupported", @"Dynamic SQL generation is not supported against multiple base tables."); }
        }
        internal static string ADP_DynamicSQLNoTableInfo {
              get { return SR.GetResourceString("ADP_DynamicSQLNoTableInfo", @"Dynamic SQL generation is not supported against a SelectCommand that does not return any base table information."); }
        }
        internal static string ADP_DynamicSQLNoKeyInfoDelete {
              get { return SR.GetResourceString("ADP_DynamicSQLNoKeyInfoDelete", @"Dynamic SQL generation for the DeleteCommand is not supported against a SelectCommand that does not return any key column information."); }
        }
        internal static string ADP_DynamicSQLNoKeyInfoUpdate {
              get { return SR.GetResourceString("ADP_DynamicSQLNoKeyInfoUpdate", @"Dynamic SQL generation for the UpdateCommand is not supported against a SelectCommand that does not return any key column information."); }
        }
        internal static string ADP_DynamicSQLNoKeyInfoRowVersionDelete {
              get { return SR.GetResourceString("ADP_DynamicSQLNoKeyInfoRowVersionDelete", @"Dynamic SQL generation for the DeleteCommand is not supported against a SelectCommand that does not contain a row version column."); }
        }
        internal static string ADP_DynamicSQLNoKeyInfoRowVersionUpdate {
              get { return SR.GetResourceString("ADP_DynamicSQLNoKeyInfoRowVersionUpdate", @"Dynamic SQL generation for the UpdateCommand is not supported against a SelectCommand that does not contain a row version column."); }
        }
        internal static string ADP_DynamicSQLNestedQuote {
              get { return SR.GetResourceString("ADP_DynamicSQLNestedQuote", @"Dynamic SQL generation not supported against table names '{0}' that contain the QuotePrefix or QuoteSuffix character '{1}'."); }
        }
        internal static string SQL_InvalidBufferSizeOrIndex {
              get { return SR.GetResourceString("SQL_InvalidBufferSizeOrIndex", @"Buffer offset '{1}' plus the bytes available '{0}' is greater than the length of the passed in buffer."); }
        }
        internal static string SQL_InvalidDataLength {
              get { return SR.GetResourceString("SQL_InvalidDataLength", @"Data length '{0}' is less than 0."); }
        }
        internal static string SqlMisc_NullString {
              get { return SR.GetResourceString("SqlMisc_NullString", @"Null"); }
        }
        internal static string SqlMisc_MessageString {
              get { return SR.GetResourceString("SqlMisc_MessageString", @"Message"); }
        }
        internal static string SqlMisc_ArithOverflowMessage {
              get { return SR.GetResourceString("SqlMisc_ArithOverflowMessage", @"Arithmetic Overflow."); }
        }
        internal static string SqlMisc_DivideByZeroMessage {
              get { return SR.GetResourceString("SqlMisc_DivideByZeroMessage", @"Divide by zero error encountered."); }
        }
        internal static string SqlMisc_NullValueMessage {
              get { return SR.GetResourceString("SqlMisc_NullValueMessage", @"Data is Null. This method or property cannot be called on Null values."); }
        }
        internal static string SqlMisc_TruncationMessage {
              get { return SR.GetResourceString("SqlMisc_TruncationMessage", @"Numeric arithmetic causes truncation."); }
        }
        internal static string SqlMisc_DateTimeOverflowMessage {
              get { return SR.GetResourceString("SqlMisc_DateTimeOverflowMessage", @"SqlDateTime overflow. Must be between 1/1/1753 12:00:00 AM and 12/31/9999 11:59:59 PM."); }
        }
        internal static string SqlMisc_ConcatDiffCollationMessage {
              get { return SR.GetResourceString("SqlMisc_ConcatDiffCollationMessage", @"Two strings to be concatenated have different collation."); }
        }
        internal static string SqlMisc_CompareDiffCollationMessage {
              get { return SR.GetResourceString("SqlMisc_CompareDiffCollationMessage", @"Two strings to be compared have different collation."); }
        }
        internal static string SqlMisc_InvalidFlagMessage {
              get { return SR.GetResourceString("SqlMisc_InvalidFlagMessage", @"Invalid flag value."); }
        }
        internal static string SqlMisc_NumeToDecOverflowMessage {
              get { return SR.GetResourceString("SqlMisc_NumeToDecOverflowMessage", @"Conversion from SqlDecimal to Decimal overflows."); }
        }
        internal static string SqlMisc_ConversionOverflowMessage {
              get { return SR.GetResourceString("SqlMisc_ConversionOverflowMessage", @"Conversion overflows."); }
        }
        internal static string SqlMisc_InvalidDateTimeMessage {
              get { return SR.GetResourceString("SqlMisc_InvalidDateTimeMessage", @"Invalid SqlDateTime."); }
        }
        internal static string SqlMisc_TimeZoneSpecifiedMessage {
              get { return SR.GetResourceString("SqlMisc_TimeZoneSpecifiedMessage", @"A time zone was specified. SqlDateTime does not support time zones."); }
        }
        internal static string SqlMisc_InvalidArraySizeMessage {
              get { return SR.GetResourceString("SqlMisc_InvalidArraySizeMessage", @"Invalid array size."); }
        }
        internal static string SqlMisc_InvalidPrecScaleMessage {
              get { return SR.GetResourceString("SqlMisc_InvalidPrecScaleMessage", @"Invalid numeric precision/scale."); }
        }
        internal static string SqlMisc_FormatMessage {
              get { return SR.GetResourceString("SqlMisc_FormatMessage", @"The input wasn't in a correct format."); }
        }
        internal static string SqlMisc_SqlTypeMessage {
              get { return SR.GetResourceString("SqlMisc_SqlTypeMessage", @"SqlType error."); }
        }
        internal static string SqlMisc_NoBufferMessage {
              get { return SR.GetResourceString("SqlMisc_NoBufferMessage", @"There is no buffer. Read or write operation failed."); }
        }
        internal static string SqlMisc_BufferInsufficientMessage {
              get { return SR.GetResourceString("SqlMisc_BufferInsufficientMessage", @"The buffer is insufficient. Read or write operation failed."); }
        }
        internal static string SqlMisc_WriteNonZeroOffsetOnNullMessage {
              get { return SR.GetResourceString("SqlMisc_WriteNonZeroOffsetOnNullMessage", @"Cannot write to non-zero offset, because current value is Null."); }
        }
        internal static string SqlMisc_WriteOffsetLargerThanLenMessage {
              get { return SR.GetResourceString("SqlMisc_WriteOffsetLargerThanLenMessage", @"Cannot write from an offset that is larger than current length. It would leave uninitialized data in the buffer."); }
        }
        internal static string SqlMisc_NotFilledMessage {
              get { return SR.GetResourceString("SqlMisc_NotFilledMessage", @"SQL Type has not been loaded with data."); }
        }
        internal static string SqlMisc_AlreadyFilledMessage {
              get { return SR.GetResourceString("SqlMisc_AlreadyFilledMessage", @"SQL Type has already been loaded with data."); }
        }
        internal static string SqlMisc_ClosedXmlReaderMessage {
              get { return SR.GetResourceString("SqlMisc_ClosedXmlReaderMessage", @"Invalid attempt to access a closed XmlReader."); }
        }
        internal static string SqlMisc_InvalidOpStreamClosed {
              get { return SR.GetResourceString("SqlMisc_InvalidOpStreamClosed", @"Invalid attempt to call {0} when the stream is closed."); }
        }
        internal static string SqlMisc_InvalidOpStreamNonWritable {
              get { return SR.GetResourceString("SqlMisc_InvalidOpStreamNonWritable", @"Invalid attempt to call {0} when the stream non-writable."); }
        }
        internal static string SqlMisc_InvalidOpStreamNonReadable {
              get { return SR.GetResourceString("SqlMisc_InvalidOpStreamNonReadable", @"Invalid attempt to call {0} when the stream non-readable."); }
        }
        internal static string SqlMisc_InvalidOpStreamNonSeekable {
              get { return SR.GetResourceString("SqlMisc_InvalidOpStreamNonSeekable", @"Invalid attempt to call {0} when the stream is non-seekable."); }
        }
        internal static string ADP_DBConcurrencyExceptionMessage {
              get { return SR.GetResourceString("ADP_DBConcurrencyExceptionMessage", @"DB concurrency violation."); }
        }
        internal static string ADP_OperationAborted {
              get { return SR.GetResourceString("ADP_OperationAborted", @"Operation aborted."); }
        }
        internal static string ADP_OperationAbortedExceptionMessage {
              get { return SR.GetResourceString("ADP_OperationAbortedExceptionMessage", @"Operation aborted due to an exception (see InnerException for details)."); }
        }
        internal static string ADP_InvalidMaxRecords {
              get { return SR.GetResourceString("ADP_InvalidMaxRecords", @"The MaxRecords value of {0} is invalid; the value must be >= 0."); }
        }
        internal static string ADP_CollectionIndexInt32 {
              get { return SR.GetResourceString("ADP_CollectionIndexInt32", @"Invalid index {0} for this {1} with Count={2}."); }
        }
        internal static string ADP_MissingTableMappingDestination {
              get { return SR.GetResourceString("ADP_MissingTableMappingDestination", @"Missing TableMapping when TableMapping.DataSetTable='{0}'."); }
        }
        internal static string ADP_InvalidStartRecord {
              get { return SR.GetResourceString("ADP_InvalidStartRecord", @"The StartRecord value of {0} is invalid; the value must be >= 0."); }
        }
        internal static string DataDom_EnforceConstraintsShouldBeOff {
              get { return SR.GetResourceString("DataDom_EnforceConstraintsShouldBeOff", @"Please set DataSet.EnforceConstraints == false before trying to edit XmlDataDocument using XML operations."); }
        }
        internal static string DataColumns_RemoveExpression {
              get { return SR.GetResourceString("DataColumns_RemoveExpression", @"Cannot remove this column, because it is part of an expression: {0} = {1}."); }
        }
        internal static string DataRow_RowInsertTwice {
              get { return SR.GetResourceString("DataRow_RowInsertTwice", @"The rowOrder value={0} has been found twice for table named '{1}'."); }
        }
        internal static string Xml_ElementTypeNotFound {
              get { return SR.GetResourceString("Xml_ElementTypeNotFound", @"Cannot find ElementType name='{0}'."); }
        }

        internal static System.Type ResourceType {
              get { return typeof(FxResources.System.Data.Common.SR); }
        }
    }
namespace FxResources.System.Data.Common
{
    // The type of this class is used to create the ResourceManager instance as the type name matches the name of the embedded resources file
    internal static class SR
    {
    }
}
