using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using Square.Models;
using Square.Utilities;
using System.Text;
using System.Text.Json;

namespace BT_INTEGRATIONS.SQUARE.Controllers;

[ApiController]
[Route("[controller]")]
public class SquareWebhookController : ControllerBase
{

    private readonly ILogger<SquareWebhookController> _logger;

    public SquareWebhookController(ILogger<SquareWebhookController> logger)
    {
        _logger = logger;
    }

    [HttpPost()]
    public async Task<IActionResult> WebhookPost()
    {
        var requestBody = "";
        using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8, true, 1024, true)) {
            requestBody = await reader.ReadToEndAsync();
        }
        var signature = Request.Headers["x-square-hmacsha256-signature"];
        var isFromSquare = WebhooksHelper.IsValidWebhookEventSignature(requestBody, signature, Program.SignatureKey, Program.EventURL);

        if (!isFromSquare)
            return StatusCode(403);

        var body = Encoding.UTF8.GetBytes(requestBody);
        Program.RabbitCheckoutChannel.BasicPublish(exchange: "square.terminal.checkout",
            routingKey: string.Empty,
            basicProperties: null,
            body: body);

        return StatusCode(200);
    } 

}
