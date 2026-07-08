using ItemsWorkService.Models;
using ItemsWorkService.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;

namespace WorkSystemManagement.Tests.Integration;

public class WorkItemsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public WorkItemsControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Distribute_ReturnsOkAndAssignsItem()
    {
        var item = new WorkItem
        {
            Title = "Integration Test Item",
            DeliveryDate = DateTime.Today.AddDays(2),
            Relevance = Relevance.High
        };

        var response = await _client.PostAsJsonAsync("/api/workitems/distribute", item);
        
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyOrList()
    {
        var response = await _client.GetAsync("/api/workitems");
        response.EnsureSuccessStatusCode();
    }
}