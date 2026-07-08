using ItemsWorkService.Models;
using ItemsWorkService.Enums;
using ItemsWorkService.Interfaces;
using System.Net.Http.Json;

namespace ItemsWorkService.Services
{
    /// <summary>
    /// Implementación del servicio de distribución de ítems.
    /// </summary>
    public class DistributionService : IDistributionService
    {
        /// <summary>
        /// Umbral máximo de ítems de alta relevancia pendientes permitidos antes de considerar saturado al usuario.
        /// </summary>
        private const int SaturationThreshold = 3;

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
        /// Determina si un usuario está saturado verificando que la cantidad de ítems 
        /// de alta relevancia pendientes supere el umbral definido (>3).
        /// </summary>
        /// <param name="username">El nombre del usuario a evaluar.</param>
        /// <returns>True si el usuario tiene más de 3 ítems de alta relevancia pendientes.</returns>
        public bool IsUserSaturated(string username)
        {
            var userItems = _repository.GetByUser(username);

            if (userItems == null || !userItems.Any())
            {
                return false;
            }

            int highRelevancePendingCount = userItems
                .Count(i => i.Relevance == Enums.Relevance.High && i.Status == Enums.ItemStatus.Pending);

            return highRelevancePendingCount > SaturationThreshold;
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

            // REGLA DE NEGOCIO: Verificación explícita de saturación por usuario.
            // Se invoca IsUserSaturated() para cada candidato antes de permitir la asignación.
            // Un usuario está saturado si tiene más de 3 ítems de alta relevancia pendientes.
            var availableUsers = candidates
                .Where(u => !IsUserSaturated(u.Username))
                .ToList();

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

                // Añadimos el ítem y ordenamos la lista post-asignación
                chosenUser.Items.Add(item);
                SortItemsByDeliveryDate(chosenUser);
            }

            return new AssignmentResult
            {
                Item = item,
                AssignmentReason = reason,
                IsNearingSaturation = chosenUser != null && IsUserSaturated(chosenUser.Username)
            };
        }

        /// <summary>
        /// Ordena la lista de ítems de un usuario por fecha de entrega (ascendente)
        /// después de cada asignación, garantizando que los ítems más urgentes
        /// queden al inicio de la lista.
        /// </summary>
        /// <param name="user">El resumen de carga del usuario cuyos ítems se van a ordenar.</param>
        public void SortItemsByDeliveryDate(UserWorkloadSummary user)
        {
            if (user == null || user.Items == null || !user.Items.Any())
            {
                return;
            }

            user.Items = user.Items.OrderBy(i => i.DeliveryDate).ToList();
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