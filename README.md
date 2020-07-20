# aiv-draw
Simple wrapper for teaching computer graphics principles to AIV first year students


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
       while (window.opened) {
        // write bytes into window.bitmap array to draw ...
        // read window.deltaTime to get float time delta (1f = 1 second)
        // call window.GetKey(KeyCode.xxx) to check for key press
        window.Blit ();
       }
    }
  }
}
```

You can get an RGBA array from images using the Sprite class:

```csharp
Sprite hero = new Sprite("heroSpriteSheet.png");
byte []bitmap = hero.bitmap;
int width = hero.width;
int height = hero.height;
```

You can update Window title like this:

```csharp
window.SetTitle("Your new title");
```

## Constraints
Library tested on:
* Visual Studio 2019 v16.6.4
* .NET Framework 4.8