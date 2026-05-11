namespace BTC_ReconciliationAutomation.Server.DTOs;

public record SignUpRequest(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName);
