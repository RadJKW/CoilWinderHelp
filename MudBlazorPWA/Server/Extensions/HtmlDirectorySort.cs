using System.Text.Encodings.Web;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace MudBlazorPWA.Server.Extensions;
public class HtmlDirectorySort : HtmlDirectoryFormatter
{
    public HtmlDirectorySort(HtmlEncoder encoder) : base(encoder) {}

    public override Task GenerateContentAsync(HttpContext context, IEnumerable<IFileInfo> contents)
    {
        var sorted = contents.OrderBy(f => f.Name);

        // add the relativePath "files" to the url used to generate the links


        return base.GenerateContentAsync(context, sorted);
    }

}
