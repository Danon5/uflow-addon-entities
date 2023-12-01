﻿namespace UFlow.Addon.ECS.Core.Runtime {
    public interface IRawComponentMethod {
        void InvokeSet(in Entity entity, IEcsComponent value, bool enableIfAdded);
        IEcsComponent InvokeGet(in Entity entity);
        bool InvokeHas(in Entity entity);
        void InvokeRemove(in Entity entity);
        bool InvokeTryRemove(in Entity entity);
        void InvokeSetEnabled(in Entity entity, bool enabled);
        bool InvokeIsEnabled(in Entity entity);
    }
}