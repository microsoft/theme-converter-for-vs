// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace ThemeTests
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    using FlaUI.Core;
    using FlaUI.Core.AutomationElements;
    using FlaUI.Core.Conditions;
    using FlaUI.Core.Input;
    using FlaUI.Core.Tools;
    using FlaUI.Core.WindowsAPI;
    using FlaUI.UIA3;

    static class AllExtensions
    {
        /// <summary>
        /// Closes the given window and invokes the "Don't save" button
        /// </summary>
        public static void CloseWindowWithDontSave(this Window window)
        {
            window.Close();
            var modalWindows = Retry.WhileEmpty(() => window.ModalWindows).Result;
            var dontSaveButton = Retry.WhileNull(() => modalWindows[0].FindFirstDescendant(cf => cf.ByAutomationId("7")).AsButton()).Result;
            dontSaveButton.Invoke();
        }

        public static void HoverButton(this Window getToCodeWindow, string elementName)
        {
            var newProjButton = Retry.WhileNull(() => getToCodeWindow.FindFirstDescendant((cf) => cf.ByName(elementName))).Result;
            newProjButton.MoveToElement();
        }

        public static Button FocusButton(this Window window, string elementName)
        {
            var element = Retry.WhileNull(() => window.FindFirstDescendant(cf => cf.ByName(elementName))).Result;
            element.FocusNative();
            return element.AsButton();
        }

        public static AutomationElement ByName(this AutomationElement element, string name)
        {
            return Retry.WhileNull(() => element.FindFirstDescendant(cf => cf.ByName(name))).Result;
        }

        public static void HoldDown(this Window window, string elementName)
        {
            var element = window.ByName(elementName);
            Mouse.MoveTo(element.GetCenter());
            Mouse.Down();
        }

        public static Point GetCenter(this AutomationElement element)
        {
            var bounds = element.BoundingRectangle;
            var center = new Point(bounds.X + (bounds.Width / 2), bounds.Y + (bounds.Height / 2));
            return center;
        }

        public static void MoveToElement(this AutomationElement element)
        {
            Mouse.MoveTo(element.GetCenter());
        }

        public static void PressNewProjectDialogButton(this Window getToCodeWindow)
        {
            var newProjButton = Retry.WhileNull(() => getToCodeWindow.FindFirstDescendant((cf) => cf.ByName("Create a _new project"))).Result.AsButton();
            newProjButton.Invoke();
        }

        public static void PressOpenExistingProjectButton(this Window getToCodeWindow)
        {
            var openProjectButton = Retry.WhileNull(() => getToCodeWindow.FindFirstDescendant(cf => cf.ByName("Open a _project or solution"))).Result.AsButton();
            openProjectButton.Invoke();
        }

        public static void PressContinueWithoutCodeButton(this Window getToCodeWindow)
        {
            var button = Retry.WhileNull(() => getToCodeWindow.FindFirstByXPath("//Button[@Name='Continue without code']")).Result.AsButton();
            button.Click();
        }

        public static void CreateNewProject(this Window getToCodeWindow, string templateName = "Console Application", params string[] tags)
        {
            var templateList = Retry.WhileNull(() => getToCodeWindow.FindFirstDescendant("ListViewTemplates").AsListBox()).Result;
            // Wait for at least 1 template to appear...
            _ = Retry.WhileNull(() => templateList.FindFirstChild()).Result;
            // Check if template we are looking for appeared...
            var consoleItem = GetTemplate(templateList, templateName, tags);
            if (consoleItem == null)
            {
                Keyboard.TypeSimultaneously(VirtualKeyShort.ALT, VirtualKeyShort.KEY_S);
                Keyboard.Type(templateName);
                consoleItem = Retry.WhileNull(() => GetTemplate(templateList, templateName, tags)).Result;
            }
            consoleItem.DoubleClick();
            var nextButton = Retry.WhileNull(() => getToCodeWindow.FindFirstDescendant("button_Next")).Result.AsButton();
            nextButton.Invoke();
            nextButton = Retry.WhileNull(() => getToCodeWindow.FindFirstDescendant("button_Next")).Result.AsButton();
            _ = Retry.WhileFalse(() => nextButton.IsEnabled);
            nextButton.Invoke();
        }

        public static void OpenProject(this Window openProjectDialog, string projectPath)
        {
            var filePathInput = Retry.WhileNull(() =>
                openProjectDialog.FindFirstDescendant(
                    new AndCondition(
                        ConditionFactory.ByControlType(FlaUI.Core.Definitions.ControlType.Edit),
                        ConditionFactory.ByText("File name:")))
                        .AsTextBox())
                        .Result;
            filePathInput.Text = projectPath;
            var okButton = Retry.WhileNull(
                () => openProjectDialog.FindFirstChild(ConditionFactory.ByName("Open")).AsButton()).Result;
            okButton.Invoke();
        }

        private static AutomationElement GetTemplate(ListBox templateList, string templateName, string[] tags)
        {
            foreach (var child in templateList.FindAllChildren((cf) => cf.ByName(templateName)))
            {
                bool missingTag = false;
                foreach (var tag in tags)
                {
                    if (child.FindFirstDescendant((c) => c.ByAutomationId("TextBlock_1018").And(c.ByName(tag))) == null)
                    {
                        missingTag = true;
                        break;
                    }
                }
                if (missingTag == false)
                {
                    return child;
                }
            }
            return null;
        }

        public static void ClickOnText(this TextBox editor, string text)
        {
            var doc = editor.Patterns.Text.Pattern.DocumentRange;
            var textRange = doc.FindText(text, false, false);
            textRange.ScrollIntoView(false);
            Mouse.Click(textRange.GetBoundingRectangles()[0].Center());
        }

        public static TreeItem OpenFile(this Tree solutionExplorer, params string[] children)
        {
            var file = SelectFile(solutionExplorer, children);
            Keyboard.Type(VirtualKeyShort.RETURN);
            return file;
        }

        public static TreeItem SelectFile(this Tree solutionExplorer, params string[] children)
        {
            var file = solutionExplorer.FindFirstChild().AsTreeItem().GetChildTreeItem(children);
            file.Patterns.ScrollItem.Pattern.ScrollIntoView();
            file.Select();
            return file;
        }

        public static void OpenSolution(this Application app, string solutionRelativeFilePath)
        {
            var mainWindow = app.GetMainVSWindow();
            var testFilesFolderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestFiles");
            var solutionFilePath = Path.Combine(testFilesFolderPath, solutionRelativeFilePath);
            var fileMenuItem = mainWindow.FindFirstByXPath("//MenuItem[@Name='File']").AsMenuItem();
            var openMenuItem = fileMenuItem.Items.Single(x => x.Name == "Open").AsMenuItem();
            var projectSolutionMenuItem = openMenuItem.Items.Single(x => x.Name == "Project/Solution...").AsMenuItem();
            _ = projectSolutionMenuItem.Invoke();

            var openDialog = app.GetFileOpenDialog();
            openDialog.OpenProject(solutionFilePath);

            // Maybe not the most correct, but works well enough for now
            app.WaitWhileBusy(TimeSpan.FromSeconds(5));
            app.WaitUntilCanStartDebugging();
        }

        public static void CloseSolution(this Application app)
        {
            var mainWindow = app.GetMainVSWindow();
            var fileMenuItem = mainWindow.FindFirstByXPath("//MenuItem[@Name='File']").AsMenuItem();
            var closeSolutionMenuItem = fileMenuItem.Items.Single(x => x.Name == "Close Solution").AsMenuItem();
            _ = closeSolutionMenuItem.Invoke();
            app.WaitWhileBusy(TimeSpan.FromSeconds(3));

            var window = app.GetGetToCodeWindow();
            Keyboard.Type(VirtualKeyShort.ESC);
        }

        public static bool CanStartDebugging(this Application app)
        {
            return app.GetMainVSWindow()?.FindFirstDescendant((c) => c.ByAutomationId("PART_FocusTarget").And(c.ByName("Debug Target")))?.IsEnabled ?? false;
        }

        public static void WaitUntilCanStartDebugging(this Application app)
        {
            Retry.WhileFalse(() => CanStartDebugging(app));
        }

        public static void WaitUntilSolutionFullyLoaded(this TextBox editor)
        {
            // editor.Parent => WpfTextViewHost
            Retry.WhileTrue(() => (editor.Parent.FindFirstDescendant("ProjectsList")?.AsComboBox().SelectedItem?.Text ?? "Miscellaneous Files") == "Miscellaneous Files");
        }

        public static bool IsDebugging(this Application app)
        {
            return app.GetMainVSWindow()?.FindFirstDescendant("PART_FocusTarget")?.IsEnabled ?? false;
        }

        public static void WaitUntilBreakpointHit(this TextBox editor)
        {
            // editor.Parent => WpfTextViewHost
            Retry.WhileNull(() => editor.Parent.FindFirstDescendant("WpfEditorUIGlyphMarginGrid")?.FindFirstDescendant(c => c.ByName("Current Statement")));
        }

        public static void WaitUntilExited(this Application app)
        {
            // Make sure that by time we exit, devenv.exe also exited...
            // So PerfView can capture it all...
            while (!app.HasExited)
            {
                Thread.Sleep(100);
            }
            // And lets add another second just to be sure
            Thread.Sleep(1000);
        }

        public static Window GetGetToCodeWindow(this Application app)
        {
            return GetWindow(app, "WorkflowHostView");
        }

        public static void OpenSolutionExplorer(this Application app)
        {
            var mainWindow = app.GetMainVSWindow();
            var viewMenuItem = mainWindow.FindFirstByXPath("//MenuItem[@Name='View']").AsMenuItem();
            var solutionExplorerMenuItem = viewMenuItem.Items.Single(x => x.Name == "Solution Explorer").AsMenuItem();
            _ = solutionExplorerMenuItem.Invoke();
        }

        public static Tree GetSolutionExplorer(this Application app)
        {
            return Retry.WhileNull(() => app.GetMainVSWindow().FindFirstDescendant("SolutionExplorer")).Result.AsTree();
        }

        public static AutomationElement GetSolutionExplorerPane(this Application app)
        {
            return Retry.WhileNull(() =>
                app.GetMainVSWindow().FindFirstDescendant(
                    new AndCondition(
                        ConditionFactory.ByControlType(FlaUI.Core.Definitions.ControlType.Pane),
                        ConditionFactory.ByName("Solution Explorer")))).Result;
        }

        public static TextBox GetTextEditor(this Application app)
        {
            return Retry.WhileNull(() => app.GetMainVSWindow().FindFirstDescendant("WpfTextView")).Result.AsTextBox();
        }

        public static TextBox GetCtrlQSearchBox(this Application app)
        {
            return Retry.WhileNull(() => app.GetMainVSWindow().FindFirstDescendant("SearchBox")).Result.AsTextBox();
        }

        public static void CloseAllTabs(this Application app)
        {
            var mainWindow = app.GetMainVSWindow();
            var windowMenu = mainWindow.FindFirstDescendant(cf => cf.ByName("Window").And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.MenuItem))).AsMenuItem();
            var closeAllTabsMenuItem = windowMenu.Items.Single(x => x.Name == "Close All Tabs").AsMenuItem();
            _ = closeAllTabsMenuItem.Invoke();
        }

        public static ITextRange FindText(this TextBox editor, string text)
        {
            return editor.Patterns.Text.Pattern.DocumentRange.FindText(text, false, false);
        }

        public static TreeItem GetChildTreeItem(this TreeItem rootItem, params string[] children)
        {
            if (children.Length == 0)
                return rootItem;
            rootItem.Expand();
            return GetChildTreeItem(Retry.WhileNull(() => rootItem.Items.FirstOrDefault(i => i.Name == children[0])).Result, children.Skip(1).ToArray());
        }

        private static Window GetWindow(Application app, string automationId, TimeSpan? timeout = null)
        {
            Window mainWindow;
            var sw = Stopwatch.StartNew();
            while (true)
            {
                //TODO: Timeout, consider using Retry
                try
                {
                    mainWindow = app.GetMainWindow(BaseTest.Automation);
                    if (mainWindow.ControlType == FlaUI.Core.Definitions.ControlType.Window && mainWindow.AutomationId == automationId)
                    {
                        break;
                    }

                    if (mainWindow.FindFirstChild(automationId).AsWindow() is { } childWindow)
                    {
                        mainWindow = childWindow;
                        break;
                    }
                }
                catch
                {
                    //Sometimes things go wrong here, probably when splash window closes
                }
                Thread.Sleep(10);
                if (timeout is TimeSpan { } timeOut)
                {
                    if (sw.Elapsed > timeout)
                        return null;
                }
            }

            return mainWindow;
        }

        public static Window GetMainVSWindow(this Application app, TimeSpan? timeout = null)
        {
            return GetWindow(app, "VisualStudioMainWindow", timeout);
        }

        public static Window GetOptionsDialogWindow(this Application app)
            => Retry.WhileNull(() => app.GetMainVSWindow().ModalWindows.Where(tlw => tlw.Name == "Options").Single()).Result.AsWindow();

        public static Window GetPopupWindow(this Application app, TimeSpan? timeout = null)
        {
            return Retry.WhileNull(() => app.GetAllTopLevelWindows(BaseTest.Automation).FirstOrDefault((w) => w.ClassName == "Popup"), timeout).Result;
        }


        public static Window GetFileOpenDialog(this Application app)
        {
            return Retry.WhileNull(() => app.GetMainWindow(BaseTest.Automation)
                .AsWindow()
                .FindFirstChild(ConditionFactory.ByName("Open Project/Solution"))).Result.AsWindow();
        }



        public static readonly ConditionFactory ConditionFactory = new(new UIA3PropertyLibrary());
    }
}
