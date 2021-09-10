// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace ThemeTests
{
    using FlaUI.Core;
    using FlaUI.Core.Input;

    using Microsoft.VisualStudio.Setup.Configuration;

    using System;
    using System.IO;

    using Xunit.Abstractions;
    using Xunit.Sdk;

    public abstract class ThemeTestFixture : IDisposable
    {
        private readonly double storedMovePixelsPerMillisecond;
        private readonly Application app;

        public Application App => this.app;

        public ThemeTestFixture(IMessageSink messageSink)
        {
            string devenvPath = GetPathToTargetVisualStudioInstall(messageSink);
            this.app = Application.Launch(devenvPath);
            this.storedMovePixelsPerMillisecond = Mouse.MovePixelsPerMillisecond;
            Mouse.MovePixelsPerMillisecond = 10;
        }

        public static string GetPathToTargetVisualStudioInstall(IMessageSink messageSink)
        {
            string vsInstallDir = Environment.GetEnvironmentVariable("VSINSTALLDIR");
            string reason = string.Empty;

            if (!string.IsNullOrEmpty(vsInstallDir) && Directory.Exists(vsInstallDir))
            {
                reason = "VSINSTALLDIR environment variable.";
            }
            else
            {
                var setupConfig = new SetupConfiguration();
                var setupInstances = setupConfig.EnumAllInstances();
                var instances = new ISetupInstance[1];

                setupInstances.Next(instances.Length, instances, out int fetched);

                if (fetched != 1)
                {
                    throw new Exception("Could not find a VS install to target");
                }

                vsInstallDir = instances[0].GetInstallationPath();
                reason = $"instance ID {instances[0].GetInstanceId()} being first in SetupConfiguration.";
                if (!Directory.Exists(vsInstallDir))
                {
                    throw new Exception($"Could not find devenv.exe at {vsInstallDir}");
                }
            }

            var devenvPath = Path.Combine(vsInstallDir, @"Common7\IDE\devenv.exe");
            messageSink.OnMessage(new DiagnosticMessage($"Targeting {devenvPath} by way of {reason}"));
            return devenvPath;
        }

        public void Dispose()
        {
            _ = this.app?.Close();
            this.app?.Dispose();
            Mouse.MovePixelsPerMillisecond = this.storedMovePixelsPerMillisecond;
        }

        protected virtual void DisposeInternal()
        {
        }
    }
}
