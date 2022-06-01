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
    public static class Mouse {
        static Stopwatch sw = new Stopwatch();
        private const int KEY_TOGGLED = 0x0001;
        private const int KEY_PRESSED = 0x8000;
        public enum MouseEvents {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            Absolute = 0x00008000,
            RightDown = 0x00000008,
            RightUp = 0x00000010,
            Wheel = 0x800
        }
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32.dll")]
        static extern bool ScreenToClient(IntPtr hWnd, ref Point lpPoint);

        public static V2 GetCursorPosition() {
            Point lpPoint;
            GetCursorPos(out lpPoint);
            return new V2(lpPoint.X, lpPoint.Y);
        }
        [DllImport("user32.dll")]
        static extern short GetKeyState(int nVirtKey);
        public static bool HotKeyPressed(Keys key, int interv = 400, bool debug = true) {
            if ((GetKeyState((int)key) & KEY_PRESSED) != 0) {
                if (!down_time.ContainsKey(key) || (down_time.ContainsKey(key) && down_time[key].AddMilliseconds(interv) < DateTime.Now)) {
                    down_time[key] = DateTime.Now;
                    if (debug)
                        AddToLog("HotKey " + key.ToString() + " used OK");
                    return true;
                }
                else {
                    //if(debug && AddToLog != null)
                    //    AddToLog("HotKey " + key.ToString() + " was already pressed recently");
                    return false;
                }
            }
            return false;
        }

        static ConcurrentDictionary<Keys, DateTime> down_time = new ConcurrentDictionary<Keys, DateTime>();
        //TODO here bug UP sametime for same mouse buttons => not detecting 
        public static bool IsButtonDown(Keys key) {
            return GetKeyState((int)key) < 0;
        }

        public static void RightClick(string _from) {
            RightDown(_from);
            RightUp();
        }
        static Stopwatch sw_right_down = new Stopwatch();
        public static void RightDown(string _from, int mdi = 300) {
            var elaps = sw_right_down.Elapsed.TotalMilliseconds;
            if (elaps < mdi) {
                Thread.Sleep((int)(mdi - elaps));
                AddToLog(_from + " Try RightDown to fast");
            }
            mouse_event((int)MouseEvents.RightDown, 0, 0, 0, 0);
            sw_right_down.Restart();
        }

        public static void RightUp() {
            mouse_event((int)MouseEvents.RightUp, 0, 0, 0, 0);
        }

        public static void VerticalScroll(bool forward, int clicks = 1) {
            if (forward)
                mouse_event((int)MouseEvents.Wheel, 0, 0, clicks * 120, 0);
            else
                mouse_event((int)MouseEvents.Wheel, 0, 0, -(clicks * 120), 0);
        }

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool BlockInput(bool block);

        public static void LeftClick(string _from) {
            LeftDown(_from);
            LeftUp(_from);
        }
        static DateTime last_left_down;
        public delegate void MDownInfoDelegate(string write);
        public const int min_down_interval = 300; //ms
        public static void LeftDown(string from, int _mdi = min_down_interval) { //minimal_down_interval  //ms
            bool debug = false;
            var run_after = last_left_down.AddMilliseconds(_mdi);
            if (run_after > DateTime.Now) {
                var need_sleep = run_after - DateTime.Now;
                AddToLog(from + " Try LeftDown to fast.. w8 [" + need_sleep.Milliseconds + "]ms more");
                Thread.Sleep(need_sleep);
            }
            mouse_event((int)MouseEvents.LeftDown, 0, 0, 0, 0);
            last_left_down = DateTime.Now;
            left_down_count += 1;
            last_set_id = 0;
            if (debug) {
                AddToLog("LeftDown.." + from);
                AddToLog("LeftDown..[" + left_down_count + "]");
            }
        }

        public static void LeftUp(string from, bool i_sure = true) {
            if (i_sure && !IsButtonDown(Keys.LButton)) {
                AddToLog("Left button Down NOT detected");
            }
            bool debug = true;
            if (IsButtonDown(Keys.LButton) || i_sure) {
                mouse_event((int)MouseEvents.LeftUp, 0, 0, 0, 0);
                if (debug)
                    AddToLog("LeftUp.." + from);
            }
        }
        //TODO: Dont try this
        public static void blockInput(bool block) {
            BlockInput(block);
        }

        public static DateTime last_setted;//last auto mouse setted time
        public static int left_down_count;
        public static int last_set_id;
        /// <summary>
        /// mouse movement simulation using linear interposition
        /// </summary>
        /// <param name="tp">target screen point</param>
        /// <param name="step">affects movement speed</param>
        public static void SetCursor(V2 tp, int step = 5, bool debug = true) {
            var ccp = GetCursorPosition();//current cursor position
            var dx = Math.Abs(tp.X - ccp.X);
            var dy = Math.Abs(tp.Y - ccp.Y);
            if (dx < 2 && dy < 2) {
                AddToLog("SetCursor.. low start delta");
                return;
            }

            if (step > 0) {
                sw.Restart();
                float dist = float.MaxValue;
                bool get_elaps() {
                    var res = sw.Elapsed.Milliseconds < step * 200;
                    return res;
                }

                while (dist > 10 && get_elaps()) {
                    if (b_alt)
                        continue;
                    var cp = GetCursorPosition(); //current point
                    dist = tp.GetDistance(cp);
                    var dir = tp.Subtract(cp);
                    dir = dir.Normalize();
                    var nsp = dir.Scale(dist / step);
                    var nt = cp.Increase(nsp);
                    if (debug)
                        AddToLog("nt=" + nt.ToString());
                    SetCursorPos((int)nt.X, (int)nt.Y);
                    MouseMove();

                    //TODO: correct next delay u need 2-3 is ok 10+ for debug ONLY
                    Thread.Sleep(20);
                }
            }
            if (debug)
                AddToLog("nt=" + tp.ToString());
            SetCursorPos((int)tp.X, (int)tp.Y); //last path part
            MouseMove();

        }
        static bool b_alt {
            get {
                var alt = IsKeyDown(Keys.LMenu);

                return alt;
            }
        }
        public static bool IsKeyDown(Keys key) {
            return GetKeyState((int)key) < 0;
        }
        static void MouseMove() {
            mouse_event((int)MouseEvents.Move, 0, 0, 0, 0);
        }
        static V2 Scale(this V2 value, float scale) {
            return new V2(value.X * scale, value.Y * scale);
        }
        static float GetDistance(this V2 value1, V2 value2) {
            float x = value1.X - value2.X;
            float y = value1.Y - value2.Y;

            var res = (float)Math.Sqrt((x * x) + (y * y));
            return res;
        }
        static float Length(this V2 p) {
            return (float)Math.Sqrt((p.X * p.X) + (p.Y * p.Y));
        }
        public static V2 Subtract(this V2 source, V2 targ) {
            return new V2(source.X - targ.X, source.Y - targ.Y);
        }
        static V2 Increase(this V2 source, V2 targ) {
            return new V2(source.X + targ.X, source.Y + targ.Y);
        }
        static V2 Normalize(this V2 p) {
            float length = p.Length();
            var res = new V2(p.X, p.Y);
            if (length > 0) {
                float inv = 1.0f / length;
                res.X *= inv;
                res.Y *= inv;
            }
            return res;
        }
        static void AddToLog(string inp) {
            Console.WriteLine(inp);
        }
    }
}
