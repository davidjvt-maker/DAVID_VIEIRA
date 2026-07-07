using ItemsWorkService.Enums;
using ItemsWorkService.Models;

namespace ItemsWorkService.Services;

public class DistributionService : IDistributionService
{
    private readonly HttpClient _httpClient;

    // Inyectamos el HttpClient a través del constructor
    public DistributionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WorkItem> AssignItemAsync(WorkItem item)
    {
        List<UserWorkloadSummary> candidates;

        try
        {
            var usernames = await _httpClient.GetFromJsonAsync<List<string>>("https://localhost:5001/api/users");

            if (usernames == null || !usernames.Any())
            {
                throw new Exception("No users were found available in the UserService.");
            }

            // Transformamos los nombres de usuario en nuestro modelo de resumen simulando sus tareas actuales
            candidates = usernames.Select(name => new UserWorkloadSummary { Username = name }).ToList();

            LoadSimulatedTasks(candidates);
        }
        catch (Exception)
        {
            // Si el microservicio de usuarios está apagado, no rompemos la app
            candidates = GetMockUsersWorkload();
        }

        // Excluimos usuarios que tengan más de 3 ítems pendientes de ALTA relevancia
        var availableUsers = candidates.Where(u => u.HighRelevanceCount <= 3).ToList();

        // Si todos están saturados, por seguridad usamos la lista completa para no dejar el ítem en el aire
        if (!availableUsers.Any())
        {
            availableUsers = candidates;
        }

        UserWorkloadSummary? chosenUser = null;

        // EVALUACIÓN DE FECHA PRÓXIMA (< 3 días)
        double diasRestantes = (item.DeliveryDate.Date - DateTime.Today).TotalDays;

        if (diasRestantes < 3)
        {
            // Asignar al usuario con MENOS ítems totales acumulados
            chosenUser = availableUsers
                .OrderBy(u => u.TotalItems)
                .FirstOrDefault();
        }
        //EVALUACIÓN POR RELEVANCIA GENERAL (Fecha holgada)
        else
        {
            // Asignar al usuario con MENOS ítems PENDIENTES generales
            chosenUser = availableUsers
                .OrderBy(u => u.PendingCount)
                .FirstOrDefault();
        }

        //ASIGNACIÓN Y ORDENAMIENTO POST-ASIGNACIÓN
        if (chosenUser != null)
        {
            item.AssignedUsername = chosenUser.Username;

            // Añadimos el ítem a su lista y la ordenamos por fecha de entrega de forma ascendente
            chosenUser.Items.Add(item);
            chosenUser.Items = chosenUser.Items.OrderBy(i => i.DeliveryDate).ToList();
        }

        return item;
    }

    private void LoadSimulatedTasks(List<UserWorkloadSummary> users)
    {
        foreach (var u in users)
        {
            if (u.Username == "usuario_juan")
            {
                u.Items.Add(new WorkItem { Id = Guid.NewGuid(), Title = "Tarea vieja", Relevance = Relevance.High, Status = ItemStatus.Pending });
            }
        }
    }

    private List<UserWorkloadSummary> GetMockUsersWorkload()
    {
        return new List<UserWorkloadSummary>
        {
            new UserWorkloadSummary { Username = "usuario_juan" },
            new UserWorkloadSummary { Username = "usuario_maria" }
        };
    }
}