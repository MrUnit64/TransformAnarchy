namespace RotationAnarchy
{
    using Parkitect.UI;
    using RotationAnarchy.Internal;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public enum ParkitectState
    {
        None,
        Placement
    }

    public class RAController : ModChange
    {
        public event Action<bool> OnActiveChanged;
        public event Action<ParkitectState> OnGameStateChanged;

        public bool Active { get; private set; }
        public bool IsWindowOpened => RAWindow.Instance != null;

        public ParkitectState GameState
        {
            get => _gameState;
            set
            {
                if (value != _gameState)
                {
                    PreviousGameState = _gameState;
                    _gameState = value;
                    HandleGameStateChange();
                    OnGameStateChanged?.Invoke(value);
                }
            }
        }
        private ParkitectState _gameState;

        public ParkitectState PreviousGameState { get; private set; }

        private HashSet<Type> AllowedBuilderTypes = new HashSet<Type>()
        {
            typeof(DecoBuilder),
            typeof(FlatRideBuilder)
        };

        public override void OnChangeApplied()
        {
            RA.RAActiveHotkey.onKeyDown += ToggleRAActive;
        }

        public override void OnModStart()
        {
            HandleActiveStateChange();
            HandleGameStateChange();
        }

        public override void OnChangeReverted() { }

        public void NotifyBuildState(bool building, Builder builder)
        {
            // If builder has been opened
            if (building)
            {
                // If this builder is one of the allowed builder types
                if (AllowedBuilderTypes.Contains(builder.GetType()))
                {
                    GameState = ParkitectState.Placement;
                    ModBase.LOG("Building");
                }
            }
            else
            {
                ModBase.LOG("Not Building");
                GameState = ParkitectState.None;
            }
        }

        public void ToggleRAActive()
        {
            SetRAActive(!Active);
        }

        public void SetRAActive(bool state)
        {
            if (state != Active)
            {
                Active = state;
                HandleActiveStateChange();
                OnActiveChanged?.Invoke(Active);
            }
        }

        private void HandleActiveStateChange()
        {
            if(Active)
            {
                RAWindowButton.Instance.SetButtonEnabled(true);
                HandleGameStateChange();
            }
            else
            {
                RAWindowButton.Instance.SetButtonEnabled(false);
                RAWindowButton.Instance.SetWindowOpened(false);
            }
        }

        private void HandleGameStateChange()
        {
            if (Active)
            {
                if (GameState == ParkitectState.Placement)
                {
                    if (!IsWindowOpened)
                    {
                        RAWindowButton.Instance.SetWindowOpened(true);
                    }
                }
            }
        }
    }
}