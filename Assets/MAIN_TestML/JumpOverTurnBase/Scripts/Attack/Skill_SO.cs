using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Skill", menuName = "JumpeeIsland/Skill", order = 1)]
public class Skill_SO : ScriptableObject
{
    //Manage attribute of unit's skills
    [Header("Skill variable")]
    [SerializeField] private string _animTrigger;
    [SerializeField] private int _duration;
    [Tooltip("It might be strength multiplier or available range of attack")]
    [SerializeField] private int _magnitude;
    
    [Header("Skill range")]
    [SerializeField] private RangeType _rangeType;
    [Tooltip("It might be range of attack for Creature or amount of targets for buildings")]
    [SerializeField] private int _numberOfSteps;
    
    [Header("Skill effect")]
    [SerializeField] private SkillEffectType _skillEffectType;
    [SerializeField] private Material _effectMaterial;
    [Tooltip("GlobalTarget mean the unit just keep attacking and don't care about hitting target or not")]
    [SerializeField] private bool _isGlobalTarget;
    [SerializeField] private bool _isPreAttack;
    private SkillEffect _skillEffect;
    
    [Header("ML property")]
    [SerializeField] private NNModel MLModel;
    private SkillRange _skillRange;

    #region Skill Range

    public IEnumerable<Vector3> CalculateSkillRange(Vector3 currPos, Vector3 direction)
    {
        direction = direction.normalized;
        CheckSkillRangeNull();
        
        return _skillRange.CalculateSkillRange(currPos, direction, _numberOfSteps);
    }

    private void CheckSkillRangeNull()
    {
        if (_skillRange == null)
            SetRangeType();
    }

    #endregion

    #region Skill Effect

    public SkillEffect GetSkillEffect()
    {
        if (_skillEffect == null)
            SetEffectType();
        return _skillEffect;
    }

    #endregion

    #region Initiate
    
    private void SetRangeType()
    {
        switch (_rangeType)
        {
            case RangeType.StraightAheadSingle:
                _skillRange = new StraightAheadSingle();
                break;
            case RangeType.FrontArea:
                _skillRange = new FrontArea();
                break;
            case RangeType.BackHandStrike:
                _skillRange = new BackHandStrike();
                break;
            case RangeType.FrontArea3D:
                _skillRange = new FrontArea3D();
                break;
            case RangeType.SquareArea:
                _skillRange = new SquareArea();
                break;
            case RangeType.CurrentPos:
                _skillRange = new CurrentPos();
                break;
            case RangeType.FrontStrike:
                _skillRange = new FrontStrike();
                break;
            case RangeType.AccurateAttackByHp:
                _skillRange = new AccurateAttackByHp();
                break;
            case RangeType.AccurateAttackByDistance:
                _skillRange = new AccurateAttackByDistance(_magnitude);
                break;
            case RangeType.Circle:
                _skillRange = new Circle();
                break;
            case RangeType.TShapeFront:
                _skillRange = new TShapeFront();
                break;
            case RangeType.PerpendicularWipe:
                _skillRange = new PerpendicularWipe();
                break;
        }
    }
    
    private void SetEffectType()
    {
        switch (_skillEffectType)
        {
            case SkillEffectType.StrengthBoost:
                _skillEffect = new StrengthBooster(_duration,_magnitude);
                break;
            case SkillEffectType.Teleport:
                _skillEffect = new Teleport();
                break;
            case SkillEffectType.Frozen:
                _skillEffect = new Frozen(_duration, _effectMaterial);
                break;
        }
    }

    #endregion

    public int GetDuration()
    {
        return _duration;
    }

    public NNModel GetModel()
    {
        return MLModel;
    }

    public string GetAnimation()
    {
        return _animTrigger;
    }

    public bool CheckGlobalTarget()
    {
        return _isGlobalTarget;
    }

    public bool CheckPreAttack()
    {
        return _isPreAttack;
    }
}