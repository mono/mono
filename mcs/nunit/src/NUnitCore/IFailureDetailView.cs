namespace NUnit.Runner {

  using System;
  using System.Windows.Forms;

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
    /// Returns the component used to present the TraceView
    /// </summary>
    Control GetComponent();

    /// <summary>
    /// Shows details of a TestFailure
    /// </summary>
    void ShowFailure(TestFailure failure);
  }
}
