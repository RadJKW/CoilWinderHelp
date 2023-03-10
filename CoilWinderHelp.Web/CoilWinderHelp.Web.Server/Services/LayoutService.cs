using CoilWinderHelp.Web.Server.Models;
using MudBlazor;

namespace CoilWinderHelp.Web.Server.Services;
public class LayoutService
{
  public bool IsDarkMode { get; set; }

  public MudTheme? CurrentTheme { get; private set; }

  public void SetDarkMode(bool value)
  {
    IsDarkMode = value;
  }

  public event EventHandler MajorUpdateOccured = null!;

  private void OnMajorUpdateOccured() => MajorUpdateOccured.Invoke(this, EventArgs.Empty);

  public void ToggleDarkMode()
  {
    IsDarkMode = !IsDarkMode;
    OnMajorUpdateOccured();
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

      return HelpBasePage.Instructions;

    }
    return HelpBasePage.Index;

  }
}
