# TermUI

**TermUI**: Cross platform terminal UI toolkit for building rich console apps for .NET Core that works on Windows, the Mac, and Linux/Unix.

**Attention**: This is a particular fork - sandbox for the development of Terminal.Gui v1.

## Documentation 

* [Overview](docfx/docs/overview.md)
* [List of Views/Controls](docfx/docs/views.md)
* [Conceptual Documentation](docfx/docs/index.md)

## Features

* **Cross Platform** - Windows, Mac, and Linux. Terminal drivers for Curses, Windows Console, and the .NET Console mean apps will work well on both color and monochrome terminals. 
* **Keyboard and Mouse Input** - Both keyboard and mouse input are supported, including support for drag & drop.
* **Flexible Layout** - Supports both *Absolute layout* and an innovative *Computed Layout* system. *Computed Layout* makes it easy to lay out controls relative to each other and enables dynamic terminal UIs.
* **Clipboard support** - Cut, Copy, and Paste of text provided through the Clipboard class.
* **Arbitrary Views** - All visible UI elements are subclasses of the `View` class, and these in turn can contain an arbitrary number of sub-views.
* **Advanced App Features** - The Mainloop supports processing events, idle handlers, timers, and monitoring file descriptors. Most classes are safe for threading.

## Showcase & Examples

**TermUI** can be used with any .Net language to create feature rich and robust applications.

* **[UI Catalog](UICatalog)** - The UI Catalog project provides an easy to use and extend sample illustrating the capabilities of **TermUI**. Run `dotnet run --project UICatalog` to run the UI Catalog.
  ![Sample app](docfx/images/sample.gif)

* **[C# Example](Example)** - Run `dotnet run` in the `Example` directory to run the C# Example.
  ![Simple Usage app](./docfx/images/Example.png)

## Building the Library and Running the Examples

* Windows, Mac, and Linux - Build and run using the .NET SDK command line tools (`dotnet build` in the root directory). Run `UICatalog` with `dotnet run --project UICatalog`.
* Windows - Open `Terminal.sln` with Visual Studio 2022.

See [CONTRIBUTING.md](CONTRIBUTING.md) for instructions for downloading and forking the source.
