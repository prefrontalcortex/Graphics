using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.ShaderGraph.Drawing.Slots;

namespace UnityEditor.ShaderGraph
{
    class Vector4Control : IShaderControl
    {
        static readonly string[] k_SubLabels = { "X", "Y", "Z", "W" };

        public SerializableValueStore defaultValue { get; }

        public SlotValueType[] validPortTypes
        {
            get { return new SlotValueType[] { SlotValueType.Vector4 }; }
        }

        public Vector4Control()
        {
        }

        public Vector4Control(Vector4 defaultValue)
        {
            this.defaultValue = new SerializableValueStore()
            {
                vectorValue = defaultValue
            };
        }

        int m_UndoGroup = -1;

        public VisualElement GetControl(IShaderValue shaderValue)
        {
            VisualElement control = new VisualElement() { name = "VectorControl" };
            control.styleSheets.Add(Resources.Load<StyleSheet>("Styles/ShaderControls/VectorControl"));
            
            for (var i = 0; i < k_SubLabels.Length; i++)
                AddField(control, shaderValue, i, k_SubLabels[i]);
            return control;
        }

        void AddField(VisualElement element, IShaderValue shaderValue, int index, string subLabel)
        {
            var dummy = new VisualElement { name = "dummy" };
            var label = new Label(subLabel);
            dummy.Add(label);
            element.Add(dummy);
            var field = new FloatField { userData = index, value = shaderValue.value.vectorValue[index] };
            var dragger = new FieldMouseDragger<float>(field);
            dragger.SetDragZone(label);
            field.RegisterValueChangedCallback(evt =>
                {
                    if(evt.newValue.Equals(shaderValue.value.vectorValue))
                        return;
                    var value = shaderValue.value.vectorValue;
                    value[index] = (float)evt.newValue;
                    shaderValue.UpdateValue(new SerializableValueStore()
                    {
                        vectorValue = value
                    });
                });
            field.Q("unity-text-input").RegisterCallback<InputEvent>(evt =>
                {
                    if (m_UndoGroup == -1)
                    {
                        m_UndoGroup = Undo.GetCurrentGroup();
                        shaderValue.UpdateValue(shaderValue.value);
                    }
                    float newValue;
                    if (!float.TryParse(evt.newData, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out newValue))
                        newValue = 0f;
                    var value = shaderValue.value.vectorValue;
                    if (Mathf.Abs(value[index] - newValue) > 1e-9)
                    {
                        value[index] = newValue;
                        shaderValue.UpdateValue(new SerializableValueStore()
                        {
                            vectorValue = value
                        });
                    }
                });
            field.Q("unity-text-input").RegisterCallback<KeyDownEvent>(evt =>
                {
                    if (evt.keyCode == KeyCode.Escape && m_UndoGroup > -1)
                    {
                        Undo.RevertAllDownToGroup(m_UndoGroup);
                        m_UndoGroup = -1;
                        evt.StopPropagation();
                    }
                    element.MarkDirtyRepaint();
                });
            element.Add(field);
        }
    }
}
