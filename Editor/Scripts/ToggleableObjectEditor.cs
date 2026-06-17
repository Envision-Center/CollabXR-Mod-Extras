using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CollabXR.ModExtras
{
    [CustomPropertyDrawer(typeof(ToggleableObject))]
    public class ToggleableObjectEditor : PropertyDrawer
    {
        SerializedProperty type, needsTransparencySlider;
        PropertyField typeField, objField, componentField, vfxField, vfxPropertyNameField, meshField, shaderPropertyNameField, defaultEnabledField, needsTransparencyField, defaultTransparency;
        VisualElement container;
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            container = new VisualElement();
            type = property.FindPropertyRelative("type");
            needsTransparencySlider = property.FindPropertyRelative("needsTransparencySlider");
            typeField = new PropertyField(type);
            typeField.RegisterValueChangeCallback(_ => OnTypeChange());
            needsTransparencyField = new PropertyField(needsTransparencySlider);
            needsTransparencyField.RegisterValueChangeCallback(_ => OnTypeChange());

            objField = new PropertyField(property.FindPropertyRelative("obj"));
            componentField = new PropertyField(property.FindPropertyRelative("component"));
            vfxField = new PropertyField(property.FindPropertyRelative("vfx"));
            vfxPropertyNameField = new PropertyField(property.FindPropertyRelative("vfxPropertyName"));
            meshField = new PropertyField(property.FindPropertyRelative("mesh"));
            shaderPropertyNameField = new PropertyField(property.FindPropertyRelative("shaderPropertyName"));
            defaultEnabledField = new PropertyField(property.FindPropertyRelative("defaultEnabled"));
            defaultTransparency = new PropertyField(property.FindPropertyRelative("defaultTransparency"));

            container.Add(typeField);
            container.Add(objField);
            container.Add(componentField);
            container.Add(vfxField);
            container.Add(vfxPropertyNameField);
            container.Add(meshField);
            container.Add(shaderPropertyNameField);
            container.Add(needsTransparencyField);
            container.Add(defaultEnabledField);
            container.Add(defaultTransparency);
            return container;
        }

        private void OnTypeChange()
        {
            ToggleableObjectType typeEnum = (ToggleableObjectType) type.enumValueIndex;
            bool isMaterialRelated = (typeEnum == ToggleableObjectType.VFXBool || typeEnum == ToggleableObjectType.ShaderBool);
            objField.style.display = (typeEnum == ToggleableObjectType.GameObject) ? DisplayStyle.Flex : DisplayStyle.None;
            componentField.style.display = (typeEnum == ToggleableObjectType.Component) ? DisplayStyle.Flex : DisplayStyle.None;
            vfxField.style.display = (typeEnum == ToggleableObjectType.VFXBool) ? DisplayStyle.Flex : DisplayStyle.None;
            meshField.style.display = (typeEnum == ToggleableObjectType.ShaderBool) ? DisplayStyle.Flex : DisplayStyle.None;
            shaderPropertyNameField.style.display = isMaterialRelated ? DisplayStyle.Flex : DisplayStyle.None;
            needsTransparencyField.style.display = isMaterialRelated ? DisplayStyle.Flex : DisplayStyle.None;
            defaultTransparency.style.display = (needsTransparencySlider.boolValue ?  DisplayStyle.Flex : DisplayStyle.None);
        }
    }
}
