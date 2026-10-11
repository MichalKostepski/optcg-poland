using Discord.WebSocket;

DiscordSocketClient client = new DiscordSocketClient();

client.MessageReceived += HandleMessage;