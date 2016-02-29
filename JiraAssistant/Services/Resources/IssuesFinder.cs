﻿using JiraAssistant.Model.Exceptions;
using JiraAssistant.Model.Jira;
using JiraAssistant.Services.Settings;
using JiraAssistant.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace JiraAssistant.Services.Resources
{
   public class IssuesFinder : BaseRestService
   {
      private const int BATCH_SIZE = 1000;
      private IDictionary<string, RawFieldDefinition> _fields;
      private readonly MetadataRetriever _metadata;

      public IssuesFinder(AssistantSettings configuration,
         MetadataRetriever metadata)
         : base(configuration)
      {
         _metadata = metadata;
      }

      private async Task<RawSearchResults> DownloadSearchResultsBatch(string jqlQuery, int startAt)
      {
         var client = BuildRestClient();
         var request = new RestRequest("/rest/api/latest/search", Method.POST);
         request.AddJsonBody(new
         {
            jql = jqlQuery,
            startAt = startAt,
            maxResults = BATCH_SIZE,
            fields = new string[] { "*all" }
         });
         var response = await client.ExecuteTaskAsync(request);

         if (response.StatusCode != HttpStatusCode.OK)
         {
            throw new SearchFailedException(string.Format("Search request failed with invalid response code: {0}.\r\nResponse content is: {1}", response.StatusCode, response.Content));
         }

         var batch = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<RawSearchResults>(response.Content));

         return batch;
      }

      public async Task<IEnumerable<JiraIssue>> Search(string jqlQuery)
      {
         var searchResults = new List<RawIssue>();

         var firstBatch = await DownloadSearchResultsBatch(jqlQuery, 0);
         var batches = new List<RawSearchResults> { firstBatch };
         if (firstBatch.Total > BATCH_SIZE)
         {
            var pendingBatchesCount = (firstBatch.Total - BATCH_SIZE) / BATCH_SIZE + 1;
            var tasks = new Task<RawSearchResults>[pendingBatchesCount];

            for (int i = 1; i <= pendingBatchesCount; i++)
               tasks[i - 1] = DownloadSearchResultsBatch(jqlQuery, i * BATCH_SIZE);

            await Task.Factory.StartNew(() => Task.WaitAll(tasks));

            batches.AddRange(tasks.Select(t => t.Result));
         }

         if (_fields == null)
         {
            _fields = (await _metadata.GetFieldsDefinitions()).ToDictionary(d => d.Name, d => d);
         }

         return ConvertIssuesToDomainModel(batches.SelectMany(b => b.Issues));
      }

      private ICollection<JiraIssue> ConvertIssuesToDomainModel(IEnumerable<RawIssue> issues)
      {
         return issues.Select(Convert).ToList();
      }

      private JiraIssue Convert(RawIssue issue)
      {
         return new JiraIssue
         {
            Key = issue.Key,
            Project = issue.BuiltInFields.Project.Name,
            Summary = issue.BuiltInFields.Summary,
            Priority = issue.BuiltInFields.Priority.Name,
            StoryPoints = GetFieldByName<float?>(issue, "Story Points") ?? 0,
            Subtasks = issue.BuiltInFields.Subtasks.Count(),
            Created = issue.BuiltInFields.Created,
            Resolved = issue.BuiltInFields.ResolutionDate,
            Status = issue.BuiltInFields.Status.Name,
            Description = issue.BuiltInFields.Description,
            Assignee = (issue.BuiltInFields.Assignee ?? RawUserInfo.EmptyInfo).DisplayName,
            Reporter = (issue.BuiltInFields.Reporter ?? RawUserInfo.EmptyInfo).DisplayName,
            BuiltInFields = issue.BuiltInFields,
            EpicLink = GetFieldByName<string>(issue, "Epic Link") ?? "",
            SprintIds = (GetArrayByName<string>(issue, "Sprint"))
                        .Select(i => int.Parse(i.Substring(i.IndexOf('=') + 1, i.IndexOf(',') - i.IndexOf('=') - 1)))
         };
      }

      private IEnumerable<T> GetArrayByName<T>(RawIssue issue, string fieldName)
      {
         if (_fields.ContainsKey(fieldName) == false)
            return Enumerable.Empty<T>();

         var fieldId = _fields[fieldName].Id;
         return issue.RawFields[fieldId].Select(i => i.Value<T>());
      }

      private T GetFieldByName<T>(RawIssue issue, string fieldName, string path = null)
      {
         if (_fields.ContainsKey(fieldName) == false)
            return default(T);

         var fieldId = _fields[fieldName].Id;
         JToken token;
         if (path == null)
            return issue.RawFields.Value<T>(fieldId);
         else
         {
            token = issue.RawFields.SelectToken(fieldId);
            foreach (var part in path.Split('/'))
            {
               token = token.SelectToken(part);
            }
         }

         if (token == null)
            return default(T);

         return token.ToObject<T>();
      }
   }
}
