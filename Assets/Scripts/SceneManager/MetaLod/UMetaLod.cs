using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace umlod
{
	/// <summary>
	/// U meta lod target factor info.
	/// </summary>
    public struct UMetaLodTargetFactorInfo
    {
        public float FactorRate_Bounds;
        public float FactorRate_GeomComplexity;
        public float FactorRate_PSysComplexity;
        public float FactorRate_VisualImpact;
        public float FactorRate_UserFactors;

		//get accumulate factor
        public float GetAccumulated()
        {
            return FactorRate_Bounds + FactorRate_GeomComplexity + FactorRate_PSysComplexity + FactorRate_VisualImpact + FactorRate_UserFactors;
        }
    }

	/// <summary>
	/// I meta lod target.
	/// </summary>
    public interface IMetaLodTarget
    {
        float GetDistance();
        float GetFactorBounds();
        float GetFactorGeomComplexity();
        float GetFactorPSysComplexity();
        float GetFactorVisualImpact();
        float GetUserFactor(string factorName);

        void SetLiveness(float liveness);

        void OutputDebugInfo(ref UMetaLodTargetFactorInfo debugInfo);
    }

	/// <summary>
	/// U meta lod config.
	/// </summary>
    public static class UMetaLodConfig
    {
        // the time interval of an update (could be done discretedly)
        public static float UpdateInterval = 0.5f;

        // the time interval of an FPS update (could be done discretedly)
        public static float FPSUpdateInterval = 5.0f;

        // debug option (would output debugging strings to lod target if enabled)
        public static bool EnableDebuggingOutput = false;

        // performance level (target platform horsepower indication)
        public static UPerfLevel PerformanceLevel = UPerfLevel.Medium;
        // performance level magnifier
        public static Dictionary<UPerfLevel, float> PerfLevelScaleLut = new Dictionary<UPerfLevel, float> 
    	{
        	{ UPerfLevel.Highend, 0.2f },
        	{ UPerfLevel.Medium, 0.0f },
        	{ UPerfLevel.Lowend, -0.2f },
    	};

        // heat attenuation parameters overriding (including the formula)
        public static float DistInnerBound = 80.0f;
        public static float DistOuterBound = 180.0f;
        public static float FpsLowerBound = 15.0f;
        public static float FpsStandard = 30.0f;
        public static float FpsUpperBound = 60.0f;
        public static float FpsMinifyFactor = -0.2f;
        public static float FpsMagnifyFactor = 0.2f;
        public static fnHeatAttenuate HeatAttenuationFormula = UMetaLodDefaults.HeatAttenuation;
    }

    public class UMetaLod
    {
        public UMetaLod()
        {
            _initDefault();//set default factor
        }

        public void AddTarget(IMetaLodTarget target)
        {
            if (!_targets.Contains(target))
            {
                _targets.Add(target);
            }
        }

        public void RemoveTarget(IMetaLodTarget target)
        {
            if (_targets.Contains(target))
            {
                _targets.Remove(target);
            }
        }

        public void AddUserFactor(UImpactFactor userFactor)
        {
            if (!_userFactors.Contains(userFactor))
            {
                _userFactors.Add(userFactor);
            }
        }

        public void Update()
        {
            if (_updateTime())//enough update time
            {
                _updateHeatAttenuation();
                _updateTargets();
            }
        }

		private void _initDefault()
		{
			_factorBounds = UMetaLodDefaults.Factor_Bounds;
			_factorGeomComplexity = UMetaLodDefaults.Factor_GeomComplexity;
			_factorPSysComplexity = UMetaLodDefaults.Factor_PSysComplexity;
			_factorVisualImpact = UMetaLodDefaults.Factor_VisualImpact;
		}

		//calculate impact value
		private float _calculateImpact(UImpactFactor factor, float value)
		{
			float normalized = factor.Normalizer != null ? factor.Normalizer(value, factor.UpperBound, factor.LowerBound) : 0.0f;
			return normalized * Mathf.Clamp01(factor.Weight);
		}

		private bool _updateTime()
		{
			float deltaTime = Time.realtimeSinceStartup - _lastUpdateTime;
			if (deltaTime < UMetaLodConfig.UpdateInterval)
				return false;

			_lastUpdateTime = Time.realtimeSinceStartup;

			// update fps as needed
			float deltaFPSTime = Time.realtimeSinceStartup - _lastUpdateFPSTime;
			if (deltaFPSTime >= UMetaLodConfig.FPSUpdateInterval)
			{
				int passedFrames = Time.frameCount - _lastUpdateFrameCount;
				_currentFPS = (float)passedFrames / deltaTime;
				_lastUpdateFrameCount = Time.frameCount;
			}

			return true;
		}

		private void _updateHeatAttenuation()
		{
			// the magnification calculation is fixed across all targets
			// so that we extract this part and do it once per frame
			UMetaLodConfig.HeatAttenuationFormula(_currentFPS, out _distInnerAttenuated, out _distOuterAttenuated);
		}

		//update all targets liveness value
		private void _updateTargets()
		{
			foreach (var target in _targets)
			{
				_updateLiveness(target);
			}
		}

		//update target liveness value
		private void _updateLiveness(IMetaLodTarget target)
		{
			UMetaLodTargetFactorInfo factorInfo = new UMetaLodTargetFactorInfo();
			factorInfo.FactorRate_Bounds = _calculateImpact(_factorBounds, target.GetFactorBounds());
			factorInfo.FactorRate_GeomComplexity = _calculateImpact(_factorGeomComplexity, target.GetFactorGeomComplexity());
			factorInfo.FactorRate_PSysComplexity = _calculateImpact(_factorPSysComplexity, target.GetFactorPSysComplexity());
			factorInfo.FactorRate_VisualImpact = _calculateImpact(_factorBounds, target.GetFactorBounds());
			factorInfo.FactorRate_UserFactors = 0.0f;
			foreach (var factor in _userFactors)
				factorInfo.FactorRate_UserFactors += _calculateImpact(factor, target.GetUserFactor(factor.Name));

			// impact factors accumulation
			float factorRate = 1.0f - factorInfo.GetAccumulated();

			// perform liveness calculation
			float distance = target.GetDistance() * factorRate;
			float liveness = 1.0f - UMetaLodUtil.Percent(_distInnerAttenuated, _distOuterAttenuated, distance);
			target.SetLiveness(liveness);//set target liveness

			if (UMetaLodConfig.EnableDebuggingOutput)//if need, output log
				target.OutputDebugInfo(ref factorInfo);
		}

		private UImpactFactor _getSysFactor(string name)
		{
			switch (name)
			{
			case UMetaLodConst.Factor_Bounds: return _factorBounds;
			case UMetaLodConst.Factor_GeomComplexity: return _factorGeomComplexity;
			case UMetaLodConst.Factor_PSysComplexity: return _factorPSysComplexity;
			case UMetaLodConst.Factor_VisualImpact: return _factorVisualImpact;
			default: return new UImpactFactor();
			}
		}

		// impact targets
		private HashSet<IMetaLodTarget> _targets = new HashSet<IMetaLodTarget>();

		// impact factors
		private UImpactFactor _factorBounds;
		private UImpactFactor _factorGeomComplexity;
		private UImpactFactor _factorPSysComplexity;
		private UImpactFactor _factorVisualImpact;
		private HashSet<UImpactFactor> _userFactors = new HashSet<UImpactFactor>();

		// heat attenuation 
		private float _distInnerAttenuated;
		private float _distOuterAttenuated;
		private float _currentFPS;

		// internal states
		private float _lastUpdateTime = 0.0f;
		private float _lastUpdateFPSTime = 0.0f;
		private int _lastUpdateFrameCount = 0;

        public float DistInnerAttenuated
        {
            get { return _distInnerAttenuated; }
        }
        public float DistOuterAttenuated
        {
            get { return _distOuterAttenuated; }
        }

        // accessor to targets
        public HashSet<IMetaLodTarget> Targets { get { return _targets; } }

        // accessor to built-in factors (use this to modify default parameters in these factors)
        public UImpactFactor GetSysFactor(string name) { return _getSysFactor(name); }
    }
}

