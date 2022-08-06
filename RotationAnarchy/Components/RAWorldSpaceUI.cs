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

                LocalSpaceText.text.text = RA.Controller.IsLocalRotation ? "Local" : "Global";

                LocalSpaceText.gameObject.SetActive(true);

            }
            else
            {
                LocalSpaceText.gameObject.SetActive(false);
            }
        }
    }
}