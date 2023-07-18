﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UFlow.Core.Runtime;
// ReSharper disable StaticMemberInGenericType

namespace UFlow.Addon.Ecs.Core.Runtime {
    internal static class ComponentSerializer<TAttribute, TComponent>
        where TAttribute : Attribute
        where TComponent : IEcsComponent {
        private static readonly List<ISerializer<TComponent>> s_fieldSerializers = new();
        private static readonly object[] s_objectBuffer = new object[1];

        static ComponentSerializer() {
            var componentType = typeof(TComponent);
            var serializerType = typeof(UnmanagedFieldSerializer<,>);
            foreach (var field in UFlowUtils.Reflection.GetAllFieldsInTypeWithAttribute<TAttribute>(componentType)) {
                var genericType = serializerType.MakeGenericType(componentType, field.FieldType);
                s_objectBuffer[0] = field;
                s_fieldSerializers.Add(Activator.CreateInstance(genericType, s_objectBuffer) as ISerializer<TComponent>);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize(in ByteBuffer buffer, ref TComponent component) {
            foreach (var serializer in s_fieldSerializers)
                serializer.Serialize(buffer, ref component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deserialize(in ByteBuffer buffer, ref TComponent component) {
            foreach (var serializer in s_fieldSerializers)
                serializer.Deserialize(buffer, ref component);
        }
    }
}