using Microsoft.AspNetCore.SignalR;
using Zezame.Api.Exceptions;
using Zezame.Api.Hubs;
using Zezame.Api.Services.Interfaces;
using Zezame.Domain.Entities;
using Zezame.Domain.Interfaces;
using static Zezame.Api.Models.TimeEntryDtos;

namespace Zezame.Api.Services {
    public class TimeService {
        private readonly ITimeEntryRepository _repo;
        private readonly IHubContext<TimeHub> _hub;
        private readonly IAuthorizationService _auth;
        private readonly IActivityRepository _activities;
        private readonly IUserRepository _users;

        public TimeService(
            ITimeEntryRepository repo,
            IHubContext<TimeHub> hub,
            IAuthorizationService auth,
            IActivityRepository activities,
            IUserRepository users)
            => (_repo, _hub, _auth, _activities, _users) = (repo, hub, auth, activities, users);

        // La organización (tenant) de cada entrada se deriva del usuario que ficha.
        private async Task<Guid> ResolveOrganizationIdAsync(Guid userId) {
            var user = await _users.GetByIdAsync(userId)
                ?? throw new UnauthorizedAccessException("Usuario no encontrado");
            return user.OrganizationId
                ?? throw new InvalidOperationException("El usuario no pertenece a ninguna organización");
        }

        public async Task<EntryVm> StartAsync(Guid userId, StartEntryDto dto) {
            if (await _repo.GetRunningAsync(userId) != null)
                throw new InvalidOperationException("Ya hay una entrada activa");

            // Solo se puede imputar tiempo a proyectos/actividades accesibles por el
            // usuario; impide asociar la entrada a recursos ajenos (IDOR).
            if (dto.ProjectId.HasValue)
                await _auth.EnsureCanAccessProjectAsync(userId, dto.ProjectId.Value);

            if (dto.ActivityId.HasValue) {
                var activity = await _activities.GetByIdAsync(dto.ActivityId.Value)
                    ?? throw new NotFoundException("Actividad no encontrada");
                await _auth.EnsureCanAccessProjectAsync(userId, activity.ProjectId);
                if (dto.ProjectId.HasValue && dto.ProjectId.Value != activity.ProjectId)
                    throw new InvalidOperationException("La actividad no pertenece al proyecto indicado.");
            }

            var now = DateTime.UtcNow;
            var entry = new TimeEntry {
                UserId = userId,
                OrganizationId = await ResolveOrganizationIdAsync(userId),
                StartTime = now,
                Description = dto.Description,
                Type = dto.Type,
                ProjectId = dto.ProjectId,
                ActivityId = dto.ActivityId,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _repo.AddAsync(entry);
            // Notifica a los clientes conectados
            await _hub.Clients.User(userId.ToString())
                      .SendAsync("TimerStarted", entry.Id, entry.StartTime);

            return ToVm(entry);
        }

        // Crea una entrada de tiempo manual (con inicio y fin explícitos).
        public async Task<EntryVm> CreateManualAsync(Guid userId, CreateManualEntryDto dto) {
            var start = DateTime.SpecifyKind(dto.StartTime, DateTimeKind.Utc);
            var end = DateTime.SpecifyKind(dto.EndTime, DateTimeKind.Utc);
            if (end <= start)
                throw new InvalidOperationException("La hora de fin debe ser posterior al inicio");

            var now = DateTime.UtcNow;
            var entry = new TimeEntry {
                UserId = userId,
                OrganizationId = await ResolveOrganizationIdAsync(userId),
                StartTime = start,
                EndTime = end,
                DurationMin = (int)(end - start).TotalMinutes,
                Description = dto.Description,
                Type = EntryType.Work,
                ProjectId = dto.ProjectId,
                ActivityId = dto.ActivityId,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _repo.AddAsync(entry);
            return ToVm(entry);
        }
        public async Task<EntryVm> StopAsync(Guid userId, StopEntryDto dto) {
            // Cierra la entrada activa sea del tipo que sea (trabajo o pausa),
            // para poder detener el cronómetro también estando en pausa.
            var entry = await _repo.GetActiveByUserAsync(userId);
            if (entry == null || entry.Id != dto.Id)
                throw new InvalidOperationException("No hay entrada activa");

            entry.EndTime = DateTime.UtcNow;
            entry.DurationMin = (int)(entry.EndTime.Value - entry.StartTime).TotalMinutes;
            entry.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(entry);

            await _hub.Clients.User(userId.ToString())
                      .SendAsync("TimerStopped", entry.Id, entry.DurationMin);

            return ToVm(entry);
        }

        public async Task<EntryVm> PauseAsync(Guid userId) {
            // 1. Cerrar entrada de trabajo activa
            var workEntry = await _repo.GetRunningAsync(userId, EntryType.Work) ?? throw new Exception("No hay trabajo activo para pausar");
            workEntry.EndTime = DateTime.UtcNow;
            workEntry.DurationMin = (int)(workEntry.EndTime.Value - workEntry.StartTime).TotalMinutes;
            await _repo.UpdateAsync(workEntry);

            // 2. Iniciar automáticamente entrada de pausa
            var breakEntry = new TimeEntry {
                UserId = userId,
                OrganizationId = workEntry.OrganizationId,
                StartTime = DateTime.UtcNow,
                Type = EntryType.Break,
                Description = "Pausa automática",
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(breakEntry);

            await _hub.Clients.User(userId.ToString()).SendAsync("TimerPaused", ToVm(workEntry), ToVm(breakEntry));
            return ToVm(breakEntry);
        }

        public async Task<EntryVm> ResumeAsync(Guid userId) {
            // 1. Cerrar pausa activa
            var breakEntry = await _repo.GetRunningAsync(userId, EntryType.Break);
            if (breakEntry == null) throw new Exception("No hay pausa activa");

            breakEntry.EndTime = DateTime.UtcNow;
            breakEntry.DurationMin = (int)(breakEntry.EndTime.Value - breakEntry.StartTime).TotalMinutes;
            await _repo.UpdateAsync(breakEntry);

            // 2. Iniciar automáticamente trabajo
            var workEntry = new TimeEntry {
                UserId = userId,
                OrganizationId = breakEntry.OrganizationId,
                StartTime = DateTime.UtcNow,
                Type = EntryType.Work,
                Description = "Trabajo reanudado",
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(workEntry);

            await _hub.Clients.User(userId.ToString()).SendAsync("TimerResumed", ToVm(breakEntry), ToVm(workEntry));
            return ToVm(workEntry);
        }

        public async Task<IReadOnlyList<EntryVm>> ListAsync(Guid userId, DateTime from, DateTime to)
            => (await _repo.ListAsync(userId, from, to)).Select(ToVm).ToList();

        private static EntryVm ToVm(TimeEntry e)
            => new(e.Id, e.StartTime, e.EndTime, e.DurationMin, e.Description, e.Type);
    }
}
