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
    internal class PhotoCanvas 
    {
        public const double DefaultTop = 10;
        public const double DefaultLeft = 10;
 
        protected DynamicCanvas _mainCanvas;        
        protected Image _mainImage; 
        
        protected double _border = 2d;
        private double _width = 800;
        private double _height = 600; 

        protected Dictionary<string, InsetImage> _imageDictionary =
            new Dictionary<string, InsetImage>(); 


        public PhotoCanvas(Uri mainImageUri, string borderColor, double borderWidth)
        {
            _border = borderWidth;

            CreatePhotoCanvas(mainImageUri);
        }

        public PhotoCanvas(double borderWidth, double width, double height)
        {
            _border = borderWidth;
            _width = width;
            _height = height; 
            CreatePhotoCanvas(null);
        }

        /// <summary>
        /// Max Width
        /// </summary>
        protected virtual double MaxWidth { get { return _width; } }

        /// <summary>
        /// Max Height
        /// </summary>
        protected virtual double MaxHeight { get { return _height; } }

        public PhotoCanvas(string borderColor, double borderWidth) : this(null, borderColor, borderWidth) 
        {
        }

        private DynamicCanvas CreatePhotoCanvas(Uri imageUri)
        {
            _mainCanvas = new DynamicCanvas();
            _mainCanvas.Name = "ImageCanvas";
            _mainCanvas.Background = Brushes.White; 

            if (imageUri != null)
                SetMainImage(ImageUtils.GetImageFromUri(imageUri), false);

           

            return _mainCanvas; 
        }

        private void CreateMainImage()
        {
            _mainImage = new Image();
            _mainImage.Name = "ImageHolder";
            _mainImage.Stretch = System.Windows.Media.Stretch.Fill;

            _mainCanvas.Children.Add(_mainImage);

            DynamicCanvas.SetTop(_mainImage, _border);
            DynamicCanvas.SetLeft(_mainImage, _border);
            DynamicCanvas.SetZIndex(_mainImage, -500);
        }

        protected void SetMainImage(ImageSource imageSource, bool addUndo)
        {
            if (imageSource == null)
                throw new ArgumentNullException("Image Source cannot be null");
            if (_mainImage == null)
                CreateMainImage();
            else if (addUndo)
                AddChangeMainImageUndo(imageSource); 

            Binding imageBinding = new Binding();
            imageBinding.Source = imageSource;
            //imageBinding.Source = new Uri("pack://application:,,,/" + imageFile, UriKind.Absolute); 
            //pack://application:,,,/foo/bar.png
            _mainImage.SetBinding(Image.SourceProperty, imageBinding);

            SetCanvasSize(); 
        }

        public bool ChangeMainImage(Uri imageUri)
        {
            ImageSource imageSource = ImageUtils.GetImageFromUri(imageUri);
            return ChangeMainImage(imageSource, true);            
        }

        public bool ChangeMainImage(ImageSource imageSource, bool addUndo)
        {            
            if (imageSource == null) return false;
            SetMainImage(imageSource, addUndo);
            return true;
        }
        

        public bool AddInsetPhoto(Uri imageUri, double imageWidth,
            double borderWidth, double imageX, double imageY)
        {   
            InsetImage insetImage = new InsetImage(imageWidth, 
                imageUri, borderWidth);
            Canvas imageCanvas =  insetImage.ImageCanvas;

            if (imageCanvas != null)
            {
                _imageDictionary.Add(imageCanvas.Name, insetImage);

                _mainCanvas.Children.Add(imageCanvas);

                DynamicCanvas.SetLeft(imageCanvas, imageX);
                DynamicCanvas.SetTop(imageCanvas, imageY);

                AddInsetImageUndo(imageCanvas);

                return true; 
            }

            return false;
        }

        public bool AddInsetPhoto(ImageSource imageSource, double imageWidth,
            double borderWidth, double imageX, double imageY)
        {
            InsetImage insetImage = new InsetImage(imageWidth,
                imageSource, borderWidth);
            Canvas imageCanvas = insetImage.ImageCanvas;

            if (imageCanvas != null)
            {                
                _imageDictionary.Add(imageCanvas.Name, insetImage);

                _mainCanvas.Children.Add(imageCanvas);

                DynamicCanvas.SetLeft(imageCanvas, imageX);
                DynamicCanvas.SetTop(imageCanvas, imageY);

                AddInsetImageUndo(imageCanvas);

                return true;
            }

            return false;
        }

        public bool AddInsetPhoto(Uri imageUri)
        {
            InsetImage insetImage = new InsetImage(imageUri);
            Canvas imageCanvas = insetImage.ImageCanvas;

            if (imageCanvas != null)
            {
                _imageDictionary.Add(imageCanvas.Name, insetImage);

                _mainCanvas.Children.Add(imageCanvas);

                DynamicCanvas.SetLeft(imageCanvas, DefaultLeft);
                DynamicCanvas.SetTop(imageCanvas, DefaultTop);

                AddInsetImageUndo(imageCanvas); 
                return true; 
            }

            return false; 
        }

        private void SetCanvasSize()
        {   
            double totalRequiredWidth = _mainImage.Source.Width + 2 * _border; 
            double totalRequiredHeight = _mainImage.Source.Height + 2 * _border; 
            if (totalRequiredHeight < MaxHeight && 
                 totalRequiredWidth < MaxWidth)
            {
                double width = _mainImage.Source.Width + 2 * _border; 
                double height = _mainImage.Source.Height + 2 * _border;
                _mainCanvas.Width = width;
                _mainCanvas.Height = height;
                return; 
            }

            if (_mainImage.Source.Width > _mainImage.Source.Height &&
                totalRequiredWidth >= MaxWidth)
            {
                double imageScale = MaxWidth / _mainImage.Source.Width; 
                _mainCanvas.Width = MaxWidth;
                double height = _mainImage.Source.Height * imageScale;
                _mainCanvas.Height = height;
 
                ChangeMainImageSize(MaxWidth, height); 
            }

            else if (totalRequiredHeight >= MaxHeight)
            {
                double imageScale = MaxHeight / _mainImage.Source.Height; 
                _mainCanvas.Height = MaxHeight;
                double width = _mainImage.Source.Width * imageScale;
                _mainCanvas.Width = width; 
                ChangeMainImageSize(width,  MaxHeight);
            }

            return;
        }
        

        public DynamicCanvas MainCanvas { get { return _mainCanvas; } }


        private void ChangeMainImageSize(double newWidth, double newHeight)
        {
            _mainImage.Width = newWidth - 2 * _border;
            _mainImage.Height = newHeight - 2 * _border; 
        }

        protected virtual void AddInsetImageUndo(UIElement element)
        {
            
        }

        protected virtual void AddChangeMainImageUndo(ImageSource imageSource)
        {
        }

        public void SaveCanvasAsImage(string imageFileName)
        {
            double newWidth = this._mainImage.Source.Width;
            double newHeight = this._mainImage.Source.Height;

            double widthAspectRatio = newWidth / this._mainCanvas.ActualWidth;
            double heightAspectRatio = newHeight / this._mainCanvas.ActualHeight; 

            PhotoCanvas saveCanvas = new PhotoCanvas(this._border, newWidth,
                newHeight);

            saveCanvas.ChangeMainImage(this._mainImage.Source, false);

            foreach (InsetImage insetImage in this._imageDictionary.Values)
            {
                double newX = DynamicCanvas.GetLeft(insetImage.ImageCanvas) * widthAspectRatio;
                double newY = DynamicCanvas.GetTop(insetImage.ImageCanvas) * heightAspectRatio;

                double newImageWidth = insetImage.ImageCanvas.Width * widthAspectRatio;

                saveCanvas.AddInsetPhoto(insetImage.DisplayImage.Source, newImageWidth, this._border,
                    newX, newY); 
            }
            
            ImageUtils.SaveDynamicCanvas(saveCanvas._mainCanvas, 96, imageFileName); 
        }
    }
}
