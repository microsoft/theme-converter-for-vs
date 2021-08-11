# ThemeTests README

This project provides some basic infrastructure to make testing theme changes across UI surface area less tedious and more consistent.

To run tests - build the project, then choose one or more tests or feature areas to capture UI for from Test Explorer.

Each test will drive the UI and take screenshots of UI in various states. This uses UIA and keyboard/mouse emulation - so try not to multitask on the machine while the tests are running.

When the tests are done, there will be a link to the output folder in the Test Explorer summary pane. You'll find a variety of screenshots for the scenarios you choose to run, and can look through these to make sure your changes look good or haven't regressed.