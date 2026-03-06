# Contributing to Terminal.Gui

We welcome contributions from the community.

## Forking and Submitting Changes

## Terminal.Gui Coding Style

**Terminal.Gui** follows the [Mono Coding Guidelines](https://www.mono-project.com/community/contributing/coding-guidelines/). 
[`/.editorconfig`](https://github.com/gui-cs/Terminal.Gui/blob/b0a43ba338adf5ec069066e5a7dff8fea39b41db/.editorconfig) enforces this style in Visual Studio. 
Use `Ctrl-K-D` in Visual Studio to have it reformat code.

## User Experience Tenets

**Terminal.Gui**, as a UI framework, heavily influences how console graphical user interfaces (GUIs) work. We use the following [tenets](https://ceklog.kindel.com/2020/02/10/tenets/) to guide us:

*NOTE: Like all tenets, these are up for debate. If you disagree, have questions, or suggestions about these tenets and guidelines submit an Issue using the [design](https://github.com/gui-cs/Terminal.Gui/issues?q=is%3Aopen+is%3Aissue+label%3Adesign) tag.*

1. **Honor What's Come Before**. The Mac and Windows OS's have well-established GUI idioms that are mostly consistent. We adhere to these versus inventing new ways for users to do things. For example, **Terminal.Gui** adopts the `ctrl/command-c`, `ctrl/command-v`, and `ctrl/command-x` keyboard shortcuts for cut, copy, and paste versus defining new shortcuts.
2. **Consistency Matters**. Common UI idioms should be consistent across the GUI framework. For example, `ctrl/command-q` quits/exits all modal views. See [Issue #456](https://github.com/gui-cs/Terminal.Gui/issues/456) as a counter-example that should be fixed.
3. **Honor the OS, but Work Everywhere**. **Terminal.Gui** is cross-platform, but we support taking advantage of a platform's unique advantages. For example, the Windows Console API is richer than the Unix API in terms of keyboard handling. Thus, in Windows pressing the `alt` key in a **Terminal.Gui** app will activate the `MenuBar`, but in Unix, the user has to press the full hotkey (e.g. `alt-f`) or `F9`. 
4. **Keyboard first, Mouse also**. Users use consoles primarily with the keyboard; **Terminal.Gui** is optimized for getting stuff done without using the Mouse. However, as a GUI framework, the Mouse is essential thus we strive to ensure that everything also works via the Mouse.

## Public API Tenets & Guidelines

## Breaking Changes to User Behavior or the Public API

## Unit Tests

## Sample Code

[UI Catalog](./UICatalog) is a great sample app for manual testing.

When adding new functionality, fixing bugs, or changing things, please either add a new `Scenario` to **UICatalog** or update an existing `Scenario` to fully illustrate your work and provide a test-case.
