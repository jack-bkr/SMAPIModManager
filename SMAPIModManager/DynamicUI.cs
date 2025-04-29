using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace SMAPIModManager;

public static class DynamicUI
{
    public static async void PopulateScrollViewer(ScrollViewer scrollViewer, List<CurseForgeAPI.Mod> mods)
    {
        StackPanel stackPanel = new StackPanel();
        Boolean alternate = false;
        
        scrollViewer.Content = stackPanel;

        foreach (CurseForgeAPI.Mod mod in mods) // Loop through the mods
        {
            if (mod.curseId == "898372")    // Skip showing SMAPI as it has its own install/update button
            {
                continue;
            }
            
            Grid OuterGrid = new Grid() // Create the outer grid, each outer grid is 1 mod
            {
                Name = mod.curseId,
                Margin = new Thickness(),
                MaxHeight = 100
            };
            OuterGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            OuterGrid.ColumnDefinitions.Add(new ColumnDefinition(3, GridUnitType.Star));
            OuterGrid.AddHandler(Grid.PointerPressedEvent, OnModClick);

            Image thumbnail = new Image()
            {
                Source = await mod.GetThumbnail(),
                Margin = new Thickness(2, 2, 0, 2)
            };
            Grid.SetRow(thumbnail, 0);

            Grid InnerGrid = new Grid()
            {
                Margin = new Thickness(0, 2, 2, 2)
            }; // Create the inner grid for styling
            InnerGrid.RowDefinitions.Add(new RowDefinition());
            InnerGrid.RowDefinitions.Add(new RowDefinition());
            InnerGrid.RowDefinitions.Add(new RowDefinition());
            InnerGrid.RowDefinitions.Add(new RowDefinition());
            Grid.SetColumn(InnerGrid, 1);

            if (alternate) // Alternate the background color for readability
            {
                InnerGrid.Background = new SolidColorBrush(Colors.Gray);
                alternate = false;
            }
            else
            {
                InnerGrid.Background = new SolidColorBrush(Colors.DarkGray);
                alternate = true;
            }

            TextBlock Name = new TextBlock()
            {
                Text = mod.name,
                Margin = new Thickness(1)
            };
            Grid.SetRow(Name, 0);

            TextBlock Author = new TextBlock()
            {
                Text = mod.author,
                Margin = new Thickness(1)
            };
            Grid.SetRow(Author, 1);

            TextBlock Description = new TextBlock()
            {
                Text = mod.description,
                Margin = new Thickness(1)
            };
            Grid.SetRow(Description, 2);

            TextBlock Version = new TextBlock()
            {
                Text = mod.version,
                Margin = new Thickness(1)
            };
            Grid.SetRow(Version, 3);

            // Add the elements to the inner grid
            InnerGrid.Children.Add(Name);
            InnerGrid.Children.Add(Author);
            InnerGrid.Children.Add(Description);
            InnerGrid.Children.Add(Version);

            // Add the details and thumbnail to the outer grid row
            OuterGrid.Children.Add(thumbnail);
            OuterGrid.Children.Add(InnerGrid);

            // Add the outer grid to the stack panel
            stackPanel.Children.Add(OuterGrid);
        }
    }

    static void OnModClick(object? sender, RoutedEventArgs e)
    {
        Grid mod = (Grid)sender;
        Grid InnerGrid = (Grid)mod.Children[1];
        Image thumbnail = (Image)mod.Children[0];
        TextBlock name = (TextBlock)InnerGrid.Children[0];
        
        StackPanel stackPanel = (StackPanel)mod.Parent;
        foreach (Grid modinfo in stackPanel.Children) // Loop through the mods
        {
            modinfo.Background = new SolidColorBrush(Colors.Transparent); // Reset the background color
        }
        
        ScrollViewer scrollViewer = (ScrollViewer)stackPanel.Parent;
        Grid windowGrid = (Grid)scrollViewer.Parent;

        Grid modInfoGrid = windowGrid.FindControl<Grid>("modInfo");
        modInfoGrid.RowDefinitions.Clear();
        modInfoGrid.RowDefinitions.Add(new RowDefinition());
        modInfoGrid.RowDefinitions.Add(new RowDefinition());
        modInfoGrid.Children.Clear(); // Clear the mod info grid
        
        Image infoThumbnail = new Image() // Copy image from selected mod
        {
            Source = thumbnail.Source,
            Margin = new Thickness(2),
            Name = mod.Name
        };

        ScrollViewer modInfoSV = new ScrollViewer()
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Visible
        };
        
        TextBlock modInfo = new TextBlock() // Copy Mod Info from selected mod in scroll viewer
        {
            Text = ($"{((TextBlock)InnerGrid.Children[0]).Text} \n" +
                    $"By {((TextBlock)InnerGrid.Children[1]).Text} \n \n" +
                    $"{((TextBlock)InnerGrid.Children[2]).Text} \n \n" +
                    $"Version: {((TextBlock)InnerGrid.Children[3]).Text}"),
            Margin = new Thickness(1),
            TextWrapping = TextWrapping.Wrap
        };
        
        Grid.SetRow(thumbnail, 0);
        Grid.SetRow(modInfoSV, 1);
        
        modInfoSV.Content = modInfo;
        modInfoGrid.Children.Add(infoThumbnail);
        modInfoGrid.Children.Add(modInfoSV);
        
        List<List<String>> result = DBConnector.SendSQL($"select * from Installed where CurseforgeID = \"{mod.Name}\"");

        if (result.Count == 0)
        {
            windowGrid.FindControl<Button>("btnInstall").IsEnabled = true;
            windowGrid.FindControl<Button>("btnDelete").IsEnabled = false;
        }
        else
        {
            windowGrid.FindControl<Button>("btnInstall").IsEnabled = false;
            windowGrid.FindControl<Button>("btnDelete").IsEnabled = true;
        }
        
        mod.Background = new SolidColorBrush(Colors.LimeGreen); // Change the background color of the mod
    }
    //CurseForgeAPI.Mod dbMod = new CurseForgeAPI.Mod(Convert.ToInt32(result[0][0]), result[0][1],  result[0][2],  result[0][3],  result[0][4], result[0][5], result[0][6],  result[0][7],  result[0][8]);
}