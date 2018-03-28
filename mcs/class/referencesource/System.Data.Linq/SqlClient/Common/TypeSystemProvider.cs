using System;
using System.Collections.Generic;
using System.Text;

namespace System.Data.Linq.SqlClient {

    /// <summary>
    /// Abstracts the provider side type system. Encapsulates:
    /// - Mapping from runtime types to provider types.
    /// - Parsing type strings in the provider's language.
    /// - Handling application defined (opaque) types.
    /// - Type coercion precedence rules.
    /// - Type family organization.
    /// </summary>
    internal abstract class TypeSystemProvider {

        internal abstract ProviderType PredictTypeForUnary(SqlNodeType unaryOp, ProviderType operandType);

        internal abstract ProviderType PredictTypeForBinary(SqlNodeType binaryOp, ProviderType leftType, ProviderType rightType);

        /// <summary>
        /// Return the provider type corresponding to the given clr type.
        /// </summary>
        internal abstract ProviderType From(Type runtimeType);

        /// <summary>
        /// Return the provider type corresponding to the given object instance.
        /// </summary>
        internal abstract ProviderType From(object o);

        /// <summary>
        /// Return the provider type corresponding to the given clr type and size.
        /// </summary>
        internal abstract ProviderType From(Type type, int? size);

        /// <summary>
        /// Return a type by parsing a string. The format of the string is
        /// provider specific.
        /// </summary>
        internal abstract ProviderType Parse(string text);

        /// <summary>
        /// Return a type understood only by the application.
        /// Each call with the same index will return the same ProviderType.
        /// </summary>
        internal abstract ProviderType GetApplicationType(int index);

        /// <summary>
        /// Returns the most precise type in the family of the type given.
        /// A family is a group types that serve similar functions. For example,
        /// in SQL SmallInt and Int are part of one family.
        /// </summary>
        internal abstract ProviderType MostPreciseTypeInFamily(ProviderType type);

        /// <summary>
        /// For LOB data types that have large type equivalents, this function returns the equivalent large
        /// data type.  If the type is not an LOB or cannot be converted, the function returns the current type.
        /// For example SqlServer defines the 'Image' LOB type, whose large type equivalent is VarBinary(MAX).
        /// </summary>
        internal abstract ProviderType GetBestLargeType(ProviderType type);

        /// <summary>
        /// Returns a type that can be used to hold values for both the current
        /// type and the specified type without data loss.
        /// </summary>
        internal abstract ProviderType GetBestType(ProviderType typeA, ProviderType typeB);

        internal abstract ProviderType ReturnTypeOfFunction(SqlFunctionCall functionCall);

        /// <summary>
        /// Get a type that can hold the same information but belongs to a different type family.
        /// For example, to represent a SQL NChar as an integer type, we need to use the type int.
        /// (SQL smallint would not be able to contain characters with unicode >32768)
        /// </summary>
        /// <param name="toType">Type of the target type family</param>
        /// <returns>Smallest type of target type family that can hold equivalent information</returns>
        internal abstract ProviderType ChangeTypeFamilyTo(ProviderType type, ProviderType typeWithFamily);

        internal abstract void InitializeParameter(ProviderType type, System.Data.Common.DbParameter parameter, object value);
    }

    /// <summary>
    /// Flags control the format of string returned by ToQueryString().
    /// </summary>
    [Flags]
    internal enum QueryFormatOptions {
        None = 0,
        SuppressSize = 1
    }

    /// <summary>
    /// An abstract type exposed by the TypeSystemProvider.
    /// </summary>
    internal abstract class ProviderType {

        /// <summary>
        /// True if this type is a Unicode type (eg, ntext, etc).
        /// </summary>
        internal abstract bool IsUnicodeType { get; }

        /// <summary>
        /// For a unicode type, return it's non-unicode equivalent.
        /// </summary>
        internal abstract ProviderType GetNonUnicodeEquivalent();

        /// <summary>
        /// True if this type has only a CLR representation and no provider representation.
        /// </summary>
        internal abstract bool IsRuntimeOnlyType { get; }

        /// <summary>
        /// True if this type is an application defined type.
        /// </summary>
        internal abstract bool IsApplicationType { get; }

        /// <summary>
        /// Determine whether this is the given application type.
        /// </summary>
        internal abstract bool IsApplicationTypeOf(int index);

        /// <summary>
        /// Returns the CLR type which most closely corresponds to this provider type.
        /// </summary>
        internal abstract Type GetClosestRuntimeType();

        /// <summary>
        /// Compare implicit type coercion precedence.
        /// -1 means there is an implicit conversion from this->type.
        /// 0 means there is a two way implicit conversion from this->type
        /// 1 means there is an implicit conversion from type->this.
        /// </summary>
        internal abstract int ComparePrecedenceTo(ProviderType type);

        /// <summary>
        /// Determines whether two types are in the same type family.
        /// A family is a group types that serve similar functions. For example,
        /// in SQL SmallInt and Int are part of one family.
        /// </summary>
        internal abstract bool IsSameTypeFamily(ProviderType type);

        /// <summary>
        /// Used to indicate if the type supports comparison in provider.
        /// </summary>
        /// <returns>Returns true if type supports comparison in provider.</returns>
        internal abstract bool SupportsComparison { get; }

        /// <summary>
        /// Used to indicate if the types supports Length function (LEN in T-SQL).  
        /// </summary>
        /// <returns>Returns true if type supports use of length function on the type.</returns>
        internal abstract bool SupportsLength { get; }

        /// <summary>
        /// Returns true if the given values will be equal to eachother for this type.
        /// </summary>
        internal abstract bool AreValuesEqual(object o1, object o2);

        /// <summary>
        /// Determines whether this type is a LOB (large object) type, or an equivalent type.
        /// For example, on SqlServer, Image and VarChar(MAX) among others are considered large types.
        /// </summary>
        /// <returns></returns>
        internal abstract bool IsLargeType { get; }

        /// <summary>
        /// Convert this type into a string that can be used in a query.
        /// </summary>
        internal abstract string ToQueryString();

        /// <summary>
        /// Convert this type into a string that can be used in a query.
        /// </summary>
        internal abstract string ToQueryString(QueryFormatOptions formatOptions);

        /// <summary>
        /// Whether this type is fixed size or not.
        /// </summary>
        internal abstract bool IsFixedSize { get; }

        /// <summary>
        /// The type has a size or is large.
        /// </summary>
        internal abstract bool HasSizeOrIsLarge { get; }

        /// <summary>
        /// The size of this type.
        /// </summary>
        internal abstract int? Size { get; }

        /// <summary>
        /// True if the type can be ordered.
        /// </summary>
        internal abstract bool IsOrderable { get; }

        /// <summary>
        /// True if the type can be grouped.
        /// </summary>
        internal abstract bool IsGroupable { get; }

        /// <summary>
        /// True if the type can appear in a column
        /// </summary>
        internal abstract bool CanBeColumn { get; }

        /// <summary>
        /// True if the type can appear as a parameter
        /// </summary>
        internal abstract bool CanBeParameter { get; }

        /// <summary>
        /// True if the type is a single character type.
        /// </summary>
        internal abstract bool IsChar { get; }

        /// <summary>
        /// True if the type is a multi-character type.
        /// </summary>
        internal abstract bool IsString { get; }

        /// <summary>
        /// True if the type is a number.
        /// </summary>
        internal abstract bool IsNumeric { get; }

        /// <summary>
        /// Returns true if the type uses precision and scale.  For example, returns true
        /// for SqlDBTypes Decimal, Float and Real.
        /// </summary>
        internal abstract bool HasPrecisionAndScale { get; }

        /// <summary>
        /// Determines if it is safe to suppress size specifications for
        /// the operand of a cast/convert.  For example, when casting to string,
        /// all these types have length less than the default sized used by SqlServer,
        /// so the length specification can be omitted without fear of truncation.
        /// </summary>
        internal abstract bool CanSuppressSizeForConversionToString{ get; }

        public static bool operator ==(ProviderType typeA, ProviderType typeB) {
            if ((object)typeA == (object)typeB)
                return true;
            if ((object)typeA != null)
                return typeA.Equals(typeB);
            return false;
        }

        public static bool operator != (ProviderType typeA, ProviderType typeB) {
            if ((object)typeA == (object)typeB)
                return false;
            if ((object)typeA != null)
                return !typeA.Equals(typeB);
            return true;
        }

        public override bool Equals(object obj) {
            return base.Equals(obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
