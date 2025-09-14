using GameCore.Skill;
using GameCore.Equipment;
using GameCore.TurnAction;
using UnityEngine;
using System;

namespace GameCore.Skill
{
    [CreateAssetMenu(fileName = "NewSkill", menuName = "GameCore/NewSkill")]
    public class SkillBase : ScriptableObject
    {
        public bool CanUse(Unit caster)
        {


            return true;
        }


    }
}