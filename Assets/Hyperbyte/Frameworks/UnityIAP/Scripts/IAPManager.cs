// ©2019 - 2020 HYPERBYTE STUDIOS LLP
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Linq;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
#if HB_UNITYIAP
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
#endif
using Hyperbyte.Localization;

namespace Hyperbyte
{
	/// <summary>
	/// This class controls in-game purchased and its reward. This sdk typically leverages the official unity iap sdk and used as wrapper to make workflow
	/// simple that suits the asset store plugin and anyone without development skills can easily configure this setup.
	/// </summary>
	public class IAPManager : Singleton<IAPManager>
#if HB_UNITYIAP
	, IDetailedStoreListener 
#endif
	{
		IAPProducts iapManager;
		bool hasInitialised = false;

#if HB_UNITYIAP
		private IStoreController storeController;
		private IExtensionProvider extensionProvider;
#endif

		/// Purchase even callbacks.
		public static event Action<ProductInfo> OnPurchaseSuccessfulEvent;
		public static event Action<string> OnPurchaseFailedEvent;
		public static event Action<bool> OnRestoreCompletedEvent;

#pragma warning disable 0169
		public static event Action<bool> OnIAPInitializeEvent;
#pragma warning restore 0169
		public string environment = "production";

		/// <summary>
		/// Awake is called when the script instance is being loaded.
		/// </summary>
		private void Awake()
		{
			Initialise();
		}

		/// <summary>
		/// Start is called on the frame when a script is enabled just before
		/// any of the Update methods is called the first time.
		/// </summary>
		private void Start()
		{
			InitializeUnityIAP();
		}

		/// <summary>
		/// Initialize all IAPs from list of product info.
		/// </summary>
		public async void InitializeUnityIAP()
		{
			try
			{
				var options = new InitializationOptions()
					.SetEnvironmentName(environment);

				await UnityServices.InitializeAsync(options);
			}
			catch (Exception exception)
			{
				// An error occurred during initialization.
				return;
			}
#if HB_UNITYIAP
			ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
			StandardPurchasingModule.Instance().useFakeStoreUIMode = FakeStoreUIMode.StandardUser;

			foreach(ProductInfo info in iapManager.productInfos) 
			{
				IDs ids = new IDs();
				if(info.iOSSku != string.Empty) { ids.Add(info.iOSSku, AppleAppStore.Name);}
				if(info.googleSku != string.Empty) { ids.Add(info.iOSSku, GooglePlay.Name);}
				if(info.amazonSku != string.Empty) { ids.Add(info.iOSSku, AmazonApps.Name);}
				builder.AddProduct(info.productName, (ProductType)info.productType, ids);
			}	
			UnityPurchasing.Initialize(this, builder);
#endif
		}

#if HB_UNITYIAP

		/// <summary>
		/// IAP Initialize success callback from Unity IAP SDK.
		/// </summary>
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions) 
		{
			storeController = controller;
			extensionProvider = extensions;
			
			if(OnIAPInitializeEvent != null) 
			{
				OnIAPInitializeEvent.Invoke(true);
			}
        }

		/// <summary>
		/// IAP Initialize fail callback from Unity IAP SDK.
		/// </summary>
		public void OnInitializeFailed(InitializationFailureReason error) {
			if(OnIAPInitializeEvent != null) {
				OnIAPInitializeEvent.Invoke(false);
			}
		}
		
		/// <summary>
		/// IAP Initialize fail callback from Unity IAP SDK.
		/// </summary>
        public void OnInitializeFailed(InitializationFailureReason error, string message) 
		{
			if(OnIAPInitializeEvent != null) {
				OnIAPInitializeEvent.Invoke(false);
			}
        }

		/// <summary>
		/// Purchase Success callback from Unity IAP SDK.
		/// </summary>
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
			//~~~~~~~~~~~~~~ You can verify receipt here before processing rewards. ~~~~~~~~~~~~~~//
			// Debug.Log("receipt is: " + e.purchasedProduct.receipt);
			ProcessPurchaseRewards(e.purchasedProduct.definition.id);
			return PurchaseProcessingResult.Complete;
        }

		/// <summary>
		/// Purchase fail callback from Unity IAP SDK.
		/// </summary>
        public void OnPurchaseFailed(Product i, PurchaseFailureReason p) 
		{
			OnPurchaseFailure(p.ToString());
        }
#endif

		// Start Initialization of IAP SDK.
		void Initialise()
		{
			if (!hasInitialised)
			{
				iapManager = (IAPProducts)Resources.Load("IAPProducts");
				hasInitialised = true;
			}
		}

		/// <summary>
		/// Returns if IAP SDK has initialized or not.
		/// </summary>
		public bool hasUnityIAPSdkInitialised
		{
#if HB_UNITYIAP
			get { return storeController != null && extensionProvider != null; }
#else
			get { return true; }
#endif
		}

		// Returns product at given index on the list.
		public ProductInfo GetProductInfoById(int productIndex)
		{
			if (!hasInitialised)
			{
				Initialise();
			}
			return iapManager.productInfos[productIndex];
		}

		// Returns product with given name.
		public ProductInfo GetProductInfoByName(string productName)
		{
			if (!hasInitialised)
			{
				Initialise();
			}
			ProductInfo productInfo = iapManager.productInfos.ToList().Find(o => o.productName == productName);
			return productInfo;
		}

#if HB_UNITYIAP
		public Product GetProductFromSku(string productName) {
			return storeController.products.WithID(productName);
		}
#endif


		/// <summary>
		/// Restores all purchased products.
		/// </summary>
		public void RestoreAllProducts()
		{
#if HB_UNITYIAP
			if (hasUnityIAPSdkInitialised)
			{
				// iOS-specific logic
				extensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions((result, message) =>
				{
					if (OnRestoreCompletedEvent != null)
					{
						OnRestoreCompletedEvent.Invoke(result);
}
				});
        		// Fallback logic for other platforms
    		}
#else
			if (OnRestoreCompletedEvent != null)
			{
				OnRestoreCompletedEvent.Invoke(true);
			}
#endif
		}

		/// <summary>
		/// Invokes purchase success event.
		/// </summary>
		public void PurchaseProduct(ProductInfo productInfo)
		{
#if HB_UNITYIAP
			if(hasUnityIAPSdkInitialised) 
			{
				Product product = storeController.products.WithID(productInfo.productName);
				if (product == null) {	}
				else if (!product.availableToPurchase) { }
				else { storeController.InitiatePurchase(product); }
			}
#else
			//GameObject sandboxPuchaseScreen = (GameObject) Instantiate (Resources.Load("SandboxPurchaseScreen")) as GameObject;
			//sandboxPuchaseScreen.GetComponent<SandboxPurchaseScreen>().InitialiseSandboxPurchase(productInfo);
			IAPManager.Instance.OnSandboxPurchaseSuccess(productInfo);
#endif
		}

		/// <summary>
		/// Invokes sandbox purchase success event.
		/// </summary>
		public void OnSandboxPurchaseSuccess(ProductInfo productInfo)
		{
			ProcessPurchaseRewards(productInfo.productName);
		}

		/// <summary>
		/// Invokes sandbox purchase fail event.
		/// </summary>
		public void OnSandboxPurchaseFailure()
		{
			OnPurchaseFailure("Sandbox Purchase Failure");
		}

		/// <summary>
		/// Process rewards for the purhcased product.
		/// </summary>
		public void ProcessPurchaseRewards(string productName)
		{
			ProductInfo productInfo = IAPManager.Instance.GetProductInfoByName(productName);
			if (OnPurchaseSuccessfulEvent != null)
			{
				OnPurchaseSuccessfulEvent.Invoke(productInfo);
			}
		}

		/// <summary>
		/// Invokes purchase fail event.
		/// </summary>
		public void OnPurchaseFailure(string reason)
		{
			if (OnPurchaseFailedEvent != null)
			{
				OnPurchaseFailedEvent.Invoke(reason);
			}
		}
#if HB_UNITYIAP
		public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
		{
			// Handle the failed purchase here
			Debug.LogError("Purchase failed: " + failureDescription);
			if (OnPurchaseFailedEvent != null)
			{
				OnPurchaseFailedEvent.Invoke(failureDescription.message);
			}
		}
#endif
	}
}