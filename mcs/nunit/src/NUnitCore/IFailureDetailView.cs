namespace NUnit.Runner {

  using System;

  using NUnit.Framework;

  /// <summary>
  ///    A view to show details about a failure
  /// </summary>
  public interface IFailureDetailView {
    /// <summary>
    /// Clears the view
    /// </summary>
    void Clear();

    /// <summary>
    /// Shows details of a TestFailure
    /// </summary>
    void ShowFailure(TestFailure failure);
  }
}
