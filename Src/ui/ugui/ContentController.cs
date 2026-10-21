using System;
using UnityEngine;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui.combo;

namespace com.github.lhervier.ksp.fix.evaconstructionmode.ui.ugui
{
    /// <summary>
    /// Behaviour of the window content: each control drives its setting through the view model, and is
    /// synced back from it whenever the window is shown.
    ///
    /// The sync matters here rather than being belt-and-braces: the window is rebuilt from scratch every
    /// time KSP destroys it, while the settings outlive the session — so what a control shows has to come
    /// from the setting, never from whatever it was built with.
    /// </summary>
    public class ContentController : MonoBehaviour
    {
        private EvaCMGroundViewModel _viewModel;
        public ContentController WithViewModel(EvaCMGroundViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        private ComboController _logLevelCombo;
        public ContentController WithControls(ComboController logLevelCombo)
        {
            this._logLevelCombo = logLevelCombo;
            return this;
        }

        public void Start()
        {
            // The options only exist once: the syncs then only select among them.
            if (_logLevelCombo != null)
            {
                _logLevelCombo.SetOptions(LogLevels.Names, _viewModel.LogLevel.ToString());
                _logLevelCombo.OnSelect.Add(OnLogLevelSelected);
            }
            // The controls do not update themselves on their own event, on purpose: what they display
            // comes back from the setting, so a value the view model refuses is never shown as chosen.
            if (_viewModel != null)
            {
                _viewModel.OnLogLevelChanged.Add(OnLogLevelChanged);
            }

            // First sync of the freshly built window: OnEnable already ran (Unity fires it from
            // AddComponent, before the view model could be injected) and will not run again until the
            // window is hidden and shown back, so nothing else would fill the controls this time round.
            SyncFromViewModel();
        }

        public void OnDestroy()
        {
            if (_logLevelCombo != null)
            {
                _logLevelCombo.OnSelect.Remove(OnLogLevelSelected);
            }
            if (_viewModel != null)
            {
                _viewModel.OnLogLevelChanged.Remove(OnLogLevelChanged);
            }
        }

        // Guarded: OnEnable can fire during AddComponent, before the view model is injected. Start()
        // then does the first sync; this one only covers the hide/show cycles that follow.
        public void OnEnable()
        {
            SyncFromViewModel();
        }

        /// <summary>Brings every control back in line with the settings.</summary>
        private void SyncFromViewModel()
        {
            if (_viewModel == null) return;
            if (_logLevelCombo != null)
            {
                _logLevelCombo.Select(_viewModel.LogLevel.ToString());
            }
        }

        // ==========================================================================
        // Controls -> view model
        // ==========================================================================

        private void OnLogLevelSelected(string value)
        {
            LogLevel level;
            if (Enum.TryParse(value, out level))
            {
                _viewModel.LogLevel = level;
            }
        }

        // ==========================================================================
        // View model -> controls
        // ==========================================================================

        // Instance methods, not static: EventVoid.Add dereferences the delegate's target.
        private void OnLogLevelChanged()
        {
            if (_logLevelCombo != null)
            {
                _logLevelCombo.Select(_viewModel.LogLevel.ToString());
            }
        }
    }
}
