﻿using System.Runtime.CompilerServices;

namespace UFlow.Addon.Ecs.Core.Runtime {
    public abstract class BaseSetIterationDeltaSystem : IPreSetupSystem, ISetupSystem, IRunDeltaSystem, IPreCleanupSystem, ICleanupSystem {
        private readonly World m_world;
        private readonly DynamicEntitySet m_query;

        public BaseSetIterationDeltaSystem(in World world, QueryBuilder query) {
            m_world = world;
            m_query = query.AsSet();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PreSetup() => PreSetup(m_world);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Setup() {
            Setup(m_world);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PreRun(float delta) => PreIterate(m_world, delta);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run(float delta) {
            foreach (var entity in m_query)
                IterateEntity(m_world, entity, delta);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PostRun(float delta) => PostIterate(m_world, delta);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PreCleanup() => PreCleanup(m_world);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Cleanup() => Cleanup(m_world);

        protected virtual void PreSetup(World world) { }
        protected virtual void Setup(World world) { }

        protected virtual void PreIterate(World world, float delta) { }

        protected virtual void IterateEntity(World world, in Entity entity, float delta) { }

        protected virtual void PostIterate(World world, float delta) { }
        
        protected virtual void PreCleanup(World world) { }
        protected virtual void Cleanup(World world) { }
    }
}