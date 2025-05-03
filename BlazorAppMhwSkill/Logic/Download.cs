using BlazorAppMhwSkill.Models;
using Microsoft.JSInterop;
using System.Text;

namespace BlazorAppMhwSkill.Logic
{
    public class Download
    {
        public async Task Execute<T>(T[] armors, IJSRuntime JS) where T : DownloadModelBase
        {
            await Execute<T>(armors, string.Empty, JS);
        }
        public async Task Execute<T>(T[] armors, string preHeader, IJSRuntime JS) where T : DownloadModelBase
        {
            var csv = new StringBuilder();
            if (!string.IsNullOrEmpty(preHeader))
            {
                csv.AppendLine(preHeader);
            }
            csv.AppendLine(T.GetHeader());

            if (armors != null)
            {
                foreach (var armor in armors)
                {
                    csv.AppendLine(armor.GetRow());
                }
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            var base64 = Convert.ToBase64String(bytes);
            var dataUrl = $"data:text/csv;base64,{base64}";

            await JS.InvokeVoidAsync("triggerFileDownload", "armor.tsv", dataUrl);
        }
    }
}
