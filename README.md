# TermUI

Cross platform terminal UI toolkit for .NET.

This is a particular fork - sandbox for the development of Terminal.Gui v1.

***The current, stable, release of Terminal.Gui is [v1.x](https://www.nuget.org/packages/Terminal.Gui). It is stable, rich, and broadly used.***

**Terminal.Gui**: A toolkit for building rich console apps for .NET, .NET Core, and Mono that works on Windows, the Mac, and Linux/Unix.

![Sample app](docfx/images/sample.gif)

## Documentation 

* [Documentation Home](https://gui-cs.github.io/Terminal.Gui/index.html)
* [Terminal.Gui Overview](https://gui-cs.github.io/Terminal.Gui/docs/overview.html)
* [List of Views/Controls](https://gui-cs.github.io/Terminal.Gui/docs/views.html)
* [Conceptual Documentation](https://gui-cs.github.io/Terminal.Gui/docs/index.html)
* [API Documentation](https://gui-cs.github.io/Terminal.Gui/api/Terminal.Gui)

_The Documentation matches the most recent Nuget release from the `v1_release_` branch ([![Version](https://img.shields.io/nuget/v/Terminal.Gui.svg)](https://www.nuget.org/packages/Terminal.Gui))_

See the [`Terminal.Gui/` README](https://github.com/gui-cs/Terminal.Gui/tree/master/Terminal.Gui) for an overview of how the library is structured. The [Conceptual Documentation](https://gui-cs.github.io/Terminal.Gui/docs/index.html) provides insight into core concepts.

## Features

* **Cross Platform** - Windows, Mac, and Linux. Terminal drivers for Curses, [Windows Console](https://github.com/gui-cs/Terminal.Gui/issues/27), and the .NET Console mean apps will work well on both color and monochrome terminals. 
* **Keyboard and Mouse Input** - Both keyboard and mouse input are supported, including support for drag & drop.
* **[Flexible Layout](https://gui-cs.github.io/Terminal.Gui/articles/overview.html#layout)** - Supports both *Absolute layout* and an innovative *Computed Layout* system. *Computed Layout* makes it easy to lay out controls relative to each other and enables dynamic terminal UIs.
* **Clipboard support** - Cut, Copy, and Paste of text provided through the [`Clipboard`](https://gui-cs.github.io/Terminal.Gui/api/Terminal.Gui/Terminal.Gui.Clipboard.html) class.
* **[Arbitrary Views](https://gui-cs.github.io/Terminal.Gui/api/Terminal.Gui/Terminal.Gui.View.html)** - All visible UI elements are subclasses of the `View` class, and these in turn can contain an arbitrary number of sub-views.
* **Advanced App Features** - The [Mainloop](https://gui-cs.github.io/Terminal.Gui/api/Terminal.Gui/Terminal.Gui.MainLoop.html) supports processing events, idle handlers, timers, and monitoring file
descriptors. Most classes are safe for threading.

## Showcase & Examples

**Terminal.Gui** can be used with any .Net language to create feature rich and robust applications.  
[Showcase](https://github.com/gui-cs/Terminal.Gui/blob/develop/Showcase.md) is a place where you can find all kind of projects from simple examples to advanced real world apps that fully utilize capabilities of the toolkit.  
The team is looking forward to seeing new amazing projects made by the community to be added there!

## Sample Usage in C#

The following example shows a basic Terminal.Gui application in C#:

```csharp
// This is a simple example application.  For the full range of functionality
// see the UICatalog project

// A simple Terminal.Gui example in C# - using C# 9.0 Top-level statements

using Terminal.Gui;

Application.Run<ExampleWindow> ();

// Before the application exits, reset Terminal.Gui for clean shutdown
Application.Shutdown ();

System.Console.WriteLine ($@"Username: {ExampleWindow.Username}");

// Defines a top-level window with border and title
public class ExampleWindow : Window {
	public static string Username { get; internal set; }
	public TextField usernameText;

	public ExampleWindow ()
	{
		Title = "Example App (Ctrl+Q to quit)";

		// Create input components and labels
		var usernameLabel = new Label () {
			Text = "Username:"
		};

		usernameText = new TextField ("") {
			// Position text field adjacent to the label
			X = Pos.Right (usernameLabel) + 1,

			// Fill remaining horizontal space
			Width = Dim.Fill (),
		};

		var passwordLabel = new Label () {
			Text = "Password:",
			X = Pos.Left (usernameLabel),
			Y = Pos.Bottom (usernameLabel) + 1
		};

		var passwordText = new TextField ("") {
			Secret = true,
			// align with the text box above
			X = Pos.Left (usernameText),
			Y = Pos.Top (passwordLabel),
			Width = Dim.Fill (),
		};

		// Create login button
		var btnLogin = new Button () {
			Text = "Login",
			Y = Pos.Bottom (passwordLabel) + 1,
			// center the login button horizontally
			X = Pos.Center (),
			IsDefault = true,
		};

		// When login button is clicked display a message popup
		btnLogin.Clicked += () => {
			if (usernameText.Text == "admin" && passwordText.Text == "password") {
				MessageBox.Query ("Logging In", "Login Successful", "Ok");
				Username = usernameText.Text.ToString ();
				Application.RequestStop ();
			} else {
				MessageBox.ErrorQuery ("Logging In", "Incorrect username or password", "Ok");
			}
		};

		// Add the views to the Window
		Add (usernameLabel, usernameText, passwordLabel, passwordText, btnLogin);
	}
}
```

When run the application looks as follows:

![Simple Usage app](./docfx/images/Example.png)

_Sample application running_

## Installing

### Installation in .NET Core Projects

To install Terminal.Gui into a .NET Core project, use the `dotnet` CLI tool with this command.

```
dotnet add package Terminal.Gui
```

Or, you can use the [Terminal.Gui.Templates](https://github.com/gui-cs/Terminal.Gui.templates).

## Building the Library and Running the Examples

* Windows, Mac, and Linux - Build and run using the .NET SDK command line tools (`dotnet build` in the root directory). Run `UICatalog` with `dotnet run --project UICatalog`.
* Windows - Open `Terminal.sln` with Visual Studio 2022.

See [CONTRIBUTING.md](CONTRIBUTING.md) for instructions for downloading and forking the source.

## Contributing

See [CONTRIBUTING.md](https://github.com/gui-cs/Terminal.Gui/blob/master/CONTRIBUTING.md).

Debates on architecture and design can be found in Issues tagged with [design](https://github.com/gui-cs/Terminal.Gui/issues?q=is%3Aopen+is%3Aissue+label%3Adesign).

## History

See [gui-cs](https://github.com/gui-cs/) for how this project came to be.
