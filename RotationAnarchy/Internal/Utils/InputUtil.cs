namespace RotationAnarchy.Internal
{
    public class BaseHotkey : Hotkey
    {
        public BaseHotkey(KeyMapping mapping) : base(mapping)
        {
        }

        public bool Down => InputManager.getKeyDown(keyMapping.keyIdentifier);
        public bool Pressed => InputManager.getKey(keyMapping.keyIdentifier);
        public bool Up => InputManager.getKeyUp(keyMapping.keyIdentifier);
    }

}