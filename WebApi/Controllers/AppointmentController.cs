﻿using System.Security.Claims;
using Application.Models;
using Application.Services.AppointmentService;
using Domain.Entities;
using Domain.Enums;
using Domain.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using WebApi.Queues.Publishers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/consultas")]
public class AppointmentController: ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IAddAppointmentSchedulePublisher _addAppointmentSchedulePublisher;
    private readonly IUpdateAppointmentStatusPublisher _updateAppointmentStatusPublisher;

    public AppointmentController(IAppointmentService appointmentService, IAddAppointmentSchedulePublisher addAppointmentSchedulePublisher, IUpdateAppointmentStatusPublisher updateAppointmentStatusPublisher)
    {
        _appointmentService = appointmentService;
        _addAppointmentSchedulePublisher = addAppointmentSchedulePublisher;
        _updateAppointmentStatusPublisher = updateAppointmentStatusPublisher;
    }

    /// <summary>
    /// Cadastra uma consulta.
    /// </summary>
    /// <returns>true</returns>
    /// <response code="201">Solicitação enviada com sucesso.</response>
    /// <response code="400">Erro na requisição.</response>
    /// <response code="401">Sem autorizacao.</response>
    /// <response code="403">Forbidden</response>
    /// <response code="500">Error</response>
    #region postConsultaConfig
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    #endregion
    [Authorize]
    [HttpPost]
    public async Task<ActionResult> PostAppointment( [FromBody] AppointmentDto request,
        CancellationToken cancellationToken)
    {
        await _addAppointmentSchedulePublisher.PublishMessage(request, cancellationToken);
        return Ok("Solicitação de agendamento enviada com sucesso!");
    }
    
    /// <summary>
    /// Lista consultas pendentes de aprovação de um médico.
    /// </summary>
    /// <returns>true</returns>
    /// <response code="200">Consultas para aprovação.</response>
    /// <response code="400">Erro na requisição.</response>
    /// <response code="401">Sem autorizacao.</response>
    /// <response code="403">Forbidden</response>
    #region getPendingConsultaConfig
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [SwaggerResponseExample(200, typeof(PendingAppointmentResponseExample))]
    #endregion
    [Authorize(Roles = "Doctor,Admin")]
    [HttpGet("pendentes/{doctorId}")]
    public async Task<ActionResult<IEnumerable<Appointment>>> GetPendingAppointment( [FromRoute] string doctorId,
        CancellationToken cancellationToken)
    {
        var res = await _appointmentService.GetPendingConfirmationAppointsAsync(doctorId);
        return Ok(res);
    }
    
    /// <summary>
    /// Confirma ou cancela consulta.
    /// </summary>
    /// <remarks>
    /// <p>Permite editar o status de uma consulta, para confirmá-la ou cancelá-la.</p>
    /// </remarks>
    /// <returns>true</returns>
    /// <response code="200">Solicitação enviada com sucesso.</response>
    /// <response code="400">Erro na requisição.</response>
    /// <response code="401">Sem autorizacao.</response>
    /// <response code="403">Forbidden</response>
    #region updateConsultaConfig
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    #endregion
    [Authorize]
    [HttpPatch("status")]
    public async Task<ActionResult> UpdatePendingAppointment(
        [FromBody]  UpdateAppointmentDto requestModel,
        CancellationToken cancellationToken)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role == "Patient" && requestModel.Status == AppointmentStatus.CancelledByPatient && string.IsNullOrWhiteSpace(requestModel.Reason))
        {
            List<Field> fields = new()
            {
                new()
                {
                    Name = "Reason",
                    Value = requestModel.Reason,
                    ExMessage = "Deve ser informada a justificativa para cancelamento."
                }
            };
            
            DataValidationException.Throw("400", "Dado inválido.", null, fields);
        }
        await _updateAppointmentStatusPublisher.PublishMessage(requestModel, cancellationToken);
        return Ok("Solicitação de confirmação/cancelamento de agendamento enviada com sucesso!");
    }
    
}