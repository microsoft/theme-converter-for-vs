// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace ThemeTests
{
    using FlaUI.Core.AutomationElements;
    using FlaUI.Core.Capturing;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    using Xunit.Abstractions;

    public class Logger
    {
        private readonly Stack<Scenario> scenarios;
        private int count;
        private AutomationElement rootElement;
        private string outputPath;
        private readonly ITestOutputHelper outputHelper;
        private string namePrefix;
        private bool diagnostic = false; // Toggle this to true to get more verbose logging
        private static readonly string RootDirectoryForRun = Path.Combine(Directory.GetCurrentDirectory(), "TestResults", DateTime.Now.ToString("yyyy-dd-MM-HHmmss"));

        public AutomationElement RootElement { get => this.rootElement; set => this.rootElement = value; }

        public Logger(AutomationElement rootScope, string scopeName, ITestOutputHelper outputHelper)
        {
            this.scenarios = new Stack<Scenario>();
            this.rootElement = rootScope;
            this.outputHelper = outputHelper;
            this.outputPath = RootDirectoryForRun;
            this.namePrefix = scopeName + ".";
            this.outputHelper.WriteLine($"INFO: Logging to {new Uri(outputPath, UriKind.Absolute).ToString()}");
            if (!Directory.Exists(this.outputPath))
            {
                Directory.CreateDirectory(this.outputPath);
            }
        }

        public Scenario RunScenario([CallerMemberName] string name = null, AutomationElement element = null, bool captureOnDispose = false)
        {
            this.namePrefix += $"{name}.";
            var scenario = new Scenario(this, name, element, captureOnDispose);
            this.scenarios.Push(scenario);
            return scenario;
        }

        public void WriteInfo(string error) => this.outputHelper.WriteLine($"INFO: {error}");
        public void WriteError(string error) => this.outputHelper.WriteLine($"ERROR: {error}");
        public void WriteDiagnostic(string error)
        {
            if (this.diagnostic)
                this.outputHelper.WriteLine($"DIAG: {error}");
        }

        public class Scenario : IDisposable
        {
            private readonly bool captureOnDispose;

            public string Name { get; }
            public AutomationElement Element { get; set; }
            public Logger Scope { get; }

            public Scenario(Logger scope, string name, AutomationElement element = null, bool captureOnDispose = false)
            {
                this.Scope = scope;
                this.Name = name;
                this.Element = element;
                this.captureOnDispose = captureOnDispose;
            }

            public void Snapshot(string description, AutomationElement scopedElement = null)
            {
                this.DoCapture(description, scopedElement);
            }

            public void Dispose()
            {
                if (this.Scope.scenarios.Peek() != this)
                    throw new Exception();

                if (this.captureOnDispose)
                    this.DoCapture("ScenarioEnd");

                var disposedScenario = this.Scope.scenarios.Pop();
                this.Scope.namePrefix = this.Scope.namePrefix.Substring(0, this.Scope.namePrefix.Length - (disposedScenario.Name.Length + 1));
            }

            private void DoCapture(string description, AutomationElement scopedElement = null)
            {
                try
                {
                    var filename = $"{this.Scope.namePrefix}{this.Scope.count++}.{description}.png";
                    var filepath = Path.Combine(this.Scope.outputPath, filename);
                    var image = Capture.Element(scopedElement ?? this.Element ?? this.Scope.rootElement);
                    this.Scope.WriteDiagnostic($"URI test: {new Uri(filepath, UriKind.Absolute).ToString()}");
                    image.ToFile(filepath);
                }
                catch (ExternalException e)
                {
                    this.Scope.WriteError(e.ToString());
                }
            }
        }
    }
}
