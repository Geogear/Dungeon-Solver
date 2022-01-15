using UnityEngine;

public class Spell : MonoBehaviour
{
    public PlayerCharacter _playerScript = null;

    [SerializeField] private float _upLift = 0.2f;

    private Animator _animator = null;
    private bool _onPlayer = false;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCharacter>();
    }

    // Update is called once per frame
    void Update()
    {
        if(_playerScript == null)
        {
            _playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCharacter>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        _onPlayer = collision.tag == "Player";
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            _onPlayer = false;
        }
    }

    public void SetPos(Vector3 pos)
    {
        transform.position = new Vector3(pos.x, pos.y + _upLift, pos.z);
    }

    public void ActivateSpell(string animName, float damage)
    {
        _animator.SetTrigger(animName);
        _playerScript.GetHit(damage);
    }
}
