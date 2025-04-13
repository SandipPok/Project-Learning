using Microsoft.AspNetCore.Http.HttpResults;
using Quiz_App_VueJs.Models;
using Scalar.AspNetCore;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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

var apiV1 = app.MapGroup("/api/v1");
apiV1.MapGet("/getTopics", () =>
{
    string filePath = @"D:\DotNet\Learning Project Net\Quiz-App-VueJs\database\topics.json";

    //if (requestFile == RequestFileName.topics.ToString())
    //{
    //    filePath = @"D:\DotNet\Learning Project Net\Quiz-App-VueJs\database\topics.json";
    //}
    //else if (requestFile == RequestFileName.subtopics.ToString())
    //{
    //    filePath = @"D:\DotNet\Learning Project Net\Quiz-App-VueJs\database\subtopics.json";
    //}
    //else if (requestFile == RequestFileName.questions.ToString())
    //{
    //    filePath = @"D:\DotNet\Learning Project Net\Quiz-App-VueJs\database\questions.json";
    //}

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

app.Run();