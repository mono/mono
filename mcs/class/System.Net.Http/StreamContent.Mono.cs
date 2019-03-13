using System.IO;
using System.Threading;

namespace System.Net.Http
{
	partial class StreamContent
	{
		//
		// Workarounds for poor .NET API
		// Instead of having SerializeToStreamAsync with CancellationToken as public API. Only LoadIntoBufferAsync
		// called internally from the send worker can be cancelled and user cannot see/do it
		//
		[Obsolete ("FIXME: Please talk to Martin about this; see https://github.com/mono/mono/issues/12996.")]
		internal StreamContent (Stream content, CancellationToken cancellationToken)
			: this (content)
		{
		}
	}
}
