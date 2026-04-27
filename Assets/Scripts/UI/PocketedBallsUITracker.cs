using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PocketedBallsUITracker : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TurnManager turnManager;

    [Header("Optional Text UI")]
    [SerializeField] private TMP_Text player1PocketedText;
    [SerializeField] private TMP_Text player2PocketedText;

    [System.Serializable]
    public class NumberedBallSlot
    {
        public GameObject slotRoot;

        [Header("Solid")]
        public int solidNumber;      // e.g. 1..7
        public Image solidImage;

        [Header("Stripe")]
        public int stripeNumber;     // e.g. 9..15
        public Image stripeImage;

        public void Hide()
        {
            if (slotRoot != null) slotRoot.SetActive(true);
            if (solidImage != null) solidImage.gameObject.SetActive(false);
            if (stripeImage != null) stripeImage.gameObject.SetActive(false);
        }

        public void ShowSolid()
        {
            if (slotRoot != null) slotRoot.SetActive(true);
            if (solidImage != null) solidImage.gameObject.SetActive(true);
            if (stripeImage != null) stripeImage.gameObject.SetActive(false);
        }

        public void ShowStripe()
        {
            if (slotRoot != null) slotRoot.SetActive(true);
            if (solidImage != null) solidImage.gameObject.SetActive(false);
            if (stripeImage != null) stripeImage.gameObject.SetActive(true);
        }
    }

    [Header("Player 1 Slots (7)")]
    [SerializeField] private NumberedBallSlot[] player1Slots = new NumberedBallSlot[7];

    [Header("Player 2 Slots (7)")]
    [SerializeField] private NumberedBallSlot[] player2Slots = new NumberedBallSlot[7];

    [Header("Settings")]
    [Tooltip("If true, hides ALL slots until groups are assigned. After assignment, it will show everything that was pocketed.")]
    [SerializeField] private bool hideSlotsUntilGroupsAssigned = true;

    [SerializeField] private bool debugLogs = true;

    // These are the FINAL “owned by player” sets used to render UI after assignment.
    private readonly HashSet<int> _p1Pocketed = new();
    private readonly HashSet<int> _p2Pocketed = new();

    // These buffer pocketed balls BEFORE groups are assigned (open table, break, mixed pockets, scratches, etc.)
    private readonly HashSet<int> _pendingSolids = new();
    private readonly HashSet<int> _pendingStripes = new();

    private void Awake()
    {
        if (turnManager == null)
            turnManager = FindFirstObjectByType<TurnManager>();

        // Hard reset visuals so nothing “sticks” visible from prefab/editor state
        HideAllSlots(player1Slots);
        HideAllSlots(player2Slots);
        RefreshTextUI();
    }

    private void OnEnable()
    {
        if (turnManager == null)
        {
            Debug.LogError("[PocketedBallsUITracker] No TurnManager found.");
            return;
        }

        turnManager.OnShotResolvedUI += HandleShotResolvedUI;
        turnManager.OnGroupsAssignedUI += HandleGroupsAssignedUI;

        // If this UI object gets enabled mid-match, make sure pending gets applied (if groups already exist)
        ApplyPendingIfGroupsAssigned();
        RefreshSlots();
        RefreshTextUI();
    }

    private void OnDisable()
    {
        if (turnManager == null) return;

        turnManager.OnShotResolvedUI -= HandleShotResolvedUI;
        turnManager.OnGroupsAssignedUI -= HandleGroupsAssignedUI;
    }

    private void HandleShotResolvedUI(IReadOnlyList<Ball> pocketed)
    {
        if (pocketed == null) return;

        bool tableOpen = turnManager.TableOpen;

        if (debugLogs)
            Debug.Log($"[PocketedBallsUITracker] ShotResolved pocketed={pocketed.Count} tableOpen={tableOpen} P1Group={turnManager.P1Group} P2Group={turnManager.P2Group}");

        for (int i = 0; i < pocketed.Count; i++)
        {
            Ball b = pocketed[i];
            if (b == null) continue;

            // UI only tracks object balls (ignore cue and 8)
            if (b.IsCue || b.IsEight) continue;

            BallGroup g = b.GetGroup();
            if (g == BallGroup.None) continue;

            // If table is open, we *always* buffer by type.
            if (tableOpen)
            {
                if (g == BallGroup.Solids) _pendingSolids.Add(b.BallNumber);
                else if (g == BallGroup.Stripes) _pendingStripes.Add(b.BallNumber);
                continue;
            }

            // If groups are assigned, attribute immediately.
            if (g == turnManager.P1Group) _p1Pocketed.Add(b.BallNumber);
            else if (g == turnManager.P2Group) _p2Pocketed.Add(b.BallNumber);
        }

        // If this shot caused groups to become assigned (or they already were), apply any pending now.
        ApplyPendingIfGroupsAssigned();

        RefreshSlots();
        RefreshTextUI();
    }

    private void HandleGroupsAssignedUI(BallGroup p1, BallGroup p2)
    {
        if (debugLogs)
            Debug.Log($"[PocketedBallsUITracker] Groups assigned: P1={p1} P2={p2} | pendingSolids={_pendingSolids.Count} pendingStripes={_pendingStripes.Count}");

        // The key: once groups are assigned, convert ALL pending pocketed balls to player ownership immediately.
        ApplyPendingIfGroupsAssigned();

        RefreshSlots();
        RefreshTextUI();
    }

    private void ApplyPendingIfGroupsAssigned()
    {
        if (turnManager == null) return;
        if (turnManager.TableOpen) return; // still no assignment

        // Move pending sets into the correct player sets based on assignment.
        if (turnManager.P1Group == BallGroup.Solids)
        {
            foreach (int n in _pendingSolids) _p1Pocketed.Add(n);
            foreach (int n in _pendingStripes) _p2Pocketed.Add(n);
        }
        else if (turnManager.P1Group == BallGroup.Stripes)
        {
            foreach (int n in _pendingStripes) _p1Pocketed.Add(n);
            foreach (int n in _pendingSolids) _p2Pocketed.Add(n);
        }

        _pendingSolids.Clear();
        _pendingStripes.Clear();
    }

    private void RefreshSlots()
    {
        bool groupsAssigned = (turnManager != null && !turnManager.TableOpen);

        if (hideSlotsUntilGroupsAssigned && !groupsAssigned)
        {
            HideAllSlots(player1Slots);
            HideAllSlots(player2Slots);
            return;
        }

        ApplyPlayerSlots(player1Slots, turnManager.P1Group, _p1Pocketed);
        ApplyPlayerSlots(player2Slots, turnManager.P2Group, _p2Pocketed);
    }

    private void ApplyPlayerSlots(NumberedBallSlot[] slots, BallGroup group, HashSet<int> pocketed)
    {
        if (slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (s == null) continue;

            // reset each time
            s.Hide();

            if (group == BallGroup.Solids)
            {
                if (pocketed.Contains(s.solidNumber))
                    s.ShowSolid();
            }
            else if (group == BallGroup.Stripes)
            {
                if (pocketed.Contains(s.stripeNumber))
                    s.ShowStripe();
            }
        }
    }

    private void HideAllSlots(NumberedBallSlot[] slots)
    {
        if (slots == null) return;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null) slots[i].Hide();
        }
    }

    private void RefreshTextUI()
    {
        if (player1PocketedText != null)
            player1PocketedText.text = _p1Pocketed.Count == 0 ? "" : string.Join(", ", Sorted(_p1Pocketed));

        if (player2PocketedText != null)
            player2PocketedText.text = _p2Pocketed.Count == 0 ? "" : string.Join(", ", Sorted(_p2Pocketed));
    }

    private static List<int> Sorted(HashSet<int> set)
    {
        var list = new List<int>(set);
        list.Sort();
        return list;
    }

    public void ResetUI()
    {
        _p1Pocketed.Clear();
        _p2Pocketed.Clear();
        _pendingSolids.Clear();
        _pendingStripes.Clear();

        HideAllSlots(player1Slots);
        HideAllSlots(player2Slots);
        RefreshTextUI();
    }
}