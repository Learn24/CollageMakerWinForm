   M S B A M L   `   `   `  ����Z  VAdsophic.PhotoEditor.GUILibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullzVclr-namespace:Adsophic.PhotoEditor.GUILibrary;assembly=Adsophic.PhotoEditor.GUILibraryAdsophic.PhotoEditor.GUILibrary  5   	    PhotoEditor$   PhotoEditor.PhotoCollageWindow   5   	   \ XPresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      8x,http://schemas.microsoft.com/winfx/2006/xaml    5      flocalVclr-namespace:Adsophic.PhotoEditor.GUILibrary;assembly=Adsophic.PhotoEditor.GUILibrary    5   	   -   5          �Name$  
MainWindow��5       � Title$ Photo Collage��6    � SizeToContent$ WidthAndHeight=�6/    � 
ResizeMode$ CanMinimize=�6N    � Icon$ AppIcon.ico��6g   .��5      � -   5        $��MainGrid��5       � RowDefinitions 5   
   �� 5      $��20 �6   6*   �� 5	      $��* �6   6)    � ColumnDefinitions 5   
   �� 5      $��* �6   6+   .+�5   
   o� -   5        $��MainMenu��5   >    o� 
IsMainMenu . $��0��6!   $��0��6.   .@�5      m� -   5        $
��Main��5      $
��Main��6#   .@�5      m� -   5        $��ChangeMainImage��5      $"��Change Background Picture...��62    m� InputGestureText$ Ctrl+M��6w   m� 5      -   5        $��AddImageOpen��5      $��Add Collage Picture...��6/   $ Ctrl+A��6f   m� 5      -   5        $��	SaveImage��5      $��Save Collage...��6,   $ Ctrl+S��6]   m� 5      -   5        $��SaveImageAs��5      $��Save Collage As...��6.   6J   m� 5      -	   5        $��CloseWindow��5      $��Close��6.   $ Ctrl+W��6X   5      m� 5      -
   5        $
��Edit��5      $��_Edit��6#   .@�5      m� -   5        $��UndoLastAction��5      $
��Undo��61   $ Ctrl+Z��6\   5      5      5      5(      