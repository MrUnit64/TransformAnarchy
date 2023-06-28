using System;
using System.Collections.Generic;
using Parkitect;

namespace RotationAnarchyEvolved
{
    public class KeybindManager
    {

        public class Keybind
        {
            public string id;
            public string name;
            public string description;
            public readonly KeyMapping mapping;

            public Keybind(string id, string name, string keyGroupID, UnityEngine.KeyCode key, string description)
            {
                this.id = id;
                this.name = name;
                this.description = description;
                mapping = new KeyMapping(id, key, UnityEngine.KeyCode.None);
                mapping.keyName = this.name;
                mapping.keyDescription = this.description;
                mapping.keyGroupIdentifier = keyGroupID;
            }
        }

        List<Keybind> _allRegistered = new List<Keybind>();
        KeyGroup _group;

        string _name;
        string _id;

        public KeybindManager(string id, string name)
        {
            _group = new KeyGroup(id);
            _id = id;
            _name = name;
            _group.keyGroupName = _name;
        }

        public void AddKeybind(string id, string name, string description, UnityEngine.KeyCode defaultKey)
        {
            _allRegistered.Add(new Keybind(id, name, _id, defaultKey, description));
        }

        public void ClearAllKeybinds()
        {
            _allRegistered.Clear();
        }

        public void RegisterAll()
        {
            // register keygroup
            InputManager.Instance.registerKeyGroup(_group);

            // register mappings
            foreach (Keybind bind in _allRegistered)
            {
                InputManager.Instance.registerKeyMapping(bind.mapping);
            }
        }

        public void UnregisterAll()
        {
            // unregister keygroup
            InputManager.Instance.unregisterKeyGroup(_group);

            // register mappings
            foreach (Keybind bind in _allRegistered)
            {
                InputManager.Instance.unregisterKeyMapping(bind.mapping);
            }
        }
    }
}
