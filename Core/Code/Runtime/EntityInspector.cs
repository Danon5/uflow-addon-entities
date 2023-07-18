﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UFlow.Odin.Runtime;
using UnityEngine;

[assembly: InternalsVisibleTo("UFlow.Addon.Ecs.Core.Editor")]

namespace UFlow.Addon.Ecs.Core.Runtime {
    [Serializable]
    internal sealed class EntityInspector
#if UNITY_EDITOR
        : ISerializationCallbackReceiver
#endif

    {
#if UNITY_EDITOR
        [ColoredBoxGroup("Entity", "$Color", GroupName = "$m_entity")]
        [ToggleLeft]
#endif
        [SerializeField]
        private bool m_enabled = true;
        
#if UNITY_EDITOR
        [ColoredFoldoutGroup("ComponentAuthoring", "$Color", GroupName = "Components")]
        [HideLabel, LabelText("Authoring")]
        [ListDrawerSettings(ShowFoldout = false)]
        [HideIf(nameof(ShouldDisplayRuntime))]
#endif
        [SerializeField]
        private List<EntityComponent> m_authoring = new();

#if UNITY_EDITOR
        [ColoredFoldoutGroup("ComponentRuntime", "$Color", GroupName = "Components")]
        [ShowInInspector, HideLabel, LabelText("Runtime")]
        [ListDrawerSettings(ShowFoldout = false, CustomAddFunction = nameof(Add))]
        [OnCollectionChanged(nameof(ApplyRuntimeState))]
        [ShowIf(nameof(ShouldDisplayRuntime))]
        private List<EntityComponent> m_runtime = new();
#endif

        private Entity m_entity;
        private World m_world;
#if UNITY_EDITOR
        private Dictionary<Type, EntityComponent> m_typeMap;
        private Queue<Type> m_typesToSet;
        private Queue<Type> m_typesToRemove;

        internal bool EntityEnabled => m_enabled;
        private bool ShouldDisplayRuntime => Application.isPlaying && m_entity.IsAlive();
        [UsedImplicitly] private Color AuthoringColor => new(.25f, .75f, 1f, 1f);
        [UsedImplicitly] private Color RuntimeColor => new(1f, .25f, 0f, 1f);
        [UsedImplicitly] private Color Color => ShouldDisplayRuntime ? 
            m_enabled ? RuntimeColor : GetDisabledColor(RuntimeColor) : 
            m_enabled ? AuthoringColor : GetDisabledColor(AuthoringColor);
        [UsedImplicitly] private Color DisabledColor => GetDisabledColor(Color);
        
        public void OnBeforeSerialize() {
            foreach (var component in m_authoring)
                component.inspector = this;
            foreach (var component in m_runtime)
                component.inspector = this;
        }

        public void OnAfterDeserialize() {
            foreach (var component in m_authoring)
                component.inspector = this;
            foreach (var component in m_runtime)
                component.inspector = this;
        }
#endif

        public void BakeAuthoringComponents(in Entity entity) {
            m_entity = entity;
            m_world = entity.World;
#if UNITY_EDITOR
            m_typeMap = new Dictionary<Type, EntityComponent>();
            m_typesToSet = new Queue<Type>();
            m_typesToRemove = new Queue<Type>();
#endif
            foreach (var component in m_authoring)
                entity.SetRaw(component.value, component.enabled);
        }

#if UNITY_EDITOR
        public void RetrieveRuntimeState() {
            if (!m_entity.IsAlive()) return;
            if (m_world == null) return;

            m_enabled = m_entity.IsEnabled();

            var componentTypes = m_world.GetEntityComponentTypes(m_entity);

            // enqueue removes
            foreach (var (type, component) in m_typeMap) {
                if (component.value == null) continue;
                if (!componentTypes.Contains(type))
                    m_typesToRemove.Enqueue(type);
            }

            // apply removes
            while (m_typesToRemove.TryDequeue(out var type)) {
                m_runtime.Remove(m_typeMap[type]);
                m_typeMap.Remove(type);
            }

            // enqueue sets
            foreach (var type in componentTypes)
                m_typesToSet.Enqueue(type);

            // apply sets
            while (m_typesToSet.TryDequeue(out var type)) {
                var componentValue = m_entity.GetRaw(type);
                if (!m_typeMap.ContainsKey(type)) {
                    var component = new EntityComponent(this, componentValue);
                    m_typeMap.Add(type, component);
                    m_runtime.Add(component);
                }
                else
                    m_typeMap[type].value = componentValue;
            }
        }

        public void ApplyRuntimeState() {
            if (!m_entity.IsAlive()) return;
            if (m_world == null) return;
            
            m_entity.SetEnabled(m_enabled);

            // enqueue removes
            foreach (var (type, component) in m_typeMap) {
                if (component.value == null || !m_runtime.Contains(component) || component.value.GetType() != type)
                    m_typesToRemove.Enqueue(type);
            }

            // apply removes
            while (m_typesToRemove.TryDequeue(out var type)) {
                m_typeMap.Remove(type);
                m_entity.RemoveRaw(type);
            }

            // enqueue sets
            foreach (var component in m_runtime) {
                if (component.value == null) continue;
                var type = component.value.GetType();
                if (!m_typeMap.ContainsKey(type))
                    m_typeMap.Add(type, component);
                m_typesToSet.Enqueue(type);
            }

            // apply sets
            while (m_typesToSet.TryDequeue(out var type)) {
                var component = m_typeMap[type];
                m_entity.SetRaw(type, component.value, component.enabled);
                m_entity.SetEnabledRaw(type, component.enabled);
            }
        }
        
        private void Add() => m_runtime.Add(new EntityComponent(this, default));

        private void SetComponentEnabled(in Type type, bool enabled) => m_entity.SetEnabledRaw(type, enabled);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Color GetDisabledColor(in Color color) {
            var col = color;
            var a = col.a;
            col /= 2f;
            col.a = a;
            return col;
        }
#endif

        [Serializable]
        [HideReferenceObjectPicker]
        internal sealed class EntityComponent {
#if UNITY_EDITOR
            [ColoredFoldoutGroup("Default", "$Color", GroupName = "$Name")] [ToggleLeft]
#endif
            public bool enabled;
            
#if UNITY_EDITOR
            [ColoredFoldoutGroup("Default", "$Color", GroupName = "$Name")]
            [ColoredBoxGroup("Default/Box", "$Color", GroupName = "Data")]
            [InlineProperty, HideLabel]
#endif
            [SerializeReference]
            public IEcsComponent value;

#if UNITY_EDITOR
            [NonSerialized] public EntityInspector inspector;

            [UsedImplicitly] private string Name => value != null ? value.GetType().Name : "None";
            [UsedImplicitly] private Color Color => (enabled && inspector.EntityEnabled) || !inspector.EntityEnabled ? 
                inspector.Color : inspector.DisabledColor;
#endif

            public EntityComponent() {
                enabled = true;
            }

#if UNITY_EDITOR
            public EntityComponent(in EntityInspector inspector, in IEcsComponent value) {
                this.inspector = inspector;
                this.value = value;
                enabled = true;
            }
#endif
        }
    }
}