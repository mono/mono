using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Linker.Steps;

namespace UnityProfileShaper
{
	internal class FailIfWeMissTypesThatWeShippedInAPreviousVersionStep : BaseStep
	{
		protected override void ProcessAssembly(AssemblyDefinition assembly)
		{
			IEnumerable<string> typenames;
			try
			{
				var path = Path.Combine(Tools.GetTuningFolder(),"TuningInput/PreviouslyShipped/" + assembly.Name.Name + ".dll");
				var previousShippedVersion =
					AssemblyFactory.GetAssembly(path);
				typenames = previousShippedVersion.MainModule.Types.Cast<TypeDefinition>().Where(t=>t.IsPublic).Select(t => t.FullName);

			} catch (FileNotFoundException)
			{
				//that's cool.
				return;
			}

			var types = assembly.MainModule.Types;

			foreach (var requiredtype in typenames.Where(requiredtype => !types.Cast<TypeDefinition>().Any(type => type.FullName == requiredtype)))
			{
				throw new Exception("The type "+requiredtype+" was shipped in a previous version of Unity, but is currently being linked away");
			}
		}
	}
}