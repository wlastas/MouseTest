using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using V2 = System.Drawing.PointF;
namespace MouseTest {
    internal class Program {
        static void Main(string[] args) {
            MoveWindoeToCentr();
            Console.WriteLine("Hello!");
            Console.WriteLine("to intercept the cursor during the test, press [left Alt] key");
            PrintInsruction();
            bool b_running = true;
            var R = new Random();
            while (b_running) {
                var cmd = Console.ReadKey();
                switch (cmd.Key) {
                    case ConsoleKey.Z:
                        Console.Clear();
                        break;
                    case ConsoleKey.E:
                        EnableConsoleQuickEdit();
                        break;
                    case ConsoleKey.D:
                        DisableConsoleQuickEdit();
                        break;
                    case ConsoleKey.B:
                        BadClick();
                        break;
                    case ConsoleKey.S:
                        Select();
                        break;
                    case ConsoleKey.C:
                        RunTest();
                        break;
                    case ConsoleKey.M:
                        MoreTest(10);
                        break;
                    case ConsoleKey.Escape:
                        b_running = false;
                        break;
                    case ConsoleKey.Oem2:
                        PrintInsruction();
                        break;
                    default:
                        break;
                }
            }
            void BadClick() {
                DisableConsoleQuickEdit();
                SentMouseCentr();
                for (int i = 0; i < 10; i++) {
                    Mouse.LeftClick("click_" + i);
                    Thread.Sleep(R.Next(5, 600));
                }
            }
            void RunTest() {
                SentMouseCentr();
            }
            void Select() {
                Console.Clear();
                DisableConsoleQuickEdit();
                var rc = GetCurrentConsoleRect();
                Mouse.SetCursor(new V2(rc.left + 20, rc.top + 100));
                EnableConsoleQuickEdit();
                Mouse.LeftDown("Start");
                Mouse.SetCursor(new V2(rc.left + 20 + 200, rc.top + 100 + 200), 5, false);
                Mouse.LeftUp("Done", true);
            }
            void SentMouseCentr() {
                var rc = GetCurrentConsoleRect();
                var cx = rc.left + (rc.right - rc.left) / 2;
                var cy = rc.top + (rc.bottom - rc.top) / 2;
                var centr = new V2(cx, cy);
                Mouse.SetCursor(centr);
            }
            RECT GetCurrentConsoleRect() {
                RECT res;
                var hWin = GetConsoleWindow();
                GetWindowRect(hWin, out res);
                return res;
            }
            void MoveWindoeToCentr() {
                RECT wrc;
                var hWin = GetConsoleWindow();
                GetWindowRect(hWin, out wrc);
                var scr = Screen.FromPoint(new Point(wrc.left, wrc.top));
                var x = scr.WorkingArea.Left + (scr.WorkingArea.Width - (wrc.right - wrc.left)) / 2;
                var y = scr.WorkingArea.Top + (scr.WorkingArea.Height - (wrc.bottom - wrc.top)) / 2;
                MoveWindow(hWin, x, y, wrc.right - wrc.left, wrc.bottom - wrc.top, false);
            }
            void MoreTest(int count) {
                for (int i = 0; i < count; i++) {
                    var np = new V2(R.Next(0, 500), R.Next(0, 500));
                    Mouse.SetCursor(np);
                    Thread.Sleep(200);
                }
            }
            void PrintInsruction() {
                Console.WriteLine("Press [?] for draw this menu");
                Console.WriteLine("Press [Z] for clear all");
                Console.WriteLine("Press [B] for bad fast click");
                Console.WriteLine("Press [C] to move cursor to Screen Centr");
                Console.WriteLine("Press [S] to select same rect");
                Console.WriteLine("Press [E] to Enable Console QuickEdit");
                Console.WriteLine("Press [D] tp Disable Consol QuickEdit");
                Console.WriteLine("Press [Esc] to Exit");
            }
        }
        static void DisableConsoleQuickEdit() {
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

            const uint ENABLE_QUICK_EDIT = 0x0040;

            //IntPtr consoleHandle = GetConsoleWindow();
            uint consoleMode;

            // get current console mode
            if (!GetConsoleMode(consoleHandle, out consoleMode)) {
                // Error: Unable to get console mode.
                Console.WriteLine(Marshal.GetLastWin32Error());
                return;
            }

            // Clear the quick edit bit in the mode flags
            consoleMode &= ~ENABLE_QUICK_EDIT;

            // set the new mode
            if (!SetConsoleMode(consoleHandle, consoleMode)) {
                // ERROR: Unable to set console mode
                Console.WriteLine(Marshal.GetLastWin32Error());
            }
        }
        static void EnableConsoleQuickEdit() {
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

            const uint ENABLE_QUICK_EDIT = 0x0040;

            //IntPtr consoleHandle = GetConsoleWindow();
            uint consoleMode;

            // get current console mode
            if (!GetConsoleMode(consoleHandle, out consoleMode)) {
                // Error: Unable to get console mode.
                Console.WriteLine(Marshal.GetLastWin32Error());
                return;
            }

            // Clear the quick edit bit in the mode flags
            consoleMode |= ENABLE_QUICK_EDIT;

            // set the new mode
            if (!SetConsoleMode(consoleHandle, consoleMode)) {
                // ERROR: Unable to set console mode
                Console.WriteLine(Marshal.GetLastWin32Error());
            }
        }
        const int STD_INPUT_HANDLE = -10;
        [DllImport("kernel32.dll")]
        static extern IntPtr GetStdHandle(int nStdHandle);
        private struct RECT { public int left, top, right, bottom; }
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT rc);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int w, int h, bool repaint);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
       
    }

    public enum MouseButton {
        Left,
        Right,
        Middle,
        Extra1,
        Extra2,
    }
   
}

   






