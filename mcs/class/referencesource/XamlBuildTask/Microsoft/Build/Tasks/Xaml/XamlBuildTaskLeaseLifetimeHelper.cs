namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Remoting.Lifetime;
    using XamlBuildTask;

    internal static class XamlBuildTaskLeaseLifetimeHelper
    {
        const string RemotingLeaseLifetimeInMinutesEnvironmentVariableName = "XamlBuildTaskTimeoutInMinutes";

        // In order to take advantage of the XamlBuildTaskRemotingLeaseLifetimeInMinutes environment variable from an MSBuild
        // project file (e.g. csproj file), the following needs to be added to that project file:
        //
        // After the initial "<Project ..." line:
        // <UsingTask TaskName="MySetEnv" TaskFactory="CodeTaskFactory" AssemblyFile="$(MSBuildToolsPath)\Microsoft.Build.Tasks.v4.0.dll" >
        //   <ParameterGroup>
        //     <Name Required="true" />
        //     <Value Required="false" />
        //   </ParameterGroup>
        //   <Task>
        //     <Code Type="Fragment" Language="cs">System.Environment.SetEnvironmentVariable(Name, Value);</Code>
        //   </Task>
        // </UsingTask>
        //
        // And at the end of the project file, before the closing </Project> :
        //
        // <Target Name="BeforeBuild">
        //   <MySetEnv Name="XamlBuildTaskTimeoutInMinutes" Value="24" />
        // </Target>
        // <Target Name="AfterBuild">
        //   <MySetEnv Name="XamlBuildTaskRemotingLeaseLifetimeInMinutes" Value="" />
        // </Target>
        //
        // This example uses a task name of "MySetEnv", but it that name could be anything desired.
        // It also sets the timeout to 24 minutes, as defined as the Value specified to the MySetEnv task.
        // The AfterBuild target is not required, but is probably desired so that the environment variable setting
        // does not persist after the processing of this particular project file.
        // The valid values for the environment variable are numbers between 1 and 2147483647 inclusive
        // (positive 32-bit integers). Any other value will result in no change to the lease lifetime.
        internal static void SetLeaseLifetimeFromEnvironmentVariable(ILease lease)
        {
            // We can only change the lease lifetime if we have an ILease and it is still in the Initial state.
            if ((lease != null) && (lease.CurrentState == LeaseState.Initial))
            {
                try
                {
                    string remotingLeaseLifetimeInMinutesStringValue = Environment.GetEnvironmentVariable(RemotingLeaseLifetimeInMinutesEnvironmentVariableName);
                    if (!string.IsNullOrEmpty(remotingLeaseLifetimeInMinutesStringValue))
                    {
                        int remotingLeaseLifetimeInMinutes = -1;
                        if (Int32.TryParse(remotingLeaseLifetimeInMinutesStringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out remotingLeaseLifetimeInMinutes))
                        {
                            // revert to the defauilt if the number specified is less than or equal to 0.
                            if (remotingLeaseLifetimeInMinutes > 0)
                            {
                                lease.InitialLeaseTime = TimeSpan.FromMinutes(remotingLeaseLifetimeInMinutes);
                                lease.RenewOnCallTime = TimeSpan.FromMinutes(remotingLeaseLifetimeInMinutes);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // simply ignore any exceptions that might have occurred and go with the default. We can't log it because
                    // we aren't initialized enough at this point.
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }
                }
            }
        }
    }
}

