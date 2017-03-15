using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HalfMoon.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace HalfMoon.Endpoint.AspNetCore.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly QueryEngine queryEngine;
        private readonly MicrosoftAppCredentials credentials;
        private readonly ILogger logger;

        public MessagesController(QueryEngine queryEngine, MicrosoftAppCredentials credentials, ILoggerFactory loggerFactory)
        {
            if (queryEngine == null) throw new ArgumentNullException(nameof(queryEngine));
            if (credentials == null) throw new ArgumentNullException(nameof(credentials));
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            this.queryEngine = queryEngine;
            this.credentials = credentials;
            logger = loggerFactory.CreateLogger<MessagesController>();
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Redirect("/");
        }

        private const string PROMPT_GUIDE = "If you want to learn something on Warriors, just type in the words. For example, you may type Half Moon to learn something about me ^_^";

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        [Authorize(Roles = "Bot")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl), credentials);
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
                    if (text.Length > 300)
                        await replyAsync("The message is rather long, huh?");
                    else if (text.ToLower().Contains("knock"))
                    {
                        await replyAsync("Who?");
                        await replyAsync("Well, I'm now runnig on .NET Core, which is unfortunately total stateless.\n" +
                            PROMPT_GUIDE);
                    }
                    else
                    {
                        try
                        {
                            var entity = await queryEngine.ExecuteQueryAsync(text);
                            if (entity == null)
                            {
                                logger.LogInformation("Processed query \"{0}\" with null response.", text);
                                await replyAsync("Sorry but I cannot find any information on it. Why not ask Gray Wing?");
                                await replyAsync("If you still have no idea what \"Warriors\" is, you can just type \"Into the Wild\", and I will tell you something.");
                            }
                            else
                            {
                                logger.LogInformation("Processed query \"{0}\" with response: {1}", text, entity);
                                await replyAsync(entity.Describe());
                                await replyAsync(string.Format("You may visit {0} for more information.",
                                    entity.DetailUrl));
                                switch (entity.Name)
                                {
                                    case "Half Moon":
                                        await replyAsync(
                                            "Well, it may seem weird, but some gray tabby nerd is also working on a two-leggish bot with the same name.");
                                        break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(new EventId(0), ex, ex.Message);
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
            //var appCredentials = new MicrosoftAppCredentials(this.configuration);
            //var connector = new ConnectorClient(new Uri(message.ServiceUrl), appCredentials);
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
               logger.LogInformation(message.Action);
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
