using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adsophic.Common.Util.Operations.Undo;
using Adsophic.PhotoEditor.GUILibrary.Operations.Undo;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;

namespace Adsophic.PhotoEditor.GUILibrary.Operations
{
    public class DeleteChildElementImplementor : IUndoCommandImplementor<UIElementUndoCommand>
    {
        private Panel _parentPanel;
        private PanelType _parentPanelType;
        private IUndoOperationsContainer<UIElementUndoCommand> _undoOperationsContainer;
        private UIElement m_SelectedElement = null;
        private Window m_DisplayWindow = null;
        public event DeletedEventHandler Deleted; 

        public DeleteChildElementImplementor(Panel parentPanel_, 
            IUndoOperationsContainer<UIElementUndoCommand> undoOperationsContainer_)
        {
            if (!(parentPanel_ is DynamicCanvas) && !(parentPanel_ is Canvas))
                MessageBox.Show("Current only Canvas and Dynamic Canvas are supported by the drag drop implementor");

            if (parentPanel_ is Canvas)
                _parentPanelType = PanelType.Canvas;
            else
                _parentPanelType = PanelType.DynamicCanvas;

            _parentPanel = parentPanel_;
            _parentPanel = parentPanel_;
            _undoOperationsContainer = undoOperationsContainer_; 
        }

        public void SelectElement(UIElement uiElement)
        {
            m_SelectedElement = uiElement;
            if (m_SelectedElement != null)
            {
                if (m_DisplayWindow != null)
                {                   
                    m_DisplayWindow.PreviewKeyUp += new KeyEventHandler(DisplayWindow_PreviewKeyUp);
                }

                m_DisplayWindow = Window.GetWindow((DependencyObject)uiElement);                
                m_DisplayWindow.PreviewKeyUp += new KeyEventHandler(DisplayWindow_PreviewKeyUp);
            }
        }

        private void DisplayWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (m_SelectedElement != null)
            {
                switch (e.Key)
                {
                    case Key.Delete:
                        if (_parentPanel.Children.Contains(m_SelectedElement))
                        {
                            AddUndo(m_SelectedElement);
                            if (Deleted != null)
                                Deleted(this, new DeletedEventArgs(m_SelectedElement, DeleteAction.Deleted));
                            _parentPanel.Children.Remove(m_SelectedElement);
                        }
                        
                        break; 
                }
            }
                

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
            UIElement element = operation.Element;
            if (Deleted != null)
                Deleted(this, new DeletedEventArgs(element, DeleteAction.DeleteUndone));
            _parentPanel.Children.Add(element); 
            UIHelper.SetLeft(element, _parentPanelType,
                (double)operation.StateDictionary["Left"]);
            UIHelper.SetTop(element, _parentPanelType,
                (double)operation.StateDictionary["Top"]);
            return true; 
        }

        #endregion
    }
}
