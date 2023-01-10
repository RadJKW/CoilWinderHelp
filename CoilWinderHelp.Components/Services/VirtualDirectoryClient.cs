using System.Net.Http;
namespace CoilWinderHelp.Components.Services;
public class VirtualDirectoryClient
{

  private readonly HttpClient _http;

  public VirtualDirectoryClient(HttpClient http)
  {
    _http = http;
    http.DefaultRequestHeaders.Add("Accept", "*/*");
  }

}