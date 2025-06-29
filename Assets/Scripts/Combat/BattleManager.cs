using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Evolution.Combat
{

    [Serializable]
    public class Combatant
    {
        public int Id;
        public int Hp;
        public int MaxHp;
        public int Attack;
        public int Defense;
        public float Speed;
        public float ATB;
        public float ATBMax = 5f;
        public List<Ability> Abilities = new();
        public Dictionary<string, float> Cooldowns = new();
        public List<StatusEffect> Effects = new();

        public float GetAttackBonus()
        {
            float bonus = 0f;
            foreach (var se in Effects)
                bonus += se.AttackBonus;
            return bonus;
        }

        public float GetDefenseBonus()
        {
            float bonus = 0f;
            foreach (var se in Effects)
                bonus += se.DefenseBonus;
            return bonus;
        }

        public Dictionary<Element, float> Resistances = new();

        public float GetSpeedBonus()
        {
            float bonus = 0f;
            foreach (var se in Effects)
                bonus += se.SpeedUp - se.SpeedDown;
            return bonus;
        }

        public void TickStatus()
        {
            List<StatusEffect> next = new();
            foreach (var se in Effects)
            {
                if (se.DamagePerTurn > 0)
                    Hp = Mathf.Max(Hp - se.DamagePerTurn, 0);
                if (se.HealPerTurn > 0)
                    Hp = Mathf.Min(Hp + se.HealPerTurn, MaxHp);
                se.Remaining--;
                if (se.Remaining > 0)
                    next.Add(se);
            }
            Effects = next;
        }
    }

    [Serializable]
    public class BattleState
    {
        public Dictionary<int, Combatant> Players = new();
        public Combatant Enemy = new();
        public bool Paused;
        public bool InBattle;

        public Dictionary<int, float> PlayerSpeeds
        {
            get
            {
                var d = new Dictionary<int, float>();
                foreach (var kv in Players)
                    d[kv.Key] = kv.Value.Speed;
                return d;
            }
        }
        public Dictionary<int, float> PlayerATB
        {
            get
            {
                var d = new Dictionary<int, float>();
                foreach (var kv in Players)
                    d[kv.Key] = kv.Value.ATB;
                return d;
            }
            set
            {
                foreach (var kv in value)
                    if (Players.ContainsKey(kv.Key))
                        Players[kv.Key].ATB = kv.Value;
            }
        }
        public Dictionary<int, float> PlayerMaxATB
        {
            get
            {
                var d = new Dictionary<int, float>();
                foreach (var kv in Players)
                    d[kv.Key] = kv.Value.ATBMax;
                return d;
            }
        }
        public float EnemyATB { get { return Enemy.ATB; } set { Enemy.ATB = value; } }
        public float EnemyMaxATB { get { return Enemy.ATBMax; } set { Enemy.ATBMax = value; } }
        public float EnemySpeed { get { return Enemy.Speed; } set { Enemy.Speed = value; } }

        public float GetSpeedBonus(int playerId)
        {
            if (!Players.ContainsKey(playerId)) return 0f;
            return Players[playerId].GetSpeedBonus();
        }
        public float GetEnemySpeedBonus() => Enemy.GetSpeedBonus();
    }

    public class BattleManager : MonoBehaviour
    {
        public ATBManager AtbManager;
        public BattleState State = new();

        public enum AIDifficulty { Easy, Normal, Hard }
        [SerializeField]
        public AIDifficulty Difficulty = AIDifficulty.Normal;

        private float Aggressiveness
        {
            get
            {
                return Difficulty switch
                {
                    AIDifficulty.Easy => 0.5f,
                    AIDifficulty.Normal => 1f,
                    AIDifficulty.Hard => 1.5f,
                    _ => 1f,
                };
            }
        }

        public event Action OnUIUpdate;
        public event Action<int, List<Ability>> OnSkillsRequested;
        public event Action OnSkillsClosed;
        public event Action<bool> OnBattleEnded;

        private Ability pendingAbility;
        public void ChooseAbility(Ability ability) => pendingAbility = ability;

        private void Awake()
        {
            if (AtbManager == null)
                AtbManager = GetComponent<ATBManager>();
            if (AtbManager != null)
            {
                AtbManager.OnPlayerReady += HandlePlayerReady;
                AtbManager.OnEnemyReady += HandleEnemyReady;
                AtbManager.OnTick += () => OnUIUpdate?.Invoke();
            }
        }

        public void StartBattle(Combatant enemy, IEnumerable<Combatant> players)
        {
            State = new BattleState { InBattle = true, Enemy = enemy };
            foreach (var p in players)
                State.Players[p.Id] = p;
            if (AtbManager != null)
                AtbManager.StartATB(this, State);
        }

        public void EndBattle()
        {
            State.InBattle = false;
            if (AtbManager != null)
                AtbManager.StopATB();
        }

        private void HandlePlayerReady(int id)
        {
            StartCoroutine(PlayerTurn(id));
        }

        private void HandleEnemyReady()
        {
            StartCoroutine(EnemyTurn());
        }

        private IEnumerator PlayerTurn(int playerId)
        {
            State.Paused = true;
            var player = State.Players[playerId];
            player.TickStatus();
            if (CheckEnd())
            {
                State.Paused = false;
                yield break;
            }

            // decrement cooldowns each turn
            var keys = new List<string>(player.Cooldowns.Keys);
            foreach (var key in keys)
                player.Cooldowns[key] = Mathf.Max(player.Cooldowns[key] - 1f, 0f);

            OnUIUpdate?.Invoke();

            pendingAbility = null;
            OnSkillsRequested?.Invoke(playerId, player.Abilities);
            yield return new WaitUntil(() => pendingAbility != null);

            var ability = pendingAbility;
            pendingAbility = null;
            OnSkillsClosed?.Invoke();

            Combatant target = ability.TargetSelf ? player : State.Enemy;
            ApplyAbility(player, target, ability);
            player.Cooldowns[ability.Name] = ability.Cooldown;
            player.ATB = 0f;

            if (CheckEnd())
            {
                State.Paused = false;
                yield break;
            }

            State.Paused = false;
        }

        private IEnumerator EnemyTurn()
        {
            State.Paused = true;
            State.Enemy.TickStatus();
            if (CheckEnd())
            {
                State.Paused = false;
                yield break;
            }
            ApplyEnemyAI();
            OnUIUpdate?.Invoke();
            yield return new WaitForSeconds(1f);
            State.Enemy.ATB = 0f;

            if (CheckEnd())
            {
                State.Paused = false;
                yield break;
            }
            State.Paused = false;
        }

        private void ApplyEnemyAI()
        {
            var target = ChooseEnemyTarget();
            var ability = ChooseEnemyAbility(target);
            if (ability == null)
                return;

            Combatant abilityTarget = ability.TargetSelf ? State.Enemy : target;
            if (abilityTarget == null)
                return;

            ApplyAbility(State.Enemy, abilityTarget, ability);
        }

        private Combatant ChooseEnemyTarget()
        {
            var alive = State.Players.Values.Where(p => p.Hp > 0).ToList();
            if (alive.Count == 0)
                return null;
            if (Difficulty == AIDifficulty.Easy)
                return alive[UnityEngine.Random.Range(0, alive.Count)];
            return alive.OrderBy(p => p.Hp).First();
        }

        private Ability ChooseEnemyAbility(Combatant target)
        {
            var enemy = State.Enemy;
            if (enemy == null)
                return null;

            // decrement cooldowns each turn
            var keys = new List<string>(enemy.Cooldowns.Keys);
            foreach (var key in keys)
                enemy.Cooldowns[key] = Mathf.Max(enemy.Cooldowns[key] - 1f, 0f);

            if (enemy.Abilities.Count == 0)
            {
                // ensure at least one basic attack
                var basic = ScriptableObject.CreateInstance<Ability>();
                basic.Name = "Attack";
                basic.Damage = 1;
                basic.Cooldown = 0f;
                enemy.Abilities.Add(basic);
            }

            var available = enemy.Abilities
                .Where(a => !enemy.Cooldowns.TryGetValue(a.Name, out float cd) || cd <= 0f)
                .ToList();
            if (available.Count == 0)
                return null;

            float bestScore = float.NegativeInfinity;
            Ability best = null;
            foreach (var ability in available)
            {
                float score = 0f;
                if (ability.Heal > 0 && ability.TargetSelf)
                {
                    float missing = enemy.MaxHp - enemy.Hp;
                    score = missing * ability.Heal;
                }
                else if (target != null)
                {
                    var tgt = ability.TargetSelf ? enemy : target;
                    score = CombatFormulas.CalculateDamage(enemy, tgt, ability);
                }

                score *= Aggressiveness;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = ability;
                }
            }

            if (best != null)
                enemy.Cooldowns[best.Name] = best.Cooldown;

            return best;
        }

        public void ApplyAbility(Combatant user, Combatant target, Ability ability)
        {
            if (ability == null) return;

            var targets = new List<Combatant>();
            if (ability.AreaOfEffect)
            {
                if (ability.TargetSelf)
                {
                    if (user == State.Enemy)
                        targets.Add(State.Enemy);
                    else
                        targets.AddRange(State.Players.Values);
                }
                else
                {
                    if (user == State.Enemy)
                        targets.AddRange(State.Players.Values);
                    else
                        targets.Add(State.Enemy);
                }
            }
            else
            {
                targets.Add(ability.TargetSelf ? user : target);
            }

            foreach (var t in targets)
            {
                if (ability.Damage > 0)
                {
                    int dmg = CombatFormulas.CalculateDamage(user, t, ability);
                    t.Hp = Mathf.Max(t.Hp - dmg, 0);
                }
                if (ability.Heal > 0)
                {
                    int heal = CombatFormulas.CalculateHealing(user, ability);
                    t.Hp = Mathf.Min(t.Hp + heal, t.MaxHp);
                }
                if (ability.Effect != null && ability.Effect.Remaining > 0)
                {
                    var effect = CombatFormulas.BuildStatusEffect(ability.Effect);
                    if (effect != null)
                        t.Effects.Add(effect);
                }
            }

            OnUIUpdate?.Invoke();
        }

        private bool CheckEnd()
        {
            bool enemyDead = State.Enemy.Hp <= 0;
            bool playersDead = true;
            foreach (var p in State.Players.Values)
                if (p.Hp > 0)
                    playersDead = false;

            if (enemyDead || playersDead)
            {
                bool victory = enemyDead && !playersDead;
                EndBattle();
                OnBattleEnded?.Invoke(victory);
                return true;
            }
            return false;
        }
    }
}
