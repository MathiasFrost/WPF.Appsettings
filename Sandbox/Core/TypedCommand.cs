using System;
using System.Diagnostics;
using System.Windows.Input;

namespace WPF.Appsettings.Sandbox.Core;

/// <inheritdoc/>
public sealed class RelayCommand<T> : ICommand
{
    /// <summary> </summary>
    private readonly Action<T> _execute;

    /// <summary> </summary>
    public RelayCommand(Action<T> execute) => _execute = execute;

    /// <inheritdoc/>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <inheritdoc/>
    public bool CanExecute(object? parameter)
    {
        try
        {
            return Convert.ChangeType(parameter, typeof(T)) != null;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            throw;
        }
    }

    /// <inheritdoc/>
    public void Execute(object? parameter)
    {
        if (Convert.ChangeType(parameter, typeof(T)) is T o) _execute(o);
    }
}