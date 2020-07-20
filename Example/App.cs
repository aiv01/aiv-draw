using Aiv.Draw;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example
{
    class App
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== AivDraw Example ===");
            Console.WriteLine("Possible examples:");
            Console.WriteLine("[1] Basic Loop");
            Console.WriteLine("[2] Drawing a Square");
            Console.WriteLine("[3] Moving Square");
            Console.WriteLine("[4] Moving Square by WASD");
            Console.WriteLine("[5] Drawing Square by Mouse");
            Console.WriteLine("[6] Drawing Sprite");
            Console.WriteLine("[7] Misc features");
            Console.WriteLine();

            int minChoice = 1;
            int maxChoice = 7;
            int choice;
            do
            {
                Console.Write("Pick a number: ");
                string input = Console.ReadLine();
                
                bool isValidNumber = int.TryParse(input, out choice);
                if (!isValidNumber ||
                    choice < minChoice || choice > maxChoice)
                {
                    Console.WriteLine("Invalid choice!!!");
                }
                else break;
            } while (true);

            switch (choice) {
                case 1: Example01_BasicLoop(); break;
                case 2: Example02_DrawingSquare(); break;
                case 3: Example03_MovingSquare(); break;
                case 4: Example04_MovingSquareByWASD(); break;
                case 5: Example05_DrawingSquareByMouse(); break;
                case 6: Example06_DrawingSprite(); break;
                case 7: Example07_MiscFeatures(); break;
            }

        }

        private static void Example01_BasicLoop()
        {
            Window win = new Window(800, 600, "AivDraw Basic Loop", PixelFormat.RGB);

            while (win.opened)
            {
                //Console.WriteLine("Looping ...");
                Console.WriteLine(win.DeltaTime);
                win.Blit();
            }
        }

        private static void Example02_DrawingSquare()
        {
            Window win = new Window(800, 600, "AivDraw Drawing Square", PixelFormat.RGB);

            while (win.opened)
            {
                DrawSquare(win, 10, 10, 50);
                win.Blit();
            }
        }

        private static void Example03_MovingSquare()
        {
            Window win = new Window(800, 600, "AivDraw Moving Square", PixelFormat.RGB);

            int speed = 200;
            float posX = 0;
            int size = 50;

            while (win.opened)
            {
                Clear(win);

                posX += (win.DeltaTime * speed);

                DrawSquare(win, (int)posX, 10, size);

                win.Blit();
            }
        }

        private static void Example04_MovingSquareByWASD()
        {
            Window win = new Window(800, 600, "AivDraw Moving Square by WASD", PixelFormat.RGB);

            int speed = 200;
            float posX = 0;
            float posY = 0;
            int size = 50;

            while (win.opened)
            {
                Clear(win);
                
                if (win.GetKey(KeyCode.W))
                {
                    posY -= (win.DeltaTime * speed);
                }
                if (win.GetKey(KeyCode.S))
                {
                    posY += (win.DeltaTime * speed);
                }
                if (win.GetKey(KeyCode.A))
                {
                    posX -= (win.DeltaTime * speed);
                }
                if (win.GetKey(KeyCode.D))
                {
                    posX += (win.DeltaTime * speed);
                }

                DrawSquare(win, (int)posX, (int)posY, size);

                win.Blit();
            }
        }

        private static void Example05_DrawingSquareByMouse()
        {
            Window win = new Window(800, 600, "AivDraw Drawing Square by Mouse", PixelFormat.RGB);

            while (win.opened)
            {
                Clear(win);

                if (win.MouseLeft)
                {
                    int posX = win.MouseX;
                    int posY = win.MouseY;
                    DrawSquare(win, posX, posY, 50);
                }
                
                win.Blit();
            }
        }

        private static void Example06_DrawingSprite()
        {
            Window win = new Window(800, 600, "AivDraw Drawing Sprite", PixelFormat.RGB);

            Sprite square = new Sprite("square.png");

            while (win.opened)
            {
                for(int y = 0; y < square.Height; y++)
                {
                    for (int x = 0; x < square.Width; x++)
                    {
                        int destinPos = (y * win.Width + x) * 3;
                        int sourcePos = (y * square.Width + x) * 3;
                        win.Bitmap[destinPos + 0] = square.Bitmap[sourcePos + 0];
                        win.Bitmap[destinPos + 1] = square.Bitmap[sourcePos + 1];
                        win.Bitmap[destinPos + 2] = square.Bitmap[sourcePos + 2];
                    }
                }

                win.Blit();
            }
        }

        private static void Example07_MiscFeatures()
        {
            Window win = new Window(800, 600, "AivDraw Misc Features", PixelFormat.RGB);

            win.SetIcon("square.ico");
            win.SetMouseVisible(false);

            int count = 0;
            while (win.opened)
            {
                win.SetTitle("Counter: " + count++);
                win.Blit();
            }
        }

        private static void Clear(Window win) 
        {
            for (int i = 0; i < win.Bitmap.Length; i++)
            {
                win.Bitmap[i] = 0;
            }
        }

        private static void DrawSquare(Window win, int posX, int posY, int size)
        {
            for(int x = posX; x < posX + size; x++)
            {
                for (int y = posY; y < posY + size; y++)
                {
                    int basePos = (y * win.Width + x) * 3; // 3 = number of bytes for RGB pixel
                    win.Bitmap[basePos + 0] = 255;  //R
                    win.Bitmap[basePos + 1] =   0;  //G
                    win.Bitmap[basePos + 2] =   0;  //B
                }
            }
        }
    }
}
