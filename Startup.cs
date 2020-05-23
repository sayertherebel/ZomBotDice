// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZomBotDice.LUIS;
using ZomBotDice.Models;
using ZomBotDice.Validators;
using ZomBotDice.Dialogs;
using ZomBotDice.Bots;
using System.Collections.Concurrent;
using Microsoft.Bot.Schema;
using Microsoft.AspNetCore.Authorization;
using ZomBotDice.Helpers;

namespace ScribeBotV4
{
    public class Startup
    {
        private const string BotOpenIdMetadataKey = "BotOpenIdMetadata";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(settings => settings.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Create the Bot Framework Adapter.
            services.AddSingleton<IMiddleware, ScribeBotMiddleware>();
            

            services.AddTransient<IAuthorizationHandler, ApiKeyRequirementHandler>();
            services.AddAuthorization(authConfig =>
            {
                authConfig.AddPolicy("ApiKeyPolicy",
                    policyBuilder => policyBuilder
                        .AddRequirements(new ApiKeyRequirement(new[] { "28c11593-0f54-49ca-93c5-c6ee63bc9147" })));
            });
            // Create a global hashset for our ConversationReferences
            services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();

            services.AddSingleton<ConcurrentDictionary<string, string>>();

            //services.AddSingleton<ScribeFunctions>();

            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();

            // Create the User state. (Used in this bot's Dialog implementation.)
            services.AddSingleton<UserState>();
            
            services.AddSingleton<GameStateHandler>();
            services.AddSingleton<GameIDValidator>();
            //services.AddSingleton<IAssigneeValidator, AssigneeValidator>();
            services.AddSingleton<INotebodyValidator, NoteBodyValidator>();
            //services.AddSingleton<IIRNumberValidator, IRNumberValidator>();

            services.AddSingleton<CardIdMappingHandler>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            // Create Dialogs
            services.AddSingleton<NewGameDialog>();
            services.AddSingleton<JoinGameDialog>();
            //services.AddSingleton<AddNoteOrResolveDialog>();
            //services.AddSingleton<MyTicketsDialog>();
            //services.AddSingleton<ReassignDialog>();
            //services.AddSingleton<SetIncidentStatusDialog>();
            //services.AddSingleton<SetOnHoldDateDialog>();
            //services.AddSingleton<ShowDialog>();

            services.AddSingleton<RootDialog>();

            services.AddSingleton<ScribeRecognizer>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            //services.AddTransient<IBot, ScribeBotV4<RootDialog>>();

            services.AddBot<ZomBotDice<RootDialog>>(scribebot => {
                scribebot.Middleware.Add(new ScribeBotMiddleware());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            
            app.UseMvc();
        }
    }
}
