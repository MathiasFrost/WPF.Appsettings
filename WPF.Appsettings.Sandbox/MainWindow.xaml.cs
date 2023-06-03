using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WpfApp1.Core;

namespace WPF.Appsettings.Sandbox;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        IncrementCounterCommand = new RelayCommand<int>(IncrementCounter);
    }
    /// <summary> </summary>
    public ICommand IncrementCounterCommand { get; }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("Click");
        Console.WriteLine("Click");
        TextBlock.Text += TextBlock.Text;
    }

    private string _text = "haha";
    /// <summary> </summary>
    private void IncrementCounter(int parameter)
    {
        Debug.WriteLine(parameter.GetType());
        Text += parameter.ToString();
    }

    public string Text
    {
        get => _text;
        set => SetField(ref _text, value);
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}