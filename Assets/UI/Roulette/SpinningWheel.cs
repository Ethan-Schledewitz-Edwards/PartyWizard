using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class SpinningWheel : MonoBehaviour
{
    [Header("Wheel Settings")]
    [SerializeField] private RectTransform wheelTransform;
    [SerializeField] private int segments = 6;
    [SerializeField] private float minSpinDuration = 3f;
    [SerializeField] private float maxSpinDuration = 5f;
    [SerializeField] private int minRevolutions = 3;
    [SerializeField] private int maxRevolutions = 5;
    
    [Header("Image Alignment")]
    [SerializeField] private float imageRotationOffset = 0f; 
    [Tooltip("If true, randomizes bullet position each spin. If false, uses bulletSegment value below.")]
    [SerializeField] private bool randomizeBulletEachSpin = true;
    [SerializeField] private int bulletSegment = 0;
    
    [Header("Animation Curve")]
    [SerializeField] private AnimationCurve spinCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Events")]
    public UnityEvent OnSpinStart;
    public UnityEvent OnSpinComplete;
    public UnityEvent OnBulletHit; 
    public UnityEvent OnSafe; 
    
    private bool isSpinning = false;
    private float degreesPerSegment;
    private int currentBulletSegment;
    
    void Start()
    {
        degreesPerSegment = 360f / segments;
        
        if (OnSpinStart == null) OnSpinStart = new UnityEvent();
        if (OnSpinComplete == null) OnSpinComplete = new UnityEvent();
        if (OnBulletHit == null) OnBulletHit = new UnityEvent();
        if (OnSafe == null) OnSafe = new UnityEvent();
        
        if (randomizeBulletEachSpin)
        {
            currentBulletSegment = Random.Range(0, segments);
        }
        else
        {
            currentBulletSegment = Mathf.Clamp(bulletSegment, 0, segments - 1);
        }
        
        Debug.Log($"Wheel initialized. Bullet in segment: {currentBulletSegment}");
    }
  
    public void Spin()
    {
        if (!isSpinning)
        {
            if (randomizeBulletEachSpin)
            {
                currentBulletSegment = Random.Range(0, segments);
                Debug.Log($"New bullet position: Segment {currentBulletSegment}");
            }
            
            StartCoroutine(SpinWheel());
        }
    }

    public void SpinToSegment(int targetSegment)
    {
        if (!isSpinning)
        {
            StartCoroutine(SpinWheel(targetSegment));
        }
    }
    

    public int GetCurrentBulletSegment()
    {
        return currentBulletSegment;
    }
    
 
    public void SetBulletSegment(int segment)
    {
        currentBulletSegment = Mathf.Clamp(segment, 0, segments - 1);
        Debug.Log($"Bullet manually set to segment: {currentBulletSegment}");
    }
    
    private IEnumerator SpinWheel(int? forcedSegment = null)
    {
        isSpinning = true;
        OnSpinStart?.Invoke();
        
        int targetSegment = forcedSegment ?? Random.Range(0, segments);
        
        float duration = Random.Range(minSpinDuration, maxSpinDuration);
        int revolutions = Random.Range(minRevolutions, maxRevolutions);
        
        float targetAngle = (revolutions * 360f) + (targetSegment * degreesPerSegment) + imageRotationOffset;
        
        float randomOffset = Random.Range(-degreesPerSegment * 0.3f, degreesPerSegment * 0.3f);
        targetAngle += randomOffset;
        
        float startAngle = wheelTransform.eulerAngles.z;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float curveValue = spinCurve.Evaluate(t);
            
            float currentAngle = startAngle + (targetAngle * curveValue);
            
            wheelTransform.eulerAngles = new Vector3(0, 0, currentAngle);
            
            yield return null;
        }
        
        wheelTransform.eulerAngles = new Vector3(0, 0, startAngle + targetAngle);
        
        isSpinning = false;
        OnSpinComplete?.Invoke();
        
        if (targetSegment == currentBulletSegment)
        {
            Debug.Log($"BULLET HIT! Landed on segment {targetSegment} (bullet was in segment {currentBulletSegment})");
            OnBulletHit?.Invoke();
        }
        else
        {
            Debug.Log($"Safe! Landed on segment {targetSegment} (bullet was in segment {currentBulletSegment})");
            OnSafe?.Invoke();
        }
    }
    

    public int GetCurrentSegment()
    {
        float currentAngle = wheelTransform.eulerAngles.z - imageRotationOffset;
        currentAngle = currentAngle % 360f;
        if (currentAngle < 0) currentAngle += 360f;
        
        
        int segment = Mathf.FloorToInt(currentAngle / degreesPerSegment) % segments;
        return segment;
    }
    
    public bool IsSpinning()
    {
        return isSpinning;
    }
}