namespace RotationAnarchy
{
    using Parkitect.UI;
    using RotationAnarchy.Internal;
    using UnityEngine;
    using UnityEngine.UI;

    public class ConstructWindowToggle : ModChange
    {
        private GameObject menuCanvasRoot;
        private GameObject raButtonGo;

        public override void OnApplied()
        {
            menuCanvasRoot = GameObject.Find("MenuCanvas");
            var painterButton = menuCanvasRoot.transform.Find("BottomMenu/Painter").gameObject;
            //Disable the painter button before cloning, so we can initialize it before its Awake is called
            painterButton.SetActive(false);

            raButtonGo = GameObject.Instantiate(painterButton, painterButton.transform.parent);
            raButtonGo.name = RA.Instance.getName();

            //Features setup
            GameObject.Destroy(raButtonGo.GetComponent<UIMenuWindowButton>());
            var raWindowButton = raButtonGo.AddComponent<RAWindowButton>();
            //at this point we expect the hotkey to be already registered
            raWindowButton.hotkeyIdentifier = RA.RAActiveHotkey.Identifier;
            var toggle = raButtonGo.GetComponent<Toggle>();
            UIUtil.RemoveListeners(toggle);
            toggle.onValueChanged.AddListener( x => raWindowButton.onChanged() );

            painterButton.SetActive(true);
            raButtonGo.SetActive(true);
        }

        public override void OnReverted()
        {
            GameObject.Destroy(raButtonGo);
        }
    }
}