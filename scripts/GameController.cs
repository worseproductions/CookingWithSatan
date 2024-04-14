using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Supabase.Gotrue;

namespace CookingWithSatan.scripts;

public partial class GameController : Control
{
    private ChatController _chatController;
    private ActivityController _activityController;
    private Json _messagesJson;

    [Export] public int Viewers = 10;
    [Export] public double Duration = 0;
    [Export] public int Subs = 0;
    [Export] public int ChatHappiness = 0;
    [Export] private float _donationChance = 0.5f;
    
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
    
    private readonly List<MessageObject> _messages = new();
    
    private string _voiceId;
    private double _timeSinceLastMessage;
    private int _messagesInLastMinute;
    private int _messageMultiplier;
    private bool _multiplierActive;
    private double _multiplierTime;
    private double _multiplierDuration = 10;
    private string _lastDonationMessage;
    private bool _responseFlood;
    private double _responseFloodTime;
    private double _responseFloodDuration;
    private readonly List<string> _responseFloodMessages = new()
    {
        "HELLO SATAN!!!",
        "OMG SATAN!",
        "HEY SATAN!",
        "SATAN PLZ NOTICE ME!",
        "HEY HELL DADDY!"
    };

    private readonly List<string> _subscribers = new();
    
    private double _updateViewersTime = 5;
    private int _updateViewersInterval = 5;
    
    public override void _Ready()
    {
        var root = GetTree().Root;
        var currentScene = root.GetChild(root.GetChildCount() - 1);
        _chatController = currentScene.GetNode<ChatController>("%ChatPanel");
        _activityController = currentScene.GetNode<ActivityController>("%ActivityPanel");
        _messagesJson = ResourceLoader.Singleton.Load("res://chat/messages.tres") as Json;
        
        var voices = DisplayServer.TtsGetVoicesForLanguage("en");
        _voiceId = voices[0];

        if (_messagesJson == null) return;
        foreach (var messageObject in _messagesJson.Data.AsGodotArray<Dictionary>())
        {
            _messages.Add(new MessageObject(messageObject["user"].ToString(), messageObject["message"].ToString(),
                messageObject["mood"].As<int>()));
        }
    }

    public override void _Process(double delta)
    {
        if (_chatController == null || _activityController == null)
        {
            var root = GetTree().Root;
            var currentScene = root.GetChild(root.GetChildCount() - 1);
            _chatController = currentScene.GetNode<ChatController>("%ChatPanel");
            _activityController = currentScene.GetNode<ActivityController>("%ActivityPanel");
        }
        Duration += delta;
        
        // chat logic
        ProcessChatLogic(delta);
        ProcessViewerLogic(delta);
    }

    private void ProcessChatLogic(double delta)
    {
        var msgPerMin = -10.446 + 4.7743 * Math.Log(Viewers) * (_responseFlood ? 2 : 1) * _messageMultiplier;
        if (msgPerMin < 0)
        {
            msgPerMin = 0;
        }

        if (_multiplierActive)
        {
            _multiplierTime += delta;
            if (_multiplierTime > _multiplierDuration)
            {
                _multiplierActive = false;
                _messageMultiplier = 1;
                _messagesInLastMinute = 0;
            }
        }
        else
        {
            _messageMultiplier = 1;
        }
        
        if (_responseFlood)
        {
            _responseFloodTime += delta;
            if (_responseFloodTime > _responseFloodDuration)
            {
                _responseFlood = false;
                _messagesInLastMinute = 0;
            }
        }

        if (!(_messagesInLastMinute < msgPerMin)) return;
        _timeSinceLastMessage += delta;
        if (!(_timeSinceLastMessage > 60 / msgPerMin)) return;
        _timeSinceLastMessage = 0;
        _messagesInLastMinute++;
        var messages = _messages.FindAll(message => message.Mood <= ChatHappiness + 1 && message.Mood >= ChatHappiness - 1);
        var message = messages[new Random().Next(0, messages.Count)];
        var text = "";
        if (_responseFlood)
            message.Message = _responseFloodMessages[new Random().Next(0, _responseFloodMessages.Count)];
        if (ChatHappiness >= 0 && _lastDonationMessage != message.Message)
        {
            var happinessMultiplier = ChatHappiness switch
            {
                0 => 1,
                1 => 1.1,
                2 => 1.2,
                _ => 0
            };
            if (new Random().NextDouble() * happinessMultiplier >= _donationChance)
            {
                _lastDonationMessage = message.Message;
                if (!_subscribers.Contains(message.User) && new Random().NextDouble() >= 0.5)
                {
                    // subscriber
                    text = $"[font_size=6]HIGHLIGHTED[/font_size]\n[bgcolor=#fcf00750][color=#ff7d46]{message.User}[/color][/bgcolor]: {message.Message}\n";
                    AddSubscriber(message.User, message.Message, new Random().Next(1, 12));
                }
                else
                {
                    // donation
                    text = $"[font_size=6]HIGHLIGHTED[/font_size]\n[bgcolor=#fcf00750][color=#ff7d46]{message.User}[/color][/bgcolor]: {message.Message}\n";
                    MakeDonation(message.User, message.Message, new Random().Next(1, 10));
                }
            }
        }

        if (string.IsNullOrEmpty(text)) text = $"[color=#ff7d46]{message.User}[/color]: {message.Message}\n";
        SendMessage(text);
    }

    private void ProcessViewerLogic(double delta)
    {
        _updateViewersTime += delta;
        if (_updateViewersTime < _updateViewersInterval) return;
        _updateViewersTime = 0;
        // calculate viewer cap depending on followers
        var viewerCap = Subs * 20 + 100;
        // calculate new viewers depending on followers, current viewers,  duration and chat happiness
        var newViewers = (int) ((Subs * 0.1 + Viewers * 0.1 + Duration * 0.1) * (ChatHappiness == 0 ? 1 : ChatHappiness * 0.1));
        GD.Print($"new viewers: {newViewers}");
        Viewers += newViewers;
        Viewers = Math.Clamp(Viewers, 0, viewerCap);
    }

    private void MakeDonation(string user, string message, int amount)
    {
        _activityController.AddDonation(user, amount);
        var satanCoinsString = amount switch
        {
            1 => "Satancoins",
            _ => "Satancoins"
        };
        DisplayServer.TtsSpeak($"{user} donated {amount} {satanCoinsString}: {message}", _voiceId, pitch: 0.0f);
    }

    private void AddSubscriber(string user, string message, int months)
    {
        _activityController.AddSubscriber(user, months);
        _subscribers.Add(user);
        var monthsString = months switch
        {
            1 => "month",
            _ => "months"
        };
        DisplayServer.TtsSpeak($"{user} just subscribed for {months} {monthsString}! {message}", _voiceId, pitch: 0.0f);
        Subs++;
    }

    private void SendMessage(string message)
    {
        _chatController.AddMessage(message);
    }

    public void TriggerResponseFlood()
    {
        _responseFlood = true;
        _responseFloodTime = 0;
        _responseFloodDuration = new Random().Next(5, 10);
    }
    
    public void HypeUpChat()
    {
        DisplayServer.TtsSpeak("Let's go chat! Hype it up!", _voiceId, pitch: 2.0F, volume: 60);
        _messageMultiplier = 2;
    }
}