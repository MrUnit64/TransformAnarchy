namespace RotationAnarchy.Internal
{
    public class BaseHotkey : Hotkey
    {
        public BaseHotkey(KeyMapping mapping) : base(mapping)
        {
        }

        public string Identifier => keyMapping.keyIdentifier;

        public bool Down => InputManager.getKeyDown(Identifier);
        public bool Pressed => InputManager.getKey(Identifier);
        public bool Up => InputManager.getKeyUp(Identifier);
    }

}