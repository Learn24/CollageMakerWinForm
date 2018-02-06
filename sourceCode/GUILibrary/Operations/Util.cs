using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Adsophic.PhotoEditor.GUILibrary.Operations
{
    internal enum PanelType
    {
        Canvas,
        DynamicCanvas
    }

    public static class UIHelper
    {
        internal static double GetLeft(UIElement element, PanelType panelType)
        {
            if (panelType == PanelType.Canvas)
                return Canvas.GetLeft(element);
            else
                return DynamicCanvas.GetLeft(element);
        }

        internal static double GetTop(UIElement element, PanelType panelType)
        {
            if (panelType == PanelType.Canvas)
                return Canvas.GetTop(element);
            else
                return DynamicCanvas.GetTop(element);
        }

        internal static void SetLeft(UIElement element, PanelType panelType, double left)
        {
            if (panelType == PanelType.Canvas)
                Canvas.SetLeft(element, left);
            else
                DynamicCanvas.SetLeft(element, left);
        }

        internal static void SetTop(UIElement element, PanelType panelType, double top)
        {
            if (panelType == PanelType.Canvas)
                Canvas.SetTop(element, top);
            else
                DynamicCanvas.SetTop(element, top);
        }
    }
}
