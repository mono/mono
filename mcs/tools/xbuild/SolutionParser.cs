//
// SolutionParser.cs: Generates a project file from a solution file.
//
// Author:
//   Jonathan Chambers (joncham@gmail.com)
//
// (C) 2009 Jonathan Chambers
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#if NET_2_0

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.Build.BuildEngine;

namespace Mono.XBuild.CommandLine {
	class ProjectInfo {
		public string Name;
		public string FileName;

		public ProjectInfo (string name, string fileName)
		{
			Name = name;
			FileName = fileName;
		}

		public Dictionary<TargetInfo, TargetInfo> TargetMap = new Dictionary<TargetInfo, TargetInfo> ();
		public List<Guid> Dependencies = new List<Guid> ();
	}

	struct TargetInfo {
		public string Configuration;
		public string Platform;
		public bool Build;

		public TargetInfo (string configuration, string platform)
			: this (configuration, platform, false)
		{
		}

		public TargetInfo (string configuration, string platform, bool build)
		{
			Configuration = configuration;
			Platform = platform;
			Build = build;
		}
	}


	class SolutionParser {
		static string[] buildTargets = new string[] { "Build", "Clean", "Rebuild", "Publish" };

		static string guidExpression = "{[A-Fa-f0-9]{8}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{12}}";

		static Regex projectRegex = new Regex ("Project\\(\"(" + guidExpression + ")\"\\) = \"(.*?)\", \"(.*?)\", \"(" + guidExpression + ")\"(\\s*?)((\\s*?)ProjectSection\\((.*?)\\) = (.*?)EndProjectSection(\\s*?))*(\\s*?)EndProject?", RegexOptions.Singleline);
		static Regex projectDependenciesRegex = new Regex ("ProjectSection\\((.*?)\\) = \\w*(.*?)EndProjectSection", RegexOptions.Singleline);
		static Regex projectDependencyRegex = new Regex ("\\s*(" + guidExpression + ") = (" + guidExpression + ")");

		static Regex globalRegex = new Regex ("Global(.*)EndGlobal", RegexOptions.Singleline);
		static Regex globalSectionRegex = new Regex ("GlobalSection\\((.*?)\\) = \\w*(.*?)EndGlobalSection", RegexOptions.Singleline);

		static Regex solutionConfigurationRegex = new Regex ("\\s*(.*?)\\|(.*?) = (.*?)\\|(.+)");
		static Regex projectConfigurationActiveCfgRegex = new Regex ("\\s*(" + guidExpression + ")\\.(.+?)\\|(.+?)\\.ActiveCfg = (.+?)\\|(.+)");
		static Regex projectConfigurationBuildRegex = new Regex ("\\s*(" + guidExpression + ")\\.(.*?)\\|(.*?)\\.Build\\.0 = (.*?)\\|(.+)");

		static string solutionFolderGuid = "{2150E333-8FDC-42A3-9474-1A3956D46DE8}";
		static string vcprojGuid = "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}";

		public void ParseSolution (string file, Project p)
		{
			AddGeneralSettings (file, p);

			StreamReader reader = new StreamReader (file);
			string line = reader.ReadToEnd ();
			line = line.Replace ("\r\n", "\n");
			string solutionDir = Path.GetDirectoryName (file);

			List<TargetInfo> solutionTargets = new List<TargetInfo> ();
			Dictionary<Guid, ProjectInfo> projectInfos = new Dictionary<Guid, ProjectInfo> ();

			Match m = projectRegex.Match (line);
			while (m.Success) {
				ProjectInfo projectInfo = new ProjectInfo (m.Groups[2].Value, m.Groups[3].Value);
				if (String.Compare (m.Groups [1].Value, solutionFolderGuid,
						StringComparison.InvariantCultureIgnoreCase) == 0) {
					// Ignore solution folders
					m = m.NextMatch ();
					continue;
				}
				if (String.Compare (m.Groups [1].Value, vcprojGuid,
						StringComparison.InvariantCultureIgnoreCase) == 0) {
					// Ignore vcproj 
					ErrorUtilities.ReportWarning (0, string.Format("Ignoring vcproj '{0}'.", projectInfo.Name));
					m = m.NextMatch ();
					continue;
				}

				projectInfos.Add (new Guid (m.Groups[4].Value), projectInfo);

				Project currentProject = p.ParentEngine.CreateNewProject ();
				currentProject.Load (Path.Combine (solutionDir,
							projectInfo.FileName.Replace ('\\', Path.DirectorySeparatorChar)));

				foreach (BuildItem bi in currentProject.GetEvaluatedItemsByName ("ProjectReference")) {
					string projectReferenceGuid = bi.GetEvaluatedMetadata ("Project");
					projectInfo.Dependencies.Add (new Guid (projectReferenceGuid));
				}

				Match projectSectionMatch = projectDependenciesRegex.Match (m.Groups[6].Value);
				while (projectSectionMatch.Success) {
					Match projectDependencyMatch = projectDependencyRegex.Match (projectSectionMatch.Value);
					while (projectDependencyMatch.Success) {
						projectInfo.Dependencies.Add (new Guid (projectDependencyMatch.Groups[1].Value));
						projectDependencyMatch = projectDependencyMatch.NextMatch ();
					}
					projectSectionMatch = projectSectionMatch.NextMatch ();
				}
				m = m.NextMatch ();
			}

			Match globalMatch = globalRegex.Match (line);
			Match globalSectionMatch = globalSectionRegex.Match (globalMatch.Groups[1].Value);
			while (globalSectionMatch.Success) {
				string sectionType = globalSectionMatch.Groups[1].Value;
				switch (sectionType) {
					case "SolutionConfigurationPlatforms":
						ParseSolutionConfigurationPlatforms (globalSectionMatch.Groups[2].Value, solutionTargets);
						break;
					case "ProjectConfigurationPlatforms":
						ParseProjectConfigurationPlatforms (globalSectionMatch.Groups[2].Value, projectInfos);
						break;
					case "SolutionProperties":
						ParseSolutionProperties (globalSectionMatch.Groups[2].Value);
						break;
					case "NestedProjects":
						break;
					default:
						ErrorUtilities.ReportWarning (0, string.Format("Don't know how to handle GlobalSection {0}, Ignoring.", sectionType));
						break;
				}
				globalSectionMatch = globalSectionMatch.NextMatch ();
			}

			AddCurrentSolutionConfigurationContents (p, solutionTargets, projectInfos);
			AddValidateSolutionConfiguration (p);
			AddProjectTargets (p, solutionTargets, projectInfos);
			AddSolutionTargets (p, projectInfos);

		}

		void AddGeneralSettings (string solutionFile, Project p)
		{
			p.DefaultTargets = "Build";
			p.InitialTargets = "ValidateSolutionConfiguration";
			p.AddNewUsingTaskFromAssemblyName ("CreateTemporaryVCProject", "Microsoft.Build.Tasks, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
			p.AddNewUsingTaskFromAssemblyName ("ResolveVCProjectOutput", "Microsoft.Build.Tasks, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");

			BuildPropertyGroup configurationPropertyGroup = p.AddNewPropertyGroup (true);
			configurationPropertyGroup.Condition = " '$(Configuration)' == '' ";
			configurationPropertyGroup.AddNewProperty ("Configuration", "Debug");

			BuildPropertyGroup platformPropertyGroup = p.AddNewPropertyGroup (true);
			platformPropertyGroup.Condition = " '$(Platform)' == '' ";
			platformPropertyGroup.AddNewProperty ("Platform", "Any CPU");

			BuildPropertyGroup aspNetConfigurationPropertyGroup = p.AddNewPropertyGroup (true);
			aspNetConfigurationPropertyGroup.Condition = " ('$(AspNetConfiguration)' == '') ";
			aspNetConfigurationPropertyGroup.AddNewProperty ("AspNetConfiguration", "$(Configuration)");

			string solutionFilePath = Path.GetFullPath (solutionFile);
			BuildPropertyGroup solutionPropertyGroup = p.AddNewPropertyGroup (true);
			solutionPropertyGroup.AddNewProperty ("SolutionDir", Path.GetDirectoryName (solutionFilePath) + Path.DirectorySeparatorChar);
			solutionPropertyGroup.AddNewProperty ("SolutionExt", Path.GetExtension (solutionFile));
			solutionPropertyGroup.AddNewProperty ("SolutionFileName", Path.GetFileName (solutionFile));
			solutionPropertyGroup.AddNewProperty ("SolutionName", Path.GetFileNameWithoutExtension (solutionFile));
			solutionPropertyGroup.AddNewProperty ("SolutionPath", solutionFilePath);
		}

		void ParseSolutionConfigurationPlatforms (string section, List<TargetInfo> solutionTargets)
		{
			Match solutionConfigurationPlatform = solutionConfigurationRegex.Match (section);
			while (solutionConfigurationPlatform.Success) {
				string solutionConfiguration = solutionConfigurationPlatform.Groups[1].Value;
				string solutionPlatform = solutionConfigurationPlatform.Groups[2].Value;
				solutionTargets.Add (new TargetInfo (solutionConfiguration, solutionPlatform));
				solutionConfigurationPlatform = solutionConfigurationPlatform.NextMatch ();
			}
		}

		void ParseProjectConfigurationPlatforms (string section, Dictionary<Guid, ProjectInfo> projectInfos)
		{
			List<Guid> missingGuids = new List<Guid> ();
			Match projectConfigurationPlatform = projectConfigurationActiveCfgRegex.Match (section);
			while (projectConfigurationPlatform.Success) {
				Guid guid = new Guid (projectConfigurationPlatform.Groups[1].Value);
				ProjectInfo projectInfo;
				if (!projectInfos.TryGetValue (guid, out projectInfo)) {
					if (!missingGuids.Contains (guid)) {
						ErrorUtilities.ReportWarning (0, string.Format("Failed to find project {0}", guid));
						missingGuids.Add (guid);
					}
					projectConfigurationPlatform = projectConfigurationPlatform.NextMatch ();
					continue;
				}
				string solConf = projectConfigurationPlatform.Groups[2].Value;
				string solPlat = projectConfigurationPlatform.Groups[3].Value;
				string projConf = projectConfigurationPlatform.Groups[4].Value;
				string projPlat = projectConfigurationPlatform.Groups[5].Value;
				// hack, what are they doing here?
				if (projPlat == "Any CPU")
					projPlat = "AnyCPU";
				projectInfo.TargetMap.Add (new TargetInfo (solConf, solPlat), new TargetInfo (projConf, projPlat));
				projectConfigurationPlatform = projectConfigurationPlatform.NextMatch ();
			}
			Match projectConfigurationPlatformBuild = projectConfigurationBuildRegex.Match (section);
			while (projectConfigurationPlatformBuild.Success) {
				Guid guid = new Guid (projectConfigurationPlatformBuild.Groups[1].Value);
				ProjectInfo projectInfo;
				if (!projectInfos.TryGetValue (guid, out projectInfo)) {
					if (!missingGuids.Contains (guid)) {
						ErrorUtilities.ReportWarning (0, string.Format("Failed to find project {0}", guid));
						missingGuids.Add (guid);
					}
					projectConfigurationPlatformBuild = projectConfigurationPlatformBuild.NextMatch ();
					continue;
				}
				string solConf = projectConfigurationPlatformBuild.Groups[2].Value;
				string solPlat = projectConfigurationPlatformBuild.Groups[3].Value;
				string projConf = projectConfigurationPlatformBuild.Groups[4].Value;
				string projPlat = projectConfigurationPlatformBuild.Groups[5].Value;
				// hack, what are they doing here?
				if (projPlat == "Any CPU")
					projPlat = "AnyCPU";
				projectInfo.TargetMap[new TargetInfo (solConf, solPlat)] = new TargetInfo (projConf, projPlat, true);
				projectConfigurationPlatformBuild = projectConfigurationPlatformBuild.NextMatch ();
			}
		}

		void ParseSolutionProperties (string section)
		{
		}

		void AddCurrentSolutionConfigurationContents (Project p, List<TargetInfo> solutionTargets, Dictionary<Guid, ProjectInfo> projectInfos)
		{
			foreach (TargetInfo solutionTarget in solutionTargets) {
				BuildPropertyGroup platformPropertyGroup = p.AddNewPropertyGroup (false);
				platformPropertyGroup.Condition = string.Format (
					" ('$(Configuration)' == '{0}') and ('$(Platform)' == '{1}') ",
					solutionTarget.Configuration,
					solutionTarget.Platform
					);

				string solutionConfigurationContents = "<SolutionConfiguration xmlns=\"\">";
				foreach (KeyValuePair<Guid, ProjectInfo> projectInfo in projectInfos) {
					foreach (KeyValuePair<TargetInfo, TargetInfo> targetInfo in projectInfo.Value.TargetMap) {
						if (solutionTarget.Configuration == targetInfo.Key.Configuration && solutionTarget.Platform == targetInfo.Key.Platform) {
							solutionConfigurationContents += string.Format ("<ProjectConfiguration Project=\"{0}\">{1}|{2}</ProjectConfiguration>",
								projectInfo.Key.ToString ("B").ToUpper (), targetInfo.Value.Configuration, targetInfo.Value.Platform);
						}
					}
				}
				solutionConfigurationContents += "</SolutionConfiguration>";

				platformPropertyGroup.AddNewProperty ("CurrentSolutionConfigurationContents", solutionConfigurationContents);
			}
		}

		void AddWarningForMissingProjectConfiguration (Target target, string slnConfig, string slnPlatform, string projectName)
		{
			BuildTask task = target.AddNewTask ("Warning");
			task.SetParameterValue ("Text",
					String.Format ("The project configuration for project '{0}' corresponding " +
						"to the solution configuration '{1}|{2}' was not found in the solution file.",
						projectName, slnConfig, slnPlatform));
			task.Condition = String.Format ("('$(Configuration)' == '{0}') and ('$(Platform)' == '{1}')",
						slnConfig, slnPlatform);

		}

		void AddValidateSolutionConfiguration (Project p)
		{
			Target t = p.Targets.AddNewTarget ("ValidateSolutionConfiguration");
			BuildTask task = t.AddNewTask ("Error");
			task.SetParameterValue ("Text", "Invalid solution configuration and platform: \"$(Configuration)|$(Platform)\".");
			task.Condition = "('$(CurrentSolutionConfigurationContents)' == '') and ('$(SkipInvalidConfigurations)' != 'true')";
			task = t.AddNewTask ("Warning");
			task.SetParameterValue ("Text", "Invalid solution configuration and platform: \"$(Configuration)|$(Platform)\".");
			task.Condition = "('$(CurrentSolutionConfigurationContents)' == '') and ('$(SkipInvalidConfigurations)' == 'true')";
			task = t.AddNewTask ("Message");
			task.SetParameterValue ("Text", "Building solution configuration \"$(Configuration)|$(Platform)\".");
			task.Condition = "'$(CurrentSolutionConfigurationContents)' != ''";
		}

		void AddProjectTargets (Project p, List<TargetInfo> solutionTargets, Dictionary<Guid, ProjectInfo> projectInfos)
		{
			foreach (KeyValuePair<Guid, ProjectInfo> projectInfo in projectInfos) {
				ProjectInfo project = projectInfo.Value;
				foreach (string buildTarget in buildTargets) {
					Target target = p.Targets.AddNewTarget (project.Name + (buildTarget == "Build" ? string.Empty : ":" + buildTarget));
					target.Condition = "'$(CurrentSolutionConfigurationContents)' != ''"; 
					string dependencies = string.Empty;
					foreach (Guid dependency in project.Dependencies) {
						ProjectInfo dependentInfo;
						if (projectInfos.TryGetValue (dependency, out dependentInfo)) {
							if (dependencies.Length > 0)
								dependencies += ";";
							dependencies += dependentInfo.Name;
							if (buildTarget != "Build")
								dependencies += ":" + buildTarget;
						}
					}
					if (dependencies != string.Empty)
						target.DependsOnTargets = dependencies;

					foreach (TargetInfo targetInfo in solutionTargets) {
						BuildTask task = null;
						TargetInfo projectTargetInfo;
						if (!project.TargetMap.TryGetValue (targetInfo, out projectTargetInfo)) {
							AddWarningForMissingProjectConfiguration (target, targetInfo.Configuration,
									targetInfo.Platform, project.Name);
							continue;
						}
						if (projectTargetInfo.Build) {
							task = target.AddNewTask ("MSBuild");
							task.SetParameterValue ("Projects", project.FileName);
							
							if (buildTarget != "Build")
								task.SetParameterValue ("Targets", buildTarget);
							task.SetParameterValue ("Properties", string.Format ("Configuration={0}; Platform={1}; BuildingSolutionFile=true; CurrentSolutionConfigurationContents=$(CurrentSolutionConfigurationContents); SolutionDir=$(SolutionDir); SolutionExt=$(SolutionExt); SolutionFileName=$(SolutionFileName); SolutionName=$(SolutionName); SolutionPath=$(SolutionPath)", projectTargetInfo.Configuration, projectTargetInfo.Platform));
						} else {
							task = target.AddNewTask ("Message");
							task.SetParameterValue ("Text", string.Format ("Project \"{0}\" is disabled for solution configuration \"{1}|{2}\".", project.Name, targetInfo.Configuration, targetInfo.Platform));
						}
						task.Condition = string.Format (" ('$(Configuration)' == '{0}') and ('$(Platform)' == '{1}') ", targetInfo.Configuration, targetInfo.Platform);
					}
				}
			}
		}

		void AddSolutionTargets (Project p, Dictionary<Guid, ProjectInfo> projectInfos)
		{
			foreach (string buildTarget in buildTargets) {
				Target t = p.Targets.AddNewTarget (buildTarget);
				t.Condition = "'$(CurrentSolutionConfigurationContents)' != ''";
				BuildTask task = t.AddNewTask ("CallTarget");
				string targets = string.Empty;
				foreach (KeyValuePair<Guid, ProjectInfo> projectInfo in projectInfos) {
					if (targets.Length > 0)
						targets += ";";
					targets += projectInfo.Value.Name;
					if (buildTarget != "Build")
						targets += ":" + buildTarget;
				}
				task.SetParameterValue ("Targets", targets);
				task.SetParameterValue ("RunEachTargetSeparately", "true");
			}
		}
	}
}

#endif
