using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Windows;
using Adsophic.PhotoEditor.GUILibrary;
using System.Windows.Data;
using System.Windows.Media;
using SizeChangedEventHandler = Adsophic.PhotoEditor.GUILibrary.Operations.SizeChangedEventHandler;
using SizeChangedEventArgs = Adsophic.PhotoEditor.GUILibrary.Operations.SizeChangedEventArgs;
using System.Windows.Shapes;
using PhotoEditor.Util;
using System.Windows.Input;
using Adsophic.PhotoEditor.GUILibrary.Operations;
using Adsophic.PhotoEditor.GUILibrary.Operations.Undo;
using Adsophic.Common.Util.Operations.Undo;
using Adsophic.Common.Util.DataStructures;
using System.Windows.Documents;
using Adsophic.PhotoEditor.GUILibrary.Imaging;
using Adsophic.PhotoEditor.GUILibrary.KeyboardSupport;

namespace PhotoEditor
{
    internal class DisplayPhotoCanvas : PhotoCanvas,
        IUndoCommandImplementor<UIElementUndoCommand>
    {
        private enum OperationType
        {
            AddImage,ChangeMainImage
        }
        
        DragMoveChildElementImplementor _dragDropImplementor = null;
        ResizeChildElementImplementor _resizeImplementor = null;
        DeleteChildElementImplementor _deleteChildImplementor = null;
        ShortcutManager _shortcutManager;
        
        private double _maxWidth = 800d;
        private double _maxHeight = 600d;
        UndoOperationsImplementor<UIElementUndoCommand> _undoImplementor = new UndoOperationsImplementor<UIElementUndoCommand>();  

        public DisplayPhotoCanvas(Uri mainImageUri, string borderColor, double borderWidth)
            :base(mainImageUri, borderColor, borderWidth)
        {            
            _dragDropImplementor = new DragMoveChildElementImplementor(_mainCanvas, _undoImplementor);
            _resizeImplementor = new ResizeChildElementImplementor(_mainCanvas, _undoImplementor);
            _deleteChildImplementor = new DeleteChildElementImplementor(_mainCanvas, _undoImplementor);
            _deleteChildImplementor.Deleted += new DeletedEventHandler(OnImageDeleted);

            _resizeImplementor.ImageResized += new 
                SizeChangedEventHandler(OnImageResized);
            _shortcutManager = new ShortcutManager(_mainCanvas);

            RegisterShortcutKeys(); 

            _mainCanvas.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(MainCanvas_PreviewMouseLeftButtonUp);            
        }

        private void RegisterShortcutKeys()
        {
            _shortcutManager.RegisterShorcut(
                new ShortcutKey(new ModifierKeys[] { ModifierKeys.Control }, Key.Z),
                () =>
                {
                    UndoLastOperation();
                    return true;
                });
        }

        public UndoOperationsImplementor<UIElementUndoCommand> UndoImplementor { get { return _undoImplementor; } }

        private void OnImageDeleted(object sender, DeletedEventArgs args)
        {
            Canvas imageCanvas = (Canvas)args.UIElement;
            switch (args.DeleteAction)
            {
                case DeleteAction.Deleted:
                    if (_imageDictionary.ContainsKey(imageCanvas.Name))
                        _imageDictionary.Remove(imageCanvas.Name);
                    break; 

                case DeleteAction.DeleteUndone:
                    if(!_imageDictionary.ContainsKey(imageCanvas.Name))
                        _imageDictionary.Add(imageCanvas.Name, new InsetImage(imageCanvas));
                    break; 
            }
        }

        public DisplayPhotoCanvas(string borderColor, double borderWidth)
            : this(null, borderColor, borderWidth) 
        {
        }

        protected override double MaxHeight { get { return _maxHeight; } }

        protected override double MaxWidth { get { return _maxWidth; } }

        public ShortcutManager ShortcutManager { get { return _shortcutManager; } }

        private void OnImageResized(object sender, SizeChangedEventArgs args)
        {
            InsetImage insetImage = null;
            if( args.UIElement is Canvas && _imageDictionary.TryGetValue(((Canvas)args.UIElement).Name, 
                out insetImage))             
                insetImage.Resize(args.X, args.Y, args.ResizeType);             
        }

        private void MainCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            UIElement uiElement = FindUIElementUnderMouseClick(e);
            _resizeImplementor.AdornElement(uiElement);
            _dragDropImplementor.SelectElement(uiElement);
            _deleteChildImplementor.SelectElement(uiElement); 
            
        }

        private UIElement FindUIElementUnderMouseClick(MouseButtonEventArgs e)
        {
            foreach (UIElement uiElement in _mainCanvas.Children)
            {
                if (uiElement is Canvas)
                {
                    Canvas canvas = (Canvas)uiElement;
                    double locationX = e.GetPosition(canvas).X;
                    double locationY = e.GetPosition(canvas).Y;
                    if (locationX >= 0 && locationX <= canvas.Width
                            && locationY >= 0 && locationY <= canvas.Height)                    
                        return uiElement;                    
                }
            }

            return null; 
        }

        public void UndoLastOperation()
        {
            _undoImplementor.UndoLastOperation();            
        }

        #region IUndoCommandImplementor<UIElementUndoCommand> Members

        public bool Undo(UIElementUndoCommand operation)
        {
            OperationType operationType = (OperationType)operation.StateDictionary["OperationType"];

            switch (operationType)
            {
                case OperationType.AddImage:
                    _mainCanvas.Children.Remove(operation.Element);
                    _imageDictionary.Remove(((Canvas)operation.Element).Name); 
                    return true; 

                case OperationType.ChangeMainImage:
                    ImageSource imageSource = (ImageSource)operation.StateDictionary["ImageSource"];
                    SetMainImage(imageSource, false); 
                    return true; 
            }
            
            return false; 
        }

        private void AddUndo(UIElementUndoCommand undoCommand)
        {
            UndoableOperation<UIElementUndoCommand> operation =
                new UndoableOperation<UIElementUndoCommand>(undoCommand, this);
            
            _undoImplementor.AddOperation(operation);

        }

        protected override void AddInsetImageUndo(UIElement element)
        {
            UIElementUndoCommand undoCommand = new UIElementUndoCommand(element);
            undoCommand.StateDictionary.Add("OperationType", OperationType.AddImage);

            AddUndo(undoCommand);
        }

        protected override void AddChangeMainImageUndo(ImageSource imageSource)
        {
            UIElementUndoCommand undoCommand = new UIElementUndoCommand(_mainImage);
            undoCommand.StateDictionary.Add("OperationType", OperationType.ChangeMainImage);
            undoCommand.StateDictionary.Add("ImageSource", _mainImage.Source);

            AddUndo(undoCommand);
        }

        #endregion

       
    }
}
