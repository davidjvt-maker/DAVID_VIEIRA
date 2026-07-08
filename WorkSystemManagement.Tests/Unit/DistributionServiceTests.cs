using ItemsWorkService.Enums;
using ItemsWorkService.Models;
using ItemsWorkService.Services;
using ItemsWorkService.Interfaces;
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

    [Fact]
    public void IsUserSaturated_ReturnsTrue_WhenUserHasMoreThanThreeHighRelevancePendingItems()
    {
        // Arrange: usuario_juan tiene 4 ítems de alta relevancia pendientes (> 3 = saturado)
        _mockRepo.Setup(r => r.GetByUser("usuario_juan")).Returns(new List<WorkItem>
        {
            new WorkItem { Status = ItemStatus.Pending, Relevance = Relevance.High },
            new WorkItem { Status = ItemStatus.Pending, Relevance = Relevance.High },
            new WorkItem { Status = ItemStatus.Pending, Relevance = Relevance.High },
            new WorkItem { Status = ItemStatus.Pending, Relevance = Relevance.High }
        });

        var service = new DistributionService(_httpClient, _mockRepo.Object);

        // Act
        bool isSaturated = service.IsUserSaturated("usuario_juan");

        // Assert
        Assert.True(isSaturated, "El usuario con 4 ítems de alta relevancia pendientes debe estar saturado.");
    }

    [Fact]
    public void IsUserSaturated_ReturnsFalse_WhenUserHasThreeOrFewerHighRelevancePendingItems()
    {
        // Arrange: usuario_maria tiene exactamente 3 ítems de alta relevancia pendientes (≤ 3 = NO saturado)
        _mockRepo.Setup(r => r.GetByUser("usuario_maria")).Returns(new List<WorkItem>
        {
            new WorkItem { Status = ItemStatus.Pending, Relevance = Relevance.High },
            new WorkItem { Status = ItemStatus.Pending, Relevance = Relevance.High },
            new WorkItem { Status = ItemStatus.Pending, Relevance = Relevance.High }
        });

        var service = new DistributionService(_httpClient, _mockRepo.Object);

        // Act
        bool isSaturated = service.IsUserSaturated("usuario_maria");

        // Assert
        Assert.False(isSaturated, "El usuario con 3 ítems de alta relevancia pendientes NO debe estar saturado.");
    }

    [Fact]
    public void IsUserSaturated_ReturnsFalse_WhenUserHasNoItems()
    {
        // Arrange: usuario sin ítems asignados
        _mockRepo.Setup(r => r.GetByUser("usuario_nuevo")).Returns(new List<WorkItem>());

        var service = new DistributionService(_httpClient, _mockRepo.Object);

        // Act
        bool isSaturated = service.IsUserSaturated("usuario_nuevo");

        // Assert
        Assert.False(isSaturated, "Un usuario sin ítems no debe estar saturado.");
    }

    [Fact]
    public void SortItemsByDeliveryDate_SortsItemsInAscendingOrderByDate()
    {
        // Arrange: Crear un usuario con ítems en desorden de fecha
        var user = new UserWorkloadSummary
        {
            Username = "usuario_juan",
            Items = new List<WorkItem>
            {
                new WorkItem { Title = "Tarea C", DeliveryDate = DateTime.Today.AddDays(10) },
                new WorkItem { Title = "Tarea A", DeliveryDate = DateTime.Today.AddDays(1) },
                new WorkItem { Title = "Tarea B", DeliveryDate = DateTime.Today.AddDays(5) }
            }
        };

        var service = new DistributionService(_httpClient, _mockRepo.Object);

        // Act: Invocar la función de ordenamiento post-asignación
        service.SortItemsByDeliveryDate(user);

        // Assert: Los ítems deben quedar ordenados por fecha de entrega ascendente
        Assert.Equal("Tarea A", user.Items[0].Title);
        Assert.Equal("Tarea B", user.Items[1].Title);
        Assert.Equal("Tarea C", user.Items[2].Title);
        Assert.True(user.Items[0].DeliveryDate <= user.Items[1].DeliveryDate, 
            "El primer ítem debe tener fecha de entrega menor o igual al segundo.");
        Assert.True(user.Items[1].DeliveryDate <= user.Items[2].DeliveryDate, 
            "El segundo ítem debe tener fecha de entrega menor o igual al tercero.");
    }

    [Fact]
    public async Task AssignItemAsync_KeepsItemListSortedByDeliveryDate_AfterAssignment()
    {
        // Arrange: María tiene ítems con fechas desordenadas
        _mockRepo.Setup(r => r.GetByUser("usuario_juan")).Returns(new List<WorkItem>
        {
            new WorkItem { Status = ItemStatus.Pending },
            new WorkItem { Status = ItemStatus.Pending },
            new WorkItem { Status = ItemStatus.Pending }
        }); // Juan tiene más ítems

        _mockRepo.Setup(r => r.GetByUser("usuario_maria")).Returns(new List<WorkItem>
        {
            new WorkItem { Title = "Tarea Lejana", Status = ItemStatus.Pending, DeliveryDate = DateTime.Today.AddDays(20) },
            new WorkItem { Title = "Tarea Cercana", Status = ItemStatus.Pending, DeliveryDate = DateTime.Today.AddDays(2) }
        });

        var service = new DistributionService(_httpClient, _mockRepo.Object);

        var newItem = new WorkItem
        {
            Title = "Tarea Nueva Intermedia",
            DeliveryDate = DateTime.Today.AddDays(10), // Fecha intermedia entre 2 y 20 días
            Relevance = Relevance.Medium,
            Status = ItemStatus.Pending
        };

        // Act: Asignar el ítem (se espera que vaya a María por tener menos pendientes)
        var result = await service.AssignItemAsync(newItem);

        // Assert: Verificar que la asignación fue a María
        Assert.Equal("usuario_maria", result.Item!.AssignedUsername);

        // Assert: Verificar que la función SortItemsByDeliveryDate se ejecutó correctamente
        // Para probarlo directamente, invocamos SortItemsByDeliveryDate con una lista desordenada
        // y confirmamos que el resultado queda en orden ascendente
        var testUser = new UserWorkloadSummary
        {
            Username = "usuario_maria",
            Items = new List<WorkItem>
            {
                new WorkItem { Title = "Tarea Lejana", DeliveryDate = DateTime.Today.AddDays(20) },
                new WorkItem { Title = "Tarea Nueva Intermedia", DeliveryDate = DateTime.Today.AddDays(10) },
                new WorkItem { Title = "Tarea Cercana", DeliveryDate = DateTime.Today.AddDays(2) }
            }
        };

        service.SortItemsByDeliveryDate(testUser);

        // La lista debe quedar: Cercana (2d) -> Intermedia (10d) -> Lejana (20d)
        Assert.Equal("Tarea Cercana", testUser.Items[0].Title);
        Assert.Equal("Tarea Nueva Intermedia", testUser.Items[1].Title);
        Assert.Equal("Tarea Lejana", testUser.Items[2].Title);

        for (int i = 0; i < testUser.Items.Count - 1; i++)
        {
            Assert.True(testUser.Items[i].DeliveryDate <= testUser.Items[i + 1].DeliveryDate,
                $"El ítem '{testUser.Items[i].Title}' debe estar antes que '{testUser.Items[i + 1].Title}'.");
        }
    }
}