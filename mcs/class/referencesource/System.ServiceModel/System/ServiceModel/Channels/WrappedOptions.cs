namespace System.ServiceModel.Channels
{
    public class WrappedOptions
    {
        bool wrappedFlag = false;
        public bool WrappedFlag { get { return this.wrappedFlag; } set { this.wrappedFlag = value; } }
    }
}
