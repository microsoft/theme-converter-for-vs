// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

using FlaUI.Core.Tools;
using FlaUI.UIA3;

using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace ThemeTests
{
    public class BaseTest
    {
        public static readonly UIA3Automation Automation = new()
        {
            TransactionTimeout = TimeSpan.FromMinutes(5),
            ConnectionTimeout = TimeSpan.FromMinutes(5)
        };

        static BaseTest()
        {
            Retry.DefaultInterval = TimeSpan.FromMilliseconds(100);
            Retry.DefaultTimeout = TimeSpan.FromMinutes(5);
        }

    }
}
