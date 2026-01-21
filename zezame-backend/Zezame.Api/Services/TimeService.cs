using Microsoft.AspNetCore.SignalR;
using Zezame.Api.Hubs;
using Zezame.Domain.Entities;
using Zezame.Domain.Interfaces;
using static Zezame.Api.Models.TimeEntryDtos;

namespace Zezame.Api.Services {
    public class TimeService {
        private readonly ITimeEntryRepository _repo;
        private readonly IHubContext<TimeHub> _hub;

        public TimeService(ITimeEntryRepository repo, IHubContext<TimeHub> hub)
            => (_repo, _hub) = (repo, hub);

        public async Task<EntryVm> StartAsync(Guid userId, StartEntryDto dto) {
            if (await _repo.GetRunningAsync(userId) != null)
                throw new InvalidOperationException("Ya hay una entrada activa");

            var now = DateTime.UtcNow;
            var entry = new TimeEntry {
                UserId = userId,
                StartTime = now,
                Description = dto.Description,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _repo.AddAsync(entry);

            // Notifica a los clientes conectados
            await _hub.Clients.User(userId.ToString())
                      .SendAsync("TimerStarted", entry.Id, entry.StartTime);

            return ToVm(entry);
        }

        public async Task<EntryVm> StopAsync(Guid userId, StopEntryDto dto) {
            var entry = await _repo.GetRunningAsync(userId);
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

        public async Task<IReadOnlyList<EntryVm>> ListAsync(Guid userId, DateTime from, DateTime to)
            => (await _repo.ListAsync(userId, from, to)).Select(ToVm).ToList();

        private static EntryVm ToVm(TimeEntry e)
            => new(e.Id, e.StartTime, e.EndTime, e.DurationMin, e.Description);
    }
}
