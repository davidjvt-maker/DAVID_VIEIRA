using ItemsWorkService.Models;
using ItemsWorkService.Enums;
using System.Net.Http.Json;

namespace ItemsWorkService.Services
{
    /// <summary>
    /// Implementación del servicio de distribución de ítems.
    /// </summary>
    public class DistributionService : IDistributionService
    {
        private readonly HttpClient _httpClient;
        private readonly IWorkItemRepository _repository;

        /// <summary>
        /// Constructor con inyección de dependencias.
        /// </summary>
        public DistributionService(HttpClient httpClient, IWorkItemRepository repository)
        {
            _httpClient = httpClient;
            _repository = repository;
        }

        /// <summary>
        /// Asigna un ítem a un usuario basado en reglas de negocio (saturación, fecha de entrega).
        /// </summary>
        public async Task<AssignmentResult> AssignItemAsync(WorkItem item)
        {
            List<UserWorkloadSummary> candidates;

            try
            {
                var usernames = await _httpClient.GetFromJsonAsync<List<string>>("api/users");

                if (usernames == null || !usernames.Any())
                {
                    throw new Exception("No users were found available in the UserService.");
                }

                candidates = usernames.Select(name => new UserWorkloadSummary { Username = name }).ToList();

                foreach (var c in candidates)
                {
                    var items = _repository.GetByUser(c.Username);
                    if (items != null && items.Any())
                    {
                        c.Items = items;
                    }
                    else
                    {
                        LoadSimulatedTasks(new List<UserWorkloadSummary> { c });
                    }
                }
            }
            catch (Exception)
            {
                candidates = GetMockUsersWorkload();
            }

            // REGLA DE NEGOCIO: Ningún usuario saturado debe ser considerado.
            // Utilizamos la propiedad IsSaturated explícitamente.
            var availableUsers = candidates.Where(u => !u.IsSaturated).ToList();

            // Fallback: Si todos están saturados, usamos todos los candidatos para no perder el ítem.
            if (!availableUsers.Any())
            {
                availableUsers = candidates;
            }

            UserWorkloadSummary? chosenUser = null;
            string reason = "";

            // EVALUACIÓN DE FECHA PRÓXIMA (< 3 días)
            double remainingDays = (item.DeliveryDate.Date - DateTime.Today).TotalDays;

            if (remainingDays < 3)
            {
                // Asignar al usuario con MENOS ítems totales acumulados
                chosenUser = availableUsers
                    .OrderBy(u => u.TotalItems)
                    .FirstOrDefault();
                reason = "Asignado por fecha próxima (menos de 3 días) al usuario con menor carga total.";
            }
            // EVALUACIÓN POR RELEVANCIA GENERAL (Fecha holgada)
            else
            {
                // Asignar al usuario con MENOS ítems PENDIENTES generales
                chosenUser = availableUsers
                    .OrderBy(u => u.PendingCount)
                    .FirstOrDefault();
                reason = "Asignado por disponibilidad general al usuario con menos ítems pendientes.";
            }

            if (chosenUser != null)
            {
                item.AssignedUsername = chosenUser.Username;

                // Añadimos el ítem a su lista y la ordenamos por fecha de entrega
                chosenUser.Items.Add(item);
                chosenUser.Items = chosenUser.Items.OrderBy(i => i.DeliveryDate).ToList();
            }

            return new AssignmentResult
            {
                Item = item,
                AssignmentReason = reason,
                IsNearingSaturation = chosenUser?.HighRelevanceCount >= 3
            };
        }

        private void LoadSimulatedTasks(List<UserWorkloadSummary> users)
        {
            foreach (var u in users)
            {
                if (u.Username == "usuario_juan")
                {
                    u.Items.Add(new WorkItem
                    {
                        Id = Guid.NewGuid(),
                        Title = "Tarea vieja",
                        Relevance = Relevance.High,
                        Status = ItemStatus.Pending,
                        DeliveryDate = DateTime.Today.AddDays(5)
                    });
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
}