namespace RotationAnarchy
{
    using HarmonyLib;
    using Parkitect.UI;
    using RotationAnarchy.Internal;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public enum ParkitectState
    {
        None,
        Placement,
    }

    public class RAController : ModComponent
    {
        public event Action<bool> OnActiveChanged;
        public event Action<ParkitectState> OnGameStateChanged;

        public bool IsWindowOpened => RAWindow.Instance != null;

        public BuildableObject SelectedBuildable { get; private set; }
        public Builder ActiveBuilder { get; private set; }
        public GameObject ActiveGhost { get; private set; }
        public float CurrentZoom { get; private set; }
        public bool IsPickingObject { get; private set; }
        public bool GizmoActive { get; private set; }

        /// <summary>
        /// Also acts as "invert" rotation direction.
        /// </summary>
        public bool HoldingChangeHeightKey => InputManager.getKey("BuildingChangeHeight");

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

        public bool IsLocalRotation { get; private set; }

        public bool IsDirectionHorizontal { get; private set; }

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

        private RAPipetteTool pipette;
        private HighlightOverlayController.HighlightHandle selectedBuildableHighlightHandle;
        private ChunkedMesh[] selectedBuildableChunkedMeshes;


        private HashSet<Type> AllowedBuilderTypes = new HashSet<Type>()
        {
            typeof(DecoBuilder),
            typeof(FlatRideBuilder)
        };

        public override void OnApplied()
        {
            pipette = new RAPipetteTool();
            pipette.OnDisabled += OnPipetteDisabled;
            pipette.OnObjectSelected += OnObjectPicked;
            RA.RAActiveHotkey.onKeyDown += ToggleRAActive;
            RA.LocalRotationHotkey.onKeyDown += ToggleLocalRotationActive;
            RA.DirectionHotkey.onKeyDown += ToggleDirection;
            RA.SelectObjectHotkey.onKeyDown += StartPickingObject;
        }

        public override void OnStart()
        {
            Active = RA.ActiveOnLoad.Value;
        }

        public override void OnReverted() { }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (Active)
            {
                DebugGUI.DrawValue("builder.getBuiltObject", ActiveBuilder?.getBuiltObject());
                if (IsPickingObject)
                    pipette.tick();

                UpdateBuildableTransformation();
            }
        }

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
            ActiveGhost = ActiveBuilder ? ghost : null;
        }

        public void NotifyCameraControllerCurrentZoom(float currentZoom)
        {
            CurrentZoom = currentZoom;
        }

        public void ToggleRAActive()
        {
            Active = !Active;
        }

        public void ToggleLocalRotationActive()
        {
            IsLocalRotation = !IsLocalRotation;
        }

        public void ToggleDirection()
        {
            IsDirectionHorizontal = !IsDirectionHorizontal;
        }

        public void NotifyWindowState(bool opened)
        {
            if(!opened)
            {
                SetGizmoActive(false);
            }
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
                    CloseWindow();
                if (IsPickingObject)
                    StopPickingObject();
                SetGizmoActive(false);
            }
        }

        private void HandleGameStateChange()
        {
            ModBase.LOG("HandleGameStateChange = " + GameState);

            if (ShouldWindowBeOpened())
            {
                if (!IsWindowOpened)
                {
                    OpenWindow();
                }
            }

            if (GameState == ParkitectState.Placement && IsPickingObject)
            {
                StopPickingObject();
                SetGizmoActive(false);
            }
        }


        private bool ShouldWindowBeOpened()
        {
            return Active || GameState == ParkitectState.Placement;
        }

        private void OpenWindow()
        {
            RAWindowButton.Instance.SetWindowOpened(true);
        }

        private void CloseWindow()
        {
            RAWindowButton.Instance.SetWindowOpened(false);
            //RAWindow.Instance.windowFrame.close();
        }

        private void StartPickingObject()
        {
            if (!IsPickingObject && GameState == ParkitectState.None)
            {
                if (!IsWindowOpened)
                    OpenWindow();

                GameController.Instance.enableMouseTool(this.pipette);
                IsPickingObject = true;
            }
        }

        private void StopPickingObject()
        {
            if (IsPickingObject)
            {
                GameController.Instance.removeMouseTool(this.pipette);
            }
        }

        private void OnPipetteDisabled()
        {
            IsPickingObject = false;
            GameController.Instance.removeMouseTool(this.pipette);
        }

        private void OnObjectPicked(BuildableObject buildableObject)
        {
            if(SelectedBuildable)
            {
                FinalizeTransformingBuildable();
                if (selectedBuildableHighlightHandle != null)
                {
                    SelectedBuildable = null;
                    selectedBuildableHighlightHandle.remove();
                    selectedBuildableHighlightHandle = null;
                }
            }

            SelectedBuildable = buildableObject;
            if (SelectedBuildable)
            {
                selectedBuildableChunkedMeshes = SelectedBuildable.GetComponentsInChildren<ChunkedMesh>();
                selectedBuildableHighlightHandle = HighlightOverlayController.Instance.add(SelectedBuildable.getRenderersToHighlight(),
                        fixedCustomColor: RA.SelectedBuildableHighlightColor);
                SetGizmoActive(true);
                StopPickingObject();
            }
        }

        private void SetGizmoActive(bool state)
        {
            GizmoActive = state;
            if (state)
            {
            }
            else
            {
                if(SelectedBuildable)
                {
                    OnObjectPicked(null);
                }
            }
        }

        private void FinalizeTransformingBuildable()
        {
            if(SelectedBuildable)
            {
                var deco = SelectedBuildable as Deco;
                if(deco)
                {
                    Traverse.Create(deco).Method("onMovedByTerraforming", SelectedBuildable.transform.position).GetValue();
                }
            }
        }

        private void UpdateBuildableTransformation()
        {
            if (SelectedBuildable && selectedBuildableChunkedMeshes != null)
            {
                foreach (var chunkedMesh in selectedBuildableChunkedMeshes)
                {
                    chunkedMesh.updateTransform();
                }
            }
        }
    }
}