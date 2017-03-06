using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HalfMoon.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Options;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace HalfMoon.Endpoint.AspNetCore.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly QueryEngine queryEngine;
        private readonly IOptions<BotFrameworkOptions> bfOptions;

        public MessagesController(QueryEngine queryEngine, IOptions<BotFrameworkOptions> bfOptions)
        {
            if (queryEngine == null) throw new ArgumentNullException(nameof(queryEngine));
            if (bfOptions == null) throw new ArgumentNullException(nameof(bfOptions));
            this.queryEngine = queryEngine;
            this.bfOptions = bfOptions;
        }

        private ConnectorClient CreateConnectorClient(Uri baseUri)
        {
            return new ConnectorClient(baseUri, bfOptions.Value.MicrosoftAppId, bfOptions.Value.MicrosoftAppPassword);
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Redirect("/");
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                var connector = CreateConnectorClient(new Uri(activity.ServiceUrl));
                Func<string, Task> replyAsync = text => connector.Conversations.ReplyToActivityAsync(
                    activity.CreateReply(text));
                // calculate something for us to return
                if (string.IsNullOrWhiteSpace(activity.Text))
                {
                    await replyAsync("You've sent me a blank message. Weird.");
                }
                else
                {
                    var text = activity.Text.Trim();
                    if (text.Length > 100)
                        await replyAsync("The message is rather long, huh?");
                    else
                    {
                        try
                        {
                            var entity = await queryEngine.ExecuteQueryAsync(activity.Text);
                            if (entity == null)
                                await replyAsync("Sorry but I cannot find any information on it. Maybe you can ask Gray Wing?");
                            else
                                await replyAsync(entity.Describe());
                        }
                        catch (Exception ex)
                        {
                            await replyAsync("Sorry but I feel there's something wrong…");
                        }
                    }
                }
            }
            else
            {
                HandleSystemMessage(activity);
            }
            return Ok();
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}
