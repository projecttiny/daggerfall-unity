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

namespace DaggerfallWorkshop{

	[RequireComponent(typeof(DaggerfallAudioSource))]
	public class MobileNPCSound : MonoBehaviour {
		
		MobilePersonAsset mobileAsset;
		MobilePersonMotor motor;
		DaggerfallAudioSource dfAudioSource;
		
		SoundClips GreetingSound;
		AudioClip greetingClip;
		
		private bool saidGreetingOnIdle = false;
		private bool cooldown = false;
		private const int cooldownTime = 8;
		

		private void Awake(){
			mobileAsset = FindMobilePersonAsset();
			motor = GetComponent<MobilePersonMotor>();
			dfAudioSource = GetComponent<DaggerfallAudioSource>();
			dfAudioSource.AudioSource.maxDistance = 16f;
			dfAudioSource.AudioSource.spatialBlend = 1;
			GreetingSound = SoundClips.AnimalPig;
			greetingClip = dfAudioSource.GetAudioClip((int)GreetingSound);
			
		}

		// Use this for initialization
		void Start () {
		}
		
		// Update is called once per frame
		void Update () {
			if(!cooldown && !saidGreetingOnIdle && motor.DistanceToPlayer <= 2.5f && mobileAsset.IsIdle){
				dfAudioSource.PlayOneShot(GreetingSound, 1, 1.1f);
				saidGreetingOnIdle = true;
				StartCoroutine(WaitGreeting());
			}else if(motor.DistanceToPlayer > 2.5f || !mobileAsset.IsIdle){
				saidGreetingOnIdle = false;
			}
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