   M S B A M L   `   `   `  ����W  SSynchroniserPresentations, Version=2019.1.0.0, Culture=neutral, PublicKeyToken=null[2clr-namespace:SynchroniserPresentations.ViewModels$SynchroniserPresentations.ViewModels  5      "    SynchroniserPresentations.Ui   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Avm2clr-namespace:SynchroniserPresentations.ViewModels     5      -   5        $��DebugUtility��5
        � Title$!  Synchroniser présentations��5      $	��340��6(   $	��460��65    � WindowStartupLocation$ CenterScreen=�5      
�������$��12(�5	       � Icon$ Resources/revit-wire.ico��5
      $	��340��5      $	��460��6   .��5      � 
������� � RowDefinitions 5   
   �� 5      $	��100 �6   �� 5      $	��100 �6   �� 5      $	��120 �6    � ColumnDefinitions 5   
   �� 5      $��* �6   �� 5      $	��400 �6   �� 5      $��* �6   .+�5   
   �� -   5         ��Name$ btnselectionvue��5      $��1��6*   
�������$$��Choisir une vue de référence��6N   $��10q�6u   $��60��6�   $��Center=�5   )   $
��Left=�6D   $	��250��6_   �� 5   
   -   5        $ btndeselection��5      $��1��6)   
�������$��Réinitialiser��6M   $��10q�6e   $��60��6q   $��Center=�5   +   $��Right=�6F   $	��100��6b   �� 5!   
   $��1��6   $��1��6!   $��10,10,10,10q�61   $	��Top=�6F   $
��Left=�6^   $	��160��6y   $%��Nom de la vue de référence : ��6�   6�   �� 5"   
   -   5        $��	txtnomvue��5"      $��1��6%   $��1��62   $��175,10,10,10q�6B   $	��Top=�6X   6r   �� 5$   
   $��1��6   $��1��6!   $��10,35,10,10q�61   $	��Top=�6F   $
��Left=�6^   $	��120��6y   $��Nom de la feuille : ��6�   6�   �� 5%   
   -   5        $��txtnomfeuille��5%      $��1��6)   $��1��66   $��175,35,10,10q�6F   $	��Top=�6\   6v   �� 5(   
   -   5        $��	recCanvas��5(      $��1��6%   $��1��62   �� 5.   
   -   5        $��btnsynchrovues��5.      $��2��6(   $��1��65   
�������$��
10,0,10,20q�6Y   $��Center=�5/   -   $��60��6H   .��50      �� .+�51      ��  �� TextAlignment$ Center=�6   Choisir des vues à replacer6P   �� 52      $ Center=�6   $	��340��63   $
��Wrap=�6?   �� 6U   .�6\   �~Les vues seront alignées pour avoir le centre placé de la même manière par rapport au coin supérieur gauche de la feuille6�   6�   53      54      57      58      