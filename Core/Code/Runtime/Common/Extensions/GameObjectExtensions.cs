﻿using System.Runtime.CompilerServices;
using UnityEngine;

namespace UFlow.Addon.Entities.Core.Runtime {
    public static class GameObjectExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity AsEntity(this GameObject gameObject) {
            if (!gameObject.TryGetComponent(out SceneEntity sceneEntity))
                sceneEntity = gameObject.AddComponent<SceneEntity>();
            return sceneEntity.Entity.IsAlive() ? sceneEntity.Entity : sceneEntity.CreateEntity();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Entity AsEntityWithIdAndGen(this GameObject gameObject, int id, ushort gen) {
            if (!gameObject.TryGetComponent(out SceneEntity sceneEntity))
                sceneEntity = gameObject.AddComponent<SceneEntity>();
            return sceneEntity.CreateEntityWithIdAndGen(id, gen);
        }
    }
}