using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Evolution.Combat;

namespace Evolution.UI
{
    /// <summary>
    /// Displays skill buttons for the active player and notifies the BattleManager
    /// when a skill is chosen.
    /// </summary>
    public class SkillsUI : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private Button skillButtonPrefab;
        [SerializeField] private Transform buttonRoot;

        private readonly List<Button> spawned = new();
        private int activePlayerId = -1;

        private void OnEnable()
        {
            if (battleManager != null)
            {
                battleManager.OnSkillsRequested += ShowSkills;
                battleManager.OnSkillsClosed += HideSkills;
                battleManager.OnUIUpdate += RefreshCooldowns;
            }
        }

        private void OnDisable()
        {
            if (battleManager != null)
            {
                battleManager.OnSkillsRequested -= ShowSkills;
                battleManager.OnSkillsClosed -= HideSkills;
                battleManager.OnUIUpdate -= RefreshCooldowns;
            }
        }

        public void SetBattleManager(BattleManager manager)
        {
            if (battleManager != null)
            {
                battleManager.OnSkillsRequested -= ShowSkills;
                battleManager.OnSkillsClosed -= HideSkills;
                battleManager.OnUIUpdate -= RefreshCooldowns;
            }
            battleManager = manager;
            if (battleManager != null)
            {
                battleManager.OnSkillsRequested += ShowSkills;
                battleManager.OnSkillsClosed += HideSkills;
                battleManager.OnUIUpdate += RefreshCooldowns;
            }
        }

        private void ShowSkills(int playerId, List<Ability> abilities)
        {
            Clear();
            activePlayerId = playerId;
            if (skillButtonPrefab == null || buttonRoot == null || abilities == null)
                return;

            foreach (var ab in abilities)
            {
                var btn = Instantiate(skillButtonPrefab, buttonRoot);
                var text = btn.GetComponentInChildren<TMP_Text>();
                if (text != null)
                    text.text = ab.Name;
                var ability = ab; // capture local
                btn.onClick.AddListener(() => SelectAbility(ability));
                spawned.Add(btn);
            }

            RefreshCooldowns();
            gameObject.SetActive(true);
        }

        private void HideSkills()
        {
            gameObject.SetActive(false);
            Clear();
            activePlayerId = -1;
        }

        private void Clear()
        {
            foreach (var b in spawned)
                if (b != null)
                    Destroy(b.gameObject);
            spawned.Clear();
        }

        private void SelectAbility(Ability ability)
        {
            if (battleManager != null)
                battleManager.ChooseAbility(ability);
        }

        private void RefreshCooldowns()
        {
            if (battleManager == null || activePlayerId < 0)
                return;

            if (!battleManager.State.Players.TryGetValue(activePlayerId, out var player))
                return;

            for (int i = 0; i < spawned.Count && i < player.Abilities.Count; i++)
            {
                var ab = player.Abilities[i];
                var btn = spawned[i];
                if (btn == null) continue;
                var text = btn.GetComponentInChildren<TMP_Text>();
                float cd = player.Cooldowns.TryGetValue(ab.Name, out var c) ? c : 0f;
                if (text != null)
                    text.text = cd > 0 ? $"{ab.Name} ({cd:0})" : ab.Name;
                btn.interactable = cd <= 0f;
            }
        }
    }
}
