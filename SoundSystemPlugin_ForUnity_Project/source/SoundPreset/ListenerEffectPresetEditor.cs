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

            var nameProp     = property.FindPropertyRelative("presetName");
            var kindProp     = property.FindPropertyRelative("kind");
            var settingsProp = property.FindPropertyRelative("settings");

            var nameField = new PropertyField(nameProp);
            var kindField = new PropertyField(kindProp);
            var settingsField = new PropertyField(settingsProp);

            kindField.RegisterValueChangeCallback(_ =>
            {
                AssignSettingInstance(settingsProp, (FilterKind)kindProp.enumValueIndex);
                settingsField.Bind(property.serializedObject);
            });

            root.Add(nameField);
            root.Add(kindField);
            root.Add(settingsField);

            //初期状態の設定インスタンスを適用
            AssignSettingInstance(settingsProp, (FilterKind)kindProp.enumValueIndex);

            return root;
        }

        private static void AssignSettingInstance(SerializedProperty settings, FilterKind k)
        {
            Type t = k switch
            {
                FilterKind.AudioChorusFilter     => typeof(ChorusFilterSettings),
                FilterKind.AudioDistortionFilter => typeof(DistortionFilterSettings),
                FilterKind.AudioEchoFilter       => typeof(EchoFilterSettings),
                FilterKind.AudioHighPassFilter   => typeof(HighPassFilterSettings),
                FilterKind.AudioLowPassFilter    => typeof(LowPassFilterSettings),
                FilterKind.AudioReverbFilter     => typeof(ReverbFilterSettings),
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
