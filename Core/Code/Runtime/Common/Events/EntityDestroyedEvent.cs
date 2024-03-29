﻿namespace UFlow.Addon.Entities.Core.Runtime {
    internal readonly struct EntityDestroyedEvent {
        public readonly Entity entity;

        public EntityDestroyedEvent(in Entity entity) {
            this.entity = entity;
        }
    }

    public delegate void EntityDestroyedHandler(in Entity entity);
}