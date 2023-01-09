using CoilWinderHelp.RCL.Models;
using CoilWinderHelp.RCL.Shared;
using MudBlazor;

namespace CoilWinderHelp.RCL.Services;
public class LayoutService
{
    public bool IsDarkMode { get; private set; } = true;

    public MudTheme CurrentTheme { get; private set; } = null!;

    public void SetDarkMode(bool value)
    {
        IsDarkMode = value;

    }

    public void ApplyUserPreferences(bool isDarkModeDefaultTheme)
    {
        IsDarkMode = true;
    }

    public event EventHandler? MajorUpdateOccured;

    private void OnMajorUpdateOccured() => MajorUpdateOccured?.Invoke(this, EventArgs.Empty);

    public Task ToggleDarkMode()
    {
        IsDarkMode = !IsDarkMode;
        OnMajorUpdateOccured();
        return Task.CompletedTask;
    }



    public void SetBaseTheme(MudTheme theme)
    {
        CurrentTheme = theme;
        OnMajorUpdateOccured();
    }

    public HelpBasePage GetBaseLayoutPage(string uri)
    {
        if (uri.Contains("/admin"))
        {
            return HelpBasePage.Admin;
        }
        else if (uri.Contains("/instructions)"))
        {
            
            return  HelpBasePage.Instructions;
            
        }
        return HelpBasePage.Index;

    }
}