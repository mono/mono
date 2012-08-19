using Mono.CodeContracts.Static.Lattices;

using NUnit.Framework;

namespace MonoTests.Mono.CodeContracts {
        [TestFixture (Category = "FlatDomainTrivialTests")]
        class FlatDomainTests : DomainTestBase<FlatDomain<int>> {
                protected override FlatDomain<int> Top { get { return FlatDomain<int>.TopValue; } }
                protected override FlatDomain<int> Bottom { get { return FlatDomain<int>.BottomValue; } }
                protected override FlatDomain<int> Normal { get { return 1; } }
        }
}