namespace RotationAnarchy.Internal
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Base class for abstracting mod changes that need to be reverted.
    /// </summary>
    public abstract class ModChange
    {
        protected ModBase ModBase { get; private set; }

        public void Initialize(ModBase _base)
        {
            this.ModBase = _base;
        }


        public abstract void OnChangeApplied();
        public abstract void OnChangeReverted();

        protected T GetChange<T>() where T : ModChange
        {
            return ModBase.GetChange<T>();
        }
    }
}