namespace RotationAnarchy.Internal
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Base class for abstracting mod changes that need to be reverted.
    /// </summary>
    public abstract class ModChange
    {
        public abstract void OnChangeApplied();
        public abstract void OnChangeReverted();
    }
}