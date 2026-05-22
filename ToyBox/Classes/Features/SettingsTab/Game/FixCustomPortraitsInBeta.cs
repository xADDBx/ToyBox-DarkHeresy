using Kingmaker.Blueprints;
using OwlPack.Runtime;

namespace ToyBox.Features.SettingsTab.Game;

#warning BETA FEATURE
public partial class FixCustomPortraitsInBeta : ToggledFeature {
    private class PortraitDataExternalSerializer : AObjectSerializer<PortraitData> {
        private const byte m_CustomId_FieldId = 0;
        public override TypeInfo TypeInfo {
            get {
                return m_TypeInfo;
            }
        }
        public override void DeserializeInternal<TFormatter>(TFormatter formatter, ref PortraitData? value, uint objectId, DeserializerState state) {
            var runtimeTypeInfo = state.TypeLibrary.GetTypeInfo<PortraitData>();
            var mapping = state.GetMappingForType(TypeInfo, runtimeTypeInfo);

            string? customId = null;

            formatter.EnterObject();
            for (var i = 0; i < runtimeTypeInfo.Fields.Length; i++) {
                formatter.ReadFieldHeader(runtimeTypeInfo, out var fieldId, out var dataSize);

                var mapped = mapping[fieldId];
                if (mapped == m_CustomId_FieldId) {
                    customId = formatter.ReadString(state);
                } else {
                    formatter.SkipField(dataSize);
                }
            }
            formatter.LeaveObject();

            value = new PortraitData(customId);

            state.References.Register(objectId, value);
        }
        public override void SerializeInternal<TFormatter>(TFormatter formatter, ref PortraitData? value, SerializerState state) {
            if (value == null) {
                formatter.NullObject();
                return;
            }

            var (id, isRef) = state.References.GetOrRegister(value);
            var objectId = id;
            if (isRef) {
                formatter.ObjectRef(objectId);
                return;
            }

            var typeId = state.TypeLibrary.RegisterType<PortraitData>(TypeInfo);
            formatter.StartObject(typeId, TypeInfo.Name, objectId);

            var customId = value.CustomId;
            formatter.StringField(m_CustomId_FieldId, "CustomId", ref customId, state);

            formatter.EndObject();
        }
    }
    public override ref bool IsEnabled {
        get {
            return ref Settings.EnableFixCustomPortraitsInBeta;
        }
    }
    [LocalizedString("ToyBox_Features_SettingsTab_Game_FixCustomPortraitsInBeta_Name", "Fix Custom Portraits")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_SettingsTab_Game_FixCustomPortraitsInBeta_Description", "Owlcat changed their save format (no more json) which breaks saving with custom portraits. This fixes that. You still need to use Party => Stats to set a custom portrait. Disabling this feature requires a restart.")]
    public override partial string Description { get; }
    public override void Enable() {
        base.Enable();
        ExternalTypeLibrary.Instance.RegisterType(typeof(PortraitData), typeof(PortraitDataExternalSerializer), m_TypeInfo);
    }
    private static readonly TypeInfo m_TypeInfo = new() {
        Name = typeof(PortraitData).FullName,
        Fields =
            [
                new OwlPack.Runtime.FieldInfo("CustomId", typeof(string), null),
            ],
        Flags = TypeFlags.IsExternal
    };

}
