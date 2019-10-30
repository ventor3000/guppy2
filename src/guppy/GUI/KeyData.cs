using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    public class KeyData
    {
        public readonly KeyCode KeyCode;    //guppy key code
        public readonly uint RawKey;        //system dependent
        public readonly bool Shift;         //true if shift was held
        public readonly bool Ctrl;          //true if ctrl was held
        public readonly bool Alt;           //true if alt was held
        public readonly char Char;          //the character this keystroke represents or '\0' if none, can be changed in key events to override keystroke

        public KeyData(KeyCode keycode, uint rawkey, char keychar, bool ctrl, bool shift, bool alt)
        {
            this.KeyCode = keycode;
            this.RawKey = rawkey;
            this.Char = keychar;
            this.Ctrl = ctrl;
            this.Shift = shift;
            this.Alt = alt;
        }


        internal static string ShortcutName(KeyData kdata) //access thru Guppy.ShortcutName
        {
            List<string> parts = new List<string>();
            int keyval = (int)kdata.KeyCode;

            if (kdata.Ctrl) parts.Add("Ctrl");
            if (kdata.Shift) parts.Add("Shift");
            if (kdata.Alt) parts.Add("Alt");

            //if this is a typable key, add it directly:
            int unmod = keyval & 0xfffff; //remove shift,control etc.
            if (unmod >= 33 && unmod < (int)KeyCode.XBase)
                parts.Add(((char)unmod).ToString().ToUpper());
            else
            {
                string s;
                switch ((KeyCode)unmod)
                {
                    case KeyCode.Space: s = "Space"; break;
                    case KeyCode.Backspace: s = "BackSpace"; break;
                    case KeyCode.Tab: s = "Tab"; break;
                    case KeyCode.LineFeed: s = "LineFeed"; break;
                    case KeyCode.Return: s = "Return"; break;
                    case KeyCode.Escape: s = "Escape"; break;
                    case KeyCode.Home: s = "Home"; break;
                    case KeyCode.Up: s = "Up"; break;
                    case KeyCode.PageUp: s = "PageUp"; break;
                    case KeyCode.PageDown: s = "PageDown"; break;
                    case KeyCode.Left: s = "Left"; break;
                    case KeyCode.Right: s = "Right"; break;
                    case KeyCode.End: s = "End"; break;
                    case KeyCode.Down: s = "Down"; break;
                    case KeyCode.Insert: s = "Insert"; break;
                    case KeyCode.Delete: s = "Delete"; break;
                    case KeyCode.Pause: s = "Pause"; break;
                    case KeyCode.CapsLock: s = "CapsLock"; break;
                    case KeyCode.F1: s = "F1"; break;
                    case KeyCode.F2: s = "F2"; break;
                    case KeyCode.F3: s = "F3"; break;
                    case KeyCode.F4: s = "F4"; break;
                    case KeyCode.F5: s = "F5"; break;
                    case KeyCode.F6: s = "F6"; break;
                    case KeyCode.F7: s = "F7"; break;
                    case KeyCode.F8: s = "F8"; break;
                    case KeyCode.F9: s = "F9"; break;
                    case KeyCode.F10: s = "F10"; break;
                    case KeyCode.F11: s = "F11"; break;
                    case KeyCode.F12: s = "F12"; break;
                    case KeyCode.Print: s = "Print"; break;
                    case KeyCode.Menu: s = "Menu"; break;
                    case KeyCode.NumLock: s = "NumLock"; break;
                    case KeyCode.ScrollLock: s = "ScrollLock"; break;
                    default: s = "???"; break; //whats this?
                }

                parts.Add(s);
            }

            return string.Join("+", parts.ToArray());
        }



        /// <summary>
        /// Returns true if this keystroke is a non-typable character,
        /// such as escape, backspace, arrow keys etc.
        /// </summary>
        /// <returns></returns>
        public bool IsControlChar()
        {
            return (KeyCode < (KeyCode)32 && KeyCode > (KeyCode)0) || (KeyCode >= KeyCode.XBase);
        }


    }


    public enum KeyCode //note: all writable characters in range 0-0xffff is not included but have their unicode value, possibly with ctrl+shift+alt ore:ed in!
    {
        Unknown = 0,

        //Keys that matches escape characters
        Backspace = '\b',
        Tab = '\t',
        LineFeed = '\n',
        Return = '\r',
        Escape = 27,

        //for readability
        Space = 32,

        //Non typable keys
        XBase = 0x10000, //base value for extra keys
        Home = XBase,
        Up = XBase + 1,
        PageUp = XBase + 2,
        PageDown = XBase + 3,
        Left = XBase + 4,
        Right = XBase + 5,
        End = XBase + 6,
        Down = XBase + 7,
        Insert = XBase + 8,
        Delete = XBase + 9,
        Pause = XBase + 10,
        CapsLock = XBase + 11,
        F1 = XBase + 12,
        F2 = XBase + 13,
        F3 = XBase + 14,
        F4 = XBase + 15,
        F5 = XBase + 16,
        F6 = XBase + 17,
        F7 = XBase + 18,
        F8 = XBase + 19,
        F9 = XBase + 20,
        F10 = XBase + 21,
        F11 = XBase + 22,
        F12 = XBase + 23,
        Print = XBase + 24,
        Menu = XBase + 25,
        NumLock = XBase + 26,
        ScrollLock = XBase + 27,


        //thoose can be combined with a key
        /*Shift = 0x100000,
        Control = 0x200000,
        Alt = 0x400000*/


    }
}
