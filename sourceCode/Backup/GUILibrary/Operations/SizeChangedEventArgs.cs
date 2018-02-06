using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Adsophic.PhotoEditor.GUILibrary.Operations
{
    public class SizeChangedEventArgs : EventArgs
    {
        private UIElement _uiElement;
        private double _x;
        private double _y;
        private ResizeType _resizeType; 

        internal SizeChangedEventArgs(UIElement uiElement, double x,
            double y, ResizeType resizeType)
        {
            _uiElement = uiElement;
            _x = x;
            _y = y;
            _resizeType = resizeType; 
        }

        /// <summary>
        /// Returns the UIElement adorned
        /// </summary>
        public UIElement UIElement { get { return _uiElement; } }

        /// <summary>
        /// X-Axis movement for the UIElement
        /// </summary>
        public double X { get { return _x; } }

        /// <summary>
        /// Y-Axis movement for the UIElement
        /// </summary>
        public double Y { get { return _y; } }

        /// <summary>
        /// Whether the resize values are absolute or change
        /// </summary>
        public ResizeType ResizeType { get { return _resizeType; } }
    }

    public enum ResizeType { Absolute, Change }
}
