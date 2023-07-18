﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// ReSharper disable SuspiciousTypeConversion.Global

namespace UFlow.Addon.Ecs.Core.Runtime {
    public abstract class BaseSystemGroup : IEnumerable<ISystem> {
        private readonly List<ISystem> m_systems;

        internal BaseSystemGroup() {
            m_systems = new List<ISystem>();
        }

        public BaseSystemGroup Add(in ISystem system) {
            m_systems.Add(system);
            return this;
        }

        public IEnumerator<ISystem> GetEnumerator() {
            return ((IEnumerable<ISystem>)m_systems).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        
        public void Remove(Type type) {
            var system = m_systems.Find(s => s.GetType() == type);
            if (system == null) return;
            m_systems.Remove(system);
        }

        public void Remove<T>() where T : ISystem {
            Remove(typeof(T));
        }

        public bool Has<T>() where T : ISystem {
            var type = typeof(T);
            return m_systems.Select(s => s.GetType() == type).Any();
        }

        public void Setup() {
            foreach (var system in m_systems) {
                if (system is IPreSetupSystem preSetupSystem)
                    preSetupSystem.PreSetup();
            }
            
            foreach (var system in m_systems) {
                if (system is ISetupSystem setupSystem)
                    setupSystem.Setup();
            }
        }

        public void Run() {
            foreach (var system in m_systems) {
                if (system is IRunSystem runSystem)
                    runSystem.PreRun();
            }
            
            foreach (var system in m_systems) {
                if (system is IRunSystem runSystem)
                    runSystem.Run();
            }
            
            foreach (var system in m_systems) {
                if (system is IRunSystem runSystem)
                    runSystem.PostRun();
            }
        }

        public void Cleanup() {
            foreach (var system in m_systems) {
                if (system is IPreCleanupSystem preCleanupSystem)
                    preCleanupSystem.PreCleanup();
            }

            foreach (var system in m_systems) {
                if (system is ICleanupSystem cleanupSystem)
                    cleanupSystem.Cleanup();
            }
        }

        internal void Sort() {
            var systemBuffer = new List<ISystem>(m_systems);
            
            foreach (var system in m_systems) {
                var systemType = system.GetType();
                foreach (var otherSystem in m_systems) {
                    if (ReferenceEquals(system, otherSystem)) continue;
                    var otherSystemType = otherSystem.GetType();
                    
                    if (ShouldPlaceBefore(systemType, otherSystemType)) {
                        MoveValueTo(systemBuffer, systemBuffer.IndexOf(system), 
                            Math.Max(systemBuffer.IndexOf(otherSystem) - 1, 0));
                        break;
                    }
                    
                    if (ShouldPlaceAfter(systemType, otherSystemType)) {
                        MoveValueTo(systemBuffer, systemBuffer.IndexOf(system), 
                            Math.Min(systemBuffer.IndexOf(otherSystem) + 1, systemBuffer.Count - 1));
                        break;
                    }
                }
            }
            
            m_systems.Clear();
            m_systems.AddRange(systemBuffer);
        }

        private static void MoveValueTo(in List<ISystem> list, in int a, in int b) {
            var temp = list[a];
            list.RemoveAt(a);
            if (b >= list.Count)
                list.Add(temp);
            else
                list.Insert(b, temp);
        }

        private static bool ShouldPlaceBefore(in Type sourceType, in Type otherType) {
            var beforeAttribute = sourceType.GetCustomAttribute<ExecuteBeforeAttribute>();
            return beforeAttribute != null && beforeAttribute.SystemType == otherType;
        }
        
        private static bool ShouldPlaceAfter(in Type sourceType, in Type otherType) {
            var afterAttribute = sourceType.GetCustomAttribute<ExecuteAfterAttribute>();
            return afterAttribute != null && afterAttribute.SystemType == otherType;
        }
    }
}