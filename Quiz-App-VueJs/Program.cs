using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Quiz_App_VueJs.Data;
using Quiz_App_VueJs.Models;
using Scalar.AspNetCore;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IRepository, Repository>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(o =>
    {
        o.RouteTemplate = "/openapi/{documentName}.json";
    });
    //app.UseSwaggerUI();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

#region v1
var apiV1 = app.MapGroup("/api/v1");
apiV1.MapGet("/getTopics", () =>
{
    string filePath = @"D:\DotNet\Learning Project Net\Quiz-App-VueJs\database\topics.json";

    using FileStream fs = File.OpenRead(filePath);
    using StreamReader reader = new StreamReader(fs);
    string json = reader.ReadToEnd();

    byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

    var readerOptions = new JsonReaderOptions
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip,
    };

    var jsonReader = new Utf8JsonReader(jsonBytes, readerOptions);

    ICollection<Topics> allTopics = new Collection<Topics>();
    Topics topics = new Topics();

    while (jsonReader.Read())
    {
        if (jsonReader.TokenType == JsonTokenType.StartObject)
        {
            topics = new Topics()
            {
                Id = -1,
                Title = null,
                Category = null
            };
        }

        if (jsonReader.TokenType == JsonTokenType.PropertyName)
        {
            string propertyName = jsonReader.GetString();
            jsonReader.Read();

            if (propertyName == "id")
                topics.Id = jsonReader.GetInt32();
            else if (propertyName == "title")
                topics.Title = jsonReader.GetString();
            else if (propertyName == "category")
                topics.Category = jsonReader.GetString();
        }

        if (jsonReader.TokenType == JsonTokenType.EndObject)
        {
            allTopics.Add(topics);
        }
    }

    return allTopics;
});

apiV1.MapGet("/getSubTopics", (int id) =>
{
    if (id <= 0)
        return Results.NotFound("Invalid Topic ID.");

    string filePath = @"D:\DotNet\Learning Project Net\Quiz-App-VueJs\database\subtopics.json";
    if (!File.Exists(filePath))
        return Results.NotFound("Data file not found.");

    using FileStream fs = File.OpenRead(filePath);
    using StreamReader reader = new StreamReader(fs);
    string json = reader.ReadToEnd();
    byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

    var readerOptions = new JsonReaderOptions
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    var jsonReader = new Utf8JsonReader(jsonBytes, readerOptions);

    var filteredSubTopics = new List<SubTopics>();
    SubTopics? current = null;
    string? propertyName = null;

    while (jsonReader.Read())
    {
        switch (jsonReader.TokenType)
        {
            case JsonTokenType.StartObject:
                current = new SubTopics();
                break;

            case JsonTokenType.PropertyName:
                propertyName = jsonReader.GetString();
                break;

            case JsonTokenType.String:
                if (current != null && propertyName == "id")
                    current.Id = jsonReader.GetString();
                else if (current != null && propertyName == "title")
                    current.Title = jsonReader.GetString();
                break;

            case JsonTokenType.Number:
                if (current != null && propertyName == "topicId")
                    current.TopicId = jsonReader.GetInt32();
                break;

            case JsonTokenType.EndObject:
                if (current != null && current.TopicId == id)
                    filteredSubTopics.Add(current);
                break;
        }
    }

    if (filteredSubTopics.Count == 0)
        return Results.NotFound($"No subtopics found for TopicId {id}.");

    return Results.Ok(filteredSubTopics);
});

apiV1.MapGet("/getQuestions", async (string? subTopicId) =>
{
    if (string.IsNullOrEmpty(subTopicId))
        return Results.BadRequest("SubTopic ID is required.");

    string filePath = @"D:\DotNet\Learning Project Net\Quiz-App-VueJs\database\questions.json";
    if (!File.Exists(filePath))
        return Results.NotFound("Questions data file not found.");

    var filteredQuestions = new List<Questions>();

    await using FileStream fs = File.OpenRead(filePath);
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    await foreach (var question in JsonSerializer.DeserializeAsyncEnumerable<Questions>(fs, options))
    {
        if (question is { SubTopicId: not null } && question.SubTopicId.Equals(subTopicId, StringComparison.OrdinalIgnoreCase))
        {
            filteredQuestions.Add(question);
        }
    }

    return filteredQuestions.Count == 0
        ? Results.NotFound($"No questions found for subTopicId '{subTopicId}'.")
        : Results.Ok(filteredQuestions);
});
#endregion

#region v2
var apiV2 = app.MapGroup("/api/v2");
apiV2.MapGet("/getTopics", async () =>
{
    string filePath = @"D:\DotNet\Learning Project Net\Quiz-App-VueJs\database\topics.json";
    var repository = new Repository();
    var topics = await repository.GetCollectionAsync<Topics>(filePath);
    return Results.Ok(topics);
});

apiV2.MapGet("/getSubTopics", async (int id) =>
{
    if (id <= 0)
        return Results.BadRequest("Invalid Topic ID.");

    string filePath = @"D:\DotNet\Learning Project Net\Quiz-App-VueJs\database\subtopics.json";
    var repository = new Repository();

    var subtopics = await repository.GetCollectionAsync<SubTopics>(
        filePath,
        s => s.TopicId == id
    );

    return subtopics.Count == 0
        ? Results.NotFound($"No subtopics found for TopicId {id}.")
        : Results.Ok(subtopics);
});

apiV2.MapGet("/getQuestions", async (string? subTopicId) =>
{
    if (string.IsNullOrEmpty(subTopicId))
        return Results.BadRequest("SubTopic ID is required.");

    string filePath = @"D:\DotNet\Learning Project Net\Quiz-App-VueJs\database\questions.json";
    var repository = new Repository();

    var questions = await repository.GetCollectionAsync<Questions>(
        filePath,
        q => q.SubTopicId?.Equals(subTopicId, StringComparison.OrdinalIgnoreCase) == true
    );

    return questions.Count == 0
        ? Results.NotFound($"No questions found for subTopicId '{subTopicId}'.")
        : Results.Ok(questions);
});

apiV2.MapPut("/updateTopics", async ([FromBody] Topics topic) =>
{
    string filePath = @"D:\DotNet\Learning Project Net\Quiz-App-VueJs\database\topics.json";

    Action<Topics> updateAction = c =>
    {
        if (!string.IsNullOrEmpty(topic.Title))
            c.Title = topic.Title;

        if (!string.IsNullOrEmpty(topic.Category))
            c.Category = topic.Category;
    };

    var repository = new Repository();
    return await repository.UpdateCollectionAsync<Topics>(filePath,
                                                   filter: c => c.Id == topic.Id,
                                                   updateAction: updateAction);
});
#endregion

app.Run();