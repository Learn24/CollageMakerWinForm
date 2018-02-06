using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Adsophic.PhotoEditor.GUILibrary.KeyboardSupport
{
    /// <summary>
    /// Create a key for a shortcut combination
    /// </summary>
    public class ShortcutKey
    {
        private string _stringCombination; 

        public ShortcutKey(ModifierKeys[] modifierKeys, Key letter)
        {
            _stringCombination = CreateStringKey(modifierKeys, letter); 
        }

        public ShortcutKey(KeyEventArgs args)
        {
            _stringCombination = CreateStringKeyFromKeyEventArgs(args);
        }

        private static string CreateStringKey(ModifierKeys[] modifierKeys, Key letter)
        {
            if(!ValidateCombination(modifierKeys, letter))
                throw new ArgumentException("combination Array cannot be null");

            Array.Sort(modifierKeys, new KeyCombinationComparer());

            StringBuilder keyBuilder = new StringBuilder(); 
            foreach (ModifierKeys key in modifierKeys)
            {
                keyBuilder.Append("_" + key.ToString());
            }

            keyBuilder.Append("_" + letter.ToString());

            return keyBuilder.ToString(); 
        }

        private static string CreateStringKeyFromKeyEventArgs(KeyEventArgs args)
        {               
            StringBuilder keyBuilder = new StringBuilder();
            //alt, control, shift, none, windows
            if ((args.KeyboardDevice.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                keyBuilder.Append("_" + ModifierKeys.Alt.ToString());

            if ((args.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                keyBuilder.Append("_" + ModifierKeys.Control.ToString());

            if ((args.KeyboardDevice.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                keyBuilder.Append("_" + ModifierKeys.Shift.ToString());

            /*
            if ((args.KeyboardDevice.Modifiers & ModifierKeys.None) == ModifierKeys.None)
                keyBuilder.Append("_" + ModifierKeys.None.ToString());
             * */

            if ((args.KeyboardDevice.Modifiers & ModifierKeys.Windows) == ModifierKeys.Windows)
                keyBuilder.Append("_" + ModifierKeys.Windows.ToString());


            keyBuilder.Append("_" + args.Key.ToString());

            return keyBuilder.ToString();
        }

        public static bool ValidateCombination(ModifierKeys[] modifierKeys, Key letter)
        {
            if (modifierKeys == null || modifierKeys.Length == 0)
                return false;

            
            bool foundAlt = false;
            bool foundControl = false;
            bool foundShift = false;
            //bool foundWindow = false; 

            foreach (ModifierKeys key in modifierKeys)
            {
                switch (key)
                {
                    case ModifierKeys.None:
                    case ModifierKeys.Windows:
                        return false;

                    case ModifierKeys.Alt:
                        if (foundAlt)
                            return false;
                        foundAlt = true;
                        break;

                    case ModifierKeys.Control:
                        if (foundControl)
                            return false;
                        foundControl = true;
                        break;

                    case ModifierKeys.Shift:
                        if (foundShift)
                            return false;
                        foundShift = true;
                        break;

                    default:
                        return false;
                }

            }


            return true;
        }

        private class KeyCombinationComparer : IComparer<ModifierKeys>
        {
            #region IComparer<ModifierKeys> Members

            public int Compare(ModifierKeys x, ModifierKeys y)
            {
                if (x == y) return 0;
                if (x == ModifierKeys.Alt)
                    return 1;
                if (y == ModifierKeys.Alt)
                    return -1;
                if (x == ModifierKeys.Control)
                    return 1;
                if (y == ModifierKeys.Control)
                    return -1;
                if (x == ModifierKeys.Shift)
                    return 1;
                if (y == ModifierKeys.Shift)
                    return -1;
                if (x == ModifierKeys.None)
                    return 1;
                if (y == ModifierKeys.None)
                    return 1;

                return 0; 
            }

            #endregion
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ShortcutKey))
                return false;

            return _stringCombination == ((ShortcutKey)obj)._stringCombination; 
        }

        public override int GetHashCode()
        {
            return _stringCombination.GetHashCode(); 
        }

        public override string ToString()
        {
            return _stringCombination; 
        }
    }
}
