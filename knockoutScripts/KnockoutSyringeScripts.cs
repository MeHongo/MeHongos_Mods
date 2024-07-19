using FistVR;
using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine;

namespace mehongo
{
    // I mostly used SnappyBoltActionTrigger_Hooks as reference because it's small, readable, and I can directly trace it back to a script 
    [BepInPlugin("h3vr.mehongo.KnockoutScripts", "Knockout Scripts", "1.0.1")]
    public class KnockoutSyringeScripts : BaseUnityPlugin
    {
        void Awake ()
        {
            Debug.Log("Init");
            Hook();
        }
        void OnDestroy()
        {
            Unhook();
        }
        // All of this is in reference to the SyringeProjectile script if a frame of reference is needed. I probably got a lot of this wrong lul
        private void SyringeProjectile_FVRUpdate(ILContext il)
        {
            Debug.Log("Script is working :)");
            ILCursor c = new(il);
            c.GotoNext(
                // Here I'm trying to both make an out label and select up to a specific point (right after the check for knockout damage but before delayed knockout is applied)
        MoveType.After,
        i => i.MatchLdarg(0),
        i => i.MatchLdfld("FistVR.SyringeProjectile", "DoesKnockoutDamage"),
        i => i.MatchLdloc(0)
);
            // And I'm just trying to insert the following:
            // If(Sosig.m_receivedHeadShot == True) {
            //      DelayedKnockout(0, 0);
            //}
            //            c.Emit(OpCodes.Ldloc_0);
            //            c.Emit(OpCodes.Ldfld, typeof(Sosig).GetField("m_receivedHeadShot"));
            //            c.Emit(OpCodes.Brfalse, label);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldfld, typeof(FVRPhysicalObject).GetField("MP"));
            c.Emit(OpCodes.Call, typeof(SosigLink).GetMethod("GetStabLink"));
            c.Emit(OpCodes.Ldfld, typeof(SosigLink).GetField("BodyPart"));
            c.Emit(OpCodes.Brtrue_S);
            c.Emit(OpCodes.Ldloc_0);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldfld, typeof(SyringeProjectile).GetField("KnockoutDamage_Amount"));
            c.Emit(OpCodes.Call, typeof(Sosig).GetMethod("KnockUnconscious"));
            c.Emit(OpCodes.Br_S);
        }

        public void Hook()
        {
            IL.FistVR.SyringeProjectile.FVRUpdate += SyringeProjectile_FVRUpdate;
            Debug.Log("Hooked");
        }

        public void Unhook()
        {
            IL.FistVR.SyringeProjectile.FVRUpdate -= SyringeProjectile_FVRUpdate;

            Debug.Log("Unhooked");
        }
    }
}

//27  005B ldarg.0
//28	005C	ldfld	bool FistVR.SyringeProjectile::DoesKnockoutDamage
//29	0061	brfalse	41 (008D) ldarg .0
//30  0066    ldloc .0
//32  0068    ldfld float32 FistVR.SyringeProjectile::KnockoutDamage_Amount
//33	006D	ldarg.0
//34	006E	ldflda	valuetype [UnityEngine]
//UnityEngine.Vector2 FistVR.SyringeProjectile::KnockoutDamage_Delay
//35	0073	ldfld float32 [UnityEngine] UnityEngine.Vector2::x
//36	0078	ldarg.0
//37	0079	ldflda valuetype [UnityEngine] UnityEngine.Vector2 FistVR.SyringeProjectile::KnockoutDamage_Delay
//38	007E	ldfld float32 [UnityEngine] UnityEngine.Vector2::y
//39	0083	call float32 [UnityEngine] UnityEngine.Random::Range(float32, float32)
//40	0088	callvirt instance void FistVR.Sosig::DelayedKnockout(float32, float32)
