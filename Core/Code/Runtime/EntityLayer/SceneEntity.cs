﻿using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UFlow.Core.Runtime;
using UFlow.Odin.Runtime;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

namespace UFlow.Addon.Entities.Core.Runtime {
    public class SceneEntity : MonoBehaviour, ISceneEntity, ISerializationCallbackReceiver {
#if UNITY_EDITOR
        [InlineProperty, HideLabel]
#endif
        [SerializeField] private EntityInspector m_inspector;
#if UNITY_EDITOR
        [HideInInspector]
#endif
        [SerializeField] private bool m_isValidPrefab;
#if UNITY_EDITOR
        [ColoredBoxGroup("Serialization", Color = nameof(Color))]
#endif
        [SerializeField] private string m_guid;
        private bool m_destroying;
        private bool m_destroyingDirectly;
        private bool m_initialized;
        private IDisposable m_destroyedSubscription;
        private IDisposable m_worldDestroyedSubscription;
#if UNITY_EDITOR
        private bool m_instantiated;
        private bool m_requiresRuntimeRetrieval;
#endif
        
        public World World { get; private set; }
        public Entity Entity { get; protected set; }
        public GameObject GameObject => gameObject;
        internal string Guid => m_guid;
        internal bool EnabledInInspector => m_inspector.EntityEnabled;
        internal bool IsValidPrefab => m_isValidPrefab;
#if UNITY_EDITOR
        internal bool IsPlaying => Application.isPlaying && m_instantiated;
        internal bool IsDirty => m_inspector.IsDirty;
        private Color Color => m_inspector.Color;
#endif

        [UsedImplicitly]
        protected virtual void Awake() => Initialize();

        [UsedImplicitly]
        protected virtual void Start() {
            if (m_initialized) return;
            Initialize();
        }

        [UsedImplicitly]
        protected virtual void Update() {
            if (m_initialized) return;
            Initialize();
        }
        
        [UsedImplicitly]
        protected virtual void OnDestroy() {
            m_destroyedSubscription?.Dispose();
            m_worldDestroyedSubscription?.Dispose();
            m_destroying = true;
            if (m_destroyingDirectly) return;
            if (World == null) return;
            if (!World.IsAlive()) return;
            if (!Entity.IsAlive()) return;
            Entity.Destroy();
        }

        [UsedImplicitly]
        protected virtual void OnEnable() {
            if (World == null) return;
            if (!World.IsAlive()) return;
            if (!Entity.IsAlive()) return;
            Entity.Enable();
        }

        [UsedImplicitly]
        protected virtual void OnDisable() {
            if (World == null) return;
            if (!World.IsAlive()) return;
            if (!Entity.IsAlive()) return;
            Entity.Disable();
        }
        
#if UNITY_EDITOR
        [UsedImplicitly]
        private void OnValidate() {
            if (Application.isPlaying) return;
            m_isValidPrefab = PrefabUtility.GetPrefabAssetType(gameObject) is not
                PrefabAssetType.NotAPrefab or PrefabAssetType.MissingAsset || PrefabStageUtility.GetPrefabStage(gameObject);
        }
#endif

        public void OnBeforeSerialize() {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(m_guid))
                m_guid = System.Guid.NewGuid().ToString();
#endif
        }
        
        public void OnAfterDeserialize() { }

        public virtual Entity CreateEntity() {
            if (World == null)
                throw new Exception("Attempting to create a SceneEntity with no valid world.");
            if (Entity.IsAlive())
                throw new Exception("Attempting to create a SceneEntity multiple times.");
            Entity = World.CreateEntity(m_inspector.EntityEnabled);
            AddSpecialComponentsBeforeBaking();
            BakeAuthoringComponents();
            gameObject.SetActive(Entity.IsEnabled());
            if (!m_isValidPrefab) return Entity;
            LogicHook<PrefabSceneEntityCreatedHook>.Execute(new PrefabSceneEntityCreatedHook(this));
            return Entity;
        }

        public void DestroyEntity() {
            if (m_destroying) return;
            m_destroyingDirectly = true;
            Destroy(gameObject);
        }
        
        public Entity CreateEntityWithIdAndGen(int id, ushort gen) {
            if (World == null)
                throw new Exception("Attempting to create a SceneEntity with no valid world.");
            if (Entity.IsAlive())
                throw new Exception("Attempting to create a SceneEntity multiple times.");
            Entity = World.CreateEntityWithIdAndGen(id, gen, m_inspector.EntityEnabled);
            m_inspector.BakeAuthoringComponents(Entity);
            gameObject.SetActive(Entity.IsEnabled());
            if (!m_isValidPrefab) return Entity;
            LogicHook<PrefabSceneEntityCreatedHook>.Execute(new PrefabSceneEntityCreatedHook(this));
            return Entity;
        }

        public virtual World GetWorld() => EcsModule<DefaultWorld>.Get().World;

        protected virtual bool WorldIsLoaded() => EcsModule<DefaultWorld>.IsLoaded();

        protected void BakeAuthoringComponents() => m_inspector.BakeAuthoringComponents(Entity);
        
        protected void Initialize(bool autoCreate = true) {
            if (!WorldIsLoaded()) return;
#if UNITY_EDITOR
            m_instantiated = true;
#endif
            World = GetWorld();
            if (autoCreate)
                CreateEntity();
            m_destroyedSubscription = World.SubscribeEntityDestroyed((in Entity e) => {
                if (e == Entity)
                    DestroyEntity();
            });
            m_worldDestroyedSubscription = World.SubscribeWorldDestroyed(DestroyEntity);
            m_initialized = true;
        }
        
        protected virtual void AddSpecialComponentsBeforeBaking() {
            Entity.Set(new GameObjectCd {
                value = gameObject
            });
            if (TryGetComponent(out RectTransform rectTransform)) {
                Entity.Set(new RectTransformCd {
                    value = rectTransform
                });
            }
            else {
                Entity.Set(new TransformCd {
                    value = transform
                });
            }
            Entity.Set(new SceneEntityCd {
                value = this
            });
        }

#if UNITY_EDITOR
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RetrieveRuntimeInspector() {
            if (World == null) return;
            if (!World.IsAlive()) return;
            if (!Entity.IsAlive()) return;
            m_inspector.RetrieveRuntimeState();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ApplyRuntimeInspector() {
            if (World == null) return;
            if (!World.IsAlive()) return;
            if (!Entity.IsAlive()) return;
            m_inspector.ApplyRuntimeState();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ResetIsDirty() {
            if (World == null) return;
            if (!World.IsAlive()) return;
            if (!Entity.IsAlive()) return;
            m_inspector.IsDirty = false;
        }

        [Button, ColoredBoxGroup("Serialization")]
        private void RegenerateGuid() {
            if (!EditorUtility.DisplayDialog(
                "Regenerate Guid Confirmation",
                "Regenerating this guid could break serialization of any existing save data. Are you sure you wish to regenerate it?",
                "Yes",
                "Cancel")) 
                return;
            m_guid = System.Guid.NewGuid().ToString();
        }
#endif
    }
}