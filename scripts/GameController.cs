using System;
using System.Collections.Generic;
using CookingWithSatan.scripts.resources;
using Godot;
using Godot.Collections;

namespace CookingWithSatan.scripts;

public partial class GameController : Control
{
    private GlobalInputHandler _globalInputHandler;
    private AudioService _audioService;
    private ScoreService _scoreService;
    private ChatController _chatController;
    private ActivityController _activityController;
    private CookingController _cookingController;
    private Json _messagesJson;

    [ExportCategory("General")] [Export] public int Viewers = 10;
    [Export] public double Duration = 0;
    [Export] public int Subs = 0;
    [Export] public int ChatHappiness = 0;

    [ExportGroup("New Viewers")] [Export] private double _newViewersSubInfluence = 0.1;
    [Export] private double _newViewersExistingViewersInfluence = 0.1;
    [Export] private double _newViewersDurationInfluence = 0.1;
    [Export] private double _newViewersHappinessInfluence = 0.1;

    // Cap = Subs * ViewerCapMultiplier + 100
    [Export] private int _viewerCapMultiplier = 20;
    [Export] private int _viewerCapStart = 100;

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
    private string _lastDonationMessage;

    [ExportCategory("Chat")] [Export] private float _donationChance = 0.8f;
    [Export] private double _timeBetweenMultipliers = 20;
    private double _timeSinceLastMultiplier = 20;

    [Export] private Array<Color> _chatColors = new()
    {
        new Color("#ff7d46"),
        new Color("#00a7e7"),
        new Color("#00b76d"),
    };

    [ExportGroup("Hype Multiplier")] private bool _hypeActive;
    private double _hypeTime;
    private double _hypeDuration;
    [Export] private int _hypeMinDuration = 5;
    [Export] private int _hypeMaxDuration = 10;

    [Export] private Array<string> _hypeVoiceLines = new()
    {
        "Let's go chat! Hype it up!",
        "Let's get some hype in the chat!",
        "Hype it up chat!",
        "Let's get some hype going!",
        "Hype it up chat!"
    };

    [Export] private Array<string> _hypeResponses = new()
    {
        "HYPE!",
        "LET'S GOOOOOO!",
        "LET'S GOOO!"
    };

    [ExportGroup("Response Flood")] private bool _responseFlood;
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

    [ExportGroup("Pog")] private bool _pogFlood;
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

    [ExportGroup("F in the chat")] private bool _fInChatFlood;
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
    
    [ExportCategory("Food")]
    [Export] private int _timesUntilRecipeBecomesBoring = 3;

    private Recipe _lastRecipe;
    private int _timesCookedRecipe = 0;

    public override void _Ready()
    {
        _globalInputHandler = GetNode<GlobalInputHandler>("/root/GlobalInputHandler");
        _audioService = GetNode<AudioService>("/root/AudioService");
        _scoreService = GetNode<ScoreService>("/root/ScoreService");
        _chatController = GetNode<ChatController>("%ChatPanel");
        _activityController = GetNode<ActivityController>("%ActivityPanel");
        _cookingController = GetNode<CookingController>("%StreamPanel");
        _messagesJson = ResourceLoader.Singleton.Load("res://chat/messages.tres") as Json;
        
        var summonController = _cookingController.GetNode<SummonController>("SummonScreen");
        
        summonController.RecipeSuccess += SuccessfullyCookedRecipe;
        summonController.RecipeFailed += FailedToCookRecipe;

        _audioService.PlayStream(AudioService.StreamType.Game);
        
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
        var msgPerMin = -10.446 + 4.7743 * Math.Log(Viewers) *
            (_responseFlood || _pogFlood || _fInChatFlood || _hypeActive ? 2 : 1);
        if (msgPerMin < 0)
        {
            msgPerMin = 0;
        }

        _timeSinceLastMultiplier += delta;
        
        ProcessMultiplier(ref _hypeActive, ref _hypeTime, ref _hypeDuration, delta);
        ProcessMultiplier(ref _pogFlood, ref _pogFloodTime, ref _pogFloodDuration, delta);
        ProcessMultiplier(ref _responseFlood, ref _responseFloodTime, ref _responseFloodDuration, delta);
        ProcessMultiplier(ref _fInChatFlood, ref _fInChatFloodTime, ref _fInChatFloodDuration, delta);

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

        if (_hypeActive)
            message.Message = _hypeResponses[new Random().Next(0, _hypeResponses.Count)];

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

    private void ProcessMultiplier(ref bool active, ref double timer, ref double duration, double delta)
    {
        if (!active) return;
        timer += delta;
        if (!(timer > duration)) return;
        active = false;
        _messagesInLastMinute = 0;
    }

    private void ProcessViewerLogic(double delta)
    {
        _updateViewersTime += delta;
        if (_updateViewersTime < _updateViewersInterval) return;
        _updateViewersTime = 0;
        // calculate viewer cap depending on followers
        var viewerCap = Subs * _viewerCapMultiplier + _viewerCapStart;
        // calculate new viewers depending on followers, current viewers,  duration and chat happiness
        var newViewers = (int)((Subs * _newViewersSubInfluence + Viewers * _newViewersExistingViewersInfluence +
                                Duration * _newViewersDurationInfluence) *
                               (ChatHappiness == 0 ? 1 : ChatHappiness * _newViewersHappinessInfluence));
        GD.Print($"new viewers: {newViewers}");
        Viewers += newViewers;
        Viewers = Math.Clamp(Viewers, 0, viewerCap);
    }

    #endregion

    #region Recipe Handling

    public void SuccessfullyCookedRecipe(Recipe recipe)
    {
        if (recipe.Name == _lastRecipe.Name)
        {
            _timesCookedRecipe++;
            if (_timesCookedRecipe >= _timesUntilRecipeBecomesBoring)
            {
                FailedToCookRecipe();
            }
        }
        else
        {
            _lastRecipe = recipe;
            _timesCookedRecipe = 1;
        }
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
        if (_timeSinceLastMultiplier < _timeBetweenMultipliers) return;
        _responseFlood = true;
        _responseFloodTime = 0;
        _timeSinceLastMultiplier = 0;
        _responseFloodDuration = new Random().Next(_responseFloodMinDuration, _responseFloodMaxDuration);
    }

    private void TriggerPogFlood()
    {
        _pogFlood = true;
        _pogFloodTime = 0;
        _timeSinceLastMultiplier = 0;
        _pogFloodDuration = new Random().Next(_pogFloodMinDuration, _pogFloodMaxDuration);
    }

    private void TriggerFInChatFlood()
    {
        _fInChatFlood = true;
        _fInChatFloodTime = 0;
        _timeSinceLastMultiplier = 0;
        _fInChatFloodDuration = new Random().Next(_fInChatFloodMinDuration, _fInChatFloodMaxDuration);
    }

    private void TriggerHype()
    {
        if (_timeSinceLastMultiplier < _timeBetweenMultipliers) return;
        var voiceLine = _hypeVoiceLines[new Random().Next(0, _hypeVoiceLines.Count)];
        DevilTts(voiceLine);
        _hypeActive = true;
        _hypeTime = 0;
        _timeSinceLastMultiplier = 0;
        _hypeDuration = new Random().Next(_hypeMinDuration, _hypeMaxDuration);
    }

    #endregion

    #region Quick Action Handlers

    public void HypeUpChat()
    {
        TriggerHype();
    }

    public void ResetIngredients()
    {
        _cookingController.ResetIngredients();
    }

    public void OpenRecipeBook()
    {
        _cookingController.OpenRecipeBook();
    }

    public void SummonRecipe()
    {
        _audioService.PlaySfx(AudioService.SoundEffectType.StartRitual);
        _cookingController.StartSummon();
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