using System;
using System.IO;
using System.Linq;
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
            List<GreetingMessage>? messages;
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    string messagejson = reader.ReadToEnd();
                    messages = JsonSerializer.Deserialize<List<GreetingMessage>>(messagejson);
                }
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Error reading messages.json: {ex.Message}", LogLevel.Error);
                messages = new List<GreetingMessage>();
            }

            if (messages == null || messages.Count == 0)
            {
                this.Monitor.Log("No messages available to display.", LogLevel.Warn);
                return;
            }

            this.Monitor.Log("Loaded 'messages.json'. Printing to log now:", LogLevel.Debug);
            for (int i = 0; i < messages.Count; i++)
            {
                this.Monitor.Log($"Message {i}: {messages[i].text} (Likelihood: {messages[i].likelihood})", LogLevel.Debug);
            }

            // Weighted random selection
            int totalWeight = messages.Sum(m => m.likelihood);
            int choice = new Random().Next(0, totalWeight);
            int cumulative = 0;
            GreetingMessage? selected = null;
            foreach (var msg in messages)
            {
                cumulative += msg.likelihood;
                if (choice < cumulative)
                {
                    selected = msg;
                    break;
                }
            }
            if (selected != null)
            {
                Game1.addHUDMessage(new HUDMessage($"Hey {Game1.player.displayName}! {selected.text}", 1));
            }
        }
    }

    public class GreetingMessage
    {
        public string text { get; set; } = "";
        public int likelihood { get; set; } = 1;
    }
}