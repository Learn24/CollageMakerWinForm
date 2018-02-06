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
using System.Windows.Shapes;
using System.Windows.Interop;
using Adsophic.PhotoEditor.GUILibrary.CommonDialog;
using Adsophic.PhotoEditor.GUILibrary.KeyboardSupport;

namespace PhotoEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<string, PhotoCollageWindow> _photoCollageWindows = null;
        private const string WINDOW_ID_PREFIX = "PhotoWindow";
        private static int sm_currentPrefix = 1;
        private FileDialogExtender _dialogExtender;        
        ShortcutManager _shortcutManager = null; 

        public MainWindow()
        {
            InitializeComponent();
            _photoCollageWindows = new Dictionary<string, PhotoCollageWindow>();
            _dialogExtender = new FileDialogExtender(FileDialogExtender.DialogViewTypes.Thumbnails, true);
            _shortcutManager = new ShortcutManager(this);

            RegisterShortcutKeys(); 
        }

        private void RegisterShortcutKeys()
        {
            _shortcutManager.RegisterShorcut(
                new ShortcutKey(new ModifierKeys[] { ModifierKeys.Control }, Key.N),
                () =>
                {
                    NewCollageWindow_Click(this, null);
                    return true;
                });
        }

        
        /*
        private void LaunchWindow_Click(object sender, RoutedEventArgs e)
        {
            if (_photoCollageWindow == null)
            {
                _photoCollageWindow = new PhotoCollageWindow(); 
            }

            _photoCollageWindow.Show(); 
        }

        private void SaveWindowAsPhoto_Click(object sender, RoutedEventArgs e)
        {
            if (_photoCollageWindow != null)
            {
                _photoCollageWindow.SaveCollage("SavedImage.png");
            }
            else
            {
                MessageBox.Show("Create the PhotoCollageWindow by clicking Launch Window");
            }
        }

        private void UndoLastCommand_Click(object sender, RoutedEventArgs e)
        {
            if (_photoCollageWindow != null)
                _photoCollageWindow.Undo(); 
        }

        private void DisplayAdornment_Click(object sender, RoutedEventArgs e)
        {
            if (_photoCollageWindow != null)
                _photoCollageWindow.AddAdornement(); 
        }
         * */

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //Do this for all existing collage windows
            while(_photoCollageWindows.Count > 0) 
            {
                //Get the first window key
                string windowKey = _photoCollageWindows.Keys.FirstOrDefault( x => true);
                //Get the window
                PhotoCollageWindow photoCollageWindow = _photoCollageWindows[windowKey];
                //Try closing the window
                photoCollageWindow.Close();

                //If window exists in the collection that means it was not closed
                //Cancel closing the application
                //We are subscribing to the window closed event below where this 
                //key will be removed from the collection. 
                if (_photoCollageWindows.ContainsKey(windowKey))
                {
                    e.Cancel = true;
                    break;
                }
            }
            base.OnClosing(e);
        }

        private void NewCollageWindow_Click(object sender, RoutedEventArgs e)
        {
            PhotoCollageWindow collageWindow = new PhotoCollageWindow(_dialogExtender);            
            collageWindow.WindowId = GetNextWindowId();
            _photoCollageWindows.Add(collageWindow.WindowId, collageWindow); 

            collageWindow.Closed += new EventHandler(OnCollageWindowClosed);
            collageWindow.Loaded += new RoutedEventHandler(collageWindow_Loaded);
            collageWindow.Show(); 
        }

        public void collageWindow_Loaded(object sender, RoutedEventArgs e)
        {
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper((Window)sender).Handle);
            source.AddHook(new HwndSourceHook(WndProc));
        }

        private void OnCollageWindowClosed(object sender, EventArgs e)
        {
            PhotoCollageWindow senderWindow = (PhotoCollageWindow)sender;
            _photoCollageWindows.Remove(senderWindow.WindowId);
            //HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper((Window)sender).Handle);
            //source.RemoveHook(new HwndSourceHook(WndProc));
        }

        private void ExitApplication_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); 
        }

        private static string GetNextWindowId()
        {
            sm_currentPrefix++;
            return WINDOW_ID_PREFIX + sm_currentPrefix;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            _dialogExtender.WndProc(hwnd, msg, wParam, lParam, ref handled);
            return IntPtr.Zero;
        }
        
    }
}
