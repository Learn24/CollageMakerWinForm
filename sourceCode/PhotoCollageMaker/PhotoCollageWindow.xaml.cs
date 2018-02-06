using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;
using Microsoft.Win32;
using Adsophic.PhotoEditor.GUILibrary.CommonDialog;
using Adsophic.PhotoEditor.GUILibrary.KeyboardSupport;
using Adsophic.PhotoEditor.GUILibrary.WinAPI;
using Adsophic.PhotoEditor.GUILibrary.Operations.Undo;
using Adsophic.Common.Util.Operations.Undo;
using Adsophic.Common.Util.ComponentModel;
using System.Text.RegularExpressions;

namespace PhotoEditor
{
    /// <summary>
    /// Interaction logic for PhotoCollageWindow.xaml
    /// </summary>
    public partial class PhotoCollageWindow : Window
    {
        private DisplayPhotoCanvas _photoCanvas = null;
        public string WindowId { get; set; }
        private FileDialogExtender _fileDialogExtender;
        ShortcutManager _shortcutManager = null;
        private bool _unsavedChanges = false;
        private string _savedFileName = string.Empty;
        private static Regex regex = new Regex(
                              @"[^\\]*$",
                            RegexOptions.IgnoreCase
                            | RegexOptions.CultureInvariant
                            | RegexOptions.IgnorePatternWhitespace
                            | RegexOptions.Compiled
                            );

        private const string FILE_CHANGED = "[Changed]";


        public PhotoCollageWindow()
        {
            InitializeComponent();
            InitializeContent(); 
        }

        public PhotoCollageWindow(FileDialogExtender fileDialogExtender)
        {            
            _fileDialogExtender = fileDialogExtender; 
            InitializeComponent();
            InitializeContent();
        }

        private void InitializeContent()
        {
            _photoCanvas = new DisplayPhotoCanvas("White", 2d);
            _shortcutManager = _photoCanvas.ShortcutManager;
            this.Closing += new System.ComponentModel.CancelEventHandler(OnWindowClosing);
            _photoCanvas.UndoImplementor.Added += new EventHandler<GenericEventArgs<UndoableOperation<UIElementUndoCommand>>>(ImageChanged);
            _photoCanvas.UndoImplementor.Undone += new EventHandler<GenericEventArgs<UndoableOperation<UIElementUndoCommand>>>(ImageChanged);
            RegisterShortcutKeys(); 

            Grid.SetColumn(_photoCanvas.MainCanvas, 0);
            Grid.SetRow(_photoCanvas.MainCanvas, 1);
            MainWindow.MainGrid.Children.Add(_photoCanvas.MainCanvas);

            ChangeBackgroudImage(true);
             
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_unsavedChanges)
            {
                MessageBoxResult result = MessageBox.Show(this, "Image has not been saved. Do you want to save?", "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
                switch (result)
                {
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        break; 

                    case MessageBoxResult.No:
                        break; 

                    case MessageBoxResult.Yes:
                        if (_savedFileName != string.Empty)
                        {
                            SaveCollage(_savedFileName);
                            break; 
                        }

                        string fileName;
                        if (SaveImageDialog(out fileName))
                            SaveCollage(fileName);
                        else
                            e.Cancel = true;
                        break; 
                }
            }
        }

        /// <summary>
        /// Returns true if the window has unsaved changes. 
        /// </summary>
        public bool UnsavedChanges { get { return _unsavedChanges; } }

        private void ImageChanged(object sender, GenericEventArgs<UndoableOperation<UIElementUndoCommand>> e)
        {
            _unsavedChanges = true;
            CreateTitle();   
        }

       

        private void RegisterShortcutKeys()
        {
            _shortcutManager.RegisterShorcut(
                new ShortcutKey(new ModifierKeys[] { ModifierKeys.Control }, Key.A),
                () =>
                {
                    AddImage_Click(this, null);
                    return true;
                });

            _shortcutManager.RegisterShorcut(
                new ShortcutKey(new ModifierKeys[] { ModifierKeys.Control }, Key.M),
                () =>
                {

                    ChangeBackgroudImage(false);
                    return true;
                });

            _shortcutManager.RegisterShorcut(
                new ShortcutKey(new ModifierKeys[] { ModifierKeys.Control }, Key.S),
                () =>
                {
                    SaveImage_Click(this, null);
                    return true;
                });

            _shortcutManager.RegisterShorcut(
                new ShortcutKey(new ModifierKeys[] { ModifierKeys.Control }, Key.W),
                () =>
                {
                    CloseWindow_Click(this, null);
                    return true;
                });
        }

        public void SaveCollage(string fileName)
        {
            _photoCanvas.SaveCanvasAsImage(fileName);
            _unsavedChanges = false;
            _savedFileName = fileName;
            CreateTitle();
        }

        private void CreateTitle()
        {
            this.Title = "Photo Collage ";
            if (_savedFileName != string.Empty)
                this.Title += regex.Match(_savedFileName) + " "; 

            if (_unsavedChanges)
                this.Title = this.Title + FILE_CHANGED;
        }

        public void Undo()
        {
            _photoCanvas.UndoLastOperation();
        }

        private void AddImage_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.DefaultExt = ".jpg"; // Default file extension
            dlg.Filter = "Image Files (.jpg)|*.jpg"; // Filter files by extension
            dlg.Multiselect = false;
            dlg.Title = "Choose Picture to Insert";

            if (_fileDialogExtender != null)
                _fileDialogExtender.DialogViewType = FileDialogExtender.DialogViewTypes.Thumbnails;

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;

                if (!_photoCanvas.AddInsetPhoto(new Uri(filename, UriKind.Absolute)))
                    MessageBox.Show("Could not add image " + filename);
            }
        }


        private void ChangeBackgroudImage(bool mustChoose)
        {
            do
            {
                // Configure open file dialog box
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.FileName = "Document"; // Default file name
                dlg.DefaultExt = ".jpg"; // Default file extension
                dlg.Filter = "Image Files (.jpg)|*.jpg"; // Filter files by extension
                dlg.Multiselect = false;
                dlg.Title = "Choose Background Image";

                if (_fileDialogExtender != null)
                    _fileDialogExtender.DialogViewType = FileDialogExtender.DialogViewTypes.Thumbnails;

                // Show open file dialog box
                Nullable<bool> result = dlg.ShowDialog();

                // Process open file dialog box results
                if (result == true)
                {
                    // Open document
                    string filename = dlg.FileName;

                    if (!_photoCanvas.ChangeMainImage(new Uri(filename, UriKind.Absolute)))
                        MessageBox.Show("Could not add image " + filename);
                    else
                    {
                        ImageChanged(null, null);
                        mustChoose = false;
                    }
                }

                if (mustChoose)
                    MessageBox.Show("Please choose background image");
            }
            while (mustChoose);
        }


        private bool SaveImageDialog(out string fileName)
        {
            // Configure save file dialog box
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "image"; // Default file name
            dlg.DefaultExt = ".png"; // Default file extension
            dlg.Filter = "Image Files (.png)|*.png"; // Filter files by extension

            dlg.Title = "Save As...";
            if (_fileDialogExtender != null)
                _fileDialogExtender.DialogViewType = FileDialogExtender.DialogViewTypes.List;

            // Show save file dialog box
            Nullable<bool> okClicked = dlg.ShowDialog();

            // Process save file dialog box results
            if (okClicked == true)
            {
                fileName = dlg.FileName;
                return true;
            }

            fileName = string.Empty;
            return false;


        }

        private void ChangeMainImage_Click(object sender, RoutedEventArgs e)
        {
            ChangeBackgroudImage(false); 
        }

        
        private void UndoLastAction_Click(object sender, RoutedEventArgs e)
        {
            _photoCanvas.UndoLastOperation(); 
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); 
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            if (_savedFileName != string.Empty)
                SaveCollage(_savedFileName);
            else
            {
                string fileName;
                if (SaveImageDialog(out fileName))
                    SaveCollage(fileName);
            }
        }


        private void SaveImageAs_Click(object sender, RoutedEventArgs e)
        {
            string fileName;
            if (SaveImageDialog(out fileName))
                SaveCollage(fileName);
        }
        
    }
}
