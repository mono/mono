namespace NUnit.Extensions {

  using System;

  using NUnit.Framework;
  /// <summary>A Decorator that runs a test repeatedly.</summary>
  public class RepeatedTest: TestDecorator {
    private readonly int fTimesRepeat;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="test"></param>
    /// <param name="repeat"></param>
    public RepeatedTest(ITest test, int repeat) : base(test) {
      if (repeat < 0) {
        throw new ArgumentOutOfRangeException("repeat", "Repetition count must be > 0");
      }
      fTimesRepeat= repeat;
    }
  /// <summary>
  /// 
  /// </summary>
    public override int CountTestCases {
      get { return base.CountTestCases * fTimesRepeat; }
    }
  /// <summary>
  /// 
  /// </summary>
  /// <param name="result"></param>
    public override void Run(TestResult result) {
      for (int i= 0; i < fTimesRepeat; i++) {
        if (result.ShouldStop)
          break;
        base.Run(result);
      }
    }
  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
    public override string ToString() {
      return base.ToString()+"(repeated)";
    }
  }
}
