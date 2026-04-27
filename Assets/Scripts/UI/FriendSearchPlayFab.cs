using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FriendSearchPlayFab : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField searchInput;
    public TMP_Text resultText;

    [SerializeField] private GameObject confirmDeletePanel;
    [SerializeField] private TMP_Text confirmDeleteText;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    [Header("Search Results")]
    public GameObject playerResultPrefab;
    public Transform resultsParent;

    [Header("Category Buttons")]
    public Button allFriendsButton;
    public Button suggestionsButton;
    public Button activityButton;

    public GameObject allFriendOutline;
    public GameObject suggestionsOutline;
    public GameObject activityOutline;

    [Header("Settings")]
    public float searchDelay = 0.4f;

    private Coroutine searchRoutine;
    private int searchVersion = 0;

    // Cache friends to avoid calling GetFriendsList per entry
    private readonly HashSet<string> cachedFriendIds = new HashSet<string>();
    private bool friendsCacheReady = false;

    private enum Category { AllFriends, Suggestions, Activity }
    private Category currentCategory = Category.AllFriends;

    private void Start()
    {
        if (searchInput != null)
            searchInput.onValueChanged.AddListener(OnSearchTextChanged);

        if (allFriendsButton != null)
            allFriendsButton.onClick.AddListener(() => ShowCategory(Category.AllFriends));
        if (suggestionsButton != null)
            suggestionsButton.onClick.AddListener(() => ShowCategory(Category.Suggestions));
        if (activityButton != null)
            activityButton.onClick.AddListener(() => ShowCategory(Category.Activity));

        if (confirmDeletePanel != null)
            confirmDeletePanel.SetActive(false);

        ShowCategory(Category.AllFriends);
    }

    private void OnDestroy()
    {
        if (searchInput != null)
            searchInput.onValueChanged.RemoveListener(OnSearchTextChanged);

        if (searchRoutine != null)
            StopCoroutine(searchRoutine);
    }

    #region Search Bar

    private void OnSearchTextChanged(string text)
    {
        // If user is switching categories, don't search.
        // (Keeps your UI from mixing search results with categories.)
        if (currentCategory != Category.AllFriends)
            return;

        // Cancel any in-flight delayed search
        if (searchRoutine != null)
            StopCoroutine(searchRoutine);

        // Increment version to invalidate in-flight async callbacks
        searchVersion++;

        if (string.IsNullOrWhiteSpace(text) || text.Length < 3)
        {
            if (resultText != null) resultText.text = "";
            ClearResults();
            return;
        }

        searchRoutine = StartCoroutine(SearchAfterDelay(text.Trim(), searchVersion));
    }

    private IEnumerator SearchAfterDelay(string username, int version)
    {
        yield return new WaitForSeconds(searchDelay);

        // Only run if still current
        if (version != searchVersion) yield break;

        SearchPlayerByUsername(username, version);
    }

    #endregion

    #region Category Switching

    private void ShowCategory(Category category)
    {
        currentCategory = category;

        // Cancel search coroutine when switching tabs
        if (searchRoutine != null)
        {
            StopCoroutine(searchRoutine);
            searchRoutine = null;
        }
        searchVersion++;

        ClearResults();
        SetOutlines(category);

        switch (category)
        {
            case Category.AllFriends:
                PopulateAllFriends();
                break;
            case Category.Suggestions:
                PopulateFriendSuggestions();
                break;
            case Category.Activity:
                PopulateActivity();
                break;
        }
    }

    private void SetOutlines(Category category)
    {
        if (allFriendOutline != null) allFriendOutline.SetActive(category == Category.AllFriends);
        if (suggestionsOutline != null) suggestionsOutline.SetActive(category == Category.Suggestions);
        if (activityOutline != null) activityOutline.SetActive(category == Category.Activity);
    }

    #endregion

    #region Friends Cache

    private void RefreshFriendsCache(Action onReady = null)
    {
        friendsCacheReady = false;
        cachedFriendIds.Clear();

        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest(),
            result =>
            {
                if (result?.Friends != null)
                {
                    foreach (var f in result.Friends)
                    {
                        if (!string.IsNullOrEmpty(f.FriendPlayFabId))
                            cachedFriendIds.Add(f.FriendPlayFabId);
                    }
                }

                friendsCacheReady = true;
                onReady?.Invoke();
            },
            error =>
            {
                Debug.LogWarning("Failed to get friends list: " + error.GenerateErrorReport());
                friendsCacheReady = true; // allow UI to continue
                onReady?.Invoke();
            });
    }

    private bool IsFriend(string playFabId)
    {
        if (!friendsCacheReady) return false;
        return cachedFriendIds.Contains(playFabId);
    }

    #endregion

    #region Search Player

    private void SearchPlayerByUsername(string username, int version)
    {
        var request = new GetAccountInfoRequest { Username = username };

        PlayFabClientAPI.GetAccountInfo(
            request,
            result =>
            {
                if (version != searchVersion) return; // stale callback

                ClearResults();

                var info = result?.AccountInfo;
                if (info == null)
                {
                    if (resultText != null) resultText.text = "No player found";
                    return;
                }

                if (resultText != null) resultText.text = "Player Found: " + (info.Username ?? "Unknown");

                // Ensure we have friends cache once, then build entry using it
                if (!friendsCacheReady)
                {
                    RefreshFriendsCache(() =>
                    {
                        if (version != searchVersion) return;
                        CreatePlayerEntryFromAccountInfo(info, version);
                    });
                }
                else
                {
                    CreatePlayerEntryFromAccountInfo(info, version);
                }
            },
            error =>
            {
                if (version != searchVersion) return;
                ClearResults();
                if (resultText != null) resultText.text = "No player found";
            }
        );
    }

    private void CreatePlayerEntryFromAccountInfo(UserAccountInfo info, int version)
    {
        if (playerResultPrefab == null || resultsParent == null) return;

        var entry = Instantiate(playerResultPrefab, resultsParent);

        // Username
        SetTMP(entry.transform, "PlayerName", info.Username ?? "Unknown");

        // Last Seen
        string lastSeen = info.TitleInfo != null && info.TitleInfo.LastLogin.HasValue
            ? info.TitleInfo.LastLogin.Value.ToLocalTime().ToString("g")
            : "Never";
        SetText(entry.transform, "Status/StatusText", lastSeen);

        // Buttons (friend state)
        bool isFriend = IsFriend(info.PlayFabId);
        SetActive(entry.transform, "Buttons/AddButton", !isFriend);
        SetActive(entry.transform, "Buttons/DeleteButton", isFriend);

        SetupFriendButtons(entry, info.PlayFabId);

        // Pull level/avatar once
        PlayFabClientAPI.GetUserData(
            new GetUserDataRequest { PlayFabId = info.PlayFabId },
            dataResult =>
            {
                if (version != searchVersion) return;
                ApplyLevelAndAvatar(entry, dataResult);
            },
            dataError =>
            {
                Debug.LogWarning("Failed to get user data: " + dataError.GenerateErrorReport());
            }
        );
    }

    #endregion

    #region Populate Categories

    private void PopulateAllFriends()
    {
        RefreshFriendsCache(() =>
        {
            PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest(),
                result =>
                {
                    if (result?.Friends == null) return;

                    foreach (var friend in result.Friends)
                    {
                        if (playerResultPrefab == null || resultsParent == null) return;

                        var entry = Instantiate(playerResultPrefab, resultsParent);

                        string display = friend.TitleDisplayName;
                        if (string.IsNullOrEmpty(display)) display = friend.Username;
                        if (string.IsNullOrEmpty(display)) display = "Unknown";

                        SetTMP(entry.transform, "PlayerName", display);

                        // Since this is AllFriends, it's a friend
                        SetActive(entry.transform, "Buttons/AddButton", false);
                        SetActive(entry.transform, "Buttons/DeleteButton", true);

                        SetupFriendButtons(entry, friend.FriendPlayFabId);
                    }
                },
                error => Debug.LogWarning("Failed to get friends list: " + error.GenerateErrorReport())
            );
        });
    }

    private void PopulateFriendSuggestions()
    {
        RefreshFriendsCache(() =>
        {
            PlayFabClientAPI.GetLeaderboard(
                new GetLeaderboardRequest
                {
                    StatisticName = "PlayerXP",
                    StartPosition = 0,
                    MaxResultsCount = 50
                },
                leaderboardResult =>
                {
                    if (leaderboardResult?.Leaderboard == null) return;

                    string localPlayFabId = PlayFabSettings.staticPlayer?.PlayFabId;

                    var candidates = new List<PlayerLeaderboardEntry>(leaderboardResult.Leaderboard.Count);
                    foreach (var lbEntry in leaderboardResult.Leaderboard)
                    {
                        if (lbEntry == null || string.IsNullOrEmpty(lbEntry.PlayFabId))
                            continue;

                        if (!string.IsNullOrEmpty(localPlayFabId) && lbEntry.PlayFabId == localPlayFabId)
                            continue;

                        if (cachedFriendIds.Contains(lbEntry.PlayFabId))
                            continue;

                        candidates.Add(lbEntry);
                    }

                    int suggestionCount = Mathf.Min(5, candidates.Count);
                    for (int i = 0; i < suggestionCount; i++)
                    {
                        int index = UnityEngine.Random.Range(0, candidates.Count);
                        var player = candidates[index];
                        candidates.RemoveAt(index);

                        var entryGO = Instantiate(playerResultPrefab, resultsParent);
                        string displayName = string.IsNullOrEmpty(player.DisplayName) ? "Unknown" : player.DisplayName;
                        SetTMP(entryGO.transform, "PlayerName", displayName);

                        // Not a friend yet
                        SetActive(entryGO.transform, "Buttons/AddButton", true);
                        SetActive(entryGO.transform, "Buttons/DeleteButton", false);
                        SetupFriendButtons(entryGO, player.PlayFabId);

                        // Level/avatar
                        PlayFabClientAPI.GetUserData(
                            new GetUserDataRequest { PlayFabId = player.PlayFabId },
                            dataResult => ApplyLevelAndAvatar(entryGO, dataResult),
                            dataError => { }
                        );
                    }
                },
                error => Debug.LogWarning("Failed to get leaderboard for suggestions: " + error.GenerateErrorReport())
            );
        });
    }

    private void PopulateActivity()
    {
        // Load local user's "RecentPlayers"
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            dataResult =>
            {
                if (dataResult?.Data == null ||
                    !dataResult.Data.TryGetValue("RecentPlayers", out var recentValue) ||
                    string.IsNullOrWhiteSpace(recentValue?.Value))
                {
                    return;
                }

                string[] recentPlayerIds = recentValue.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (recentPlayerIds.Length == 0) return;

                RefreshFriendsCache(() =>
                {
                    foreach (string playFabId in recentPlayerIds)
                    {
                        string trimmed = playFabId.Trim();
                        if (string.IsNullOrEmpty(trimmed)) continue;

                        PlayFabClientAPI.GetAccountInfo(
                            new GetAccountInfoRequest { PlayFabId = trimmed },
                            result =>
                            {
                                var info = result?.AccountInfo;
                                if (info == null) return;

                                var entry = Instantiate(playerResultPrefab, resultsParent);

                                SetTMP(entry.transform, "PlayerName", info.Username ?? "Unknown");

                                string lastSeen = info.TitleInfo != null && info.TitleInfo.LastLogin.HasValue
                                    ? info.TitleInfo.LastLogin.Value.ToLocalTime().ToString("g")
                                    : "Never";
                                SetText(entry.transform, "Status/StatusText", lastSeen);

                                bool isFriend = IsFriend(trimmed);
                                SetActive(entry.transform, "Buttons/AddButton", !isFriend);
                                SetActive(entry.transform, "Buttons/DeleteButton", isFriend);

                                SetupFriendButtons(entry, trimmed);

                                PlayFabClientAPI.GetUserData(
                                    new GetUserDataRequest { PlayFabId = trimmed },
                                    userDataResult => ApplyLevelAndAvatar(entry, userDataResult),
                                    userDataError => { }
                                );
                            },
                            error => Debug.LogWarning("Failed to get account info for recent player: " + error.GenerateErrorReport())
                        );
                    }
                });
            },
            error => Debug.LogWarning("Failed to get recent players: " + error.GenerateErrorReport())
        );
    }

    private void ApplyLevelAndAvatar(GameObject entryGO, GetUserDataResult dataResult)
    {
        if (entryGO == null || dataResult?.Data == null) return;

        string level = "1";
        if (dataResult.Data.TryGetValue("Level", out var levelValue) && !string.IsNullOrEmpty(levelValue?.Value))
            level = levelValue.Value;

        SetTMP(entryGO.transform, "PlayerAvatar/StarIcon/StarAmountText", level);

        if (dataResult.Data.TryGetValue("AvatarURL", out var avatarValue) && !string.IsNullOrEmpty(avatarValue?.Value))
        {
            var img = GetImage(entryGO.transform, "PlayerAvatar/Player1Pic");
            if (img != null)
                StartCoroutine(LoadAvatarImage(img, avatarValue.Value));
        }
    }

    #endregion

    #region Friend Buttons

    private void ShowDeleteConfirmation(GameObject entry, string targetPlayFabId, Button addButton, Button deleteButton)
    {
        if (confirmDeletePanel == null || confirmDeleteText == null || confirmYesButton == null || confirmNoButton == null)
        {
            // Fallback: if panel isn't wired, just delete immediately.
            RemoveFriendImmediate(targetPlayFabId, addButton, deleteButton);
            return;
        }

        confirmDeletePanel.SetActive(true);
        confirmDeleteText.text = "Are you sure you want to remove this friend?";

        confirmYesButton.onClick.RemoveAllListeners();
        confirmNoButton.onClick.RemoveAllListeners();

        confirmYesButton.onClick.AddListener(() =>
        {
            confirmDeletePanel.SetActive(false);
            RemoveFriendImmediate(targetPlayFabId, addButton, deleteButton);
        });

        confirmNoButton.onClick.AddListener(() =>
        {
            confirmDeletePanel.SetActive(false);
        });
    }

    private void RemoveFriendImmediate(string targetPlayFabId, Button addButton, Button deleteButton)
    {
        PlayFabClientAPI.RemoveFriend(
            new RemoveFriendRequest { FriendPlayFabId = targetPlayFabId },
            result =>
            {
                cachedFriendIds.Remove(targetPlayFabId);

                if (addButton != null) addButton.gameObject.SetActive(true);
                if (deleteButton != null) deleteButton.gameObject.SetActive(false);
            },
            error => Debug.LogWarning("Failed to remove friend: " + error.GenerateErrorReport())
        );
    }

    private void SetupFriendButtons(GameObject entry, string targetPlayFabId)
    {
        if (entry == null) return;

        Button addButton = GetButton(entry.transform, "Buttons/AddButton");
        Button deleteButton = GetButton(entry.transform, "Buttons/DeleteButton");

        if (addButton != null)
        {
            addButton.onClick.RemoveAllListeners();
            addButton.onClick.AddListener(() =>
            {
                PlayFabClientAPI.AddFriend(
                    new AddFriendRequest { FriendPlayFabId = targetPlayFabId },
                    result =>
                    {
                        cachedFriendIds.Add(targetPlayFabId);

                        addButton.gameObject.SetActive(false);
                        if (deleteButton != null) deleteButton.gameObject.SetActive(true);
                    },
                    error => Debug.LogWarning("Failed to add friend: " + error.GenerateErrorReport())
                );
            });
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() =>
            {
                ShowDeleteConfirmation(entry, targetPlayFabId, addButton, deleteButton);
            });
        }
    }

    #endregion

    #region Results + Avatar Loading

    private void ClearResults()
    {
        if (resultsParent == null) return;

        for (int i = resultsParent.childCount - 1; i >= 0; i--)
        {
            var child = resultsParent.GetChild(i);
            if (child != null) Destroy(child.gameObject);
        }
    }

    private IEnumerator LoadAvatarImage(Image image, string url)
    {
        if (image == null || string.IsNullOrEmpty(url))
            yield break;

        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Avatar load failed: " + req.error);
                yield break;
            }

            var tex = DownloadHandlerTexture.GetContent(req);
            if (tex == null) yield break;

            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            image.sprite = sprite;
        }
    }

    #endregion

    #region Safe UI Helpers

    private static void SetActive(Transform root, string path, bool active)
    {
        var t = root.Find(path);
        if (t != null) t.gameObject.SetActive(active);
    }

    private static void SetTMP(Transform root, string path, string value)
    {
        var t = root.Find(path);
        if (t == null) return;
        var tmp = t.GetComponent<TMP_Text>();
        if (tmp != null) tmp.text = value;
    }

    private static void SetText(Transform root, string path, string value)
    {
        var t = root.Find(path);
        if (t == null) return;
        var txt = t.GetComponent<Text>();
        if (txt != null) txt.text = value;
    }

    private static Button GetButton(Transform root, string path)
    {
        var t = root.Find(path);
        return t != null ? t.GetComponent<Button>() : null;
    }

    private static Image GetImage(Transform root, string path)
    {
        var t = root.Find(path);
        return t != null ? t.GetComponent<Image>() : null;
    }

    #endregion
}
