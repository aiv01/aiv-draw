# aiv-draw &middot; ![Nuget](https://img.shields.io/nuget/v/Aiv.Draw?color=blue) ![Nuget](https://img.shields.io/nuget/dt/Aiv.Draw?color=yellow) [![Api Doc](https://img.shields.io/badge/api--doc-read-blue)](http://aiv01.github.io/aiv-draw/) [![Build Status](https://travis-ci.org/aiv01/aiv-draw.svg?branch=master)](https://travis-ci.org/aiv01/aiv-draw) 


Simple wrapper for teaching computer graphics principles to AIV first year students.

Main features:
* Window wrapper
* Window bitmap format management (Black & White, Gray scale, RGB and RGBA)
* Input management (Mouse and Keyboard)
* Image loading (in RGB and RGBA format)

# Example
Below a very basic usage example.

> More examples are available in [Example project](./Example).

```csharp
using System;
using Aiv.Draw;

namespace DrawTest
{
  class MainClass
  {
    public static void Main (string[] args)
    {
       Window window = new Window (1024, 768, "Hello", PixelFormat.RGB);
       while (window.IsOpened) {
        // write bytes into window.Bitmap array to draw ...
        // read window.deltaTime to get float time delta (1f = 1 second)
        // call window.GetKey(KeyCode.xxx) to check for key press
        window.Blit();
       }
    }
  }
}
```

# Documentation
API documentation related to the last version of the library is published [here](http://aiv01.github.io/aiv-draw/).

# Compliance
Library tested on:
* Visual Studio 2019 v16.6.4
* .NET Framework 4.8
* Any Cpu architecture

# Contribution
If you want to contribute to the project, please follow the [guidelines](CONTRIBUTING.md).