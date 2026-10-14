using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.combo;
using com.github.lhervier.ksp.evaconstructionmodefix.ui.styles;

namespace com.github.lhervier.ksp.evaconstructionmodefix.ui.ugui
{
    /// <summary>
    /// Popup content (everything below the shared title bar): the log level combo.
    /// Mounted and stretched to fill the content host by the PopupBuilder, so it only fills what it is
    /// given.
    /// </summary>
    public class ContentBuilder : IUGUIBuilder<ContentController>
    {
        private EvaCMGroundViewModel _viewModel;
        public ContentBuilder WithViewModel(EvaCMGroundViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        public ContentController Build()
        {
            var rootGo = new GameObject("EvaCMGround.Content", typeof(RectTransform));

            var layout = rootGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(
                Mathf.RoundToInt(EvaCMGroundPalette.ContentPaddingH),
                Mathf.RoundToInt(EvaCMGroundPalette.ContentPaddingH),
                Mathf.RoundToInt(EvaCMGroundPalette.ContentPaddingV),
                Mathf.RoundToInt(EvaCMGroundPalette.ContentPaddingV));
            layout.spacing = EvaCMGroundPalette.ContentSpacing;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Parented by the builder itself: the combo hands its floating dropdown and its click trap
            // that same parent, and only reparents them onto the canvas while open.
            ComboController logLevelCombo = new ComboBuilder()
                .WithParent(rootGo.transform)
                .WithLabel(ModLocalization.GetString("settingsLogLevel"))
                .WithLabelFor(LogLevels.LabelFor)
                .Build();

            return rootGo
                .AddComponent<ContentController>()
                .WithViewModel(_viewModel)
                .WithControls(logLevelCombo);
        }
    }
}
