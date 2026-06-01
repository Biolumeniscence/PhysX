using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PhysX;

public partial class ImagePreviewWindow : Window
{
    public ImagePreviewWindow(string imageSource, string title)
    {
        InitializeComponent();

        Title = title;
        PreviewTitle.Text = title;
        PreviewImage.Source = new BitmapImage(new Uri(imageSource, UriKind.Absolute));
    }

    private void ClosePreview(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void CloseOnEscape(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }
}
