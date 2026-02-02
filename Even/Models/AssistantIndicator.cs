using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using Logger = Even.Utils.Logger;
// ReSharper disable MustUseReturnValue
namespace Even.Models;

public class AssistantIndicator : MonoBehaviour
{
    public enum IndicatorState
    {
        Sleeping,
        Listening
    }

    private GameObject _indicator;
    private Material _material;
    private float _pulse;

    private Transform _parentCamera;

    private static Material _iconMaterialBase;
    private static readonly int Surface = Shader.PropertyToID("_Surface");
    private static readonly int Blend = Shader.PropertyToID("_Blend");
    private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
    private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
    private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");
    
    private readonly Vector3 _baseLocalPosition = new(0f, -0.28f, 0.474f);
    private readonly Vector3 _baseScale = new(0.105f, 0.105f, 0.105f);

    private float _targetAlpha;
    private float _currentAlpha;
    private const float FadeSpeed = 4f;

    private void Awake()
    {
        if (Camera.main != null)
            _parentCamera = Camera.main.transform;

        if (_iconMaterialBase == null)
        {
            _iconMaterialBase = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            _iconMaterialBase.SetFloat(Surface, 1);
            _iconMaterialBase.SetFloat(Blend, 0);
            _iconMaterialBase.SetFloat(SrcBlend, (float)BlendMode.SrcAlpha);
            _iconMaterialBase.SetFloat(DstBlend, (float)BlendMode.OneMinusSrcAlpha);
            _iconMaterialBase.SetFloat(ZWrite, 0);
            _iconMaterialBase.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            _iconMaterialBase.renderQueue = (int)RenderQueue.Transparent;
        }

        CreateIndicator();
        SetState(IndicatorState.Sleeping);
    }

    private void CreateIndicator()
    {
        _indicator = GameObject.CreatePrimitive(PrimitiveType.Quad);
        _indicator.name = "AssistantIndicator";
        _indicator.SetLayer(UnityLayer.FirstPersonOnly);
        
        Destroy(_indicator.GetComponent<Collider>());

        _indicator.transform.SetParent(_parentCamera, false);
        _indicator.transform.localPosition = _baseLocalPosition;
        _indicator.transform.localRotation = Quaternion.identity;
        _indicator.transform.localScale = _baseScale;

        var tex = LoadTextureFromResource("Even.Assets.Resources.Images.speaker_icon.png");

        _material = new Material(_iconMaterialBase)
        {
            mainTexture = tex,
            color = new Color(1f, 1f, 1f, 0f)
        };

        var renderer = _indicator.GetComponent<Renderer>();
        renderer.material = _material;
    }

    public void SetState(IndicatorState state)
    {
        if (!_indicator) return;
        
        _targetAlpha = state == IndicatorState.Sleeping ? 0f : 1f;
    }

    private void Update()
    {
        if (!_indicator) return;
        
        _currentAlpha = Mathf.MoveTowards(_currentAlpha, _targetAlpha, Time.deltaTime * FadeSpeed);
        if (_material)
        {
            var col = _material.color;
            col.a = _currentAlpha;
            _material.color = col;
        }
        
        _indicator.SetActive(_currentAlpha > 0f);

        if (!(_currentAlpha > 0f)) return;
        _pulse += Time.deltaTime;
        var bob = Mathf.Sin(_pulse * 2f) * 0.015f;
        var sway = Mathf.Sin(_pulse) * 0.008f;

        _indicator.transform.localPosition = _baseLocalPosition + new Vector3(sway, bob, 0f);

        var rotZ = Mathf.Sin(_pulse * 1.5f) * 3f;
        _indicator.transform.localRotation = Quaternion.Euler(0f, 0f, rotZ);

        var scalePulse = Mathf.Sin(_pulse * 2.5f) * 0.01f;
        _indicator.transform.localScale = _baseScale + new Vector3(scalePulse, scalePulse, scalePulse);
    }

    public static Texture2D LoadTextureFromResource(string resourcePath)
    {
        var texture = new Texture2D(2, 2);

        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
        if (stream != null)
        {
            var fileData = new byte[stream.Length];
            stream.Read(fileData, 0, (int)stream.Length);
            texture.LoadImage(fileData);
        }
        else
        {
            Logger.Error("Failed to load texture from resource: " + resourcePath);
        }

        return texture;
    }

    private void OnDestroy()
    {
        if (_indicator != null)
            Destroy(_indicator);

        if (_material != null)
            Destroy(_material);
    }
}