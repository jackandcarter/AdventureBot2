using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

        private void OnEnable()
        {
            if (battleManager != null)
            {
                battleManager.OnSkillsRequested += ShowSkills;
                battleManager.OnSkillsClosed += HideSkills;
            }
        }

        private void OnDisable()
        {
            if (battleManager != null)
            {
                battleManager.OnSkillsRequested -= ShowSkills;
                battleManager.OnSkillsClosed -= HideSkills;
            }
        }

        public void SetBattleManager(BattleManager manager)
        {
            if (battleManager != null)
            {
                battleManager.OnSkillsRequested -= ShowSkills;
                battleManager.OnSkillsClosed -= HideSkills;
            }
            battleManager = manager;
            if (battleManager != null)
            {
                battleManager.OnSkillsRequested += ShowSkills;
                battleManager.OnSkillsClosed += HideSkills;
            }
        }

        private void ShowSkills(int playerId, List<Ability> abilities)
        {
            Clear();
            if (skillButtonPrefab == null || buttonRoot == null || abilities == null)
                return;

            foreach (var ab in abilities)
            {
                var btn = Instantiate(skillButtonPrefab, buttonRoot);
                var text = btn.GetComponentInChildren<Text>();
                if (text != null)
                    text.text = ab.Name;
                btn.onClick.AddListener(() => SelectAbility(ab));
                spawned.Add(btn);
            }
        }

        private void HideSkills()
        {
            Clear();
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
    }
}
