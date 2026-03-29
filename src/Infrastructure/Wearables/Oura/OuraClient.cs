using System.Net.Http.Headers;
using System.Text.Json;
using Connecvita.Domain.Entities;
using Connecvita.Domain.Enums;
using Connecvita.Infrastructure.Wearables.Abstractions;
using Connecvita.Infrastructure.Wearables.Oura.Models;
using Microsoft.Extensions.Configuration;

namespace Connecvita.Infrastructure.Wearables.Oura;

public class OuraClient : IWearableClient
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private const string BaseUrl = "https://api.ouraring.com/v2";
    private const string AuthUrl = "https://cloud.ouraring.com/oauth/authorize";
    private const string TokenUrl = "https://api.ouraring.com/oauth/token";

    public WearablePlatform Platform => WearablePlatform.OuraRing;

    public OuraClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _clientId = configuration["Wearables:Oura:ClientId"]!;
        _clientSecret = configuration["Wearables:Oura:ClientSecret"]!;
    }

    public string GetAuthorizationUrl(string userId, string redirectUri)
    {
        var scopes = "email+personal+daily+heartrate+workout+tag+session+spo2+stress+cardiovascular_age";
        return $"{AuthUrl}?response_type=code&client_id={_clientId}" +
               $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
               $"&scope={scopes}" +
               $"&state={Uri.EscapeDataString(userId)}";
    }

    public async Task<WearableToken> ExchangeCodeForTokenAsync(
        string code, string userId, string redirectUri)
    {
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = redirectUri,
            ["client_id"] = _clientId,
            ["client_secret"] = _clientSecret
        });

        var response = await _httpClient.PostAsync(TokenUrl, form);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Oura token exchange failed: {response.StatusCode} - {errorBody}");
        }
        //response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var token = JsonSerializer.Deserialize<OuraTokenResponse>(json)!;

        return WearableToken.Create(
            userId,
            Platform,
            token.AccessToken,
            token.RefreshToken,
            DateTime.UtcNow.AddSeconds(token.ExpiresIn)
        );
    }

    public async Task<WearableToken> RefreshTokenAsync(WearableToken token)
    {
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = token.RefreshToken!,
            ["client_id"] = _clientId,
            ["client_secret"] = _clientSecret
        });

        var response = await _httpClient.PostAsync(TokenUrl, form);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var newToken = JsonSerializer.Deserialize<OuraTokenResponse>(json)!;

        token.UpdateTokens(
            newToken.AccessToken,
            newToken.RefreshToken,
            DateTime.UtcNow.AddSeconds(newToken.ExpiresIn)
        );

        return token;
    }

    public async Task<WearableMetrics> FetchLatestMetricsAsync(
        WearableToken token, Guid userProfileId)
    {
        var yesterday = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
        var metrics = await FetchMetricsForDateAsync(token, userProfileId, yesterday);
        return metrics;
    }

    public async Task<List<WearableMetrics>> FetchHistoricalMetricsAsync(
    WearableToken token, Guid userProfileId, DateTime from)
    {
        var results = new List<WearableMetrics>();
        var startDate = from.ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Fetch all endpoints in parallel
        var sleepTask = FetchAsync<OuraSleepData>(token,
            $"/usercollection/daily_sleep?start_date={startDate}&end_date={endDate}");
        var readinessTask = FetchAsync<OuraReadinessData>(token,
            $"/usercollection/daily_readiness?start_date={startDate}&end_date={endDate}");
        var activityTask = FetchAsync<OuraActivityData>(token,
            $"/usercollection/daily_activity?start_date={startDate}&end_date={endDate}");
        var sleepDetailTask = FetchAsync<OuraSleepDetailData>(token,
            $"/usercollection/sleep?start_date={startDate}&end_date={endDate}");
        var spO2Task = FetchAsync<OuraSpO2Data>(token,
            $"/usercollection/daily_spo2?start_date={startDate}&end_date={endDate}");
        var stressTask = FetchAsync<OuraStressData>(token,
            $"/usercollection/daily_stress?start_date={startDate}&end_date={endDate}");

        await Task.WhenAll(sleepTask, readinessTask, activityTask,
            sleepDetailTask, spO2Task, stressTask);

        var sleepData = await sleepTask;
        var readinessData = await readinessTask;
        var activityData = await activityTask;
        var sleepDetailData = await sleepDetailTask;
        var spO2Data = await spO2Task;
        var stressData = await stressTask;

        // Group by date and merge all data sources
        var allDates = sleepData.Select(s => s.Day)
            .Union(readinessData.Select(r => r.Day))
            .Union(activityData.Select(a => a.Day))
            .Distinct()
            .OrderBy(d => d);

        foreach (var date in allDates)
        {
            var sleep = sleepData.FirstOrDefault(s => s.Day == date);
            var readiness = readinessData.FirstOrDefault(r => r.Day == date);
            var activity = activityData.FirstOrDefault(a => a.Day == date);
            var sleepDetail = sleepDetailData.FirstOrDefault(s => s.Day == date);
            var spO2 = spO2Data.FirstOrDefault(s => s.Day == date);
            var stress = stressData.FirstOrDefault(s => s.Day == date);

            var metrics = WearableMetrics.Create(
                userProfileId,
                Platform,
                DateTime.Parse(date)
            );

            metrics.UpdateMetrics(
                sleepScore: sleep?.Score,
                readinessScore: readiness?.Score,
                activityScore: activity?.Score,
                heartRate: sleepDetail?.AverageHeartRate,
                hrv: sleepDetail?.AverageHrv,
                temperature: readiness?.TemperatureDeviation,
                workoutSummary: null,
                tags: null,
                sessionData: null,
                spO2: spO2?.SpO2Percentage?.Average,
                stressHigh: stress?.StressHigh,
                recoveryHigh: stress?.RecoveryHigh,
                stressSummary: stress?.DaySummary,
                sleepEfficiency: sleepDetail?.Efficiency,
                totalSleepMinutes: sleepDetail?.TotalSleepDuration.HasValue == true
                    ? sleepDetail.TotalSleepDuration / 60
                    : null
            );

            results.Add(metrics);
        }

        return results;
    }

    private async Task<WearableMetrics> FetchMetricsForDateAsync(
        WearableToken token, Guid userProfileId, string date)
    {
        var history = await FetchHistoricalMetricsAsync(
            token, userProfileId, DateTime.Parse(date));
        return history.FirstOrDefault()
            ?? WearableMetrics.Create(userProfileId, Platform, DateTime.Parse(date));
    }

    private async Task<List<T>> FetchAsync<T>(WearableToken token, string endpoint)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.AccessToken);

        var response = await _httpClient.GetAsync($"{BaseUrl}{endpoint}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OuraDataResponse<T>>(json);
        return result?.Data ?? [];
    }
}