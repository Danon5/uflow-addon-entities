﻿namespace UFlow.Addon.Entities.Core.Runtime {
    internal readonly struct EnsureAddedAndDisabledCommand<T> : IEcsCommand where T : IEcsComponentData {
        public void Execute(in Entity entity) => entity.EnsureAddedAndDisabled<T>();
    }
}