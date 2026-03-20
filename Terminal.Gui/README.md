# Terminal.Gui Project

All files required to build the **Terminal.Gui** library (and NuGet package).

## Project Folder Structure

- `Terminal.Gui.sln` - The Visual Studio solution
- `Core/` - Source files for all types that comprise the core building blocks of **Terminal-Gui** 
    - `Application` - A `static` class that provides the base 'application driver'. Given it defines a **Terminal.Gui** application it is both logically and literally (because `static`) a singleton. It has direct dependencies on `MainLoop`, `Events.cs` `NetDriver`, `CursesDriver`, `WindowsDriver`, `Responder`, `View`, and `TopLevel` (and nothing else).
    - `MainLoop` - Defines `IMainLoopDriver` and implements the `MainLoop` class.
    - `ConsoleDriver` - Definition for the Console Driver API.
    - `Events.cs` - Defines keyboard and mouse-related structs & classes. 
    - `PosDim.cs` - Implements *Computed Layout* system. These classes have deep dependencies on `View`.
    - `Responder` - Base class for the windowing class hierarchy. Implements support for keyboard & mouse input.
    - `View` - Derived from `Responder`, the base class for non-modal visual elements such as controls.
    - `Toplevel` - Derived from `View`, the base class for modal visual elements such as top-level windows and dialogs. Supports the concept of `MenuBar` and `StatusBar`.
    - `Window` - Derived from `TopLevel`; implements toplevel views with a visible frame and Title.
- `Types/` - A folder (not namespace) containing implementations of `Point`, `Rect`, and `Size` which are ancient versions of the modern `System.Drawing.Point`, `System.Drawing.Size`, and `System.Drawning.Rectangle`.
- `ConsoleDrivers/` - Source files for the three `ConsoleDriver`-based drivers: .NET: `NetDriver`, Unix & Mac: `UnixDriver`, and Windows: `WindowsDriver`.
- `Views/` - A folder (not namespace) containing the source for all built-in classes that derive from `View` (non-modals). 
- `Windows/` - A folder (not namespace) containing the source of all built-in classes that derive from `Window`.
