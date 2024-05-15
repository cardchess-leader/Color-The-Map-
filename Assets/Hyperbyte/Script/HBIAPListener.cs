// ©2019 - 2020 HYPERBYTE STUDIOS LLP
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Collections;
using System.Collections.Generic;
using Hyperbyte.Localization;
using UnityEngine;

#if HB_UNITYIAP
using UnityEngine.Purchasing;
#endif

namespace Hyperbyte
{
    public class HBIAPListener : MonoBehaviour
    {
        /// <summary>
        /// This function is called when the behaviour becomes enabled or active.
        /// </summary>
        private void OnEnable()
        {
            IAPManager.OnPurchaseSuccessfulEvent += OnPurchaseSuccessful;
            IAPManager.OnPurchaseFailedEvent += OnPurchaseFailed;
            IAPManager.OnRestoreCompletedEvent += OnRestoreCompleted;
        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled or inactive.
        /// </summary>
        private void OnDisable()
        {
            IAPManager.OnPurchaseSuccessfulEvent -= OnPurchaseSuccessful;
            IAPManager.OnPurchaseFailedEvent -= OnPurchaseFailed;
            IAPManager.OnRestoreCompletedEvent -= OnRestoreCompleted;
        }

        /// <summary>
        /// Purchase Rewards will be processed from here. You can adjust your code based on your requirements.
        /// </summary>
        /// <param name="productInfo"></param>
        void OnPurchaseSuccessful(ProductInfo productInfo)
        {
            RewardType rewardType = ((RewardType)productInfo.rewardType);

            switch (rewardType)
            {
                case RewardType.REMOVE_ADS:
                    ProfileManager.Instance.SetAppAsAdFree();
                    UITKController.Instance.ShowUISegment("iap-success-popup");
                    UITKController.Instance.UpdateIAPButton();
                    StartCoroutine(UITKController.Instance.InitializeCountryList());
                    break;
            }

#if HB_UNITYIAP
            Product product = IAPManager.Instance.GetProductFromSku(productInfo.productName);
#endif
        }

        void OnPurchaseFailed(string reason)
        {
            UITKController.Instance.ShowUISegment("iap-fail-popup");
            Debug.Log("reason is: " + reason);
        }

        void OnRestoreCompleted(bool result)
        {
            if (result)
            {
                Debug.Log("Restore Successful");
            }
            else
            {
                Debug.Log("Restore Not Successful");
            }
        }
    }
}
