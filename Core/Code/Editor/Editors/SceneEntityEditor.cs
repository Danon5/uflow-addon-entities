﻿using Sirenix.OdinInspector.Editor;
using UFlow.Addon.Entities.Core.Runtime;
using UFlow.Core.Runtime;
using UnityEditor;
using UnityEngine;

namespace UFlow.Addon.ECS.Core.Editor {
    [CustomEditor(typeof(SceneEntity), true)]
    [CanEditMultipleObjects]
    public sealed class SceneEntityEditor : OdinEditor {
        protected override void OnEnable() {
            base.OnEnable();
            EditorApplication.update += Update;
        }

        protected override void OnDisable() {
            base.OnDisable();
            EditorApplication.update -= Update;
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            var sceneEntity = target as SceneEntity;
            if (!Application.isPlaying || sceneEntity == null || !sceneEntity.Entity.IsAlive()) return;
            if (!GUI.changed) return;
            sceneEntity.ApplyRuntimeInspector();
        }

        private void Update() {
            var sceneEntity = target as SceneEntity;
            if (!Application.isPlaying || sceneEntity == null || !sceneEntity.Entity.IsAlive()) return;
            sceneEntity.RetrieveRuntimeInspector();
            // if (!UFlowUtils.Addons.GetSettings<EcsAddonSettings>().EnableRealtimeInspector) return;
            return;
            if (!sceneEntity.IsDirty) return;
            Repaint();
            sceneEntity.ResetIsDirty();
        }
    }
}