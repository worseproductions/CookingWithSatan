using System;
using System.Threading.Tasks;
using CookingWithSatan.scripts.dto;
using Godot;
using Postgrest.Responses;
using Supabase;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using Supabase.Gotrue.Interfaces;
using Supabase.Realtime.Interfaces;
using Supabase.Realtime.PostgresChanges;
using Client = Supabase.Client;
using User = CookingWithSatan.scripts.dto.User;
using Ordering = Postgrest.Constants.Ordering;

namespace CookingWithSatan.scripts;

public partial class SupabaseService : Node
{
    public bool UseOnlineServices { get; set; }
    private Client Supabase { get; set; }
    public User CurrentUser { get; private set; }

    [Signal]
    public delegate void ChatMessageReceivedEventHandler(string username, string message);

    public override async void _Ready()
    {
        // ReSharper disable once StringLiteralTypo
        const string url = "https://ynleorbdwwfdfbcwsxys.supabase.co";
        // ReSharper disable once StringLiteralTypo
        const string key =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InlubGVvcmJkd3dmZGZiY3dzeHlzIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MTMwNzkzODIsImV4cCI6MjAyODY1NTM4Mn0.1KwalyXJqsQkxGslGV4VB0M-R3yxUib0liJj6DxgO-A";

        var options = new SupabaseOptions
        {
            AutoConnectRealtime = true
        };

        Supabase = new Client(url, key, options);
        await Supabase.InitializeAsync();

        Supabase.Auth.AddStateChangedListener(AuthEventHandler);
    }
    
    #region Login Handling

    private async void AuthEventHandler(IGotrueClient<Supabase.Gotrue.User, Session> sender,
        Constants.AuthState changed)
    {
        switch (changed)
        {
            case Constants.AuthState.SignedIn:
                CurrentUser = await Supabase.From<User>().Where(x => x.UserId == sender.CurrentUser.Id).Single();
                UseOnlineServices = true;
                break;
            case Constants.AuthState.SignedOut:
                CurrentUser = null;
                UseOnlineServices = false;
                break;
            case Constants.AuthState.UserUpdated:
                CurrentUser = await Supabase.From<User>().Where(x => x.UserId == sender.CurrentUser.Id).Single();
                UseOnlineServices = true;
                break;
            case Constants.AuthState.PasswordRecovery:
            case Constants.AuthState.TokenRefreshed:
            case Constants.AuthState.Shutdown:
            default:
                throw new ArgumentOutOfRangeException(nameof(changed), changed, null);
        }
    }

    /// <summary>
    /// Tries to sign up a user with the provided email and password. If the user already exists, it will try to sign in instead.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns>true if the user was signed up, false it the user was logged in instead</returns>
    public async Task<bool> LoginOrSignup(string email, string password)
    {
        try
        {
            _ = await Supabase.Auth.SignUp(email, password);
        }
        catch (GotrueException e)
        {
            if (e.Reason == FailureHint.Reason.UserAlreadyRegistered)
            {
                _ = await Supabase.Auth.SignIn(email, password);
                return false;
            }
            // TODO: Catch other exceptions like wrong password, etc.
        }

        _ = await Supabase.Auth.SignIn(email, password);
        return true;
    }

    public async Task SetUsername(string username)
    {
        await Supabase
            .From<User>()
            .Where(x => x.UserId == Supabase.Auth.CurrentUser.Id)
            .Set(x => x.DisplayName, username)
            .Update();
    }
    
    #endregion

    #region Chat Handling

    public async void SendChatMessage(string message)
    {
        if (!UseOnlineServices) return;
        var user = CurrentUser;
        if (user == null)
        {
            GD.PrintErr("User not logged in");
            return;
        }

        var data = new Livechat()
        {
            UserId = user.Id,
            Message = message
        };
        await Supabase.From<Livechat>().Insert(data);
    }

    public async Task SubscribeToChat()
    {
        await Supabase.From<Livechat>().On(PostgresChangesOptions.ListenType.Inserts, ChatMessageHandler);
    }

    private async void ChatMessageHandler(IRealtimeChannel sender, PostgresChangesResponse change)
    {
        var newChat = change.Model<Livechat>();
        var user = await Supabase.From<User>().Where(x => x.Id == newChat.UserId).Single();
        if (newChat == null || user == null || newChat.UserId == CurrentUser?.Id) return;
        Callable.From(() =>
            EmitSignal(SignalName.ChatMessageReceived, user.DisplayName, newChat.Message)).CallDeferred();
    }

    #endregion

    #region Leaderboard

    public async void AddLeaderboardScore(int score)
    {
        var entry = new Leaderboard
        {
            UserId = CurrentUser.Id,
            Score = score
        };

        await Supabase
            .From<Leaderboard>()
            .Where(x => x.UserId == CurrentUser.Id && x.Score < score)
            .Set(x => x.Score, score)
            .Update();
    }

    public async Task<ModeledResponse<Leaderboard>> GetLeaderboard()
    {
        return await Supabase.From<Leaderboard>().Order("score", Ordering.Descending).Limit(10).Get();
    }

    #endregion
}