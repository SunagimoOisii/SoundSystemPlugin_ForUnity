#if UNITY_EDITOR
namespace SoundSystem
{
    using System;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;

    [CustomPropertyDrawer(typeof(SoundPresetProperty.ListenerEffectPreset))]
    internal sealed class ListenerEffectPresetDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            var name     = property.FindPropertyRelative("presetName");
            var kind     = property.FindPropertyRelative("kind");
            var settings = property.FindPropertyRelative("settings");

            var nameField     = new PropertyField(property.FindPropertyRelative("presetName"));
            var kindField     = new PropertyField(kind);
            var settingsField = new PropertyField(settings);

            kindField.RegisterValueChangeCallback(_ =>
            {
                AssignSettingInstance(settings, (FilterKind)kind.enumValueIndex);
                settingsField.Bind(property.serializedObject);
            });

            root.Add(nameField);
            root.Add(kindField);
            root.Add(settingsField);

            //初期状態の設定インスタンスを適用
            AssignSettingInstance(settings, (FilterKind)kind.enumValueIndex);

            return root;
        }

        private static void AssignSettingInstance(SerializedProperty settings, FilterKind k)
        {
            Type t = k switch
            {
                FilterKind.Chorus     => typeof(ChorusFilterSettings),
                FilterKind.Distortion => typeof(DistortionFilterSettings),
                FilterKind.Echo       => typeof(EchoFilterSettings),
                FilterKind.HighPass   => typeof(HighPassFilterSettings),
                FilterKind.LowPass    => typeof(LowPassFilterSettings),
                FilterKind.Reverb     => typeof(ReverbFilterSettings),
                _ => null,
            };

            if (t == null)
            {
                settings.managedReferenceValue = null;
                return;
            }

            if (settings.managedReferenceValue == null ||
                settings.managedReferenceValue.GetType() != t)
            {
                settings.managedReferenceValue = Activator.CreateInstance(t);
            }

            settings.serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
