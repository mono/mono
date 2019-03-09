namespace System
{
	partial class DefaultBinder
	{
		private static bool CanChangePrimitive(Type source, Type target)
		{
			throw new NotImplementedException ();
		}

		private static bool CanChangePrimitiveObjectToType(object source, Type type)
		{
			if (source == null)
				return true;
			if (!type.IsPrimitive || !source.GetType ().IsPrimitive)
				return false;
			throw new NotImplementedException ();
		}
	}
}