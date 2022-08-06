namespace RotationAnarchy.Internal
{
    /// <summary>
    /// Base class for abstracting mod changes that need to be reverted.
    /// </summary>
    public abstract class ModChange
    {
        /// <summary>
        /// The mod base this change belongs to.
        /// </summary>
        protected ModBase ModBase { get; private set; }

        /// <summary>
        /// Called internally by ModBase
        /// </summary>
        /// <param name="_base"></param>
        public void InjectDependencies(ModBase _base)
        {
            this.ModBase = _base;
        }

        /// <summary>
        /// Similar to unity's Awake method, here we only create/construct internals
        /// </summary>
        public abstract void OnApplied();
        /// <summary>
        /// Similar to unity's Start, here we can reference other changes
        /// </summary>
        public virtual void OnStart() { }

        public virtual void OnUpdate() { }

        public virtual void OnLateUpdate() { }

        public virtual void OnFixedUpdate() { }
        /// <summary>
        /// Here we revert anything this change did, destroy objects, revert ui changes etc.
        /// </summary>
        public abstract void OnReverted();

        /// <summary>
        /// Get other mod change in the same ModBase
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected T GetChange<T>() where T : ModChange
        {
            return ModBase.GetChange<T>();
        }
    }
}