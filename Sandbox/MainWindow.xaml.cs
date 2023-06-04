using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WPF.Appsettings.Sandbox.Core;

namespace WPF.Appsettings.Sandbox;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
internal sealed partial class MainWindow : INotifyPropertyChanged
{
    private string _text = "haha";

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        IncrementCounterCommand = new RelayCommand<int>(IncrementCounter);
    }

    /// <summary> </summary>
    public ICommand IncrementCounterCommand { get; }

    public string Text
    {
        get => _text;
        private set => SetField(ref _text, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("Click");
        Console.WriteLine("Click");
        TextBlock.Text += TextBlock.Text;
    }

    /// <summary> </summary>
    private void IncrementCounter(int parameter)
    {
        Debug.WriteLine(parameter.GetType());
        Text += parameter.ToString();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(propertyName);
    }
}