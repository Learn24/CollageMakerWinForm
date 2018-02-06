using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using Adsophic.PhotoEditor.GUILibrary.Operations.Undo;
using Adsophic.Common.Util.Operations.Undo;

namespace Adsophic.PhotoEditor.GUILibrary.Operations
{
    /// <summary>
    /// Adds support for Dragging UIElements in a panel. 
    /// </summary>
    public class DragMoveChildElementImplementor : IUndoCommandImplementor<UIElementUndoCommand>
    {
        private Panel _parentPanel;
        private PanelType _parentPanelType;
        private IUndoOperationsContainer<UIElementUndoCommand> _undoOperationsContainer; 

        private Point m_StartPoint;  // Where did the mouse start off from?
        private double m_OriginalLeft; // What was the element's original x offset?
        private double m_OriginalTop; // What was the element's original y offset?
        private bool m_IsDown; //Is the mouse down right now?
        private bool m_IsDragging; //Are we actually dragging the shape around?
        private UIElement m_OriginalElement; //What is it that we're dragging?
        private Rectangle m_OverlayElement; //rectangle used to display the dragging.
        private UIElement m_SelectedElement = null;
        private Window m_DisplayWindow = null;
        private bool m_HasMoveStarted = false;

        private double m_VerticalMoveDistance = 1;//SystemParameters.MinimumVerticalDragDistance;
        private double m_HorizontalMoveDistance = 1;//SystemParameters.MinimumHorizontalDragDistance;

        /// <summary>
        /// Parent panel control whose child controls we want to be able to 
        /// drag and drop 
        /// </summary>
        /// <param name="parentPanel_"></param>
        public DragMoveChildElementImplementor(Panel parentPanel_, IUndoOperationsContainer<UIElementUndoCommand> undoOperationsContainer_)
        {
            if (!(parentPanel_ is DynamicCanvas) && !(parentPanel_ is Canvas))
                MessageBox.Show("Current only Canvas and Dynamic Canvas are supported by the drag drop implementor");

            if (parentPanel_ is Canvas)
                _parentPanelType = PanelType.Canvas;
            else
                _parentPanelType = PanelType.DynamicCanvas;

            _parentPanel = parentPanel_;
            _undoOperationsContainer = undoOperationsContainer_; 
            SetupMainCanvasEvents(); 
        }

        public DragMoveChildElementImplementor(Panel parentPanel_)
            : this(parentPanel_, null)
        {
        }

        private void SetupMainCanvasEvents()
        {
            _parentPanel.PreviewMouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(MainCanvas_PreviewMouseLeftButtonDown);
            _parentPanel.PreviewMouseMove += new System.Windows.Input.MouseEventHandler(MainCanvas_PreviewMouseMove);
            _parentPanel.PreviewMouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(MainCanvas_PreviewMouseLeftButtonUp);            
        }

        public void SelectElement(UIElement uiElement)
        {
            m_SelectedElement = uiElement;
            if (m_SelectedElement != null)
            {
                if (m_DisplayWindow != null)
                {
                    m_DisplayWindow.PreviewKeyDown -= new KeyEventHandler(DisplayWindow_PreviewKeyDown);
                    m_DisplayWindow.PreviewKeyUp += new KeyEventHandler(DisplayWindow_PreviewKeyUp);
                }
                
                m_DisplayWindow = Window.GetWindow((DependencyObject)uiElement);
                m_DisplayWindow.PreviewKeyDown += new KeyEventHandler(DisplayWindow_PreviewKeyDown);
                m_DisplayWindow.PreviewKeyUp += new KeyEventHandler(DisplayWindow_PreviewKeyUp);

            }
        }

        private void DisplayWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (m_SelectedElement != null && _parentPanel.Children.Contains(m_SelectedElement))            
                SetMoveEnded();             
        }

        private void DisplayWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (m_SelectedElement != null && 
                _parentPanel.Children.Contains(m_SelectedElement) && !m_IsDown)
            {                 
                double newTop = -1; 
                double newLeft = -1;

                double currentTop = UIHelper.GetTop(m_SelectedElement, _parentPanelType);
                double currentLeft = UIHelper.GetLeft(m_SelectedElement, _parentPanelType); 
 
                switch(e.Key) 
                {
                    case Key.Left:
                        if (currentLeft - m_HorizontalMoveDistance >= 0)
                        {
                            newLeft = currentLeft - m_HorizontalMoveDistance;
                            SetMoveStarted(); 
                        }
                            break; 
                    case Key.Right:
                            if (currentLeft + m_HorizontalMoveDistance <=
                                _parentPanel.Width)
                            {
                                newLeft = currentLeft + m_HorizontalMoveDistance;
                                SetMoveStarted(); 
                            }
                        break;
                    case Key.Up:
                        if (currentTop - m_VerticalMoveDistance >= 0)
                        {
                            newTop = currentTop - m_VerticalMoveDistance;
                            SetMoveStarted(); 
                        }
                        break; 
                    case Key.Down:
                        if (currentTop + m_VerticalMoveDistance <=
                                _parentPanel.Height)
                        {
                            newTop = currentTop + m_VerticalMoveDistance;
                            SetMoveStarted(); 
                        }
                        break; 
                }
                
                if(newLeft != -1) 
                    UIHelper.SetLeft(m_SelectedElement, _parentPanelType, newLeft); 

                if(newTop != -1)
                    UIHelper.SetTop(m_SelectedElement, _parentPanelType, newTop);                     
            }
        }

        private void SetMoveStarted()
        {
            if (!m_HasMoveStarted)
            {
                m_HasMoveStarted = true;
                AddUndo(m_SelectedElement); 
                //m_MoveStartLeft = UIHelper.GetLeft(m_SelectedElement, _parentPanelType);
                //m_MoveStartTop = UIHelper.GetTop(m_SelectedElement, _parentPanelType);
            }
        }

        private void SetMoveEnded()
        {
            if (m_HasMoveStarted)
                m_HasMoveStarted = false;
        }

        private void MainCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_parentPanel == e.Source || m_HasMoveStarted)
                return;

            m_IsDown = true;
            m_StartPoint = e.GetPosition(_parentPanel);
            m_OriginalElement = (UIElement)e.Source;
            _parentPanel.CaptureMouse();
            e.Handled = true;
        }

        private void MainCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (m_IsDown)
            {
                if (!m_IsDragging &&
                    IsElementMoved(e.GetPosition(_parentPanel).X, e.GetPosition(_parentPanel).Y))
                    DragStarted();

                if (m_IsDragging)
                    DragMoved();
            }
        }

        private void MainCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (m_IsDown)
                DragFinished(false);
            if (IsElementMoved(e.GetPosition(_parentPanel).X, e.GetPosition(_parentPanel).Y))
                e.Handled = true;
        }

        private bool IsElementMoved(double currentX, double currentY)
        {
            return Math.Abs(currentX - m_StartPoint.X) > SystemParameters.MinimumHorizontalDragDistance &&
                 Math.Abs(currentY - m_StartPoint.Y) > SystemParameters.MinimumVerticalDragDistance;
        }

        private void DragStarted()
        {
            m_IsDragging = true;

            m_OriginalLeft = UIHelper.GetLeft(m_OriginalElement, _parentPanelType);
            m_OriginalTop = UIHelper.GetTop(m_OriginalElement, _parentPanelType);

            //' Add a rectangle to indicate where we'll end up.
            //' We'll just use an alpha-blended visual brush.
            VisualBrush brush;
            brush = new VisualBrush(m_OriginalElement);
            brush.Opacity = 0.5;

            m_OverlayElement = new Rectangle();
            m_OverlayElement.Width = m_OriginalElement.RenderSize.Width;
            m_OverlayElement.Height = m_OriginalElement.RenderSize.Height;
            m_OverlayElement.Fill = brush;

            _parentPanel.Children.Add(m_OverlayElement);
        }

        private void DragMoved()
        {
            Point currentPosition = System.Windows.Input.Mouse.GetPosition(_parentPanel);
            double elementLeft = (currentPosition.X - m_StartPoint.X) + m_OriginalLeft;
            double elementTop = (currentPosition.Y - m_StartPoint.Y) + m_OriginalTop;
            UIHelper.SetLeft(m_OverlayElement, _parentPanelType, elementLeft);
            UIHelper.SetTop(m_OverlayElement, _parentPanelType, elementTop);
        }

        private void DragFinished(bool canceled)
        {
            System.Windows.Input.Mouse.Capture(null);
            if (m_IsDragging)
            {
                _parentPanel.Children.Remove(m_OverlayElement);
                if (!canceled)
                {
                    AddUndo(m_OriginalElement);
                    UIHelper.SetLeft(m_OriginalElement, _parentPanelType,
                        UIHelper.GetLeft(m_OverlayElement, _parentPanelType));
                    UIHelper.SetTop(m_OriginalElement, _parentPanelType,
                        UIHelper.GetTop(m_OverlayElement, _parentPanelType));
                }

                m_OverlayElement = null;
            }
            m_IsDragging = false;
            m_IsDown = false;
        }

        private void AddUndo(UIElement element)
        {
            if (_undoOperationsContainer != null)
            {
                UIElementUndoCommand undoCommand = new UIElementUndoCommand(element);
                undoCommand.StateDictionary.Add("Left", 
                    UIHelper.GetLeft(element, _parentPanelType));
                undoCommand.StateDictionary.Add("Top", 
                    UIHelper.GetTop(element, _parentPanelType));

                UndoableOperation<UIElementUndoCommand> operation =
                    new UndoableOperation<UIElementUndoCommand>(undoCommand, this);
                _undoOperationsContainer.AddOperation(operation); 
            }
        }
        

        #region IUndoCommandImplementor<UIElementUndoCommand> Members

        public bool Undo(UIElementUndoCommand operation)
        {
            UIElement elment = operation.Element;
            UIHelper.SetLeft(elment, _parentPanelType,
                (double)operation.StateDictionary["Left"]);
            UIHelper.SetTop(elment, _parentPanelType, 
                (double)operation.StateDictionary["Top"]);
            return true; 
        }

        #endregion
    }
}
