partial class SR
{
	public const string ArgumentOutOfRange_ConsoleKey = "Console key values must be between 0 and 255 inclusive.";
	public const string Arg_InvalidComObjectException = "Attempt has been made to use a COM object that does not have a backing class factory.";
	public const string Arg_MustBeNullTerminatedString = "The string must be null-terminated.";
	public const string Arg_InvalidOleVariantTypeException = "Specified OLE variant was invalid.";
	public const string Arg_SafeArrayRankMismatchException = "Specified array was not of the expected rank.";
	public const string Arg_SafeArrayTypeMismatchException = "Specified array was not of the expected type.";
	public const string TypeNotDelegate = "'Type '{0}' is not a delegate type.  EventTokenTable may only be used with delegate types.'";
	public const string InvalidOperationException_ActorGraphCircular = "An Actor must not create a circular reference between itself (or one of its child Actors) and one of its parents.";
	public const string InvalidOperation_ClaimCannotBeRemoved = "The Claim '{0}' was not able to be removed.  It is either not part of this Identity or it is a Claim that is owned by the Principal that contains this Identity. For example, the Principal will own the Claim when creating a GenericPrincipal with roles. The roles will be exposed through the Identity that is passed in the constructor, but not actually owned by the Identity.  Similar logic exists for a RolePrincipal.";
	public const string PlatformNotSupported_Serialization = "This instance contains state that cannot be serialized and deserialized on this platform.";
	public const string PrivilegeNotHeld_Default = "The process does not possess some privilege required for this operation.";
	public const string PrivilegeNotHeld_Named = "The process does not possess the '{0}' privilege which is required for this operation.";
	public const string CountdownEvent_Decrement_BelowZero = "Invalid attempt made to decrement the event's count below zero.";
	public const string CountdownEvent_Increment_AlreadyZero = "The event is already signaled and cannot be incremented.";
	public const string CountdownEvent_Increment_AlreadyMax = "The increment operation would cause the CurrentCount to overflow.";
	public const string ArrayWithOffsetOverflow = "ArrayWithOffset: offset exceeds array size.";
	public const string Arg_NotIsomorphic = "Object contains non-primitive or non-blittable data.";
	public const string StructArrayTooLarge = "Array size exceeds addressing limitations.";
	public const string IO_DriveNotFound = "Could not find the drive. The drive might not be ready or might not be mapped.";
	public const string Argument_MustSupplyParent = "When supplying the ID of a containing object, the FieldInfo that identifies the current field within that object must also be supplied.";
	public const string Argument_MemberAndArray = "Cannot supply both a MemberInfo and an Array to indicate the parent of a value type.";
	public const string Argument_MustSupplyContainer = "When supplying a FieldInfo for fixing up a nested type, a valid ID for that containing object must also be supplied.";
	public const string Serialization_NoID = "Object has never been assigned an objectID";
	public const string Arg_SwitchExpressionException = "Non-exhaustive switch expression failed to match its input.";
	public const string SwitchExpressionException_UnmatchedValue = "Unmatched value was {0}.";
	public const string Argument_InvalidRandomRange = "Range of random number does not contain at least one possibility.";
	public const string BufferWriterAdvancedTooFar = "Cannot advance past the end of the buffer, which has a size of {0}.";
}