using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using TelegramBotNavigation.Bot;

namespace TelegramBotNavigation.Controllers
{
    [ApiController]
    [Route("api/bot/update")]
    public class TelegramWebhookController : ControllerBase
    {
        private readonly IUpdateProcessor _updateHandler;

        public TelegramWebhookController(IUpdateProcessor updateHandler)
        {
            _updateHandler = updateHandler;
        }

        [HttpPost]
        public async Task<IActionResult> HandleUpdate([FromBody] Update update, CancellationToken ct)
        {
            if (update == null) return BadRequest();

            await _updateHandler.HandleAsync(update, ct);
            return Ok();
        }
    }
}
