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

        public override void OnApplied()
        {
            LocalSpaceText = GameObject.Instantiate(UIAssetManager.Instance.uiWorldSpaceTextGO, UIWorldSpaceController.Instance.transform);
            LocalSpaceText.text.color = Color.white;
            LocalSpaceText.text.fontSize = 48f;
        }

        public override void OnReverted()
        { 
            GameObject.Destroy(LocalSpaceText);
        }

        public override void OnUpdate()
        {
            if (RA.Controller.ActiveGhost && RA.Controller.ActiveBuilder && RA.Controller.GameState == ParkitectState.Placement)
            {
                var Ghost = RA.Controller.ActiveGhost;

                LocalSpaceText.transform.position = Ghost.transform.position;
                //LocalSpaceText.rectTransform.ForceUpdateRectTransforms();
                LocalSpaceText.transform.up = Vector3.up;

                LocalSpaceText.text.text = (RA.LocalRotationHotkey.Pressed) ? "Local" : "Global";

                LocalSpaceText.gameObject.SetActive(true);

            }
            else
            {
                LocalSpaceText.gameObject.SetActive(false);
            }
        }
    }
}