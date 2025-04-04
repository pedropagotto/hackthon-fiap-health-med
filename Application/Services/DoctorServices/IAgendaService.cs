using Application.Models;
using Domain.Dto;
using Domain.Entities;

namespace Application.Services.DoctorServices;

public interface IAgendaService
{
    Task<bool> AddNewAvailableAgenda(string doctorId, DateTime startDateTime, DateTime endDateTime, CancellationToken cancellationToken = default);
    Task<bool> UpdateAgenda(string agendaId, UpdateAgendaDto updateAgendaDto, CancellationToken cancellationToken = default);
    Task<List<DoctorAgendaDto>> GetDoctorAvailableAgendaByTime(string doctorId, DateTime startQueryDate,
        DateTime endQueryDate, CancellationToken cancellationToken = default);
}