using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Hyperbyte;

public class LeaderboardController : Singleton<LeaderboardController>
{
    public bool Initialized { get; private set; }
    public bool Authenticated { get; private set; }

    void Awake()
    {
        StartCoroutine(EnsureServicesInitialized());
    }

    private IEnumerator EnsureServicesInitialized()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            OnServicesInitialized();
            yield break;
        }

        while (UnityServices.State != ServicesInitializationState.Initialized)
        {
            yield return new WaitForSeconds(1);
        }

        OnServicesInitialized();
    }

    private void OnServicesInitialized()
    {
        Initialized = true;
        SetupEvents();
        Authenticate();
    }

    private async void Authenticate()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Authenticated = true;
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async Task UpdateScore()
    {
        try
        {
            string clearedCtryStr = PlayerPrefs.GetString("CtryMapProgress");
            int totalScore = GameManager.Instance.ClearedCtryList().Count;

            await LeaderboardsService.Instance.AddPlayerScoreAsync(
                "Color_Score",
                totalScore,
                new AddPlayerScoreOptions
                {
                    Metadata = new Dictionary<string, string> { { "ctry_list", clearedCtryStr } }
                }
            );
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
        };

        AuthenticationService.Instance.SignInFailed += err =>
        {
            Debug.LogError(err);
        };

        AuthenticationService.Instance.SignedOut += () =>
        {
            Debug.Log("Player signed out.");
        };

        AuthenticationService.Instance.Expired += () =>
        {
            Debug.Log("Player session could not be refreshed and expired.");
        };
    }
}
