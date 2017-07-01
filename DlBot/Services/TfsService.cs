using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DlBot.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog.Events;
using System.Text.RegularExpressions;

namespace DlBot.Services
{
    public class TfsService
    {
        private readonly IOptions<SettingsModel> _settings;

        public TfsService(IOptions<SettingsModel> settings)
        {
            _settings = settings;
        }

        public async Task<TfsWorkItemModel> GetWorkItem(int workItemId)
        {
            string url = $"{_settings.Value.TfsUrl}_apis/wit/workItems/{workItemId}?$expand=relations&api-version=1.0";
            NetworkCredential _networkCredential = new NetworkCredential(_settings.Value.TfsUsername, _settings.Value.TfsPassword);

            using (var client = new HttpClient(new HttpClientHandler { Credentials = _networkCredential }))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                var response = client.SendAsync(request).Result;
                var responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var tfsWorkItemModel = JsonConvert.DeserializeObject<TfsWorkItemModel>(responseString);
                    return tfsWorkItemModel;
                }
                else
                {
                    Serilog.Log.Warning($"TFS returned code: {(int)response.StatusCode} {response.StatusCode}");
                    Serilog.Log.Warning(responseString);
                    return null;
                }
            }
        }

        public string GetSlackMessage(List<TfsWorkItemModel> workItems, string inText = null)
        {
            var message = "";
            foreach (var workItem in workItems)
            {
                try
                {
                    var workItemType = workItem.Fields.WorkItemType;
                    if (workItemType == "Product Backlog Item")
                    {
                        workItemType = "PBI";
                    }
                    message += $"<{workItem.Links.Html.Href}|{workItemType} {workItem.Id}: {workItem.Fields.Title.Replace("<", "&lt;").Replace(">", "&gt;")}>\n";
                }
                catch(Exception ex)
                {
                    Serilog.Log.Error(ex, "");
                }
            }

            if (!String.IsNullOrWhiteSpace(inText))
            {
                string note = Regex.Replace(inText, @"^([0-9 ,])*", "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (!String.IsNullOrWhiteSpace(note))
                {
                    message += $"{note}\n";
                }
            }

            return message;
        }
    }
}
