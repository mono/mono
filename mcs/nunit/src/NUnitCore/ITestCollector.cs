namespace NUnit.Runner {
  using System.Collections;

  /// <summary>
  ///    Collects Test class names to be presented by the TestSelector.
  ///  <see foocref="TestSelector"/>
  /// </summary>
  public interface ITestCollector {

    /// <summary>
    ///    Returns a StringCollection of qualified class names.
    /// </summary>
    Hashtable CollectTests();
  }
}
