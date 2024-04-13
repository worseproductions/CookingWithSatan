using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace CookingWithSatan.scripts;

public partial class GameController : Control
{

    [Export] public ChatController ChatController;

    [Export] public ActivityController ActivityController;
    [Export] private Json _messagesJson;
    [Export] private float _donationChance = 0.9f;

    private static ChatController _chatController;
    private static ActivityController _activityController;
    
    public static int Viewers { get; set; }
    public static double Duration { get; set; }
    public static int Followers { get; set; }
    public static int ChatHappiness { get; set; }
    
    private struct MessageObject
    {
        public readonly string User;
        public string Message;
        public readonly int Mood;

        public MessageObject(string user, string message, int mood)
        {
            User = user;
            Message = message;
            Mood = mood;
        }
    }
    
    private static readonly List<MessageObject> Messages = new();
    
    private static string _voiceId;
    private static double _timeSinceLastMessage;
    private static int _messagesInLastMinute;
    private static bool _responseFlood;
    private static double _responseFloodTime;
    private static double _responseFloodDuration;
    private static readonly List<string> ResponseFloodMessages = new()
    {
        "HELLO SATAN!!!",
        "OMG SATAN!",
        "HEY SATAN!",
        "SATAN PLZ NOTICE ME!",
        "HEY HELL DADDY!"
    };
    
    public override void _Ready()
    {
        _chatController = ChatController;
        _activityController = ActivityController;
        
        Viewers = 100;
        Duration = 0;
        Followers = 0;
        ChatHappiness = 2;
        
        var voices = DisplayServer.TtsGetVoicesForLanguage("en");
        _voiceId = voices[0];
        
        foreach (var messageObject in _messagesJson.Data.AsGodotArray<Dictionary>())
        {
            Messages.Add(new MessageObject(messageObject["user"].ToString(), messageObject["message"].ToString(), messageObject["mood"].As<int>()));
        }
    }

    public override void _Process(double delta)
    {
        Duration += delta;
        
        // chat logic
        var msgPerMin = -10.446 + 4.7743 * Math.Log(Viewers);
        if (msgPerMin < 0)
        {
            msgPerMin = 0;
        }
        
        if (_responseFlood)
        {
            _responseFloodTime += delta;
            if (_responseFloodTime > _responseFloodDuration)
            {
                _responseFlood = false;
            }
        }
        
        if (_responseFlood) msgPerMin *= 2;

        if (!(_messagesInLastMinute < msgPerMin)) return;
        _timeSinceLastMessage += delta;
        if (!(_timeSinceLastMessage > 60 / msgPerMin)) return;
        _timeSinceLastMessage = 0;
        _messagesInLastMinute++;
        var messages = Messages.FindAll(message => message.Mood <= ChatHappiness + 1 && message.Mood >= ChatHappiness - 1);
        var message = messages[new Random().Next(0, messages.Count)];
        var text = "";
        if (_responseFlood)
            message.Message = ResponseFloodMessages[new Random().Next(0, ResponseFloodMessages.Count)];
        if (ChatHappiness == 2)
        {
            if (new Random().NextDouble() >= _donationChance)
            {
                text = $"[font_size=6]HIGHLIGHTED[/font_size]\n[bgcolor=#fcf00750][color=#ff7d46]{message.User}[/color][/bgcolor]: {message.Message}\n";
                MakeDonation(message.User, message.Message, new Random().Next(1, 10));
            }
        }

        if (string.IsNullOrEmpty(text)) text = $"[color=#ff7d46]{message.User}[/color]: {message.Message}\n";
        SendMessage(text);
    }

    private static void MakeDonation(string user, string message, int amount)
    {
        _activityController.AddDonation(user, amount);
        DisplayServer.TtsSpeak($"{user} donated {amount} Satancoins: {message}", _voiceId);
    }

    private static void SendMessage(string message)
    {
        _chatController.AddMessage(message);
    }

    public static void TriggerResponseFlood()
    {
        _responseFlood = true;
        _responseFloodTime = 0;
        _responseFloodDuration = new Random().Next(5, 10);
    }
}