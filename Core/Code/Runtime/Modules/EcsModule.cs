﻿using UFlow.Core.Runtime;
// ReSharper disable Unity.PerformanceCriticalCodeInvocation

namespace UFlow.Addon.ECS.Core.Runtime {
    public sealed class EcsModule<T> : BaseBehaviourModule<EcsModule<T>> {
        public World World { get; private set; }
        
        public override void LoadDirect() {
            World = EcsUtils.Worlds.CreateWorldFromType<DefaultWorld>();
            World.SetupSystemGroups();
            World.WhenReset(() => World.ResetSystemGroups());
        }

        public override void UnloadDirect() {
            World.Destroy();
        }

        public override void Update() {
            World.RunSystemGroup<FrameSimulationSystemGroup>();
            World.RunSystemGroup<FrameRenderSystemGroup>();
        }

        public override void FixedUpdate() {
            World.RunSystemGroup<FixedSimulationSystemGroup>();
            World.RunSystemGroup<FixedRenderSystemGroup>();
        }

        public override void LateUpdate() {
            World.RunSystemGroup<LateFrameSimulationSystemGroup>();
            World.RunSystemGroup<LateFrameRenderSystemGroup>();
        }

        public override void OnDrawGizmos() => World.RunSystemGroup<GizmoSystemGroup>();

        public override void OnGUI() => World.RunSystemGroup<GUISystemGroup>();
    }
}