using System;


namespace Microsoft.Build.Framework
{
	[Serializable]
	public class BuildEventContext
	{
		static readonly Random rnd = new Random ();

		public static BuildEventContext Invalid = new BuildEventContext (
			InvalidSubmissionId,
			InvalidNodeId,
			InvalidProjectInstanceId,
			InvalidTargetId,
			InvalidProjectContextId,
			InvalidTaskId);

		internal static BuildEventContext NewInstance ()
		{
			return new BuildEventContext (rnd.Next (), rnd.Next (), rnd.Next (), rnd.Next ());
		}

		public BuildEventContext (int nodeId, int targetId, int projectContextId, int taskId)
			: this (nodeId, rnd.Next (), targetId, projectContextId, taskId)
		{
		}

		public BuildEventContext (int nodeId, int projectInstanceId, int targetId, int projectContextId, int taskId)
			: this (rnd.Next (), nodeId, projectInstanceId, targetId, projectContextId, taskId)
		{
		}

		public BuildEventContext (int submissionId, int nodeId, int projectInstanceId, int targetId, int projectContextId, int taskId)
		{
			SubmissionId = submissionId;
			NodeId = nodeId;
			ProjectInstanceId = projectInstanceId;
			TargetId = targetId;
			ProjectContextId = projectContextId;
			TaskId = taskId;
		}

		public const int InvalidSubmissionId = -1;
		public const int InvalidNodeId = -2;
		public const int InvalidProjectInstanceId = -1;
		public const int InvalidTargetId = -1;
		public const int InvalidProjectContextId = -2;
		public const int InvalidTaskId = -1;

		public int SubmissionId { get; private set; }
		public int NodeId { get; private set; }
		public int ProjectInstanceId { get; private set; }
		public int TargetId { get; private set; }
		public int ProjectContextId { get; private set; }
		public int TaskId { get; private set; }

		// MSDN document says "true if the references are equal, false otherwise." but that doesn't make sense.
		public override bool Equals (object other)
		{
			var o = other as BuildEventContext;
			return (object) o != null &&
				o.NodeId == NodeId &&
				o.ProjectContextId == ProjectContextId &&
				o.ProjectInstanceId == ProjectInstanceId &&
				o.SubmissionId == SubmissionId &&
				o.TargetId == TargetId &&
				o.TaskId == TaskId;
		}

		public override int GetHashCode ()
		{
			return
				(NodeId << 5) +
				(ProjectContextId << 4) +
				(ProjectInstanceId << 3) +
				(SubmissionId << 2) +
				(TargetId << 1) +
				TaskId;
		}

		public static bool operator == (BuildEventContext left, BuildEventContext right)
		{
			return (object) left == null ? (object)right == null : left.Equals (right);
		}

		public static bool operator != (BuildEventContext left, BuildEventContext right)
		{
			return (object) left == null ? (object)right != null : !left.Equals (right);
		}
	}
}


