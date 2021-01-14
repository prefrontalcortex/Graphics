using System.Linq;
using UnityEngine;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph.Drawing.Controls;
using UnityEditor.ShaderGraph.Internal;

namespace UnityEditor.ShaderGraph
{
    [Title("Input", "Texture", "Texture Sampler")]
    class TextureSamplerNode : AbstractMaterialNode, IGeneratesBodyCode, IMayRequireMeshUV
    {
        public const int TextureInputId = 0;
        public const int SamplerOutputId = 1;

        const string kTextureInputName = "Texture";
        const string kSamplerOutputName = "Sampler";

        public override bool hasPreview { get { return false; } }

        public TextureSamplerNode()
        {
            name = "Texture Sampler";
            UpdateNodeAfterDeserialization();
        }

        public sealed override void UpdateNodeAfterDeserialization()
        {
            AddSlot(new Texture2DInputMaterialSlot(TextureInputId, kTextureInputName, kTextureInputName));
            AddSlot(new SamplerStateMaterialSlot(SamplerOutputId, kSamplerOutputName, kSamplerOutputName, SlotType.Output));

            RemoveSlotsNameNotMatching(new[] { TextureInputId, SamplerOutputId });
        }

        // Node generations
        public virtual void GenerateNodeCode(ShaderStringBuilder sb, GenerationMode generationMode)
        {
            var id = GetSlotValue(TextureInputId, generationMode);
            var result = string.Format("UnitySamplerState {0};\n#ifdef SHADERGRAPH_PREVIEW\n{0}.samplerstate = default_sampler_Linear_Repeat;\n#else\n{0}.samplerstate = {1}.samplerstate;\n#endif", GetVariableNameForSlot(SamplerOutputId), id);
            sb.AppendLine(result);
        }

        public bool RequiresMeshUV(UVChannel channel, ShaderStageCapability stageCapability)
        {
            return true;
        }
    }
}
