# Description

This test plan is for verifying whether a theme converted by [Theme-Converter](https://github.com/microsoft/theme-converter/tree/main/ThemeConverter/ThemeConverter) will severely break the readability on common user workflows.
For each step, observe if the UI is readable under default/selected/hovered situation, when the window has focus and doesn't have focus.

# Prerequisites

VS 2022 Preview 3 or later, with the theme to be tested installed.
VS Code *(optional)*

### Note

Scenarios marked with **VS Code compare** in the title should be compared with VS Code to see if there's any obvious color mismatch.

# Scenarios

## Scenario 1: Create a new project from the Start Window

1. Open the Start Window: navigate to File -> Start Window; select the Start Window icon from the toolbar; 
2. Select "Create a new project"
3. Create a new project using an available template

## Scenario 2: Editing a file (VS Code Compare)

1. Open different types of files ([sample files](https://github.com/kai-oswald/NightOwl-VS-Theme/tree/master/demo))

## Scenario 3: Debugging (VS Code Compare)

1. Open the project created earlier and place a breakpoint
2. Start debugging
3. Open different debug windows

### VS Code instruction:

1. Open the project's folder (File > Open Folder...)
2. Place a breakpoint at the same location
3. Select Run > Start Debugging 

## Scenario 4: Install extension from extension manager

1. Open Extensions > Manage Extensions
2. Open different pages and try to scroll/select items

## Scenario 5: Run/debug unit tests

1. Create a unit test project with some basic test methods
2. Open Test > Test Explorer
3. Run/Debug/Select tests.

## Scenario 6: Solution Explorer (VS Code Compare)

1. Open a project and open the solution explorer
2. Try selecting/right clicking
3. Do a search

### VS Code instruction:

1. The corresponding page is the File Explorer 
 
![image](https://user-images.githubusercontent.com/14095891/129639337-f78e5b53-bcce-4ee5-a885-fa15cb390f19.png)

2. The search window will be invoked by 'Ctrl + T`

## Scenario 7: Version Control (VS Code Compare)

1. Clone a git repo
2. Open View > Git Changes
3. Edit some files and check the window

### VS Code instruction:

1. Corresponding page is the Source Control window:

![image](https://user-images.githubusercontent.com/14095891/129639468-eafe2687-1284-42cf-b3c7-0e4e0da52ec2.png)
