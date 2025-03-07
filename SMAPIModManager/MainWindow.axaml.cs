using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SMAPIModManager;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public void onButtonPress(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("Button pressed!"); 
    }
}