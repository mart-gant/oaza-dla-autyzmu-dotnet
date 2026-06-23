using Microsoft.Maui.Controls;

namespace OazaDlaAutyzmu.Maui;

public partial class MainPage : ContentPage
{
    int count = 0;

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnCounterClicked(object sender, System.EventArgs e)
    {
        count++;
        ((Button)sender).Text = $"Kliknięto {count} razy";
    }
}
