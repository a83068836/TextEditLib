   M S B A M L   `   `   `  ����F  BTextEditLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null)clr-namespace:TextEditLibTextEditLib  5      F BAvalonEditB, Version=2.4.0.0, Culture=neutral, PublicKeyToken=nullN6clr-namespace:AvalonEditB.Editing;assembly=AvalonEditBAvalonEditB.Editing 5      R8clr-namespace:AvalonEditB.Rendering;assembly=AvalonEditBAvalonEditB.Rendering 5      7 clr-namespace:TextEditLib.ThemesTextEditLib.Themes  5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089    TextEditLib.TextEdit  �� 
TargetType �� Property �� Value%   TextEditLib.Themes.ResourceKeys   EditorBackground   EditorForeground  AvalonEditB.TextEditor"   EditorLineNumbersForeground*   #EditorCurrentLineBackgroundBrushKey&   EditorCurrentLineBorderBrushKey m� Command	 m� Icon
 J� SnapsToDevicePixels   	Image_Cut  
Image_Copy  Image_Paste  Image_Delete  
Image_Undo  
Image_Redo �� 
TargetType � 	Resources$  AvalonEditB.Rendering.TextView"  AvalonEditB.Editing.TextArea >� Brush >� 	Thickness!  System.Windows.SystemColors   HighlightTextBrushKey �� RelativeSource �� Path �� Orientation K� ZIndex �� Triggers P� Property P� Value �� 
TargetName�� 5      H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      :x,http://schemas.microsoft.com/winfx/2006/xaml     5      #localclr-namespace:TextEditLib  5      Bediting6clr-namespace:AvalonEditB.Editing;assembly=AvalonEditB  5      F	rendering8clr-namespace:AvalonEditB.Rendering;assembly=AvalonEditB  5      ,reskeys clr-namespace:TextEditLib.Themes  5      %x  '         �� 5      "    6   .�5   
   ��  � ��$ LeftToRight=�6*   �� 5   
    � ��# �  6'   �� 5   
    � ��# �  6'   �� 5   
    �@ LineNumbersForeground# �  62   �� 5   
   # �@  EditorCurrentLineBackground# �  68   �� 5   
    �@  EditorCurrentLineBorder# �  64   �� 5   
   ( �@   EditorCurrentLineBorderThickness$ 1Q�6=   �� 5   
    �@�ContextMenu 5      �� 5      .@�5      m� $	 Cut��6   $	��Cut��6-   	 5      �� 5      $��16��6$   $

 True��5   $   #���   5   $   5      m� 5      $
 Copy��6   $
��Copy��6.   	 5      �� 5      $��16��6$   $

 True��5    $   #���  5!   $   5#      m� 5$      $ Paste��6   $��Paste��6/   	 5%      �� 5&      $��16��6$   $

 True��5'   $   #���  5(   $   5*      m� 5+      $ Delete��6   $��Delete��60   	 5,      �� 5-      $��16��6$   $

 True��5.   $   #���  5/   $   51      �� 53      m� 55      $
 Undo��6   $
��Undo��6.   	 56      �� 57      $��16��6$   $

 True��58   $   #���  59   $   5;      m� 5<      $
 Redo��6   $
��Redo��6.   	 5=      �� 5>      $��16��6$   $

 True��5?   $   #���  5@   $   5B      5C      5E      �� 5F   
    � �� 5G      �� 5H      "   6"    5I      ���� 5J      '        "   6    .�5K      �� " �@ NonPrintableCharacterBrush �	#3F8080FF6K   �� 5L       �@ LinkTextForegroundBrush �	#FF4040FF6H   �� 5N       �@ LinkTextBackgroundBrush
 �����6H   5O      �� 5Q      '        "   6    .�5R      ��  �@�FocusVisualStyle 6A   Z� �� 5S       �@ SelectionBrush �#1ba1e26?   �� 5T       �@ SelectionBorder 5U   "   >� 5V   &    �#1ba1e2$ 1Q�6:   5X      �� 5Y       �@ SelectionCornerRadius$ 0Q�6F   �� 5Z       �@ SelectionForeground# �  6D   �� 5[       � �� 5\   "   �� 5]   &   "  66   .R�5^   *   ]� $|�False��64   .+�5_   .   �� $
��Left=�5`   9   $|�False��5a   9   ��5b   9   ��  �� *TemplatedParent+$ LeftMargins���5c   2   �� 5d   6   .R�5e   :   �� $ 
Horizontal=�6E   5f   7   5h   /   �� 5i   .   $ -1��5j   9   $|�False��5l   9   ��5k   9   ��  �� *TemplatedParent+$ TextView�5l   9   5m   +   5n   '   5p      5q      5r      .R�5s      �� #��z��6   #��z��5t      #��z��5u      ._�5v      �� $��PART_ScrollViewer��5w   %   #��z��5x   %   $
��Left=�5y   %   $	��Top=�5z   %   $
��True��5{   %   $|�False��5}   %   $
��Auto=�5~   %   $
��Auto=�5   %   ��5|   %   ��  �� *TemplatedParent+$ TextArea�5   %   5�       5�      P� 5�       �@ WordWrap$
 True��66   . �5�      �� $ PART_ScrollViewer��6%    � ��$ Disabled=�6m   5�      5�      5�      5�      5�      