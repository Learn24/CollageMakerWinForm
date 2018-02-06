using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Adsophic.PhotoEditor.GUILibrary.KeyboardSupport
{
    /// <summary>
    /// Called back when shortcut registered is clicked
    /// </summary>
    public interface IShortcutHandler
    {
        void HandleShortcut(object sender, KeyEventArgs args, ShortcutKey key);
    }
}
