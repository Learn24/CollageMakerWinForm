using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Adsophic.PhotoEditor.GUILibrary.Operations.Undo
{
    public class UIElementUndoCommand
    {
        private UIElement _uiElement;
        private Dictionary<string, object> _stateDictionary; 

        public UIElementUndoCommand(UIElement uiElement_)
        {
            _uiElement = uiElement_;
            _stateDictionary = new Dictionary<string, object>(); 
        }

        /// <summary>
        /// Returns the UI element that is identified by this command
        /// </summary>
        public UIElement Element { get { return _uiElement; } }

        /// <summary>
        /// Returns the state dicitonary for command specific details. 
        /// </summary>
        public Dictionary<string, object> StateDictionary { get { return _stateDictionary; } }
    }
}
