using System;

namespace RotationAnarchy 
{

    using UnityEngine;
    using UnityEngine.UI;
    using Parkitect.UI;
    using TMPro;
    using RotationAnarchy.Internal;

    public class RAWorldSpaceText : ModComponent
    {

        // World Space texts
        private UIWorldSpaceText LocalSpaceText;

        public static string[] worldAxesNames = new string[]
        {
            "Global",
            "Local"
        };

        // Named in order of X, Y, Z
        private static string[] directionAxesNames = new string[]
{
            "Pitch",    // X
            "Yaw",      // Y
            "Roll"      // Z
        };

        public override void OnApplied()
        {
            LocalSpaceText = GameObject.Instantiate(UIAssetManager.Instance.uiWorldSpaceTextGO, UIWorldSpaceController.Instance.transform);
            LocalSpaceText.text.color = Color.white;
            LocalSpaceText.text.fontSize = 18;
            LocalSpaceText.text.parseCtrlCharacters = true;
        }

        public override void OnReverted()
        { 
            GameObject.Destroy(LocalSpaceText);
        }

        public override void OnUpdate()
        {
            if (RA.Controller.ActiveGhost && RA.Controller.ActiveBuilder && RA.Controller.GameState != ParkitectState.None)
            {
                var Ghost = RA.Controller.ActiveGhost;

                // Copied code from BuilderHeightMarker
                LandPatch terrain = GameController.Instance.park.getTerrain(Ghost.transform.position);
                Vector3 vector = Ghost.transform.position;
                if (terrain != null)
                {
                    vector = terrain.getPoint(Ghost.transform.position);
                }

                LocalSpaceText.transform.up = Vector3.forward;

                // this is shit but like what can we do
                Vector3 forwardVec = Camera.main.transform.forward;
                forwardVec.y = 0;
                forwardVec.Normalize();

                LocalSpaceText.transform.position = vector - forwardVec * 1.5f;

                var rotationSpaceText = RA.Controller.IsLocalRotation ? "Local" : "Global";
                var axesText = GetAxisText();
                var angleText = GetAngleText();

                LocalSpaceText.text.text = $"{rotationSpaceText}\n{axesText}\n{angleText}";
                LocalSpaceText.text.alignment = TextAlignmentOptions.TopLeft;

                LocalSpaceText.gameObject.SetActive(true);

            }
            else
            {
                LocalSpaceText.gameObject.SetActive(false);
            }
        }

        private static string GetAngleText()
        {
            switch (RA.Controller.GameState)
            {
                case ParkitectState.Trackball when RA.TrackballController.AngleAmount != null:
                    return $"{RA.TrackballController.AngleAmount:0.00}°";
                case ParkitectState.Trackball:
                case ParkitectState.None:
                case ParkitectState.Placement:
                    return "";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string GetAxisText()
        {
            switch (RA.Controller.GameState)
            {
                case ParkitectState.None:
                    return "";
                case ParkitectState.Placement:
                    return directionAxesNames[(int)RA.Controller.CurrentRotationAxis];
                case ParkitectState.Trackball when RA.TrackballController.SelectedAxis != null:
                    return directionAxesNames[(int)RA.TrackballController.SelectedAxis];
                case ParkitectState.Trackball when RA.TrackballController.SelectedAxis == null:
                    return "Free Axis";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}