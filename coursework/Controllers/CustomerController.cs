using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.DTOs.Customers;
using VehicleParts.Application.Interfaces;

namespace coursework.Controllers;

[ApiController]
[Route("api/customer")]
[Authorize(Policy = "CustomerOnly")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerProfileService _service;

    public CustomerController(ICustomerProfileService service)
    {
        _service = service;
    }

    private string? GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();
        var result = await _service.GetProfileAsync(userId);
        if (!result.IsSuccess)
            return NotFound(result.Message);
        return Ok(result.Data);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateCustomerProfileRequestDto dto)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();
        var result = await _service.UpdateProfileAsync(userId, dto.FullName, dto.PhoneNumber);
        if (!result.IsSuccess)
            return BadRequest(result.Message);
        return Ok();
    }

    [HttpGet("vehicles")]
    public async Task<IActionResult> GetVehicles()
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();
        var result = await _service.GetVehiclesAsync(userId);
        return Ok(result.Data);
    }

    [HttpGet("vehicles/{id}")]
    public async Task<IActionResult> GetVehicle(int id)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();
        var result = await _service.GetVehicleByIdAsync(userId, id);
        if (!result.IsSuccess)
            return NotFound(result.Message);
        return Ok(result.Data);
    }

    [HttpPost("vehicles")]
    public async Task<IActionResult> CreateVehicle([FromBody] CustomerVehicleRequestDto dto)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();
        var result = await _service.CreateVehicleAsync(userId, dto);
        if (!result.IsSuccess)
            return BadRequest(result.Message);
        return Ok();
    }

    [HttpPut("vehicles/{id}")]
    public async Task<IActionResult> UpdateVehicle(int id, [FromBody] CustomerVehicleRequestDto dto)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();
        var result = await _service.UpdateVehicleAsync(userId, id, dto);
        if (!result.IsSuccess)
            return BadRequest(result.Message);
        return Ok();
    }

    [HttpDelete("vehicles/{id}")]
    public async Task<IActionResult> DeleteVehicle(int id)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();
        var result = await _service.DeleteVehicleAsync(userId, id);
        if (!result.IsSuccess)
            return NotFound(result.Message);
        return NoContent();
    }
}
