namespace System.Threading.Tasks
{
	partial class TaskScheduler
	{
		internal void InternalQueueTask(Task task)
		{
			task.FireTaskScheduledIfNeeded(this);
			this.QueueTask(task);
		}
	}
}