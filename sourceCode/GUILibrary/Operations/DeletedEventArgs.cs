using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Adsophic.PhotoEditor.GUILibrary.Operations
{
    public class DeletedEventArgs
    {
        private UIElement _uiElement;
        private DeleteAction _action; 

        internal DeletedEventArgs(UIElement uiElement, DeleteAction action)
        {
            _uiElement = uiElement;
            _action = action; 
        }

        public UIElement UIElement { get { return _uiElement; } }

        public DeleteAction DeleteAction { get { return _action; } }

    }

    public enum DeleteAction { Deleted, DeleteUndone };
}
