using System.Globalization;
using Domain.Dto;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Shared;
using Microsoft.Extensions.Logging;

namespace Application.Services.DoctorServices;

public class AgendaService : IAgendaService
{
    private readonly IAgendaRepository _agendaRepository;
    private readonly ILogger<AgendaService> _logger;

    public AgendaService(IAgendaRepository agendaRepository, ILogger<AgendaService> logger)
    {
        _agendaRepository = agendaRepository;
        _logger = logger;
    }

    public async Task<bool> AddNewAvailableAgenda(string doctorId, DateTime startDateTime, DateTime endDateTime)
    {
        var doctorAvaiableAgendas = await GetDoctorAvailableAgendaByTime(doctorId, startDateTime, endDateTime);

        if (doctorAvaiableAgendas.Count > 0)
        {
            Field field = new()
            {
                Name = "startDateTime",
                Value = startDateTime.ToString(CultureInfo.InvariantCulture),
                ExMessage = "Horário livre já cadastrado."
            };
            List<Field> fields = new() { field };
                
            DataValidationException.Throw("400", "Erro no registro de agenda", "Agenda já cadastrada.", fields);
        }
        
        var newAgenda = new Agenda
        {
            StartTime = startDateTime,
            EndTime = endDateTime,
            Available = true,
            DoctorId = doctorId,
        };
        
        var res = await _agendaRepository.AddAvailableSlotAsync(newAgenda);

        return res;
    }
    
    public async Task<bool> UpdateAgenda(string agendaId, bool available, DateTime? startDateTime, DateTime? endDateTime)
    {
        var agenda = await _agendaRepository.GetAgendaById(agendaId);

        if (agenda == null)
        {
            NotFoundException.Throw("404", "Agenda não encontrada.");
        }

        var newAgenda = new Agenda
        {
            Id = agendaId,
            Available = available,
            StartTime = startDateTime ?? agenda!.StartTime,
            EndTime = endDateTime ?? agenda!.EndTime,
            DoctorId = agenda!.DoctorId
        };
        
        var res = await _agendaRepository.EditSlotAsync(newAgenda);
        
        return res;
    }
    public async Task<List<DoctorAgendaDto>> GetDoctorAvailableAgendaByTime(string doctorId, DateTime startQueryDate, DateTime endQueryDate)
    {
        return await _agendaRepository.GetAvailableSlotsAsync(doctorId, startQueryDate, endQueryDate);
    }
}