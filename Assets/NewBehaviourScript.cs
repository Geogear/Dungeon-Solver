using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    private Color _color = new Color();
    private SpriteRenderer _spriteRenderer;

    private float _baseFlickerTime = 3.0f;
    private float _currentFlickerTime = 0.0f;
    private float _baseFlickTime = 0.1f;
    private float _currentFlickTime = 0.0f;
    private float _flickerAlpha = 0.5f;
    private bool _flickering = false;
    private bool _flick = true;

    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _color.a = _spriteRenderer.color.a;
        _color.r = _spriteRenderer.color.r;
        _color.g = _spriteRenderer.color.g;
        _color.b = _spriteRenderer.color.b;
    }

    // Update is called once per frame
    void Update()
    {
        if(_flickering)
        {
            _currentFlickerTime -= Time.deltaTime;
            if(_currentFlickerTime <= float.Epsilon)
            {
                _flickering = false;
                _flick = true;
                _color.a = 1.0f;
                _spriteRenderer.color = _color;
            }
            else
            {
                _currentFlickTime -= Time.deltaTime;
                if(_currentFlickTime <= float.Epsilon)
                {
                    _currentFlickTime = _baseFlickTime;
                    _color.a = _flick ? _flickerAlpha : 1.0f;
                    _spriteRenderer.color = _color;
                    _flick = !_flick;
                }
            }
            
        }

        /* TODO, change when to trigger flicking. */
        if(Input.GetButtonDown("Fire1"))
        {
            TriggerFlick();
        }
    }

    public void TriggerFlick()
    {
        _flickering = true;
        _flick = true;
        _currentFlickTime = 0.0f;
        _currentFlickerTime = _baseFlickerTime;
    }
}
