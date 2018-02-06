using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Linq.Expressions;

namespace Adsophic.PhotoEditor.GUILibrary.KeyboardSupport
{
    /// <summary>
    /// Simplifies the task of setting up shortcuts. 
    /// </summary>
    public class ShortcutManager
    {
        private FrameworkElement _frameworkElement;
        private Window _window;

        private Dictionary<ShortcutKey, IList<Func<bool>>> _shortcutHandlers =
            new Dictionary<ShortcutKey, IList<Func<bool>>>();

        public ShortcutManager(FrameworkElement frameworkElement)
        {
            _frameworkElement = frameworkElement;
            _frameworkElement.Loaded += new RoutedEventHandler(OnElementLoaded);
        }

        public ShortcutManager(Window window)
        {
            if (window == null)
                throw new ArgumentNullException("Window argument cannot be null");
            _window = window;
            _window.Loaded += new RoutedEventHandler(OnWindowLoaded);
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            _window.PreviewKeyDown += new KeyEventHandler(PreviewKeydown);
            _window.PreviewKeyUp += new KeyEventHandler(PreviewKeyUp);
        }

        private void OnElementLoaded(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow((DependencyObject)sender);

            if (parentWindow != null)
            {
                parentWindow.PreviewKeyDown += new KeyEventHandler(PreviewKeydown);
                parentWindow.PreviewKeyUp += new KeyEventHandler(PreviewKeyUp);
            }
        }


        public void RegisterShorcut(ShortcutKey key, Func<bool> handler)
        {
            IList<Func<bool>> handlerList = GetOrCreateShortcutList(key);
            handlerList.Add(handler); 
        }


        private IList<Func<bool>> GetOrCreateShortcutList(ShortcutKey key)
        {
            IList<Func<bool>> handlerList = null;
            if (!_shortcutHandlers.TryGetValue(key, out handlerList))
            {
                handlerList = new List<Func<bool>>();
                _shortcutHandlers.Add(key, handlerList);
            }

            return handlerList;
        }

        private void PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (!e.IsRepeat)
            {
                ShortcutKey key = new ShortcutKey(e);

                IList<Func<bool>> handlerList = null;
                if (_shortcutHandlers.TryGetValue(key, out handlerList))
                    foreach (Func<bool> handler in handlerList)
                    {
                        if (handler())
                            break; 
                    }
            }
        }

        private void PreviewKeydown(object sender, KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("is Repeated:" + e.IsRepeat + " key value:" + e.Key);
        }
    }
}
