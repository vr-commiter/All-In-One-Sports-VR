using Appnori.Archery;
using Appnori.Badminton;
using Appnori.BasketBall;
using Appnori.Billiards;
using Appnori.Bowling;
using Appnori.Boxing.HeavyBag;
using Appnori.Boxing.Multi;
using Appnori.Boxing.Rythm;
using Appnori.Boxing.Single;
using Appnori.Nexus;
using Appnori.Soccer;
using Appnori.Squash;
using TENNIS;
using Baseball;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using Dart;
using HarmonyLib;
using PINGPONG;
using System;
using System.Drawing.Text;
using UnityEngine;
using static Appnori.Nexus.GameDataManager.PlayerData_Steam;
using VOLLEYBALL;
using System.Threading;
using FootHockey;
using Appnori.KungFu;
using MyTrueGear;
using System.Collections.Generic;

namespace AllInOneSportsVR_TrueGear
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;

        private static bool canBowPull = true;
        private static bool canThrowBasketBall = true;
        private static bool canLeftHitVolleyBall = true;
        private static bool canRightHitVolleyBall = true;
        private static bool canLeftHandHitHockey = true;
        private static bool canRightHandHitHockey = true;
        private static float lastPullingForce = 0;

        private static TrueGearMod _TrueGear = null;

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;

            Harmony.CreateAndPatchAll(typeof(Plugin));

            _TrueGear = new TrueGearMod();
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            _TrueGear.Play("HeartBeat");

        }

        public static KeyValuePair<float, float> GetAngle(Transform player, Vector3 hit)
        {
            // 计算玩家和击中点之间的向量
            Vector3 direction = hit - player.position;

            // 计算玩家正前方向量在水平面上的投影
            Vector3 forward = Vector3.ProjectOnPlane(player.forward, Vector3.up).normalized;

            // 计算夹角
            float angle = Vector3.SignedAngle(forward, direction, Vector3.up);

            angle = 360f - ((angle + 360f) % 360f);

            // 计算垂直偏移量
            float verticalOffset = player.transform.position.y - hit.y;

            return new KeyValuePair<float, float>(angle, verticalOffset);
        }


        //********************************射箭********************************//
        [HarmonyPostfix, HarmonyPatch(typeof(ArcheryPlayerControl), "ArrowShot", new Type[] { })]
        private static void ArcheryPlayerControl_ArrowShot1_Postfix(ArcheryPlayerControl __instance)
        {
            if (__instance.isRightHand)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandArrowShoot");
                _TrueGear.Play("RightHandArrowShoot");
            }
            else
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandArrowShoot");
                _TrueGear.Play("LeftHandArrowShoot");
            }
            Logger.LogInfo(__instance.isRightHand);
            Logger.LogInfo(GameDataManager.playType);
            Logger.LogInfo(GameDataManager.userInfo_mine.playerRole);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ArcheryPlayerControl), "ArrowTriggerCheck")]
        private static void ArcheryPlayerControl_ArrowTriggerCheck_Postfix(ArcheryPlayerControl __instance)
        {
            //if (lastPullingForce <= 0 && __instance.pullingForce > 0f)
            //{
            //    Logger.LogInfo("--------------------------------------");
            //    Logger.LogInfo("PlayerPullBow");
            //    _TrueGear.Play("PlayerPullBow");
            //}
            lastPullingForce = __instance.pullingForce;
            if (__instance.pullingForce <= 0f || __instance.pullingForce > 1f)
            {
                return;
            }
            if (!canBowPull)
            {
                return;
            }
            canBowPull = false;
            new Timer(BowPullTimerCallBack, null, 130, Timeout.Infinite);
            Logger.LogInfo("--------------------------------------");
            Logger.LogInfo("PlayerPullBow");
            _TrueGear.Play("PlayerPullBow");
            Logger.LogInfo(__instance.isRightHand);
            Logger.LogInfo(GameDataManager.playType);
            Logger.LogInfo(GameDataManager.userInfo_mine.playerRole);
        }

        private static void BowPullTimerCallBack(object o)
        {
            canBowPull = true;
        }


        //********************************羽毛球********************************//

        [HarmonyPostfix, HarmonyPatch(typeof(BadmintonPhysicsManager), "CreateShuttlecock_Player",new Type[] {typeof(BadmintonPlayer) })]
        private static void BadmintonPhysicsManager_CreateShuttlecock_Player_Postfix(BadmintonPhysicsManager __instance, BadmintonPlayer player)
        {
            if (player.isRightHand)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandPickupShuttlecock");
                _TrueGear.Play("LeftHandPickupShuttlecock");
            }
            else
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandPickupShuttlecock");
                _TrueGear.Play("RightHandPickupShuttlecock");
            }

            Logger.LogInfo(player.isRightHand);
        }


        [HarmonyPostfix, HarmonyPatch(typeof(BadmintonPhysicsManager), "HitShuttlecock_Player")]
        private static void BadmintonPhysicsManager_HitShuttlecock_Player_Postfix(BadmintonPhysicsManager __instance)
        {
            bool isRightHand = true;
            if (GameDataManager.playType != GameDataManager.PlayType.Multi && GameDataManager.userInfo_mine.playerRole == NetPlayerRole.Player2)
            {
                isRightHand = SingletonPunBase.Singleton<BadmintonGameManager>.GetInstance.player2.isRightHand;
            }
            else
            {
                isRightHand = SingletonPunBase.Singleton<BadmintonGameManager>.GetInstance.player1.isRightHand;
            }
            if (isRightHand)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandHitShuttlecock");
                _TrueGear.Play("RightHandHitShuttlecock");
            }
            else
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandHitShuttlecock");
                _TrueGear.Play("LeftHandHitShuttlecock");
            }

            Logger.LogInfo(GameDataManager.playType);
            Logger.LogInfo(GameDataManager.userInfo_mine.playerRole);
        }

        //********************************棒球********************************//

        private static bool isBaseBallRightHand = true;
        [HarmonyPostfix, HarmonyPatch(typeof(BaseballManager), "SetPlayerHand")]
        private static void BaseballManager_SetPlayerHand_Postfix(BaseballManager __instance, bool isRight)
        {
            Logger.LogInfo("--------------------------------------");
            Logger.LogInfo("SetPlayerHand");
            isBaseBallRightHand = isRight;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BaseballManager), "Call_HitBall")]
        private static void BaseballManager_Call_HitBall_Postfix(BaseballManager __instance)
        {
            if (CsGameData.Instance.curTeam != TeamPosition.Offense)
            {
                return;
            }
            if (isBaseBallRightHand)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandHitBaseBall");
                _TrueGear.Play("RightHandHitBaseBall");
            }
            else
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandHitBaseBall");
                _TrueGear.Play("LeftHandHitBaseBall");
            }
            Logger.LogInfo(CsGameData.Instance.curTeam);
            Logger.LogInfo(CsGameData.Instance.curMode);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CsDefender), "Call_ThrowCatchBall")]
        private static void CsDefender_Call_ThrowCatchBall_Postfix(CsDefender __instance)
        {
            if (CsGameData.Instance.curTeam != TeamPosition.Defense)
            {
                return;
            }
            if (isBaseBallRightHand)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandPickupBaseBall");
                _TrueGear.Play("RightHandPickupBaseBall");
            }
            else
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandPickupBaseBall");
                _TrueGear.Play("LeftHandPickupBaseBall");
            }
            Logger.LogInfo(CsGameData.Instance.curTeam);
            Logger.LogInfo(CsGameData.Instance.curMode);
        }


        [HarmonyPostfix, HarmonyPatch(typeof(BaseballManager), "Call_PlayerThrowBall")]
        private static void BaseballManager_Call_PlayerThrowBall_Postfix(BaseballManager __instance,int _select)
        {
            if (CsGameData.Instance.curTeam != TeamPosition.Defense)
            {
                return;
            }
            if (isBaseBallRightHand)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandThrowBaseBall");
                _TrueGear.Play("RightHandThrowBaseBall");
            }
            else
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandThrowBaseBall");
                _TrueGear.Play("LeftHandThrowBaseBall");
            }
            Logger.LogInfo(_select);
            Logger.LogInfo(CsGameData.Instance.curTeam);
            Logger.LogInfo(CsGameData.Instance.curMode);
        }
        //********************************篮球********************************//


        [HarmonyPostfix, HarmonyPatch(typeof(BasketBallPlayer), "ChangeHand")]
        private static void BasketBallPlayer_ChangeHand_Postfix(BasketBallPlayer __instance, HandState _HandState, bool _IsThrow)
        {
            if (_IsThrow)
            {
                if (_HandState == HandState.LeftHand)
                {
                    Logger.LogInfo("--------------------------------------");
                    Logger.LogInfo("LeftHandPickupBasketBall");
                    _TrueGear.Play("LeftHandPickupBasketBall");
                }
                else
                {
                    Logger.LogInfo("--------------------------------------");
                    Logger.LogInfo("RightHandPickupBasketBall");
                    _TrueGear.Play("RightHandPickupBasketBall");
                }
                return;
            }
            if (!canThrowBasketBall)
            {
                return;
            }
            canThrowBasketBall = false;
            new Timer(ThrowBasketBallTimerCallBack,null,100,Timeout.Infinite);
            if (_HandState == HandState.LeftHand)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandThrowBasketBall");
                _TrueGear.Play("LeftHandThrowBasketBall");
                return;
            }
            Logger.LogInfo("--------------------------------------");
            Logger.LogInfo("RightHandThrowBasketBall");
            _TrueGear.Play("RightHandThrowBasketBall");
        }

        private static void ThrowBasketBallTimerCallBack(object o)
        {
            canThrowBasketBall = true;
        }

        //********************************台球********************************//
        [HarmonyPrefix, HarmonyPatch(typeof(ShotController), "SetState")]
        private static void ShotController_SetState_Prefix(ShotController __instance, ShotController.GameStateType nextState)
        {

            if (nextState == ShotController.GameStateType.CameraFixAndWaitShot)
            {
                if (ShotController.IsRightHanded)
                {
                    Logger.LogInfo("--------------------------------------");
                    Logger.LogInfo("RightHandGrabCue");
                    _TrueGear.Play("RightHandGrabCue");
                }
                else
                {
                    Logger.LogInfo("--------------------------------------");
                    Logger.LogInfo("LeftHandGrabCue");
                    _TrueGear.Play("LeftHandGrabCue");
                }
            }
            if (nextState == ShotController.GameStateType.Shot)
            {
                if (ShotController.IsRightHanded)
                {
                    Logger.LogInfo("--------------------------------------");
                    Logger.LogInfo("RightHandHitBilliard");
                    _TrueGear.Play("RightHandHitBilliard");
                }
                else
                {
                    Logger.LogInfo("--------------------------------------");
                    Logger.LogInfo("LeftHandHitBilliard");
                    _TrueGear.Play("LeftHandHitBilliard");
                }
            }

        }

        //********************************保龄球********************************//
        [HarmonyPrefix, HarmonyPatch(typeof(BowlingPlayer), "OnTrigger_OnDataChanged")]
        private static void BowlingPlayer_OnTrigger_OnDataChanged_Prefix(BowlingPlayer __instance, HandState hand, bool obj)
        {
            Logger.LogInfo("--------------------------------------");
            if (hand == HandState.LeftHand)
            {
                if (obj)
                {
                    Logger.LogInfo("LeftHandPickupBowling");
                    _TrueGear.Play("LeftHandPickupBowling");
                }
                else
                {
                    Logger.LogInfo("LeftHandThrowBowling");
                    _TrueGear.Play("LeftHandThrowBowling");
                }
            }
            else if (hand == HandState.RightHand)
            {
                if (obj)
                {
                    Logger.LogInfo("RightHandPickupBowling");
                    _TrueGear.Play("RightHandPickupBowling");
                }
                else
                {
                    Logger.LogInfo("RightHandThrowBowling");
                    _TrueGear.Play("RightHandThrowBowling");
                }
            }
        }
        //********************************沙袋********************************//

        [HarmonyPostfix, HarmonyPatch(typeof(Glove), "HitWeakEvent")]
        private static void Glove_HitWeakEvent_Postfix(Glove __instance)
        {
            if (__instance.handDir == HeavyBagDataContainer.HandDirection.Left)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandWeakHit");
                _TrueGear.Play("LeftHandWeakHit");
            }
            else
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandWeakHit");
                _TrueGear.Play("RightHandWeakHit");
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Glove), "HitStrongEvent")]
        private static void Glove_HitStrongEvent_Postfix(Glove __instance)
        {
            if (__instance.handDir == HeavyBagDataContainer.HandDirection.Left)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandStrongHit");
                _TrueGear.Play("LeftHandStrongHit");
            }
            else
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandStrongHit");
                _TrueGear.Play("RightHandStrongHit");
            }
        }

        //********************************节奏拳击********************************//

        private static bool isNoteHit = false;
        [HarmonyPrefix, HarmonyPatch(typeof(Note), "OnTriggerEnter")]
        private static void Note_OnTriggerEnter_Prefix(Note __instance, Collider other)
        {
            if (__instance.IsReleased)
            {
                return;
            }
            if (__instance.currentData._cutDirection == 1)
            {
                return;
            }
            if (other == Appnori.Nexus.Singleton<RhythmBoxingDataContainer>.Instance.LeftHandCollider.CurrentData && __instance.currentData._type == 0)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandHitBoxingNote");
                _TrueGear.Play("LeftHandHitBoxingNote");
                isNoteHit = true;
                return;
            }
            if (other == Appnori.Nexus.Singleton<RhythmBoxingDataContainer>.Instance.RightHandCollider.CurrentData && __instance.currentData._type == 1)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandHitBoxingNote");
                _TrueGear.Play("RightHandHitBoxingNote");
                isNoteHit = true;
                return;
            }
            if (other == Appnori.Nexus.Singleton<RhythmBoxingDataContainer>.Instance.LeftHandCollider.CurrentData || other == Appnori.Nexus.Singleton<RhythmBoxingDataContainer>.Instance.RightHandCollider.CurrentData)
            {
                if (other == Appnori.Nexus.Singleton<RhythmBoxingDataContainer>.Instance.LeftHandCollider.CurrentData)
                {
                    Logger.LogInfo("--------------------------------------");
                    Logger.LogInfo("LeftHandHitBoxingNote");
                    _TrueGear.Play("LeftHandHitBoxingNote");
                    isNoteHit = true;
                }
                else
                {
                    Logger.LogInfo("--------------------------------------");
                    Logger.LogInfo("RightHandHitBoxingNote");
                    _TrueGear.Play("RightHandHitBoxingNote");
                    isNoteHit = true;
                }
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Note), "Note_OnRelease")]
        private static void Note_Note_OnRelease_Postfix(Note __instance)
        {
            if (isNoteHit)
            { 
                isNoteHit = false;
                return;
            }
            isNoteHit = false;
            Logger.LogInfo("--------------------------------------");
            Logger.LogInfo("PlayerMissBoxingNote");
            _TrueGear.Play("PlayerMissBoxingNote");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ObstacleDetector), "OnTriggerEnter")]
        private static void ObstacleDetector_OnTriggerEnter_Postfix(ObstacleDetector __instance,Collider other)
        {
            if (other.gameObject.name.Contains("Safe"))
            {
                return;
            }
            Logger.LogInfo("--------------------------------------");
            Logger.LogInfo("PlayerCollideObstacle");
            _TrueGear.Play("PlayerCollideObstacle");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Selector), "OnTriggerEnter")]
        private static void Selector_OnTriggerEnter_Postfix(Selector __instance)
        {
            Logger.LogInfo("--------------------------------------");
            Logger.LogInfo("PlayerHitSelector");
            _TrueGear.Play("PlayerHitSelector");
        }


        //********************************单人拳击********************************//

        [HarmonyPrefix, HarmonyPatch(typeof(BoxingPlayerCtrl), "CheckAttack")]
        private static void BoxingPlayerCtrl_CheckAttack_Prefix(BoxingPlayerCtrl __instance,Collision collision, bool isRight)
        {
            int num1 = isRight ? 1 : 0;
            Collider collider1 = collision.collider;
            if (collider1.CompareTag("Enemy_Glove") && BoxingCPUCtrl.Instance.IsStartPunchTime())
            {
                return;
            }
            if (!collider1.CompareTag("Enemy_Glove") && !collider1.CompareTag("Enemy_Head") && !collider1.CompareTag("Enemy_Body") && !collider1.CompareTag("Enemy_Arm"))
            {
                return;
            }
            GloveManager.GloveDatum gloveDatum = LocalSingleton<GloveManager>.Instance.GetGloveDatum(num1);
            if (Vector3.Dot(gloveDatum.dir, -collision.contacts[0].normal) <= 0.3f)
            {
                return;
            }
            if (isRight)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandHitEnemy");
                _TrueGear.Play("RightHandHitEnemy");
            }
            else
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandHitEnemy");
                _TrueGear.Play("LeftHandHitEnemy");
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BoxingPlayerCtrl), "StartDamage")]
        private static void BoxingPlayerCtrl_StartDamage_Prefix(BoxingPlayerCtrl __instance, in Vector3 damageVec)
        {
            if (__instance.playerState == BoxingSingleData.PlayerState.Down)
            {
                return;
            }
            Logger.LogInfo("--------------------------------------");
            Logger.LogInfo("PlayerBoxingDamage");
            //_TrueGear.Play("PlayerBoxingDamage");
            //Logger.LogInfo($"damageVec :{damageVec.x},{damageVec.y},{damageVec.z}");
            //Logger.LogInfo($"PlayerPos :{__instance.transform.position.x},{__instance.transform.position.y},{__instance.transform.position.z}");
            var angle = GetAngle(__instance.transform, damageVec);
            _TrueGear.PlayAngle("PlayerBoxingDamage", angle.Key, angle.Value);
            Logger.LogInfo($"Angle :{angle.Key},{angle.Value}");

            if (LocalSingleton<Appnori.Boxing.Single.GameManager>.Instance.testMode == Appnori.Boxing.Single.GameManager.TestMode.Infinite)
            {
                return;
            }
            if (__instance.curStamina <= 0f)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("PlayerBoxingFail");
                _TrueGear.Play("PlayerBoxingFail");
            }
        }

        //********************************多人拳击********************************//

        [HarmonyPrefix, HarmonyPatch(typeof(MultiGameManager), "Instance_OnDamageMessageReceive")]
        private static void MultiGameManager_Instance_OnDamageMessageReceive_Prefix(MultiGameManager __instance, ValidDamagePacket obj)
        {
            if (__instance.localPlayerData.InvincibilityEndTime > Time.time)
            {
                return;
            }
            Logger.LogInfo("--------------------------------------");
            Logger.LogInfo("PlayerBoxingDamage");
            _TrueGear.Play("PlayerBoxingDamage");
            //_TrueGear.Play("PlayerBoxingDamage");
            //Logger.LogInfo($"damageVec :{damageVec.x},{damageVec.y},{damageVec.z}");
            //Logger.LogInfo($"PlayerPos :{__instance.transform.position.x},{__instance.transform.position.y},{__instance.transform.position.z}");
            //var angle = GetAngle(__instance.transform, obj.position);
            //_TrueGear.PlayAngle("PlayerBoxingDamage", angle.Key, angle.Value);
            //Logger.LogInfo($"Angle :{angle.Key},{angle.Value}");
        }

        [HarmonyPrefix, HarmonyPatch(typeof(HitCollisionManager), "RemotePlayerDown_OnDataChanged")]
        private static void HitCollisionManager_RemotePlayerDown_OnDataChanged_Prefix(HitCollisionManager __instance, bool isDown)
        {
            if (isDown)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("PlayerBoxingFail");
                _TrueGear.Play("PlayerBoxingFail");
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(HitCollisionManager), "Detection_OnHit")]
        private static void HitCollisionManager_Detection_OnHit_Prefix(HitCollisionManager __instance, HitCollisionManager.HitInfo info)
        {
            if (LocalSingleton<MultiGameManager>.Instance.isPause)
            {
                return;
            }
            if (MultiGameManager.LocalPlayerData.IsDown.CurrentData)
            {
                return;
            }
            if (info.controller.BlockedBy.CurrentData != Appnori.Boxing.Multi.ControllerData.BlockType.None)
            {
                return;
            }
            if (__instance.ReadyTime > Time.time)
            {
                return;
            }
            if (info.controller.Type == Appnori.Boxing.Multi.ControllerData.ControllerType.Left)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandHitEnemy");
                _TrueGear.Play("LeftHandHitEnemy");
            }
            else
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandHitEnemy");
                _TrueGear.Play("RightHandHitEnemy");
            }
        }

        //[HarmonyPostfix, HarmonyPatch(typeof(HitCollisionManager), "Detection_OnHit")]
        //private static void HitCollisionManager_Detection_OnHit_Postfix(HitCollisionManager __instance, HitCollisionManager.HitInfo info)
        //{
        //    if (__instance.isKO)
        //    {
        //        Logger.LogInfo("--------------------------------------");
        //        Logger.LogInfo("PlayerBoxingFail");
        //        _TrueGear.Play("PlayerBoxingFail");
        //        return;
        //    }
        //}
        //********************************飞镖********************************//

        [HarmonyPrefix, HarmonyPatch(typeof(CsMain), "InputTrigger")]
        private static void CsMain_InputTrigger_Prefix(CsMain __instance)
        {
            if (!__instance.throwCheck && PublicGameUIManager.GetCurrentState() == PublicGameUIManager.ViewState.None)
            {
                if (GameSettingCtrl.IsRightHanded())
                {
                    Logger.LogInfo("--------------------------------------");
                    Logger.LogInfo("RightHandGrabDart");
                    _TrueGear.Play("RightHandGrabDart");
                }
                else
                {
                    Logger.LogInfo("--------------------------------------");
                    Logger.LogInfo("LeftHandGrabDart");
                    _TrueGear.Play("LeftHandGrabDart");
                }
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(CsMain), "OutputTrigger")]
        private static void CsMain_OutputTrigger_Prefix(CsMain __instance)
        {
            if (__instance.DartImageParent.activeSelf)
            {
                if (GameSettingCtrl.IsRightHanded())
                {
                    Logger.LogInfo("--------------------------------------");
                    Logger.LogInfo("RightHandShootDart");
                    _TrueGear.Play("RightHandShootDart");
                }
                else
                {
                    Logger.LogInfo("--------------------------------------");
                    Logger.LogInfo("LeftHandShootDart");
                    _TrueGear.Play("LeftHandShootDart");
                }

            }

        }
        //********************************曲棍球********************************//


        [HarmonyPrefix, HarmonyPatch(typeof(BallCtrl), "OnCollisionEnterMyPlayerFoot")]
        private static void BallCtrl_OnCollisionEnterMyPlayerFoot_Prefix(BallCtrl __instance, int LeftRightIdx)
        {
            if (LeftRightIdx == 0)
            {
                if (!canLeftHandHitHockey)
                {
                    return;
                }
                canLeftHandHitHockey = false;
                new Timer(LeftHandHitHockeyTimerCallBack,null,110,Timeout.Infinite);
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandHitHockeyBall");
                _TrueGear.Play("LeftHandHitHockeyBall");
            }
            else
            {
                if (!canRightHandHitHockey)
                {
                    return;
                }
                canRightHandHitHockey = false;
                new Timer(RightHandHitHockeyTimerCallBack, null, 110, Timeout.Infinite);
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandHitHockeyBall");
                _TrueGear.Play("RightHandHitHockeyBall");
            }
        }

        private static void LeftHandHitHockeyTimerCallBack(object o)
        {
            canLeftHandHitHockey = true;
        }
        private static void RightHandHitHockeyTimerCallBack(object o)
        {
            canRightHandHitHockey = true;
        }

        //********************************高尔夫********************************//

        [HarmonyPrefix, HarmonyPatch(typeof(GOLF.PlayerBehaviour), "CFollower_hitBallEvent")]
        private static void PlayerBehaviour_CFollower_hitBallEvent_Prefix(GOLF.PlayerBehaviour __instance)
        {
            Logger.LogInfo("--------------------------------------");
            Logger.LogInfo("PlayerHitGolfBall");
            _TrueGear.Play("PlayerHitGolfBall");
        }

        [HarmonyPrefix, HarmonyPatch(typeof(GOLF.PlayerBehaviour), "HandEvent")]
        private static void PlayerBehaviour_HandEvent_Prefix(GOLF.PlayerBehaviour __instance,HandState HandState, bool isGripOn)
        {
            if (isGripOn)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("PlayerGrabGolfClub");
                _TrueGear.Play("PlayerGrabGolfClub");
            }

            //Logger.LogInfo(HandState);
            //Logger.LogInfo(isGripOn);
        }
        //********************************功夫********************************//

        [HarmonyPrefix, HarmonyPatch(typeof(Appnori.KungFu.GameManager), "SetPlayerDamaged")]
        private static void GameManager_SetPlayerDamaged_Prefix(Appnori.KungFu.GameManager __instance)
        {
            if (Appnori.KungFu.GameManager.DebugMode.HasFlag(DebugFlag.DamageImmune))
            {
                return;
            }
            Logger.LogInfo("--------------------------------------");
            Logger.LogInfo("PlayerKungFuDamage");
            _TrueGear.Play("PlayerKungFuDamage");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Appnori.KungFu.GameManager), "SetPlayerDamaged")]
        private static void GameManager_SetPlayerDamaged_Postfix(Appnori.KungFu.GameManager __instance)
        {
            if (Appnori.KungFu.GameManager.DebugMode.HasFlag(DebugFlag.DamageImmune))
            {
                return;
            }
            if (!__instance.IsGameProgress)
            {
                return;
            }
            if (__instance.localPlayerHP <= 0f)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("PlayerKungFuFail");
                _TrueGear.Play("PlayerKungFuFail");
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(KungFuTutorialManager), "SetPlayerDamaged")]
        private static void KungFuTutorialManager_SetPlayerDamaged_Prefix(KungFuTutorialManager __instance)
        {
            if (KungFuTutorialManager.DebugMode.HasFlag(DebugFlag.DamageImmune))
            {
                return;
            }
            Logger.LogInfo("--------------------------------------");
            Logger.LogInfo("PlayerKungFuDamage");
            _TrueGear.Play("PlayerKungFuDamage");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(KungFuTutorialManager), "SetPlayerDamaged")]
        private static void KungFuTutorialManager_SetPlayerDamaged_Postfix(KungFuTutorialManager __instance)
        {
            if (KungFuTutorialManager.DebugMode.HasFlag(DebugFlag.DamageImmune))
            {
                return;
            }
            if (__instance.playerHP <= 0f)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("PlayerKungFuFail");
                _TrueGear.Play("PlayerKungFuFail");
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(KungFuLethalTarget), "Hitted")]
        private static void KungFuLethalTarget_Hitted_Prefix(KungFuLethalTarget __instance)
        {
            if (__instance.isLeft)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandHitKungFuLethalNote");
                _TrueGear.Play("LeftHandHitKungFuLethalNote");
            }
            else
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandHitKungFuLethalNote");
                _TrueGear.Play("RightHandHitKungFuLethalNote");
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(KungFuNoteAttack), "Hitted")]
        private static void KungFuNoteAttack_Hitted_Prefix(KungFuNoteAttack __instance, HandState hand)
        {
            if (hand == HandState.LeftHand)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandHitKungFuAttackNote");
                _TrueGear.Play("LeftHandHitKungFuAttackNote");
            }
            else
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandHitKungFuAttackNote");
                _TrueGear.Play("RightHandHitKungFuAttackNote");
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(KungFuNoteHand), "Hitted")]
        private static void KungFuNoteHand_Hitted_Prefix(KungFuNoteHand __instance, HandState hand)
        {
            if (hand == HandState.LeftHand)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandHitKungFuNote");
                _TrueGear.Play("LeftHandHitKungFuNote");
            }
            else
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandHitKungFuNote");
                _TrueGear.Play("RightHandHitKungFuNote");
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(KungFuNoteLethal), "Guarded")]
        private static void KungFuNoteLethal_Guarded_Prefix(KungFuNoteLethal __instance)
        {
            Logger.LogInfo("--------------------------------------");
            Logger.LogInfo("KungFuNoteLethalGuarded");
        }

        [HarmonyPrefix, HarmonyPatch(typeof(KungFuNoteStamping), "Hitted")]
        private static void KungFuNoteStamping_Hitted_Prefix(KungFuNoteStamping __instance, HandState hand)
        {
            if (hand == HandState.LeftHand)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandHitKungFuStampNote");
                _TrueGear.Play("LeftHandHitKungFuStampNote");
            }
            else
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandHitKungFuStampNote");
                _TrueGear.Play("RightHandHitKungFuStampNote");
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(KungFuPlayerAttackMove), "Guarded")]
        private static void KungFuPlayerAttackMove_Guarded_Prefix(KungFuPlayerAttackMove __instance, HandState hand)
        {
            if (hand == HandState.LeftHand)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandGuardKungFuAttackNote");
                _TrueGear.Play("LeftHandGuardKungFuAttackNote");
            }
            else
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandGuardKungFuAttackNote");
                _TrueGear.Play("RightHandGuardKungFuAttackNote");
            }
        }


        [HarmonyPrefix, HarmonyPatch(typeof(KungFuTutorialLethalTarget), "Hitted")]
        private static void KungFuTutorialLethalTarget_Hitted_Prefix(KungFuTutorialLethalTarget __instance)
        {
            if (__instance.isLeft)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandHitKungFuLethalNote");
                _TrueGear.Play("LeftHandHitKungFuLethalNote");
            }
            else
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandHitKungFuLethalNote");
                _TrueGear.Play("RightHandHitKungFuLethalNote");
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(KungFuTutorialNoteAttack), "Hitted")]
        private static void KungFuTutorialNoteAttack_Hitted_Prefix(KungFuTutorialNoteAttack __instance, HandState hand)
        {
            if (hand == HandState.LeftHand)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandHitKungFuAttackNote");
                _TrueGear.Play("LeftHandHitKungFuAttackNote");
            }
            else
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandHitKungFuAttackNote");
                _TrueGear.Play("RightHandHitKungFuAttackNote");
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(KungFuTutorialNoteHand), "Hitted")]
        private static void KungFuTutorialNoteHand_Hitted_Prefix(KungFuTutorialNoteHand __instance, HandState hand)
        {
            if (hand == HandState.LeftHand)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandHitKungFuNote");
                _TrueGear.Play("LeftHandHitKungFuNote");
            }
            else
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandHitKungFuNote");
                _TrueGear.Play("RightHandHitKungFuNote");
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(KungFuTutorialNoteStamping), "Hitted")]
        private static void KungFuTutorialNoteStamping_Hitted_Prefix(KungFuTutorialNoteStamping __instance, HandState hand)
        {
            if (hand == HandState.LeftHand)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandHitKungFuStampNote");
                _TrueGear.Play("LeftHandHitKungFuStampNote");
            }
            else
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandHitKungFuStampNote");
                _TrueGear.Play("RightHandHitKungFuStampNote");
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(KungFuTutorialPlayerAttack), "Guarded")]
        private static void KungFuTutorialPlayerAttack_Guarded_Prefix(KungFuTutorialPlayerAttack __instance, HandState hand)
        {
            if (hand == HandState.LeftHand)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandGuardKungFuAttackNote");
                _TrueGear.Play("LeftHandGuardKungFuAttackNote");
            }
            else
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandGuardKungFuAttackNote");
                _TrueGear.Play("RightHandGuardKungFuAttackNote");
            }
        }


        //********************************乒乓球********************************//

        [HarmonyPrefix, HarmonyPatch(typeof(PINGPONG.HandModelAnimation), "HandEvent")]
        private static void HandModelAnimation_HandEvent_Prefix(PINGPONG.HandModelAnimation __instance,HandState handState, bool isGripOn)
        {
            if (GameDataManager.userInfo_mine.playerRole == NetPlayerRole.Spectator)
            {
                return;
            }
            if (__instance.isRacketGrab)
            {
                return;
            }
            if (PublicGameUIManager.IsOverlay())
            {
                return;
            }
            if (__instance.HandState != handState)
            {
                return;
            }
            if (isGripOn)
            {
                if (__instance.isGetBall)
                {
                    return;
                }
                if (PINGPONG.GameManager.instance.serveTurn != PINGPONG.ServeTurn.my)
                {
                    return;
                }
                if (PINGPONG.GameManager.instance.isPlaying)
                {
                    return;
                }
                if (PINGPONG.GameManager.instance.isOnPanjung)
                {
                    return;
                }
                if (PINGPONG.ServeBallViewer.instance.GetisMove())
                {
                    return;
                }
                if (handState == HandState.LeftHand)
                {
                    Logger.LogInfo("--------------------------------------");
                    Logger.LogInfo("LeftHandPickupPingPong");
                    _TrueGear.Play("LeftHandPickupPingPong");
                }
                else
                {
                    Logger.LogInfo("--------------------------------------");
                    Logger.LogInfo("RightHandPickupPingPong");
                    _TrueGear.Play("RightHandPickupPingPong");
                }
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PINGPONG.PlayerBehaviour), "HitBall")]
        private static void PlayerBehaviour_HitBall_Postfix(PINGPONG.PlayerBehaviour __instance)
        {
            if (GameSettingCtrl.GetHandedState() == HandState.RightHand)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandHitPingPong");
                _TrueGear.Play("RightHandHitPingPong");
            }
            else
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandHitPingPong");
                _TrueGear.Play("LeftHandHitPingPong");
            }
        }

        //********************************足球********************************//

        [HarmonyPrefix, HarmonyPatch(typeof(FollowMesh), "CollisionEnterEvent")]
        private static void FollowMesh_CollisionEnterEvent_Prefix(FollowMesh __instance)
        {
            if (__instance.delay || CsMoving.Instance.myId != CsBall.Instance.OwnerId)
            {
                return;
            }
            if (__instance.isRight)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightFootKickBall");
                _TrueGear.Play("RightFootKickBall");
            }
            else
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftFootKickBall");
                _TrueGear.Play("LeftFootKickBall");
            }
        }

        private static bool canBodyCrush = true;

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerAction), "CheckBodyCrush")]
        private static void PlayerAction_CheckBodyCrush_Postfix(PlayerAction __instance, bool _bool)
        {
            if (_bool)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("BodyCrushSomeone");
                _TrueGear.Play("BodyCrushSomeone");
            }
            else
            {
                if (!canBodyCrush)
                {
                    return;
                }
                canBodyCrush = false;
                new Timer(BodyCrushTimerCallBack,null,110,Timeout.Infinite);
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("BodyCrush");
                _TrueGear.Play("BodyCrush");
            }
        }

        private static void BodyCrushTimerCallBack(object o)
        {
            canBodyCrush = true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(StunEffect), "OnApplied")]
        private static void StunEffect_OnApplied_Prefix(StunEffect __instance, StatusEffectManager manager)
        {
            if (manager.CharData.Id == CsMoving.Instance.myId)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("PlayerStun");
                _TrueGear.Play("PlayerStun");
            }
        }

        //[HarmonyPrefix, HarmonyPatch(typeof(CsKick), "KickFunc")]
        //private static void CsKick_KickFunc_Prefix(CsKick __instance)
        //{
        //    if (-0.91f < Appnori.Soccer.Singleton<LegKickFollow>.Instance.GetFootPos(__instance.isRight) || __instance.isKick)
        //    {
        //        if (__instance.isRight)
        //        {
        //            Logger.LogInfo("--------------------------------------");
        //            Logger.LogInfo("RightFootKickBall");
        //        }
        //        else
        //        {
        //            Logger.LogInfo("--------------------------------------");
        //            Logger.LogInfo("LeftFootKickBall");
        //        }
        //        Logger.LogInfo(__instance.threeStack);
        //    }
        //}

        //[HarmonyPrefix, HarmonyPatch(typeof(PlayerKeeperHand), "OnCollisionEnter")]
        //private static void PlayerKeeperHand_OnCollisionEnter_Prefix(PlayerKeeperHand __instance)
        //{
        //    if (__instance.isRight)
        //    {
        //        Logger.LogInfo("--------------------------------------");
        //        Logger.LogInfo("RightHandKickBall");
        //    }
        //    else
        //    {
        //        Logger.LogInfo("--------------------------------------");
        //        Logger.LogInfo("LeftHandKickBall");
        //    }
        //}

        //[HarmonyPrefix, HarmonyPatch(typeof(CsMoving), "FuncMove")]
        //private static void CsMoving_FuncMove_Prefix(CsMoving __instance)
        //{
        //    if (__instance.MyChar.gAutoChar.gStatus.IsHardCrowdControl() || PlayerAction.Instance.dashing || CsStickMove.Instance.moveValue == 0f || AllData.Instance.GameMode == GameType.shootOut)
        //    {
        //        return;
        //    }
        //    Logger.LogInfo("--------------------------------------");
        //    Logger.LogInfo("Moving");
        //}

        //********************************壁球********************************//

        [HarmonyPrefix, HarmonyPatch(typeof(SquashPlayerCtrl), "GameManager_OnHitBall")]
        private static void SquashPlayerCtrl_GameManager_OnHitBall_Prefix(SquashPlayerCtrl __instance, Appnori.Squash.GameManager.Turn player)
        {
            if (!LocalSingleton<Appnori.Squash.GameManager>.Instance.IsGamePlaying)
            {
                return;
            }
            if (player != __instance.PlayerTurn)
            {
                return;
            }
            if (GameSettingCtrl.IsRightHanded())
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandHitSquashBall");
                _TrueGear.Play("RightHandHitSquashBall");
                return;
            }
            Logger.LogInfo("--------------------------------------");
            Logger.LogInfo("LeftHandHitSquashBall");
            _TrueGear.Play("LeftHandHitSquashBall");
        }

        //********************************网球********************************//

        [HarmonyPrefix, HarmonyPatch(typeof(TENNIS.PlayerBehaviour), "HitBall")]
        private static void PlayerBehaviour_HitBall_Prefix(TENNIS.PlayerBehaviour __instance)
        {
            if (GameSettingCtrl.IsRightHanded())
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandHitTennisBall");
                _TrueGear.Play("RightHandHitTennisBall");
            }
            else
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandHitTennisBall");
                _TrueGear.Play("LeftHandHitTennisBall");
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(TENNIS.HandModelAnimation), "HandEvent")]
        private static void HandModelAnimation_HandEvent_Prefix(TENNIS.HandModelAnimation __instance, HandState handState, bool isGripOn)
        {
            if (GameDataManager.userInfo_mine.playerRole == NetPlayerRole.Spectator)
            {
                return;
            }
            if (__instance.isRacketGrab)
            {
                return;
            }
            if (PublicGameUIManager.IsOverlay())
            {
                return;
            }
            if (__instance.HandState != handState)
            {
                return;
            }
            if (isGripOn)
            {
                if (__instance.isGetBall)
                {
                    return;
                }
                if (TENNIS.GameManager.instance.serveTurn != TENNIS.ServeTurn.my)
                {
                    return;
                }
                if (TENNIS.GameManager.instance.isPlaying)
                {
                    return;
                }
                if (TENNIS.GameManager.instance.isOnPanjung)
                {
                    return;
                }
                if (TENNIS.ServeBallViewer.instance.GetisMove())
                {
                    return;
                }
                if (__instance.HandState == HandState.LeftHand)
                {
                    Logger.LogInfo("--------------------------------------");
                    Logger.LogInfo("LeftHandPickupTennis");
                    _TrueGear.Play("LeftHandPickupTennis");
                }
                else
                {
                    Logger.LogInfo("--------------------------------------");
                    Logger.LogInfo("RightHandPickupTennis");
                    _TrueGear.Play("RightHandPickupTennis");
                }
            }
        }

        //********************************排球********************************//

        [HarmonyPrefix, HarmonyPatch(typeof(RacketManager), "GetBall")]
        private static void RacketManager_GetBall_Prefix(RacketManager __instance, bool isLeft)
        {
            if (__instance.isGetBall)
            {
                return;
            }
            if (__instance.ab.playerInfo.actionType != VOLLEYBALL.AIBehaviour.ActionType.serve)
            {
                return;
            }
            if (VOLLEYBALL.GameManager.instance.isPlaying)
            {
                return;
            }
            if (VOLLEYBALL.GameManager.instance.GetIsOnPanjung())
            {
                return;
            }
            if (VOLLEYBALL.ServeBallViewer.instance.GetisMove())
            {
                return;
            }
            if (GameDataManager.userInfo_mine.playerRole == NetPlayerRole.Spectator)
            {
                return;
            }
            if (isLeft)
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("LeftHandPickupVolleyBall");
                _TrueGear.Play("LeftHandPickupVolleyBall");
            }
            else
            {
                Logger.LogInfo("--------------------------------------");
                Logger.LogInfo("RightHandPickupVolleyBall");
                _TrueGear.Play("RightHandPickupVolleyBall");
            }
        }


        [HarmonyPrefix, HarmonyPatch(typeof(RacketBehaviour), "HitBall")]
        private static void RacketBehaviour_HitBall_Prefix(RacketBehaviour __instance)
        {
            if (GameDataManager.userInfo_mine.playerRole != NetPlayerRole.Spectator)
            {
                if (__instance.HandState == HandState.LeftHand)
                {
                    if (!canLeftHitVolleyBall)
                    {
                        return;
                    }
                    canLeftHitVolleyBall = false;
                    new Timer(LeftHandHitVolleyBallCallBack,null,150,Timeout.Infinite);
                    Logger.LogInfo("--------------------------------------");
                    Logger.LogInfo("LeftHandHitVolleyBall");
                    _TrueGear.Play("LeftHandHitVolleyBall");
                }
                else
                {
                    if (!canRightHitVolleyBall)
                    {
                        return;
                    }
                    canRightHitVolleyBall = false;
                    new Timer(RightHandHitVolleyBallCallBack, null, 150, Timeout.Infinite);
                    Logger.LogInfo("--------------------------------------");
                    Logger.LogInfo("RightHandHitVolleyBall");
                    _TrueGear.Play("RightHandHitVolleyBall");
                }
            }

        }
        private static void LeftHandHitVolleyBallCallBack(object o)
        {
            canLeftHitVolleyBall = true;
        }
        private static void RightHandHitVolleyBallCallBack(object o)
        {
            canRightHitVolleyBall = true;
        }




    }
}
