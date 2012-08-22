using System;
using System.Data.Objects.DataClasses;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Mono.CodeContracts.Static.Analysis;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis
{
  internal class MethodModel : EntityObject
  {
    private long _Id;
    private byte[] _Hash;
    private string _Name;
    private bool _Timeout;
    private int _StatsTop;
    private int _StatsBottom;
    private int _StatsTrue;
    private int _StatsFalse;
    private int _SwallowedTop;
    private int _SwallowedBottom;
    private int _SwallowedTrue;
    private int _SwallowedFalse;
    private long _Contracts;
    private long _MethodInstructions;
    private long _ContractInstructions;
    private long _PureParametersMask;
    private byte[] _InferredExpr;
    private byte[] _InferredExprHash;
    private string _InferredExprString;
    private string _FullName;

    public AnalysisStatistics Statistics
    {
      get
      {
        return new AnalysisStatistics()
        {
          Bottom = (uint) this.StatsBottom,
          Top = (uint) this.StatsTop,
          True = (uint) this.StatsTrue,
          False = (uint) this.StatsFalse,
          Total = (uint) (this.StatsBottom + this.StatsTop + this.StatsTrue + this.StatsFalse)
        };
      }
      set
      {
        this.StatsBottom = (int) value.Bottom;
        this.StatsFalse = (int) value.False;
        this.StatsTrue = (int) value.True;
        this.StatsTop = (int) value.Top;
      }
    }

    public ContractDensity ContractDensity
    {
      get
      {
        return new ContractDensity((ulong) this.MethodInstructions, (ulong) this.ContractInstructions, (ulong) this.Contracts);
      }
      set
      {
        this.MethodInstructions = (long) value.MethodInstructions;
        this.ContractInstructions = (long) value.ContractInstructions;
        this.Contracts = (long) value.Contracts;
      }
    }

    public SwallowedBuckets Swallowed
    {
      get
      {
        return new SwallowedBuckets((SwallowedBuckets.CounterGetter) (outcome =>
        {
          switch (outcome)
          {
            case ProofOutcome.Top:
              return this.SwallowedTop;
            case ProofOutcome.Bottom:
              return this.SwallowedBottom;
            case ProofOutcome.True:
              return this.SwallowedTrue;
            case ProofOutcome.False:
              return this.SwallowedFalse;
            default:
              throw new ArgumentException();
          }
        }));
      }
      set
      {
        this.SwallowedTop = value.GetCounter(ProofOutcome.Top);
        this.SwallowedBottom = value.GetCounter(ProofOutcome.Bottom);
        this.SwallowedTrue = value.GetCounter(ProofOutcome.True);
        this.SwallowedFalse = value.GetCounter(ProofOutcome.False);
      }
    }

    public long Id
    {
      get
      {
        return this._Id;
      }
      set
      {
        if (this._Id == value)
          return;
        this.ReportPropertyChanging("Id");
        this._Id = StructuralObject.SetValidValue(value);
        this.ReportPropertyChanged("Id");
      }
    }

    public byte[] Hash
    {
      get
      {
        return StructuralObject.GetValidValue(this._Hash);
      }
      set
      {
        this.ReportPropertyChanging("Hash");
        this._Hash = StructuralObject.SetValidValue(value, false);
        this.ReportPropertyChanged("Hash");
      }
    }

    public string Name
    {
      get
      {
        return this._Name;
      }
      set
      {
        this.ReportPropertyChanging("Name");
        this._Name = StructuralObject.SetValidValue(value, false);
        this.ReportPropertyChanged("Name");
      }
    }

    public bool Timeout
    {
      get
      {
        return this._Timeout;
      }
      set
      {
        this.ReportPropertyChanging("Timeout");
        this._Timeout = StructuralObject.SetValidValue(value);
        this.ReportPropertyChanged("Timeout");
      }
    }

    internal int StatsTop
    {
      get
      {
        return this._StatsTop;
      }
      private set
      {
        this.ReportPropertyChanging("StatsTop");
        this._StatsTop = StructuralObject.SetValidValue(value);
        this.ReportPropertyChanged("StatsTop");
      }
    }

    internal int StatsBottom
    {
      get
      {
        return this._StatsBottom;
      }
      private set
      {
        this.ReportPropertyChanging("StatsBottom");
        this._StatsBottom = StructuralObject.SetValidValue(value);
        this.ReportPropertyChanged("StatsBottom");
      }
    }

    internal int StatsTrue
    {
      get
      {
        return this._StatsTrue;
      }
      private set
      {
        this.ReportPropertyChanging("StatsTrue");
        this._StatsTrue = StructuralObject.SetValidValue(value);
        this.ReportPropertyChanged("StatsTrue");
      }
    }

    internal int StatsFalse
    {
      get
      {
        return this._StatsFalse;
      }
      private set
      {
        this.ReportPropertyChanging("StatsFalse");
        this._StatsFalse = StructuralObject.SetValidValue(value);
        this.ReportPropertyChanged("StatsFalse");
      }
    }

    internal int SwallowedTop
    {
      get
      {
        return this._SwallowedTop;
      }
      private set
      {
        this.ReportPropertyChanging("SwallowedTop");
        this._SwallowedTop = StructuralObject.SetValidValue(value);
        this.ReportPropertyChanged("SwallowedTop");
      }
    }

    internal int SwallowedBottom
    {
      get
      {
        return this._SwallowedBottom;
      }
      private set
      {
        this.ReportPropertyChanging("SwallowedBottom");
        this._SwallowedBottom = StructuralObject.SetValidValue(value);
        this.ReportPropertyChanged("SwallowedBottom");
      }
    }

    internal int SwallowedTrue
    {
      get
      {
        return this._SwallowedTrue;
      }
      private set
      {
        this.ReportPropertyChanging("SwallowedTrue");
        this._SwallowedTrue = StructuralObject.SetValidValue(value);
        this.ReportPropertyChanged("SwallowedTrue");
      }
    }

    internal int SwallowedFalse
    {
      get
      {
        return this._SwallowedFalse;
      }
      private set
      {
        this.ReportPropertyChanging("SwallowedFalse");
        this._SwallowedFalse = StructuralObject.SetValidValue(value);
        this.ReportPropertyChanged("SwallowedFalse");
      }
    }

    internal long Contracts
    {
      get
      {
        return this._Contracts;
      }
      private set
      {
        this.ReportPropertyChanging("Contracts");
        this._Contracts = StructuralObject.SetValidValue(value);
        this.ReportPropertyChanged("Contracts");
      }
    }

    internal long MethodInstructions
    {
      get
      {
        return this._MethodInstructions;
      }
      private set
      {
        this.ReportPropertyChanging("MethodInstructions");
        this._MethodInstructions = StructuralObject.SetValidValue(value);
        this.ReportPropertyChanged("MethodInstructions");
      }
    }

    internal long ContractInstructions
    {
      get
      {
        return this._ContractInstructions;
      }
      private set
      {
        this.ReportPropertyChanging("ContractInstructions");
        this._ContractInstructions = StructuralObject.SetValidValue(value);
        this.ReportPropertyChanged("ContractInstructions");
      }
    }

    public long PureParametersMask
    {
      get
      {
        return this._PureParametersMask;
      }
      set
      {
        this.ReportPropertyChanging("PureParametersMask");
        this._PureParametersMask = StructuralObject.SetValidValue(value);
        this.ReportPropertyChanged("PureParametersMask");
      }
    }

    public byte[] InferredExpr
    {
      get
      {
        return StructuralObject.GetValidValue(this._InferredExpr);
      }
      set
      {
        this.ReportPropertyChanging("InferredExpr");
        this._InferredExpr = StructuralObject.SetValidValue(value, true);
        this.ReportPropertyChanged("InferredExpr");
      }
    }

    public byte[] InferredExprHash
    {
      get
      {
        return StructuralObject.GetValidValue(this._InferredExprHash);
      }
      set
      {
        this.ReportPropertyChanging("InferredExprHash");
        this._InferredExprHash = StructuralObject.SetValidValue(value, true);
        this.ReportPropertyChanged("InferredExprHash");
      }
    }

    public string InferredExprString
    {
      get
      {
        return this._InferredExprString;
      }
      set
      {
        this.ReportPropertyChanging("InferredExprString");
        this._InferredExprString = StructuralObject.SetValidValue(value, true);
        this.ReportPropertyChanged("InferredExprString");
      }
    }

    public string FullName
    {
      get
      {
        return this._FullName;
      }
      set
      {
        this.ReportPropertyChanging("FullName");
        this._FullName = StructuralObject.SetValidValue(value, false);
        this.ReportPropertyChanged("FullName");
      }
    }

    public static MethodModel CreateMethodModel(long id, byte[] hash, string name, bool timeout, long pureParametersMask, string fullName)
    {
      return new MethodModel()
      {
        Id = id,
        Hash = hash,
        Name = name,
        Timeout = timeout,
        PureParametersMask = pureParametersMask,
        FullName = fullName
      };
    }
  }
}
