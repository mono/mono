using System.Threading.Tasks;

namespace System.IO
{
    partial class Stream
    {
        public virtual ValueTask DisposeAsync()
        {
            try
            {
                Dispose();
                return default;
            }
            catch (Exception exc)
            {
                return new ValueTask(Task.FromException(exc));
            }
        }
    }
}