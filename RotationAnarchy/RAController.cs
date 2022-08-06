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

    public class RAController : ModComponent
    {
        public event Action<bool> OnActiveChanged;
        public event Action<ParkitectState> OnGameStateChanged;

        public bool IsWindowOpened => RAWindow.Instance != null;
        public Builder ActiveBuilder { get; private set; }
        public GameObject ActiveGhost { get; private set; }

        public bool Active
        {
            get => _active;
            set
            {
                if (_active != value)
                {
                    _active = value;
                    HandleActiveStateChange();
                    OnActiveChanged?.Invoke(Active);
                }
            }
        }
        private bool _active;

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
        private ParkitectState _gameState = ParkitectState.None;
        public ParkitectState PreviousGameState { get; private set; }

        private HashSet<Type> AllowedBuilderTypes = new HashSet<Type>()
        {
            typeof(DecoBuilder),
            typeof(FlatRideBuilder)
        };

        public override void OnApplied()
        {
            RA.RAActiveHotkey.onKeyDown += ToggleRAActive;
        }

        public override void OnStart()
        {
            Active = RA.ActiveOnLoad.Value;
        }

        public override void OnReverted() { }

        public void NotifyBuildState(bool building, Builder builder)
        {
            if (builder == null)
                return;

            // If this builder is one of the allowed builder types
            if (AllowedBuilderTypes.Contains(builder.GetType()))
            {
                // If builder has been opened
                if (building)
                {
                    ActiveBuilder = builder;
                    GameState = ParkitectState.Placement;
                    ModBase.LOG($"Building  {builder.GetType()} : {builder.name}");
                }
                else
                {
                    ActiveBuilder = null;
                    GameState = ParkitectState.None;
                    ModBase.LOG($"Not building  {builder.GetType()} : {builder.name}");
                }
            }
        }

        public void NotifyGhost(GameObject ghost)
        {
            if(ActiveBuilder)
            {
                ActiveGhost = ghost;
            }
            else
            {
                ActiveGhost = null;
            }
        }

        public void ToggleRAActive()
        {
            Active = !Active;
        }

        private void HandleActiveStateChange()
        {
            ModBase.LOG("HandleActiveStateChange = " + Active);
            if (Active)
            {
                RAWindowButton.Instance.SetButtonEnabled(true);
                HandleGameStateChange();
            }
            else
            {
                RAWindowButton.Instance.SetButtonEnabled(false);
                if (IsWindowOpened)
                    RAWindowButton.Instance.SetWindowOpened(false);
            }
        }

        private void HandleGameStateChange()
        {
            ModBase.LOG("HandleGameStateChange = " + GameState);

            if (ShouldWindowBeOpened())
            {
                if (!IsWindowOpened)
                {
                    RAWindowButton.Instance.SetWindowOpened(true);
                }
            }
        }

        private bool ShouldWindowBeOpened()
        {
            return Active && GameState == ParkitectState.Placement;
        }
    }
}