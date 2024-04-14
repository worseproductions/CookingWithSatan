using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace CookingWithSatan.scripts;

public partial class GameController : Control
{
    private GlobalInputHandler _globalInputHandler;
    private ScoreService _scoreService;
    private ChatController _chatController;
    private ActivityController _activityController;
    private Json _messagesJson;

    [ExportCategory("General")]
    [Export] public int Viewers = 10;
    
    // Cap = Subs * ViewerCapMultiplier + 100
    [Export] public int ViewerCapMultiplier = 20;
    [Export] public int ViewerCapStart = 100;
    [Export] public double Duration = 0;
    [Export] public int Subs = 0;
    [Export] public int ChatHappiness = 0;

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
    
    [ExportCategory("Chat")]
    
    [Export] private float _donationChance = 0.8f;

    [Export] private Array<Color> _chatColors = new()
    {
        new Color("#ff7d46"),
        new Color("#00a7e7"),
        new Color("#00b76d"),
    };
    
    [ExportGroup("Hype Multiplier")]
    private bool _multiplierActive;
    private double _multiplierTime;
    private double _multiplierDuration;
    [Export] private int _multiplierMinDuration = 5;
    [Export] private int _multiplierMaxDuration = 10;
    private string _lastDonationMessage;
    
    [ExportCategory("Response Flood")]
    private bool _responseFlood;
    private double _responseFloodTime;
    private double _responseFloodDuration;
    [Export] private int _responseFloodMinDuration = 5;
    [Export] private int _responseFloodMaxDuration = 10;
    [Export] private Array<string> _responseFloodMessages = new()
    {
        "HELLO SATAN!!!",
        "OMG SATAN!",
        "HEY SATAN!",
        "SATAN PLZ NOTICE ME!",
        "HEY HELL DADDY!"
    };

    [ExportCategory("Pog")]
    private bool _pogFlood;
    private double _pogFloodTime;
    private double _pogFloodDuration;
    [Export] private int _pogFloodMinDuration = 5;
    [Export] private int _pogFloodMaxDuration = 10;
    [Export] private Array<string> _pogFloodMessages = new()
    {
        "POGGERS",
        "PogChamp",
        "PogU",
        "Pog",
        "Poggers in the chat!"
    };
    
    [ExportCategory("F in the chat")]
    private bool _fInChatFlood;
    private double _fInChatFloodTime;
    private double _fInChatFloodDuration;
    [Export] private int _fInChatFloodMinDuration = 5;
    [Export] private int _fInChatFloodMaxDuration = 10;
    [Export] private Array<string> _fInChatFloodMessages = new()
    {
        "F",
        "F in the chat",
        "Press F to pay respects",
        "F",
        "F"
    };

    private readonly List<string> _subscribers = new();
    
    private double _updateViewersTime = 5;
    private int _updateViewersInterval = 5;
    private bool _donationGoalReached;
    private bool _manuallyEndStream;

    public override void _Ready()
    {
        _globalInputHandler = GetNode<GlobalInputHandler>("/root/GlobalInputHandler");
        _scoreService = GetNode<ScoreService>("/root/ScoreService");
        _chatController = GetNode<ChatController>("%ChatPanel");
        _activityController = GetNode<ActivityController>("%ActivityPanel");
        _messagesJson = ResourceLoader.Singleton.Load("res://chat/messages.tres") as Json;

        _activityController.DonationGoalReached += OnDonationGoalReached;

        var voices = DisplayServer.TtsGetVoicesForLanguage("en");
        _voiceId = voices[0];

        if (_messagesJson == null) return;
        foreach (var messageObject in _messagesJson.Data.AsGodotArray<Dictionary>())
        {
            _messages.Add(new MessageObject(messageObject["user"].ToString(), messageObject["message"].ToString(),
                messageObject["mood"].As<int>()));
        }
    }

    #region Processing
    
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

        ProcessWinLoseConditions(delta);

        // chat logic
        ProcessChatLogic(delta);
        ProcessViewerLogic(delta);
    }

    private void ProcessWinLoseConditions(double delta)
    {
        if (_manuallyEndStream || Viewers <= 0)
        {
            _scoreService.Win = false;
            _scoreService.Viewers = 0;
            _scoreService.Uptime = (int)Duration;
            _scoreService.Subs = Subs;
            GetTree().ChangeSceneToFile("res://scenes/end.tscn");
        }

        if (!_donationGoalReached) return;
        _scoreService.Win = true;
        _scoreService.Viewers = Viewers;
        _scoreService.Uptime = (int)Duration;
        _scoreService.Subs = Subs;
        GetTree().ChangeSceneToFile("res://scenes/end.tscn");
    }

    private void ProcessChatLogic(double delta)
    {
        var msgPerMin = -10.446 + 4.7743 * Math.Log(Viewers) * (_responseFlood || _pogFlood || _fInChatFlood ? 2 : 1) * _messageMultiplier;
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
        
        if (_pogFlood)
        {
            _pogFloodTime += delta;
            if (_pogFloodTime > _pogFloodDuration)
            {
                _pogFlood = false;
                _messagesInLastMinute = 0;
            }
        }
        
        if (_fInChatFlood)
        {
            _fInChatFloodTime += delta;
            if (_fInChatFloodTime > _fInChatFloodDuration)
            {
                _fInChatFlood = false;
                _messagesInLastMinute = 0;
            }
        }

        if (!(_messagesInLastMinute < msgPerMin)) return;
        _timeSinceLastMessage += delta;
        if (!(_timeSinceLastMessage > 60 / msgPerMin)) return;
        _timeSinceLastMessage = 0;
        _messagesInLastMinute++;
        var messages =
            _messages.FindAll(message => message.Mood <= ChatHappiness + 1 && message.Mood >= ChatHappiness - 1);
        var message = messages[new Random().Next(0, messages.Count)];
        var color = _chatColors[new Random().Next(0, _chatColors.Count)];
        var text = "";
        
        if (_responseFlood)
            message.Message = _responseFloodMessages[new Random().Next(0, _responseFloodMessages.Count)];
        
        if (_pogFlood)
            message.Message = _pogFloodMessages[new Random().Next(0, _pogFloodMessages.Count)];
        
        if (_fInChatFlood)
            message.Message = _fInChatFloodMessages[new Random().Next(0, _fInChatFloodMessages.Count)];
        
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
                    text =
                        $"[font_size=6]HIGHLIGHTED[/font_size]\n[bgcolor=#fcf00750][color=#{color.ToHtml()}]{message.User}[/color][/bgcolor]: {message.Message}\n";
                    AddSubscriber(message.User, message.Message, new Random().Next(1, 12));
                }
                else
                {
                    // donation
                    text =
                        $"[font_size=6]HIGHLIGHTED[/font_size]\n[bgcolor=#fcf00750][color=#{color.ToHtml()}]{message.User}[/color][/bgcolor]: {message.Message}\n";
                    MakeDonation(message.User, message.Message, new Random().Next(1, 10));
                }
            }
        }

        if (string.IsNullOrEmpty(text)) text = $"[color=#{color.ToHtml()}]{message.User}[/color]: {message.Message}\n";
        SendMessage(text);
    }

    private void ProcessViewerLogic(double delta)
    {
        _updateViewersTime += delta;
        if (_updateViewersTime < _updateViewersInterval) return;
        _updateViewersTime = 0;
        // calculate viewer cap depending on followers
        var viewerCap = Subs * ViewerCapMultiplier + ViewerCapStart;
        // calculate new viewers depending on followers, current viewers,  duration and chat happiness
        var newViewers = (int)((Subs * 0.1 + Viewers * 0.1 + Duration * 0.1) *
                               (ChatHappiness == 0 ? 1 : ChatHappiness * 0.1));
        GD.Print($"new viewers: {newViewers}");
        Viewers += newViewers;
        Viewers = Math.Clamp(Viewers, 0, viewerCap);
    }
    
    #endregion
    
    #region Recipe Handling
    
    public void SuccessfullyCookedRecipe()
    {
        ChatHappiness = Math.Clamp(ChatHappiness + 1, -2, 2);
        TriggerPogFlood();
    }
    
    public void FailedToCookRecipe()
    {
        ChatHappiness = Math.Clamp(ChatHappiness + 1, -2, 2);
        TriggerFInChatFlood();
    }
    
    #endregion

    #region Donation and Subscriber Handling
    
    private void MakeDonation(string user, string message, int amount)
    {
        _activityController.AddDonation(user, amount);
        var satanCoinsString = amount switch
        {
            1 => "Satancoin",
            _ => "Satancoins"
        };
        message = amount > 5 ? message : "";
        var thankYouString = amount switch
        {
            > 10 => "Oh my god thank you so much",
            > 5 => "Thank you so much",
            _ => "Thank you"
        };
        ChatTts($"{user.Replace('_', ' ')} donated {amount} {satanCoinsString}: {message}");
        DevilTts($"{thankYouString} for the {amount} {satanCoinsString} {user.Replace('_', ' ')}!");
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
        message = months > 3 ? message : "";
        var thankYouString = months switch
        {
            > 6 => "Oh my god thank you so much",
            > 3 => "Thank you so much",
            _ => "Thank you"
        };
        ChatTts($"{user.Replace('_', ' ')} just subscribed for {months} {monthsString}! {message}");
        DevilTts($"{thankYouString} for the {months} {monthsString} {user.Replace('_', ' ')}!");
        Subs++;
    }
    
    private void OnDonationGoalReached()
    {
        const int id = 123;
        DevilTts(
            "Oh my god chat thank you so much for reaching the donation goal! That's it for today, see y'all next time!",
            id);

        DisplayServer.TtsSetUtteranceCallback(DisplayServer.TtsUtteranceEvent.Ended,
            Callable.From((int idToCheck) =>
                {
                    if (idToCheck != id) return;
                    DisplayServer.TtsStop();
                    _donationGoalReached = true;
                }
            )
        );
    }
    
    #endregion
    
    #region Chat Handling

    private void SendMessage(string message)
    {
        _chatController.AddMessage(message);
    }

    public void TriggerResponseFlood()
    {
        _responseFlood = true;
        _responseFloodTime = 0;
        _responseFloodDuration = new Random().Next(_responseFloodMinDuration, _responseFloodMaxDuration);
    }

    public void TriggerPogFlood()
    {
        _pogFlood = true;
        _pogFloodTime = 0;
        _pogFloodDuration = new Random().Next(_pogFloodMinDuration, _pogFloodMaxDuration);
    }
    
    public void TriggerFInChatFlood()
    {
        _fInChatFlood = true;
        _fInChatFloodTime = 0;
        _fInChatFloodDuration = new Random().Next(_fInChatFloodMinDuration, _fInChatFloodMaxDuration);
    }
    
    #endregion

    #region Quick Action Handlers
    
    public void HypeUpChat()
    {
        DevilTts("Let's go chat! Hype it up!");
        _messageMultiplier = 2;
        _multiplierActive = true;
        _multiplierTime = 0;
        _multiplierDuration = new Random().Next(_multiplierMinDuration, _multiplierMaxDuration);
    }
    
    public void ResetIngredients()
    {
        TriggerFInChatFlood(); // TODO temporary to test f in chat
    }
    
    public void OpenRecipeBook()
    {
        TriggerPogFlood(); // TODO temporary to test pogs in chat
    }

    public void EndStream()
    {
        const int id = 178;
        DevilTts("Have to wrap up early today, see you guys next time!", id);
        DisplayServer.TtsSetUtteranceCallback(DisplayServer.TtsUtteranceEvent.Ended,
            Callable.From((int idToCheck) =>
            {
                if (idToCheck != id) return;
                DisplayServer.TtsStop();
                _manuallyEndStream = true;
            })
        );
    }
    
    #endregion
    
    #region TTS

    private void DevilTts(string text, int id = 0)
    {
        DisplayServer.TtsSpeak(text, _voiceId, pitch: 2.0F, volume: _globalInputHandler.SoundEnabled ? 50 : 0,
            utteranceId: id);
    }

    private void ChatTts(string text, int id = 0)
    {
        DisplayServer.TtsSpeak(text, _voiceId, pitch: 0.0F, volume: _globalInputHandler.SoundEnabled ? 50 : 0,
            utteranceId: id);
    }
    
    #endregion
}