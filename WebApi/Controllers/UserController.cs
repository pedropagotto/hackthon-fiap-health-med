﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Models;
using Application.Services.DoctorServices;
using Application.Services.EmailService;
using Application.Services.UserService;
using Domain.Dto;
using Domain.Enums;
using Domain.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UserController(IUserService userService, IEmailService emailService) : ControllerBase
{
    /// <summary>
    /// Lista usuários.
    /// </summary>
    /// <remarks>
    /// <p>A consulta permite listar todos os usuários cadastrados.</p>
    /// </remarks>
    /// <returns>Retorna usuarios.</returns>
    /// <response code="200">Usuarios disponíveis</response>
    /// <response code="401">Sem autorizacao.</response>
    /// <response code="403">Forbidden</response>
    /// <response code="500">Internal Server Error</response>
    #region getUsuariosonfig
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    #endregion
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers(CancellationToken cancellationToken = default)
    {
        var res = await userService.GetAllUsersAsync(cancellationToken); 
        return Ok(res);
    }
    
}