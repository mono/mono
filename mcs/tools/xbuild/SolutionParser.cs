//
// SolutionParser.cs: Generates a project file from a solution file.
//
// Author:
//   Jonathan Chambers (joncham@gmail.com)
//   Ankit Jain <jankit@novell.com>
//   Lluis Sanchez Gual <lluis@novell.com>
//
// (C) 2009 Jonathan Chambers
// Copyright 2008, 2009 Novell, Inc (http://www.novell.com)
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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.Build.BuildEngine;

namespace Mono.XBuild.CommandLine {
	class ProjectInfo {
		public string Name;
		public string FileName;
		public Guid Guid;

		public ProjectInfo (string name, string fileName)
		{
			Name = name;
			FileName = fileName;
		}

		public Dictionary<TargetInfo, TargetInfo> TargetMap = new Dictionary<TargetInfo, TargetInfo> ();
		public Dictionary<Guid, ProjectInfo> Dependencies = new Dictionary<Guid, ProjectInfo> ();
		public Dictionary<string, ProjectSection> ProjectSections = new Dictionary<string, ProjectSection> ();
		public List<string> AspNetConfigurations = new List<string> ();
	}

	class ProjectSection {
		public string Name;
		public Dictionary<string, string> Properties = new Dictionary<string, string> ();

		public ProjectSection (string name)
		{
			Name = name;
		}
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


	internal delegate void RaiseWarningHandler (int errorNumber, string message);

	class SolutionParser {
		static string[] buildTargets = new string[] { "Build", "Clean", "Rebuild", "Publish" };

		static string guidExpression = "{[A-Fa-f0-9]{8}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{12}}";

		static Regex slnVersionRegex = new Regex (@"Microsoft Visual Studio Solution File, Format Version (\d?\d.\d\d)");
		static Regex projectRegex = new Regex ("Project\\(\"(" + guidExpression + ")\"\\) = \"(.*?)\", \"(.*?)\", \"(" + guidExpression + ")\"(\\s*?)((\\s*?)ProjectSection\\((.*?)\\) = (.*?)EndProjectSection(\\s*?))*(\\s*?)(EndProject)?", RegexOptions.Singleline);
		static Regex projectDependenciesRegex = new Regex ("ProjectSection\\((.*?)\\) = \\w*(.*?)EndProjectSection", RegexOptions.Singleline);
		static Regex projectDependencyRegex = new Regex ("\\s*(" + guidExpression + ") = (" + guidExpression + ")");
		static Regex projectSectionPropertiesRegex = new Regex ("\\s*(?<name>.*) = \"(?<value>.*)\"");

		static Regex globalRegex = new Regex ("Global(.*)EndGlobal", RegexOptions.Singleline);
		static Regex globalSectionRegex = new Regex ("GlobalSection\\((.*?)\\) = \\w*(.*?)EndGlobalSection", RegexOptions.Singleline);

		static Regex solutionConfigurationRegex = new Regex ("\\s*(.*?)\\|(.*?) = (.*?)\\|(.+)");
		static Regex projectConfigurationActiveCfgRegex = new Regex ("\\s*(" + guidExpression + ")\\.(.+?)\\|(.+?)\\.ActiveCfg = (.+?)\\|(.+)");
		static Regex projectConfigurationBuildRegex = new Regex ("\\s*(" + guidExpression + ")\\.(.*?)\\|(.*?)\\.Build\\.0 = (.*?)\\|(.+)");

		static string solutionFolderGuid = "{2150E333-8FDC-42A3-9474-1A3956D46DE8}";
		static string vcprojGuid = "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}";
		static string websiteProjectGuid = "{E24C65DC-7377-472B-9ABA-BC803B73C61A}";

		RaiseWarningHandler RaiseWarning;

		public void ParseSolution (string file, Project p, RaiseWarningHandler RaiseWarning)
		{
			this.RaiseWarning = RaiseWarning;
			AddGeneralSettings (file, p);

			StreamReader reader = new StreamReader (file);
			string slnVersion = GetSlnFileVersion (reader);
			if (slnVersion == "11.00")
				p.DefaultToolsVersion = "4.0";
			else if (slnVersion == "10.00")
				p.DefaultToolsVersion = "3.5";
			else
				p.DefaultToolsVersion = "2.0";

			string line = reader.ReadToEnd ();
			line = line.Replace ("\r\n", "\n");
			string solutionDir = Path.GetDirectoryName (file);

			List<TargetInfo> solutionTargets = new List<TargetInfo> ();
			Dictionary<Guid, ProjectInfo> projectInfos = new Dictionary<Guid, ProjectInfo> ();
			Dictionary<Guid, ProjectInfo> websiteProjectInfos = new Dictionary<Guid, ProjectInfo> ();
			List<ProjectInfo>[] infosByLevel = null;
			Dictionary<Guid, ProjectInfo> unsupportedProjectInfos = new Dictionary<Guid, ProjectInfo> ();

			Match m = projectRegex.Match (line);
			while (m.Success) {
				ProjectInfo projectInfo = new ProjectInfo (m.Groups[2].Value,
								Path.GetFullPath (Path.Combine (solutionDir,
									m.Groups [3].Value.Replace ('\\', Path.DirectorySeparatorChar))));
				if (String.Compare (m.Groups [1].Value, solutionFolderGuid,
						StringComparison.InvariantCultureIgnoreCase) == 0) {
					// Ignore solution folders
					m = m.NextMatch ();
					continue;
				}

				projectInfo.Guid = new Guid (m.Groups [4].Value);

				if (String.Compare (m.Groups [1].Value, vcprojGuid,
						StringComparison.InvariantCultureIgnoreCase) == 0) {
					// Ignore vcproj 
					RaiseWarning (0, string.Format("Ignoring vcproj '{0}'.", projectInfo.Name));

					unsupportedProjectInfos [projectInfo.Guid] = projectInfo;
					m = m.NextMatch ();
					continue;
				}

				if (String.Compare (m.Groups [1].Value, websiteProjectGuid,
						StringComparison.InvariantCultureIgnoreCase) == 0)
					websiteProjectInfos.Add (new Guid (m.Groups[4].Value), projectInfo);
				else
					projectInfos.Add (projectInfo.Guid, projectInfo);

				Match projectSectionMatch = projectDependenciesRegex.Match (m.Groups[6].Value);
				while (projectSectionMatch.Success) {
					string section_name = projectSectionMatch.Groups [1].Value;
					if (String.Compare (section_name, "ProjectDependencies") == 0) {
						Match projectDependencyMatch = projectDependencyRegex.Match (projectSectionMatch.Value);
						while (projectDependencyMatch.Success) {
							// we might not have projectInfo available right now, so
							// set it to null, and fill it in later
							projectInfo.Dependencies [new Guid (projectDependencyMatch.Groups[1].Value)] = null;
							projectDependencyMatch = projectDependencyMatch.NextMatch ();
						}
					} else {
						ProjectSection section = new ProjectSection (section_name);
						Match propertiesMatch = projectSectionPropertiesRegex.Match (
									projectSectionMatch.Groups [2].Value);
						while (propertiesMatch.Success) {
							section.Properties [propertiesMatch.Groups ["name"].Value] =
								propertiesMatch.Groups ["value"].Value;

							propertiesMatch = propertiesMatch.NextMatch ();
						}

						projectInfo.ProjectSections [section_name] = section;
					}
					projectSectionMatch = projectSectionMatch.NextMatch ();
				}
				m = m.NextMatch ();
			}

			foreach (ProjectInfo projectInfo in projectInfos.Values) {
				string filename = projectInfo.FileName;
				string projectDir = Path.GetDirectoryName (filename);

				if (!File.Exists (filename)) {
					RaiseWarning (0, String.Format ("Project file {0} referenced in the solution file, " +
								"not found. Ignoring.", filename));
					continue;
				}

				Project currentProject = p.ParentEngine.CreateNewProject ();
				try {
					currentProject.Load (filename, ProjectLoadSettings.IgnoreMissingImports);
				} catch (InvalidProjectFileException e) {
					RaiseWarning (0, e.Message);
					continue;
				}

				foreach (BuildItem bi in currentProject.GetEvaluatedItemsByName ("ProjectReference")) {
					ProjectInfo info = null;
					string projectReferenceGuid = bi.GetEvaluatedMetadata ("Project");
					bool hasGuid = !String.IsNullOrEmpty (projectReferenceGuid);

					// try to resolve the ProjectReference by GUID
					// and fallback to project filename

					if (hasGuid) {
						Guid guid = new Guid (projectReferenceGuid);
						projectInfos.TryGetValue (guid, out info);
						if (info == null && unsupportedProjectInfos.TryGetValue (guid, out info)) {
							RaiseWarning (0, String.Format (
									"{0}: ProjectReference '{1}' is of an unsupported type. Ignoring.",
									filename, bi.Include));
							continue;
						}
					}

					if (info == null || !hasGuid) {
						// Project not found by guid or guid not available
						// Try to find by project file

						string fullpath = Path.GetFullPath (Path.Combine (projectDir, bi.Include.Replace ('\\', Path.DirectorySeparatorChar)));
						info = projectInfos.Values.FirstOrDefault (pi => pi.FileName == fullpath);

						if (info == null) {
							if (unsupportedProjectInfos.Values.Any (pi => pi.FileName == fullpath))
								RaiseWarning (0, String.Format (
										"{0}: ProjectReference '{1}' is of an unsupported type. Ignoring.",
										filename, bi.Include));
							else
								RaiseWarning (0, String.Format (
										"{0}: ProjectReference '{1}' not found, neither by guid '{2}' nor by project file name '{3}'.",
										filename, bi.Include, projectReferenceGuid.Replace ("{", "").Replace ("}", ""), fullpath));
						}

					}

					if (info != null)
						projectInfo.Dependencies [info.Guid] = info;
				}
			}

			// fill in the project info for deps found in the .sln file
			foreach (ProjectInfo projectInfo in projectInfos.Values) {
				List<Guid> missingInfos = new List<Guid> ();
				foreach (KeyValuePair<Guid, ProjectInfo> dependency in projectInfo.Dependencies) {
					if (dependency.Value == null)
						missingInfos.Add (dependency.Key);
				}

				foreach (Guid guid in missingInfos) {
					ProjectInfo info;
					if (projectInfos.TryGetValue (guid, out info))
						projectInfo.Dependencies [guid] = info;
					else
						projectInfo.Dependencies.Remove (guid);
				}
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
						ParseProjectConfigurationPlatforms (globalSectionMatch.Groups[2].Value,
								projectInfos, websiteProjectInfos);
						break;
					case "SolutionProperties":
						ParseSolutionProperties (globalSectionMatch.Groups[2].Value);
						break;
					case "NestedProjects":
						break;
					case "MonoDevelopProperties":
						break;
					default:
						RaiseWarning (0, string.Format("Don't know how to handle GlobalSection {0}, Ignoring.", sectionType));
						break;
				}
				globalSectionMatch = globalSectionMatch.NextMatch ();
			}

			int num_levels = AddBuildLevels (p, solutionTargets, projectInfos, ref infosByLevel);

			AddCurrentSolutionConfigurationContents (p, solutionTargets, projectInfos, websiteProjectInfos);
			AddWebsiteProperties (p, websiteProjectInfos, projectInfos);
			AddValidateSolutionConfiguration (p);

			AddGetFrameworkPathTarget (p);
			AddWebsiteTargets (p, websiteProjectInfos, projectInfos, infosByLevel, solutionTargets);
			AddProjectTargets (p, solutionTargets, projectInfos);
			AddSolutionTargets (p, num_levels, websiteProjectInfos.Values);
		}

                string GetSlnFileVersion (StreamReader reader)
                {
                        string strVersion = null;
                        string strInput = null;
                        Match match;

                        strInput = reader.ReadLine();
                        if (strInput == null)
                                return null;

                        match = slnVersionRegex.Match(strInput);
                        if (!match.Success) {
                                strInput = reader.ReadLine();
                                if (strInput == null)
                                        return null;
                                match = slnVersionRegex.Match (strInput);
                        }

                        if (match.Success)
                                return match.Groups[1].Value;

                        return null;
                }

		void AddGeneralSettings (string solutionFile, Project p)
		{
			p.DefaultTargets = "Build";
			p.InitialTargets = "ValidateSolutionConfiguration";
			p.AddNewUsingTaskFromAssemblyName ("CreateTemporaryVCProject", "Microsoft.Build.Tasks, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
			p.AddNewUsingTaskFromAssemblyName ("ResolveVCProjectOutput", "Microsoft.Build.Tasks, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");

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

		// ignores the website projects, in the websiteProjectInfos
		void ParseProjectConfigurationPlatforms (string section, Dictionary<Guid, ProjectInfo> projectInfos,
				Dictionary<Guid, ProjectInfo> websiteProjectInfos)
		{
			List<Guid> missingGuids = new List<Guid> ();
			Match projectConfigurationPlatform = projectConfigurationActiveCfgRegex.Match (section);
			while (projectConfigurationPlatform.Success) {
				Guid guid = new Guid (projectConfigurationPlatform.Groups[1].Value);
				ProjectInfo projectInfo;
				if (!projectInfos.TryGetValue (guid, out projectInfo)) {
					if (!missingGuids.Contains (guid)) {
						if (!websiteProjectInfos.ContainsKey (guid))
							// ignore website projects
							RaiseWarning (0, string.Format("Failed to find project {0}", guid));
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
						RaiseWarning (0, string.Format("Failed to find project {0}", guid));
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

		void AddCurrentSolutionConfigurationContents (Project p, List<TargetInfo> solutionTargets,
				Dictionary<Guid, ProjectInfo> projectInfos,
				Dictionary<Guid, ProjectInfo> websiteProjectInfos)
		{
			TargetInfo default_target_info = new TargetInfo ("Debug", "Any CPU");
			if (solutionTargets.Count > 0) {
				bool found = false;
				foreach (TargetInfo tinfo in solutionTargets) {
					if (String.Compare (tinfo.Platform, "Mixed Platforms") == 0) {
						default_target_info = tinfo;
						found = true;
						break;
					}
				}

				if (!found)
					default_target_info = solutionTargets [0];
			}

			AddDefaultSolutionConfiguration (p, default_target_info);

			foreach (TargetInfo solutionTarget in solutionTargets) {
				BuildPropertyGroup platformPropertyGroup = p.AddNewPropertyGroup (false);
				platformPropertyGroup.Condition = string.Format (
					" ('$(Configuration)' == '{0}') and ('$(Platform)' == '{1}') ",
					solutionTarget.Configuration,
					solutionTarget.Platform
					);

				StringBuilder solutionConfigurationContents = new StringBuilder ();
				solutionConfigurationContents.Append ("<SolutionConfiguration xmlns=\"\">");
				foreach (KeyValuePair<Guid, ProjectInfo> projectInfo in projectInfos) {
					AddProjectConfigurationItems (projectInfo.Key, projectInfo.Value, solutionTarget, solutionConfigurationContents);
				}
				solutionConfigurationContents.Append ("</SolutionConfiguration>");

				platformPropertyGroup.AddNewProperty ("CurrentSolutionConfigurationContents",
						solutionConfigurationContents.ToString ());
			}
		}

		void AddProjectConfigurationItems (Guid guid, ProjectInfo projectInfo, TargetInfo solutionTarget,
				StringBuilder solutionConfigurationContents)
		{
			foreach (KeyValuePair<TargetInfo, TargetInfo> targetInfo in projectInfo.TargetMap) {
				if (solutionTarget.Configuration == targetInfo.Key.Configuration &&
						solutionTarget.Platform == targetInfo.Key.Platform) {
					solutionConfigurationContents.AppendFormat (
							"<ProjectConfiguration Project=\"{0}\">{1}|{2}</ProjectConfiguration>",
					guid.ToString ("B").ToUpper (), targetInfo.Value.Configuration, targetInfo.Value.Platform);
				}
			}
		}

		void AddDefaultSolutionConfiguration (Project p, TargetInfo target)
		{
			BuildPropertyGroup configurationPropertyGroup = p.AddNewPropertyGroup (true);
			configurationPropertyGroup.Condition = " '$(Configuration)' == '' ";
			configurationPropertyGroup.AddNewProperty ("Configuration", target.Configuration);

			BuildPropertyGroup platformPropertyGroup = p.AddNewPropertyGroup (true);
			platformPropertyGroup.Condition = " '$(Platform)' == '' ";
			platformPropertyGroup.AddNewProperty ("Platform", target.Platform);
			
			// emit default for AspNetConfiguration also
			BuildPropertyGroup aspNetConfigurationPropertyGroup = p.AddNewPropertyGroup (true);
			aspNetConfigurationPropertyGroup.Condition = " ('$(AspNetConfiguration)' == '') ";
			aspNetConfigurationPropertyGroup.AddNewProperty ("AspNetConfiguration", "$(Configuration)");
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

		// Website project methods

		void AddWebsiteProperties (Project p, Dictionary<Guid, ProjectInfo> websiteProjectInfos,
				Dictionary<Guid, ProjectInfo> projectInfos)
		{
			var propertyGroupByConfig = new Dictionary<string, BuildPropertyGroup> ();
			foreach (KeyValuePair<Guid, ProjectInfo> infoPair in websiteProjectInfos) {
				ProjectInfo info = infoPair.Value;
				string projectGuid = infoPair.Key.ToString ();

				ProjectSection section;
				if (!info.ProjectSections.TryGetValue ("WebsiteProperties", out section)) {
					RaiseWarning (0, String.Format ("Website project '{0}' does not have the required project section: WebsiteProperties. Ignoring project.", info.Name));
					return;
				}

				//parse project references
				string [] ref_guids = null;
				string references;
				if (section.Properties.TryGetValue ("ProjectReferences", out references)) {
					ref_guids = references.Split (new char [] {';'}, StringSplitOptions.RemoveEmptyEntries);
					for (int i = 0; i < ref_guids.Length; i ++) {
						// "{guid}|foo.dll"
						ref_guids [i] = ref_guids [i].Split ('|') [0];

						Guid r_guid = new Guid (ref_guids [i]);
						ProjectInfo ref_info;
						if (projectInfos.TryGetValue (r_guid, out ref_info))
							// ignore if not found
							info.Dependencies [r_guid] = ref_info;
					}
				}

				foreach (KeyValuePair<string, string> pair in section.Properties) {
					//looking for -- ConfigName.AspNetCompiler.PropName
					string [] parts = pair.Key.Split ('.');
					if (parts.Length != 3 || String.Compare (parts [1], "AspNetCompiler") != 0)
						continue;

					string config = parts [0];
					string propertyName = parts [2];

					BuildPropertyGroup bpg;
					if (!propertyGroupByConfig.TryGetValue (config, out bpg)) {
						bpg = p.AddNewPropertyGroup (true);
						bpg.Condition = String.Format (" '$(AspNetConfiguration)' == '{0}' ", config);
						propertyGroupByConfig [config] = bpg;
					}

					bpg.AddNewProperty (String.Format ("Project_{0}_AspNet{1}", projectGuid, propertyName),
								pair.Value);

					if (!info.AspNetConfigurations.Contains (config))
						info.AspNetConfigurations.Add (config);
				}
			}
		}

		// For WebSite projects
		// The main "Build" target:
		//	1. builds all non-website projects
		//	2. calls target for website project
		//		- gets target path for the referenced projects
		//		- Resolves dependencies, satellites etc for the
		//		  referenced project assemblies, and copies them
		//		  to bin/ folder
		void AddWebsiteTargets (Project p, Dictionary<Guid, ProjectInfo> websiteProjectInfos,
				Dictionary<Guid, ProjectInfo> projectInfos, List<ProjectInfo>[] infosByLevel,
				List<TargetInfo> solutionTargets)
		{
			foreach (ProjectInfo w_info in websiteProjectInfos.Values) {
				// gets a linear list of dependencies
				List<ProjectInfo> depInfos = new List<ProjectInfo> ();
				foreach (List<ProjectInfo> pinfos in infosByLevel) {
					foreach (ProjectInfo pinfo in pinfos)
						if (w_info.Dependencies.ContainsKey (pinfo.Guid))
							depInfos.Add (pinfo);
				}

				foreach (string buildTarget in new string [] {"Build", "Rebuild"})
					AddWebsiteTarget (p, w_info, projectInfos, depInfos, solutionTargets, buildTarget);

				// clean/publish are not supported for website projects
				foreach (string buildTarget in new string [] {"Clean", "Publish"})
					AddWebsiteUnsupportedTarget (p, w_info, depInfos, buildTarget);
			}
		}

		void AddWebsiteTarget (Project p, ProjectInfo webProjectInfo,
				Dictionary<Guid, ProjectInfo> projectInfos, List<ProjectInfo> depInfos,
				List<TargetInfo> solutionTargets, string buildTarget)
		{
			string w_guid = webProjectInfo.Guid.ToString ().ToUpper ();

			Target target = p.Targets.AddNewTarget (GetTargetNameForProject (webProjectInfo.Name, buildTarget));
			target.Condition = "'$(CurrentSolutionConfigurationContents)' != ''"; 
			target.DependsOnTargets = GetWebsiteDependsOnTarget (depInfos, buildTarget);

			// this item collects all the references
			string final_ref_item = String.Format ("Project_{0}_References{1}", w_guid,
							buildTarget != "Build" ? "_" + buildTarget : String.Empty);

			foreach (TargetInfo targetInfo in solutionTargets) {
				int ref_num = 0;
				foreach (ProjectInfo depInfo in depInfos) {
					TargetInfo projectTargetInfo;
					if (!depInfo.TargetMap.TryGetValue (targetInfo, out projectTargetInfo))
						// Ignore, no config, so no target path
						continue;

					// GetTargetPath from the referenced project
					AddWebsiteMSBuildTaskForReference (target, depInfo, projectTargetInfo, targetInfo,
							final_ref_item, ref_num);
					ref_num ++;
				}
			}

			// resolve the references
			AddWebsiteResolveAndCopyReferencesTasks (target, webProjectInfo, final_ref_item, w_guid);
		}

		// emits the MSBuild task to GetTargetPath for the referenced project
		void AddWebsiteMSBuildTaskForReference (Target target, ProjectInfo depInfo, TargetInfo projectTargetInfo,
				TargetInfo solutionTargetInfo, string final_ref_item, int ref_num)
		{
			BuildTask task = target.AddNewTask ("MSBuild");
			task.SetParameterValue ("Projects", depInfo.FileName);
			task.SetParameterValue ("Targets", "GetTargetPath");

			task.SetParameterValue ("Properties", string.Format ("Configuration={0}; Platform={1}; BuildingSolutionFile=true; CurrentSolutionConfigurationContents=$(CurrentSolutionConfigurationContents); SolutionDir=$(SolutionDir); SolutionExt=$(SolutionExt); SolutionFileName=$(SolutionFileName); SolutionName=$(SolutionName); SolutionPath=$(SolutionPath)", projectTargetInfo.Configuration, projectTargetInfo.Platform));
			task.Condition = string.Format (" ('$(Configuration)' == '{0}') and ('$(Platform)' == '{1}') ", solutionTargetInfo.Configuration, solutionTargetInfo.Platform);

			string ref_item = String.Format ("{0}_{1}",
						final_ref_item, ref_num); 

			task.AddOutputItem ("TargetOutputs", ref_item);

			task = target.AddNewTask ("CreateItem");
			task.SetParameterValue ("Include", String.Format ("@({0})", ref_item));
			task.SetParameterValue ("AdditionalMetadata", String.Format ("Guid={{{0}}}",
						depInfo.Guid.ToString ().ToUpper ()));
			task.AddOutputItem ("Include", final_ref_item);
		}

		void AddWebsiteResolveAndCopyReferencesTasks (Target target, ProjectInfo webProjectInfo,
				string final_ref_item, string w_guid)
		{
			BuildTask task = target.AddNewTask ("ResolveAssemblyReference");
			task.SetParameterValue ("Assemblies", String.Format ("@({0}->'%(FullPath)')", final_ref_item));
			task.SetParameterValue ("TargetFrameworkDirectories", "$(TargetFrameworkPath)");
			task.SetParameterValue ("SearchPaths", "{RawFileName};{TargetFrameworkDirectory};{GAC}");
			task.SetParameterValue ("FindDependencies", "true");
			task.SetParameterValue ("FindSatellites", "true");
			task.SetParameterValue ("FindRelatedFiles", "true");
			task.Condition = String.Format ("Exists ('%({0}.Identity)')", final_ref_item);

			string copylocal_item = String.Format ("{0}_CopyLocalFiles", final_ref_item);
			task.AddOutputItem ("CopyLocalFiles", copylocal_item);

			// Copy the references
			task = target.AddNewTask ("Copy");
			task.SetParameterValue ("SourceFiles", String.Format ("@({0})", copylocal_item));
			task.SetParameterValue ("DestinationFiles", String.Format (
						"@({0}->'$(Project_{1}_AspNetPhysicalPath)\\Bin\\%(DestinationSubDirectory)%(Filename)%(Extension)')",
						copylocal_item, w_guid));

			// AspNetConfiguration, is config for the website project, useful
			// for overriding from command line
			StringBuilder cond = new StringBuilder ();
			foreach (string config in webProjectInfo.AspNetConfigurations) {
				if (cond.Length > 0)
					cond.Append (" or ");
				cond.AppendFormat (" ('$(AspNetConfiguration)' == '{0}') ", config);
			}
			task.Condition = cond.ToString ();

			task = target.AddNewTask ("Message");
			cond = new StringBuilder ();
			foreach (string config in webProjectInfo.AspNetConfigurations) {
				if (cond.Length > 0)
					cond.Append (" and ");
				cond.AppendFormat (" ('$(AspNetConfiguration)' != '{0}') ", config);
			}
			task.Condition = cond.ToString ();
			task.SetParameterValue ("Text", "Skipping as the '$(AspNetConfiguration)' configuration is " +
						"not supported by this website project.");
		}

		void AddWebsiteUnsupportedTarget (Project p, ProjectInfo webProjectInfo, List<ProjectInfo> depInfos,
				string buildTarget)
		{
			Target target = p.Targets.AddNewTarget (GetTargetNameForProject (webProjectInfo.Name, buildTarget));
			target.DependsOnTargets = GetWebsiteDependsOnTarget (depInfos, buildTarget);

			BuildTask task = target.AddNewTask ("Message");
			task.SetParameterValue ("Text", String.Format (
						"Target '{0}' not support for website projects", buildTarget));
		}

		string GetWebsiteDependsOnTarget (List<ProjectInfo> depInfos, string buildTarget)
		{
			StringBuilder deps = new StringBuilder ();
			foreach (ProjectInfo pinfo in depInfos) {
				if (deps.Length > 0)
					deps.Append (";");
				deps.Append (GetTargetNameForProject (pinfo.Name, buildTarget));
			}
			deps.Append (";GetFrameworkPath");
			return deps.ToString ();
		}

		void AddGetFrameworkPathTarget (Project p)
		{
			Target t = p.Targets.AddNewTarget ("GetFrameworkPath");
			BuildTask task = t.AddNewTask ("GetFrameworkPath");
			task.AddOutputProperty ("Path", "TargetFrameworkPath");
		}

		void AddValidateSolutionConfiguration (Project p)
		{
			Target t = p.Targets.AddNewTarget ("ValidateSolutionConfiguration");
			BuildTask task = t.AddNewTask ("Warning");
			task.SetParameterValue ("Text", "On windows, an environment variable 'Platform' is set to MCD sometimes, and this overrides the Platform property" +
						" for xbuild, which could be an invalid Platform for this solution file. And so you are getting the following error." +
						" You could override it by either setting the environment variable to nothing, as\n" +
						"   set Platform=\n" +
						"Or explicity specify its value on the command line, as\n" +
						"   xbuild Foo.sln /p:Platform=Release");
			task.Condition = "('$(CurrentSolutionConfigurationContents)' == '') and ('$(SkipInvalidConfigurations)' != 'true')" +
					" and '$(Platform)' == 'MCD' and '$(OS)' == 'Windows_NT'";

			task = t.AddNewTask ("Error");
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
					string target_name = GetTargetNameForProject (project.Name, buildTarget);
					Target target = p.Targets.AddNewTarget (target_name);
					target.Condition = "'$(CurrentSolutionConfigurationContents)' != ''"; 
					if (project.Dependencies.Count > 0)
						target.DependsOnTargets = String.Join (";",
								project.Dependencies.Values.Select (
									di => GetTargetNameForProject (di.Name, buildTarget)).ToArray ());

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
							task.SetParameterValue ("ToolsVersion", "$(ProjectToolsVersion)");

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


		string GetTargetNameForProject (string projectName, string buildTarget)
		{
			//FIXME: hack
			projectName = projectName.Replace ("\\", "/").Replace (".", "_");
			string target_name = projectName +
					(buildTarget == "Build" ? string.Empty : ":" + buildTarget);

			if (IsBuildTargetName (projectName))
				target_name = "Solution:" + target_name;

			return target_name;
		}

		bool IsBuildTargetName (string name)
		{
			foreach (string tgt in buildTargets)
				if (name == tgt)
					return true;
			return false;
		}

		// returns number of levels
		int AddBuildLevels (Project p, List<TargetInfo> solutionTargets, Dictionary<Guid, ProjectInfo> projectInfos,
				ref List<ProjectInfo>[] infosByLevel)
		{
			infosByLevel = TopologicalSort<ProjectInfo> (projectInfos.Values);

			foreach (TargetInfo targetInfo in solutionTargets) {
				BuildItemGroup big = p.AddNewItemGroup ();
				big.Condition = String.Format (" ('$(Configuration)' == '{0}') and ('$(Platform)' == '{1}') ",
						targetInfo.Configuration, targetInfo.Platform);

				//FIXME: every level has projects that can be built in parallel.
				//	 levels are ordered on the basis of the dependency graph

				for (int i = 0; i < infosByLevel.Length; i ++) {
					string build_level = String.Format ("BuildLevel{0}", i);
					string skip_level = String.Format ("SkipLevel{0}", i);
					string missing_level = String.Format ("MissingConfigLevel{0}", i);

					foreach (ProjectInfo projectInfo in infosByLevel [i]) {
						TargetInfo projectTargetInfo;
						if (!projectInfo.TargetMap.TryGetValue (targetInfo, out projectTargetInfo)) {
							// missing project config
							big.AddNewItem (missing_level, projectInfo.Name);
							continue;
						}

						if (projectTargetInfo.Build) {
							BuildItem item = big.AddNewItem (build_level, projectInfo.FileName);
							item.SetMetadata ("Configuration", projectTargetInfo.Configuration);
							item.SetMetadata ("Platform", projectTargetInfo.Platform);
						} else {
							// build disabled
							big.AddNewItem (skip_level, projectInfo.Name);
						}
					}
				}
			}

			return infosByLevel.Length;
		}

		void AddSolutionTargets (Project p, int num_levels, IEnumerable<ProjectInfo> websiteProjectInfos)
		{
			foreach (string buildTarget in buildTargets) {
				Target t = p.Targets.AddNewTarget (buildTarget);
				t.Condition = "'$(CurrentSolutionConfigurationContents)' != ''";

				BuildTask task = null;
				for (int i = 0; i < num_levels; i ++) {
					string level_str = String.Format ("BuildLevel{0}", i);
					task = t.AddNewTask ("MSBuild");
					task.SetParameterValue ("Condition", String.Format ("'@({0})' != ''", level_str));
					task.SetParameterValue ("Projects", String.Format ("@({0})", level_str));
					task.SetParameterValue ("ToolsVersion", "$(ProjectToolsVersion)");
					task.SetParameterValue ("Properties",
						string.Format ("Configuration=%(Configuration); Platform=%(Platform); BuildingSolutionFile=true; CurrentSolutionConfigurationContents=$(CurrentSolutionConfigurationContents); SolutionDir=$(SolutionDir); SolutionExt=$(SolutionExt); SolutionFileName=$(SolutionFileName); SolutionName=$(SolutionName); SolutionPath=$(SolutionPath)"));
					if (buildTarget != "Build")
						task.SetParameterValue ("Targets", buildTarget);
					//FIXME: change this to BuildInParallel=true, when parallel
					//	 build support gets added
					task.SetParameterValue ("RunEachTargetSeparately", "true");

					level_str = String.Format ("SkipLevel{0}", i);
					task = t.AddNewTask ("Message");
					task.Condition = String.Format ("'@({0})' != ''", level_str);
					task.SetParameterValue ("Text",
						String.Format ("The project '%({0}.Identity)' is disabled for solution " +
							"configuration '$(Configuration)|$(Platform)'.", level_str));

					level_str = String.Format ("MissingConfigLevel{0}", i);
					task = t.AddNewTask ("Warning");
					task.Condition = String.Format ("'@({0})' != ''", level_str);
					task.SetParameterValue ("Text",
						String.Format ("The project configuration for project '%({0}.Identity)' " +
							"corresponding to the solution configuration " +
							"'$(Configuration)|$(Platform)' was not found.", level_str));
				}

				// "build" website projects also
				StringBuilder w_targets = new StringBuilder ();
				foreach (ProjectInfo info in websiteProjectInfos) {
					if (w_targets.Length > 0)
						w_targets.Append (";");
					w_targets.Append (GetTargetNameForProject (info.Name, buildTarget));
				}

				task = t.AddNewTask ("CallTarget");
				task.SetParameterValue ("Targets", w_targets.ToString ());
				task.SetParameterValue ("RunEachTargetSeparately", "true");
			}
		}

		// Sorts the ProjectInfo dependency graph, to obtain
		// a series of build levels with projects. Projects
		// in each level can be run parallel (no inter-dependency).
		static List<T>[] TopologicalSort<T> (IEnumerable<T> items) where T: ProjectInfo
		{
			IList<T> allItems;
			allItems = items as IList<T>;
			if (allItems == null)
				allItems = new List<T> (items);

			bool[] inserted = new bool[allItems.Count];
			bool[] triedToInsert = new bool[allItems.Count];
			int[] levels = new int [allItems.Count];

			int maxdepth = 0;
			for (int i = 0; i < allItems.Count; ++i) {
				int d = Insert<T> (i, allItems, levels, inserted, triedToInsert);
				if (d > maxdepth)
					maxdepth = d;
			}

			// Separate out the project infos by build level
			List<T>[] infosByLevel = new List<T>[maxdepth];
			for (int i = 0; i < levels.Length; i ++) {
				int level = levels [i] - 1;
				if (infosByLevel [level] == null)
					infosByLevel [level] = new List<T> ();

				infosByLevel [level].Add (allItems [i]);
			}

			return infosByLevel;
		}

		// returns level# for the project
		static int Insert<T> (int index, IList<T> allItems, int[] levels, bool[] inserted, bool[] triedToInsert)
			where T: ProjectInfo
		{
			if (inserted [index])
				return levels [index];

			if (triedToInsert[index])
				throw new InvalidOperationException (String.Format (
						"Cyclic dependency involving project {0} found in the project dependency graph",
						allItems [index].Name));

			triedToInsert[index] = true;
			ProjectInfo insertItem = allItems[index];

			int maxdepth = 0;
			foreach (ProjectInfo dependency in insertItem.Dependencies.Values) {
				for (int j = 0; j < allItems.Count; ++j) {
					ProjectInfo checkItem = allItems [j];
					if (dependency.FileName == checkItem.FileName) {
						int d = Insert (j, allItems, levels, inserted, triedToInsert);
						maxdepth = d > maxdepth ? d : maxdepth;
						break;
					}
				}
			}
			levels [index] = maxdepth + 1;
			inserted [index] = true;

			return levels [index];
		}

		public static IEnumerable<string> GetAllProjectFileNames (string solutionFile)
		{
			StreamReader reader = new StreamReader (solutionFile);
			string line = reader.ReadToEnd ();
			line = line.Replace ("\r\n", "\n");
			string soln_dir = Path.GetDirectoryName (solutionFile);

			Match m = projectRegex.Match (line);
			while (m.Success) {
				if (String.Compare (m.Groups [1].Value, solutionFolderGuid,
						StringComparison.InvariantCultureIgnoreCase) != 0)
					yield return Path.Combine (soln_dir, m.Groups [3].Value).Replace ("\\", "/");

				m = m.NextMatch ();
			}
		}
	}
}

#endif
