using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Lattices
{
    internal static class AbstractDomainExtensions
    {
        public static bool IsNormal<T>(this IAbstractDomain<T> domain)
        {
            return !domain.IsTop && !domain.IsBottom;
        }

        public static string BottomSymbolIfAny<T>(this IAbstractDomain<T> domain)
        {
            return domain.IsBottom ? "_|_" : string.Empty;
        }

        public static bool TryTrivialLessEqual<T>(this T left, T right, out bool result)
            where T: IAbstractDomain<T>
        {
            if (ReferenceEquals(left, right))
                return true.With (true, out result);

            if (left.IsBottom)
                return true.With (true, out result);

            if (left.IsTop)
                return true.With (right.IsTop, out result);

            if (right.IsBottom)
                return true.With (false, out result);

            if (right.IsTop)
                return true.With (true, out result);

            return false.Without (out result);
        }

        public static bool TryTrivialJoin<T>(this T left, T right, out T result )
            where T : IAbstractDomain<T>
        {
            if (ReferenceEquals(left, right))
                return true.With (left, out result);

            if (left.IsBottom)
                return true.With (right, out result);

            if (left.IsTop)
                return true.With (left, out result);

            if (right.IsBottom)
                return true.With (left, out result);

            if (right.IsTop)
                return true.With (right, out result);

            return false.Without (out result);
        }

        public static bool TryTrivialMeet<T>(this T left, T right, out T result )
            where T : IAbstractDomain<T>
        {
            if (ReferenceEquals(left, right))
                return true.With (left, out result);

            if (left.IsBottom)
                return true.With (left, out result);

            if (left.IsTop)
                return true.With (right, out result);

            if (right.IsBottom)
                return true.With (right, out result);

            if (right.IsTop)
                return true.With (left, out result);

            return false.Without (out result);
        }
    }
}