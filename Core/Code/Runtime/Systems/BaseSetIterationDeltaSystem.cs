﻿using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

namespace UFlow.Addon.ECS.Core.Runtime {
    [Preserve]
    public abstract class BaseSetIterationDeltaSystem : IPreSetupSystem, 
                                                        ISetupSystem, 
                                                        IRunDeltaSystem, 
                                                        IPreCleanupSystem, 
                                                        ICleanupSystem, 
                                                        IResetSystem,
                                                        IEnableDisableSystem {
        private readonly World m_world;
        private bool m_enabled;

        protected DynamicEntitySet Query { get; }
        protected EntityCommandBuffer CommandBuffer { get; }
        
        public BaseSetIterationDeltaSystem(in World world, QueryBuilder query) {
            m_world = world;
            Query = query.AsSet();
            CommandBuffer = new EntityCommandBuffer();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PreSetup() => PreSetup(m_world);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Setup() => Setup(m_world);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PreRun(float delta) => PreRun(m_world, delta);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run(float delta) {
            PreIterate(m_world, delta);
            foreach (var entity in Query)
                IterateEntity(m_world, entity, delta);
            ExecuteCommandBuffers();
            PostIterate(m_world, delta);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PostRun(float delta) => PostRun(m_world, delta);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PreCleanup() => PreCleanup(m_world);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Cleanup() {
            Cleanup(m_world);
            CommandBuffer.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => Reset(m_world);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetEnabled(bool value) => m_enabled = value;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enable() => m_enabled = true;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Disable() => m_enabled = false;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEnabled() => m_enabled;
        
        internal virtual void ExecuteCommandBuffers() => CommandBuffer.ExecuteCommands();

        protected virtual void PreSetup(World world) { }
        
        protected virtual void Setup(World world) { }
        
        protected virtual void PreRun(World world, float delta) { }

        protected virtual void PreIterate(World world, float delta) { }

        protected virtual void IterateEntity(World world, in Entity entity, float delta) { }

        protected virtual void PostIterate(World world, float delta) { }
        
        protected virtual void PostRun(World world, float delta) { }
        
        protected virtual void PreCleanup(World world) { }
        
        protected virtual void Cleanup(World world) { }

        protected virtual void Reset(World world) { }
    }
}