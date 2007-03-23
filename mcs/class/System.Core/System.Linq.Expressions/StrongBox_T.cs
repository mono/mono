namespace System.Linq.Expressions
{
    public class StrongBox<T> : IStrongBox
    {
        #region .ctor
        public StrongBox(T value)
        {
            Value = value;
        }
        #endregion

        #region Fields
        public T Value;
        #endregion

        #region IStrongBox Members
        object IStrongBox.Value
        {
            get { return Value; }
            set { Value = (T)value; }
        }
        #endregion
    }
}