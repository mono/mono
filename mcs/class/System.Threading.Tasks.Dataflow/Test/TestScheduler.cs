using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonoTests {
	public class TestScheduler : TaskScheduler {
		readonly ConcurrentQueue<Task> queue = new ConcurrentQueue<Task> ();

		protected override void QueueTask (Task task)
		{
			queue.Enqueue (task);
		}

		protected override bool TryExecuteTaskInline (Task task, bool taskWasPreviouslyQueued)
		{
			queue.Enqueue (task);
			return false;
		}

		protected override IEnumerable<Task> GetScheduledTasks ()
		{
			return queue;
		}

		public int ExecuteAll ()
		{
			int i = 0;
			Task task;
			while (queue.TryDequeue (out task)) {
				TryExecuteTask (task);
				i++;
			}
			return i;
		}
	}
}