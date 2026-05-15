using DynamicShock.Models;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using WatsonWebserver;
using WatsonWebserver.Core;
using WatsonWebserver.Core.WebSockets;


namespace DynamicShock.Services
{
    public class DynamicWebSocket
    {
        const string Base_Url = "ws://localhost:4569";

        internal static WebserverSettings settings = new("localhost", 4569)
        {
            WebSockets = new()
            {
                Enable = true
            }
        };
        static async Task DefaultRoute(HttpContextBase ctx)
        {
            ctx.Response.StatusCode = 404;
            await ctx.Response.Send("Not found");
        }

        internal static Webserver server = new(settings, DefaultRoute);



        public static void Add_Endpoint(string plugin_id, string endpoint_name,
            Func<string, string?>? message_func = null, 
            Func<string?>? open_func = null)
        {
            if (!endpoint_name.StartsWith('/'))
            {
                endpoint_name = "/" + endpoint_name;
            }
            endpoint_name = "/" + plugin_id + endpoint_name;
            endpoint_name = endpoint_name.Replace("//", "/").Replace(":", "");

            DynamicShockPlugin.Log_Info(endpoint_name);

            server.Get(endpoint_name, async req => new { Mode = "http" });
            server.WebSocket(endpoint_name, async (ctx, session) =>
            {
                if (Config_Values.Websocket_Logging)
                    DynamicShockPlugin.Log_Info("Connected to " + endpoint_name);
                open_func?.Invoke();
                await foreach (WebSocketMessage message in session.ReadMessagesAsync(ctx.Token))
                {
                    if (message.MessageType == WebSocketMessageType.Text && Config_Values.Websocket_Enabled)
                    {
                        if (Config_Values.Websocket_Logging)
                            DynamicShockPlugin.Log_Info(endpoint_name + " Incoming: " + message.Text);
                        
                        string? response = message_func?.Invoke(message.Text);

                        if (response is not null)
                        {
                            if (Config_Values.Websocket_Logging)
                                DynamicShockPlugin.Log_Info(endpoint_name + " Outgoing: " + response);

                            await session.SendTextAsync(response, ctx.Token);
                        }
                        else
                        {
                            await session.SendTextAsync("", ctx.Token);
                        }
                    }
                }
            });
        }


        public DynamicWebSocket()
        {
            if (!server.IsListening)
            {
                server.Start();
                DynamicShockPlugin.Log_Info("Websocket Server Started");
            }
        }

        public static void Unload()
        {
            if (server.IsListening)
            {
                server.Stop();
            }
        }
    }
}
