   M S B A M L   `   `   `  ����P  LIgorKL.ACAD3.Model, Version=1.1.0.5362, Culture=neutral, PublicKeyToken=nullY1clr-namespace:IgorKL.ACAD3.Model.CogoPoints.Views#IgorKL.ACAD3.Model.CogoPoints.Views  5      D    >IgorKL.ACAD3.Model.CogoPoints.Views.PointStylesSelectorControl   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Clocal1clr-namespace:IgorKL.ACAD3.Model.CogoPoints.Views     5      -   5          >�Name$  userControl��5      .��5      � 
�������.+�5   
   �� $+��%Стиль маркера точки:��6   $��
10,10,10,0q�61   $	��Top=�6E   $��74��6]   $��Italic$�6i   $
��Bold"�6|    J� Effect 5      3  -System.Windows.Media.Effects.DropShadowEffect  5      ��5      �� 5       �� EndPoint$ 0.5,1&�6&    � MappingMode$ RelativeToBoundingBox=�67    �� 
StartPoint$ 0.5,0&�6[   .��5      �  � Color$ 	#FFE1E8F5��6#   � 5      $ White��6#    � Offset$ 1Q�61   5      .��5      �� -   5         ��Name$ _comboBox_PointStyles��5      $��
10,10,-2,0q�66   $	��Top=�6J   $��25��6b    �� SelectedValuePath$	 Key��5      	 �� DisplayMemberPath$	 Value��5      
 �� SelectedIndex$
 0��5       � DataContext 5      ��  �� ElementName$ userControl����5      ��  �� Path$ CogoPointStyles�5      5      �� 5    
   $'��!Стиль метки точки:��6   $��
10,89,10,0q�6/   $	��Top=�6C   $��74��6[   $
��Bold"�6g   $��Italic$�6y    5!        5"      ��5$      �� 5%      $ 0.5,1&�6&   $ RelativeToBoundingBox=�67   $ 0.5,0&�6[   .��5&      � $ 	#FFE1E8F5��6#   � 5'      $ White��6#   $ 1Q�61   5(      .��5*      �� -   5        $  _comboBox_PointLabelStyles��5*      $��
10,10,-2,0q�6;   $	��Top=�6O   $��25��6g   $	 Key��5-      $	 Value��5.      $
 0��5/       5+      �� $ userControl����5,      �� $ CogoPointLabelStyles�5/      51      �� 52   
   $��Отмена��6   $��
0,172,10,0q�6"   $	��Top=�66   $��Right=�6N   $��75��6j   {�.  6u   �� 53   
   -   5        $!��Вставить точки��53      $��
0,172,90,0q�6*   $	��Top=�6>   $��Right=�6V   $	��103��6r   6~   55      56      