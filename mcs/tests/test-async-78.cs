using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public static class ExceptionHelper
{
	public static async Task ConditionalCatchExceptThreadAbortAsync (Func<Task>
		funcTask, Action<Exception> conditionalCatchAction)
	{
		funcTask ();
		return;
	}
}

class ATask
{
	readonly object _asyncTaskCancellationSource = new object ();

	readonly object aname;

	public async Task<bool> OnDoWorkAsync ()
	{
		await ExceptionHelper.ConditionalCatchExceptThreadAbortAsync (
			async () => {
				if (_asyncTaskCancellationSource != null) {
					string item = null;

					await ExceptionHelper.ConditionalCatchExceptThreadAbortAsync (
						async () => {
							Console.WriteLine (aname);
						},
						(e) => {
							Console.WriteLine (item);
						}
					);
				}
			},
			null
			);

		return true;
	}
}

public class Tests
{
	public static void Main ()
	{
		var a = new ATask ();
		var res = a.OnDoWorkAsync ().Result;
	}
}