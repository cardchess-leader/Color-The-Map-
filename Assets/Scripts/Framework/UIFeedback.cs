using UnityEngine;
using Hyperbyte;
using Hyperbyte.HapticFeedback;

namespace Hyperbyte
{
	public class UIFeedback : Singleton<UIFeedback>
	{
		/// Play Haptic/Vibration Light.
		public void PlayHapticLight()
		{
			if (ProfileManager.Instance.IsVibrationEnabled)
			{
				HapticFeedbackGenerator.Haptic(HapticFeedback.FeedbackType.LightImpact);
			}
		}

		/// Play Haptic/Vibration Medium.
		public void PlayHapticMedium()
		{
			if (ProfileManager.Instance.IsVibrationEnabled)
			{
				HapticFeedbackGenerator.Haptic(HapticFeedback.FeedbackType.MediumImpact);
			}
		}

		/// Play Haptic/Vibration Heavy.
		public void PlayHapticHeavy()
		{
			if (ProfileManager.Instance.IsVibrationEnabled)
			{
				HapticFeedbackGenerator.Haptic(HapticFeedback.FeedbackType.HeavyImpact);
			}
		}

		/// Plays Button Click Sound and Haptic Feedback.
		public void PlayButtonPressEffect()
		{
			AudioController.Instance.PlayButtonClickSound();
			PlayHapticLight();
		}
	}
}