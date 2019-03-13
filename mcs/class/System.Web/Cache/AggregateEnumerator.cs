using System.Collections;

namespace System.Web.Caching
{
  internal class AggregateEnumerator : IDictionaryEnumerator, IEnumerator
  {
    private IDictionaryEnumerator[] _enumerators;
    private int _iCurrent;

    internal AggregateEnumerator(IDictionaryEnumerator[] enumerators)
    {
      this._enumerators = enumerators;
    }

    public bool MoveNext()
    {
      bool flag;
      while (true)
      {
        flag = this._enumerators[this._iCurrent].MoveNext();
        if (!flag && this._iCurrent != this._enumerators.Length - 1)
          ++this._iCurrent;
        else
          break;
      }
      return flag;
    }

    public void Reset()
    {
      for (int index = 0; index <= this._iCurrent; ++index)
        this._enumerators[index].Reset();
      this._iCurrent = 0;
    }

    public object Current
    {
      get
      {
        return this._enumerators[this._iCurrent].Current;
      }
    }

    public object Key
    {
      get
      {
        return this._enumerators[this._iCurrent].Key;
      }
    }

    public object Value
    {
      get
      {
        return this._enumerators[this._iCurrent].Value;
      }
    }

    public DictionaryEntry Entry
    {
      get
      {
        return this._enumerators[this._iCurrent].Entry;
      }
    }
  }
}
