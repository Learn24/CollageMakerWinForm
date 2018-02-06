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
using System.Windows.Shapes;
using Adsophic.PhotoEditor.GUILibrary.Operations;
using Adsophic.PhotoEditor.GUILibrary.Imaging;

namespace PhotoEditor
{
    internal class InsetImage
    {
        public const double DefaultBorder = 1; 
        public const double DefaultSize = 120; 

        private static int _canvasNameNumberSuffix = 0; 
        private static int _imageNameNumberSuffix = 0;
        
        private Canvas _insetCanvas;
        private Image _insetImage; 
        private double _borderWidth;

        public InsetImage(double imageWidth,
                Uri imageUri, double borderWidth)
        {
            _borderWidth = borderWidth; 
            CreateInsetCanvasFromXmal(imageWidth, ImageUtils.GetImageFromUri(imageUri), 
                borderWidth, ScaleType.UseWidth);            
        }

        public InsetImage(Canvas canvas)
        {
            _insetCanvas = canvas;
            _insetImage = FindImage(canvas); 
        }

        public InsetImage(double imageWidth,
                ImageSource imageSource, double borderWidth)
        {
            _borderWidth = borderWidth;
            CreateInsetCanvasFromXmal(imageWidth, imageSource, borderWidth, ScaleType.UseWidth);
        }

        public InsetImage(Uri imageUri)
        {
            _borderWidth = DefaultBorder;
            CreateInsetCanvasFromXmal(DefaultSize, ImageUtils.GetImageFromUri(imageUri), 
                _borderWidth, ScaleType.Default); 
        }

        public Canvas ImageCanvas { get { return _insetCanvas; } }

        public Image DisplayImage { get { return _insetImage; } } 

        private void CreateInsetCanvasFromXmal(
            double size,
            ImageSource imageSource,
            double borderWidth, 
            ScaleType scaleType)
        {
            System.Uri imageCanvasXamlUri = new System.Uri("/PhotoEditor;component/Design/ImageCanvas.xaml", 
                System.UriKind.Relative);
            _insetCanvas = (Canvas)Application.LoadComponent(imageCanvasXamlUri);
            _insetCanvas.Name = GetCanvasName();
            
            SetChildCanvasTop(_insetCanvas, borderWidth);
            SetChildCanvasLeft(_insetCanvas, borderWidth);

            _insetImage = FindImage(_insetCanvas);
            _insetImage.Name = GetImageName();

            Binding imageSourceBinding = new Binding();
            

            if (imageSource != null)
            {
                imageSourceBinding.Source = imageSource;
                _insetImage.SetBinding(Image.SourceProperty, imageSourceBinding);

                //double imageScaling = (width - 2 * borderWidth) / _insetImage.Source.Width;

                if (scaleType != ScaleType.Default)
                    ScaleImage(size, scaleType);
                else

                    ScaleImage(size,
                        imageSource.Width > imageSource.Height ? ScaleType.UseWidth : ScaleType.UseHeight);

                /*
                SetChildrenWidths(_insetCanvas, width - 2 * borderWidth);
                double height = image.Source.Height * imageScaling;
                SetChildrenHeights(_insetCanvas, height);

                _insetCanvas.Height = height + 2 * borderWidth;
                */
            }
            else
                _insetCanvas = null;
            
        }

        private enum ScaleType 
        {
            UseWidth, 
            UseHeight,
            Default
        }

        private void ScaleImage(double imageLength, ScaleType scaleType)
        {
            switch (scaleType)
            {
                case ScaleType.UseWidth:
                    double imageScaling = (imageLength - 2 * _borderWidth) / _insetImage.Source.Width;

                    _insetCanvas.Width = imageLength;
                    SetChildrenWidths(_insetCanvas, imageLength - 2 * _borderWidth);
                    double height = _insetImage.Source.Height * imageScaling;
                    SetChildrenHeights(_insetCanvas, height);

                    _insetCanvas.Height = height + 2 * _borderWidth;
                    break;

                case ScaleType.UseHeight:
                    imageScaling = (imageLength - 2 * _borderWidth) / _insetImage.Source.Height;

                    _insetCanvas.Height = imageLength;
                    SetChildrenHeights(_insetCanvas, imageLength - 2 * _borderWidth);
                    double width = _insetImage.Source.Width * imageScaling;
                    SetChildrenWidths(_insetCanvas, width);

                    _insetCanvas.Width = width + 2 * _borderWidth;
                    break; 

                default:
                    break; 
            }
            
        }

        

        public void Resize(double newXVal, double newYVal, ResizeType resizeType )
        {
            if (resizeType == ResizeType.Change)
            {
                ScaleType scaleType = newXVal / _insetCanvas.Width >= newYVal / _insetCanvas.Height ?
                    ScaleType.UseWidth : ScaleType.UseHeight;

                if (scaleType == ScaleType.UseWidth)
                    ScaleImage(_insetCanvas.Width + newXVal, scaleType);
                else
                    ScaleImage(_insetCanvas.Height + newYVal, scaleType);
            }
            else            
                ScaleImage(newXVal, ScaleType.UseWidth);
        }

        private void SetChildrenWidths(Canvas canvas, double width)
        {
            Image image = (Image)canvas.FindName("image");
            Rectangle rectHorizontal = (Rectangle)canvas.FindName("rectHorizontal");
            Rectangle rectVertical = (Rectangle)canvas.FindName("rectVertical");

            image.Width = rectHorizontal.Width = rectVertical.Width = width;
        }

        private void SetChildCanvasTop(Canvas canvas, double top)
        {
            Image image = (Image)canvas.FindName("image");
            Rectangle rectHorizontal = (Rectangle)canvas.FindName("rectHorizontal");
            Rectangle rectVertical = (Rectangle)canvas.FindName("rectVertical");

            Canvas.SetTop(image, top);
            Canvas.SetTop(rectHorizontal, top);
            Canvas.SetTop(rectVertical, top);
        }

        private void SetChildCanvasLeft(Canvas canvas, double left)
        {
            Image image = (Image)canvas.FindName("image");
            Rectangle rectHorizontal = (Rectangle)canvas.FindName("rectHorizontal");
            Rectangle rectVertical = (Rectangle)canvas.FindName("rectVertical");

            Canvas.SetLeft(image, left);
            Canvas.SetLeft(rectHorizontal, left);
            Canvas.SetLeft(rectVertical, left);
        }

        private void SetChildrenHeights(Canvas canvas, double height)
        {
            Image image = (Image)canvas.FindName("image");
            Rectangle rectHorizontal = (Rectangle)canvas.FindName("rectHorizontal");
            Rectangle rectVertical = (Rectangle)canvas.FindName("rectVertical");

            image.Height = rectHorizontal.Height = rectVertical.Height = height;
        }

        private static Image FindImage(Canvas imageHolderCanvas)
        {
            foreach (UIElement uiElement in imageHolderCanvas.Children)
                if (uiElement is Image)
                    return (Image)uiElement;

            return null;
        }

        private string GetCanvasName()
        {
            _canvasNameNumberSuffix++;
            return "canvas" + _canvasNameNumberSuffix;
        }

        private string GetImageName()
        {
            _imageNameNumberSuffix++;
            return "image" + _imageNameNumberSuffix;
        }
    }
}
