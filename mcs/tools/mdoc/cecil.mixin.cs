namespace Mono.Cecil
{
	static partial class Mixin {

		public static bool IsTypeSpecification (this TypeReference type)
		{
			return type is GenericParameter || type is TypeSpecification;
		}
	}
}