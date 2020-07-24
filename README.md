# aiv-draw
Simple wrapper for teaching computer graphics principles to AIV first year students

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
* x64 architecture

# Contribution
If you want to contribute to the project, please follow the [guidelines](CONTRIBUTING.md).