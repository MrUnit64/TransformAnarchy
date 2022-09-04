namespace RotationAnarchy.Internal
{
    using UnityEngine;

    /// <summary>
    /// Starts inactive
    /// </summary>
    public abstract class GameObjectWrapper
    {
        public bool Active
        {
            get => gameObject.activeInHierarchy;
            set
            {
                gameObject.SetActive(value);
            }
        }

        public GameObject gameObject { get; private set; }
        public Transform transform => gameObject.transform;
        
        private bool _lastActiveState;

        /// <summary>
        /// Do not do anything in the constructor
        /// </summary>
        /// <param name="name"></param>
        public GameObjectWrapper(string name = null)
        {
            gameObject = new GameObject(name ?? "Gizmo");
            OnConstruct();
        }

        /// <summary>
        /// Because constructors suck, the go down the chain, then up the chain, so yeah
        /// </summary>
        protected virtual void OnConstruct() { }
        protected virtual void OnEnabled() { }
        protected virtual void OnDisabled() { }
        protected virtual void OnUpdate() { }

        public virtual void Update()
        {
            if (_lastActiveState != Active)
            {
                if (Active)
                    OnEnabled();
                else
                    OnDisabled();

                _lastActiveState = Active;
            }

            OnUpdate();
        }


        public virtual void Destroy()
        {
            GameObject.Destroy(gameObject);
        }
    }
}