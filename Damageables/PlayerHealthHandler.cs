using System;
using PlayerScripts;
using UnityEngine;

namespace Damageables
{
    public class PlayerHealthHandler : HealthHandler
    {
        public Transform[] targets;
        public bool IsDead { get; private set; }
        private new void Start()
        {
            base.Start();
            healthScriptableObject.Death += OnDeath;
            
        }

        private void OnDestroy()
        {
            healthScriptableObject.Death -= OnDeath;
        }

        private void OnDeath()
        {
            IsDead = true;
            Player.Instance.ChangePlayerState(new PlayerStateInfo(PlayerState.Dead));
        }
    }
}