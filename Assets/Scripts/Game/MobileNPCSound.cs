using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Utility.AssetInjection;

namespace DaggerfallWorkshop{

	[RequireComponent(typeof(DaggerfallAudioSource))]
	public class MobileNPCSound : MonoBehaviour {


		public enum VillagerGreetings
		{
			Common, Villain, Scoundrel, Admired, Honored, Revered 
		}
		
		private static string[] greetings_common = new string[]{
			"greet_common_1_1",
			"greet_common_1_2",
			"greet_common_1_3",
			"greet_common_1_4",
			"greet_common_1_5",
		};

		MobilePersonAsset mobileAsset;
		MobilePersonMotor motor;
		DaggerfallAudioSource dfAudioSource;

		private bool saidGreetingOnIdle = false;
		private bool cooldown = false;
		private const int cooldownTime = 8;

		private void Awake(){
			mobileAsset = FindMobilePersonAsset();
			motor = GetComponent<MobilePersonMotor>();
			dfAudioSource = GetComponent<DaggerfallAudioSource>();
			dfAudioSource.AudioSource.maxDistance = 16f;
			dfAudioSource.AudioSource.spatialBlend = 1;
		}

		// Use this for initialization
		void Start () {
		}

		void Update () {
			if(!cooldown && !saidGreetingOnIdle && motor.DistanceToPlayer <= 2.5f && mobileAsset.IsIdle){
                string str = ChooseGreetingSound();
                Debug.LogError(str);
                dfAudioSource.PlayOneShot(str, 1, 1.1f);
				saidGreetingOnIdle = true;
				StartCoroutine(WaitGreeting());
			}else if(motor.DistanceToPlayer > 2.5f || !mobileAsset.IsIdle){
				saidGreetingOnIdle = false;
			}
		}
		
		private string ChooseGreetingSound(){
			return greetings_common[DFRandom.random_range(greetings_common.Length)];
		}

		IEnumerator WaitGreeting(){
			cooldown = true;
			yield return new WaitForSeconds(cooldownTime);
			cooldown = false;
		}

		private MobilePersonAsset FindMobilePersonAsset()
		{
			var mobilePersonAsset = GetComponentInChildren<MobilePersonAsset>();

			GameObject customMobilePersonAssetGo;
			if (ModManager.Instance && ModManager.Instance.TryGetAsset<GameObject>("MobilePersonAsset", true, out customMobilePersonAssetGo))
			{
				var customMobilePersonAsset = customMobilePersonAssetGo.GetComponent<MobilePersonAsset>();
				if (customMobilePersonAsset)
				{
					GameObject.Destroy(mobilePersonAsset.gameObject);
					customMobilePersonAssetGo.transform.SetParent(gameObject.transform);
					mobilePersonAsset = customMobilePersonAsset;
					mobilePersonAsset.Trigger = GetComponent<CapsuleCollider>();
				}
				else
				{
					Debug.LogError("Failed to retrieve MobilePersonAsset component from GameObject.");
				}
			}

			return mobilePersonAsset;
		}
	}

}