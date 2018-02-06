using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Adsophic.Common.Util.Operations.Undo;
using Adsophic.PhotoEditor.GUILibrary.Operations.Undo;
using System.Windows.Shapes;

namespace Adsophic.PhotoEditor.GUILibrary.Operations
{
    public class ResizeChildElementImplementor : IUndoCommandImplementor<UIElementUndoCommand>
    {
        private Panel _parentPanel;
        private PanelType _parentPanelType;
        private IUndoOperationsContainer<UIElementUndoCommand> _undoOperationsContainer; 
        private Adorner _adorner = null;
        

        /// <summary>
        /// Raised when an image is resized either by mouse or by an undo event.  
        /// </summary>
        public event SizeChangedEventHandler ImageResized; 

        private AdornerLayer _adornerLayer;

        public ResizeChildElementImplementor(Panel parentPanel_,
            IUndoOperationsContainer<UIElementUndoCommand> undoOperationsContainer_)
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

        private void SetupMainCanvasEvents()
        {
            _parentPanel.Loaded += new RoutedEventHandler(ParentPanel_Loaded);
        }

        private void ParentPanel_Loaded(object sender, RoutedEventArgs e)
        {
            _adornerLayer = AdornerLayer.GetAdornerLayer(_parentPanel);
        }

        internal Panel ParentPanel { get { return _parentPanel; } }

        internal PanelType ParentPanelType { get { return _parentPanelType; } }

        internal bool RaiseResizeCompletedEvent(SizeChangedEventArgs args)
        {
            if (ImageResized != null)
            {
                ImageResized(this, args); 
                return true; 
            }
            return false; 
        }

        public void AdornElement(UIElement uiElement)
        {
            if (_adorner != null)
            {
                _adornerLayer.Remove(_adorner);
                _adorner = null; 
            }

            if (uiElement != null)
            {
                _adorner = new ResizingAdorner(uiElement, this);
                _adornerLayer.Add(_adorner);
            }
        }

        #region IUndoCommandImplementor<UIElementUndoCommand> Members

        public bool Undo(UIElementUndoCommand operation)
        {
            if (ImageResized != null)
            {
                ImageResized(this, new SizeChangedEventArgs(operation.Element,
                    (double)operation.StateDictionary["Width"],
                    (double)operation.StateDictionary["Height"],
                    ResizeType.Absolute));
                return true;
            }
            else
            {
                FrameworkElement element = operation.Element as FrameworkElement;

                element.Width = (double)operation.StateDictionary["Width"];
                element.Height = (double)operation.StateDictionary["Height"];
            }
            return true; 
        }

        #endregion

        private void AddUndo(FrameworkElement element)
        {
            if (_undoOperationsContainer != null)
            {
                UIElementUndoCommand undoCommand = new UIElementUndoCommand(element);
                undoCommand.StateDictionary.Add("Height", element.Height);
                undoCommand.StateDictionary.Add("Width", element.Width);

                UndoableOperation<UIElementUndoCommand> operation =
                    new UndoableOperation<UIElementUndoCommand>(undoCommand, this);
                _undoOperationsContainer.AddOperation(operation);
            }
        }

        private class ResizingAdorner : Adorner
        {
            // Resizing adorner uses Thumbs for visual elements.  
            // The Thumbs have built-in mouse input handling.
            private Thumb bottomRight;
            private ResizeChildElementImplementor _resizeImplementor;
            private Rectangle _overlayRectangle = null;
            private bool _hasResizeStarted = false;
            private double _originalWidth = 0;
            private double _originalHeight = 0; 
            

            // To store and manage the adorner's visual children.
            VisualCollection visualChildren;

            // Initialize the ResizingAdorner.
            public ResizingAdorner(UIElement adornedElement, ResizeChildElementImplementor resizeImplementor)
                : base(adornedElement)
            {
                _resizeImplementor = resizeImplementor; 
                visualChildren = new VisualCollection(this);

                BuildAdornerCorner(ref bottomRight, Cursors.Cross);

                bottomRight.DragCompleted += new DragCompletedEventHandler(HandleDragCompleted);
                // Add handlers for resizing.
                bottomRight.DragDelta += new DragDeltaEventHandler(HandleBottomRight);
            }

            void HandleDragCompleted(object sender, DragCompletedEventArgs e)
            {
                FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
                Thumb hitThumb = sender as Thumb;

                if (adornedElement == null || hitThumb == null) return;
                FrameworkElement parentElement = adornedElement.Parent as FrameworkElement;

                // Ensure that the Width and Height are properly initialized after the resize.
                //EnforceSize(adornedElement);

                _resizeImplementor.AddUndo(adornedElement); 

                if (!_resizeImplementor.RaiseResizeCompletedEvent(
                    new SizeChangedEventArgs(adornedElement, 
                        e.HorizontalChange, 
                        e.VerticalChange, ResizeType.Change)
                    ))
                {

                    // Change the size by the amount the user drags the mouse, as long as it's larger 
                    // than the width or height of an adorner, respectively.
                    adornedElement.Width = Math.Max(adornedElement.Width + e.HorizontalChange, hitThumb.DesiredSize.Width);
                    adornedElement.Height = Math.Max(e.VerticalChange + adornedElement.Height, hitThumb.DesiredSize.Height);
                }

                _hasResizeStarted = false;
                _resizeImplementor.ParentPanel.Children.Remove(_overlayRectangle);
                _overlayRectangle = null; 
            }

            // Handler for resizing from the bottom-right.
            void HandleBottomRight(object sender, DragDeltaEventArgs args)
            {
                FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
                Thumb hitThumb = sender as Thumb;

                if (adornedElement == null || hitThumb == null) return;
                FrameworkElement parentElement = adornedElement.Parent as FrameworkElement;

                if (!_hasResizeStarted)
                {
                    ResizeStarted(adornedElement); 
                }
                // Ensure that the Width and Height are properly initialized after the resize.
                //EnforceSize(_overlayRectangle);

                // Change the size by the amount the user drags the mouse, as long as it's larger 
                // than the width or height of an adorner, respectively.
                
                _overlayRectangle.Width = Math.Max(_originalWidth + args.HorizontalChange, hitThumb.DesiredSize.Width);
                _overlayRectangle.Height = Math.Max(_originalHeight + args.VerticalChange, hitThumb.DesiredSize.Height);
                //System.Diagnostics.Debug.WriteLine("New Width" + _overlayRectangle.Width + " Change:" + args.HorizontalChange);
            }

            private void ResizeStarted(FrameworkElement adornedElement)
            {
                _hasResizeStarted = true;

                //' Add a rectangle to indicate where we'll end up.
                //' We'll just use an alpha-blended visual brush.
                VisualBrush brush;
                brush = new VisualBrush(adornedElement);
                brush.Opacity = 0.5;

                _overlayRectangle = new Rectangle();
                _originalWidth = adornedElement.RenderSize.Width;
                _originalHeight = adornedElement.RenderSize.Height;
                _overlayRectangle.Width = adornedElement.RenderSize.Width;
                _overlayRectangle.Height = adornedElement.RenderSize.Height;
                _overlayRectangle.Fill = brush;
                UIHelper.SetLeft(_overlayRectangle, _resizeImplementor.ParentPanelType, 
                    UIHelper.GetLeft(adornedElement, _resizeImplementor.ParentPanelType));
                UIHelper.SetTop(_overlayRectangle, _resizeImplementor.ParentPanelType,
                    UIHelper.GetTop(adornedElement, _resizeImplementor.ParentPanelType));

                _resizeImplementor.ParentPanel.Children.Add(_overlayRectangle);
            }

            

            // Arrange the Adorners.
            protected override Size ArrangeOverride(Size finalSize)
            {
                // desiredWidth and desiredHeight are the width and height of the element that's being adorned.  
                // These will be used to place the ResizingAdorner at the corners of the adorned element.  
                double desiredWidth = AdornedElement.DesiredSize.Width;
                double desiredHeight = AdornedElement.DesiredSize.Height;
                // adornerWidth & adornerHeight are used for placement as well.
                double adornerWidth = this.DesiredSize.Width;
                double adornerHeight = this.DesiredSize.Height;

                bottomRight.Arrange(new Rect(desiredWidth - adornerWidth / 2, desiredHeight - adornerHeight / 2, adornerWidth, adornerHeight));

                // Return the final size.
                return finalSize;
            }

            // Helper method to instantiate the corner Thumbs, set the Cursor property, 
            // set some appearance properties, and add the elements to the visual tree.
            void BuildAdornerCorner(ref Thumb cornerThumb, Cursor customizedCursor)
            {
                if (cornerThumb != null) return;

                cornerThumb = new Thumb();

                // Set some arbitrary visual characteristics.
                cornerThumb.Cursor = customizedCursor;
                cornerThumb.Height = cornerThumb.Width = 10;
                cornerThumb.Opacity = 0.40;
                cornerThumb.Background = new SolidColorBrush(Colors.MediumBlue);

                visualChildren.Add(cornerThumb);
            }

            // This method ensures that the Widths and Heights are initialized.  Sizing to content produces
            // Width and Height values of Double.NaN.  Because this Adorner explicitly resizes, the Width and Height
            // need to be set first.  It also sets the maximum size of the adorned element.
            void EnforceSize(FrameworkElement adornedElement)
            {
                if (adornedElement.Width.Equals(Double.NaN))
                    adornedElement.Width = adornedElement.DesiredSize.Width;
                if (adornedElement.Height.Equals(Double.NaN))
                    adornedElement.Height = adornedElement.DesiredSize.Height;

                FrameworkElement parent = adornedElement.Parent as FrameworkElement;
                if (parent != null)
                {
                    adornedElement.MaxHeight = parent.ActualHeight;
                    adornedElement.MaxWidth = parent.ActualWidth;
                }
            }
            // Override the VisualChildrenCount and GetVisualChild properties to interface with 
            // the adorner's visual collection.
            protected override int VisualChildrenCount { get { return visualChildren.Count; } }
            protected override Visual GetVisualChild(int index) { return visualChildren[index]; }
        }

       
    }
}
