// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace ThemeTests
{
    using FlaUI.Core;
    using FlaUI.Core.AutomationElements;

    using Xunit.Abstractions;

    public class BaseThemeTest : BaseTest
    {
        private readonly Logger logger;
        private readonly ThemeTestFixture fixture;

        protected Logger Logger => this.logger;
        protected Application App => this.fixture.App;

        public BaseThemeTest(ThemeTestFixture fixture, ITestOutputHelper outputHelper)
        {
            this.fixture = fixture;
            this.logger = new Logger(this.App.GetMainWindow(BaseTest.Automation), this.GetType().Name, outputHelper);
        }

        protected Logger.Scenario Scenario(string description, AutomationElement element = null) => this.logger.RunScenario(description, element);
    }
}
