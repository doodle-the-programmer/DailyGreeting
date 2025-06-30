using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace DailyGreeting
{
    internal sealed class ModEntry : Mod
    {
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.DayStarted += this.OnNewDay;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.\nThe time is:{Game1.timeOfDay}", LogLevel.Debug);
        }

        private void OnNewDay(object? sender, EventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                this.Monitor.Log("Context is not ready, skipping daily greeting.", LogLevel.Debug);
                return;
            }

            this.Monitor.Log($"Good morning, {Game1.player.Name}! Today is {Game1.dayOfMonth} {Game1.currentSeason}.", LogLevel.Info);

            if (Game1.player.displayName == null)
            {
                this.Monitor.Log("Player display name is null, using default name.", LogLevel.Warn);
                Game1.player.displayName = "Adventurer";
            }

            string path = Path.Combine(this.Helper.DirectoryPath, "messages.json");
            string[]? messages;
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    string messagejson = reader.ReadToEnd();
                    messages = JsonSerializer.Deserialize<string[]>(messagejson);
                }
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Error reading messages.json: {ex.Message}", LogLevel.Error);
                messages = Array.Empty<string>();
            }

            if (messages == null || messages.Length == 0)
            {
                this.Monitor.Log("No messages available to display.", LogLevel.Warn);
                return;
            }

            this.Monitor.Log("Loaded messages.json. Printing to log now:", LogLevel.Debug);
            for (int i = 0; i < messages.Length; i++)
            {
                this.Monitor.Log($"Message {i}: {messages[i]}", LogLevel.Debug);
            }

            Random random = new Random();

            if (Game1.player.isMarriedOrRoommates())
            {
                int randChanceInt = random.Next(0, 10);
                if (randChanceInt == 1)
                {
                    Game1.addHUDMessage(new HUDMessage($"{Game1.player.spouse} is a wonderful spouse! Wise choice."));
                    return;
                }
            }

            int randInt = random.Next(0, messages.Length);
            Game1.addHUDMessage(new HUDMessage($"Hi {Game1.player.displayName}! {messages[randInt]}", 1));
        }
    }
}