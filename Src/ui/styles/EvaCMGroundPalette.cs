namespace com.github.lhervier.ksp.evacmgroundmod.ui.styles
{
    /// <summary>
    /// Sizes proper to the settings window. Anything already shared (label color, combo metrics) comes
    /// from the KSP-Shared palettes instead: no builder of this mod hard-codes a dimension.
    /// </summary>
    public static class EvaCMGroundPalette
    {
        // ==============================================================
        // Main window
        // ==============================================================

        // Sized to its content rather than to a mockup: the title bar and the log level combo, which opens
        // its dropdown on the canvas, outside the window.
        public const float WindowWidth = 340f;
        public const float WindowHeight = 80f;

        // ==============================================================
        // Content
        // ==============================================================
        public const float ContentPaddingH = 10f;
        public const float ContentPaddingV = 10f;
        public const float ContentSpacing = 10f;
    }
}
