namespace ToyBox.Infrastructure.Enums;

public enum EtudeState {
    NotStarted = 0,
    Started = 1,
    Active = 2,
    CompleteBeforeActive = 3,
    Completed = 4,
    CompletionBlocked = 5
}
public static partial class EtudeState_Localizer {
    public static string GetLocalized(this EtudeState type) {
        return type switch {
            EtudeState.NotStarted => m_NotStartedLocalizedText,
            EtudeState.Started => m_StartedLocalizedText,
            EtudeState.Active => m_ActiveLocalizedText,
            EtudeState.CompleteBeforeActive => m_CompleteBeforeActiveLocalizedText,
            EtudeState.Completed => m_CompletedLocalizedText,
            EtudeState.CompletionBlocked => m_CompletionBlockedLocalizedText,
            _ => "!!Error Unknown EtudeState!!",
        };
    }

    [LocalizedString("ToyBox_Infrastructure_Enums_EtudeState_Localizer_m_NotStartedLocalizedText", "Not Started")]
    private static partial string m_NotStartedLocalizedText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_EtudeState_Localizer_m_StartedLocalizedText", "Started")]
    private static partial string m_StartedLocalizedText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_EtudeState_Localizer_m_ActiveLocalizedText", "Active")]
    private static partial string m_ActiveLocalizedText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_EtudeState_Localizer_m_CompleteBeforeActiveLocalizedText", "Complete Before Active")]
    private static partial string m_CompleteBeforeActiveLocalizedText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_EtudeState_Localizer_m_CompletedLocalizedText", "Completed")]
    private static partial string m_CompletedLocalizedText { get; }
    [LocalizedString("ToyBox_Infrastructure_Enums_EtudeState_Localizer_m_CompletionBlockedLocalizedText", "Completion Blocked")]
    private static partial string m_CompletionBlockedLocalizedText { get; }
}
