using ItemsWorkService.Enums;
using ItemsWorkService.Models;
using ItemsWorkService.Services;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace WorkSystemManagement.Tests.Unit;

public class DistributionServiceTests
{
    private readonly Mock<IWorkItemRepository> _mockRepo;
    private readonly HttpClient _httpClient;

    public DistributionServiceTests()
    {
        _mockRepo = new Mock<IWorkItemRepository>();

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new List<string> { "usuario_juan", "usuario_maria" })
            });

        _httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost") };
    }

    [Fact]
    public async Task AssignItemAsync_AssignsToUserWithLessItems_WhenDeliveryIsNear()
    {
        _mockRepo.Setup(r => r.GetByUser("usuario_juan")).Returns(new List<WorkItem> 
        { 
            new WorkItem { Status = ItemStatus.Pending },
            new WorkItem { Status = ItemStatus.Pending }
        }); // Juan = 2 items
        
        _mockRepo.Setup(r => r.GetByUser("usuario_maria")).Returns(new List<WorkItem> 
        { 
            new WorkItem { Status = ItemStatus.Pending }
        }); // Maria = 1 item

        var service = new DistributionService(_httpClient, _mockRepo.Object);

        var item = new WorkItem
        {
            DeliveryDate = DateTime.Today.AddDays(1) // Near date
        };

        var result = await service.AssignItemAsync(item);

        Assert.Equal("usuario_maria", result.Item.AssignedUsername);
        Assert.Contains("fecha próxima", result.AssignmentReason);
    }

    [Fact]
    public async Task AssignItemAsync_ExcludesSaturatedUsers()
    {
        _mockRepo.Setup(r => r.GetByUser("usuario_juan")).Returns(new List<WorkItem> 
        { 
            new WorkItem { Status = ItemStatus.Pending, Relevance = Relevance.High },
            new WorkItem { Status = ItemStatus.Pending, Relevance = Relevance.High },
            new WorkItem { Status = ItemStatus.Pending, Relevance = Relevance.High },
            new WorkItem { Status = ItemStatus.Pending, Relevance = Relevance.High } // 4 high relevance = saturated
        });
        
        _mockRepo.Setup(r => r.GetByUser("usuario_maria")).Returns(new List<WorkItem> 
        { 
            new WorkItem { Status = ItemStatus.Pending },
            new WorkItem { Status = ItemStatus.Pending },
            new WorkItem { Status = ItemStatus.Pending } 
        }); 
        // Maria no está saturada pero tiene más items

        var service = new DistributionService(_httpClient, _mockRepo.Object);

        var item = new WorkItem
        {
            DeliveryDate = DateTime.Today.AddDays(5) // Fecha lejana
        };

        var result = await service.AssignItemAsync(item);

        Assert.Equal("usuario_maria", result.Item.AssignedUsername);
    }
}